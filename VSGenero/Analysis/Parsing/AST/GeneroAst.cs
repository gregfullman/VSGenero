using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public sealed class GeneroAst : ILocationResolver
    {
        private readonly GeneroLanguageVersion _langVersion;
        private readonly AstNode _body;
        internal readonly int[] _lineLocations;
        private readonly IProjectEntry _projEntry;
        private readonly string _filename;

        private readonly Dictionary<AstNode, Dictionary<object, object>> _attributes = new Dictionary<AstNode, Dictionary<object, object>>();

        public GeneroAst(AstNode body, int[] lineLocations, GeneroLanguageVersion langVersion = GeneroLanguageVersion.None, IProjectEntry projEntry = null, string filename = null)
        {
            if (body == null) {
                throw new ArgumentNullException("body");
            }
            _langVersion = langVersion;
            _body = body;
            _lineLocations = lineLocations;
            _projEntry = projEntry;
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
            AstNode node = (AstNode)location;
            var span = node.GetSpan(this);
            return new LocationInfo(project, span.Start.Line, span.Start.Column);
        }

        public LocationInfo ResolveLocation(object location)
        {
            AstNode node = (AstNode)location;
            var span = node.GetSpan(this);
            return _projEntry == null ? new LocationInfo(_filename, span.Start.Line, span.Start.Column) : new LocationInfo(_projEntry, span.Start.Line, span.Start.Column);
        }

        public MemberResult[] GetModules(bool topLevelOnly = false)
        {
            List<MemberResult> res = new List<MemberResult>();

            //var children = GlobalScope.GetChildrenPackages(InterpreterContext);

            //foreach (var child in children)
            //{
            //    res.Add(new MemberResult(child.Key, PythonMemberType.Module));
            //}

            return res.ToArray();
        }

        public MemberResult[] GetModuleMembers(string[] names, bool includeMembers = false)
        {
            var res = new List<MemberResult>();
            //var children = GlobalScope.GetChildrenPackages(InterpreterContext);

            //foreach (var child in children)
            //{
            //    var mod = (ModuleInfo)child.Value;
            //    var childName = mod.Name.Substring(this.GlobalScope.Name.Length + 1);

            //    if (childName.StartsWith(names[0]))
            //    {
            //        res.AddRange(PythonAnalyzer.GetModuleMembers(InterpreterContext, names, includeMembers, mod as IModule));
            //    }
            //}

            return res.ToArray();
        }

        /// <summary>
        /// Gets the available names at the given location.  This includes built-in variables, global variables, and locals.
        /// </summary>
        /// <param name="index">The 0-based absolute index into the file where the available mebmers should be looked up.</param>
        public IEnumerable<MemberResult> GetAllAvailableMembersByIndex(int index, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            /*
             * Need to return (depending on context):
             * 1) Local variables, types, or constants
             * 2) Module variables, types, or constants
             * 3) Module cursors
             * 4) Functions within the current module
             * 5) Global variables, types, or constants
             * 6) Functions within the current project
             * 7) Public function modules
             */
            List<MemberResult> results = new List<MemberResult>();
            return results;
            //var result = new Dictionary<string, List<AnalysisValue>>();

            //// collect builtins
            //foreach (var variable in ProjectState.BuiltinModule.GetAllMembers(ProjectState._defaultContext))
            //{
            //    result[variable.Key] = new List<AnalysisValue>(variable.Value);
            //}

            //// collect variables from user defined scopes
            //var scope = FindScope(index);
            //foreach (var s in scope.EnumerateTowardsGlobal)
            //{
            //    foreach (var kvp in s.GetAllMergedVariables())
            //    {
            //        result[kvp.Key] = new List<AnalysisValue>(kvp.Value.TypesNoCopy);
            //    }
            //}

            //var res = MemberDictToResultList(GetPrivatePrefix(scope), options, result);
            //if (options.Keywords())
            //{
            //    res = GetKeywordMembers(options, scope).Union(res);
            //}

            //return res;
        }

        /// <summary>
        /// Gets information about the available signatures for the given expression.
        /// </summary>
        /// <param name="exprText">The expression to get signatures for.</param>
        /// <param name="index">The 0-based absolute index into the file.</param>
        public IEnumerable<IFunctionResult> GetSignaturesByIndex(string exprText, int index)
        {
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

            return null;
        }

        /// <summary>
        /// Evaluates the given expression in at the provided line number and returns the values
        /// that the expression can evaluate to.
        /// </summary>
        /// <param name="exprText">The expression to determine the result of.</param>
        /// <param name="index">The 0-based absolute index into the file where the expression should be evaluated within the module.</param>
        public IAnalysisResult GetValueByIndex(string exprText, int index)
        {
            // do a binary search to determine what node we're in
            List<int> keys = _body.Children.Select(x => x.Key).ToList();
            int searchIndex = keys.BinarySearch(index);
            if(searchIndex < 0)
            {
                searchIndex = ~searchIndex;
                if (searchIndex > 0)
                    searchIndex--;
            }

            int key = keys[searchIndex];

            // TODO: need to handle multiple results of the same name
            AstNode containingNode = _body.Children[key];
            if(containingNode != null)
            {
                if(containingNode is IFunctionResult)
                {
                    // Check for local vars, types, and constants
                    IFunctionResult func = containingNode as IFunctionResult;
                    IAnalysisResult res;
                    if(func.Variables.TryGetValue(exprText, out res) ||
                       func.Types.TryGetValue(exprText, out res) ||
                       func.Constants.TryGetValue(exprText, out res))
                    {
                        return res;
                    }
                }

                if(_body is IModuleResult)
                {
                    // check for module vars, types, and constants (and globals defined in this module)
                    IModuleResult mod = _body as IModuleResult;
                    IAnalysisResult res;
                    if(mod.Variables.TryGetValue(exprText, out res) ||
                       mod.Types.TryGetValue(exprText, out res) ||
                       mod.Constants.TryGetValue(exprText, out res) ||
                       mod.GlobalVariables.TryGetValue(exprText, out res) ||
                       mod.GlobalTypes.TryGetValue(exprText, out res) ||
                       mod.GlobalConstants.TryGetValue(exprText, out res))
                    {
                        return res;
                    }

                    // check for cursors in this module
                    if(mod.Cursors.TryGetValue(exprText, out res))
                    {
                        return res;
                    }

                    // check for module functions
                    IFunctionResult funcRes;
                    if(mod.Functions.TryGetValue(exprText, out funcRes))
                    {
                        return funcRes;
                    }
                }

                // TODO: this could probably be done more efficiently by having each GeneroAst load globals and functions into
                // dictionaries stored on the IGeneroProject, instead of in each project entry.
                // However, this does required more upkeep when changes occur. Will look into it...
                if(_projEntry != null && _projEntry is IGeneroProjectEntry)
                {
                    IGeneroProjectEntry genProj = _projEntry as IGeneroProjectEntry;
                    if(genProj.ParentProject != null)
                    {
                        foreach(var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                        {
                            if(projEntry.Value.Analysis != null &&
                               projEntry.Value.Analysis.Body != null)
                            {
                                IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                                if(modRes != null)
                                {
                                    // check global vars, types, and constants
                                    IAnalysisResult res;
                                    if(modRes.GlobalVariables.TryGetValue(exprText, out res) ||
                                       modRes.GlobalTypes.TryGetValue(exprText, out res) ||
                                       modRes.GlobalConstants.TryGetValue(exprText, out res))
                                    {
                                        return res;
                                    }

                                    // check project functions
                                    IFunctionResult funcRes;
                                    if (modRes.Functions.TryGetValue(exprText, out funcRes))
                                    {
                                        if(funcRes.AccessModifier == AccessModifier.Public)
                                            return funcRes;
                                    }

                                    // check for cursors in this module
                                    if (modRes.Cursors.TryGetValue(exprText, out res))
                                    {
                                        return res;
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
                 * 7) Public functions
                 */
            }


            return null;
        }

        /// <summary>
        /// Evaluates a given expression and returns a list of members which exist in the expression.
        /// 
        /// If the expression is an empty string returns all available members at that location.
        /// 
        /// index is a zero-based absolute index into the file.
        /// </summary>
        public IEnumerable<MemberResult> GetMembersByIndex(string exprText, int index, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            List<MemberResult> results = new List<MemberResult>();
            return results;
            //if (exprText.Length == 0)
            //{
            //    return GetAllAvailableMembersByIndex(index, options);
            //}

            //var scope = FindScope(index);
            //var privatePrefix = GetPrivatePrefixClassName(scope);

            //var expr = Statement.GetExpression(GetAstFromText(exprText, privatePrefix).Body);
            //if (expr is ConstantExpression && ((ConstantExpression)expr).Value is int)
            //{
            //    // no completions on integer ., the user is typing a float
            //    return new MemberResult[0];
            //}

            //var unit = GetNearestEnclosingAnalysisUnit(scope);
            //var lookup = new ExpressionEvaluator(unit.CopyForEval(), scope, mergeScopes: true).Evaluate(expr);
            //return GetMemberResults(lookup, scope, options);
        }

        /// <summary>
        /// Gets the variables the given expression evaluates to.  Variables include parameters, locals, and fields assigned on classes, modules and instances.
        /// 
        /// Variables are classified as either definitions or references.  Only parameters have unique definition points - all other types of variables
        /// have only one or more references.
        /// 
        /// index is a 0-based absolute index into the file.
        /// </summary>
        public IEnumerable<IAnalysisVariable> GetVariablesByIndex(string exprText, int index)
        {
            List<IAnalysisVariable> results = new List<IAnalysisVariable>();
            return results;
            //var scope = FindScope(index);
            //string privatePrefix = GetPrivatePrefixClassName(scope);
            //var expr = Statement.GetExpression(GetAstFromText(exprText, privatePrefix).Body);

            //var unit = GetNearestEnclosingAnalysisUnit(scope);
            //NameExpression name = expr as NameExpression;
            //if (name != null)
            //{
            //    var defScope = scope.EnumerateTowardsGlobal.FirstOrDefault(s =>
            //        s.Variables.ContainsKey(name.Name) && (s == scope || s.VisibleToChildren || IsFirstLineOfFunction(scope, s, index)));

            //    if (defScope == null)
            //    {
            //        var variables = _unit.ProjectState.BuiltinModule.GetDefinitions(name.Name);
            //        return variables.SelectMany(ToVariables);
            //    }

            //    return GetVariablesInScope(name, defScope).Distinct();
            //}

            //MemberExpression member = expr as MemberExpression;
            //if (member != null)
            //{
            //    var eval = new ExpressionEvaluator(unit.CopyForEval(), scope, mergeScopes: true);
            //    var objects = eval.Evaluate(member.Target);

            //    foreach (var v in objects)
            //    {
            //        var container = v as IReferenceableContainer;
            //        if (container != null)
            //        {
            //            return ReferencablesToVariables(container.GetDefinitions(member.Name));
            //        }
            //    }
            //}
        }
    }
}
