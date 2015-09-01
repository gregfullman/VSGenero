﻿/* ****************************************************************************
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
    public class NameExpression : ExpressionNode
    {
        public string Name { get; private set; }
        private string _firstPiece;

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
                    ArrayIndexNameExpressionPiece arrayIndex;
                    if(MemberAccessNameExpressionPiece.TryParse(parser, out memberAccess) && memberAccess != null)
                    {
                        node.Children.Add(memberAccess.StartIndex, memberAccess);
                        node.EndIndex = memberAccess.EndIndex;
                        node.IsComplete = true;
                        sb.Append(memberAccess.ToString());
                    }
                    else if(ArrayIndexNameExpressionPiece.TryParse(parser, out arrayIndex, breakToken) && arrayIndex != null)
                    {
                        node.Children.Add(arrayIndex.StartIndex, arrayIndex);
                        node.EndIndex = arrayIndex.EndIndex;
                        node.IsComplete = true;
                        sb.Append(arrayIndex.ToString());
                    }
                    else
                    {
                        //parser.ReportSyntaxError("Invalid token detected in name expression.");
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

        public override string GetExpressionType(Genero4glAst ast)
        {
            // need to determine the type from the variables available
            IGeneroProject dummyProj;
            IProjectEntry dummyProjEntry;
            bool dummy;
            IAnalysisResult res = Genero4glAst.GetValueByIndex(Name, StartIndex, ast, out dummyProj, out dummyProjEntry, out dummy);
            if (res != null)
            {
                return res.Typename;
            }
            return null;
        }

        public IAnalysisResult ResolvedResult { get; private set; }

        public override void CheckForErrors(Genero4glAst ast, Action<string, int, int> errorFunc,
                                            Dictionary<string, List<int>> deferredFunctionSearches,
                                            Genero4glAst.FunctionProviderSearchMode searchInFunctionProvider = Genero4glAst.FunctionProviderSearchMode.NoSearch, 
                                            bool isFunctionCallOrDefinition = false)
        {
            // Check to see if the _firstPiece exists
            IGeneroProject proj;
            IProjectEntry projEntry;
            string searchStr = _firstPiece;
            if (searchInFunctionProvider != Genero4glAst.FunctionProviderSearchMode.NoSearch ||
                isFunctionCallOrDefinition)
            {
                StringBuilder sb = new StringBuilder(searchStr);
                foreach(var child in Children.Values)
                {
                    if (child is ArrayIndexNameExpressionPiece)
                        sb.Append(child.ToString());
                    else if(child is MemberAccessNameExpressionPiece)
                    {
                        if ((child as MemberAccessNameExpressionPiece).Text != ".*")
                            sb.Append(child.ToString());
                        else
                            break;
                    }
                }
                searchStr = sb.ToString();
            }
            bool isDeferred;
            // TODO: need to defer database lookups too
            var res = Genero4glAst.GetValueByIndex(searchStr, StartIndex, ast, out proj, out projEntry, out isDeferred, searchInFunctionProvider, isFunctionCallOrDefinition);
            if(res == null)
            {
                if (isDeferred)
                {
                    if (deferredFunctionSearches.ContainsKey(searchStr))
                        deferredFunctionSearches[searchStr].Add(StartIndex);
                    else
                        deferredFunctionSearches.Add(searchStr, new List<int> { StartIndex });
                }
                else
                {
                    errorFunc(string.Format("No definition found for {0}", searchStr), StartIndex, StartIndex + searchStr.Length);
                }
            }
            else
            {
                if(Name.EndsWith(".*") && res is VariableDef && (res as VariableDef).ResolvedType == null)
                {
                    // need to make sure that the res has a resolved type
                    (res as VariableDef).Type.CheckForErrors(ast, errorFunc, deferredFunctionSearches);
                    (res as VariableDef).ResolvedType = (res as VariableDef).Type.ResolvedType;
                }
                // TODO: need to check array element type
                ResolvedResult = res;
            }

            base.CheckForErrors(ast, errorFunc, deferredFunctionSearches, searchInFunctionProvider, isFunctionCallOrDefinition);
        }
    }

    public class ArrayIndexNameExpressionPiece : AstNode4gl
    {
        private ExpressionNode _expression;

        public override string ToString()
        {
            if (_expression == null)
                return "[]";
            return string.Format("[{0}]", _expression.ToString());
        }

        public static bool TryParse(IParser parser, out ArrayIndexNameExpressionPiece node, TokenKind breakToken = TokenKind.EndOfFile)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.LeftBracket))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                result = true;
                node = new ArrayIndexNameExpressionPiece();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // TODO: need to get an integer expression
                // for right now, we'll just check for a constant or a ident/keyword
                ExpressionNode indexExpr;
                while (ExpressionNode.TryGetExpressionNode(parser, out indexExpr, new List<TokenKind> { TokenKind.RightBracket, TokenKind.Comma }))
                {
                    if (node._expression == null)
                        node._expression = indexExpr;
                    else
                        node._expression.AppendExpression(indexExpr);

                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();
                    else
                        break;
                }

                //if(parser.PeekToken(TokenCategory.NumericLiteral) ||
                //   parser.PeekToken(TokenCategory.Keyword) ||
                //   parser.PeekToken(TokenCategory.Identifier))
                //{
                //    parser.NextToken();
                //    sb.Append(parser.Token.Token.Value.ToString());
                //}
                //else
                //{
                //    parser.ReportSyntaxError("The parser is unable to parse a complex expression as an array index. This may not be a syntax error.");
                //}

                //// TODO: check for a nested array index access
                //ArrayIndexNameExpressionPiece arrayIndex;
                //if (ArrayIndexNameExpressionPiece.TryParse(parser, out arrayIndex, breakToken))
                //{
                //    sb.Append(arrayIndex._expression);
                //}

                //while(!parser.PeekToken(TokenKind.RightBracket))
                //{
                //    if(parser.PeekToken().Kind == breakToken)
                //    {
                //        parser.ReportSyntaxError("Unexpected end of array index expression.");
                //        break;
                //    }
                //    parser.NextToken();
                //    sb.Append(parser.Token.Token.Value.ToString());
                //}

                if (parser.PeekToken(TokenKind.RightBracket))
                {
                    parser.NextToken();
                    node.EndIndex = parser.Token.Span.End;
                    node.IsComplete = true;
                }
                else
                    parser.ReportSyntaxError("Expected right-bracket in array index.");
            }

            return result;
        }
    }

    public class MemberAccessNameExpressionPiece : AstNode4gl
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