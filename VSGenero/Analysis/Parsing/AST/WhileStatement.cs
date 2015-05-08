using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class WhileStatement : FglStatement
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Parser parser, out WhileStatement node, 
                                        Func<string, PrepareStatement> prepStatementResolver = null,
                                        Action<PrepareStatement> prepStatementBinder = null)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.WhileKeyword))
            {
                result = true;
                node = new WhileStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode conditionExpr;
                if (!ExpressionNode.TryGetExpressionNode(parser, out conditionExpr, GeneroAst.ValidStatementKeywords.ToList()))
                {
                    parser.ReportSyntaxError("A while statement must have a condition expression.");
                }
                else
                {
                    node.ConditionExpression = conditionExpr;
                }

                while(!parser.PeekToken(TokenKind.EndOfFile) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.WhileKeyword, 2)))
                {
                    FglStatement statement;
                    if (parser.StatementFactory.TryParseNode(parser, out statement, prepStatementResolver, prepStatementBinder))
                    {
                        AstNode stmtNode = statement as AstNode;
                        node.Children.Add(stmtNode.StartIndex, stmtNode);

                        if(statement is ExitStatement &&
                           (statement as ExitStatement).ExitType != TokenKind.WhileKeyword)
                        {
                            parser.ReportSyntaxError("Invalid exit statement for while loop detected.");
                        }
                        
                        if(statement is ContinueStatement &&
                           (statement as ContinueStatement).ContinueType != TokenKind.WhileKeyword)
                        {
                            parser.ReportSyntaxError("Invalid continue statement for while loop detected.");
                        }
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
    }
}
