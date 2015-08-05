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

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class FreeStatement : FglStatement
    {
        public NameExpression Target { get; private set; }

        public static bool TryParseNode(Parser parser, out FreeStatement defNode)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.FreeKeyword))
            {
                result = true;
                defNode = new FreeStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                NameExpression expr;
                if(!NameExpression.TryParseNode(parser, out expr))
                {
                    parser.ReportSyntaxError("Invalid name found in free statement.");
                }
                else
                {
                    defNode.Target = expr;
                }

                defNode.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
