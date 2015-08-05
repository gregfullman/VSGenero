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
    public class AcceptStatement : FglStatement
    {
        public TokenKind AcceptType { get; private set; }

        public static bool TryParseNode(Parser parser, out AcceptStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.AcceptKeyword))
            {
                result = true;
                node = new AcceptStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                TokenKind tokKind = parser.PeekToken().Kind;
                switch (tokKind)
                {
                    case TokenKind.ConstructKeyword:
                    case TokenKind.InputKeyword:
                    case TokenKind.DialogKeyword:
                    case TokenKind.DisplayKeyword:
                        node.AcceptType = tokKind;
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                        break;
                    default:
                        parser.ReportSyntaxError("Accept statement must be of form: accept { CONSTRUCT | INPUT | DIALOG | DISPLAY }");
                        break;
                }
            }

            return result;
        }
    }
}
