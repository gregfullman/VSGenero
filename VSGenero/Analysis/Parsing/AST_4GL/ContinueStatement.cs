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

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class ContinueStatement : FglStatement
    {
        public TokenKind ContinueType { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out ContinueStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.ContinueKeyword))
            {
                result = true;
                node = new ContinueStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                TokenKind tokKind = parser.PeekToken().Kind;
                switch (tokKind)
                {
                    case TokenKind.ForKeyword:
                    case TokenKind.ForeachKeyword:
                    case TokenKind.WhileKeyword:
                    case TokenKind.MenuKeyword:
                    case TokenKind.ConstructKeyword:
                    case TokenKind.InputKeyword:
                    case TokenKind.DialogKeyword:
                    case TokenKind.DisplayKeyword:
                        node.ContinueType = tokKind;
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                        break;
                    default:
                        parser.ReportSyntaxError("Continue statement must be of form: continue { FOR | FOREACH | WHILE | MENU | CONSTRUCT | INPUT | DIALOG | DISPLAY }");
                        break;
                }
            }

            return result;
        }
    }
}
