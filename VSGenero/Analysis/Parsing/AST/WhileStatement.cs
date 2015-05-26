using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class WhileStatement : FglStatement, IOutlinableResult
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Parser parser, out WhileStatement node, 
                                        IModuleResult containingModule,
                                        Action<PrepareStatement> prepStatementBinder = null,
                                        List<TokenKind> validExitKeywords = null)
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

                node.DecoratorEnd = parser.Token.Span.End;

                List<TokenKind> validExits = new List<TokenKind>();
                if (validExitKeywords != null)
                    validExits.AddRange(validExitKeywords);
                validExits.Add(TokenKind.WhileKeyword);

                while(!parser.PeekToken(TokenKind.EndOfFile) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.WhileKeyword, 2)))
                {
                    FglStatement statement;
                    if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinder, false, validExits))
                    {
                        AstNode stmtNode = statement as AstNode;
                        node.Children.Add(stmtNode.StartIndex, stmtNode);

                        if(statement is ExitStatement &&
                           (statement as ExitStatement).ExitType != TokenKind.WhileKeyword)
                        {
                            if (validExitKeywords == null || !validExitKeywords.Contains((statement as ExitStatement).ExitType))
                                parser.ReportSyntaxError("Invalid exit statement for while loop detected.");
                        }
                        
                        if(statement is ContinueStatement &&
                           (statement as ContinueStatement).ContinueType != TokenKind.WhileKeyword)
                        {
                            if (validExitKeywords == null || !validExitKeywords.Contains((statement as ContinueStatement).ContinueType))
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
