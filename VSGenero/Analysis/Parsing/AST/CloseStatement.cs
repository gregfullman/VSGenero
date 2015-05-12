using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class CloseStatement : FglStatement
    {
        public NameExpression CursorId { get; private set; }

        public static bool TryParseNode(Parser parser, out CloseStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.CloseKeyword))
            {
                result = true;
                node = new CloseStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression cid;
                if (NameExpression.TryParseNode(parser, out cid))
                    node.CursorId = cid;
                else
                    parser.ReportSyntaxError("Invalid declared cursor id found in close statement.");

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
