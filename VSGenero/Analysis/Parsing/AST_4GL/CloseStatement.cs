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
    public class CloseStatement : FglStatement
    {
        public FglNameExpression CursorId { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out CloseStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.CloseKeyword))
            {
                result = true;
                node = new CloseStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                FglNameExpression cid;
                if (FglNameExpression.TryParseNode(parser, out cid))
                    node.CursorId = cid;
                else
                    parser.ReportSyntaxError("Invalid declared cursor id found in close statement.");

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
