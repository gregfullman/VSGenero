using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{ 
    public enum MenuComponent
    {
        Menu,
        Group,
        Command,
        Separator
    }

    public class TopMenuNode : AstNodePer
    {
        public NameExpression MenuName { get; private set; }
        public List<MenuAttribute> Attributes { get; private set; }

        public static bool TryParseNode(IParser parser, out TopMenuNode node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.TopMenuKeyword))
            {
                parser.NextToken();
                result = true;
                node = new TopMenuNode();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression nameExpr;
                if(NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.MenuName = nameExpr;
                }

                if(parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();

                    MenuAttribute attrib;
                    while (MenuAttribute.TryParseNode(parser, out attrib, MenuComponent.Menu))
                    {
                        node.Attributes.Add(attrib);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }

                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expecting right-paren in menu attributes section.");
                }

                TopMenuGroup group;
                while(TopMenuGroup.TryParseNode(parser, out group))
                {
                    node.Children.Add(group.StartIndex, group);
                }

                if(parser.PeekToken(TokenKind.EndKeyword))
                {
                    parser.NextToken();
                    node.EndIndex = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError("Expected \"end\" token.");
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    public class TopMenuGroup : AstNodePer
    {
        public NameExpression Identifier { get; private set; }
        public List<MenuAttribute> Attributes { get; private set; }

        public static bool TryParseNode(IParser parser, out TopMenuGroup node)
        {
            bool result = false;
            node = null;

            if(parser.PeekToken(TokenKind.GroupKeyword))
            {
                result = true;
                node = new TopMenuGroup();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression nameExpr;
                if(NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.Identifier = nameExpr;

                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        MenuAttribute attrib;
                        while (MenuAttribute.TryParseNode(parser, out attrib, MenuComponent.Group))
                        {
                            node.Attributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in group attributes section.");
                    }

                    bool continueGroup = true;
                    while (continueGroup)
                    {
                        switch (parser.PeekToken().Kind)
                        {
                            case TokenKind.CommandKeyword:
                                {
                                    TopMenuCommand cmd;
                                    if (TopMenuCommand.TrParseNode(parser, out cmd))
                                    {
                                        node.Children.Add(cmd.StartIndex, cmd);
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError("Invalid top menu command block found.");
                                    }
                                }
                                break;
                            case TokenKind.SeparatorKeyword:
                                {
                                    TopMenuSeparator sep;
                                    if (TopMenuSeparator.TrParseNode(parser, out sep))
                                    {
                                        node.Children.Add(sep.StartIndex, sep);
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError("Invalid top menu separator block found.");
                                    }
                                }
                                break;
                            case TokenKind.GroupKeyword:
                                {
                                    TopMenuGroup grp;
                                    if (TryParseNode(parser, out grp))
                                    {
                                        node.Children.Add(grp.StartIndex, grp);
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError("Invalid top menu group block found.");
                                    }
                                }
                                break;
                            default:
                                continueGroup = false;
                                break;
                        }
                    }

                    if(parser.PeekToken(TokenKind.EndKeyword))
                    {
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.Start;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid token found, expected \"end\".");
                    }    
                }
                else
                {
                    parser.ReportSyntaxError("Invalid GROUP name found.");
                }
            }

            return result;
        }
    }

    public class TopMenuCommand : AstNodePer
    {
        public NameExpression Identifier { get; private set; }
        public List<MenuAttribute> Attributes { get; private set; }

        public static bool TrParseNode(IParser parser, out TopMenuCommand node)
        {
            bool result = false;
            node = null;

            if (parser.PeekToken(TokenKind.CommandKeyword))
            {
                result = true;
                node = new TopMenuCommand();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression nameExpr;
                if (NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.Identifier = nameExpr;

                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        MenuAttribute attrib;
                        while (MenuAttribute.TryParseNode(parser, out attrib, MenuComponent.Command))
                        {
                            node.Attributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in group attributes section.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid COMMAND name found.");
                }
            }

            return result;
        }
    }

    public class TopMenuSeparator : AstNodePer
    {
        public NameExpression Identifier { get; private set; }
        public List<MenuAttribute> Attributes { get; private set; }

        public static bool TrParseNode(IParser parser, out TopMenuSeparator node)
        {
            bool result = false;
            node = null;

            if (parser.PeekToken(TokenKind.SeparatorKeyword))
            {
                result = true;
                node = new TopMenuSeparator();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression nameExpr;
                if (NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.Identifier = nameExpr;

                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        MenuAttribute attrib;
                        while (MenuAttribute.TryParseNode(parser, out attrib, MenuComponent.Separator))
                        {
                            node.Attributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in group attributes section.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid COMMAND name found.");
                }
            }

            return result;
        }
    }

    public class MenuAttribute : AstNodePer
    {
        public static bool TryParseNode(IParser parser, out MenuAttribute node, MenuComponent component)
        {
            node = null;
            bool result = false;

            switch(parser.PeekToken().Kind)
            {
                case TokenKind.AcceleratorKeyword:
                    {
                        if (component == MenuComponent.Command)
                        {
                            parser.NextToken();
                            result = true;
                            node = new MenuAttribute();
                            node.StartIndex = parser.Token.Span.Start;

                            if (parser.PeekToken(TokenKind.Equals))
                            {
                                parser.NextToken();
                                VirtualKey vKey;
                                if (VirtualKey.TryGetKey(parser, out vKey))
                                {
                                    node.Children.Add(vKey.StartIndex, vKey);
                                    node.EndIndex = parser.Token.Span.End;
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Invalid accelerator key specified.");
                                }
                            }
                            else
                            {
                                parser.ReportSyntaxError("Expected equals token for menu attribute.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Menu attribute \"ACCELERATOR\" is only allowed within a command block.");
                        }
                    }
                    break;
                case TokenKind.StyleKeyword:
                case TokenKind.TagKeyword:
                    {
                        parser.NextToken();
                        result = true;
                        node = new MenuAttribute();
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
                            parser.ReportSyntaxError("Expected equals token for menu attribute.");
                        }
                    }
                    break;
                case TokenKind.CommentKeyword:
                case TokenKind.ImageKeyword:
                case TokenKind.TextKeyword:
                    {
                        if (component == MenuComponent.Group ||
                           component == MenuComponent.Command)
                        {
                            parser.NextToken();
                            result = true;
                            node = new MenuAttribute();
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
                                parser.ReportSyntaxError("Expected equals token for menu attribute.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Menu attribute not allowed for this block.");
                        }
                    }
                    break;
                case TokenKind.HiddenKeyword:
                    {
                        if (component == MenuComponent.Group ||
                           component == MenuComponent.Command ||
                           component == MenuComponent.Separator)
                        {
                            parser.NextToken();
                            result = true;
                            node = new MenuAttribute();
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
                            parser.ReportSyntaxError("Menu attribute \"HIDDEN\" not allowed for the current menu block.");
                        }
                    }
                    break;
            }

            return result;
        }
    }
}
