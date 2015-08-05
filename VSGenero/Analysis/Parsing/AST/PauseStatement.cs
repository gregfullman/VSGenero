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
    public class PauseStatement : FglStatement
    {
        public static bool TryParseNode(IParser parser, out PauseStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.PauseKeyword))
            {
                result = true;
                node = new PauseStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if (parser.PeekToken(TokenCategory.StringLiteral))
                    parser.NextToken();

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
