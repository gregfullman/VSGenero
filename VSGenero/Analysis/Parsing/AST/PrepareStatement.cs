using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class PrepareStatement : FglStatement
    {
        public string Identifier { get; private set; }

        public static bool TryParseNode(Parser parser, out PrepareStatement defNode)
        {
            defNode = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.PrepareKeyword))
            {
                result = true;
                defNode = new PrepareStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                {
                    parser.NextToken();
                    defNode.Identifier = parser.Token.Token.Value.ToString();

                    if (parser.PeekToken(TokenKind.FromKeyword))
                    {
                        parser.NextToken();

                        ExpressionNode exprNode;
                        if (ExpressionNode.TryGetExpressionNode(parser, out exprNode))
                        {
                            defNode.Children.Add(exprNode.StartIndex, exprNode);
                            defNode.EndIndex = exprNode.EndIndex;
                        }
                        else
                        {
                            parser.ReportSyntaxError("SQL prepare statement must specify an expression to prepare.");
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("SQL prepare statement is missing keyword \"from\".");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("SQL prepare statement must specify an identifier to prepare.");
                }
            }

            return result;
        }
    }
}
