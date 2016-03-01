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
    public class LoadStatement : FglStatement
    {
        public ExpressionNode Filename { get; private set; }
        public ExpressionNode DelimiterChar { get; private set; }
        public FglNameExpression TableName { get; private set; }
        public List<FglNameExpression> ColumnNames { get; private set; }
        public ExpressionNode InsertString { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out LoadStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.LoadKeyword))
            {
                result = true;
                node = new LoadStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.ColumnNames = new List<FglNameExpression>();

                if (parser.PeekToken(TokenKind.FromKeyword))
                    parser.NextToken();
                else
                    parser.ReportSyntaxError("Expected \"from\" keyword in load statement.");

                ExpressionNode filenameExpr;
                if (FglExpressionNode.TryGetExpressionNode(parser, out filenameExpr))
                    node.Filename = filenameExpr;
                else
                    parser.ReportSyntaxError("Invalid filename found in load statement.");

                if(parser.PeekToken(TokenKind.DelimiterKeyword))
                {
                    parser.NextToken();
                    if (FglExpressionNode.TryGetExpressionNode(parser, out filenameExpr))
                        node.DelimiterChar = filenameExpr;
                    else
                        parser.ReportSyntaxError("Invalid delimiter character found in load statement.");
                }

                if(parser.PeekToken(TokenKind.InsertKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.IntoKeyword))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expected \"into\" keyword in load statement.");

                    FglNameExpression tableName;
                    if (FglNameExpression.TryParseNode(parser, out tableName))
                        node.TableName = tableName;
                    else
                        parser.ReportSyntaxError("Invalid table name found in load statement.");

                    if(parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        while(FglNameExpression.TryParseNode(parser, out tableName, TokenKind.Comma))
                        {
                            node.ColumnNames.Add(tableName);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected right-paren in load statement.");
                    }
                }
                else
                {
                    if (FglExpressionNode.TryGetExpressionNode(parser, out filenameExpr, Genero4glAst.ValidStatementKeywords.ToList()))
                        node.InsertString = filenameExpr;
                    else
                        parser.ReportSyntaxError("Invalid insert string found in load statement.");
                }
            }

            return result;
        }
    }
}
