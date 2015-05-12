using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class OpenStatement : FglStatement
    {
        public NameExpression CursorId { get; private set; }
        public List<NameExpression> InputVars { get; private set; }

        public static bool TryParseNode(Parser parser, out OpenStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.OpenKeyword))
            {
                result = true;
                node = new OpenStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.InputVars = new List<NameExpression>();

                NameExpression cid;
                if (NameExpression.TryParseNode(parser, out cid))
                    node.CursorId = cid;
                else
                    parser.ReportSyntaxError("Invalid declared cursor id found in open statement.");

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

                if(parser.PeekToken(TokenKind.WithKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.ReoptimizationKeyword))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expecting keyword \"reoptimization\" in open statement.");
                }
                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
