using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class LabelStatement : FglStatement
    {
        public NameExpression LabelId { get; private set; }

        public static bool TryParseNode(Parser parser, out LabelStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.LabelKeyword))
            {
                result = true;
                node = new LabelStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression expr;
                if (!NameExpression.TryParseNode(parser, out expr, TokenKind.Colon))
                    parser.ReportSyntaxError("Invalid name found in label statement.");
                else
                    node.LabelId = expr;

                if (parser.PeekToken(TokenKind.Colon))
                    parser.NextToken();
                else
                    parser.ReportSyntaxError("Label statement requires a colon at the end.");

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
