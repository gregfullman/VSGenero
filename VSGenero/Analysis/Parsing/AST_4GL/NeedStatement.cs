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
    public class NeedStatement : FglStatement
    {
        public ExpressionNode NumLines { get; private set; }

        public static bool TryParseNode(IParser parser, out NeedStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.NeedKeyword))
            {
                result = true;
                node = new NeedStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode numLines;
                if (ExpressionNode.TryGetExpressionNode(parser, out numLines))
                    node.NumLines = numLines;
                else
                    parser.ReportSyntaxError("Invalid expression found in need statement.");

                if (parser.PeekToken(TokenKind.LineKeyword) || parser.PeekToken(TokenKind.LinesKeyword))
                    parser.NextToken();

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
