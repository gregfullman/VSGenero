using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
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
