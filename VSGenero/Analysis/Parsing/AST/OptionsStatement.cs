using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class OptionsStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out OptionsStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.OptionsKeyword))
            {
                result = true;
                node = new OptionsStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                switch(parser.PeekToken().Kind)
                {
                    case TokenKind.SqlKeyword:
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.InterruptKeyword))
                            {
                                parser.NextToken();
                                if(parser.PeekToken(TokenKind.OnKeyword) || parser.PeekToken(TokenKind.OffKeyword))
                                    parser.NextToken();
                                else
                                    parser.ReportSyntaxError("Invalid token found in options sql interrupt statement.");
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid token found in options sql statement.");
                            }
                            break;
                        }
                    case TokenKind.InputKeyword:
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.NoKeyword))
                                parser.NextToken();
                            if (parser.PeekToken(TokenKind.WrapKeyword))
                                parser.NextToken();
                            else
                                parser.ReportSyntaxError("Invalid token found in options input statement.");
                            break;
                        }
                    case TokenKind.FieldKeyword:
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.OrderKeyword))
                            {
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.ConstrainedKeyword) ||
                                   parser.PeekToken(TokenKind.UnconstrainedKeyword) ||
                                   parser.PeekToken(TokenKind.FormKeyword))
                                {
                                    parser.NextToken();
                                }
                                else
                                    parser.ReportSyntaxError("Expected one of keywords \"constrained\", \"unconstrained\", or \"form\" in options field statement.");
                            }
                            else
                                parser.ReportSyntaxError("Expected keyword \"order\" in options field statement.");
                            break;
                        }
                    default:
                        {
                            parser.ReportSyntaxError("Unsupported options statement found.");
                            break;
                        }
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
