using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class NameExpression : AstNode
    {
        public string Name { get; private set; }

        public static bool TryParseNode(Parser parser, out NameExpression node, TokenKind breakToken = TokenKind.EndOfFile)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
            {
                result = true;
                node = new NameExpression();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                StringBuilder sb = new StringBuilder(parser.Token.Token.Value.ToString());
                node.EndIndex = parser.Token.Span.End;
                while(true)
                {
                    if(breakToken != TokenKind.EndOfFile &&
                       parser.PeekToken(breakToken))
                    {
                        break;
                    }

                    MemberAccessNameExpressionPiece memberAccess;
                    ArrayIndexNameExpressionPiece arrayIndex;
                    if(MemberAccessNameExpressionPiece.TryParse(parser, out memberAccess))
                    {
                        node.Children.Add(memberAccess.StartIndex, memberAccess);
                        node.EndIndex = memberAccess.EndIndex;
                    }
                    else if(ArrayIndexNameExpressionPiece.TryParse(parser, out arrayIndex, breakToken))
                    {
                        node.Children.Add(arrayIndex.StartIndex, arrayIndex);
                        node.EndIndex = arrayIndex.EndIndex;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid token detected in name expression.");
                        break;
                    }
                }
            }

            return result;
        }
    }

    public class ArrayIndexNameExpressionPiece : AstNode
    {
        private string _expression;

        public override string ToString()
        {
            return string.Format("[{0}]", _expression);
        }

        public static bool TryParse(Parser parser, out ArrayIndexNameExpressionPiece node, TokenKind breakToken = TokenKind.EndOfFile)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.LeftBracket))
            {
                result = true;
                node = new ArrayIndexNameExpressionPiece();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // TODO: need to get an integer expression
                // for right now, we'll just check for a constant or a ident/keyword
                if(parser.PeekToken(TokenCategory.NumericLiteral) ||
                   parser.PeekToken(TokenCategory.Keyword) ||
                   parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    node._expression = parser.Token.Token.Value.ToString();
                }
                else
                {
                    parser.ReportSyntaxError("The parser is unable to parse a complex expression as an array index. This may not be a syntax error.");
                }

                while(!parser.PeekToken(TokenKind.RightBracket))
                {
                    if(parser.PeekToken().Kind == breakToken)
                    {
                        parser.ReportSyntaxError("Unexpected end of array index expression.");
                        break;
                    }
                    parser.NextToken();
                }

                if (parser.PeekToken(TokenKind.RightBracket))
                {
                    parser.NextToken();
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }
    }

    public class MemberAccessNameExpressionPiece : AstNode
    {
        private string _text;

        public override string ToString()
        {
            return _text;
        }

        public static bool TryParse(Parser parser, out MemberAccessNameExpressionPiece node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.Dot))
            {
                result = true;
                node = new MemberAccessNameExpressionPiece();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if(parser.PeekToken(TokenKind.Multiply) || parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                {
                    node._text = parser.NextToken().Value.ToString();
                    node.EndIndex = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError("Invalid token found in member access.");
                }
            }

            return result;
        }
    }
}
