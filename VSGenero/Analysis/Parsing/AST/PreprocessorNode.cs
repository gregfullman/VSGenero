/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public enum PreprocessorType
    {
        Include,
        Define,
        Undef,
        Ifdef,
        Else,
        Endif
    }

    public class PreprocessorNode : AstNode
    {
        public PreprocessorType Type { get; private set; }
        public string IncludeFile { get; private set; }

        private List<Token> _preprocessorTokens;
        public List<Token> PreprocessorTokens
        {
            get
            {
                if (_preprocessorTokens == null)
                    _preprocessorTokens = new List<Token>();
                return _preprocessorTokens;
            }
        }

        public static bool TryParseNode(Parser parser, out PreprocessorNode node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.Ampersand))
            {
                var options = parser.Tokenizer.CurrentOptions;                              // backup the current parser options
                var newOptions = options | TokenizerOptions.VerbatimCommentsAndLineJoins;   // allow us to continue until the newline
                parser.Tokenizer.AdjustOptions(newOptions);
                parser.NextToken();
                result = true;
                node = new PreprocessorNode();
                node.StartIndex = parser.Token.Span.Start;
                StringBuilder sb = new StringBuilder();
                
                switch(parser.PeekToken().Kind)
                {
                    case TokenKind.IncludeKeyword:
                        parser.NextToken();
                        node.Type = PreprocessorType.Include;
                        ExpressionNode strExpr;
                        if (ExpressionNode.TryGetExpressionNode(parser, out strExpr) && strExpr is StringExpressionNode)
                            node.IncludeFile = (strExpr as StringExpressionNode).LiteralValue;
                        else
                            parser.ReportSyntaxError("Invalid include file preprocessor directive found.");
                        break;
                    case TokenKind.DefineKeyword:
                        parser.NextToken();
                        node.Type = PreprocessorType.Define;
                        break;
                    case TokenKind.UndefKeyword:
                        parser.NextToken();
                        node.Type = PreprocessorType.Undef;
                        break;
                    case TokenKind.IfdefKeyword:
                        parser.NextToken();
                        node.Type = PreprocessorType.Ifdef;
                        break;
                    case TokenKind.ElseKeyword:
                        parser.NextToken();
                        node.Type = PreprocessorType.Else;
                        break;
                    case TokenKind.EndifKeyword:
                        parser.NextToken();
                        node.Type = PreprocessorType.Endif;
                        break;
                    default:
                        break;
                }

                while(!parser.PeekToken(TokenKind.NewLine) && !parser.PeekToken(TokenKind.EndOfFile))
                {
                    node.PreprocessorTokens.Add(parser.NextToken());
                }
                parser.Tokenizer.AdjustOptions(options);                                    // restore the backed up parser options
                while(parser.PeekToken(TokenKind.NewLine))
                    parser.NextToken();
                node.IsComplete = true;
            }

            return result;
        }
    }

    public class IncludedPreprocessorNode : AstNode
    {

    }
}
