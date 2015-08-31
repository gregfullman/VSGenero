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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class AttributeSpecifier : AstNode4gl
    {
        private List<Token> _specifierTokens;
        public List<Token> SpecifierTokens
        {
            get
            {
                if(_specifierTokens == null)
                    _specifierTokens = new List<Token>();
                return _specifierTokens;
            }
        }

        public static bool TryParseNode(IParser parser, out AttributeSpecifier node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.AttributeKeyword) ||
               parser.PeekToken(TokenKind.AttributesKeyword))
            {
                result = true;
                node = new AttributeSpecifier();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if(!parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.ReportSyntaxError("Invalid attribute specifier found.");
                }
                else
                {
                    while (!parser.PeekToken(TokenKind.RightParenthesis))
                    {
                        node.SpecifierTokens.Add(parser.NextToken());
                        if(parser.PeekToken(TokenKind.EndOfFile))
                        {
                            parser.ReportSyntaxError("Unexpected end of attribute specifier.");
                            return result;
                        }
                    }
                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    node.EndIndex = parser.Token.Span.End;
                    node.IsComplete = true;
                }
            }

            return result;
        }
    }
}
