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
    public class ExitStatement : FglStatement
    {
        public TokenKind ExitType { get; private set; }

        public static bool TryParseNode(Parser parser, out ExitStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.ExitKeyword))
            {
                result = true;
                node = new ExitStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                TokenKind tokKind = parser.PeekToken().Kind;
                switch(tokKind)
                {
                    case TokenKind.ForKeyword:
                    case TokenKind.ForeachKeyword:
                    case TokenKind.WhileKeyword:
                    case TokenKind.MenuKeyword:
                    case TokenKind.ConstructKeyword:
                    case TokenKind.ReportKeyword:
                    case TokenKind.DisplayKeyword:
                    case TokenKind.InputKeyword:
                    case TokenKind.DialogKeyword:
                    case TokenKind.CaseKeyword:
                        node.ExitType = tokKind;
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                        break;
                    case TokenKind.ProgramKeyword:
                        node.ExitType = tokKind;
                        parser.NextToken();
                        bool requireCode = false;
                        if(parser.PeekToken(TokenKind.Subtract))
                        {
                            parser.NextToken();
                            requireCode = true;
                        }
                        if(parser.PeekToken(TokenCategory.NumericLiteral))
                        {
                            parser.NextToken();
                        }
                        else if(requireCode)
                        {
                            parser.ReportSyntaxError("Exit program statement is missing an exit code.");
                        }
                        node.EndIndex = parser.Token.Span.End;
                        break;
                    default:
                        parser.ReportSyntaxError("Exit statement must be of form: exit { FOR | FOREACH | WHILE | MENU | CONSTRUCT | REPORT | DISPLAY | INPUT | DIALOG | CASE | PROGRAM }");
                        break;
                }
            }

            return result;
        }
    }
}
