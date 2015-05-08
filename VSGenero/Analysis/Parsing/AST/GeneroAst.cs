using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public partial class GeneroAst : ILocationResolver
    {
        private readonly GeneroLanguageVersion _langVersion;
        private readonly AstNode _body;
        internal readonly int[] _lineLocations;
        private readonly IProjectEntry _projEntry;
        private readonly string _filename;

        private readonly Dictionary<AstNode, Dictionary<object, object>> _attributes = new Dictionary<AstNode, Dictionary<object, object>>();

        public GeneroAst(AstNode body, int[] lineLocations, GeneroLanguageVersion langVersion = GeneroLanguageVersion.None, IProjectEntry projEntry = null, string filename = null)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            _langVersion = langVersion;
            _body = body;
            _lineLocations = lineLocations;
            _projEntry = projEntry;

            InitializeCompletionContextMaps();
            InitializeBuiltins();
            InitializeImportedPackages();   // for this instance
            InitializePackages();

            if (_body is ModuleNode)
            {
                foreach (var import in (_body as ModuleNode).CExtensionImports)
                {
                    if (_importedPackages.ContainsKey(import))
                    {
                        _importedPackages[import] = true;
                    }
                }
            }
        }

        public AstNode Body
        {
            get { return _body; }
        }

        public GeneroLanguageVersion LanguageVersion
        {
            get { return _langVersion; }
        }

        public IGeneroProjectEntry ProjectEntry
        {
            get
            {
                return _projEntry as IGeneroProjectEntry;
            }
        }

        internal bool TryGetAttribute(AstNode node, object key, out object value)
        {
            Dictionary<object, object> nodeAttrs;
            if (_attributes.TryGetValue(node, out nodeAttrs))
            {
                return nodeAttrs.TryGetValue(key, out value);
            }
            else
            {
                value = null;
            }
            return false;
        }

        internal void SetAttribute(AstNode node, object key, object value)
        {
            Dictionary<object, object> nodeAttrs;
            if (!_attributes.TryGetValue(node, out nodeAttrs))
            {
                nodeAttrs = _attributes[node] = new Dictionary<object, object>();
            }
            nodeAttrs[key] = value;
        }

        /// <summary>
        /// Copies attributes that apply to one node and makes them available for the other node.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void CopyAttributes(AstNode from, AstNode to)
        {
            Dictionary<object, object> nodeAttrs;
            if (_attributes.TryGetValue(from, out nodeAttrs))
            {
                _attributes[to] = new Dictionary<object, object>(nodeAttrs);
            }
        }

        internal SourceLocation IndexToLocation(int index)
        {
            if (index == -1)
            {
                return SourceLocation.Invalid;
            }

            var locs = _lineLocations;
            int match = Array.BinarySearch(locs, index);
            if (match < 0)
            {
                // If our index = -1, it means we're on the first line.
                if (match == -1)
                {
                    return new SourceLocation(index, 1, index + 1);
                }

                // If we couldn't find an exact match for this line number, get the nearest
                // matching line number less than this one
                match = ~match - 1;
            }
            return new SourceLocation(index, match + 2, index - locs[match] + 1);
        }

        LocationInfo ILocationResolver.ResolveLocation(IProjectEntry project, object location)
        {
            IAnalysisResult result = location as IAnalysisResult;
            if (result != null)
            {
                var locIndex = result.LocationIndex;
                var loc = IndexToLocation(locIndex);
                return new LocationInfo(project, loc.Line, loc.Column);
            }
            return null;
        }

        public LocationInfo ResolveLocation(object location)
        {
            IAnalysisResult result = location as IAnalysisResult;
            if (result != null)
            {
                if (result.Location != null)
                {
                    return result.Location;
                }
                else
                {
                    var locIndex = result.LocationIndex;
                    var loc = IndexToLocation(locIndex);
                    return _projEntry == null ? new LocationInfo(_filename, loc.Line, loc.Column) : new LocationInfo(_projEntry, loc.Line, loc.Column);
                }
            }
            return null;
        }

        public MemberResult[] GetModules(bool topLevelOnly = false)
        {
            List<MemberResult> res = new List<MemberResult>();

            return res.ToArray();
        }

        public MemberResult[] GetModuleMembers(string[] names, bool includeMembers = false)
        {
            var res = new List<MemberResult>();

            return res.ToArray();
        }

        public FunctionBlockNode GetContainingFunction(int index)
        {
            AstNode containingNode = null;
            List<int> keys = null;
            int searchIndex = -1;
            int key = -1;
            if (_body.Children.Count > 0)
            {
                // do a binary search to determine what node we're in
                keys = _body.Children.Select(x => x.Key).ToList();
                searchIndex = keys.BinarySearch(index);
                if (searchIndex < 0)
                {
                    searchIndex = ~searchIndex;
                    if (searchIndex > 0)
                        searchIndex--;
                }

                key = keys[searchIndex];

                // TODO: need to handle multiple results of the same name
                containingNode = _body.Children[key];
            }

            if (containingNode != null &&
                containingNode is FunctionBlockNode)
            {
                return containingNode as FunctionBlockNode;
            }
            return null;
        }

        /// <summary>
        /// Gets information about the available signatures for the given expression.
        /// </summary>
        /// <param name="exprText">The expression to get signatures for.</param>
        /// <param name="index">The 0-based absolute index into the file.</param>
        public IEnumerable<IFunctionResult> GetSignaturesByIndex(string exprText, int index, IReverseTokenizer revTokenizer, IFunctionInformationProvider functionProvider)
        {
            _functionProvider = functionProvider;

            // First see if we're in the process of defining a function
            List<MemberResult> dummyList;
            if (TryFunctionDefContext(index, revTokenizer, out dummyList))
            {
                return null;
            }

            /*
             * Need to check for:
             * 1) Functions within the current module
             * 2) Functions within the current project
             * 3) Public functions
             */
            if (_body is IModuleResult)
            {
                // check for module vars, types, and constants (and globals defined in this module)
                IModuleResult mod = _body as IModuleResult;

                // check for module functions
                IFunctionResult funcRes;
                if (mod.Functions.TryGetValue(exprText, out funcRes))
                {
                    return new IFunctionResult[1] { funcRes };
                }
            }

            if (_projEntry != null && _projEntry is IGeneroProjectEntry)
            {
                IGeneroProjectEntry genProj = _projEntry as IGeneroProjectEntry;
                if (genProj.ParentProject != null)
                {
                    foreach (var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                    {
                        if (projEntry.Value.Analysis != null &&
                           projEntry.Value.Analysis.Body != null)
                        {
                            IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                            if (modRes != null)
                            {
                                // check project functions
                                IFunctionResult funcRes;
                                if (modRes.Functions.TryGetValue(exprText, out funcRes))
                                {
                                    if (funcRes.AccessModifier == AccessModifier.Public)
                                        return new IFunctionResult[1] { funcRes };
                                }
                            }
                        }
                    }
                }
            }

            // Check for class methods
            IAnalysisResult member = GetValueByIndex(exprText, index, _functionProvider, _databaseProvider);
            if (member is IFunctionResult)
            {
                return new IFunctionResult[1] { member as IFunctionResult };
            }

            if (_functionProvider != null)
            {
                // check for the function name in the function provider
                var func = _functionProvider.GetFunction(exprText);
                if (func != null)
                {
                    return new IFunctionResult[1] { func };
                }
            }

            return null;
        }

        /// <summary>
        /// Evaluates the given expression in at the provided line number and returns the values
        /// that the expression can evaluate to.
        /// </summary>
        /// <param name="exprText">The expression to determine the result of.</param>
        /// <param name="index">The 0-based absolute index into the file where the expression should be evaluated within the module.</param>
        public IAnalysisResult GetValueByIndex(string exprText, int index, IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider, bool searchInFunctionProvider = false)
        {
            _functionProvider = functionProvider;
            _databaseProvider = databaseProvider;

            AstNode containingNode = null;
            List<int> keys = null;
            int searchIndex = -1;
            int key = -1;
            if (_body.Children.Count > 0)
            {
                // do a binary search to determine what node we're in
                keys = _body.Children.Select(x => x.Key).ToList();
                searchIndex = keys.BinarySearch(index);
                if (searchIndex < 0)
                {
                    searchIndex = ~searchIndex;
                    if (searchIndex > 0)
                        searchIndex--;
                }

                key = keys[searchIndex];

                // TODO: need to handle multiple results of the same name
                containingNode = _body.Children[key];
            }

            string[] dottedPieces = exprText.Split(new[] { '.' });
            IAnalysisResult res = null;
            bool dotPieceNotFound = false;

            for (int i = 0; i < dottedPieces.Length; i++)
            {
                string dotPiece = dottedPieces[i];
                int leftBrackIndex = dotPiece.IndexOf('[');
                if (leftBrackIndex > 0)
                {
                    dotPiece = dotPiece.Substring(0, leftBrackIndex);
                }

                if (res != null)
                {
                    IAnalysisResult tempRes = res.GetMember(dotPiece, this);
                    res = tempRes;
                    if (tempRes == null)
                    {
                        dotPieceNotFound = true;
                    }
                }
                else if (!dotPieceNotFound)
                {
                    if (SystemVariables.TryGetValue(dotPiece, out res) ||
                       SystemConstants.TryGetValue(dotPiece, out res))
                    {
                        continue;
                    }

                    IFunctionResult funcRes;
                    if (SystemFunctions.TryGetValue(dotPiece, out funcRes))
                    {
                        res = funcRes;
                        continue;
                    }

                    if (_functionProvider != null && _functionProvider.Name.Equals(dotPiece, StringComparison.OrdinalIgnoreCase))
                    {
                        res = _functionProvider;
                        continue;
                    }

                    if (containingNode != null && containingNode is IFunctionResult)
                    {
                        // Check for local vars, types, and constants
                        IFunctionResult func = containingNode as IFunctionResult;
                        if (func.Variables.TryGetValue(dotPiece, out res) ||
                           func.Types.TryGetValue(dotPiece, out res) ||
                           func.Constants.TryGetValue(dotPiece, out res))
                        {
                            continue;
                        }
                    }

                    if (_body is IModuleResult)
                    {
                        // check for module vars, types, and constants (and globals defined in this module)
                        IModuleResult mod = _body as IModuleResult;
                        if (mod.Variables.TryGetValue(dotPiece, out res) ||
                           mod.Types.TryGetValue(dotPiece, out res) ||
                           mod.Constants.TryGetValue(dotPiece, out res) ||
                           mod.GlobalVariables.TryGetValue(dotPiece, out res) ||
                           mod.GlobalTypes.TryGetValue(dotPiece, out res) ||
                           mod.GlobalConstants.TryGetValue(dotPiece, out res))
                        {
                            continue;
                        }

                        // check for cursors in this module
                        if (mod.Cursors.TryGetValue(dotPiece, out res))
                        {
                            continue;
                        }

                        // check for module functions
                        if (mod.Functions.TryGetValue(dotPiece, out funcRes))
                        {
                            res = funcRes;
                            continue;
                        }
                    }

                    // TODO: this could probably be done more efficiently by having each GeneroAst load globals and functions into
                    // dictionaries stored on the IGeneroProject, instead of in each project entry.
                    // However, this does required more upkeep when changes occur. Will look into it...
                    if (_projEntry != null && _projEntry is IGeneroProjectEntry)
                    {
                        IGeneroProjectEntry genProj = _projEntry as IGeneroProjectEntry;
                        if (genProj.ParentProject != null)
                        {
                            bool found = false;
                            foreach (var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                            {
                                if (projEntry.Value.Analysis != null &&
                                   projEntry.Value.Analysis.Body != null)
                                {
                                    IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                                    if (modRes != null)
                                    {
                                        // check global vars, types, and constants
                                        if (modRes.GlobalVariables.TryGetValue(dotPiece, out res) ||
                                           modRes.GlobalTypes.TryGetValue(dotPiece, out res) ||
                                           modRes.GlobalConstants.TryGetValue(dotPiece, out res))
                                        {
                                            found = true;
                                            break;
                                        }

                                        // check project functions
                                        if (modRes.Functions.TryGetValue(dotPiece, out funcRes))
                                        {
                                            if (funcRes.AccessModifier == AccessModifier.Public)
                                            {
                                                res = funcRes;
                                                found = true;
                                                break;
                                            }
                                        }

                                        // check for cursors in this module
                                        if (modRes.Cursors.TryGetValue(dotPiece, out res))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (found)
                                continue;
                        }
                    }

                    // check for classes
                    GeneroPackage package;
                    if (Packages.TryGetValue(dotPiece, out package))
                    {
                        res = package;
                        continue;
                    }

                    /* TODO:
                     * Need to check for:
                     * 1) Temp tables
                     * 2) DB Tables and columns
                     */
                    if(_databaseProvider != null)
                    {
                        res = _databaseProvider.GetTable(dotPiece);
                        if (res != null)
                            continue;
                    }


                    // Nothing found yet...
                    // If our containing node is at the function or globals level, we need to go deeper
                    if (containingNode != null &&
                        (containingNode is GlobalsNode ||
                         containingNode is FunctionBlockNode ||
                         containingNode is ReportBlockNode))
                    {
                        keys = containingNode.Children.Select(x => x.Key).ToList();
                        searchIndex = keys.BinarySearch(index);
                        if (searchIndex < 0)
                        {
                            searchIndex = ~searchIndex;
                            if (searchIndex > 0)
                                searchIndex--;
                        }
                        if (keys.Count > 0)
                        {
                            key = keys[searchIndex];
                            containingNode = containingNode.Children[key];
                        }
                    }
                    // check for record field
                    if (containingNode != null &&
                        (containingNode is DefineNode ||
                         containingNode is TypeDefNode))
                    {
                        keys = containingNode.Children.Select(x => x.Key).ToList();
                        searchIndex = keys.BinarySearch(index);
                        if (searchIndex < 0)
                        {
                            searchIndex = ~searchIndex;
                            if (searchIndex > 0)
                                searchIndex--;
                        }
                        key = keys[searchIndex];
                        containingNode = containingNode.Children[key];

                        if (containingNode != null &&
                            (containingNode is VariableDefinitionNode ||
                            containingNode is TypeDefinitionNode) &&
                            containingNode.Children.Count == 1 &&
                            containingNode.Children[containingNode.Children.Keys[0]] is TypeReference)
                        {
                            var typeRef = containingNode.Children[containingNode.Children.Keys[0]] as TypeReference;
                            while (typeRef != null &&
                                   typeRef.Children.Count == 1)
                            {
                                if (typeRef.Children[typeRef.Children.Keys[0]] is RecordDefinitionNode)
                                {
                                    var recNode = typeRef.Children[typeRef.Children.Keys[0]] as RecordDefinitionNode;
                                    VariableDef recField;
                                    if (recNode.MemberDictionary.TryGetValue(exprText, out recField))
                                    {
                                        res = recField;
                                        break;
                                    }
                                    else
                                    {
                                        recField = recNode.MemberDictionary.Where(x => x.Value.LocationIndex < index)
                                                                           .OrderByDescending(x => x.Value.LocationIndex)
                                                                           .Select(x => x.Value)
                                                                           .FirstOrDefault();
                                        if (recField != null)
                                        {
                                            typeRef = recField.Type;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                else if (typeRef.Children[typeRef.Children.Keys[0]] is TypeReference)
                                {
                                    typeRef = typeRef.Children[typeRef.Children.Keys[0]] as TypeReference;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (searchInFunctionProvider)
                    {
                        if (res == null && _functionProvider != null)
                        {
                            // check for the function name in the function provider
                            res = _functionProvider.GetFunction(exprText);
                            if (res != null)
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Gets the variables the given expression evaluates to.  Variables include parameters, locals, and fields assigned on classes, modules and instances.
        /// 
        /// Variables are classified as either definitions or references.  Only parameters have unique definition points - all other types of variables
        /// have only one or more references.
        /// 
        /// index is a 0-based absolute index into the file.
        /// </summary>
        public IEnumerable<IAnalysisVariable> GetVariablesByIndex(string exprText, int index, IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider)
        {
            _functionProvider = functionProvider;
            _databaseProvider = databaseProvider;

            List<IAnalysisVariable> vars = new List<IAnalysisVariable>();

            // do a binary search to determine what node we're in
            List<int> keys = _body.Children.Select(x => x.Key).ToList();
            int searchIndex = keys.BinarySearch(index);
            if (searchIndex < 0)
            {
                searchIndex = ~searchIndex;
                if (searchIndex > 0)
                    searchIndex--;
            }

            int key = keys[searchIndex];

            AstNode containingNode = _body.Children[key];
            if (containingNode != null)
            {
                if (containingNode is IFunctionResult)
                {
                    // Check for local vars, types, and constants
                    IFunctionResult func = containingNode as IFunctionResult;
                    IAnalysisResult res;

                    if (func.Variables.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (func.Types.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (func.Constants.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }
                }

                if (_body is IModuleResult)
                {
                    // check for module vars, types, and constants (and globals defined in this module)
                    IModuleResult mod = _body as IModuleResult;
                    IAnalysisResult res;

                    if (mod.Variables.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.Types.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.Constants.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.GlobalVariables.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.GlobalTypes.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.GlobalConstants.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    // check for cursors in this module
                    if (mod.Cursors.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    // check for module functions
                    IFunctionResult funcRes;
                    if (mod.Functions.TryGetValue(exprText, out funcRes))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(funcRes), VariableType.Definition));
                    }
                }
            }

            // TODO: this could probably be done more efficiently by having each GeneroAst load globals and functions into
            // dictionaries stored on the IGeneroProject, instead of in each project entry.
            // However, this does required more upkeep when changes occur. Will look into it...
            if (_projEntry != null && _projEntry is IGeneroProjectEntry)
            {
                IGeneroProjectEntry genProj = _projEntry as IGeneroProjectEntry;
                if (genProj.ParentProject != null)
                {
                    foreach (var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                    {
                        if (projEntry.Value.Analysis != null &&
                           projEntry.Value.Analysis.Body != null)
                        {
                            IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                            if (modRes != null)
                            {
                                // check global vars, types, and constants
                                IAnalysisResult res;
                                if (modRes.GlobalVariables.TryGetValue(exprText, out res))
                                {
                                    vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                }

                                if (modRes.GlobalTypes.TryGetValue(exprText, out res))
                                {
                                    vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                }

                                if (modRes.GlobalConstants.TryGetValue(exprText, out res))
                                {
                                    vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                }

                                // check for cursors in this module
                                if (modRes.Cursors.TryGetValue(exprText, out res))
                                {
                                    vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                }

                                // check for module functions
                                IFunctionResult funcRes;
                                if (modRes.Functions.TryGetValue(exprText, out funcRes))
                                {
                                    vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(funcRes), VariableType.Definition));
                                }
                            }
                        }
                    }
                }
            }

            /* TODO:
             * Need to check for:
             * 1) Temp tables
             * 2) DB Tables and columns
             * 3) Record fields
             */

            if(_functionProvider != null)
            {
                IFunctionResult funcRes = _functionProvider.GetFunction(exprText);
                if(funcRes != null)
                {
                    vars.Add(new AnalysisVariable(funcRes.Location, VariableType.Definition));
                }
            }

            return vars;
        }
    }
}
