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
    public class StartReportStatement : FglStatement
    {
        public NameExpression ReportName { get; private set; }
        public ExpressionNode ToFilename { get; private set; }
        public ExpressionNode PipeProgram { get; private set; }
        public ExpressionNode XmlHandlerObject { get; private set; }
        public ExpressionNode Destination { get; private set; }
        public ExpressionNode DestTarget { get; private set; }

        public static bool TryParseNode(IParser parser, out StartReportStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.StartKeyword) &&
               parser.PeekToken(TokenKind.ReportKeyword, 2))
            {
                result = true;
                node = new StartReportStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                parser.NextToken();

                NameExpression rptName;
                if (NameExpression.TryParseNode(parser, out rptName))
                    node.ReportName = rptName;
                else
                    parser.ReportSyntaxError("Invalid report name found in start report driver.");

                if(parser.PeekToken(TokenKind.ToKeyword))
                {
                    parser.NextToken();
                    switch(parser.PeekToken().Kind)
                    {
                        case TokenKind.ScreenKeyword:
                        case TokenKind.PrinterKeyword:
                            parser.NextToken();
                            break;
                        case TokenKind.FileKeyword:
                            parser.NextToken();
                            ExpressionNode fileName;
                            if (ExpressionNode.TryGetExpressionNode(parser, out fileName))
                                node.ToFilename = fileName;
                            else
                                parser.ReportSyntaxError("Invalid filename found in start report driver.");
                            break;
                        case TokenKind.PipeKeyword:
                            parser.NextToken();
                            
                            ExpressionNode prog;
                            if (ExpressionNode.TryGetExpressionNode(parser, out prog))
                                node.PipeProgram = prog;
                            else
                                parser.ReportSyntaxError("Invalid program name found in start report driver.");

                            if(parser.PeekToken(TokenKind.InKeyword))
                            {
                                parser.NextToken();
                                if ((parser.PeekToken(TokenKind.FormKeyword) || parser.PeekToken(TokenKind.LineKeyword))
                                    && parser.PeekToken(TokenKind.ModeKeyword, 2))
                                {
                                    parser.NextToken();
                                    parser.NextToken();
                                }
                                else
                                    parser.ReportSyntaxError("Invalid token found in start report driver.");
                            }
                            break;
                        case TokenKind.XmlKeyword:
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.HandlerKeyword))
                            {
                                parser.NextToken();
                                ExpressionNode objHandler;
                                if (ExpressionNode.TryGetExpressionNode(parser, out objHandler))
                                    node.XmlHandlerObject = objHandler;
                                else
                                    parser.ReportSyntaxError("Invalid xml handler object found in start report driver.");
                            }
                            else
                                parser.ReportSyntaxError("Expected \"handler\" keyword in start report driver.");
                            break;
                        case TokenKind.OutputKeyword:
                            parser.NextToken();
                            ExpressionNode destExpr;
                            if (ExpressionNode.TryGetExpressionNode(parser, out destExpr))
                                node.Destination = destExpr;
                            else
                                parser.ReportSyntaxError("Invalid destination expression found in start report driver.");

                            if(parser.PeekToken(TokenKind.DestinationKeyword))
                            {
                                parser.NextToken();
                                if (ExpressionNode.TryGetExpressionNode(parser, out destExpr))
                                    node.DestTarget = destExpr;
                                else
                                    parser.ReportSyntaxError("Invalid destination target found in start report driver.");
                            }
                            break;
                        default:
                            if (ExpressionNode.TryGetExpressionNode(parser, out fileName))
                                node.ToFilename = fileName;
                            else
                                parser.ReportSyntaxError("Invalid filename found in start report driver.");
                            break;
                    }
                }

                if(parser.PeekToken(TokenKind.WithKeyword))
                {
                    parser.NextToken();
                    bool isStillValid = true;
                    ExpressionNode dimensionExpr;
                    var comma = new List<TokenKind> { TokenKind.Comma };
                    while (isStillValid)
                    {
                        switch(parser.PeekToken().Kind)
                        {
                            case TokenKind.TopKeyword:
                                parser.NextToken();
                                if(parser.PeekToken(TokenKind.MarginKeyword))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.Equals))
                                    {
                                        parser.NextToken();
                                        // TODO: need to store the dimension expressions...
                                        if (!ExpressionNode.TryGetExpressionNode(parser, out dimensionExpr, comma))
                                            parser.ReportSyntaxError("Invalid dimension expression found in start report driver.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Expected '=' token in start report driver.");
                                }
                                else if (parser.PeekToken(TokenKind.OfKeyword) && parser.PeekToken(TokenKind.PageKeyword, 2))
                                {
                                    parser.NextToken();
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.Equals))
                                    {
                                        parser.NextToken();
                                        // TODO: need to store the dimension expressions...
                                        if (!ExpressionNode.TryGetExpressionNode(parser, out dimensionExpr, comma))
                                            parser.ReportSyntaxError("Invalid dimension expression found in start report driver.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Expected '=' token in start report driver.");
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Unexpected token found in start report driver.");
                                }
                                break;
                            case TokenKind.LeftKeyword:
                            case TokenKind.RightKeyword:
                            case TokenKind.BottomKeyword:
                                parser.NextToken();
                                if(parser.PeekToken(TokenKind.MarginKeyword))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.Equals))
                                    {
                                        parser.NextToken();
                                        // TODO: need to store the dimension expressions...
                                        if (!ExpressionNode.TryGetExpressionNode(parser, out dimensionExpr, comma))
                                            parser.ReportSyntaxError("Invalid dimension expression found in start report driver.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Expected '=' token in start report driver.");
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Expected \"margin\" keyword in start report driver.");
                                }
                                break;
                            case TokenKind.PageKeyword:
                                parser.NextToken();
                                if(parser.PeekToken(TokenKind.LengthKeyword))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.Equals))
                                    {
                                        parser.NextToken();
                                        // TODO: need to store the dimension expressions...
                                        if (!ExpressionNode.TryGetExpressionNode(parser, out dimensionExpr, comma))
                                            parser.ReportSyntaxError("Invalid dimension expression found in start report driver.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Expected '=' token in start report driver.");
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Expected \"length\" keyword in start report driver.");
                                }
                                break;
                        }

                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                        else
                            isStillValid = false;
                    }

                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }
    }
}
