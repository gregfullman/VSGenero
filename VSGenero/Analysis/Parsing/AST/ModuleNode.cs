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
            MemberDefinitions,
            Main,
            Body
        }

        private List<string> _cExtensionImports;
        public List<string> CExtensionImports
        {
            get
            {
                if (_cExtensionImports == null)
                    _cExtensionImports = new List<string>();
                return _cExtensionImports;
            }
        }

        private List<string> _javaImports;
        public List<string> JavaImports
        {
            get
            {
                if (_javaImports == null)
                    _javaImports = new List<string>();
                return _javaImports;
            }
        }

        private HashSet<string> _fglImports;
        public HashSet<string> FglImports
        {
            get
            {
                if (_fglImports == null)
                    _fglImports = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                return _fglImports;
            }
        }

        private Dictionary<string, List<PreprocessorNode>> _includeFiles;
        public Dictionary<string, List<PreprocessorNode>> IncludeFiles
        {
            get
            {
                if (_includeFiles == null)
                    _includeFiles = new Dictionary<string, List<PreprocessorNode>>(StringComparer.OrdinalIgnoreCase);
                return _includeFiles;
            }
        }

        private static bool CheckForPreprocessorNode(Parser parser, ModuleNode node)
        {
            PreprocessorNode preNode;
            if (PreprocessorNode.TryParseNode(parser, out preNode))
            {
                if(preNode.Type == PreprocessorType.Include && !string.IsNullOrWhiteSpace(preNode.IncludeFile))
                {
                    if(node.IncludeFiles.ContainsKey(preNode.IncludeFile))
                    {
                        node.IncludeFiles[preNode.IncludeFile].Add(preNode);
                    }
                    else
                    {
                        node.IncludeFiles.Add(preNode.IncludeFile, new List<PreprocessorNode>() { preNode });
                    }
                }
                
                //if(preNode.Type == PreprocessorType.Include &&
                //   !string.IsNullOrWhiteSpace(preNode.IncludeFile) &&
                //   VSGeneroPackage.Instance.ProgramFileProvider != null)
                //{
                //    var fullFilename = VSGeneroPackage.Instance.ProgramFileProvider.GetIncludeFile(preNode.IncludeFile);
                //    if(!string.IsNullOrWhiteSpace(fullFilename))
                //    {
                //        parser.CreateIncludeParser(fullFilename);
                //    }
                //}
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
                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                CompilerOptionsNode compOptionsNode;
                if (CompilerOptionsNode.TryParseNode(parser, out compOptionsNode) && compOptionsNode != null)
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
                if (processed == NodesProcessed.None)
                    processed = NodesProcessed.CompilerOption;

                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                ImportModuleNode importNode;
                if (ImportModuleNode.TryParseNode(parser, out importNode) && importNode != null)
                {
                    if (processed == NodesProcessed.CompilerOption)
                    {
                        if (!defNode.Children.ContainsKey(importNode.StartIndex))
                        {
                            defNode.Children.Add(importNode.StartIndex, importNode);
                            if (!string.IsNullOrWhiteSpace(importNode.ImportName))
                            {
                                if (importNode.ImportType == ImportModuleType.C)
                                    defNode.CExtensionImports.Add(importNode.ImportName);
                                else if (importNode.ImportType == ImportModuleType.Java)
                                    defNode.JavaImports.Add(importNode.ImportName);
                                else
                                    defNode.FglImports.Add(importNode.ImportName);
                            }
                        }
                        continue;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Import statement found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.CompilerOption)
                    processed = NodesProcessed.Imports;

                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                SchemaSpecificationNode schemaNode;
                if (SchemaSpecificationNode.TryParseDefine(parser, out schemaNode) && schemaNode != null)
                {
                    if (processed == NodesProcessed.Imports)
                    {
                        if(!defNode.Children.ContainsKey(schemaNode.StartIndex))
                            defNode.Children.Add(schemaNode.StartIndex, schemaNode);
                    }
                    else
                    {
                        parser.ReportSyntaxError("Schema statement found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.Imports)
                    processed = NodesProcessed.SchemaSpec;

                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                GlobalsNode globalNode;
                if (GlobalsNode.TryParseNode(parser, out globalNode) && globalNode != null)
                {
                    if (processed == NodesProcessed.SchemaSpec || processed == NodesProcessed.MemberDefinitions)
                    {
                        defNode.Children.Add(globalNode.StartIndex, globalNode);
                        foreach (var cGlobKVP in globalNode.Constants)
                        {
                            if (!defNode.GlobalConstants.ContainsKey(cGlobKVP.Key))
                                defNode.GlobalConstants.Add(cGlobKVP);
                            else
                                parser.ReportSyntaxError(cGlobKVP.Value.LocationIndex, cGlobKVP.Value.LocationIndex + cGlobKVP.Value.Name.Length, string.Format("Global constant {0} defined more than once.", cGlobKVP.Key), Severity.Warning);
                        }
                        foreach (var tGlobKVP in globalNode.Types)
                        {
                            if (!defNode.GlobalTypes.ContainsKey(tGlobKVP.Key))
                                defNode.GlobalTypes.Add(tGlobKVP);
                            else
                                parser.ReportSyntaxError(tGlobKVP.Value.LocationIndex, tGlobKVP.Value.LocationIndex + tGlobKVP.Value.Name.Length, string.Format("Global type {0} defined more than once.", tGlobKVP.Key), Severity.Warning);
                        }
                        foreach (var vGlobKVP in globalNode.Variables)
                        {
                            if (!defNode.GlobalVariables.ContainsKey(vGlobKVP.Key))
                                defNode.GlobalVariables.Add(vGlobKVP);
                            else
                                parser.ReportSyntaxError(vGlobKVP.Value.LocationIndex, vGlobKVP.Value.LocationIndex + vGlobKVP.Value.Name.Length, string.Format("Global variable {0} defined more than once.", vGlobKVP.Key), Severity.Warning);
                        }
                        continue;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Globals statement found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.SchemaSpec)
                    processed = NodesProcessed.MemberDefinitions;

                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                bool matchedBreakSequence = false;
                ConstantDefNode constNode;
                List<List<TokenKind>> breakSequences = new List<List<TokenKind>>() 
                    {
                        new List<TokenKind> { TokenKind.GlobalsKeyword },
                        new List<TokenKind> { TokenKind.PublicKeyword },
                        new List<TokenKind> { TokenKind.PrivateKeyword },
                        new List<TokenKind> { TokenKind.ConstantKeyword },
                        new List<TokenKind> { TokenKind.DefineKeyword },
                        new List<TokenKind> { TokenKind.TypeKeyword },
                        new List<TokenKind> { TokenKind.FunctionKeyword },
                        new List<TokenKind> { TokenKind.MainKeyword },
                        new List<TokenKind> { TokenKind.ReportKeyword }
                    };
                if (ConstantDefNode.TryParseNode(parser, out constNode, out matchedBreakSequence, breakSequences) && constNode != null)
                {
                    if (processed == NodesProcessed.SchemaSpec || processed == NodesProcessed.MemberDefinitions)
                    {
                        defNode.Children.Add(constNode.StartIndex, constNode);
                        foreach (var def in constNode.GetDefinitions())
                        {
                            def.Scope = "module constant";
                            if (!defNode.Constants.ContainsKey(def.Name))
                                defNode.Constants.Add(def.Name, def);
                            else
                                parser.ReportSyntaxError(def.LocationIndex, def.LocationIndex + def.Name.Length, string.Format("Module constant {0} defined more than once.", def.Name), Severity.Warning);
                        }
                        continue;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Constant definition found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.SchemaSpec)
                    processed = NodesProcessed.MemberDefinitions;

                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                TypeDefNode typeNode;
                if (TypeDefNode.TryParseNode(parser, out typeNode, out matchedBreakSequence, breakSequences) && typeNode != null)
                {
                    if (processed == NodesProcessed.SchemaSpec || processed == NodesProcessed.MemberDefinitions)
                    {
                        if(!defNode.Children.ContainsKey(typeNode.StartIndex))
                            defNode.Children.Add(typeNode.StartIndex, typeNode);
                        foreach (var def in typeNode.GetDefinitions())
                        {
                            def.Scope = "module type";
                            if (!defNode.Types.ContainsKey(def.Name))
                                defNode.Types.Add(def.Name, def);
                            else
                                parser.ReportSyntaxError(def.LocationIndex, def.LocationIndex + def.Name.Length, string.Format("Module type {0} defined more than once.", def.Name), Severity.Warning);
                        }
                        continue;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Type definition found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.SchemaSpec)
                    processed = NodesProcessed.MemberDefinitions;

                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                DefineNode defineNode;
                if (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences) && defineNode != null)
                {
                    if (processed == NodesProcessed.SchemaSpec || processed == NodesProcessed.MemberDefinitions)
                    {
                        defNode.Children.Add(defineNode.StartIndex, defineNode);
                        foreach (var def in defineNode.GetDefinitions())
                            foreach (var vardef in def.VariableDefinitions)
                            {
                                vardef.Scope = "module variable";
                                vardef.SetIsPublic(defineNode.AccessModifier == AccessModifier.Public);
                                if (!defNode.Variables.ContainsKey(vardef.Name))
                                    defNode.Variables.Add(vardef.Name, vardef);
                                else
                                    parser.ReportSyntaxError(vardef.LocationIndex, vardef.LocationIndex + vardef.Name.Length, string.Format("Module variable {0} defined more than once.", vardef.Name), Severity.Warning);
                            }
                        continue;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Variable definition found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.SchemaSpec)
                    processed = NodesProcessed.MemberDefinitions;

                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                MainBlockNode mainBlock;
                if (MainBlockNode.TryParseNode(parser, out mainBlock, defNode) && mainBlock != null)
                {
                    if (processed == NodesProcessed.MemberDefinitions)
                    {
                        defNode.Children.Add(mainBlock.StartIndex, mainBlock);
                        defNode.Functions.Add(mainBlock.Name, mainBlock);
                        foreach (var cursor in mainBlock.Children.Values.Where(x => x is PrepareStatement || x is DeclareStatement))
                        {
                            IAnalysisResult curRes = cursor as IAnalysisResult;
                            if (!defNode.Cursors.ContainsKey(curRes.Name))
                                defNode.Cursors.Add(curRes.Name, curRes);
                            else
                                parser.ReportSyntaxError(curRes.LocationIndex, curRes.LocationIndex + curRes.Name.Length, string.Format("Module variable {0} defined more than once.", curRes.Name), Severity.Warning);
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Main block found in incorrect position.");
                    }
                }
                if (processed == NodesProcessed.MemberDefinitions)
                    processed = NodesProcessed.Main;

                if (CheckForPreprocessorNode(parser, defNode))
                    continue;

                // TODO: declared dialog block

                FunctionBlockNode funcNode;
                ReportBlockNode repNode;
                int dummy;
                if (FunctionBlockNode.TryParseNode(parser, out funcNode, out dummy, defNode) && funcNode != null)
                {
                    defNode.Children.Add(funcNode.StartIndex, funcNode);
                    funcNode.Scope = "function";
                    
                    if(string.IsNullOrWhiteSpace(funcNode.Name))
                    {
                        parser.ReportSyntaxError(funcNode.LocationIndex, funcNode.LocationIndex, "Invalid function definition found.");
                    }
                    else if (!defNode.Functions.ContainsKey(funcNode.Name))
                    {
                        defNode.Functions.Add(funcNode.Name, funcNode);
                    }
                    else
                    {
                        parser.ReportSyntaxError(funcNode.LocationIndex, funcNode.LocationIndex + funcNode.Name.Length, string.Format("Function {0} defined more than once.", funcNode.Name), Severity.Warning);
                    }
                }
                else if (ReportBlockNode.TryParseNode(parser, out repNode, defNode) && repNode != null)
                {
                    defNode.Children.Add(repNode.StartIndex, repNode);
                    repNode.Scope = "report";
                    if (string.IsNullOrWhiteSpace(repNode.Name))
                    {
                        parser.ReportSyntaxError(repNode.LocationIndex, repNode.LocationIndex, "Invalid report definition found.");
                    }
                    else if (!defNode.Functions.ContainsKey(repNode.Name))
                    {
                        defNode.Functions.Add(repNode.Name, repNode);
                    }
                    else
                    {
                        parser.ReportSyntaxError(repNode.LocationIndex, repNode.LocationIndex + repNode.Name.Length, string.Format("Report {0} defined more than once.", repNode.Name), Severity.Warning);
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
                defNode.IsComplete = true;
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

        public void BindCursorResult(IAnalysisResult cursorResult, IParser parser)
        {
            if (!Cursors.ContainsKey(cursorResult.Name))
                Cursors.Add(cursorResult.Name, cursorResult);
            else
                parser.ReportSyntaxError(cursorResult.LocationIndex, cursorResult.LocationIndex + cursorResult.Name.Length, string.Format("Module cursor {0} defined more than once.", cursorResult.Name), Severity.Warning);
        }

        public PrepareStatement PreparedCursorResolver(string prepIdent)
        {
            IAnalysisResult prepRes;
            if (Cursors.TryGetValue(prepIdent, out prepRes))
            {
                if (prepRes is PrepareStatement)
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
    }
}
