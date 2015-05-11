using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class CaseStatement :FglStatement
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Parser parser, out CaseStatement node,
                                        Func<string, PrepareStatement> prepStatementResolver = null,
                                        Action<PrepareStatement> prepStatementBinder = null)
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

                // need to allow multiple when statements
                bool whenCases = true;
                while(!parser.PeekToken(TokenKind.EndOfFile) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.CaseKeyword, 2)))
                {
                    WhenStatement whenStmt;
                    if(WhenStatement.TryParseNode(parser, out whenStmt, prepStatementResolver, prepStatementBinder))
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
                        if(OtherwiseStatement.TryParseNode(parser, out otherStmt, prepStatementResolver, prepStatementBinder))
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
    }

    public class WhenStatement : AstNode
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Parser parser, out WhenStatement node,
                                        Func<string, PrepareStatement> prepStatementResolver = null,
                                        Action<PrepareStatement> prepStatementBinder = null)
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
                if (ExpressionNode.TryGetExpressionNode(parser, out expr, GeneroAst.ValidStatementKeywords.ToList()))
                    node.ConditionExpression = expr;
                else
                    parser.ReportSyntaxError("Case value or conditional expression expected.");

                while (!parser.PeekToken(TokenKind.EndOfFile) && 
                      !parser.PeekToken(TokenKind.WhenKeyword) &&
                      !parser.PeekToken(TokenKind.OtherwiseKeyword) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.CaseKeyword, 2)))
                {
                    FglStatement statement;
                    if (parser.StatementFactory.TryParseNode(parser, out statement, prepStatementResolver, prepStatementBinder))
                    {
                        AstNode stmtNode = statement as AstNode;
                        node.Children.Add(stmtNode.StartIndex, stmtNode);

                        if (statement is ExitStatement &&
                           (statement as ExitStatement).ExitType != TokenKind.CaseKeyword)
                        {
                            parser.ReportSyntaxError("Invalid exit statement for case statement block detected.");
                        }
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
    }

    public class OtherwiseStatement : AstNode
    {
        public static bool TryParseNode(Parser parser, out OtherwiseStatement node,
                                        Func<string, PrepareStatement> prepStatementResolver = null,
                                        Action<PrepareStatement> prepStatementBinder = null)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.OtherwiseKeyword))
            {
                result = true;
                node = new OtherwiseStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                while (!parser.PeekToken(TokenKind.EndOfFile) && 
                      !parser.PeekToken(TokenKind.WhenKeyword) &&
                      !parser.PeekToken(TokenKind.OtherwiseKeyword) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.CaseKeyword, 2)))
                {
                    FglStatement statement;
                    if (parser.StatementFactory.TryParseNode(parser, out statement, prepStatementResolver, prepStatementBinder))
                    {
                        AstNode stmtNode = statement as AstNode;
                        node.Children.Add(stmtNode.StartIndex, stmtNode);

                        if (statement is ExitStatement &&
                           (statement as ExitStatement).ExitType != TokenKind.CaseKeyword)
                        {
                            parser.ReportSyntaxError("Invalid exit statement for case statement block detected.");
                        }
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
    }
}
