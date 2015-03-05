﻿using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
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
    public class ModuleNode : AstNode
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
                if (GlobalsNode.TryParseNode(parser, out globalNode))
                {
                    if (processed == NodesProcessed.SchemaSpec)
                    {
                        defNode.Children.Add(globalNode.StartIndex, globalNode);
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
                        new List<TokenKind> { TokenKind.DefineKeyword }
                    };
                while (ConstantDefNode.TryParseNode(parser, out constNode, out matchedBreakSequence, breakSequences))
                {
                    if (processed == NodesProcessed.Globals)
                    {
                        defNode.Children.Add(constNode.StartIndex, constNode);
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
                        new List<TokenKind> { TokenKind.TypeKeyword }
                    };
                while (TypeDefNode.TryParseNode(parser, out typeNode, out matchedBreakSequence, breakSequences))
                {
                    if (processed == NodesProcessed.Constants)
                    {
                        defNode.Children.Add(typeNode.StartIndex, typeNode);
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
                        new List<TokenKind> { TokenKind.ConstantKeyword }
                    };
                while (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences))
                {
                    if (processed == NodesProcessed.TypeDefs)
                    {
                        defNode.Children.Add(defineNode.StartIndex, defineNode);
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
                if (MainBlockNode.TryParseNode(parser, out mainBlock))
                {
                    if (processed == NodesProcessed.VarDefs)
                    {
                        defNode.Children.Add(mainBlock.StartIndex, mainBlock);
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
                if (FunctionBlockNode.TryParseNode(parser, out funcNode))
                {
                    defNode.Children.Add(funcNode.StartIndex, funcNode);
                    //parser.NextToken();
                }
                else if (ReportBlockNode.TryParseNode(parser, out repNode))
                {
                    defNode.Children.Add(repNode.StartIndex, repNode);
                    //parser.NextToken();
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
    }
}