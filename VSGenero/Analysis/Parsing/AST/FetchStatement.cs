using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class FetchStatement : FglStatement
    {
        public NameExpression CursorId { get; private set; }
        public List<NameExpression> OutputVars { get; private set; }

        public static bool TryParseNode(Parser parser, out FetchStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.FetchKeyword))
            {
                result = true;
                node = new FetchStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.OutputVars = new List<NameExpression>();

                switch(parser.PeekToken().Kind)
                {
                    case TokenKind.NextKeyword:
                    case TokenKind.PreviousKeyword:
                    case TokenKind.PriorKeyword:
                    case TokenKind.CurrentKeyword:
                    case TokenKind.FirstKeyword:
                    case TokenKind.LastKeyword:
                        parser.NextToken();
                        break;
                    case TokenKind.AbsoluteKeyword:
                    case TokenKind.RelativeKeyword:
                        {
                            parser.NextToken();
                            ExpressionNode expr;
                            if (!ExpressionNode.TryGetExpressionNode(parser, out expr))
                                parser.ReportSyntaxError("Invalid expression found in fetch statement.");
                            break;
                        }
                }

                NameExpression cid;
                if (NameExpression.TryParseNode(parser, out cid))
                    node.CursorId = cid;
                else
                    parser.ReportSyntaxError("Invalid declared cursor id found in fetch statement.");

                if(parser.PeekToken(TokenKind.IntoKeyword))
                {
                    parser.NextToken();
                    NameExpression outVar;
                    while (NameExpression.TryParseNode(parser, out outVar, TokenKind.Comma))
                    {
                        node.OutputVars.Add(outVar);
                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                        else
                            break;
                    }
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
