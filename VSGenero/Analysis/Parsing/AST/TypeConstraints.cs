using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public static class TypeConstraints
    {
        public static HashSet<TokenKind> ConstrainedTypes = new HashSet<TokenKind>
        {
            TokenKind.CharKeyword,
            TokenKind.CharacterKeyword,
            TokenKind.VarcharKeyword,
            TokenKind.DecKeyword,
            TokenKind.DecimalKeyword,
            TokenKind.NumericKeyword,
            TokenKind.MoneyKeyword,
            TokenKind.DatetimeKeyword,
            TokenKind.IntervalKeyword
        };

        private static HashSet<TokenKind> _dtQuals = new HashSet<TokenKind>
        {
            TokenKind.YearKeyword,
            TokenKind.MonthKeyword,
            TokenKind.DayKeyword,
            TokenKind.HourKeyword,
            TokenKind.MinuteKeyword,
            TokenKind.SecondKeyword,
            TokenKind.FractionKeyword
        };

        public static HashSet<TokenKind> DateTimeQualifiers
        {
            get { return _dtQuals; }
        }

        public static bool VerifyValidConstraint(IParser parser, out string typeConstraintString, TokenKind specifiedToken = TokenKind.EndOfFile, bool suppressErrors = false, TokenKind alreadyEatenToken = TokenKind.EndOfFile)
        {
            typeConstraintString = null;
            StringBuilder sb = new StringBuilder();
            TokenKind tokenToCheck = specifiedToken;
            if(tokenToCheck == TokenKind.EndOfFile)
            {
                if (ConstrainedTypes.Contains(parser.Token.Token.Kind))
                {
                    sb.Append(Tokens.TokenKinds[parser.Token.Token.Kind]);
                    tokenToCheck = parser.Token.Token.Kind;
                }
                else
                {
                    return false;
                }
            }

            switch (tokenToCheck)
            {
                case TokenKind.CharKeyword:
                case TokenKind.CharacterKeyword:
                case TokenKind.VarcharKeyword:
                    {
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            sb.Append("(");
                            ExpressionNode expr;
                            if (ExpressionNode.TryGetExpressionNode(parser, out expr))
                            {
                                sb.Append(expr.ToString());
                                if(tokenToCheck == TokenKind.VarcharKeyword &&
                                   parser.PeekToken(TokenKind.Comma))
                                {
                                    parser.NextToken();
                                    sb.Append(", ");
                                    if (ExpressionNode.TryGetExpressionNode(parser, out expr))
                                    {
                                        sb.Append(expr.ToString());
                                    }
                                    else
                                        parser.ReportSyntaxError("Invalid expression found in varchar definition.");
                                }
                                if (parser.PeekToken(TokenKind.RightParenthesis))
                                {
                                    parser.NextToken();
                                    sb.Append(")");
                                    typeConstraintString = sb.ToString();
                                    return true;
                                }
                            }
                            parser.ReportSyntaxError("Incomplete character-type size specification found.");
                            return false;
                        }
                        break;
                    }
                case TokenKind.DecKeyword:
                case TokenKind.DecimalKeyword:
                case TokenKind.MoneyKeyword:
                case TokenKind.NumericKeyword:
                    {
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            sb.Append("(");
                            if (parser.PeekToken(TokenCategory.NumericLiteral))
                            {
                                parser.NextToken();
                                sb.Append(parser.Token.Token.Value.ToString());

                                if (parser.PeekToken(TokenKind.Comma))
                                {
                                    parser.NextToken();
                                    sb.Append(", ");
                                    if (parser.PeekToken(TokenCategory.NumericLiteral))
                                    {
                                        parser.NextToken();
                                        sb.Append(parser.Token.Token.Value.ToString());
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError("Incomplete decimal/money-type scale specification found.");
                                        return false;
                                    }
                                }

                                if (parser.PeekToken(TokenKind.RightParenthesis))
                                {
                                    parser.NextToken();
                                    sb.Append(")");
                                    typeConstraintString = sb.ToString();
                                    return true;
                                }
                            }
                            parser.ReportSyntaxError("Incomplete decimal/money-type precision specification found.");
                            return false;
                        }
                        break;
                    }
                case TokenKind.CurrentKeyword:
                case TokenKind.DatetimeKeyword:
                    {
                        TokenKind tokenToLookAt = alreadyEatenToken;
                        if (tokenToLookAt == TokenKind.EndOfFile)
                            tokenToLookAt = parser.PeekToken().Kind;
                        if (_dtQuals.Contains(tokenToLookAt))
                        {
                            if(alreadyEatenToken == TokenKind.EndOfFile)
                                parser.NextToken();
                            sb.AppendFormat(" {0}", parser.Token.Token.Value.ToString());

                            if (parser.PeekToken(TokenKind.ToKeyword))
                            {
                                parser.NextToken();
                                sb.AppendFormat(" {0}", parser.Token.Token.Value.ToString());

                                if (_dtQuals.Contains(parser.PeekToken().Kind))
                                {
                                    parser.NextToken();
                                    sb.AppendFormat(" {0}", parser.Token.Token.Value.ToString());

                                    if (parser.Token.Token.Kind == TokenKind.FractionKeyword && parser.PeekToken(TokenKind.LeftParenthesis))
                                    {
                                        parser.NextToken();
                                        sb.Append("(");

                                        if (parser.PeekToken(TokenCategory.NumericLiteral))
                                        {
                                            parser.NextToken();
                                            sb.Append(parser.Token.Token.Value.ToString());

                                            if (parser.PeekToken(TokenKind.RightParenthesis))
                                            {
                                                parser.NextToken();
                                                sb.Append(")");
                                                typeConstraintString = sb.ToString();
                                                return true;
                                            }
                                        }
                                        if (!suppressErrors)
                                            parser.ReportSyntaxError("Invalid datetime fraction specification found.");
                                        return false;
                                    }

                                    typeConstraintString = sb.ToString();
                                    return true;
                                }
                            }
                        }
                        if (!suppressErrors)
                            parser.ReportSyntaxError("Invalid datetime specification found.");
                        return false;
                    }
                case TokenKind.IntervalKeyword:
                    {
                        if (_dtQuals.Contains(parser.PeekToken().Kind))
                        {
                            parser.NextToken();
                            sb.AppendFormat(" {0}", parser.Token.Token.Value.ToString());

                            if (parser.Token.Token.Kind == TokenKind.FractionKeyword && parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.ReportSyntaxError("A scale cannot be defined on the first span of the interval.");
                                return false;
                            }

                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.NextToken();
                                sb.Append("(");
                                if (parser.PeekToken(TokenCategory.NumericLiteral))
                                {
                                    parser.NextToken();
                                    sb.Append(parser.Token.Token.Value.ToString());
                                    if (parser.PeekToken(TokenKind.RightParenthesis))
                                    {
                                        parser.NextToken();
                                        sb.Append(")");
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError("Invalid interval specification found.");
                                        return false;
                                    }
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Invalid interval specification found.");
                                    return false;
                                }
                            }

                            if (parser.PeekToken(TokenKind.ToKeyword))
                            {
                                parser.NextToken();
                                sb.Append(" to");

                                if (_dtQuals.Contains(parser.PeekToken().Kind))
                                {
                                    parser.NextToken();
                                    sb.AppendFormat(" {0}", parser.Token.Token.Value.ToString());

                                    if (parser.Token.Token.Kind == TokenKind.FractionKeyword && parser.PeekToken(TokenKind.LeftParenthesis))
                                    {
                                        parser.NextToken();
                                        sb.Append("(");
                                        if (parser.PeekToken(TokenCategory.NumericLiteral))
                                        {
                                            parser.NextToken();
                                            sb.Append(parser.Token.Token.Value.ToString());
                                            if (parser.PeekToken(TokenKind.RightParenthesis))
                                            {
                                                parser.NextToken();
                                                sb.Append(")");
                                                typeConstraintString = sb.ToString();
                                                return true;
                                            }
                                        }
                                        parser.ReportSyntaxError("Invalid fraction scale specification found.");
                                        return false;
                                    }

                                    typeConstraintString = sb.ToString();
                                    return true;
                                }
                            }
                        }
                        parser.ReportSyntaxError("Invalid interval specification found.");
                        return false;
                    }
            }

            return false;
        }
    }
}
