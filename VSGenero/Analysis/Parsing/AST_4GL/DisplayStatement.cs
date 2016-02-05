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
    public class DisplayBlock : FglStatement
    {
        public bool IsArray { get; private set; }
        public NameExpression ArrayName { get; private set; }
        public NameExpression ScreenArrayName { get; private set; }
        public ExpressionNode HelpNumber { get; private set; }

        public ExpressionNode Expression { get; private set; }
        public List<NameExpression> FieldSpecs { get; private set; }
        public List<DisplayAttribute> Attributes { get; private set; }
        public List<NameExpression> ByNameFields { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out DisplayBlock node,
                                 IModuleResult containingModule,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.DisplayKeyword))
            {
                result = true;
                node = new DisplayBlock();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.Attributes = new List<DisplayAttribute>();
                node.ByNameFields = new List<NameExpression>();
                node.FieldSpecs = new List<NameExpression>();
                node.DecoratorEnd = parser.Token.Span.End;

                if (parser.PeekToken(TokenKind.ByKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.NameKeyword))
                    {
                        parser.NextToken();

                        // get the bynamefields
                        NameExpression nameExpr;
                        while (NameExpression.TryParseNode(parser, out nameExpr))
                        {
                            node.ByNameFields.Add(nameExpr);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        node.DecoratorEnd = parser.Token.Span.End;

                        // get the optional attributes
                        if (parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.NextToken();
                                DisplayAttribute attrib;
                                while (DisplayAttribute.TryParseNode(parser, out attrib, node.IsArray))
                                {
                                    node.Attributes.Add(attrib);
                                    if (!parser.PeekToken(TokenKind.Comma))
                                        break;
                                    parser.NextToken();
                                }

                                if (parser.PeekToken(TokenKind.RightParenthesis))
                                    parser.NextToken();
                                else
                                    parser.ReportSyntaxError("Expecting right-paren in display attributes section.");
                            }
                            else
                                parser.ReportSyntaxError("Expecting left-paren in display attributes section.");
                        }
                        node.EndIndex = parser.Token.Span.End;
                    }
                    else
                        parser.ReportSyntaxError("Expected \"name\" keyword in display statement.");
                }
                else if (parser.PeekToken(TokenKind.ArrayKeyword))
                {
                    parser.NextToken();
                    node.IsArray = true;

                    NameExpression arrName;
                    if (NameExpression.TryParseNode(parser, out arrName))
                        node.ArrayName = arrName;
                    else
                        parser.ReportSyntaxError("Invalid array name found in display array statement.");

                    node.DecoratorEnd = parser.Token.Span.End;

                    if (parser.PeekToken(TokenKind.ToKeyword))
                    {
                        parser.NextToken();
                        if (NameExpression.TryParseNode(parser, out arrName))
                            node.ScreenArrayName = arrName;
                        else
                            parser.ReportSyntaxError("Invalid array name found in display array statement.");

                        if (parser.PeekToken(TokenKind.HelpKeyword))
                        {
                            parser.NextToken();

                            // get the help number
                            ExpressionNode optionNumber;
                            if (ExpressionNode.TryGetExpressionNode(parser, out optionNumber))
                                node.HelpNumber = optionNumber;
                            else
                                parser.ReportSyntaxError("Invalid help-number found in input statement.");
                        }

                        // get the optional attributes
                        if (parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.NextToken();
                                DisplayAttribute attrib;
                                while (DisplayAttribute.TryParseNode(parser, out attrib, node.IsArray))
                                {
                                    node.Attributes.Add(attrib);
                                    if (!parser.PeekToken(TokenKind.Comma))
                                        break;
                                    parser.NextToken();
                                }

                                if (parser.PeekToken(TokenKind.RightParenthesis))
                                    parser.NextToken();
                                else
                                    parser.ReportSyntaxError("Expecting right-paren in display attributes section.");
                            }
                            else
                                parser.ReportSyntaxError("Expecting left-paren in display attributes section.");
                        }

                        List<TokenKind> validExits = new List<TokenKind>();
                        if (validExitKeywords != null)
                            validExits.AddRange(validExitKeywords);
                        validExits.Add(TokenKind.DisplayKeyword);

                        HashSet<TokenKind> newEndKeywords = new HashSet<TokenKind>();
                        if (endKeywords != null)
                            newEndKeywords.AddRange(endKeywords);
                        newEndKeywords.Add(TokenKind.DisplayKeyword);

                        bool hasControlBlocks = false;
                        DisplayControlBlock icb;
                        prepStatementBinders.Insert(0, node.BindPrepareCursorFromIdentifier);
                        while (DisplayControlBlock.TryParseNode(parser, out icb, containingModule, hasControlBlocks, node.IsArray, prepStatementBinders, 
                                                                returnStatementBinder, limitedScopeVariableAdder, validExits, contextStatementFactories, newEndKeywords) && icb != null)
                        {
                            // check for include file sign
                            if (icb.StartIndex < 0)
                                continue;

                            node.Children.Add(icb.StartIndex, icb);
                            hasControlBlocks = true;
                            if (parser.PeekToken(TokenKind.EndOfFile) ||
                                (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.DisplayKeyword, 2)))
                            {
                                break;
                            }
                        }
                        prepStatementBinders.RemoveAt(0);

                        if (hasControlBlocks ||
                           (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.DisplayKeyword, 2)))
                        {
                            if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.DisplayKeyword, 2)))
                            {
                                parser.ReportSyntaxError("A display block must be terminated with \"end display\".");
                            }
                            else
                            {
                                parser.NextToken(); // advance to the 'end' token
                                parser.NextToken(); // advance to the 'display' token
                                node.EndIndex = parser.Token.Span.End;
                            }
                        }
                    }
                    else
                        parser.ReportSyntaxError("Expected \"to\" keyword in display array statement.");
                }
                else
                {
                    // get the expression(s)
                    ExpressionNode mainExpression = null;
                    while (true)
                    {
                        ExpressionNode expr;
                        if (!ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.ToKeyword, TokenKind.AttributeKeyword, TokenKind.AttributesKeyword }))
                        {
                            parser.ReportSyntaxError("Display statement must have one or more comma-separated expressions.");
                            break;
                        }
                        if (mainExpression == null)
                            mainExpression = expr;
                        else
                            mainExpression.AppendExpression(expr);

                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }

                    if (mainExpression != null)
                        node.Expression = mainExpression;
                    else
                        parser.ReportSyntaxError("Invalid expression found in display statement.");

                    if (parser.PeekToken(TokenKind.ToKeyword))
                    {
                        parser.NextToken();

                        // get the field specs
                        NameExpression nameExpr;
                        while (NameExpression.TryParseNode(parser, out nameExpr))
                        {
                            node.FieldSpecs.Add(nameExpr);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        // get the optional attributes
                        if (parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.NextToken();
                                DisplayAttribute attrib;
                                while (DisplayAttribute.TryParseNode(parser, out attrib, node.IsArray))
                                {
                                    node.Attributes.Add(attrib);
                                    if (!parser.PeekToken(TokenKind.Comma))
                                        break;
                                    parser.NextToken();
                                }

                                if (parser.PeekToken(TokenKind.RightParenthesis))
                                    parser.NextToken();
                                else
                                    parser.ReportSyntaxError("Expecting right-paren in input attributes section.");
                            }
                            else
                                parser.ReportSyntaxError("Expecting left-paren in display attributes section.");
                        }
                    }

                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }

        public override bool CanOutline
        {
            get { return true; }
        }

        public override int DecoratorEnd { get; set; }
    }

    public class DisplayAttribute : AstNode4gl
    {
        public static bool TryParseNode(Genero4glParser parser, out DisplayAttribute node, bool isArray)
        {
            node = new DisplayAttribute();
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
                case TokenKind.NormalKeyword:
                case TokenKind.ReverseKeyword:
                case TokenKind.BlinkKeyword:
                case TokenKind.UnderlineKeyword:
                case TokenKind.InvisibleKeyword:
                    parser.NextToken();
                    break;
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
                                parser.ReportSyntaxError("Invalid boolean expression found in input attribute.");
                        }
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
                        if (isArray)
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
                        }
                        else
                            parser.ReportSyntaxError("Attribute can only be used for an input array statement.");
                        break;
                    }
                case TokenKind.KeepKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.CurrentKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.RowKeyword))
                            {
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.Equals))
                                {
                                    parser.NextToken();
                                    ExpressionNode boolExpr;
                                    if (!ExpressionNode.TryGetExpressionNode(parser, out boolExpr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                                        parser.ReportSyntaxError("Invalid boolean expression found in input array attribute.");
                                }
                            }
                            else
                                parser.ReportSyntaxError("Expected \"row\" keyword in input array attribute.");
                        }
                        else
                            parser.ReportSyntaxError("Expected \"current\" keyword in input array attribute.");
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            return result;
        }
    }

    public enum DisplayControlBlockType
    {
        None,
        Display,
        Row,
        Idle,
        Action,
        Fill,
        Append,
        Insert,
        Update,
        Delete,
        Expand,
        Collapse,
        DragStart,
        DragFinish,
        DragEnter,
        DragOver,
        Drop,
        Key
    }

    public class DisplayControlBlock : AstNode4gl
    {
        public ExpressionNode IdleSeconds { get; private set; }
        public NameExpression ActionName { get; private set; }
        public NameExpression RowIndex { get; private set; }

        public NameExpression DragAndDropObject { get; private set; }
        public List<VirtualKey> KeyNameList { get; private set; }

        public DisplayControlBlockType Type { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out DisplayControlBlock node, IModuleResult containingModule, bool allowNonControlBlocks,
                                 bool isArray,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = new DisplayControlBlock();
            bool result = true;
            node.KeyNameList = new List<VirtualKey>();

            bool isDragAndDrop = false;
            switch (parser.PeekToken().Kind)
            {
                case TokenKind.Ampersand:
                    {
                        // handle include file
                        PreprocessorNode preNode;
                        PreprocessorNode.TryParseNode(parser, out preNode);
                        node.StartIndex = -1;
                        break;
                    }
                case TokenKind.BeforeKeyword:
                case TokenKind.AfterKeyword:
                    {
                        parser.NextToken();
                        node.StartIndex = parser.Token.Span.Start;
                        if (parser.PeekToken(TokenKind.DisplayKeyword))
                        {
                            parser.NextToken();
                            node.Type = DisplayControlBlockType.Display;
                        }
                        else if (parser.PeekToken(TokenKind.RowKeyword))
                        {
                            parser.NextToken();
                            node.Type = DisplayControlBlockType.Row;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Unexpected keyword found in display control block.");
                            result = false;
                        }
                        break;
                    }
                case TokenKind.OnKeyword:
                    {
                        parser.NextToken();
                        node.StartIndex = parser.Token.Span.Start;
                        switch (parser.PeekToken().Kind)
                        {
                            case TokenKind.IdleKeyword:
                                parser.NextToken();
                                node.Type = DisplayControlBlockType.Idle;
                                // get the idle seconds
                                ExpressionNode idleExpr;
                                if (ExpressionNode.TryGetExpressionNode(parser, out idleExpr))
                                    node.IdleSeconds = idleExpr;
                                else
                                    parser.ReportSyntaxError("Invalid idle-seconds found in display array statement.");
                                break;
                            case TokenKind.ActionKeyword:
                                parser.NextToken();
                                node.Type = DisplayControlBlockType.Action;
                                // get the action name
                                NameExpression actionName;
                                if (NameExpression.TryParseNode(parser, out actionName))
                                    node.ActionName = actionName;
                                else
                                    parser.ReportSyntaxError("Invalid action-name found in display array statement.");
                                break;
                            case TokenKind.KeyKeyword:
                                parser.NextToken();
                                node.Type = DisplayControlBlockType.Key;
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
                                        parser.ReportSyntaxError("Expected right-paren in display array block.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected left-paren in display array block.");
                                break;
                            case TokenKind.AppendKeyword:
                                node.Type = DisplayControlBlockType.Append;
                                parser.NextToken();
                                break;
                            case TokenKind.InsertKeyword:
                                node.Type = DisplayControlBlockType.Insert;
                                parser.NextToken();
                                break;
                            case TokenKind.UpdateKeyword:
                                node.Type = DisplayControlBlockType.Update;
                                parser.NextToken();
                                break;
                            case TokenKind.DeleteKeyword:
                                node.Type = DisplayControlBlockType.Delete;
                                parser.NextToken();
                                break;
                            case TokenKind.ExpandKeyword:
                                node.Type = DisplayControlBlockType.Expand;
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.LeftParenthesis))
                                {
                                    parser.NextToken();
                                    NameExpression rowInd;
                                    if (NameExpression.TryParseNode(parser, out rowInd))
                                        node.RowIndex = rowInd;
                                    else
                                        parser.ReportSyntaxError("Invalid row-index found in display array statement.");

                                    if (parser.PeekToken(TokenKind.RightParenthesis))
                                        parser.NextToken();
                                    else
                                        parser.ReportSyntaxError("Expected right-paren in display array statement.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected left-paren in display array statement.");
                                break;
                            case TokenKind.CollapseKeyword:
                                node.Type = DisplayControlBlockType.Collapse;
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.LeftParenthesis))
                                {
                                    parser.NextToken();
                                    NameExpression rowInd1;
                                    if (NameExpression.TryParseNode(parser, out rowInd1))
                                        node.RowIndex = rowInd1;
                                    else
                                        parser.ReportSyntaxError("Invalid row-index found in display array statement.");
                                    if (parser.PeekToken(TokenKind.RightParenthesis))
                                        parser.NextToken();
                                    else
                                        parser.ReportSyntaxError("Expected right-paren in display array statement.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected left-paren in display array statement.");
                                break;
                            case TokenKind.Drag_EnterKeyword:
                                node.Type = DisplayControlBlockType.DragEnter;
                                parser.NextToken();
                                isDragAndDrop = true;
                                break;
                            case TokenKind.Drag_FinishKeyword:
                            case TokenKind.Drag_FinishedKeyword:
                                node.Type = DisplayControlBlockType.DragFinish;
                                parser.NextToken();
                                isDragAndDrop = true;
                                break;
                            case TokenKind.Drag_OverKeyword:
                                node.Type = DisplayControlBlockType.DragOver;
                                parser.NextToken();
                                isDragAndDrop = true;
                                break;
                            case TokenKind.Drag_StartKeyword:
                                node.Type = DisplayControlBlockType.DragStart;
                                parser.NextToken();
                                isDragAndDrop = true;
                                break;
                            case TokenKind.DropKeyword:
                                node.Type = DisplayControlBlockType.Drop;
                                parser.NextToken();
                                isDragAndDrop = true;
                                break;
                            default:
                                parser.ReportSyntaxError("Unexpected token found in input control block.");
                                result = false;
                                break;
                        }
                        break;
                    }
                default:
                    if (!allowNonControlBlocks)
                        result = false;
                    else
                        node.StartIndex = parser.Token.Span.Start;
                    break;
            }

            if (result && node.StartIndex >= 0)
            {
                node.DecoratorEnd = parser.Token.Span.End;
                if (isDragAndDrop)
                {
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();
                        NameExpression keyName;
                        if (NameExpression.TryParseNode(parser, out keyName))
                            node.DragAndDropObject = keyName;
                        else
                            parser.ReportSyntaxError("Invalid drag-and-drop object name found in display array statement.");

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected right-paren in input control block.");
                    }
                    else
                        parser.ReportSyntaxError("Expected left-paren in input control block.");
                }

                // get the dialog statements
                FglStatement dispStmt;
                prepStatementBinders.Insert(0, node.BindPrepareCursorFromIdentifier);
                while (DisplayStatementFactory.TryGetStatement(parser, out dispStmt, isArray, containingModule, prepStatementBinders, returnStatementBinder, 
                                                               limitedScopeVariableAdder, validExitKeywords, contextStatementFactories, endKeywords) && dispStmt != null)
                {
                    node.Children.Add(dispStmt.StartIndex, dispStmt);
                    node.EndIndex = dispStmt.EndIndex;
                }
                prepStatementBinders.RemoveAt(0);

                if (node.Type == DisplayControlBlockType.None && node.Children.Count == 0)
                    result = false;
            }

            return result;
        }

        public override bool CanOutline
        {
            get { return true; }
        }

        public override int DecoratorEnd { get; set; }
    }

    public class DisplayStatementFactory
    {
        private static bool TryGetDisplayStatement(Genero4glParser parser, out DisplayStatement node, bool isArray, bool returnFalseInsteadOfErrors = false)
        {
            bool result = false;
            node = null;

            DisplayStatement inputStmt;
            if ((result = DisplayStatement.TryParseNode(parser, out inputStmt, isArray, null, returnFalseInsteadOfErrors)))
            {
                node = inputStmt;
            }

            return result;
        }

        public static bool TryGetStatement(Genero4glParser parser, out FglStatement node, bool isArray,
                                 IModuleResult containingModule,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            bool result = false;
            node = null;

            DisplayStatement inputStmt;
            if ((result = TryGetDisplayStatement(parser, out inputStmt, isArray)))
            {
                node = inputStmt;
            }
            else
            {
                List<ContextStatementFactory> csfs = new List<ContextStatementFactory>();
                if (contextStatementFactories != null)
                    csfs.AddRange(contextStatementFactories);
                csfs.Add((x) =>
                {
                    DisplayStatement testNode;
                    TryGetDisplayStatement(x, out testNode, isArray, true);
                    return testNode;
                });
                result = parser.StatementFactory.TryParseNode(parser, out node, containingModule, prepStatementBinders, 
                                                              returnStatementBinder, limitedScopeVariableAdder, false, validExitKeywords, csfs, null, endKeywords);
            }

            return result;
        }
    }

    public class DisplayStatement : FglStatement
    {
        public static bool TryParseNode(Genero4glParser parser, out DisplayStatement node, bool isArray, List<TokenKind> validExitKeywords = null, bool returnFalseInsteadOfErrors = false)
        {
            node = new DisplayStatement();
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.AcceptKeyword:
                case TokenKind.ContinueKeyword:
                case TokenKind.ExitKeyword:
                    {
                        if (parser.PeekToken(TokenKind.DisplayKeyword, 2))
                        {
                            parser.NextToken();
                            node.StartIndex = parser.Token.Span.Start;
                            parser.NextToken();
                        }
                        else
                            result = false;
                        break;
                    }
                default:
                    {
                        result = false;
                        break;
                    }
            }

            return result;
        }
    }
}
