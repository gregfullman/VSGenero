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
    public enum LocateLocation
    {
        Memory,
        File
    }

    /// <summary>
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_LOCATE.html
    /// </summary>
    public class LocateStatement : FglStatement
    {
        public List<FglNameExpression> TargetVariables { get; private set; }
        public LocateLocation Location { get; private set; }
        public ExpressionNode Filename { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out LocateStatement defNode)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.LocateKeyword))
            {
                result = true;
                defNode = new LocateStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode.TargetVariables = new List<FglNameExpression>();

                FglNameExpression name;
                while (FglNameExpression.TryParseNode(parser, out name))
                {
                    defNode.TargetVariables.Add(name);
                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }

                if(parser.PeekToken(TokenKind.InKeyword))
                {
                    parser.NextToken();
                    if(parser.PeekToken(TokenKind.MemoryKeyword))
                    {
                        parser.NextToken();
                        defNode.Location = LocateLocation.Memory;
                    }
                    else if(parser.PeekToken(TokenKind.FileKeyword))
                    {
                        parser.NextToken();
                        defNode.Location = LocateLocation.File;

                        ExpressionNode filename;
                        if(FglExpressionNode.TryGetExpressionNode(parser, out filename, Genero4glAst.ValidStatementKeywords.ToList()))
                        {
                            defNode.Filename = filename;
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Locate statement can only specify memory or a file.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Locate statement missing \"in\" keyword.");
                }

                defNode.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
