using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class PrintxStatement : FglStatement
    {
        public NameExpression Name { get; private set; }
        public List<ExpressionNode> Expressions { get; private set; }

        public static bool TryParseNode(IParser parser, out PrintxStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.PrintxKeyword))
            {
                result = true;
                node = new PrintxStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.Expressions = new List<ExpressionNode>();

                if(parser.PeekToken(TokenKind.NameKeyword))
                {
                    parser.NextToken();
                    if(parser.PeekToken(TokenKind.Equals))
                    {
                        parser.NextToken();
                        NameExpression name;
                        if (NameExpression.TryParseNode(parser, out name))
                            node.Name = name;
                        else
                            parser.ReportSyntaxError("Invalid name expression found in printx statement.");
                    }
                    else
                        parser.ReportSyntaxError("Expected '=' in printx statement.");
                }

                ExpressionNode expr;
                while (ExpressionNode.TryGetExpressionNode(parser, out expr))
                {
                    node.Expressions.Add(expr);
                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();
                    else
                        break;
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
