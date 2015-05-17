using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class InputBlock : FglStatement
    {
        public bool IsImplicitMapping { get; private set; }
        public List<NameExpression> VariableList { get; private set; }
        public List<NameExpression> FieldList { get; private set; }
        public List<ExpressionNode> Attributes { get; private set; }
        public ExpressionNode HelpNumber { get; private set; }

        public static bool TryParseNode(Parser parser, out InputBlock node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.InputKeyword))
            {
                result = true;
                node = new InputBlock();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.VariableList = new List<NameExpression>();
                node.FieldList = new List<NameExpression>();
                node.Attributes = new List<ExpressionNode>();

                if(parser.PeekToken(TokenKind.ByKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.NameKeyword))
                    {
                        parser.NextToken();

                        // Implicit field mapping
                        node.IsImplicitMapping = true;
                    }
                    else
                        parser.ReportSyntaxError("Expected \"name\" token in input statement.");
                }

                // read the variable list
                NameExpression nameExpr;
                while (NameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.VariableList.Add(nameExpr);
                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }
                    

                if (parser.PeekToken(TokenKind.WithoutKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.DefaultsKeyword))
                    {
                        parser.NextToken();
                    }
                    else
                        parser.ReportSyntaxError("Expected \"defaults\" token in input statement.");
                }

                if(!node.IsImplicitMapping)
                {
                    if(parser.PeekToken(TokenKind.FromKeyword))
                    {
                        parser.NextToken();

                        // read the field list
                        while (NameExpression.TryParseNode(parser, out nameExpr))
                        {
                            node.FieldList.Add(nameExpr);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }
                    }
                    else
                        parser.ReportSyntaxError("Expected \"from\" token in input statement.");
                }

                if(parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        // get the list of display or control attributes
                        ExpressionNode expr;
                        while (ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                        {
                            node.Attributes.Add(expr);
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
                        parser.ReportSyntaxError("Expecting left-paren in input attributes section.");
                }

                if(parser.PeekToken(TokenKind.HelpKeyword))
                {
                    parser.NextToken();

                    // get the help number
                    ExpressionNode optionNumber;
                    if (ExpressionNode.TryGetExpressionNode(parser, out optionNumber))
                        node.HelpNumber = optionNumber;
                    else
                        parser.ReportSyntaxError("Invalid help-number found in input statement.");
                }

                // get the dialog control blocks
                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                       !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.InputKeyword, 2)))
                {
                    InputControlBlock icb;
                    if (InputControlBlock.TryParseNode(parser, out icb))
                        node.Children.Add(icb.StartIndex, icb);
                    else
                        parser.NextToken();
                }

                if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.InputKeyword, 2)))
                {
                    parser.ReportSyntaxError("A input block must be terminated with \"end input\".");
                }
                else
                {
                    parser.NextToken(); // advance to the 'end' token
                    parser.NextToken(); // advance to the 'input' token
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }
    }

    public enum InputControlBlockType
    {
        None,
        Field,
        Input,
        Change,
        Idle,
        Action,
        Key
    }

    public class InputControlBlock : AstNode
    {
        public ExpressionNode IdleSeconds { get; private set; }
        public NameExpression ActionName { get; private set; }
        public NameExpression ActionField { get; private set; }

        public List<NameExpression> FieldSpecList { get; private set; }
        public List<NameExpression> KeyNameList { get; private set; }

        public InputControlBlockType Type { get; private set; }

        public static bool TryParseNode(Parser parser, out InputControlBlock node,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            node = new InputControlBlock();
            bool result = true;
            node.StartIndex = parser.Token.Span.Start;
            node.FieldSpecList = new List<NameExpression>();
            node.KeyNameList = new List<NameExpression>();

            switch(parser.PeekToken().Kind)
            {
                case TokenKind.BeforeKeyword:
                case TokenKind.AfterKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.FieldKeyword))
                        {
                            parser.NextToken();
                            node.Type = InputControlBlockType.Field;
                            // get the list of field specs
                            NameExpression fieldSpec;
                            while (NameExpression.TryParseNode(parser, out fieldSpec))
                            {
                                node.FieldSpecList.Add(fieldSpec);
                                if (!parser.PeekToken(TokenKind.Comma))
                                    break;
                                parser.NextToken();
                            }
                        }
                        else if (parser.PeekToken(TokenKind.InputKeyword))
                        {
                            parser.NextToken();
                            node.Type = InputControlBlockType.Input;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Unexpected token found in input control block.");
                            result = false;
                        }
                        break;
                    }
                case TokenKind.OnKeyword:
                    {
                        parser.NextToken();
                        switch(parser.PeekToken().Kind)
                        {
                            case TokenKind.ChangeKeyword:
                                parser.NextToken();
                                node.Type = InputControlBlockType.Change;
                                // get the list of field specs
                                NameExpression fieldSpec;
                                while (NameExpression.TryParseNode(parser, out fieldSpec))
                                {
                                    node.FieldSpecList.Add(fieldSpec);
                                    if (!parser.PeekToken(TokenKind.Comma))
                                        break;
                                    parser.NextToken();
                                }
                                break;
                            case TokenKind.IdleKeyword:
                                parser.NextToken();
                                node.Type = InputControlBlockType.Idle;
                                // get the idle seconds
                                ExpressionNode idleExpr;
                                if (ExpressionNode.TryGetExpressionNode(parser, out idleExpr))
                                    node.IdleSeconds = idleExpr;
                                else
                                    parser.ReportSyntaxError("Invalid idle-seconds found in input statement.");
                                break;
                            case TokenKind.ActionKeyword:
                                parser.NextToken();
                                node.Type = InputControlBlockType.Action;
                                // get the action name
                                NameExpression actionName;
                                if (NameExpression.TryParseNode(parser, out actionName))
                                    node.ActionName = actionName;
                                else
                                    parser.ReportSyntaxError("Invalid action-name found in input statement.");
                                if(parser.PeekToken(TokenKind.InfieldKeyword))
                                {
                                    parser.NextToken();
                                    // get the field-spec
                                    if (NameExpression.TryParseNode(parser, out actionName))
                                        node.ActionField = actionName;
                                    else
                                        parser.ReportSyntaxError("Invalid field-spec found in input statement.");
                                }
                                break;
                            case TokenKind.KeyKeyword:
                                parser.NextToken();
                                node.Type = InputControlBlockType.Key;
                                if (parser.PeekToken(TokenKind.LeftParenthesis))
                                {
                                    parser.NextToken();
                                    // get the list of key names
                                    NameExpression keyName;
                                    while (NameExpression.TryParseNode(parser, out keyName))
                                    {
                                        node.KeyNameList.Add(keyName);
                                        if (!parser.PeekToken(TokenKind.Comma))
                                            break;
                                        parser.NextToken();
                                    }
                                    if (parser.PeekToken(TokenKind.RightParenthesis))
                                        parser.NextToken();
                                    else
                                        parser.ReportSyntaxError("Expected right-paren in input control block.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected left-paren in input control block.");
                                break;
                            default:
                                parser.ReportSyntaxError("Unexpected token found in input control block.");
                                //result = false;
                                break;
                        }
                        break;
                    }
                default:
                    //result = false;
                    break;
            }

            if(result)
            {
                // get the dialog statements
                FglStatement inputStmt;
                while (InputDialogStatementFactory.TryGetStatement(parser, out inputStmt, prepStatementResolver, prepStatementBinder))
                    node.Children.Add(inputStmt.StartIndex, inputStmt);

                if (node.Type == InputControlBlockType.None && node.Children.Count == 0)
                    result = false;
            }

            return result;
        }
    }

    public class InputDialogStatementFactory
    {
        public static bool TryGetStatement(Parser parser, out FglStatement node,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            bool result = false;
            node = null;

            InputDialogStatement inputStmt;
            if ((result = InputDialogStatement.TryParseNode(parser, out inputStmt)))
            {
                node = inputStmt;
            }
            else
            {
                result = parser.StatementFactory.TryParseNode(parser, out node, prepStatementResolver, prepStatementBinder);
            }

            return result;
        }
    }

    public class InputDialogStatement : FglStatement
    {
        public NameExpression FieldSpec { get; private set; }

        public static bool TryParseNode(Parser parser, out InputDialogStatement node)
        {
            node = new InputDialogStatement();
            bool result = true;

            switch(parser.PeekToken().Kind)
            {
                case TokenKind.AcceptKeyword:
                case TokenKind.ContinueKeyword:
                case TokenKind.ExitKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.InputKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting \"input\" keyword in input statement.");
                        break;
                    }
                case TokenKind.NextKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.FieldKeyword))
                        {
                            parser.NextToken();
                            switch(parser.PeekToken().Kind)
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
                                            parser.ReportSyntaxError("Invalid field-spec found in input statement.");
                                        break;
                                    }
                            }
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"field\" keyword in input statement.");
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
