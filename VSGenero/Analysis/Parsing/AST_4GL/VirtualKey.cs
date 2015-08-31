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
    public class VirtualKey : AstNode4gl
    {
        private static char[] _controlKeyExlusions = new char[]
        {
            'A', 'D', 'H', 'I', 'J', 'K', 'L', 'M', 'R', 'X'
        };

        public static bool TryGetKey(IParser parser, out VirtualKey key)
        {
            key = new VirtualKey();
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
                        // do letter
                        if (parser.PeekToken(TokenCategory.Identifier))
                        {
                            parser.NextToken();
                            var name = parser.Token.Token.Value.ToString().ToUpper();
                            if (name.Length != 1 ||
                                ((int)name[0] < 65 || (int)name[0] > 90) &&
                                _controlKeyExlusions.Contains(name[0]))
                            {
                                parser.ReportSyntaxError("Invalid virtual key specified.");
                            }
                        }
                    }
                    else
                        parser.ReportSyntaxError("Expected '-' token in virtual key.");
                    break;
                default:
                    {
                        // do F1 - F255
                        parser.NextToken();
                        var name = parser.Token.Token.Value.ToString();
                        if (name.StartsWith("F", StringComparison.OrdinalIgnoreCase))
                        {
                            var numberStr = name.Substring(1);
                            int number;
                            if (!int.TryParse(numberStr, out number) || number < 1 || number > 255)
                            {
                                parser.ReportSyntaxError("Invalid virtual key name found.");
                            }
                        }
                        else if(Tokenizer.GetTokenInfo(parser.Token.Token).Category == TokenCategory.StringLiteral)
                        {
                            // TODO: do we want to verify the key string?
                        }
                        else
                            return false;
                        break;
                    }
            }
            return true;
        }
    }
}
