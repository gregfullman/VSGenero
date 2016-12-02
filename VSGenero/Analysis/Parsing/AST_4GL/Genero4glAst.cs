/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public partial class Genero4glAst : GeneroAst
    {
        public Genero4glAst(AstNode4gl body, int[] lineLocations, GeneroLanguageVersion langVersion = GeneroLanguageVersion.None, IProjectEntry projEntry = null, string filename = null)
            : base(body, lineLocations, langVersion, projEntry, filename)
        {
            ReloadContextMap();
            InitializeBuiltins();
            InitializeImportedPackages();   // for this instance
            InitializePackages();

            if (body is ModuleNode)
            {
                foreach (var import in (body as ModuleNode).CExtensionImports)
                {
                    if (_importedPackages.ContainsKey(import))
                    {
                        _importedPackages[import] = true;
                    }
                }
            }
        }

        public IEnumerable<string> GetImportedModules()
        {
            if (Body is ModuleNode)
            {
                return (Body as ModuleNode).FglImports;
            }
            return new string[0];
        }

        public IEnumerable<string> GetIncludedFiles()
        {
            return (Body as AstNode4gl)?.IncludeFiles.Keys;
        }

        public FunctionBlockNode GetContainingFunction(int index, bool returnNextIfOutside)
        {
            AstNode4gl containingNode = null;
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
                containingNode = Body.Children[key] as AstNode4gl;
                
                if (returnNextIfOutside && containingNode != null && containingNode.EndIndex < index)
                {
                    // need to go to the next function
                    searchIndex++;
                    if (keys.Count > searchIndex)
                    {
                        key = keys[searchIndex];
                        containingNode = Body.Children[key] as AstNode4gl;
                    }
                    else
                    {
                        containingNode = null;
                    }
                }
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
        public override IEnumerable<IFunctionResult> GetSignaturesByIndex(string exprText, int index, IReverseTokenizer revTokenizer, IFunctionInformationProvider functionProvider)
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
                _body.SetNamespace(null);
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
                if (genProj.ParentProject != null &&
                    !genProj.FilePath.ToLower().EndsWith(".inc"))
                {
                    foreach (var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                    {
                        if (projEntry.Value.Analysis != null &&
                           projEntry.Value.Analysis.Body != null)
                        {
                            projEntry.Value.Analysis.Body.SetNamespace(null);
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
            IGeneroProject dummyProj;
            IProjectEntry dummyProjEntry;
            bool dummyDef;
            IAnalysisResult member = GetValueByIndex(exprText, index, _functionProvider, _databaseProvider, _programFileProvider, true, out dummyDef, out dummyProj, out dummyProjEntry);
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
                    return func;
                }
            }

            return null;
        }

        public static IAnalysisResult GetValueByIndex(string exprText, int index, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projectEntry, out bool isDeferredFunction,
                                                      FunctionProviderSearchMode searchInFunctionProvider = FunctionProviderSearchMode.NoSearch, bool isFunctionCallOrDefinition = false,
                                                      bool getTypes = true, bool getVariables = true, bool getConstants = true)
        {
            isDeferredFunction = false;
            definingProject = null;
            projectEntry = null;
            //_functionProvider = functionProvider;
            //_databaseProvider = databaseProvider;
            //_programFileProvider = programFileProvider;

            AstNode containingNode = null;
            if (ast != null)
            {
                containingNode = GetContainingNode(ast.Body, index);
                ast.Body.SetNamespace(null);
            }

            IAnalysisResult res = null;
            int tmpIndex = 0;
            int bracketDepth = 0;
            bool doSearch = false;
            bool resetStartIndex = false;
            int startIndex = 0, endIndex = 0;
            bool noEndIndexSet = false;

            while (tmpIndex < exprText.Length)
            {
                if (resetStartIndex)
                {
                    startIndex = tmpIndex;
                    resetStartIndex = false;
                    if (startIndex + 1 == exprText.Length)
                        break;
                }

                doSearch = false;
                switch (exprText[tmpIndex])
                {
                    case '.':
                        {
                            if (bracketDepth == 0)
                            {
                                endIndex = tmpIndex - 1;
                                if (endIndex >= startIndex)
                                {
                                    // we have our 'piece'
                                    doSearch = true;
                                }
                                if (exprText[startIndex] == '.')
                                    startIndex++;
                            }
                            tmpIndex++;
                        }
                        break;
                    case '[':
                        if (noEndIndexSet)
                            noEndIndexSet = false;
                        else
                        {
                            if (bracketDepth == 0)
                                endIndex = tmpIndex - 1;
                        }
                        bracketDepth++;
                        tmpIndex++;
                        break;
                    case ']':
                        {
                            bracketDepth--;
                            if (bracketDepth == 0)
                            {
                                if(exprText.Length <= tmpIndex + 1 ||
                                   exprText[tmpIndex+1] != '[')
                                {
                                    // we have our first 'piece'
                                    doSearch = true;
                                }
                                else
                                {
                                    noEndIndexSet = true;
                                }
                            }
                            tmpIndex++;
                        }
                        break;
                    default:
                        {
                            if (bracketDepth == 0 && (tmpIndex + 1 == exprText.Length))
                            {
                                endIndex = tmpIndex;
                                doSearch = true;
                            }
                            tmpIndex++;
                        }
                        break;
                }

                if (!doSearch)
                {
                    continue;
                }

                // we can do our search
                var dotPiece = exprText.Substring(startIndex, (endIndex - startIndex) + 1);
                if (dotPiece.Contains('('))
                {
                    // remove the params piece
                    int remIndex = dotPiece.IndexOf('(');
                    dotPiece = dotPiece.Substring(0, remIndex);
                }

                bool lookForFunctions = isFunctionCallOrDefinition && (endIndex + 1 == exprText.Length);

                resetStartIndex = true;

                if (res != null)
                {
                    IGeneroProject tempProj;
                    IProjectEntry tempProjEntry;
                    if (ast != null)
                    {
                        IAnalysisResult tempRes = res.GetMember(dotPiece, ast, out tempProj, out tempProjEntry, lookForFunctions);
                        if (tempProj != null && definingProject != tempProj)
                            definingProject = tempProj;
                        if(tempProjEntry != null && projectEntry != tempProjEntry)
                            projectEntry = tempProjEntry;
                        res = tempRes;
                        if (tempRes == null)
                        {
                            res = null;
                            break;
                        }
                    }
                    else
                    {
                        res = null;
                        break;
                    }
                }
                else
                {
                    IFunctionResult funcRes;
                    if (!lookForFunctions)
                    {
                        if (SystemVariables.TryGetValue(dotPiece, out res) ||
                           SystemConstants.TryGetValue(dotPiece, out res) ||
                            SystemMacros.TryGetValue(dotPiece, out res))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (SystemFunctions.TryGetValue(dotPiece, out funcRes))
                        {
                            res = funcRes;
                            continue;
                        }

                        if (ast != null && ast._functionProvider != null && ast._functionProvider.Name.Equals(dotPiece, StringComparison.OrdinalIgnoreCase))
                        {
                            res = ast._functionProvider;
                            continue;
                        }
                    }

                    if (containingNode != null && containingNode is IFunctionResult)
                    {
                        if (!lookForFunctions)
                        {
                            // Check for local vars, types, and constants
                            IFunctionResult func = containingNode as IFunctionResult;
                            if ((getVariables && func.Variables.TryGetValue(dotPiece, out res)) ||
                                (getTypes && func.Types.TryGetValue(dotPiece, out res)) ||
                                (getConstants && func.Constants.TryGetValue(dotPiece, out res)))
                            {
                                continue;
                            }

                            List<Tuple<IAnalysisResult, IndexSpan>> limitedScopeVars;
                            if((getVariables &&func.LimitedScopeVariables.TryGetValue(dotPiece, out limitedScopeVars)))
                            {
                                foreach(var item in limitedScopeVars)
                                {
                                    if(item.Item2.IsInSpan(index))
                                    {
                                        res = item.Item1;
                                        break;
                                    }
                                }
                                if (res != null)
                                    continue;
                            }
                        }
                    }

                    if (ast != null && ast.Body is IModuleResult)
                    {
                        IModuleResult mod = ast.Body as IModuleResult;
                        if (!lookForFunctions)
                        {
                            // check for module vars, types, and constants (and globals defined in this module)
                            if ((getVariables && (mod.Variables.TryGetValue(dotPiece, out res) || mod.GlobalVariables.TryGetValue(dotPiece, out res))) ||
                                (getTypes && (mod.Types.TryGetValue(dotPiece, out res) || mod.GlobalTypes.TryGetValue(dotPiece, out res))) ||
                                (getConstants && (mod.Constants.TryGetValue(dotPiece, out res) || mod.GlobalConstants.TryGetValue(dotPiece, out res))))
                            {
                                continue;
                            }

                            // check for cursors in this module
                            if (mod.Cursors.TryGetValue(dotPiece, out res))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            // check for module functions
                            if (mod.Functions.TryGetValue(dotPiece, out funcRes))
                            {
                                // check for any function info collected at the project entry level, and update the function's documentation with that.
                                if (ast._projEntry != null && ast._projEntry is IGeneroProjectEntry)
                                {
                                    var commentInfo = (ast._projEntry as IGeneroProjectEntry).GetFunctionInfo(funcRes.Name);
                                    if(commentInfo != null)
                                    {
                                        funcRes.SetCommentDocumentation(commentInfo);
                                    }
                                }
                                res = funcRes;
                                continue;
                            }
                        }
                    }

                    // TODO: this could probably be done more efficiently by having each GeneroAst load globals and functions into
                    // dictionaries stored on the IGeneroProject, instead of in each project entry.
                    // However, this does required more upkeep when changes occur. Will look into it...
                    if (ast != null && ast.ProjectEntry != null && ast.ProjectEntry is IGeneroProjectEntry)
                    {
                        IGeneroProjectEntry genProj = ast.ProjectEntry as IGeneroProjectEntry;
                        if (genProj.ParentProject != null &&
                            !genProj.FilePath.ToLower().EndsWith(".inc"))
                        {
                            bool found = false;
                            foreach (var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                            {
                                if (projEntry.Value.Analysis != null &&
                                   projEntry.Value.Analysis.Body != null)
                                {
                                    projEntry.Value.Analysis.Body.SetNamespace(null);
                                    IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                                    if (modRes != null)
                                    {
                                        if (!lookForFunctions)
                                        {
                                            // check global vars, types, and constants
                                            // TODO: need option to enable/disable legacy linking
                                            if ((getVariables && (modRes.Variables.TryGetValue(dotPiece, out res) || modRes.GlobalVariables.TryGetValue(dotPiece, out res))) ||
                                                (getTypes && (modRes.Types.TryGetValue(dotPiece, out res) || modRes.GlobalTypes.TryGetValue(dotPiece, out res))) ||
                                                (getConstants && (modRes.Constants.TryGetValue(dotPiece, out res) || modRes.GlobalConstants.TryGetValue(dotPiece, out res))))
                                            {
                                                found = true;
                                                break;
                                            }

                                            // check for cursors in this module
                                            if (modRes.Cursors.TryGetValue(dotPiece, out res))
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
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
                     */

                    // Nothing found yet...
                    // If our containing node is at the function or globals level, we need to go deeper
                    if (containingNode != null &&
                        (containingNode is GlobalsNode ||
                         containingNode is FunctionBlockNode ||
                         containingNode is ReportBlockNode))
                    {
                        containingNode = GetContainingNode(containingNode, index);
                    }
                    // check for record field
                    if (containingNode != null &&
                        (containingNode is DefineNode ||
                         containingNode is TypeDefNode))
                    {
                        containingNode = GetContainingNode(containingNode, index);

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

                    // try an imported module
                    if (ast != null && ast.Body is IModuleResult &&
                        ast.ProjectEntry != null && ast.ProjectEntry != null)
                    {
                        if ((ast.Body as IModuleResult).FglImports.Contains(dotPiece))
                        {
                            // need to get the ast for the other project entry
                            var refProjKVP = (ast.ProjectEntry as IGeneroProjectEntry).ParentProject.ReferencedProjects.Values.FirstOrDefault(
                                x =>
                                {
                                    var fn = Path.GetFileName(x.Directory);
                                    return fn?.Equals(dotPiece, StringComparison.OrdinalIgnoreCase) ?? false;
                                });
                            if (refProjKVP is IAnalysisResult)
                            {
                                definingProject = refProjKVP;
                                res = refProjKVP as IAnalysisResult;
                                continue;
                            }

                            IAnalysisResult sysImportMod;
                            // check the system imports
                            if(SystemImportModules.TryGetValue(dotPiece, out sysImportMod))
                            {
                                res = sysImportMod;
                                continue;
                            }
                        }
                    }

                    if (!lookForFunctions)
                    {
                        // try include files
                        var foundInclude = false;
                        if (ast?.ProjectEntry != null)
                        {
                            foreach (var includeFile in ast.ProjectEntry.GetIncludedFiles())
                            {
                                if (includeFile.Analysis?.Body is IModuleResult)
                                {
                                    var mod = includeFile.Analysis.Body as IModuleResult;
                                    if ((getTypes && (mod.Types.TryGetValue(dotPiece, out res) || mod.GlobalTypes.TryGetValue(dotPiece, out res))) ||
                                        (getConstants && (mod.Constants.TryGetValue(dotPiece, out res) || mod.GlobalConstants.TryGetValue(dotPiece, out res))))
                                    {
                                        foundInclude = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (foundInclude)
                            continue;

                        if (ast?._databaseProvider != null)
                        {
                            res = ast._databaseProvider.GetTable(dotPiece);
                            if (res != null)
                                continue;
                        }
                    }

                    // Only do a public function search if the dotPiece is the whole text we're searching for
                    // I.e. no namespaces
                    if (lookForFunctions && dotPiece == exprText)   
                    {
                        if (searchInFunctionProvider == FunctionProviderSearchMode.Search)
                        {
                            if (res == null && ast?._functionProvider != null)
                            {
                                // check for the function name in the function provider
                                var funcs = ast._functionProvider.GetFunction(dotPiece);
                                if (funcs != null)
                                {
                                    res = funcs.FirstOrDefault();
                                    if (res != null)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        else if (searchInFunctionProvider == FunctionProviderSearchMode.Deferred)
                        {
                            isDeferredFunction = true;
                        }
                    }

                    if (res == null)
                        break;
                }
            }

            return res;
        }

        /// <summary>
        /// Evaluates the given expression in at the provided line number and returns the values
        /// that the expression can evaluate to.
        /// </summary>
        /// <param name="exprText">The expression to determine the result of.</param>
        /// <param name="index">The 0-based absolute index into the file where the expression should be evaluated within the module.</param>
        public override IAnalysisResult GetValueByIndex(string exprText, int index, IFunctionInformationProvider functionProvider,
                                               IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider,
                                               bool isFunctionCallOrDefinition, out bool isDeferred,
                                               out IGeneroProject definingProject, out IProjectEntry projectEntry, 
                                               FunctionProviderSearchMode searchInFunctionProvider = FunctionProviderSearchMode.NoSearch)
        {
            _functionProvider = functionProvider;
            _databaseProvider = databaseProvider;
            _programFileProvider = programFileProvider;

            return GetValueByIndex(exprText, index, this, out definingProject, out projectEntry, out isDeferred,
                                   searchInFunctionProvider, 
                                   isFunctionCallOrDefinition);
        }

        /// <summary>
        /// Gets the variables the given expression evaluates to.  Variables include parameters, locals, and fields assigned on classes, modules and instances.
        /// 
        /// Variables are classified as either definitions or references.  Only parameters have unique definition points - all other types of variables
        /// have only one or more references.
        /// 
        /// index is a 0-based absolute index into the file.
        /// </summary>
        public override IEnumerable<IAnalysisVariable> GetVariablesByIndex(string exprText, int index, IFunctionInformationProvider functionProvider,
                                                                  IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider,
                                                                  bool isFunctionCallOrDefinition)
        {
            _functionProvider = functionProvider;
            _databaseProvider = databaseProvider;
            _programFileProvider = programFileProvider;

            List<IAnalysisVariable> vars = new List<IAnalysisVariable>();

            _body.SetNamespace(null);
            AstNode containingNode = GetContainingNode(_body, index);
            if (containingNode != null)
            {
                if (!isFunctionCallOrDefinition)
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

                        List<Tuple<IAnalysisResult, IndexSpan>> limitedScopeVars;
                        if(func.LimitedScopeVariables.TryGetValue(exprText, out limitedScopeVars))
                        {
                            foreach(var item in limitedScopeVars)
                            {
                                if(item.Item2.IsInSpan(index))
                                {
                                    vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Reference));
                                }
                            }
                        }
                    }
                }

                if (_body is IModuleResult)
                {
                    // check for module vars, types, and constants (and globals defined in this module)
                    IModuleResult mod = _body as IModuleResult;
                    IAnalysisResult res;

                    if (!isFunctionCallOrDefinition)
                    {
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
                    }
                    else
                    {
                        // check for module functions
                        IFunctionResult funcRes;
                        if (mod.Functions.TryGetValue(exprText, out funcRes))
                        {
                            vars.Add(new AnalysisVariable(this.ResolveLocation(funcRes), VariableType.Definition, funcRes.Name, 1));
                        }
                    }
                }
            }

            // TODO: this could probably be done more efficiently by having each GeneroAst load globals and functions into
            // dictionaries stored on the IGeneroProject, instead of in each project entry.
            // However, this does required more upkeep when changes occur. Will look into it...
            if (_projEntry != null && _projEntry is IGeneroProjectEntry)
            {
                IGeneroProjectEntry genProj = _projEntry as IGeneroProjectEntry;
                if (genProj.ParentProject != null &&
                    !genProj.FilePath.ToLower().EndsWith(".inc"))
                {
                    foreach (var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                    {
                        if (projEntry.Value.Analysis != null &&
                           projEntry.Value.Analysis.Body != null)
                        {
                            projEntry.Value.Analysis.Body.SetNamespace(null);
                            IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                            if (modRes != null)
                            {
                                if (!isFunctionCallOrDefinition)
                                {
                                    // check global vars, types, and constants
                                    // TODO: need to introduce option to enable/disable legacy linking
                                    IAnalysisResult res;
                                    if (modRes.GlobalVariables.TryGetValue(exprText, out res) || modRes.Variables.TryGetValue(exprText, out res))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                    }

                                    if (modRes.GlobalTypes.TryGetValue(exprText, out res) || modRes.Types.TryGetValue(exprText, out res))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                    }

                                    if (modRes.GlobalConstants.TryGetValue(exprText, out res) || modRes.Constants.TryGetValue(exprText, out res))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                    }

                                    // check for cursors in this module
                                    if (modRes.Cursors.TryGetValue(exprText, out res))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                    }
                                }
                                else
                                {
                                    // check for module functions
                                    IFunctionResult funcRes;
                                    if (modRes.Functions.TryGetValue(exprText, out funcRes))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(funcRes), VariableType.Definition, funcRes.Name, 1));
                                    }
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

            if (isFunctionCallOrDefinition && _functionProvider != null)
            {
                var funcRes = _functionProvider.GetFunction(exprText);
                if (funcRes != null)
                {
                    vars.AddRange(funcRes.Select(x => new AnalysisVariable(x.Location, VariableType.Definition, x.Name, 2)));
                }

                IFunctionResult funcResult;
                if (SystemFunctions.TryGetValue(exprText, out funcResult))
                {
                    vars.Add(new AnalysisVariable(funcResult.Location, VariableType.Definition, funcResult.Name, 2));
                }
            }

            if (exprText.Contains('.') && vars.Count == 0)
            {
                IGeneroProject definingProj;
                IProjectEntry projEntry;
                bool dummyDef;
                IAnalysisResult res = GetValueByIndex(exprText, index, functionProvider, databaseProvider, _programFileProvider, isFunctionCallOrDefinition, out dummyDef, out definingProj, out projEntry);
                if (res != null)
                {
                    LocationInfo locInfo = null;
                    if (definingProj != null || projEntry != null)
                    {
                        locInfo = ResolveLocationInternal(definingProj, projEntry, res);
                    }
                    else
                        locInfo = this.ResolveLocation(res);
                    if (locInfo != null && 
                        (
                            (locInfo.Index > 0 || (locInfo.Line > 0 && locInfo.Column > 0)) ||
                            !string.IsNullOrWhiteSpace(locInfo.DefinitionURL)
                        )
                       )
                        vars.Add(new AnalysisVariable(locInfo, VariableType.Definition));
                }
            }

            if (_body is IModuleResult &&
                _projEntry is IGeneroProjectEntry)
            {
                string dotPiece = exprText;
                string[] dotPieces = exprText.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (dotPieces.Length > 1)
                    dotPiece = dotPieces[0];
                if ((_body as IModuleResult).FglImports.Contains(dotPiece))
                {
                    // need to get the ast for the other project entry
                    var refProjKVP = (_projEntry as IGeneroProjectEntry).ParentProject.ReferencedProjects.Values.FirstOrDefault(
                        x =>
                        {
                            var fn = Path.GetFileName(x.Directory);
                            return fn?.Equals(dotPiece, StringComparison.OrdinalIgnoreCase) ?? false;
                        });
                    if (refProjKVP is IAnalysisResult)
                    {
                        IProjectEntry dummyEntry;
                        bool dummyDef;
                        IAnalysisResult res = GetValueByIndex(exprText, index, functionProvider, databaseProvider, _programFileProvider, isFunctionCallOrDefinition, out dummyDef, out refProjKVP, out dummyEntry);
                        if (res != null)
                        {
                            LocationInfo locInfo = null;
                            locInfo = refProjKVP != null ? ResolveLocationInternal(refProjKVP, dummyEntry, res) : this.ResolveLocation(res);
                            if (locInfo != null && (locInfo.Index > 0 || (locInfo.Line > 0 && locInfo.Column > 0)))
                                vars.Add(new AnalysisVariable(locInfo, VariableType.Definition));
                        }
                    }
                }
            }

            if (!isFunctionCallOrDefinition)
            {
                foreach (var includeFile in this.ProjectEntry.GetIncludedFiles())
                {
                    IAnalysisResult res;
                    if (includeFile.Analysis != null &&
                       includeFile.Analysis.Body is IModuleResult)
                    {
                        includeFile.Analysis.Body.SetNamespace(null);
                        var mod = includeFile.Analysis.Body as IModuleResult;
                        if (mod.Types.TryGetValue(exprText, out res))
                        {
                            vars.Add(new AnalysisVariable(ResolveLocationInternal(null, includeFile, res), VariableType.Definition));
                        }
                        if (mod.Constants.TryGetValue(exprText, out res))
                        {
                            vars.Add(new AnalysisVariable(ResolveLocationInternal(null, includeFile, res), VariableType.Definition));
                        }
                    }
                }
            }

            return vars;
        }

        //public IEnumerable<IndexSpan> FindAllReferences(IAnalysisResult item)
        //{
        //    List<IndexSpan> references = new List<IndexSpan>();

        //    if (item.LocationIndex >= 0)
        //    {
        //        if ((item is AstNode && (item as AstNode).SyntaxTree == this) ||
        //            (item.Location.ProjectEntry != null && item.Location.ProjectEntry == this.ProjectEntry) ||
        //            (!string.IsNullOrWhiteSpace(item.Location.FilePath) && item.Location.FilePath == this._filename))
        //        {
        //            references.Add(new IndexSpan(item.LocationIndex, item.Name.Length));

        //            // TODO: traverse the entire children tree of the the module node (body)
        //            var containingNode = GetContainingNode(Body, item.LocationIndex);
        //            if (containingNode != null && containingNode != Body && containingNode is IFunctionResult)
        //            {
        //                containingNode.FindAllReferences(item, references);
        //            }
        //            else
        //            {
        //                Body.FindAllReferences(item, references);
        //            }
        //        }
        //    }
        //    return references;
        //}
    }
}
