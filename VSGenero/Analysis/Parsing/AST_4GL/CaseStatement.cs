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
    public class CaseStatement : FglStatement
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out CaseStatement node,
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

            if (parser.PeekToken(TokenKind.CaseKeyword))
            {
                result = true;
                node = new CaseStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;


                if(!parser.PeekToken(TokenKind.WhenKeyword))
                {
                    ExpressionNode tempExpr;
                    if (ExpressionNode.TryGetExpressionNode(parser, out tempExpr, new List<TokenKind> { TokenKind.WhenKeyword }))
                    {
                        node.ConditionExpression = tempExpr;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid conditional expression found in case statement");
                    }
                }
                node.DecoratorEnd = parser.Token.Span.End;

                List<TokenKind> validExits = new List<TokenKind>();
                if (validExitKeywords != null)
                    validExits.AddRange(validExitKeywords);
                validExits.Add(TokenKind.CaseKeyword);

                HashSet<TokenKind> newEndKeywords = new HashSet<TokenKind>();
                if (endKeywords != null)
                    newEndKeywords.AddRange(endKeywords);
                newEndKeywords.Add(TokenKind.CaseKeyword);

                // need to allow multiple when statements
                bool whenCases = true;
                while(!parser.PeekToken(TokenKind.EndOfFile) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.CaseKeyword, 2)))
                {
                    WhenStatement whenStmt;
                    if (WhenStatement.TryParseNode(parser, out whenStmt, containingModule, prepStatementBinder, 
                                                   returnStatementBinder, limitedScopeVariableAdder, validExits, 
                                                   contextStatementFactories, expressionOptions, newEndKeywords) && whenStmt != null)
                    {
                        if (whenCases)
                        {
                            node.Children.Add(whenStmt.StartIndex, whenStmt);
                        }
                        else
                        {
                            parser.ReportSyntaxError("A when case cannot come after an otherwise case block.");
                            break;
                        }
                    }
                    else
                    {
                        OtherwiseStatement otherStmt;
                        if (OtherwiseStatement.TryParseNode(parser, out otherStmt, containingModule, prepStatementBinder, returnStatementBinder, 
                                                            limitedScopeVariableAdder, validExits, contextStatementFactories, expressionOptions, newEndKeywords) && otherStmt != null)
                        {
                            whenCases = false;
                            node.Children.Add(otherStmt.StartIndex, otherStmt);
                        }
                        else
                        {
                            parser.ReportSyntaxError("Invalid statement detected within case statement block.");
                            break;
                        }
                    }
                }

                if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.CaseKeyword, 2)))
                {
                    parser.ReportSyntaxError("A case statement must be terminated with \"end case\".");
                }
                else
                {
                    parser.NextToken(); // advance to the 'end' token
                    parser.NextToken(); // advance to the 'case' token
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

    public class WhenStatement : AstNode4gl
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out WhenStatement node,
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

            if (parser.PeekToken(TokenKind.WhenKeyword))
            {
                result = true;
                node = new WhenStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode expr;
                if (ExpressionNode.TryGetExpressionNode(parser, out expr, Genero4glAst.ValidStatementKeywords.ToList()))
                    node.ConditionExpression = expr;
                else
                    parser.ReportSyntaxError("Case value or conditional expression expected.");
                node.DecoratorEnd = parser.Token.Span.End;
                while (!parser.PeekToken(TokenKind.EndOfFile) && 
                      !parser.PeekToken(TokenKind.WhenKeyword) &&
                      !parser.PeekToken(TokenKind.OtherwiseKeyword) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.CaseKeyword, 2)))
                {
                    FglStatement statement;
                    if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinder, 
                                                             returnStatementBinder, limitedScopeVariableAdder, false, validExitKeywords, 
                                                             contextStatementFactories, expressionOptions, endKeywords) && statement != null)
                    {
                        AstNode4gl stmtNode = statement as AstNode4gl;
                        node.Children.Add(stmtNode.StartIndex, stmtNode);

                        if (statement is ExitStatement &&
                           (statement as ExitStatement).ExitType != TokenKind.CaseKeyword)
                        {
                            if (!validExitKeywords.Contains((statement as ExitStatement).ExitType))
                                parser.ReportSyntaxError("Invalid exit statement for case statement block detected.");
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

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }

        public override bool CanOutline
        {
            get { return true; }
        }

        public override int DecoratorEnd { get; set; }
    }

    public class OtherwiseStatement : AstNode4gl
    {
        public static bool TryParseNode(Genero4glParser parser, out OtherwiseStatement node,
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

            if (parser.PeekToken(TokenKind.OtherwiseKeyword))
            {
                result = true;
                node = new OtherwiseStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.DecoratorEnd = parser.Token.Span.End;
                while (!parser.PeekToken(TokenKind.EndOfFile) && 
                      !parser.PeekToken(TokenKind.WhenKeyword) &&
                      !parser.PeekToken(TokenKind.OtherwiseKeyword) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.CaseKeyword, 2)))
                {
                    FglStatement statement;
                    if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinder, returnStatementBinder,
                                                             limitedScopeVariableAdder, false, validExitKeywords, contextStatementFactories, expressionOptions) 
                                                          && statement != null)
                    {
                        AstNode4gl stmtNode = statement as AstNode4gl;
                        node.Children.Add(stmtNode.StartIndex, stmtNode);

                        if (statement is ExitStatement &&
                           (statement as ExitStatement).ExitType != TokenKind.CaseKeyword)
                        {
                            if (!validExitKeywords.Contains((statement as ExitStatement).ExitType))
                                parser.ReportSyntaxError("Invalid exit statement for case statement block detected.");
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

                node.EndIndex = parser.Token.Span.End;
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
