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

namespace VSGenero.Analysis.Parsing.AST
{
    public class PromptStatement : FglStatement
    {
        public ExpressionNode Question { get; private set; }
        public List<PromptDisplayAttribute> DisplayAttributes { get; private set; }
        public List<PromptControlAttribute> ControlAttributes { get; private set; }
        public NameExpression VariableName { get; private set; }
        public ExpressionNode HelpNumber { get; private set; }

        public static bool TryParseNode(Parser parser, out PromptStatement node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.PromptKeyword))
            {
                result = true;
                node = new PromptStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.DisplayAttributes = new List<PromptDisplayAttribute>();
                node.ControlAttributes = new List<PromptControlAttribute>();

                ExpressionNode question;
                if (ExpressionNode.TryGetExpressionNode(parser, out question))
                    node.Question = question;
                else
                    parser.ReportSyntaxError("Invalid question expression found in prompt statement.");

                // get the optional display attributes
                if (parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();
                        PromptDisplayAttribute attrib;
                        while (PromptDisplayAttribute.TryParseNode(parser, out attrib))
                        {
                            node.DisplayAttributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in prompt display attributes section.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting left-paren in prompt display attributes section.");
                }

                if (parser.PeekToken(TokenKind.ForKeyword))
                    parser.NextToken();
                else
                    parser.ReportSyntaxError("Expected \"for\" keyword in prompt statement.");

                if (parser.PeekToken(TokenKind.CharKeyword) ||
                   parser.PeekToken(TokenKind.CharacterKeyword))
                {
                    parser.NextToken();
                }

                NameExpression varName;
                if (NameExpression.TryParseNode(parser, out varName))
                    node.VariableName = varName;
                else
                    parser.ReportSyntaxError("Invalid variable name found in prompt statement.");

                if (parser.PeekToken(TokenKind.HelpKeyword))
                {
                    parser.NextToken();
                    ExpressionNode helpNumber;
                    if (ExpressionNode.TryGetExpressionNode(parser, out helpNumber))
                        node.HelpNumber = helpNumber;
                    else
                        parser.ReportSyntaxError("Invalid help number found in prompt statement.");
                }

                // get the optional control attributes
                if (parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();
                        PromptControlAttribute attrib;
                        while (PromptControlAttribute.TryParseNode(parser, out attrib))
                        {
                            node.ControlAttributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in prompt control attributes section.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting left-paren in prompt control attributes section.");
                }

                List<TokenKind> validExits = new List<TokenKind>();
                if (validExitKeywords != null)
                    validExits.AddRange(validExitKeywords);
                validExits.Add(TokenKind.PromptKeyword);

                HashSet<TokenKind> newEndKeywords = new HashSet<TokenKind>();
                if (endKeywords != null)
                    newEndKeywords.AddRange(endKeywords);
                newEndKeywords.Add(TokenKind.PromptKeyword);

                bool hasControlBlocks = false;
                PromptControlBlock controlBlock;
                while(PromptControlBlock.TryParseNode(parser, out controlBlock, containingModule, prepStatementBinder, returnStatementBinder, 
                                                      limitedScopeVariableAdder, validExitKeywords, contextStatementFactories) && controlBlock != null)
                {
                    node.Children.Add(controlBlock.StartIndex, controlBlock);
                    hasControlBlocks = true;
                    if(parser.PeekToken(TokenKind.EndOfFile) ||
                       (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.PromptKeyword, 2)))
                    {
                        break;
                    }
                }

                if(hasControlBlocks)
                {
                    if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.PromptKeyword, 2)))
                    {
                        parser.ReportSyntaxError("A prompt block must be terminated with \"end prompt\".");
                    }
                    else
                    {
                        parser.NextToken(); // advance to the 'end' token
                        parser.NextToken(); // advance to the 'prompt' token
                        node.EndIndex = parser.Token.Span.End;
                    }
                }
            }

            return result;
        }
    }

    public class PromptDisplayAttribute : AstNode
    {
        public static bool TryParseNode(Parser parser, out PromptDisplayAttribute node)
        {
            node = new PromptDisplayAttribute();
            node.StartIndex = parser.Token.Span.Start;
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.BlackKeyword:
                case TokenKind.BlueKeyword:
                case TokenKind.CyanKeyword:
                case TokenKind.GreenKeyword:
                case TokenKind.MagentaKeyword:
                case TokenKind.RedKeyword:
                case TokenKind.WhiteKeyword:
                case TokenKind.YellowKeyword:
                case TokenKind.BoldKeyword:
                case TokenKind.DimKeyword:
                case TokenKind.InvisibleKeyword:
                case TokenKind.NormalKeyword:
                case TokenKind.ReverseKeyword:
                case TokenKind.BlinkKeyword:
                case TokenKind.UnderlineKeyword:
                    parser.NextToken();
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }
    }

    public class PromptControlAttribute : AstNode
    {
        public static bool TryParseNode(Parser parser, out PromptControlAttribute node)
        {
            node = new PromptControlAttribute();
            node.StartIndex = parser.Token.Span.Start;
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.AcceptKeyword:
                case TokenKind.CancelKeyword:
                case TokenKind.UnbufferedKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            ExpressionNode boolExpr;
                            if (!ExpressionNode.TryGetExpressionNode(parser, out boolExpr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                                parser.ReportSyntaxError("Invalid boolean expression found in prompt attribute.");
                        }
                        break;
                    }
                case TokenKind.WithoutKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.DefaultsKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.Equals))
                            {
                                parser.NextToken();
                                ExpressionNode boolExpr;
                                if (!ExpressionNode.TryGetExpressionNode(parser, out boolExpr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                                    parser.ReportSyntaxError("Invalid boolean expression found in prompt attribute.");
                            }
                        }
                        else
                            parser.ReportSyntaxError("Expected \"defaults\" keyword in prompt attribute.");
                        break;
                    }
                case TokenKind.HelpKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected equals token in input attribute.");

                        // get the help number
                        ExpressionNode optionNumber;
                        if (!ExpressionNode.TryGetExpressionNode(parser, out optionNumber))
                            parser.ReportSyntaxError("Invalid help-number found in input attribute.");
                        break;
                    }
                case TokenKind.CountKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            ExpressionNode boolExpr;
                            if (!ExpressionNode.TryGetExpressionNode(parser, out boolExpr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                                parser.ReportSyntaxError("Invalid expression found in input attribute.");
                        }
                        else
                            parser.ReportSyntaxError("Expected integer expression in input array attribute.");
                        break;
                    }
                case TokenKind.CenturyKeyword:
                case TokenKind.FormatKeyword:
                case TokenKind.PictureKeyword:
                case TokenKind.ShiftKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected equals token in input attribute.");
                        ExpressionNode strExpr;
                        if (!ExpressionNode.TryGetExpressionNode(parser, out strExpr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                            parser.ReportSyntaxError("Invalid string expression found in prompt attribute.");
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            return result;
        }
    }

    public enum PromptControlBlockType
    {
        None,
        Idle,
        Action,
        Key
    }

    public class PromptControlBlock : AstNode
    {
        public ExpressionNode IdleSeconds { get; private set; }
        public NameExpression ActionName { get; private set; }
        public List<VirtualKey> KeyNameList { get; private set; }
        public PromptControlBlockType Type { get; private set; }

        public static bool TryParseNode(Parser parser, out PromptControlBlock node, IModuleResult containingModule,
                                Action<PrepareStatement> prepStatementBinder = null,
                                Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = new PromptControlBlock();
            bool result = true;
            node.KeyNameList = new List<VirtualKey>();
            
            switch(parser.PeekToken().Kind)
            {
                case TokenKind.OnKeyword:
                    {
                        parser.NextToken();
                        node.StartIndex = parser.Token.Span.Start;
                        switch (parser.PeekToken().Kind)
                        {
                            case TokenKind.IdleKeyword:
                                parser.NextToken();
                                node.Type = PromptControlBlockType.Idle;
                                // get the idle seconds
                                ExpressionNode idleExpr;
                                if (ExpressionNode.TryGetExpressionNode(parser, out idleExpr))
                                    node.IdleSeconds = idleExpr;
                                else
                                    parser.ReportSyntaxError("Invalid idle-seconds found in prompt statement.");
                                break;
                            case TokenKind.ActionKeyword:
                                parser.NextToken();
                                node.Type = PromptControlBlockType.Action;
                                // get the action name
                                NameExpression actionName;
                                if (NameExpression.TryParseNode(parser, out actionName))
                                    node.ActionName = actionName;
                                else
                                    parser.ReportSyntaxError("Invalid action-name found in prompt statement.");
                                break;
                            case TokenKind.KeyKeyword:
                                parser.NextToken();
                                node.Type = PromptControlBlockType.Key;
                                if (parser.PeekToken(TokenKind.LeftParenthesis))
                                {
                                    parser.NextToken();
                                    // get the list of key names
                                    VirtualKey keyName;
                                    while (VirtualKey.TryGetKey(parser, out keyName))
                                    {
                                        node.KeyNameList.Add(keyName);
                                        if (!parser.PeekToken(TokenKind.Comma))
                                            break;
                                        parser.NextToken();
                                    }
                                    if (parser.PeekToken(TokenKind.RightParenthesis))
                                        parser.NextToken();
                                    else
                                        parser.ReportSyntaxError("Expected right-paren in prompt block.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected left-paren in prompt block.");
                                break;
                            default:
                                parser.ReportSyntaxError("Unexpected token found in prompt block.");
                                result = false;
                                break;
                        }
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            if(result)
            {
                // get the dialog statements
                FglStatement dispStmt;
                while (parser.StatementFactory.TryParseNode(parser, out dispStmt, containingModule, prepStatementBinder, returnStatementBinder, 
                                                            limitedScopeVariableAdder, false, validExitKeywords, contextStatementFactories, null, endKeywords) && dispStmt != null)
                {
                    node.Children.Add(dispStmt.StartIndex, dispStmt);
                    node.EndIndex = dispStmt.EndIndex;
                }

                if (node.Type == PromptControlBlockType.None && node.Children.Count == 0)
                    result = false;
            }
            return result;
        }
    }
}
