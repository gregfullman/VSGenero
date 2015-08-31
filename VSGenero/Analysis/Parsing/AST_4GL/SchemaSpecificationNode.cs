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
    /// SCHEMA dbname
    /// 
    /// or
    /// 
    /// [DESCRIBE] DATABASE dbname
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_DatabaseSchema_SCHEMA.html
    /// </summary>
    public class SchemaSpecificationNode : AstNode4gl
    {
        public string SchemaName { get; private set; }

        public static bool TryParseDefine(Genero4glParser parser, out SchemaSpecificationNode defNode)
        {
            defNode = null;
            bool result = false;
            
            if(parser.PeekToken(TokenKind.SchemaKeyword))
            {
                defNode = new SchemaSpecificationNode();
                defNode.StartIndex = parser.Token.Span.Start;
                parser.NextToken();
                result = true;

                parser.NextToken();
                var tokenInfo = Tokenizer.GetTokenInfo(parser.Token.Token);
                if (tokenInfo.Category == TokenCategory.Keyword || 
                    tokenInfo.Category == TokenCategory.Identifier ||
                    tokenInfo.Category == TokenCategory.StringLiteral)
                {
                    defNode.SchemaName = parser.Token.Token.Value.ToString();
                    defNode.EndIndex = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError(defNode.StartIndex, parser.Token.Span.End, "Invalid token type found.");
                }
            }
            else
            {
                if(parser.PeekToken(TokenKind.DescribeKeyword))
                {
                    defNode = new SchemaSpecificationNode();
                    defNode.StartIndex = parser.Token.Span.Start;
                    parser.NextToken();
                    result = true;
                }

                if(parser.PeekToken(TokenKind.DatabaseKeyword))
                {
                    if(defNode == null)
                    {
                        defNode = new SchemaSpecificationNode();
                        defNode.StartIndex = parser.Token.Span.Start;
                        parser.NextToken();
                        result = true;
                    }

                    parser.NextToken();
                    parser.NextToken();
                    var tokenInfo = Tokenizer.GetTokenInfo(parser.Token.Token);
                    if (tokenInfo.Category == TokenCategory.Keyword || tokenInfo.Category == TokenCategory.Identifier)
                    {
                        defNode.SchemaName = parser.Token.Token.Value.ToString();
                        defNode.EndIndex = parser.Token.Span.End;
                        defNode.IsComplete = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError(defNode.StartIndex, parser.Token.Span.End, "Invalid token type found.");
                    }
                }
                else if(defNode != null)
                {
                    parser.ReportSyntaxError(defNode.StartIndex, parser.Token.Span.End, "Legacy schema specification is incomplete.");
                }
            }

            return result;
        }
    }
}
