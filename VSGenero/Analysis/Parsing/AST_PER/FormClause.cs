using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class FormClause : AstNodePer
    {
        public StringExpressionNode FormFile { get; private set; }

        public static bool TryParseNode(IParser parser, out FormClause node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.FormKeyword))
            {
                node = new FormClause();
                result = true;
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                StringExpressionNode expr;
                if(StringExpressionNode.TryGetExpressionNode(parser, out expr))
                {
                    node.FormFile = expr;
                }
                else
                {
                    parser.ReportSyntaxError("Invalid token found, expecting string literal.");
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
