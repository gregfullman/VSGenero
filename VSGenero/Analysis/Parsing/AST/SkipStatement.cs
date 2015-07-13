using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class SkipStatement : FglStatement
    {
        public ExpressionNode NumLines { get; private set; }

        public static bool TryParseNode(IParser parser, out SkipStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.SkipKeyword))
            {
                result = true;
                node = new SkipStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if(parser.PeekToken(TokenKind.ToKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.TopKeyword) &&
                       parser.PeekToken(TokenKind.OfKeyword, 2) &&
                       parser.PeekToken(TokenKind.PageKeyword, 3))
                    {
                        parser.NextToken();
                        parser.NextToken();
                        parser.NextToken();
                    }
                    else
                        parser.ReportSyntaxError("Expected \"top of page\" in skip statement.");
                }
                else
                {
                    ExpressionNode numLines;
                    if (ExpressionNode.TryGetExpressionNode(parser, out numLines))
                        node.NumLines = numLines;
                    else
                        parser.ReportSyntaxError("Invalid expression found in skip statement.");

                    if (parser.PeekToken(TokenKind.LineKeyword) || parser.PeekToken(TokenKind.LinesKeyword))
                        parser.NextToken();
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
