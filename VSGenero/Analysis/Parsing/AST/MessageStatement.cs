using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class MessageStatement : FglStatement
    {
        public List<ExpressionNode> Messages { get; private set; }
        public List<MessageAttribute> Attributes { get; private set; }

        public static bool TryParseNode(Parser parser, out MessageStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.MessageKeyword))
            {
                result = true;
                node = new MessageStatement();
                node.Messages = new List<ExpressionNode>();
                node.Attributes = new List<MessageAttribute>();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                while (true)
                {
                    ExpressionNode msgExpr;
                    if (ExpressionNode.TryGetExpressionNode(parser, out msgExpr))
                        node.Messages.Add(msgExpr);
                    else
                        parser.ReportSyntaxError("Invalid message expression found.");

                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    else
                        parser.NextToken();
                }

                // get the optional attributes
                if (parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();
                        MessageAttribute attrib;
                        while (MessageAttribute.TryParseNode(parser, out attrib))
                        {
                            node.Attributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in display attributes section.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting left-paren in display attributes section.");
                }
            }

            return result;
        }
    }

    public class MessageAttribute : AstNode
    {
        public static bool TryParseNode(Parser parser, out MessageAttribute node)
        {
            node = new MessageAttribute();
            node.StartIndex = parser.Token.Span.Start;
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.BlackKeyword:
                case TokenKind.BlueKeyword:
                case TokenKind.CyanKeyword:
                case TokenKind.GreenKeyword:
                case TokenKind.MagentaKeyword:
                case TokenKind.RedKeyword:
                case TokenKind.WhiteKeyword:
                case TokenKind.YellowKeyword:
                case TokenKind.BoldKeyword:
                case TokenKind.DimKeyword:
                case TokenKind.InvisibleKeyword:
                case TokenKind.NormalKeyword:
                case TokenKind.ReverseKeyword:
                case TokenKind.BlinkKeyword:
                case TokenKind.UnderlineKeyword:
                    parser.NextToken();
                    break;
                case TokenKind.StyleKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected equals token in message attribute.");

                        // get the style name
                        ExpressionNode styleName;
                        if (!ExpressionNode.TryGetExpressionNode(parser, out styleName))
                            parser.ReportSyntaxError("Invalid style name found in message attribute.");
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            return result;
        }
    }
}
