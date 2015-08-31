/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class OptionsStatement : FglStatement
    {
        private static char[] _controlKeyExlusions = new char[]
        {
            'A', 'D', 'H', 'I', 'J', 'K', 'L', 'M', 'R', 'X'
        };

        public static bool TryParseNode(Genero4glParser parser, out OptionsStatement node)
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
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.WrapKeyword))
                                        parser.NextToken();
                                    else
                                        parser.ReportSyntaxError("Invalid token found in options input statement.");
                                }
                                else if(parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                                    {
                                        parser.NextToken();
                                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                                        {
                                            while (!parser.PeekToken(TokenKind.EndOfFile) && !parser.PeekToken(TokenKind.RightParenthesis))
                                                parser.NextToken();
                                            if (parser.PeekToken(TokenKind.RightParenthesis))
                                                parser.NextToken();
                                            else
                                                parser.ReportSyntaxError("Expected rigt-paren in options input statement.");
                                        }
                                        else
                                            parser.ReportSyntaxError("Expected left-paren in options input statement.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Expected left-paren in options input statement.");
                                }
                                break;
                            }
                        case TokenKind.DisplayKeyword:
                            {
                                if (parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                                    {
                                        while (!parser.PeekToken(TokenKind.EndOfFile) && !parser.PeekToken(TokenKind.RightParenthesis))
                                            parser.NextToken();
                                        if (parser.PeekToken(TokenKind.RightParenthesis))
                                            parser.NextToken();
                                        else
                                            parser.ReportSyntaxError("Expected rigt-paren in options input statement.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Expected left-paren in options input statement.");
                                }
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
                                    VirtualKey vKey;
                                    if(!VirtualKey.TryGetKey(parser, out vKey))
                                        parser.ReportSyntaxError("Invalid virtual key found in options statement.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected keyword \"key\" in options statement.");
                                break;
                            }
                        case TokenKind.OnKeyword:
                            {
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.TerminateKeyword) &&
                                   parser.PeekToken(TokenKind.SignalKeyword, 2) &&
                                   parser.PeekToken(TokenKind.CallKeyword, 3))
                                {
                                    parser.NextToken();
                                    parser.NextToken();
                                    parser.NextToken();
                                    NameExpression funcName;
                                    if (!NameExpression.TryParseNode(parser, out funcName))
                                        parser.ReportSyntaxError("Invalid function name found in options statement.");
                                }
                                else if(parser.PeekToken(TokenKind.CloseKeyword) &&
                                        parser.PeekToken(TokenKind.ApplicationKeyword, 2) &&
                                        parser.PeekToken(TokenKind.CallKeyword, 3))
                                {
                                    parser.NextToken();
                                    parser.NextToken();
                                    parser.NextToken();
                                    NameExpression funcName;
                                    if (!NameExpression.TryParseNode(parser, out funcName))
                                        parser.ReportSyntaxError("Invalid function name found in options statement.");
                                }
                                else
                                    parser.ReportSyntaxError("Invalid token found in options statement.");
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
