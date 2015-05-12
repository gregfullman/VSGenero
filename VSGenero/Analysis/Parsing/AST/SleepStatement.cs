using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class SleepStatement : FglStatement
    {
        public ExpressionNode SleepSeconds { get; private set; }

        public static bool TryParseNode(Parser parser, out SleepStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.SleepKeyword))
            {
                result = true;
                node = new SleepStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode expr;
                if (!ExpressionNode.TryGetExpressionNode(parser, out expr, GeneroAst.ValidStatementKeywords.ToList()))
                    parser.ReportSyntaxError("Invalid expression found in sleep statement.");
                else
                    node.SleepSeconds = expr;

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
