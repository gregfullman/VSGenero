using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class LayoutNode : AstNodePer
    {
        public List<LayoutAttribute> Attributes = new List<LayoutAttribute>();

        public static bool TryParseNode(IParser parser, out LayoutNode node)
        {
            bool result = false;
            node = null;

            if (parser.PeekToken(TokenKind.LayoutKeyword))
            {
                result = true;
                node = new LayoutNode();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if(parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();
                    LayoutAttribute attrib;
                    while(LayoutAttribute.TryParseAttribute(parser, out attrib))
                    {
                        node.Attributes.Add(attrib);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        else
                            parser.NextToken();
                    }

                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expected right-paren.");
                }



                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    public enum LayoutAttributes
    {
        Image,
        MinHeight,
        MinWidth,
        Spacing,
        Style,
        Text,
        Tag,
        Version,
        WindowStyle
    }

    public enum LayoutSpacing
    {
        Normal,
        Compact
    }

    public class LayoutAttribute : AstNodePer
    {
        public LayoutAttributes Type { get; private set; }
        public StringExpressionNode Image { get; private set; }
        public int MinHeight { get; private set; }
        public int MinWidth { get; private set; }
        public LayoutSpacing Spacing { get; private set; }
        public StringExpressionNode Style { get; private set; }
        public StringExpressionNode Text { get; private set; }
        public StringExpressionNode Tag { get; private set; }
        public StringExpressionNode Version { get; private set; }
        public StringExpressionNode WindowStyle { get; private set; }

    public static bool TryParseAttribute(IParser parser, out LayoutAttribute attrib)
        {
            attrib = new LayoutAttribute();
            bool result = true;

            int? intVal = null;
            StringExpressionNode stringNode = null;
            switch (parser.PeekToken().Kind)
            {
                case TokenKind.ImageKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.Image;

                    if (StringExpressionNode.TryGetExpressionNode(parser, out stringNode))
                        attrib.Image = stringNode;
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found in Image attribute, expected string literal.");
                    }

                    break;
                case TokenKind.MinHeightKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.MinHeight;

                    intVal = GetIntegerValue(parser);
                    if (intVal.HasValue)
                        attrib.MinHeight = intVal.Value;
                    break;
                case TokenKind.MinWidthKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.MinWidth;

                    intVal = GetIntegerValue(parser);
                    if (intVal.HasValue)
                        attrib.MinWidth = intVal.Value;

                    break;
                case TokenKind.SpacingKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.Spacing;

                    if (parser.PeekToken(TokenKind.NormalKeyword))
                    {
                        parser.NextToken();
                        attrib.Spacing = LayoutSpacing.Normal;
                    }
                    else if(parser.PeekToken(TokenKind.CompactKeyword))
                    {
                        parser.NextToken();
                        attrib.Spacing = LayoutSpacing.Compact;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found in Spacing attribute, expected \"NORMAL\" or \"COMPACT\".");
                    }

                    break;
                case TokenKind.StyleKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.Style;

                    if (StringExpressionNode.TryGetExpressionNode(parser, out stringNode))
                        attrib.Style = stringNode;
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found in Style attribute, expected string literal.");
                    }

                    break;
                case TokenKind.TextKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.Text;

                    if (StringExpressionNode.TryGetExpressionNode(parser, out stringNode))
                        attrib.Text = stringNode;
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found in Text attribute, expected string literal.");
                    }
                    break;
                case TokenKind.TagKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.Tag;

                    if (StringExpressionNode.TryGetExpressionNode(parser, out stringNode))
                        attrib.Tag = stringNode;
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found in Tag attribute, expected string literal.");
                    }

                    break;
                case TokenKind.VersionKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.Version;

                    if (StringExpressionNode.TryGetExpressionNode(parser, out stringNode))
                        attrib.Version = stringNode;
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found in Version attribute, expected string literal.");
                    }
                    break;
                case TokenKind.WindowStyleKeyword:
                    parser.NextToken();
                    attrib.StartIndex = parser.Token.Span.Start;
                    attrib.Type = LayoutAttributes.WindowStyle;

                    if (StringExpressionNode.TryGetExpressionNode(parser, out stringNode))
                        attrib.WindowStyle = stringNode;
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found in WindowStyle attribute, expected string literal.");
                    }
                    break;
                default:
                    result = false;
                    attrib = null;
                    break;
            }

            if (attrib != null)
            {
                attrib.EndIndex = parser.Token.Span.End;
            }


            return result;
        }

        private static int? GetIntegerValue(IParser parser)
        {
            if (parser.PeekToken(TokenCategory.NumericLiteral))
            {
                var tok = parser.NextToken() as ConstantValueToken;
                if (tok != null)
                {
                    int intVal;
                    string val = tok.Value.ToString();
                    if (int.TryParse(val, out intVal))
                    {
                        return intVal;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found in MinHeight attribute, expected integer value");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid token found in MinHeight attribute, expected integer value");
                }
            }
            else
            {
                parser.ReportSyntaxError("Invalid token found in MinHeight attribute, expected integer value");
            }
            return null;
        }
    }
}
