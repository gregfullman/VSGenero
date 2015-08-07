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
    public class IfStatement : FglStatement, IOutlinableResult
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Parser parser, out IfStatement node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 ExpressionParsingOptions expressionOptions = null)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.IfKeyword))
            {
                result = true;
                node = new IfStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode conditionExpr;
                if (!ExpressionNode.TryGetExpressionNode(parser, out conditionExpr, new List<TokenKind> { TokenKind.ThenKeyword }, expressionOptions))
                {
                    parser.ReportSyntaxError("An if statement must have a condition expression.");
                }
                else
                {
                    node.ConditionExpression = conditionExpr;
                }

                if (parser.PeekToken(TokenKind.ThenKeyword))
                {
                    parser.NextToken();

                    node.DecoratorEnd = parser.Token.Span.End;

                    IfBlockContentsNode ifBlock;
                    if (IfBlockContentsNode.TryParseNode(parser, out ifBlock, containingModule, prepStatementBinder, returnStatementBinder, validExitKeywords, contextStatementFactories, expressionOptions))
                    {
                        if(ifBlock != null)
                            node.Children.Add(ifBlock.StartIndex, ifBlock);
                    }

                    if (parser.PeekToken(TokenKind.ElseKeyword))
                    {
                        parser.NextToken();
                        ElseBlockContentsNode elseBlock;
                        if (ElseBlockContentsNode.TryParseNode(parser, out elseBlock, containingModule, prepStatementBinder, returnStatementBinder, validExitKeywords, contextStatementFactories, expressionOptions))
                        {
                            if(elseBlock != null)
                                node.Children.Add(elseBlock.StartIndex, elseBlock);
                        }
                    }

                    if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.IfKeyword, 2)))
                    {
                        parser.ReportSyntaxError("An if statement must be terminated with \"end if\".");
                    }
                    else
                    {
                        parser.NextToken(); // advance to the 'end' token
                        parser.NextToken(); // advance to the 'if' token
                        node.EndIndex = parser.Token.Span.End;
                    }
                }
                else
                    parser.ReportSyntaxError("An if statement must have a \"then\" keyword prior to containing code.");
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

    public class IfBlockContentsNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out IfBlockContentsNode node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 ExpressionParsingOptions expressionOptions = null)
        {
            node = new IfBlockContentsNode();
            node.StartIndex = parser.Token.Span.Start;
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                  !parser.PeekToken(TokenKind.ElseKeyword) &&
                  !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.IfKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinder, returnStatementBinder, false, validExitKeywords, contextStatementFactories, expressionOptions))
                {
                    AstNode stmtNode = statement as AstNode;
                    if (stmtNode != null && !node.Children.ContainsKey(stmtNode.StartIndex))
                        node.Children.Add(stmtNode.StartIndex, stmtNode);
                }
                else
                {
                    parser.NextToken();
                }
            }
            node.EndIndex = parser.Token.Span.End;

            return true;
        }
    }

    public class ElseBlockContentsNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out ElseBlockContentsNode node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 ExpressionParsingOptions expressionOptions = null)
        {
            node = new ElseBlockContentsNode();
            node.StartIndex = parser.Token.Span.Start;
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                   !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.IfKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinder, returnStatementBinder, false, validExitKeywords, contextStatementFactories, expressionOptions))
                {
                    AstNode stmtNode = statement as AstNode;
                    if(stmtNode != null && !node.Children.ContainsKey(stmtNode.StartIndex))
                        node.Children.Add(stmtNode.StartIndex, stmtNode);
                }
                else
                {
                    parser.NextToken();
                }
            }
            node.EndIndex = parser.Token.Span.End;

            return true;
        }
    }
}
