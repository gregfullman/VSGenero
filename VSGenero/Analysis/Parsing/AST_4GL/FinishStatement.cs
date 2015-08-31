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
    public class FinishReportStatement : FglStatement
    {
        public NameExpression ReportName { get; private set; }

        public static bool TryParseNode(IParser parser, out FinishReportStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.FinishKeyword) &&
               parser.PeekToken(TokenKind.ReportKeyword, 2))
            {
                result = true;
                node = new FinishReportStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                parser.NextToken();

                NameExpression rptName;
                if (NameExpression.TryParseNode(parser, out rptName))
                    node.ReportName = rptName;
                else
                    parser.ReportSyntaxError("Invalid report name found in finish report driver.");
                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
