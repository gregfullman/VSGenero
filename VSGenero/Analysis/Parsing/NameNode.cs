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

namespace VSGenero.Analysis.Parsing
{
    public class NameExpression : ExpressionNode
    {
        public string Name { get; protected set; }
        protected string _firstPiece;

        public static bool TryParseNode(IParser parser, out NameExpression node, TokenKind breakToken = TokenKind.EndOfFile)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
            {
                result = true;
                node = new NameExpression();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                node._firstPiece = parser.Token.Token.Value.ToString();
                StringBuilder sb = new StringBuilder(node._firstPiece);
                node.EndIndex = parser.Token.Span.End;
                while(true)
                {
                    if(breakToken != TokenKind.EndOfFile &&
                       parser.PeekToken(breakToken))
                    {
                        break;
                    }

                    MemberAccessNameExpressionPiece memberAccess;
                    if(MemberAccessNameExpressionPiece.TryParse(parser, out memberAccess) && memberAccess != null)
                    {
                        node.Children.Add(memberAccess.StartIndex, memberAccess);
                        node.EndIndex = memberAccess.EndIndex;
                        node.IsComplete = true;
                        sb.Append(memberAccess.ToString());
                    }
                    else if(parser.PeekToken(TokenKind.Ampersand))
                    {
                        parser.NextToken();
                        sb.Append("@");
                        if(parser.PeekToken(TokenCategory.Identifier) ||
                           parser.PeekToken(TokenCategory.Keyword))
                        {
                            sb.Append(parser.NextToken().Value.ToString());
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                node.Name = sb.ToString();
            }

            return result;
        }

        protected override string GetStringForm()
        {
            return Name;
        }

        public override string GetExpressionType(GeneroAst ast)
        {
            // need to determine the type from the variables available
            IGeneroProject dummyProj;
            IProjectEntry dummyProjEntry;
            bool dummy;
            IAnalysisResult res = ast.GetValueByIndex(Name,
                                                      StartIndex,
                                                      ast._functionProvider,
                                                      ast._databaseProvider,
                                                      ast._programFileProvider,
                                                      false,
                                                      out dummy,
                                                      out dummyProj,
                                                      out dummyProjEntry);
            if (res != null)
            {
                return res.Typename;
            }
            return null;
        }

        public IAnalysisResult ResolvedResult { get; protected set; }
    }

    public class MemberAccessNameExpressionPiece : AstNode
    {
        public string Text { get; private set; }

        public override string ToString()
        {
            return Text;
        }

        public static bool TryParse(IParser parser, out MemberAccessNameExpressionPiece node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.Dot))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(".");
                result = true;
                node = new MemberAccessNameExpressionPiece();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if(parser.PeekToken(TokenKind.Multiply) || parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                {
                    sb.Append(parser.NextToken().Value.ToString());
                    node.Text = sb.ToString();
                    node.EndIndex = parser.Token.Span.End;
                    node.IsComplete = true;
                }
                else
                {
                    parser.ReportSyntaxError("Invalid token found in member access.");
                }
            }

            return result;
        }
    }
}
