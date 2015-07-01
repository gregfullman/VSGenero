using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class DialogBlock : FglStatement, IOutlinableResult
    {
        public List<DialogAttribute> Attributes { get; private set; }
        public List<NameExpression> Subdialogs { get; private set; }

        public static bool TryParseNode(Parser parser, out DialogBlock node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.DialogKeyword))
            {
                result = true;
                node = new DialogBlock();
                node.Attributes = new List<DialogAttribute>();
                node.Subdialogs = new List<NameExpression>();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.DecoratorEnd = parser.Token.Span.End;

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
                while(moreBlocks)
                {
                    switch(parser.PeekToken().Kind)
                    {
                        case TokenKind.InputKeyword:
                            {
                                InputBlock inputBlock;
                                if (InputBlock.TryParseNode(parser, out inputBlock, containingModule, prepStatementBinder, validExitKeywords) && inputBlock != null)
                                    node.Children.Add(inputBlock.StartIndex, inputBlock);
                                else
                                    parser.ReportSyntaxError("Invalid input block found in dialog statement.");
                                break;
                            }
                        case TokenKind.ConstructKeyword:
                            {
                                ConstructBlock constructBlock;
                                if (ConstructBlock.TryParseNode(parser, out constructBlock, containingModule, prepStatementBinder, validExitKeywords) && constructBlock != null)
                                    node.Children.Add(constructBlock.StartIndex, constructBlock);
                                else
                                    parser.ReportSyntaxError("Invalid construct block found in dialog statement.");
                                break;
                            }
                        case TokenKind.DisplayKeyword:
                            {
                                DisplayBlock dispBlock;
                                if (DisplayBlock.TryParseNode(parser, out dispBlock, containingModule, prepStatementBinder, validExitKeywords) && dispBlock != null)
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

                // get the dialog control blocks
                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                       !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.DialogKeyword, 2)))
                {
                    DialogControlBlock icb;
                    if (DialogControlBlock.TryParseNode(parser, out icb, containingModule, prepStatementBinder, validExits) && icb != null)
                        node.Children.Add(icb.StartIndex, icb);
                    else
                        parser.NextToken();
                }

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

        public bool CanOutline
        {
            get { return true; }
        }

        public int DecoratorStart
        {
            get
            {
                return StartIndex;
            }
            set
            {
            }
        }

        public int DecoratorEnd { get; set; }
    }

    public class DialogAttribute : AstNode
    {
        public static bool TryParseNode(Parser parser, out DialogAttribute node)
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

    public class DialogControlBlock : AstNode
    {
        public DialogControlBlockType Type { get; private set; }
        public NameExpression ActionName { get; private set; }
        public List<NameExpression> KeyNames { get; private set; }
        public ExpressionNode IdleSeconds { get; private set; }
        public NameExpression OptionName { get; private set; }
        public ExpressionNode OptionComment { get; private set; }
        public ExpressionNode HelpNumber { get; private set; }

        public static bool TryParseNode(Parser parser, out DialogControlBlock node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null)
        {
            node = new DialogControlBlock();
            node.StartIndex = parser.Token.Span.Start;
            node.KeyNames = new List<NameExpression>();
            bool result = true;

            switch(parser.PeekToken().Kind)
            {
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

                        NameExpression nameExpr;
                        if(parser.PeekToken(TokenKind.KeyKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                parser.NextToken();
                                while (NameExpression.TryParseNode(parser, out nameExpr))
                                {
                                    node.KeyNames.Add(nameExpr);
                                    if (parser.PeekToken(TokenKind.Comma))
                                        break;
                                    parser.NextToken();
                                }
                            }
                            else
                                parser.ReportSyntaxError("Expecting left-paren in dialog statement.");
                        }

                        if(NameExpression.TryParseNode(parser, out nameExpr))
                            node.OptionName = nameExpr;
                        else
                            parser.ReportSyntaxError("Invalid expression found in dialog statement.");

                        if(!parser.PeekToken(TokenKind.HelpKeyword))
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
                                    while (NameExpression.TryParseNode(parser, out nameExpr))
                                    {
                                        node.KeyNames.Add(nameExpr);
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

            if (result)
            {
                // get the dialog statements
                FglStatement inputStmt;
                while (DialogStatementFactory.TryGetStatement(parser, out inputStmt, containingModule, prepStatementBinder, validExitKeywords) && inputStmt != null)
                    node.Children.Add(inputStmt.StartIndex, inputStmt);

                if (node.Type == DialogControlBlockType.None && node.Children.Count == 0)
                    result = false;
            }

            return result;
        }
    }

    public class DialogStatementFactory
    {
        public static bool TryGetStatement(Parser parser, out FglStatement node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null)
        {
            bool result = false;
            node = null;

            DialogStatement inputStmt;
            if ((result = DialogStatement.TryParseNode(parser, out inputStmt)))
            {
                node = inputStmt;
            }
            else
            {
                result = parser.StatementFactory.TryParseNode(parser, out node, containingModule, prepStatementBinder, false, validExitKeywords);
            }

            return result;
        }
    }

    public class DialogStatement : FglStatement
    {
        public NameExpression FieldSpec { get; private set; }

        public static bool TryParseNode(Parser parser, out DialogStatement node)
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
                            parser.ReportSyntaxError("Expecting \"field\" keyword in dialog statement.");
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
