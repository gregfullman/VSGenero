using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// [ <see cref="CompilerOptionsNode"/>
    /// | <see cref="ImportModuleNode"/> [...]
    /// | <see cref="SchemaSpecificationNode"/>
    /// | <see cref="GlobalsNode"/>
    /// | <see cref="ConstantDefNode"/> [...]
    /// | <see cref="TypeDefNode"/> [...]
    /// | <see cref="DefineNode"/> [...]
    /// ]
    /// 
    /// [ <see cref="MainBlockNode"/> ]
    /// 
    /// [ declared-dialog-block
    /// | function-declaration
    /// | report-declaration
    ///     [...] ]
    /// ]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_019.html
    /// </summary>
    public class ModuleNode : AstNode, IModuleResult
    {
        private enum NodesProcessed
        {
            None = 0,
            CompilerOption,
            Imports,
            SchemaSpec,
            Globals,
            Constants,
            TypeDefs,
            VarDefs,
            Main,
            Body
        }

        private static TokenKind[] ValidKeywords = new TokenKind[]
        {
            TokenKind.OptionsKeyword,
            TokenKind.ImportKeyword,
            TokenKind.SchemaKeyword,
            TokenKind.DescribeKeyword,
            TokenKind.DatabaseKeyword,
            TokenKind.GlobalsKeyword,
            TokenKind.ConstantKeyword,
            TokenKind.TypeKeyword,
            TokenKind.DefineKeyword,
            TokenKind.MainKeyword,
            TokenKind.FunctionKeyword,
            TokenKind.ReportKeyword,
            TokenKind.PrivateKeyword,
            TokenKind.PublicKeyword
        };

        private static bool CheckForPreprocessorNode(Parser parser)
        {
            PreprocessorNode preNode;
            if (PreprocessorNode.TryParseNode(parser, out preNode))
            {
                // not storing it right now
                return true;
            }
            return false;
        }

        public static bool TryParseNode(Parser parser, out ModuleNode defNode)
        {
            defNode = new ModuleNode();
            NodesProcessed processed = NodesProcessed.None;
            
            while (!parser.PeekToken(TokenKind.EndOfFile))
            {
                if (CheckForPreprocessorNode(parser))
                    continue;

                CompilerOptionsNode compOptionsNode;
                if (CompilerOptionsNode.TryParseNode(parser, out compOptionsNode))
                {
                    if (processed == NodesProcessed.None)
                    {
                        defNode.Children.Add(compOptionsNode.StartIndex, compOptionsNode);
                    }
                    else
                    {
                        parser.ReportSyntaxError("Compiler options statement found in incorrect position.");
                    }
                }
                if(processed == NodesProcessed.None)
                    processed = NodesProcessed.CompilerOption;

                if (CheckForPreprocessorNode(parser))
                    continue;

                ImportModuleNode importNode;
                while (ImportModuleNode.TryParseNode(parser, out importNode))
                {
                    if (processed == NodesProcessed.CompilerOption)
                    {
                        defNode.Children.Add(importNode.StartIndex, importNode);
                        continue;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Import statement found in incorrect position.");
                    }
                }
                if(processed == NodesProcessed.CompilerOption)
                    processed = NodesProcessed.Imports;

                if (CheckForPreprocessorNode(parser))
                    continue;

                SchemaSpecificationNode schemaNode;
                if (SchemaSpecificationNode.TryParseDefine(parser, out schemaNode))
                {
                    if (processed == NodesProcessed.Imports)
                    {
                        defNode.Children.Add(schemaNode.StartIndex, schemaNode);
                    }
                    else
                    {
                        parser.ReportSyntaxError("Schema statement found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.Imports)
                    processed = NodesProcessed.SchemaSpec;

                if (CheckForPreprocessorNode(parser))
                    continue;

                GlobalsNode globalNode;
                while (GlobalsNode.TryParseNode(parser, out globalNode))
                {
                    if (processed == NodesProcessed.SchemaSpec)
                    {
                        defNode.Children.Add(globalNode.StartIndex, globalNode);
                        foreach (var cGlobKVP in globalNode.Constants)
                        {
                            if (!defNode.GlobalConstants.ContainsKey(cGlobKVP.Key))
                                defNode.GlobalConstants.Add(cGlobKVP);
                            else
                                parser.ReportSyntaxError(cGlobKVP.Value.LocationIndex, cGlobKVP.Value.LocationIndex + cGlobKVP.Value.Name.Length, string.Format("Global constant {0} defined more than once.", cGlobKVP.Key));
                        }
                        foreach (var tGlobKVP in globalNode.Types)
                        {
                            if(!defNode.GlobalTypes.ContainsKey(tGlobKVP.Key))
                                defNode.GlobalTypes.Add(tGlobKVP);
                            else
                                parser.ReportSyntaxError(tGlobKVP.Value.LocationIndex, tGlobKVP.Value.LocationIndex + tGlobKVP.Value.Name.Length, string.Format("Global type {0} defined more than once.", tGlobKVP.Key));
                        }
                        foreach (var vGlobKVP in globalNode.Variables)
                        {
                            if(!defNode.GlobalVariables.ContainsKey(vGlobKVP.Key))
                                defNode.GlobalVariables.Add(vGlobKVP);
                            else
                                parser.ReportSyntaxError(vGlobKVP.Value.LocationIndex, vGlobKVP.Value.LocationIndex + vGlobKVP.Value.Name.Length, string.Format("Global variable {0} defined more than once.", vGlobKVP.Key));
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Globals statement found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.SchemaSpec)
                    processed = NodesProcessed.Globals;

                if (CheckForPreprocessorNode(parser))
                    continue;

                bool matchedBreakSequence = false;
                ConstantDefNode constNode;
                List<List<TokenKind>> breakSequences = new List<List<TokenKind>>() 
                    { 
                        new List<TokenKind> { TokenKind.TypeKeyword },
                        new List<TokenKind> { TokenKind.DefineKeyword },
                        new List<TokenKind> { TokenKind.FunctionKeyword },
                        new List<TokenKind> { TokenKind.MainKeyword },
                        new List<TokenKind> { TokenKind.ReportKeyword }
                    };
                while (ConstantDefNode.TryParseNode(parser, out constNode, out matchedBreakSequence, breakSequences))
                {
                    if (processed == NodesProcessed.Globals)
                    {
                        defNode.Children.Add(constNode.StartIndex, constNode);
                        foreach (var def in constNode.GetDefinitions())
                        {
                            def.Scope = "module constant";
                            if(!defNode.Constants.ContainsKey(def.Name))
                                defNode.Constants.Add(def.Name, def);
                            else
                                parser.ReportSyntaxError(def.LocationIndex, def.LocationIndex + def.Name.Length, string.Format("Module constant {0} defined more than once.", def.Name));
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Constant definition found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.Globals)
                    processed = NodesProcessed.Constants;

                if (CheckForPreprocessorNode(parser))
                    continue;

                TypeDefNode typeNode;
                breakSequences = new List<List<TokenKind>>() 
                    { 
                        new List<TokenKind> { TokenKind.ConstantKeyword },
                        new List<TokenKind> { TokenKind.DefineKeyword },
                        new List<TokenKind> { TokenKind.TypeKeyword },
                        new List<TokenKind> { TokenKind.FunctionKeyword },
                        new List<TokenKind> { TokenKind.MainKeyword },
                        new List<TokenKind> { TokenKind.ReportKeyword }
                    };
                while (TypeDefNode.TryParseNode(parser, out typeNode, out matchedBreakSequence, breakSequences))
                {
                    if (processed == NodesProcessed.Constants)
                    {
                        defNode.Children.Add(typeNode.StartIndex, typeNode);
                        foreach (var def in typeNode.GetDefinitions())
                        {
                            def.Scope = "module type";
                            if(!defNode.Types.ContainsKey(def.Name))
                                defNode.Types.Add(def.Name, def);
                            else
                                parser.ReportSyntaxError(def.LocationIndex, def.LocationIndex + def.Name.Length, string.Format("Module type {0} defined more than once.", def.Name));
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Type definition found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.Constants)
                    processed = NodesProcessed.TypeDefs;

                if (CheckForPreprocessorNode(parser))
                    continue;

                DefineNode defineNode;
                breakSequences = new List<List<TokenKind>>() 
                    { 
                        new List<TokenKind> { TokenKind.TypeKeyword },
                        new List<TokenKind> { TokenKind.ConstantKeyword },
                        new List<TokenKind> { TokenKind.FunctionKeyword },
                        new List<TokenKind> { TokenKind.MainKeyword },
                        new List<TokenKind> { TokenKind.ReportKeyword }
                    };
                while (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences))
                {
                    if (processed == NodesProcessed.TypeDefs)
                    {
                        defNode.Children.Add(defineNode.StartIndex, defineNode);
                        foreach(var def in defineNode.GetDefinitions())
                            foreach (var vardef in def.VariableDefinitions)
                            {
                                vardef.Scope = "module variable";
                                if(!defNode.Variables.ContainsKey(vardef.Name))
                                    defNode.Variables.Add(vardef.Name, vardef);
                                else
                                    parser.ReportSyntaxError(vardef.LocationIndex, vardef.LocationIndex + vardef.Name.Length, string.Format("Module variable {0} defined more than once.", vardef.Name));
                            }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Variable definition found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.TypeDefs)
                    processed = NodesProcessed.VarDefs;

                if (CheckForPreprocessorNode(parser))
                    continue;

                MainBlockNode mainBlock;
                if (MainBlockNode.TryParseNode(parser, out mainBlock, defNode.PreparedCursorResolver))
                {
                    if (processed == NodesProcessed.VarDefs)
                    {
                        defNode.Children.Add(mainBlock.StartIndex, mainBlock);
                        defNode.Functions.Add(mainBlock.Name, mainBlock);
                        foreach(var cursor in mainBlock.Children.Values.Where(x => x is PrepareStatement || x is DeclareStatement))
                        {
                            IAnalysisResult curRes = cursor as IAnalysisResult;
                            if(!defNode.Cursors.ContainsKey(curRes.Name))
                                defNode.Cursors.Add(curRes.Name, curRes);
                            else
                                parser.ReportSyntaxError(curRes.LocationIndex, curRes.LocationIndex + curRes.Name.Length, string.Format("Module variable {0} defined more than once.", curRes.Name));
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Main block found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.VarDefs)
                    processed = NodesProcessed.Main;

                if (CheckForPreprocessorNode(parser))
                    continue;

                // TODO: declared dialog block

                FunctionBlockNode funcNode;
                ReportBlockNode repNode;
                if (FunctionBlockNode.TryParseNode(parser, out funcNode, defNode.PreparedCursorResolver))
                {
                    defNode.Children.Add(funcNode.StartIndex, funcNode);
                    funcNode.Scope = "function";
                    if (!defNode.Functions.ContainsKey(funcNode.Name))
                    {
                        defNode.Functions.Add(funcNode.Name, funcNode);
                        foreach (var cursor in funcNode.Children.Values.Where(x => x is PrepareStatement || x is DeclareStatement))
                        {
                            IAnalysisResult curRes = cursor as IAnalysisResult;
                            defNode.Cursors.Add(curRes.Name, curRes);
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError(funcNode.LocationIndex, funcNode.LocationIndex + funcNode.Name.Length, string.Format("Function {0} defined more than once.", funcNode.Name));
                    }
                }
                else if (ReportBlockNode.TryParseNode(parser, out repNode, defNode.PreparedCursorResolver))
                {
                    defNode.Children.Add(repNode.StartIndex, repNode);
                    repNode.Scope = "report";
                    if (!defNode.Functions.ContainsKey(repNode.Name))
                    {
                        defNode.Functions.Add(repNode.Name, repNode);
                        foreach (var cursor in repNode.Children.Values.Where(x => x is PrepareStatement || x is DeclareStatement))
                        {
                            IAnalysisResult curRes = cursor as IAnalysisResult;
                            defNode.Cursors.Add(curRes.Name, curRes);
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError(repNode.LocationIndex, repNode.LocationIndex + repNode.Name.Length, string.Format("Function {0} defined more than once.", repNode.Name));
                    }
                }
                else
                {
                    parser.NextToken();
                }
            }

            if (defNode.Children.Count > 0)
            {
                defNode.StartIndex = defNode.Children[defNode.Children.Keys[0]].StartIndex;
                defNode.EndIndex = defNode.Children[defNode.Children.Keys[defNode.Children.Count - 1]].EndIndex;
            }

            return true;
        }

        private Dictionary<string, IAnalysisResult> _variables;
        public IDictionary<string, IAnalysisResult> Variables
        {
            get
            {
                if (_variables == null)
                    _variables = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _variables;
            }
        }

        private Dictionary<string, IAnalysisResult> _types;
        public IDictionary<string, IAnalysisResult> Types
        {
            get
            {
                if (_types == null)
                    _types = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _types;
            }
        }

        private Dictionary<string, IAnalysisResult> _constants;
        public IDictionary<string, IAnalysisResult> Constants
        {
            get
            {
                if (_constants == null)
                    _constants = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _constants;
            }
        }

        private Dictionary<string, IAnalysisResult> _globalVariables;
        public IDictionary<string, IAnalysisResult> GlobalVariables
        {
            get
            {
                if (_globalVariables == null)
                    _globalVariables = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _globalVariables;
            }
        }

        private Dictionary<string, IAnalysisResult> _globalTypes;
        public IDictionary<string, IAnalysisResult> GlobalTypes
        {
            get
            {
                if (_globalTypes == null)
                    _globalTypes = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _globalTypes;
            }
        }

        private Dictionary<string, IAnalysisResult> _globalConstants;
        public IDictionary<string, IAnalysisResult> GlobalConstants
        {
            get
            {
                if (_globalConstants == null)
                    _globalConstants = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _globalConstants;
            }
        }

        private Dictionary<string, IFunctionResult> _functions = new Dictionary<string, IFunctionResult>();
        public IDictionary<string, IFunctionResult> Functions
        {
            get
            {
                if (_functions == null)
                    _functions = new Dictionary<string, IFunctionResult>(StringComparer.OrdinalIgnoreCase);
                return _functions;
            }
        }

        private void BindCursorResult(IAnalysisResult cursorResult)
        {
            if(!Cursors.ContainsKey(cursorResult.Name))
            {
                Cursors.Add(cursorResult.Name, cursorResult);
            }
        }

        private PrepareStatement PreparedCursorResolver(string prepIdent)
        {
            IAnalysisResult prepRes;
            if(Cursors.TryGetValue(prepIdent, out prepRes))
            {
                if(prepRes is PrepareStatement)
                {
                    return prepRes as PrepareStatement;
                }
            }
            return null;
        }

        private Dictionary<string, IAnalysisResult> _cursors;
        public IDictionary<string, IAnalysisResult> Cursors
        {
            get
            {
                if (_cursors == null)
                    _cursors = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _cursors;
            }
        }

        public override IEnumerable<MemberResult> GetValidMembersByContext(int index, IReverseTokenizer revTokenizer, GeneroAst ast, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            // do a binary search to determine what node we're in
            List<int> keys = Children.Select(x => x.Key).ToList();
            int searchIndex = keys.BinarySearch(index);
            if (searchIndex < 0)
            {
                searchIndex = ~searchIndex;
                if (searchIndex > 0)
                    searchIndex--;
            }

            int key = keys[searchIndex];

            // TODO: need to handle multiple results of the same name
            AstNode containingNode = Children[key];
            if (containingNode != null)
            {
                if (containingNode.StartIndex < index && containingNode.EndIndex >= index)
                {
                    return containingNode.GetValidMembersByContext(index, revTokenizer, ast, options);
                }
                else
                {
                    // try to narrow down valid keywords by checking the previous node
                    int tokenKindLimit = 0;
                    if (containingNode is CompilerOptionsNode)
                        tokenKindLimit = 1;
                    else if (containingNode is ImportModuleNode)
                        tokenKindLimit = 2;
                    else if (containingNode is SchemaSpecificationNode)
                        tokenKindLimit = 5;
                    else if (containingNode is GlobalsNode)
                        tokenKindLimit = 5;
                    else if (containingNode is ConstantDefNode)
                        tokenKindLimit = 6;
                    else if (containingNode is TypeDefNode)
                        tokenKindLimit = 7;
                    else if (containingNode is DefineNode)
                        tokenKindLimit = 8;
                    else if (containingNode is MainBlockNode)
                        tokenKindLimit = 10;
                    else
                        tokenKindLimit = 10;
                    return ValidKeywords.SubArray(tokenKindLimit, ValidKeywords.Length - tokenKindLimit)
                                        .Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, ast));
                }
            }
            else
            {
                // TODO: return all valid keywords 
                return ValidKeywords.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, ast));
            }
        }


    }
}
