using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class MenuBlock : FglStatement
    {
        public ExpressionNode MenuTitle { get; private set; }
        public List<ExpressionNode> Attributes { get; private set; }
        public List<FglStatement> BeforeMenuStatements { get; private set; }

        public static bool TryParseNode(Parser parser, out MenuBlock node,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.MenuKeyword))
            {
                result = true;
                node = new MenuBlock();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.Attributes = new List<ExpressionNode>();
                node.BeforeMenuStatements = new List<FglStatement>();

                ExpressionNode titleExpr;
                if (ExpressionNode.TryGetExpressionNode(parser, out titleExpr, new List<TokenKind>
                    {
                        TokenKind.AttributesKeyword, TokenKind.BeforeKeyword, TokenKind.CommandKeyword,
                        TokenKind.OnKeyword, TokenKind.EndKeyword
                    }))
                    node.MenuTitle = titleExpr;
                else
                    parser.ReportSyntaxError("Invalid expression found in menu title.");

                if(parser.PeekToken(TokenKind.AttributesKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();
                        ExpressionNode expr;
                        while (ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                        {
                            node.Attributes.Add(expr);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren for menu attributes.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting left-paren for menu attributes.");
                }

                if(parser.PeekToken(TokenKind.BeforeKeyword))
                {
                    parser.NextToken();
                    if(parser.PeekToken(TokenKind.MenuKeyword))
                    {
                        parser.NextToken();
                        FglStatement menuStmt;
                        while (MenuStatementFactory.TryGetStatement(parser, out menuStmt, prepStatementResolver, prepStatementBinder))
                            node.BeforeMenuStatements.Add(menuStmt);
                    }
                    else
                        parser.ReportSyntaxError("Expecting \"before\" keyword for menu block.");
                }

                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                       !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.MenuKeyword, 2)))
                {
                    MenuOption menuOpt;
                    if (MenuOption.TryParseNode(parser, out menuOpt))
                        node.Children.Add(menuOpt.StartIndex, menuOpt);
                    else
                        parser.NextToken();
                }

                if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.MenuKeyword, 2)))
                {
                    parser.ReportSyntaxError("A menu block must be terminated with \"end menu\".");
                }
                else
                {
                    parser.NextToken(); // advance to the 'end' token
                    parser.NextToken(); // advance to the 'menu' token
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }
    }

    public class MenuStatementFactory
    {
        public static bool TryGetStatement(Parser parser, out FglStatement node,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            bool result = false;
            node = null;

            MenuStatement menuStmt;
            if((result = MenuStatement.TryParseNode(parser, out menuStmt)))
            {
                node = menuStmt;
            }
            else
            {
                result = parser.StatementFactory.TryParseNode(parser, out node, prepStatementResolver, prepStatementBinder);
            }

            return result;
        }
    }

    public class MenuStatement : FglStatement
    {
        public List<NameExpression> OptionNames { get; private set; }

        public static bool TryParseNode(Parser parser, out MenuStatement node)
        {
            node = new MenuStatement();
            node.OptionNames = new List<NameExpression>();
            bool result = true;

            switch(parser.PeekToken().Kind)
            {
                case TokenKind.ContinueKeyword:
                case TokenKind.ExitKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.MenuKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting \"menu\" keyword in menu statement.");
                        break;
                    }
                case TokenKind.NextKeyword:
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.OptionKeyword))
                        {
                            parser.NextToken();
                            NameExpression nameExpr;
                            if (NameExpression.TryParseNode(parser, out nameExpr))
                                node.OptionNames.Add(nameExpr);
                            else
                                parser.ReportSyntaxError("Invalid option name found in menu statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"option\" keyword in menu statement.");
                        break;
                    }
                case TokenKind.ShowKeyword:
                case TokenKind.HideKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.OptionKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.AllKeyword))
                                parser.NextToken();
                            else
                            {
                                NameExpression name;
                                while (NameExpression.TryParseNode(parser, out name))
                                {
                                    node.OptionNames.Add(name);
                                    if (!parser.PeekToken(TokenKind.Comma))
                                        break;
                                    parser.NextToken();
                                }
                            }
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"option\" keyword in menu statement.");
                        break;
                    }
                default:
                    {
                        result = false;
                        break;
                    }
            }

            node.EndIndex = parser.Token.Span.End;

            return result;
        }
    }

    public class MenuOption : AstNode
    {
        public ExpressionNode OptionName { get; private set; }
        public ExpressionNode OptionComment { get; private set; }
        public ExpressionNode HelpNumber { get; private set; }
        public ExpressionNode KeyName { get; private set; }

        public NameExpression ActionName { get; private set; }

        public ExpressionNode IdleSeconds { get; private set; }

        public static bool TryParseNode(Parser parser, out MenuOption node,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            node = new MenuOption();
            node.StartIndex = parser.Token.Span.Start;
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.CommandKeyword:
                    {
                        parser.NextToken();
                        bool getOptionName = false;
                        if(parser.PeekToken(TokenKind.KeyKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.NextToken();
                                ExpressionNode keyName;
                                if (ExpressionNode.TryGetExpressionNode(parser, out keyName))
                                    node.KeyName = keyName;
                                else
                                    parser.ReportSyntaxError("Invalid key-name found in menu command option.");

                                if (parser.PeekToken(TokenKind.RightParenthesis))
                                    parser.NextToken();
                                else
                                    parser.ReportSyntaxError("Expecting right-paren in menu command option.");
                            }
                            else
                                parser.ReportSyntaxError("Expecting left-paren in menu command option.");
                        }
                        else
                        {
                            getOptionName = true;
                        }
                        
                        // at this point we need to try to get a menu-statement. If it doesn't work, we have some other stuff to gather
                        FglStatement menuStmt = null;
                        if(getOptionName || !MenuStatementFactory.TryGetStatement(parser, out menuStmt, prepStatementResolver, prepStatementBinder))
                        {
                            ExpressionNode optionName;
                            if (ExpressionNode.TryGetExpressionNode(parser, out optionName))
                                node.OptionName = optionName;
                            else
                                parser.ReportSyntaxError("Invalid option-name found in menu command option.");

                            ExpressionNode optionComment;
                            if(ExpressionNode.TryGetExpressionNode(parser, out optionComment))
                                node.OptionComment = optionComment;
                           
                            if(parser.PeekToken(TokenKind.HelpKeyword))
                            {
                                parser.NextToken();

                                ExpressionNode optionNumber;
                                if (ExpressionNode.TryGetExpressionNode(parser, out optionNumber))
                                    node.HelpNumber = optionNumber;
                                else
                                    parser.ReportSyntaxError("Invalid help-number found in menu command option.");
                            }
                        }
                        else if(menuStmt != null)
                        {
                            node.Children.Add(menuStmt.StartIndex, menuStmt);
                        }

                        while (MenuStatementFactory.TryGetStatement(parser, out menuStmt, prepStatementResolver, prepStatementBinder))
                            node.Children.Add(menuStmt.StartIndex, menuStmt);

                        break;
                    }
                case TokenKind.OnKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.ActionKeyword))
                        {
                            parser.NextToken();
                            NameExpression action;
                            if (NameExpression.TryParseNode(parser, out action))
                                node.ActionName = action;
                            else
                                parser.ReportSyntaxError("Invalid action-name found in menu option.");

                            FglStatement menuStmt = null;
                            while (MenuStatementFactory.TryGetStatement(parser, out menuStmt, prepStatementResolver, prepStatementBinder))
                                node.Children.Add(menuStmt.StartIndex, menuStmt);
                        }
                        else if (parser.PeekToken(TokenKind.IdleKeyword))
                        {
                            parser.NextToken();
                            ExpressionNode idleExpr;
                            if (ExpressionNode.TryGetExpressionNode(parser, out idleExpr))
                                node.IdleSeconds = idleExpr;
                            else
                                parser.ReportSyntaxError("Invalid idle-seconds found in menu block.");

                            FglStatement menuStmt = null;
                            while (MenuStatementFactory.TryGetStatement(parser, out menuStmt, prepStatementResolver, prepStatementBinder))
                                node.Children.Add(menuStmt.StartIndex, menuStmt);
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"action\" or \"idle\" keyword in menu option.");
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            node.EndIndex = parser.Token.Span.End;

            return result;
        }
    }
}
