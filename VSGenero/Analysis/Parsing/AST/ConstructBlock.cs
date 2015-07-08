using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class ConstructBlock : FglStatement, IOutlinableResult
    {
        public bool IsImplicitMapping { get; private set; }
        public NameExpression Variable { get; private set; }
        public List<NameExpression> ColumnList { get; private set; }
        public List<NameExpression> FieldList { get; private set; }
        public List<ConstructAttribute> Attributes { get; private set; }
        public ExpressionNode HelpNumber { get; private set; }


        public static bool TryParseNode(Parser parser, out ConstructBlock node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.ConstructKeyword))
            {
                result = true;
                node = new ConstructBlock();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.ColumnList = new List<NameExpression>();
                node.FieldList = new List<NameExpression>();
                node.Attributes = new List<ConstructAttribute>();

                if (parser.PeekToken(TokenKind.ByKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.NameKeyword))
                    {
                        parser.NextToken();

                        // Implicit field mapping
                        node.IsImplicitMapping = true;
                    }
                    else
                        parser.ReportSyntaxError("Expected \"name\" token in construct statement.");
                }

                NameExpression varName;
                if (NameExpression.TryParseNode(parser, out varName))
                    node.Variable = varName;
                else
                    parser.ReportSyntaxError("Invalid variable name found in construct statement.");

                node.DecoratorEnd = parser.Token.Span.End;

                if (parser.PeekToken(TokenKind.OnKeyword))
                    parser.NextToken();
                else
                    parser.ReportSyntaxError("Expecting \"on\" keyword in construct statement.");

                NameExpression colName;
                while (NameExpression.TryParseNode(parser, out colName))
                {
                    node.ColumnList.Add(colName);
                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }

                if (!node.IsImplicitMapping)
                {
                    if (parser.PeekToken(TokenKind.FromKeyword))
                    {
                        parser.NextToken();

                        // read the field list
                        NameExpression nameExpr;
                        while (NameExpression.TryParseNode(parser, out nameExpr))
                        {
                            node.FieldList.Add(nameExpr);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }
                    }
                    else
                        parser.ReportSyntaxError("Expected \"from\" token in construct statement.");
                }

                if (parser.PeekToken(TokenKind.AttributesKeyword) || parser.PeekToken(TokenKind.AttributeKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        // get the list of display or control attributes
                        ConstructAttribute attrib;
                        while (ConstructAttribute.TryParseNode(parser, out attrib))
                        {
                            node.Attributes.Add(attrib);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting right-paren in construct attributes section.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting left-paren in construct attributes section.");
                }

                if (parser.PeekToken(TokenKind.HelpKeyword))
                {
                    parser.NextToken();

                    // get the help number
                    ExpressionNode optionNumber;
                    if (ExpressionNode.TryGetExpressionNode(parser, out optionNumber))
                        node.HelpNumber = optionNumber;
                    else
                        parser.ReportSyntaxError("Invalid help-number found in construct statement.");
                }

                List<TokenKind> validExits = new List<TokenKind>();
                if (validExitKeywords != null)
                    validExits.AddRange(validExitKeywords);
                validExits.Add(TokenKind.ConstructKeyword);

                bool hasControlBlocks = false;
                ConstructControlBlock icb;
                while(ConstructControlBlock.TryParseNode(parser, out icb, containingModule, prepStatementBinder, validExits, contextStatementFactories) && icb != null)
                {
                    if (icb.StartIndex < 0)
                        continue;

                    node.Children.Add(icb.StartIndex, icb);
                    hasControlBlocks = true;
                    if (parser.PeekToken(TokenKind.EndOfFile) ||
                        (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ConstructKeyword, 2)))
                    {
                        break;
                    }
                }

                if (hasControlBlocks ||
                    (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ConstructKeyword, 2)))
                {
                    if (!(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ConstructKeyword, 2)))
                    {
                        parser.ReportSyntaxError("A construct block must be terminated with \"end construct\".");
                    }
                    else
                    {
                        parser.NextToken(); // advance to the 'end' token
                        parser.NextToken(); // advance to the 'construct' token
                        node.EndIndex = parser.Token.Span.End;
                    }
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

    public enum ConstructControlBlockType
    {
        None,
        Construct,
        Field,
        Idle,
        Action,
        Key
    }

    public class ConstructControlBlock : AstNode
    {
        public ExpressionNode IdleSeconds { get; private set; }
        public NameExpression ActionName { get; private set; }
        public NameExpression ActionField { get; private set; }

        public List<NameExpression> FieldSpecList { get; private set; }
        public List<VirtualKey> KeyNameList { get; private set; }

        public ConstructControlBlockType Type { get; private set; }

        public static bool TryParseNode(Parser parser, out ConstructControlBlock node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
        {
            node = new ConstructControlBlock();
            bool result = true;
            node.StartIndex = parser.Token.Span.Start;
            node.FieldSpecList = new List<NameExpression>();
            node.KeyNameList = new List<VirtualKey>();

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
                        if (parser.PeekToken(TokenKind.FieldKeyword))
                        {
                            parser.NextToken();
                            node.Type = ConstructControlBlockType.Field;
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
                        else if (parser.PeekToken(TokenKind.ConstructKeyword))
                        {
                            parser.NextToken();
                            node.Type = ConstructControlBlockType.Construct;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Unexpected token found in construct control block.");
                            result = false;
                        }
                        break;
                    }
                case TokenKind.OnKeyword:
                    {
                        parser.NextToken();
                        switch (parser.PeekToken().Kind)
                        {
                            case TokenKind.IdleKeyword:
                                parser.NextToken();
                                node.Type = ConstructControlBlockType.Idle;
                                // get the idle seconds
                                ExpressionNode idleExpr;
                                if (ExpressionNode.TryGetExpressionNode(parser, out idleExpr))
                                    node.IdleSeconds = idleExpr;
                                else
                                    parser.ReportSyntaxError("Invalid idle-seconds found in construct statement.");
                                break;
                            case TokenKind.ActionKeyword:
                                parser.NextToken();
                                node.Type = ConstructControlBlockType.Action;
                                // get the action name
                                NameExpression actionName;
                                if (NameExpression.TryParseNode(parser, out actionName))
                                    node.ActionName = actionName;
                                else
                                    parser.ReportSyntaxError("Invalid action-name found in construct statement.");
                                if (parser.PeekToken(TokenKind.InfieldKeyword))
                                {
                                    parser.NextToken();
                                    // get the field-spec
                                    if (NameExpression.TryParseNode(parser, out actionName))
                                        node.ActionField = actionName;
                                    else
                                        parser.ReportSyntaxError("Invalid field-spec found in construct statement.");
                                }
                                break;
                            case TokenKind.KeyKeyword:
                                parser.NextToken();
                                node.Type = ConstructControlBlockType.Key;
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
                                        parser.ReportSyntaxError("Expected right-paren in construct control block.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected left-paren in construct control block.");
                                break;
                            default:
                                parser.ReportSyntaxError("Unexpected token found in input control block.");
                                //result = false;
                                break;
                        }
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            if (result && node.StartIndex >= 0)
            {
                // get the dialog statements
                FglStatement inputStmt;
                while (ConstructDialogStatementFactory.TryGetStatement(parser, out inputStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories) && inputStmt != null)
                    node.Children.Add(inputStmt.StartIndex, inputStmt);

                if (node.Type == ConstructControlBlockType.None && node.Children.Count == 0)
                    result = false;
            }

            return result;
        }
    }

    public class ConstructDialogStatementFactory
    {
        private static bool TryGetConstructStatement(Parser parser, out ConstructDialogStatement node, bool returnFalseInsteadOfErrors = false)
        {
            bool result = false;
            node = null;

            ConstructDialogStatement constStmt;
            if ((result = ConstructDialogStatement.TryParseNode(parser, out constStmt, returnFalseInsteadOfErrors)))
            {
                node = constStmt;
            }

            return result;
        }

        public static bool TryGetStatement(Parser parser, out FglStatement node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
        {
            bool result = false;
            node = null;

            ConstructDialogStatement inputStmt;
            if ((result = TryGetConstructStatement(parser, out inputStmt)))
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
                    ConstructDialogStatement testNode;
                    TryGetConstructStatement(x, out testNode, true);
                    return testNode;
                });
                result = parser.StatementFactory.TryParseNode(parser, out node, containingModule, prepStatementBinder, false, validExitKeywords, csfs);
            }

            return result;
        }
    }

    public class ConstructDialogStatement : FglStatement
    {
        public NameExpression FieldSpec { get; private set; }

        public static bool TryParseNode(Parser parser, out ConstructDialogStatement node, bool returnFalseInsteadOfErrors = false)
        {
            node = new ConstructDialogStatement();
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.ContinueKeyword:
                case TokenKind.ExitKeyword:
                    {
                        if (parser.PeekToken(TokenKind.ConstructKeyword, 2))
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
                                            parser.ReportSyntaxError("Invalid field-spec found in construct statement.");
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (!returnFalseInsteadOfErrors)
                                parser.ReportSyntaxError("Expecting \"field\" keyword in construct statement.");
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

    public class ConstructAttribute : AstNode
    {
        public static bool TryParseNode(Parser parser, out ConstructAttribute node)
        {
            node = new ConstructAttribute();
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
                case TokenKind.AcceptKeyword:
                case TokenKind.CancelKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            ExpressionNode boolExpr;
                            if (!ExpressionNode.TryGetExpressionNode(parser, out boolExpr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                                parser.ReportSyntaxError("Invalid boolean expression found in construct attribute.");
                        }
                        break;
                    }
                case TokenKind.HelpKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected equals token in construct attribute.");

                        // get the help number
                        ExpressionNode optionNumber;
                        if (!ExpressionNode.TryGetExpressionNode(parser, out optionNumber))
                            parser.ReportSyntaxError("Invalid help-number found in construct attribute.");
                        break;
                    }
                case TokenKind.NameKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.Equals))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected equals token in construct attribute.");

                        // get the help number
                        ExpressionNode optionNumber;
                        if (!ExpressionNode.TryGetExpressionNode(parser, out optionNumber))
                            parser.ReportSyntaxError("Invalid dialog name found in construct attribute.");
                        break;
                    }
                case TokenKind.FieldKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.OrderKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected \"order\" keyword in input attribute.");
                        if (parser.PeekToken(TokenKind.FormKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected \"form\" keyword in input attribute.");
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            return result;
        }
    }
}
