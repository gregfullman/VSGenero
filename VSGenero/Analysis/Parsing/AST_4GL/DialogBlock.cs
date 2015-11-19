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
    public class DeclarativeDialogBlock : FunctionBlockNode
    {
        private DialogBlock Dialog { get; set; }

        public static bool TryParseNode(Genero4glParser parser, out DeclarativeDialogBlock node, IModuleResult containingModule)
        {
            node = null;
            bool result = false;
            AccessModifier? accMod = null;
            string accModToken = null;

            if (parser.PeekToken(TokenKind.PublicKeyword))
            {
                accMod = AccessModifier.Public;
                accModToken = parser.PeekToken().Value.ToString();
            }
            else if (parser.PeekToken(TokenKind.PrivateKeyword))
            {
                accMod = AccessModifier.Private;
                accModToken = parser.PeekToken().Value.ToString();
            }

            uint lookAheadBy = (uint)(accMod.HasValue ? 2 : 1);
            if (parser.PeekToken(TokenKind.DialogKeyword, lookAheadBy))
            {
                result = true;
                node = new DeclarativeDialogBlock();
                if (accMod.HasValue)
                {
                    parser.NextToken();
                    node.AccessModifier = accMod.Value;
                }
                else
                {
                    node.AccessModifier = AccessModifier.Public;
                }

                parser.NextToken(); // move past the Function keyword
                node.StartIndex = parser.Token.Span.Start;

                // get the name
                if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    node.Name = parser.Token.Token.Value.ToString();
                    node.LocationIndex = parser.Token.Span.Start;
                    node.DecoratorEnd = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError("A declarative dialog must have a name.");
                }

                if (!parser.PeekToken(TokenKind.LeftParenthesis))
                    parser.ReportSyntaxError("A declarative dialog must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                // get the parameters
                while (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    string errMsg;
                    if (!node.AddArgument(parser.Token, out errMsg))
                    {
                        parser.ReportSyntaxError(errMsg);
                    }
                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();

                    // TODO: probably need to handle "end" "function" case...won't right now
                }

                if (!parser.PeekToken(TokenKind.RightParenthesis))
                    parser.ReportSyntaxError("A declarative dialog must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                List<List<TokenKind>> breakSequences =
                    new List<List<TokenKind>>
                    { 
                        new List<TokenKind> { TokenKind.InputKeyword },
                        new List<TokenKind> { TokenKind.ConstructKeyword },
                        new List<TokenKind> { TokenKind.DisplayKeyword },
                        new List<TokenKind> { TokenKind.EndKeyword, TokenKind.DialogKeyword }
                    };

                // only defines are allowed in declarative dialogs
                DefineNode defineNode;
                bool matchedBreakSequence = false;
                while (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences) && defineNode != null)
                {
                    node.Children.Add(defineNode.StartIndex, defineNode);
                    foreach (var def in defineNode.GetDefinitions())
                        foreach (var vardef in def.VariableDefinitions)
                        {
                            vardef.Scope = "local variable";
                            if (!node.Variables.ContainsKey(vardef.Name))
                                node.Variables.Add(vardef.Name, vardef);
                            else
                                parser.ReportSyntaxError(vardef.LocationIndex, vardef.LocationIndex + vardef.Name.Length, string.Format("Variable {0} defined more than once.", vardef.Name), Severity.Error);
                        }

                    if (parser.PeekToken(TokenKind.EndOfFile) ||
                          (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ReportKeyword, 2)))
                    {
                        break;
                    }

                    // if a break sequence was matched, we don't want to advance the token
                    if (!matchedBreakSequence)
                    {
                        // TODO: not sure whether to break or keep going...for right now, let's keep going until we hit the end keyword
                        parser.NextToken();
                    }
                }

                // we don't want the dialog piece itself to be outlinable, since it's a declarative block
                node.Dialog = new DialogBlock(false);
                node.Dialog.Attributes = new List<DialogAttribute>();
                node.Dialog.Subdialogs = new List<NameExpression>();

                DialogBlock.BuildDialogBlock(parser, node.Dialog, containingModule);

                foreach (var child in node.Dialog.Children)
                    node.Children.Add(child.Key, child.Value);

                if (!parser.PeekToken(TokenKind.EndOfFile))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.DialogKeyword))
                    {
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                        node.IsComplete = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of declarative dialog definition.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Unexpected end of declarative dialog definition");
                }
            }

            return result;
        }
    }

    public class DialogBlock : FglStatement
    {
        private readonly bool _canOutline;

        internal DialogBlock(bool canOutline)
        {
            _canOutline = canOutline;
        }

        public List<DialogAttribute> Attributes { get; internal set; }
        public List<NameExpression> Subdialogs { get; internal set; }

        internal static void BuildDialogBlock(Genero4glParser parser, DialogBlock node, IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            if(parser.PeekToken(TokenKind.AttributeKeyword) || parser.PeekToken(TokenKind.AttributesKeyword))
            {
                parser.NextToken();
                if (parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();

                    // get the list of display or control attributes
                    DialogAttribute attrib;
                    while (DialogAttribute.TryParseNode(parser, out attrib))
                    {
                        node.Attributes.Add(attrib);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }

                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expecting right-paren in dialog attributes section.");
                }
                else
                    parser.ReportSyntaxError("Expecting left-paren in dialog attributes section.");
            }

            // parse input, construct, display or SUBDIALOG
            bool moreBlocks = true;
            List<ContextStatementFactory> csfs = new List<ContextStatementFactory>();
            if (contextStatementFactories != null)
                csfs.AddRange(contextStatementFactories);
            csfs.Add((x) =>
            {
                DialogStatement testNode;
                DialogStatementFactory.TryGetDialogStatement(x, out testNode, true);
                return testNode;
            });
            while(moreBlocks)
            {
                switch(parser.PeekToken().Kind)
                {
                    case TokenKind.InputKeyword:
                        {
                            InputBlock inputBlock;
                            if (InputBlock.TryParseNode(parser, out inputBlock, containingModule, prepStatementBinder, returnStatementBinder, 
                                                        limitedScopeVariableAdder, validExitKeywords, csfs, endKeywords) && inputBlock != null)
                                node.Children.Add(inputBlock.StartIndex, inputBlock);
                            else
                                parser.ReportSyntaxError("Invalid input block found in dialog statement.");
                            break;
                        }
                    case TokenKind.ConstructKeyword:
                        {
                            ConstructBlock constructBlock;
                            if (ConstructBlock.TryParseNode(parser, out constructBlock, containingModule, prepStatementBinder, returnStatementBinder, 
                                                            limitedScopeVariableAdder, validExitKeywords, csfs, endKeywords) && constructBlock != null)
                                node.Children.Add(constructBlock.StartIndex, constructBlock);
                            else
                                parser.ReportSyntaxError("Invalid construct block found in dialog statement.");
                            break;
                        }
                    case TokenKind.DisplayKeyword:
                        {
                            DisplayBlock dispBlock;
                            if (DisplayBlock.TryParseNode(parser, out dispBlock, containingModule, prepStatementBinder, returnStatementBinder, 
                                                          limitedScopeVariableAdder, validExitKeywords, csfs, endKeywords) && dispBlock != null)
                                node.Children.Add(dispBlock.StartIndex, dispBlock);
                            else
                                parser.ReportSyntaxError("Invalid display block found in dialog statement.");
                            break;
                        }
                    case TokenKind.SubdialogKeyword:
                        {
                            parser.NextToken();
                            NameExpression nameExpr;
                            if (NameExpression.TryParseNode(parser, out nameExpr))
                                node.Subdialogs.Add(nameExpr);
                            else
                                parser.ReportSyntaxError("Invalid subdialog name found in dialog statement.");
                            break;
                        }
                    default:
                        moreBlocks = false;
                        break;
                }
            }

            List<TokenKind> validExits = new List<TokenKind>();
            if (validExitKeywords != null)
                validExits.AddRange(validExits);
            validExits.Add(TokenKind.DialogKeyword);

            HashSet<TokenKind> newEndKeywords = new HashSet<TokenKind>();
            if (endKeywords != null)
                newEndKeywords.AddRange(endKeywords);
            newEndKeywords.Add(TokenKind.DialogKeyword);

            // get the dialog control blocks
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                    !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.DialogKeyword, 2)))
            {
                DialogControlBlock icb;
                if (DialogControlBlock.TryParseNode(parser, out icb, containingModule, prepStatementBinder, returnStatementBinder, 
                                                    limitedScopeVariableAdder, validExits, contextStatementFactories, newEndKeywords) && icb != null)
                {
                    if (icb.StartIndex < 0)
                        continue;
                    node.Children.Add(icb.StartIndex, icb);
                }
                else if (parser.PeekToken(TokenKind.EndKeyword) && endKeywords != null && endKeywords.Contains(parser.PeekToken(2).Kind))
                {
                    break;
                }
                else
                    parser.NextToken();
            }
        }

        public static bool TryParseNode(Genero4glParser parser, out DialogBlock node,
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

            if(parser.PeekToken(TokenKind.DialogKeyword))
            {
                result = true;
                node = new DialogBlock(true);
                node.Attributes = new List<DialogAttribute>();
                node.Subdialogs = new List<NameExpression>();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.DecoratorEnd = parser.Token.Span.End;

                BuildDialogBlock(parser, node, containingModule, prepStatementBinder, returnStatementBinder, limitedScopeVariableAdder, 
                                 validExitKeywords, contextStatementFactories, endKeywords);

                if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.DialogKeyword, 2)))
                {
                    parser.ReportSyntaxError("A dialog block must be terminated with \"end dialog\".");
                }
                else
                {
                    parser.NextToken(); // advance to the 'end' token
                    parser.NextToken(); // advance to the 'dialog' token
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }

        public override bool CanOutline
        {
            get { return _canOutline; }
        }

        public override int DecoratorEnd { get; set; }
    }

    public class DialogAttribute : AstNode4gl
    {
        public static bool TryParseNode(Genero4glParser parser, out DialogAttribute node)
        {
            node = new DialogAttribute();
            node.StartIndex = parser.Token.Span.Start;
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.FieldKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.OrderKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected \"order\" keyword in dialog attribute.");
                        if (parser.PeekToken(TokenKind.FormKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected \"form\" keyword in dialog attribute.");
                        break;
                    }
                case TokenKind.UnbufferedKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            ExpressionNode boolExpr;
                            if (!ExpressionNode.TryGetExpressionNode(parser, out boolExpr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                                parser.ReportSyntaxError("Invalid boolean expression found in dialog attribute.");
                        }
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            return result;
        }
    }

    public enum DialogControlBlockType
    {
        None,
        Dialog,
        Command,
        Action,
        Key,
        Idle
    }

    public class DialogControlBlock : AstNode4gl
    {
        public DialogControlBlockType Type { get; private set; }
        public NameExpression ActionName { get; private set; }
        public List<VirtualKey> KeyNames { get; private set; }
        public ExpressionNode IdleSeconds { get; private set; }
        public ExpressionNode OptionName { get; private set; }
        public ExpressionNode OptionComment { get; private set; }
        public ExpressionNode HelpNumber { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out DialogControlBlock node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = new DialogControlBlock();
            node.StartIndex = parser.Token.Span.Start;
            node.KeyNames = new List<VirtualKey>();
            bool result = true;

            switch(parser.PeekToken().Kind)
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
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.DialogKeyword))
                    {
                        parser.NextToken();
                        node.Type = DialogControlBlockType.Dialog;
                    }
                    else
                        parser.ReportSyntaxError("Expected \"dialog\" keyword in dialog statement.");
                    break;
                case TokenKind.CommandKeyword:
                    {
                        parser.NextToken();
                        node.Type = DialogControlBlockType.Command;

                        if(parser.PeekToken(TokenKind.KeyKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.NextToken();
                                VirtualKey vKey;
                                while (VirtualKey.TryGetKey(parser, out vKey))
                                {
                                    node.KeyNames.Add(vKey);
                                    if (parser.PeekToken(TokenKind.Comma))
                                        break;
                                    parser.NextToken();
                                }
                            }
                            else
                                parser.ReportSyntaxError("Expecting left-paren in dialog statement.");
                        }

                        ExpressionNode nameExpression;
                        if (ExpressionNode.TryGetExpressionNode(parser, out nameExpression))
                            node.OptionName = nameExpression;
                        else
                            parser.ReportSyntaxError("Invalid expression found in dialog statement.");

                        if(!parser.PeekToken(TokenKind.HelpKeyword) && parser.PeekToken(TokenCategory.StringLiteral))
                        {
                            ExpressionNode commentExpr;
                            if (ExpressionNode.TryGetExpressionNode(parser, out commentExpr, new List<TokenKind> { TokenKind.HelpKeyword }))
                                node.OptionComment = commentExpr;
                            else
                                parser.ReportSyntaxError("Invalid expression found in dialog statement.");
                        }

                        if (parser.PeekToken(TokenKind.HelpKeyword))
                        {
                            parser.NextToken();
                            ExpressionNode optionNumber;
                            if (ExpressionNode.TryGetExpressionNode(parser, out optionNumber))
                                node.HelpNumber = optionNumber;
                            else
                                parser.ReportSyntaxError("Invalid expression found in dialog statement.");
                        }

                        break;
                    }
                case TokenKind.OnKeyword:
                    {
                        parser.NextToken();
                        switch(parser.PeekToken().Kind)
                        {
                            case TokenKind.ActionKeyword:
                                parser.NextToken();
                                node.Type = DialogControlBlockType.Action;
                                NameExpression nameExpr;
                                if (NameExpression.TryParseNode(parser, out nameExpr))
                                    node.ActionName = nameExpr;
                                else
                                    parser.ReportSyntaxError("Invalid name found in dialog statement.");
                                break;
                            case TokenKind.KeyKeyword:
                                parser.NextToken();
                                node.Type = DialogControlBlockType.Key;
                                if (parser.PeekToken(TokenKind.LeftParenthesis))
                                {
                                    parser.NextToken();
                                    VirtualKey vKey;
                                    while (VirtualKey.TryGetKey(parser, out vKey))
                                    {
                                        node.KeyNames.Add(vKey);
                                        if (parser.PeekToken(TokenKind.Comma))
                                            break;
                                        parser.NextToken();
                                    }
                                }
                                else
                                    parser.ReportSyntaxError("Expecting left-paren in dialog statement.");
                                break;
                            case TokenKind.IdleKeyword:
                                parser.NextToken();
                                node.Type = DialogControlBlockType.Idle;
                                // get the idle seconds
                                ExpressionNode idleExpr;
                                if (ExpressionNode.TryGetExpressionNode(parser, out idleExpr))
                                    node.IdleSeconds = idleExpr;
                                else
                                    parser.ReportSyntaxError("Invalid idle-seconds found in dialog statement.");
                                break;
                            default:
                                parser.ReportSyntaxError("Unexpected token found in dialog control block.");
                                break;
                        }
                        break;
                    }
                default:
                    break;
            }

            if (result && node.StartIndex >= 0)
            {
                // get the dialog statements
                FglStatement inputStmt;
                while (DialogStatementFactory.TryGetStatement(parser, out inputStmt, containingModule, prepStatementBinder, returnStatementBinder, 
                                                              limitedScopeVariableAdder, validExitKeywords, contextStatementFactories, endKeywords) && inputStmt != null)
                    node.Children.Add(inputStmt.StartIndex, inputStmt);

                if (node.Type == DialogControlBlockType.None && node.Children.Count == 0)
                    result = false;
            }

            return result;
        }
    }

    public class DialogStatementFactory
    {
        internal static bool TryGetDialogStatement(Genero4glParser parser, out DialogStatement node, bool returnFalseInsteadOfErrors = false)
        {
            bool result = false;
            node = null;
            DialogStatement inputStmt;
            if ((result = DialogStatement.TryParseNode(parser, out inputStmt, returnFalseInsteadOfErrors)))
            {
                node = inputStmt;
            }
            return result;
        }

        public static bool TryGetStatement(Genero4glParser parser, out FglStatement node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            bool result = false;
            node = null;

            DialogStatement inputStmt;
            if ((result = TryGetDialogStatement(parser, out inputStmt)))
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
                    DialogStatement testNode;
                    TryGetDialogStatement(x, out testNode, true);
                    return testNode;
                });
                result = parser.StatementFactory.TryParseNode(parser, out node, containingModule, prepStatementBinder, 
                                                              returnStatementBinder, limitedScopeVariableAdder, false, validExitKeywords, csfs, null, endKeywords);
            }

            return result;
        }
    }

    public class DialogStatement : FglStatement
    {
        public NameExpression FieldSpec { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out DialogStatement node, bool returnFalseInsteadOfErrors = false)
        {
            node = new DialogStatement();
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.AcceptKeyword:
                case TokenKind.ContinueKeyword:
                case TokenKind.ExitKeyword:
                    {
                        if (parser.PeekToken(TokenKind.DialogKeyword, 2))
                        {
                            parser.NextToken();
                            node.StartIndex = parser.Token.Span.Start;
                            parser.NextToken();
                        }
                        else
                            result = false;
                        break;
                    }
                case TokenKind.NextKeyword:
                    {
                        parser.NextToken();
                        node.StartIndex = parser.Token.Span.Start;
                        if (parser.PeekToken(TokenKind.FieldKeyword))
                        {
                            parser.NextToken();
                            switch (parser.PeekToken().Kind)
                            {
                                case TokenKind.CurrentKeyword:
                                case TokenKind.NextKeyword:
                                case TokenKind.PreviousKeyword:
                                    parser.NextToken();
                                    break;
                                default:
                                    {
                                        // get the field-spec
                                        NameExpression fieldSpec;
                                        if (NameExpression.TryParseNode(parser, out fieldSpec))
                                            node.FieldSpec = fieldSpec;
                                        else
                                            parser.ReportSyntaxError("Invalid field-spec found in dialog statement.");
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (!returnFalseInsteadOfErrors)
                                parser.ReportSyntaxError("Expecting \"field\" keyword in dialog statement.");
                            else
                                return false;
                        }
                        break;
                    }
                default:
                    {
                        result = false;
                        break;
                    }
            }

            if (result)
                node.EndIndex = parser.Token.Span.End;

            return result;
        }
    }
}
