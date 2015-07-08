using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// Types of create statements:
    /// 1) sequence
    /// 2) table
    /// 3) index
    /// 4) view
    /// 5) synonym
    /// </summary>
    public class CreateStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out CreateStatement node)
        {
            node = null;
            bool result = true;

            if(parser.PeekToken(TokenKind.CreateKeyword))
            {
                parser.NextToken();
                switch(parser.PeekToken().Kind)
                {
                    case TokenKind.SequenceKeyword:
                        CreateSequenceStatement sequenceNode;
                        result = CreateSequenceStatement.TryParseNode(parser, out sequenceNode);
                        node = sequenceNode;
                        break;
                    case TokenKind.TableKeyword:
                    case TokenKind.TempKeyword:
                        CreateTableStatement tableNode;
                        result = CreateTableStatement.TryParseNode(parser, out tableNode);
                        node = tableNode;
                        break;
                    default:
                        result = false;
                        break;
                }
            }

            return result;
        }
    }

    public class CreateTableStatement : CreateStatement
    {
        public NameExpression TableName { get; private set; }
        public bool TempTable { get; private set; }
        public NameExpression TablespaceName { get; private set; }
        public ExpressionNode ExtentSize { get; private set; }
        public ExpressionNode NextSize { get; private set; }

        internal static bool TryParseNode(Parser parser, out CreateTableStatement node)
        {
            node = null;
            bool result = false;
            bool temp = false;

            int tempIndex = -1;
            if(parser.PeekToken(TokenKind.TempKeyword))
            {
                tempIndex = parser.Token.Span.Start;
                temp = true;
                parser.NextToken();
            }

            if (parser.PeekToken(TokenKind.TableKeyword))
            {
                result = true;
                node = new CreateTableStatement();
                node.TempTable = temp;
                if (tempIndex >= 0)
                    node.StartIndex = tempIndex;
                else
                    node.StartIndex = parser.Token.Span.Start;
                parser.NextToken();

                if (parser.PeekToken(TokenKind.IfKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.NotKeyword))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.ExistsKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting \"exists\" keyword in create table statement.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting \"not\" keyword in create table statement.");
                }

                NameExpression nameExpr;
                if (NameExpression.TryParseNode(parser, out nameExpr))
                    node.TableName = nameExpr;
                else
                    parser.ReportSyntaxError("Invalid name found for create table statement.");

                if (parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();
                    CreatedTableColumn tableCol;
                    while(CreatedTableColumn.TryParseNode(parser, out tableCol) && tableCol != null)
                    {
                        node.Children.Add(tableCol.StartIndex, tableCol);
                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                        else
                            break;
                    }

                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expected right-paren in create table statement.");

                    if(parser.PeekToken(TokenKind.WithKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.NoKeyword))
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.LogKeyword))
                                parser.NextToken();
                            else
                                parser.ReportSyntaxError("Expecting \"log\" keyword in create table statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"no\" keyword in create table statement.");
                    }

                    if(parser.PeekToken(TokenKind.InKeyword))
                    {
                        parser.NextToken();
                        if (NameExpression.TryParseNode(parser, out nameExpr))
                            node.TablespaceName = nameExpr;
                        else
                            parser.ReportSyntaxError("Invalid name found for create table statement.");
                    }

                    if(parser.PeekToken(TokenKind.ExtentKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.SizeKeyword))
                        {
                            parser.NextToken();
                            ExpressionNode extSize;
                            if (ExpressionNode.TryGetExpressionNode(parser, out extSize))
                                node.ExtentSize = extSize;
                            else
                                parser.ReportSyntaxError("Invalid expression found for extent size in create table statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"size\" keyword in create table statement.");
                    }

                    if (parser.PeekToken(TokenKind.NextKeyword))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.SizeKeyword))
                        {
                            parser.NextToken();
                            ExpressionNode extSize;
                            if (ExpressionNode.TryGetExpressionNode(parser, out extSize))
                                node.NextSize = extSize;
                            else
                                parser.ReportSyntaxError("Invalid expression found for next size in create table statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"size\" keyword in create table statement.");
                    }

                    if(parser.PeekToken(TokenKind.LockKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.ModeKeyword))
                        {
                            parser.NextToken();
                            switch(parser.PeekToken().Kind)
                            {
                                case TokenKind.PageKeyword:
                                    parser.NextToken();
                                    break;
                                case TokenKind.RowKeyword:
                                    parser.NextToken();
                                    break;
                                default:
                                    parser.ReportSyntaxError("Expecting \"page\" or \"row\" keyword in create table statement.");
                                    break;
                            }
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"mode\" keyword in create table statement.");
                    }
                }
                else
                    parser.ReportSyntaxError("Expected left-paren in create table statement.");
            }
            return result;
        }
    }

    public class CreatedTableColumn : AstNode
    {
        public NameExpression ColumnName { get; private set; }
        public TypeReference DataType { get; private set; }

        public ExpressionNode DefaultValue { get; private set; }
        public bool NotNull { get; private set; }
        public NameExpression ConstraintName { get; private set; }
        public NameExpression ReferencingTableName { get; private set; }
        public List<NameExpression> ReferencingTableColumnNames { get; private set; }

        public static bool TryParseNode(Parser parser, out CreatedTableColumn node)
        {
            node = new CreatedTableColumn();
            node.StartIndex = parser.Token.Span.Start;
            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.PrimaryKeyword:
                case TokenKind.UniqueKeyword:
                case TokenKind.CheckKeyword:
                case TokenKind.ForeignKeyword:
                    parser.ReportSyntaxError("Token not supported at this time for create table statements.");
                    break;
                default:
                    {
                        NameExpression colName;
                        if (NameExpression.TryParseNode(parser, out colName))
                            node.ColumnName = colName;
                        else
                            parser.ReportSyntaxError("Invalid name found for table column in create table statement.");

                        TypeReference dataType;
                        if (TypeReference.TryParseNode(parser, out dataType))
                        {
                            // TODO: should probably ensure that only SQL compatible types are used here...
                            node.DataType = dataType;
                        }
                        else
                            parser.ReportSyntaxError("Invalid data-type found for table column in create table statement.");

                        if(parser.PeekToken(TokenKind.DefaultKeyword))
                        {
                            parser.NextToken();
                            ExpressionNode defVal;
                            if (ExpressionNode.TryGetExpressionNode(parser, out defVal))
                                node.DefaultValue = defVal;
                            else
                                parser.ReportSyntaxError("Invalid expression found for default value in create table statement.");
                        }

                        if(parser.PeekToken(TokenKind.NotKeyword))
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.NullKeyword))
                            {
                                parser.NextToken();
                                node.NotNull = true;
                            }
                            else
                                parser.ReportSyntaxError("Expecting \"null\" keyword in create table statement.");
                        }

                        // TODO: do the other modifiers (primary key, unique, check, references)
                    }
                    break;
            }

            return result;
        }
    }

    public class CreateSequenceStatement : CreateStatement
    {
        public NameExpression SequenceName { get; private set; }

        internal static bool TryParseNode(Parser parser, out CreateSequenceStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.SequenceKeyword))
            {
                result = true;
                node = new CreateSequenceStatement();
                node.StartIndex = parser.Token.Span.Start; // want to get the start of the create token
                parser.NextToken();

                if(parser.PeekToken(TokenKind.IfKeyword))
                {
                    parser.NextToken();
                    if(parser.PeekToken(TokenKind.NotKeyword))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.ExistsKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting \"exists\" keyword in create sequence statement.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting \"not\" keyword in create sequence statement.");
                }

                NameExpression nameExpr;
                if (NameExpression.TryParseNode(parser, out nameExpr))
                    node.SequenceName = nameExpr;
                else
                    parser.ReportSyntaxError("Invalid name found for create sequence statement.");

                // TODO: finish getting the modifiers
            }

            return result;
        }
    }
}
