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

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    /// <summary>
    /// Format:
    /// INITIALIZE target [,...]
    /// {
    ///    TO NULL
    ///     |
    ///    LIKE {table.*|table.column}
    /// }
    /// 
    /// For more info: see http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_INITIALIZE.html
    /// </summary>
    public class InitializeStatement : FglStatement
    {
        public List<NameExpression> TargetVariables { get; private set; }
        public string SourceTable { get; private set; }
        public string SourceColumn { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out InitializeStatement defNode)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.InitializeKeyword))
            {
                result = true;
                defNode = new InitializeStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode.TargetVariables = new List<NameExpression>();

                NameExpression name;
                while(NameExpression.TryParseNode(parser, out name))
                {
                    defNode.TargetVariables.Add(name);
                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }

                if(parser.PeekToken(TokenKind.ToKeyword))
                {
                    parser.NextToken();
                    if(parser.PeekToken(TokenKind.NullKeyword))
                    {
                        parser.NextToken();
                    }
                    else
                    {
                        parser.ReportSyntaxError("Variables can only be initialized to null or a database table spec.");
                    }
                }
                else if(parser.PeekToken(TokenKind.LikeKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenCategory.Identifier))
                    {
                        parser.NextToken();
                        defNode.SourceTable = parser.Token.Token.Value.ToString();
                        parser.NextToken(); // advance to the dot
                        if (parser.Token.Token.Kind == TokenKind.Dot)
                        {
                            if (parser.PeekToken(TokenKind.Multiply) ||
                                parser.PeekToken(TokenCategory.Identifier) ||
                                parser.PeekToken(TokenCategory.Keyword))
                            {
                                parser.NextToken(); // advance to the column name
                                defNode.SourceColumn = parser.Token.Token.Value.ToString();
                                defNode.IsComplete = true;
                                defNode.EndIndex = parser.Token.Span.End;
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid initialization form detected.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Invalid initialization form detected.");
                        }
                    }
                    else
                        parser.ReportSyntaxError("Expected table name in initialization statement.");
                }
                else
                {
                    parser.ReportSyntaxError("Variables can only be initialized to null or a database table spec.");
                }
                defNode.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
