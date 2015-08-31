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
    /// C-Extension:
    /// IMPORT filename 
    /// 
    /// FGL:
    /// IMPORT FGL modulename
    /// 
    /// Java:
    /// IMPORT JAVA classname 
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_IMPORT.html
    /// </summary>
    public class ImportModuleNode : AstNode4gl
    {
        public ImportModuleType ImportType { get; private set; }
        public string ImportName { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out ImportModuleNode defNode)
        {
            defNode = null;
            bool result = false;
            
            if(parser.PeekToken(TokenKind.ImportKeyword))
            {
                parser.NextToken();
                defNode = new ImportModuleNode();
                defNode.StartIndex = parser.Token.Span.Start;
                
                if(parser.PeekToken(TokenKind.FglKeyword))
                {
                    defNode.ImportType = ImportModuleType.FGL;
                    parser.NextToken();
                    if(parser.PeekToken(TokenCategory.Identifier))
                    {
                        parser.NextToken();
                        defNode.ImportName = parser.Token.Token.Value.ToString();
                        defNode.EndIndex = parser.Token.Span.End;
                        defNode.IsComplete = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "An FGL import must specify an identifier");
                    }
                }
                else if(parser.PeekToken(TokenKind.JavaKeyword))
                {
                    defNode.ImportType = ImportModuleType.Java;
                    StringBuilder sb = new StringBuilder();
                    parser.NextToken();
                    parser.NextToken();
                    var tokenInfo = Tokenizer.GetTokenInfo(parser.Token.Token);
                    if(tokenInfo.Category == TokenCategory.Keyword || tokenInfo.Category == TokenCategory.Identifier)
                    {
                        sb.Append(parser.Token.Token.Value);
                        bool valid = true;
                        while(parser.MaybeEat(TokenKind.Dot))
                        {
                            sb.Append('.');
                            parser.NextToken();
                            tokenInfo = Tokenizer.GetTokenInfo(parser.Token.Token);
                            if (tokenInfo.Category != TokenCategory.Keyword && tokenInfo.Category != TokenCategory.Identifier)
                            {
                                valid = false;
                                break;
                            }
                            sb.Append(parser.Token.Token.Value);
                        }

                        if (valid)
                        {
                            defNode.ImportName = sb.ToString();
                            defNode.EndIndex = parser.Token.Span.End;
                            defNode.IsComplete = true;
                        }
                    }
                }
                else
                {
                    defNode.ImportType = ImportModuleType.C;
                    if(parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                    {
                        parser.NextToken();
                        defNode.ImportName = parser.Token.Token.Value.ToString();
                        defNode.EndIndex = parser.Token.Span.End;
                        defNode.IsComplete = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "A filename must be specified to import a C-Extension");
                    }
                }
                result = true;
            }

            return result;
        }
    }
}
