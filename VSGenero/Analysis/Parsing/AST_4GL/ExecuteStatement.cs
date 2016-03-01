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
    public class ExecuteStatement : FglStatement
    {
        public ExpressionNode ImmediateExpression { get; private set; }
        public FglNameExpression PreparedStatementId { get; private set; }
        public List<ExpressionNode> InputVars { get; private set; }
        public List<FglNameExpression> OutputVars { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out ExecuteStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.ExecuteKeyword))
            {
                result = true;
                node = new ExecuteStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.InputVars = new List<ExpressionNode>();
                node.OutputVars = new List<FglNameExpression>();

                if(parser.PeekToken(TokenKind.ImmediateKeyword))
                {
                    parser.NextToken();
                    ExpressionNode immExpr;
                    if (FglExpressionNode.TryGetExpressionNode(parser, out immExpr, Genero4glAst.ValidStatementKeywords.ToList()))
                        node.ImmediateExpression = immExpr;
                    else
                        parser.ReportSyntaxError("Invalid expression found in execute immediate statement.");
                }
                else
                {
                    FglNameExpression cid;
                    if (FglNameExpression.TryParseNode(parser, out cid))
                        node.PreparedStatementId = cid;
                    else
                        parser.ReportSyntaxError("Invalid prepared statement id found in execute statement.");

                    HashSet<TokenKind> inVarMods = new HashSet<TokenKind> { TokenKind.InKeyword, TokenKind.OutKeyword, TokenKind.InOutKeyword };
                    bool hitUsing = false;
                    if(parser.PeekToken(TokenKind.UsingKeyword))
                    {
                        hitUsing = true;
                        parser.NextToken();
                        ExpressionNode inVar;
                        while (FglExpressionNode.TryGetExpressionNode(parser, out inVar))
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

                    if(parser.PeekToken(TokenKind.IntoKeyword))
                    {
                        parser.NextToken();
                        FglNameExpression outVar;
                        while (FglNameExpression.TryParseNode(parser, out outVar, TokenKind.Comma))
                        {
                            node.InputVars.Add(outVar);
                            if (parser.PeekToken(TokenKind.Comma))
                                parser.NextToken();
                            else
                                break;
                        }
                    }

                    if (!hitUsing && parser.PeekToken(TokenKind.UsingKeyword))
                    {
                        parser.NextToken();
                        ExpressionNode inVar;
                        while (FglExpressionNode.TryGetExpressionNode(parser, out inVar))
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
                }
                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
