using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class OutputToReportStatement : FglStatement
    {
        public FunctionCallExpressionNode ReportCall { get; private set; }

        public static bool TryParseNode(IParser parser, out OutputToReportStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.OutputKeyword) &&
               parser.PeekToken(TokenKind.ToKeyword, 2) &&
               parser.PeekToken(TokenKind.ReportKeyword, 3))
            {
                result = true;
                parser.NextToken();
                node = new OutputToReportStatement();
                node.StartIndex = parser.Token.Span.Start;
                parser.NextToken();
                parser.NextToken();

                FunctionCallExpressionNode reportCall;
                NameExpression dummyName;
                if (FunctionCallExpressionNode.TryParseExpression(parser, out reportCall, out dummyName, true))
                    node.ReportCall = reportCall;
                else
                    parser.ReportSyntaxError("Invalid report call found in output statement.");

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
