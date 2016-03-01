using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public enum ToolbarComponent
    {
        Toolbar,
        Item,
        Separator
    }

    public class ToolbarNode : AstNodePer
    {
        public NameExpression Identifier { get; private set; }
        public List<ToolbarAttribute> Attributes { get; private set; }

        public static bool TryParseNode(IParser parser, out ToolbarNode node)
        {
            bool result = false;
            node = null;

            if(parser.PeekToken(TokenKind.ToolbarKeyword))
            {
                result = true;
                node = new ToolbarNode();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression nameExpr;
                if(NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.Identifier = nameExpr;

                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        ToolbarAttribute attrib;
                        while (ToolbarAttribute.TryParseNode(parser, out attrib, ToolbarComponent.Toolbar))
                        {
                            node.Attributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in toolbar attributes section.");
                    }

                    bool continueToolbar = true;
                    ToolbarItem item;
                    ToolbarSeparator sep;
                    while(continueToolbar)
                    {
                        if(ToolbarItem.TryParseNode(parser, out item))
                        {
                            node.Children.Add(item.StartIndex, item);
                            continue;
                        }
                        else if(ToolbarSeparator.TryParseNode(parser, out sep))
                        {
                            node.Children.Add(sep.StartIndex, sep);
                            continue;
                        }
                        else
                        {
                            continueToolbar = false;
                        }
                    }

                    if (parser.PeekToken(TokenKind.EndKeyword))
                    {
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Expected \"end\" token.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid toolbar identifier found.");
                }
            }

            return result;
        }
    }

    public class ToolbarItem : AstNodePer
    {
        public NameExpression Identifier { get; private set; }
        public List<ToolbarAttribute> Attributes { get; private set; }

        public static bool TryParseNode(IParser parser, out ToolbarItem node)
        {
            bool result = false;
            node = null;

            if(parser.PeekToken(TokenKind.ItemKeyword))
            {
                parser.NextToken();
                result = true;
                node = new ToolbarItem();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression nameExpr;
                if (NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.Identifier = nameExpr;

                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        ToolbarAttribute attrib;
                        while (ToolbarAttribute.TryParseNode(parser, out attrib, ToolbarComponent.Item))
                        {
                            node.Attributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in toolbar item attributes section.");
                    }

                    if (parser.PeekToken(TokenKind.EndKeyword))
                    {
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Expected \"end\" token.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid toolbar item identifier found.");
                }
            }

            return result;
        }
    }

    public class ToolbarSeparator : AstNodePer
    {
        public NameExpression Identifier { get; private set; }
        public List<ToolbarAttribute> Attributes { get; private set; }

        public static bool TryParseNode(IParser parser, out ToolbarSeparator node)
        {
            bool result = false;
            node = null;

            if (parser.PeekToken(TokenKind.ItemKeyword))
            {
                parser.NextToken();
                result = true;
                node = new ToolbarSeparator();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression nameExpr;
                if (NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.Identifier = nameExpr;

                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        ToolbarAttribute attrib;
                        while (ToolbarAttribute.TryParseNode(parser, out attrib, ToolbarComponent.Separator))
                        {
                            node.Attributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in toolbar separator attributes section.");
                    }

                    if (parser.PeekToken(TokenKind.EndKeyword))
                    {
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Expected \"end\" token.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid toolbar separator identifier found.");
                }
            }

            return result;
        }
    }

    public class ToolbarAttribute : AstNodePer
    {
        public static bool TryParseNode(IParser parser, out ToolbarAttribute node, ToolbarComponent component)
        {
            node = null;
            bool result = false;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.ButtonTextHiddenKeyword:
                    {
                        if (component == ToolbarComponent.Toolbar)
                        {
                            parser.NextToken();
                            result = true;
                            node = new ToolbarAttribute();
                            node.StartIndex = parser.Token.Span.Start;
                            node.EndIndex = parser.Token.Span.End;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Toolbar attribute \"BUTTONTEXTHIDDEN\" is only allowed within a TOOLBAR block.");
                        }
                    }
                    break;
                case TokenKind.StyleKeyword:
                case TokenKind.TagKeyword:
                    {
                        parser.NextToken();
                        result = true;
                        node = new ToolbarAttribute();
                        node.StartIndex = parser.Token.Span.Start;

                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenCategory.StringLiteral))
                            {
                                parser.NextToken();
                                node.EndIndex = parser.Token.Span.End;
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid option found. Expecting string literal.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Expected equals token for toolbar attribute.");
                        }
                    }
                    break;
                case TokenKind.CommentKeyword:
                case TokenKind.ImageKeyword:
                case TokenKind.TextKeyword:
                    {
                        if (component == ToolbarComponent.Item)
                        {
                            parser.NextToken();
                            result = true;
                            node = new ToolbarAttribute();
                            node.StartIndex = parser.Token.Span.Start;

                            if (parser.PeekToken(TokenKind.Equals))
                            {
                                parser.NextToken();
                                if (parser.PeekToken(TokenCategory.StringLiteral))
                                {
                                    parser.NextToken();
                                    node.EndIndex = parser.Token.Span.End;
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Invalid option found. Expecting string literal.");
                                }
                            }
                            else
                            {
                                parser.ReportSyntaxError("Expected equals token for toolbar attribute.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Toolbar attribute not allowed for this block.");
                        }
                    }
                    break;
                case TokenKind.HiddenKeyword:
                    {
                        if (component == ToolbarComponent.Item ||
                           component == ToolbarComponent.Separator)
                        {
                            parser.NextToken();
                            result = true;
                            node = new ToolbarAttribute();
                            node.StartIndex = parser.Token.Span.Start;

                            if (parser.PeekToken(TokenKind.Equals))
                            {
                                if (parser.PeekToken(TokenKind.UserKeyword))
                                {
                                    parser.NextToken();
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Invalid token found. Expecting \"USER\".");
                                }
                            }

                            node.EndIndex = parser.Token.Span.End;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Toolbar attribute \"HIDDEN\" not allowed for the current toolbar block.");
                        }
                    }
                    break;
            }

            return result;
        }
    }
}
