using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class ForeachStatement : FglStatement
    {
        public NameExpression CursorId { get; private set; }
        public List<NameExpression> InputVars { get; private set; }
        public List<NameExpression> OutputVars { get; private set; }

        public static bool TryParseNode(Parser parser, out ForeachStatement node,
                                        Func<string, PrepareStatement> prepStatementResolver = null,
                                        Action<PrepareStatement> prepStatementBinder = null)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.ForeachKeyword))
            {
                result = true;
                node = new ForeachStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.InputVars = new List<NameExpression>();
                node.OutputVars = new List<NameExpression>();

                NameExpression cid;
                if (NameExpression.TryParseNode(parser, out cid))
                    node.CursorId = cid;
                else
                    parser.ReportSyntaxError("Invalid declared cursor id found in foreach statement.");

                HashSet<TokenKind> inVarMods = new HashSet<TokenKind> { TokenKind.InKeyword, TokenKind.OutKeyword, TokenKind.InOutKeyword };
                if (parser.PeekToken(TokenKind.UsingKeyword))
                {
                    parser.NextToken();
                    NameExpression inVar;
                    while (NameExpression.TryParseNode(parser, out inVar, TokenKind.Comma))
                    {
                        node.InputVars.Add(inVar);
                        if (inVarMods.Contains(parser.PeekToken().Kind))
                            parser.NextToken();
                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                        else
                            break;
                    }
                }

                if (parser.PeekToken(TokenKind.IntoKeyword))
                {
                    parser.NextToken();
                    NameExpression outVar;
                    while (NameExpression.TryParseNode(parser, out outVar, TokenKind.Comma))
                    {
                        node.InputVars.Add(outVar);
                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                        else
                            break;
                    }
                }

                if (parser.PeekToken(TokenKind.WithKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.ReoptimizationKeyword))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expecting keyword \"reoptimization\" in open statement.");
                }

                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                      !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ForeachKeyword, 2)))
                {
                    FglStatement statement;
                    if (parser.StatementFactory.TryParseNode(parser, out statement, prepStatementResolver, prepStatementBinder))
                    {
                        AstNode stmtNode = statement as AstNode;
                        node.Children.Add(stmtNode.StartIndex, stmtNode);

                        if (statement is ExitStatement &&
                           (statement as ExitStatement).ExitType != TokenKind.ForeachKeyword)
                        {
                            parser.ReportSyntaxError("Invalid exit statement for for loop detected.");
                        }

                        if (statement is ContinueStatement &&
                           (statement as ContinueStatement).ContinueType != TokenKind.ForeachKeyword)
                        {
                            parser.ReportSyntaxError("Invalid continue statement for for loop detected.");
                        }
                    }
                    else
                    {
                        parser.NextToken();
                    }
                }

                if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ForeachKeyword, 2)))
                {
                    parser.ReportSyntaxError("A foreach statement must be terminated with \"end foreach\".");
                }
                else
                {
                    parser.NextToken(); // advance to the 'end' token
                    parser.NextToken(); // advance to the 'foreach' token
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }
    }
}
