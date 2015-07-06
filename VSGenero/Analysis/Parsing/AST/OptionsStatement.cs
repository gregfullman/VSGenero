using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class OptionsStatement : FglStatement
    {
        private static char[] _controlKeyExlusions = new char[]
        {
            'A', 'D', 'H', 'I', 'J', 'K', 'L', 'M', 'R', 'X'
        };

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

                while (true)
                {
                    switch (parser.PeekToken().Kind)
                    {
                        case TokenKind.SqlKeyword:
                            {
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.InterruptKeyword))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.OnKeyword) || parser.PeekToken(TokenKind.OffKeyword))
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
                                if (parser.PeekToken(TokenKind.NoKeyword))
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
                        case TokenKind.InsertKeyword:
                        case TokenKind.DeleteKeyword:
                        case TokenKind.NextKeyword:
                        case TokenKind.PreviousKeyword:
                        case TokenKind.AcceptKeyword:
                        case TokenKind.HelpKeyword:
                            {
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.KeyKeyword))
                                {
                                    parser.NextToken();
                                    switch (parser.PeekToken().Kind)
                                    {
                                        case TokenKind.EscapeKeyword:
                                        case TokenKind.EscKeyword:
                                        case TokenKind.InterruptKeyword:
                                        case TokenKind.TagKeyword:
                                        case TokenKind.LeftKeyword:
                                        case TokenKind.ReturnKeyword:
                                        case TokenKind.EnterKeyword:
                                        case TokenKind.RightKeyword:
                                        case TokenKind.DownKeyword:
                                        case TokenKind.UpKeyword:
                                        case TokenKind.PreviousKeyword:
                                        case TokenKind.NextKeyword:
                                        case TokenKind.PrevpageKeyword:
                                        case TokenKind.NextpageKeyword:
                                            parser.NextToken();
                                            break;
                                        case TokenKind.ControlKeyword:
                                            parser.NextToken();
                                            if (parser.PeekToken(TokenKind.Subtract))
                                            {
                                                parser.NextToken();
                                                // TODO: do letter
                                                if (parser.PeekToken(TokenCategory.Identifier))
                                                {
                                                    parser.NextToken();
                                                    var name = parser.Token.Token.Value.ToString().ToUpper();
                                                    if (name.Length != 1 ||
                                                        ((int)name[0] < 65 || (int)name[0] > 90) &&
                                                        _controlKeyExlusions.Contains(name[0]))
                                                    {
                                                        parser.ReportSyntaxError("Invalid control key specified.");
                                                    }
                                                }
                                            }
                                            else
                                                parser.ReportSyntaxError("Expected '-' token in options statement.");
                                            break;
                                        default:
                                            {
                                                // TODO: do F1 - F255
                                                parser.NextToken();
                                                var name = parser.Token.Token.Value.ToString();
                                                if (name.StartsWith("F", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    var numberStr = name.Substring(1);
                                                    int number;
                                                    if (!int.TryParse(numberStr, out number) || number > 0 || number < 256)
                                                    {
                                                        parser.ReportSyntaxError("Invalid function key name found.");
                                                    }
                                                }
                                                else
                                                    parser.ReportSyntaxError("Function key name must start with 'F'.");
                                                break;
                                            }
                                    }
                                }
                                else
                                    parser.ReportSyntaxError("Expected keyword \"key\" in options statement.");
                                break;
                            }
                        default:
                            {
                                parser.ReportSyntaxError("Unsupported options statement found.");
                                break;
                            }
                    }

                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
