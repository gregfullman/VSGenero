/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class MenuBlock : FglStatement, IOutlinableResult
    {
        public string MenuTitle { get; private set; }
        public List<ExpressionNode> Attributes { get; private set; }

        public static bool TryParseNode(Parser parser, out MenuBlock node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
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

                if(parser.PeekToken(TokenCategory.StringLiteral))
                {
                    parser.NextToken();
                    node.MenuTitle = parser.Token.Token.Value.ToString();
                }

                node.DecoratorEnd = parser.Token.Span.End;

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

                List<TokenKind> validExits = new List<TokenKind>();
                if (validExitKeywords != null)
                    validExits.AddRange(validExitKeywords);
                validExits.Add(TokenKind.MenuKeyword);

                int beforeStart = -1, beforeDecEnd = -1, beforeEnd = -1;
                if(parser.PeekToken(TokenKind.BeforeKeyword))
                {
                    parser.NextToken();
                    beforeStart = parser.Token.Span.Start;
                    if(parser.PeekToken(TokenKind.MenuKeyword))
                    {
                        parser.NextToken();
                        beforeDecEnd = parser.Token.Span.End;
                        FglStatement menuStmt;
                        List<FglStatement> stmts = new List<FglStatement>();
                        while (MenuStatementFactory.TryGetStatement(parser, out menuStmt, containingModule, prepStatementBinder, returnStatementBinder, validExits, contextStatementFactories))
                        {
                            stmts.Add(menuStmt);
                            beforeEnd = menuStmt.EndIndex;
                        }

                        if (beforeEnd < 0)
                            beforeEnd = beforeDecEnd;
                        MenuOption beforeMenu = new MenuOption(beforeStart, beforeDecEnd, beforeEnd, stmts);
                        if(beforeMenu != null)
                            node.Children.Add(beforeMenu.StartIndex, beforeMenu);
                    }
                    else
                        parser.ReportSyntaxError("Expecting \"before\" keyword for menu block.");
                }

                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                       !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.MenuKeyword, 2)))
                {
                    MenuOption menuOpt;
                    if (MenuOption.TryParseNode(parser, out menuOpt, containingModule, prepStatementBinder, returnStatementBinder, validExits, contextStatementFactories) && menuOpt != null)
                    {
                        if (menuOpt.StartIndex < 0)
                            continue;
                        node.Children.Add(menuOpt.StartIndex, menuOpt);
                    }
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

        public bool CanOutline
        {
            get { return true; }
        }

        public int DecoratorStart
        {
            get
            {
                return StartIndex;
            }
            set
            {
            }
        }

        public int DecoratorEnd { get; set; }
    }

    public class MenuStatementFactory
    {
        private static bool TryGetMenuStatement(Parser parser, out MenuStatement node, bool returnFalseInsteadOfErrors = false)
        {
            bool result = false;
            node = null;

            MenuStatement menuStmt;
            if ((result = MenuStatement.TryParseNode(parser, out menuStmt, returnFalseInsteadOfErrors)))
            {
                node = menuStmt;
            }

            return result;
        }

        public static bool TryGetStatement(Parser parser, out FglStatement node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
        {
            bool result = false;
            node = null;

            MenuStatement menuStmt;
            if ((result = TryGetMenuStatement(parser, out menuStmt)))
            {
                node = menuStmt;
            }
            else
            {
                List<ContextStatementFactory> csfs = new List<ContextStatementFactory>();
                if (contextStatementFactories != null)
                    csfs.AddRange(contextStatementFactories);
                csfs.Add((x) =>
                {
                    MenuStatement testNode;
                    TryGetMenuStatement(x, out testNode, true);
                    return testNode;
                });
                result = parser.StatementFactory.TryParseNode(parser, out node, containingModule, prepStatementBinder, returnStatementBinder, false, validExitKeywords, csfs);
            }

            return result;
        }
    }

    public class MenuStatement : FglStatement
    {
        public List<NameExpression> OptionNames { get; private set; }

        public static bool TryParseNode(Parser parser, out MenuStatement node, bool returnFalseInsteadOfErrors = false)
        {
            node = new MenuStatement();
            node.StartIndex = parser.Token.Span.Start;
            node.OptionNames = new List<NameExpression>();
            bool result = true;

            switch(parser.PeekToken().Kind)
            {
                case TokenKind.ContinueKeyword:
                case TokenKind.ExitKeyword:
                    {
                        if (parser.PeekToken(TokenKind.MenuKeyword, 2))
                        {
                            parser.NextToken();
                            node.StartIndex = parser.Token.Span.Start;
                            parser.NextToken();
                        }
                        else
                            result = false;
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
                        {
                            if (!returnFalseInsteadOfErrors)
                                parser.ReportSyntaxError("Expecting \"option\" keyword in menu statement.");
                            else
                                return false;
                        }
                            
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
                        {
                            if (!returnFalseInsteadOfErrors)
                                parser.ReportSyntaxError("Expecting \"option\" keyword in menu statement.");
                            else
                                return false;
                        }
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

    public class MenuOption : AstNode, IOutlinableResult
    {
        public ExpressionNode OptionName { get; private set; }
        public ExpressionNode OptionComment { get; private set; }
        public ExpressionNode HelpNumber { get; private set; }
        public VirtualKey KeyName { get; private set; }

        public NameExpression ActionName { get; private set; }

        public ExpressionNode IdleSeconds { get; private set; }
        public bool IsBeforeMenuBlock { get; private set;}

        private MenuOption()
        {
        }

        /// <summary>
        /// Only use this constructor to create a "before menu" block!
        /// </summary>
        /// <param name="beforeMenuStartIndex"></param>
        /// <param name="beforeMenuDecoratorEndIndex"></param>
        /// <param name="beforeMenuEndIndex"></param>
        /// <param name="beforeMenuStatements"></param>
        internal MenuOption(int beforeMenuStartIndex, int beforeMenuDecoratorEndIndex, int beforeMenuEndIndex, IEnumerable<FglStatement> beforeMenuStatements)
        {
            IsBeforeMenuBlock = true;
            StartIndex = beforeMenuStartIndex;
            DecoratorEnd = beforeMenuDecoratorEndIndex;
            EndIndex = beforeMenuEndIndex;

            foreach (var stmt in beforeMenuStatements)
                Children.Add(stmt.StartIndex, stmt);
        }

        public static bool TryParseNode(Parser parser, out MenuOption node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
        {
            node = new MenuOption();
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.Ampersand:
                    {
                        PreprocessorNode preNode;
                        PreprocessorNode.TryParseNode(parser, out preNode);
                        node.StartIndex = -1;
                        break;
                    }
                case TokenKind.CommandKeyword:
                    {
                        parser.NextToken();
                        node.StartIndex = parser.Token.Span.Start;
                        bool getOptionName = false;
                        if(parser.PeekToken(TokenKind.KeyKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.NextToken();
                                VirtualKey keyName;
                                if (VirtualKey.TryGetKey(parser, out keyName))
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
                            node.DecoratorEnd = parser.Token.Span.End;
                        }
                        else
                        {
                            getOptionName = true;
                        }
                        
                        
                        // at this point we need to try to get a menu-statement. If it doesn't work, we have some other stuff to gather
                        FglStatement menuStmt = null;
                        if (getOptionName || !MenuStatementFactory.TryGetStatement(parser, out menuStmt, containingModule, prepStatementBinder, returnStatementBinder, validExitKeywords, contextStatementFactories))
                        {
                            ExpressionNode optionName;
                            if (ExpressionNode.TryGetExpressionNode(parser, out optionName))
                                node.OptionName = optionName;
                            else
                                parser.ReportSyntaxError("Invalid option-name found in menu command option.");

                            node.DecoratorEnd = parser.Token.Span.End;

                            if(parser.PeekToken(TokenCategory.StringLiteral) ||
                               parser.PeekToken(TokenCategory.Identifier))
                            {
                                ExpressionNode optionComment;
                                if (ExpressionNode.TryGetExpressionNode(parser, out optionComment))
                                    node.OptionComment = optionComment;

                            }
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

                        while (MenuStatementFactory.TryGetStatement(parser, out menuStmt, containingModule, prepStatementBinder, returnStatementBinder, validExitKeywords, contextStatementFactories))
                            if(menuStmt != null && !node.Children.ContainsKey(menuStmt.StartIndex))
                                node.Children.Add(menuStmt.StartIndex, menuStmt);

                        break;
                    }
                case TokenKind.OnKeyword:
                    {
                        parser.NextToken();
                        node.StartIndex = parser.Token.Span.Start;
                        if (parser.PeekToken(TokenKind.ActionKeyword))
                        {
                            parser.NextToken();
                            NameExpression action;
                            if (NameExpression.TryParseNode(parser, out action))
                                node.ActionName = action;
                            else
                                parser.ReportSyntaxError("Invalid action-name found in menu option.");
                            node.DecoratorEnd = parser.Token.Span.End;
                            FglStatement menuStmt = null;
                            while (MenuStatementFactory.TryGetStatement(parser, out menuStmt, containingModule, prepStatementBinder, returnStatementBinder, validExitKeywords, contextStatementFactories) && menuStmt != null)
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
                            node.DecoratorEnd = parser.Token.Span.End;
                            FglStatement menuStmt = null;
                            while (MenuStatementFactory.TryGetStatement(parser, out menuStmt, containingModule, prepStatementBinder, returnStatementBinder, validExitKeywords, contextStatementFactories) && menuStmt != null)
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

        public bool CanOutline
        {
            get { return true; }
        }

        public int DecoratorStart
        {
            get
            {
                return StartIndex;
            }
            set
            {
            }
        }

        public int DecoratorEnd { get; set; }
    }
}
