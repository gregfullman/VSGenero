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
    public class WhileStatement : FglStatement
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out WhileStatement node,
                                        IModuleResult containingModule,
                                        Action<PrepareStatement> prepStatementBinder = null,
                                        Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                        Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                        List<TokenKind> validExitKeywords = null,
                                        IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                        ExpressionParsingOptions expressionOptions = null,
                                        HashSet<TokenKind> endKeywords = null)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.WhileKeyword))
            {
                result = true;
                node = new WhileStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode conditionExpr;
                if (!ExpressionNode.TryGetExpressionNode(parser, out conditionExpr, Genero4glAst.ValidStatementKeywords.ToList(), expressionOptions))
                {
                    parser.ReportSyntaxError("A while statement must have a condition expression.");
                }
                else
                {
                    node.ConditionExpression = conditionExpr;
                }

                node.DecoratorEnd = parser.Token.Span.End;

                List<TokenKind> validExits = new List<TokenKind>();
                if (validExitKeywords != null)
                    validExits.AddRange(validExitKeywords);
                validExits.Add(TokenKind.WhileKeyword);

                HashSet<TokenKind> newEndKeywords = new HashSet<TokenKind>();
                if (endKeywords != null)
                    newEndKeywords.AddRange(endKeywords);
                newEndKeywords.Add(TokenKind.WhileKeyword);

                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.WhileKeyword, 2)))
                {
                    FglStatement statement;
                    if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinder, 
                                                             returnStatementBinder, limitedScopeVariableAdder, false, validExits,
                                                             contextStatementFactories, expressionOptions, newEndKeywords) && statement != null)
                    {
                        AstNode4gl stmtNode = statement as AstNode4gl;
                        node.Children.Add(stmtNode.StartIndex, stmtNode);

                        if (statement is ExitStatement &&
                           (statement as ExitStatement).ExitType != TokenKind.WhileKeyword)
                        {
                            if (validExitKeywords == null || !validExitKeywords.Contains((statement as ExitStatement).ExitType))
                                parser.ReportSyntaxError("Invalid exit statement for while loop detected.");
                        }

                        if (statement is ContinueStatement &&
                           (statement as ContinueStatement).ContinueType != TokenKind.WhileKeyword)
                        {
                            if (validExitKeywords == null || !validExitKeywords.Contains((statement as ContinueStatement).ContinueType))
                                parser.ReportSyntaxError("Invalid continue statement for while loop detected.");
                        }
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

                if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.WhileKeyword, 2)))
                {
                    parser.ReportSyntaxError("A while statement must be terminated with \"end while\".");
                }
                else
                {
                    parser.NextToken(); // advance to the 'end' token
                    parser.NextToken(); // advance to the 'while' token
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }

        public override bool CanOutline
        {
            get { return true; }
        }

        public override int DecoratorEnd { get; set; }
    }
}
