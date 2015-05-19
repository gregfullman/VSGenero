using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class IfStatement : FglStatement
    {
        public ExpressionNode ConditionExpression { get; private set; }

        public static bool TryParseNode(Parser parser, out IfStatement node, 
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.IfKeyword))
            {
                result = true;
                node = new IfStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode conditionExpr;
                if (!ExpressionNode.TryGetExpressionNode(parser, out conditionExpr, new List<TokenKind> { TokenKind.ThenKeyword }))
                {
                    parser.ReportSyntaxError("An if statement must have a condition expression.");
                }
                else
                {
                    node.ConditionExpression = conditionExpr;
                }

                if (!parser.PeekToken(TokenKind.ThenKeyword))
                    parser.ReportSyntaxError("An if statement must have a \"then\" keyword prior to containing code.");
                else
                    parser.NextToken();

                IfBlockContentsNode ifBlock;
                if(IfBlockContentsNode.TryParseNode(parser, out ifBlock, prepStatementResolver, prepStatementBinder, validExitKeywords))
                {
                    node.Children.Add(ifBlock.StartIndex, ifBlock);
                }

                if(parser.PeekToken(TokenKind.ElseKeyword))
                {
                    parser.NextToken();
                    ElseBlockContentsNode elseBlock;
                    if(ElseBlockContentsNode.TryParseNode(parser, out elseBlock, prepStatementResolver, prepStatementBinder, validExitKeywords))
                    {
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

            return result;
        }
    }

    public class IfBlockContentsNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out IfBlockContentsNode node,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null)
        {
            node = new IfBlockContentsNode();
            node.StartIndex = parser.Token.Span.Start;
            while(!parser.PeekToken(TokenKind.EndOfFile) &&
                  !parser.PeekToken(TokenKind.ElseKeyword) &&
                  !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.IfKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, prepStatementResolver, prepStatementBinder, false, validExitKeywords))
                {
                    AstNode stmtNode = statement as AstNode;
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
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null)
        {
            node = new ElseBlockContentsNode();
            node.StartIndex = parser.Token.Span.Start;
            while (!parser.PeekToken(TokenKind.EndOfFile) && 
                   !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.IfKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, prepStatementResolver, prepStatementBinder, false, validExitKeywords))
                {
                    AstNode stmtNode = statement as AstNode;
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
