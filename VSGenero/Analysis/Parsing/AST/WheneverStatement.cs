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

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// WHENEVER exception-class
    ///     exception-action
    ///     
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Exceptions_006.html
    /// </summary>
    public class WheneverStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out WheneverStatement defNode)
        {
            defNode = null;
            bool result = false;
            
            if(parser.PeekToken(TokenKind.WheneverKeyword))
            {
                result = true;
                defNode = new WheneverStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                // Parse the exception class
                switch (parser.PeekToken().Kind)
                {
                    case TokenKind.AnyKeyword:
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.ErrorKeyword) ||
                               parser.PeekToken(TokenKind.SqlerrorKeyword))
                            {
                                parser.NextToken();
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid token found in whenever statement.");
                            }
                            break;
                        }
                    case TokenKind.NotKeyword:
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.FoundKeyword))
                            {
                                parser.NextToken();
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid token found in whenever statement.");
                            }
                            break;
                        }
                    case TokenKind.ErrorKeyword:
                    case TokenKind.SqlerrorKeyword:
                    case TokenKind.WarningKeyword:
                        {
                            parser.NextToken();
                            break;
                        }
                    default:
                        parser.ReportSyntaxError("Invalid exception class found in whenever statement.");
                        break;
                }

                // Parse the exception-action
                switch(parser.PeekToken().Kind)
                {
                    case TokenKind.ContinueKeyword:
                    case TokenKind.StopKeyword:
                    case TokenKind.RaiseKeyword:
                        parser.NextToken();
                        break;
                    case TokenKind.CallKeyword:
                    case TokenKind.GotoKeyword:
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenCategory.Identifier) ||
                               parser.PeekToken(TokenCategory.Keyword))
                            {
                                parser.NextToken();
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid token found in exception action.");
                            }
                            break;
                        }
                    default:
                        parser.ReportSyntaxError("Invalid exception action found in whenever statement.");
                        break;
                }

                defNode.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
