using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class ActionDefaultsNode : AstNodePer
    {
        public static bool TryParseNode(IParser parser, out ActionDefaultsNode node)
        {
            bool result = false;
            node = null;

            if(parser.PeekToken(TokenKind.ActionKeyword) &&
               parser.PeekToken(TokenKind.DefaultsKeyword, 2))
            {
                node = new ActionDefaultsNode();
                result = true;
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                parser.NextToken();

                ActionNode actionNode;
                while(!parser.PeekToken(TokenKind.EndKeyword) && ActionNode.TryParseNode(parser, out actionNode))
                {
                    node.Children.Add(actionNode.StartIndex, actionNode);
                }

                if(parser.PeekToken(TokenKind.EndKeyword))
                {
                    parser.NextToken();
                    node.EndIndex = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError("ACTION DEFAULTS section must be terminated with \"end\" keyword.");
                }
            }

            return result;
        }
    }

    public class ActionNode : AstNodePer
    {
        public NameExpression ActionIdentifier { get; private set; }
        public List<ActionAttribute> Attributes { get; private set; }

        public static bool TryParseNode(IParser parser, out ActionNode node)
        {
            bool result = false;
            node = null;

            if(parser.PeekToken(TokenKind.ActionKeyword))
            {
                node = new ActionNode();
                result = true;
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.Attributes = new List<ActionAttribute>();


                // TODO: get the action id and attributes
                NameExpression nameExpr;
                if(NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.ActionIdentifier = nameExpr;
                }
                else
                {
                    parser.ReportSyntaxError("Invalid action identifier found.");
                }

                if(parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();
                    ActionAttribute attrib;
                    while(ActionAttribute.TryParseNode(parser, out attrib))
                    {
                        node.Attributes.Add(attrib);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }

                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expecting right-paren in action attributes section.");
                }
                else
                {
                    parser.ReportSyntaxError("Expected left-paren in action attributes section.");
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    public class ActionAttribute : AstNodePer
    {
        public static bool TryParseNode(IParser parser, out ActionAttribute node)
        {
            bool result = false;
            node = null;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.AcceleratorKeyword:
                case TokenKind.Accelerator2Keyword:
                case TokenKind.Accelerator3Keyword:
                case TokenKind.Accelerator4Keyword:
                    {
                        parser.NextToken();
                        result = true;
                        node = new ActionAttribute();
                        node.StartIndex = parser.Token.Span.Start;

                        if(parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            VirtualKey vKey;
                            if(VirtualKey.TryGetKey(parser, out vKey))
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
                            parser.ReportSyntaxError("Expected equals token for action attribute.");
                        }
                    }
                    break;
                case TokenKind.DefaultViewKeyword:
                case TokenKind.ContextMenuKeyword:
                    {
                        parser.NextToken();
                        result = true;
                        node = new ActionAttribute();
                        node.StartIndex = parser.Token.Span.Start;

                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.AutoKeyword) ||
                               parser.PeekToken(TokenKind.YesKeyword) ||
                               parser.PeekToken(TokenKind.NoKeyword))
                            {
                                parser.NextToken();
                                node.EndIndex = parser.Token.Span.End;
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid option specified. Allowed values are Auto, Yes, or No.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Expected equals token for action attribute.");
                        }
                    }
                    break;
                case TokenKind.CommentKeyword:
                case TokenKind.ImageKeyword:
                case TokenKind.TextKeyword:
                    {
                        parser.NextToken();
                        result = true;
                        node = new ActionAttribute();
                        node.StartIndex = parser.Token.Span.Start;

                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenCategory.StringLiteral))
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
                            parser.ReportSyntaxError("Expected equals token for action attribute.");
                        }
                    }
                    break;
                case TokenKind.ValidateKeyword:
                    {
                        parser.NextToken();
                        result = true;
                        node = new ActionAttribute();
                        node.StartIndex = parser.Token.Span.Start;

                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.NoKeyword))
                            {
                                parser.NextToken();
                                node.EndIndex = parser.Token.Span.End;
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid validate value specified. Allowed values is No.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Expected equals token for action attribute.");
                        }
                    }
                    break;
            }

            return result;
        }
    }
}
