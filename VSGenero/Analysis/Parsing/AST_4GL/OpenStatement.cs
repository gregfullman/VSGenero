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
    public class OpenStatement : FglStatement
    {
        public NameExpression CursorId { get; private set; }
        public List<ExpressionNode> InputVars { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out OpenStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.OpenKeyword))
            {
                result = true;
                node = new OpenStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.InputVars = new List<ExpressionNode>();

                NameExpression cid;
                if (NameExpression.TryParseNode(parser, out cid))
                    node.CursorId = cid;
                else
                    parser.ReportSyntaxError("Invalid declared cursor id found in open statement.");

                HashSet<TokenKind> inVarMods = new HashSet<TokenKind> { TokenKind.InKeyword, TokenKind.OutKeyword, TokenKind.InOutKeyword };
                if (parser.PeekToken(TokenKind.UsingKeyword))
                {
                    parser.NextToken();
                    ExpressionNode inVar;
                    while (ExpressionNode.TryGetExpressionNode(parser, out inVar))
                    {
                        node.InputVars.Add(inVar);
                        if (inVarMods.Contains(parser.PeekToken().Kind))
                            parser.NextToken();
                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                        else
                            break;
                    }
                }

                if(parser.PeekToken(TokenKind.WithKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.ReoptimizationKeyword))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expecting keyword \"reoptimization\" in open statement.");
                }
                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
