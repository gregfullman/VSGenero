using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class ExecuteStatement : FglStatement
    {
        public ExpressionNode ImmediateExpression { get; private set; }
        public NameExpression PreparedStatementId { get; private set; }
        public List<NameExpression> InputVars { get; private set; }
        public List<NameExpression> OutputVars { get; private set; }

        public static bool TryParseNode(Parser parser, out ExecuteStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.ExecuteKeyword))
            {
                result = true;
                node = new ExecuteStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.InputVars = new List<NameExpression>();
                node.OutputVars = new List<NameExpression>();

                if(parser.PeekToken(TokenKind.ImmediateKeyword))
                {
                    parser.NextToken();
                    ExpressionNode immExpr;
                    if (ExpressionNode.TryGetExpressionNode(parser, out immExpr, GeneroAst.ValidStatementKeywords.ToList()))
                        node.ImmediateExpression = immExpr;
                    else
                        parser.ReportSyntaxError("Invalid expression found in execute immediate statement.");
                }
                else
                {
                    NameExpression cid;
                    if (NameExpression.TryParseNode(parser, out cid))
                        node.PreparedStatementId = cid;
                    else
                        parser.ReportSyntaxError("Invalid prepared statement id found in execute statement.");

                    HashSet<TokenKind> inVarMods = new HashSet<TokenKind> { TokenKind.InKeyword, TokenKind.OutKeyword, TokenKind.InOutKeyword };
                    if(parser.PeekToken(TokenKind.UsingKeyword))
                    {
                        parser.NextToken();
                        NameExpression inVar;
                        while(NameExpression.TryParseNode(parser, out inVar, TokenKind.Comma))
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

                    if(parser.PeekToken(TokenKind.IntoKeyword))
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
                }
                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
