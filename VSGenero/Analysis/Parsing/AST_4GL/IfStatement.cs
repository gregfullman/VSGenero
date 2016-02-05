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

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class IfStatement : FglStatement
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out IfStatement node,
                                 IModuleResult containingModule,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 ExpressionParsingOptions expressionOptions = null,
                                 HashSet<TokenKind> endKeywords = null)
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

                    HashSet<TokenKind> newEndKeywords = new HashSet<TokenKind>();
                    if (endKeywords != null)
                        newEndKeywords.AddRange(endKeywords);
                    newEndKeywords.Add(TokenKind.IfKeyword);

                    IfBlockContentsNode ifBlock;
                    prepStatementBinders.Insert(0, node.BindPrepareCursorFromIdentifier);
                    if (IfBlockContentsNode.TryParseNode(parser, out ifBlock, containingModule, prepStatementBinders, returnStatementBinder,
                                                         limitedScopeVariableAdder, validExitKeywords, contextStatementFactories, expressionOptions, newEndKeywords))
                    {
                        if(ifBlock != null)
                            node.Children.Add(ifBlock.StartIndex, ifBlock);
                    }

                    if (parser.PeekToken(TokenKind.ElseKeyword))
                    {
                        parser.NextToken();
                        ElseBlockContentsNode elseBlock;
                        if (ElseBlockContentsNode.TryParseNode(parser, out elseBlock, containingModule, prepStatementBinders, returnStatementBinder,
                                                               limitedScopeVariableAdder, validExitKeywords, contextStatementFactories, expressionOptions, newEndKeywords))
                        {
                            if (elseBlock != null)
                            {
                                node.Children.Add(elseBlock.StartIndex, elseBlock);

                                // Add the span of "else" to the additional decorators
                                node.AdditionalDecoratorRanges.Add(elseBlock.StartIndex, elseBlock.StartIndex + 4);
                            }
                        }
                    }
                    prepStatementBinders.RemoveAt(0);

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

        public override bool CanOutline
        {
            get { return true; }
        }

        public override int DecoratorEnd { get; set; }
    }

    public class IfBlockContentsNode : AstNode4gl
    {
        public static bool TryParseNode(Genero4glParser parser, out IfBlockContentsNode node,
                                 IModuleResult containingModule,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 ExpressionParsingOptions expressionOptions = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = new IfBlockContentsNode();
            node.StartIndex = parser.Token.Span.Start;
            prepStatementBinders.Insert(0, node.BindPrepareCursorFromIdentifier);
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                  !parser.PeekToken(TokenKind.ElseKeyword) &&
                  !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.IfKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinders, 
                                                         returnStatementBinder, limitedScopeVariableAdder, false, validExitKeywords, 
                                                         contextStatementFactories, expressionOptions, endKeywords))
                {
                    AstNode4gl stmtNode = statement as AstNode4gl;
                    if (stmtNode != null && !node.Children.ContainsKey(stmtNode.StartIndex))
                        node.Children.Add(stmtNode.StartIndex, stmtNode);
                }
                else if(parser.PeekToken(TokenKind.EndKeyword) && endKeywords != null && endKeywords.Contains(parser.PeekToken(2).Kind))
                {
                    break;
                }
                else
                {
                    parser.NextToken();
                }
            }
            prepStatementBinders.RemoveAt(0);
            node.EndIndex = parser.Token.Span.End;

            return true;
        }
    }

    public class ElseBlockContentsNode : AstNode4gl
    {
        public static bool TryParseNode(Genero4glParser parser, out ElseBlockContentsNode node,
                                 IModuleResult containingModule,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 ExpressionParsingOptions expressionOptions = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = new ElseBlockContentsNode();
            node.StartIndex = parser.Token.Span.Start;
            prepStatementBinders.Insert(0, node.BindPrepareCursorFromIdentifier);
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                   !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.IfKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinders, 
                                                         returnStatementBinder, limitedScopeVariableAdder, false, validExitKeywords, 
                                                         contextStatementFactories, expressionOptions, endKeywords))
                {
                    AstNode4gl stmtNode = statement as AstNode4gl;
                    if(stmtNode != null && !node.Children.ContainsKey(stmtNode.StartIndex))
                        node.Children.Add(stmtNode.StartIndex, stmtNode);
                }
                else if (parser.PeekToken(TokenKind.EndKeyword) && endKeywords != null && endKeywords.Contains(parser.PeekToken(2).Kind))
                {
                    break;
                }
                else
                {
                    parser.NextToken();
                }
            }
            prepStatementBinders.RemoveAt(0);
            node.EndIndex = parser.Token.Span.End;

            return true;
        }
    }
}
