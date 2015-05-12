using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class GotoStatement : FglStatement
    {
        public NameExpression LabelId { get; private set; }

        public static bool TryParseNode(Parser parser, out GotoStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.GotoKeyword))
            {
                result = true;
                node = new GotoStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                
                // colon is optional
                if (parser.PeekToken(TokenKind.Colon))
                    parser.NextToken();

                NameExpression expr;
                if (!NameExpression.TryParseNode(parser, out expr))
                    parser.ReportSyntaxError("Invalid name found in goto statement.");
                else
                    node.LabelId = expr;

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
