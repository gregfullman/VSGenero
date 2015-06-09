using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class SqlStatementFactory : FglStatement
    {
        public static bool IsValidStatementStart(TokenKind tokenKind)
        {
            switch(tokenKind)
            {
                case TokenKind.SelectKeyword:
                case TokenKind.UpdateKeyword:
                case TokenKind.InsertKeyword:
                case TokenKind.DeleteKeyword:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryParseSqlStatement(Parser parser, out FglStatement node, out bool matchedBreakSequence, TokenKind limitTo = TokenKind.EndOfFile, List<List<TokenKind>> breakSequences = null)
        {
            matchedBreakSequence = false;
            node = null;
            bool result = false;

            if(limitTo != TokenKind.EndOfFile)
            {
                if(!parser.PeekToken(limitTo))
                    return result;
            }

            switch(parser.PeekToken().Kind)
            {
                case TokenKind.SelectKeyword:
                    {
                        SelectStatement selStmt;
                        if((result = SelectStatement.TryParseNode(parser, out selStmt, out matchedBreakSequence)))
                        {
                            node = selStmt;
                        }
                        break;
                    }
                case TokenKind.UpdateKeyword:
                    {
                        UpdateStatement updStmt;
                        if((result = UpdateStatement.TryParseNode(parser, out updStmt)))
                        {
                            node = updStmt;
                        }
                        break;
                    }
                case TokenKind.InsertKeyword:
                    {
                        InsertStatement insStmt;
                        if((result = InsertStatement.TryParseNode(parser, out insStmt)))
                        {
                            node = insStmt;
                        }
                        break;
                    }
                case TokenKind.DeleteKeyword:
                    {
                        DeleteStatement delStmt;
                        if((result = DeleteStatement.TryParseNode(parser, out delStmt)))
                        {
                            node = delStmt;
                        }
                        break;
                    }
            }

            if(node != null)
            {
                result = true;
            }

            return result;
        }
    }

    #region Select Statement

    public enum SelectSubsetType
    {
        None,
        First,
        Middle,
        Limit
    }

    public enum SelectDuplicatesOption
    {
        None,
        All,
        Distinct,
        Unique
    }

    public class SelectStatement : FglStatement
    {
        public List<SelectStatement> UnionedStatements { get; private set; }

        public bool SubsetSkip { get; private set; }
        public ExpressionNode SubsetSkipNum { get; private set; }
        public SelectSubsetType SubsetType { get; private set; }
        public ExpressionNode SubsetTypeNum { get; private set; }

        public SelectDuplicatesOption DuplicatesOption { get; private set; }

        public bool SelectAll { get; private set; }
        public List<ExpressionNode> SelectList { get; private set; }

        public List<NameExpression> IntoVariables { get; private set; }

        public List<ExpressionNode> Tables { get; private set; }

        public ExpressionNode ConditionalExpression { get; private set; }

        public List<NameExpression> GroupByList { get; private set; }
        public ExpressionNode GroupByCondition { get; private set;}

        public List<ExpressionNode> OrderByList { get; private set; }

        public static bool TryParseNode(Parser parser, out SelectStatement node, out bool matchedBreakSequence, List<List<TokenKind>> breakSequences = null)
        {
            node = null;
            matchedBreakSequence = false;
            bool result = false;

            if(parser.PeekToken(TokenKind.SelectKeyword))
            {
                result = true;
                node = new SelectStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // get the subset clause, if available
                if(parser.PeekToken(TokenKind.SkipKeyword))
                {
                    node.SubsetSkip = true;
                    parser.NextToken();

                    ExpressionNode skipExpr;
                    if(ExpressionNode.TryGetExpressionNode(parser, out skipExpr, new List<TokenKind> 
                    { 
                        TokenKind.FirstKeyword, TokenKind.MiddleKeyword, TokenKind.LimitKeyword,
                        TokenKind.AllKeyword, TokenKind.DistinctKeyword, TokenKind.UniqueKeyword,
                        TokenKind.Multiply, TokenKind.IntoKeyword, TokenKind.FromKeyword
                    }))
                    {
                        node.SubsetSkipNum = skipExpr;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Number expected in subset-clause after \"skip\".");
                    }
                }

                node.SubsetType = SelectSubsetType.None;
                switch(parser.PeekToken().Kind)
                {
                    case TokenKind.FirstKeyword: node.SubsetType = SelectSubsetType.First; break;
                    case TokenKind.MiddleKeyword: node.SubsetType = SelectSubsetType.Middle; break;
                    case TokenKind.LimitKeyword: node.SubsetType = SelectSubsetType.Limit; break;
                }
                if(node.SubsetType != SelectSubsetType.None)
                {
                    parser.NextToken();
                    string subsetTypeStr = parser.Token.Token.Value.ToString();
                    ExpressionNode skipExpr;
                    if(ExpressionNode.TryGetExpressionNode(parser, out skipExpr, new List<TokenKind> 
                    { 
                        TokenKind.AllKeyword, TokenKind.DistinctKeyword, TokenKind.UniqueKeyword,
                        TokenKind.Multiply, TokenKind.IntoKeyword, TokenKind.FromKeyword
                    }))
                    {
                        node.SubsetTypeNum = skipExpr;
                    }
                    else
                    {
                        parser.ReportSyntaxError(string.Format("Number expected in subset-clause after \"{0}\".", subsetTypeStr));
                    }
                }

                node.DuplicatesOption = SelectDuplicatesOption.None;
                switch (parser.PeekToken().Kind)
                {
                    case TokenKind.AllKeyword: node.DuplicatesOption = SelectDuplicatesOption.All; break;
                    case TokenKind.DistinctKeyword: node.DuplicatesOption = SelectDuplicatesOption.Distinct; break;
                    case TokenKind.UniqueKeyword: node.DuplicatesOption = SelectDuplicatesOption.Unique; break;
                }

                // get the select list
                node.SelectList = new List<ExpressionNode>();
                if(parser.PeekToken(TokenKind.Multiply))
                {
                    parser.NextToken();
                    node.SelectAll = true;
                }
                else
                {
                    node.SelectAll = false;
                    ExpressionNode name;
                    while (ExpressionNode.TryGetExpressionNode(parser, out name, new List<TokenKind>
                        {
                            TokenKind.Comma, TokenKind.IntoKeyword, TokenKind.FromKeyword
                        }, true))
                    {
                        node.SelectList.Add(name);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }
                }

                node.IntoVariables = new List<NameExpression>();
                if(parser.PeekToken(TokenKind.IntoKeyword))
                {
                    parser.NextToken();
                    NameExpression name;
                    while (NameExpression.TryParseNode(parser, out name))
                    {
                        node.IntoVariables.Add(name);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }
                }

                if(!parser.PeekToken(TokenKind.FromKeyword))
                {
                    parser.ReportSyntaxError("Select statement is missing \"from\" keyword.");
                }
                else
                {
                    parser.NextToken();
                }

                node.Tables = new List<ExpressionNode>();
                // get the tables
                ExpressionNode tableName;
                while (ExpressionNode.TryGetExpressionNode(parser, out tableName, new List<TokenKind>
                        {
                            TokenKind.Comma
                        }))
                {
                    node.Tables.Add(tableName);
                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }

                // get the where clause
                if(parser.PeekToken(TokenKind.WhereKeyword))
                {
                    parser.NextToken();
                    ExpressionNode conditionalExpr;
                    if(ExpressionNode.TryGetExpressionNode(parser, out conditionalExpr, GeneroAst.ValidStatementKeywords.Union(new List<TokenKind>
                        {
                            TokenKind.GroupKeyword, TokenKind.OrderKeyword
                        }).ToList()))
                    {
                        node.ConditionalExpression = conditionalExpr;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid conditional expression found in select statement.");
                    }
                }

                // get group-by clause
                node.GroupByList = new List<NameExpression>();
                if(parser.PeekToken(TokenKind.GroupKeyword) && parser.PeekToken(TokenKind.ByKeyword, 2))
                {
                    parser.NextToken();
                    parser.NextToken();
                    NameExpression name;
                    while (NameExpression.TryParseNode(parser, out name))
                    {
                        node.GroupByList.Add(name);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }

                    if(parser.PeekToken(TokenKind.HavingKeyword))
                    {
                        parser.NextToken();
                        ExpressionNode conditionalExpr;
                        if (ExpressionNode.TryGetExpressionNode(parser, out conditionalExpr, GeneroAst.ValidStatementKeywords.Union(new List<TokenKind>
                        {
                            TokenKind.OrderKeyword
                        }).ToList()))
                        {
                            node.GroupByCondition = conditionalExpr;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Invalid conditional expression found in select statement's group-by clause.");
                        }
                    }
                }

                node.OrderByList = new List<ExpressionNode>();
                if(parser.PeekToken(TokenKind.OrderKeyword) && parser.PeekToken(TokenKind.ByKeyword, 2))
                {
                    parser.NextToken();
                    parser.NextToken();
                    ExpressionNode colName;
                    while (ExpressionNode.TryGetExpressionNode(parser, out colName, GeneroAst.ValidStatementKeywords.Union(new List<TokenKind> { TokenKind.Comma }).ToList()))
                    {
                        node.OrderByList.Add(colName);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    #endregion

    #region Insert Statement

    public class InsertStatement : FglStatement
    {
        public NameExpression TableSpec { get; private set; }
        public List<NameExpression> ColumnsSpec { get; private set; }
        public bool UsesSelectStmt { get; private set; }
        public List<ExpressionNode> Values { get; private set; }

        public static bool TryParseNode(Parser parser, out InsertStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.InsertKeyword))
            {
                result = true;
                node = new InsertStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.Values = new List<ExpressionNode>();

                if(parser.PeekToken(TokenKind.IntoKeyword))
                {
                    parser.NextToken();

                    NameExpression tableExpr;
                    if(NameExpression.TryParseNode(parser, out tableExpr))
                    {
                        node.TableSpec = tableExpr;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid table name found in insert statement.");
                    }

                    node.ColumnsSpec = new List<NameExpression>();
                    // get the column list
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();
                        NameExpression name;
                        while (NameExpression.TryParseNode(parser, out name))
                        {
                            node.ColumnsSpec.Add(name);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Column list missing right-paren in insert statement.");
                    }

                    if(parser.PeekToken(TokenKind.ValuesKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.LeftParenthesis))
                            parser.NextToken();

                        ExpressionNode expr;
                        while (ExpressionNode.TryGetExpressionNode(parser, out expr, GeneroAst.ValidStatementKeywords.Union(new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }).ToList()))
                        {
                            node.Values.Add(expr);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                    }
                    else
                    {
                        node.UsesSelectStmt = true;
                        SelectStatement selStmt;
                        bool dummy = false;
                        if (SelectStatement.TryParseNode(parser, out selStmt, out dummy))
                            node.Children.Add(selStmt.StartIndex, selStmt);
                        else
                            parser.ReportSyntaxError("Expecting select statement in insert statement.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("An insert keyword must be followed by \"into\".");
                }
                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    #endregion

    #region Delete Statement

    public class DeleteStatement : FglStatement
    {
        public NameExpression TableSpec { get; private set; }
        public ExpressionNode ConditionalExpression { get; private set; }
        public NameExpression CursorName { get; private set; }

        public static bool TryParseNode(Parser parser, out DeleteStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.DeleteKeyword))
            {
                result = true;
                node = new DeleteStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if (parser.PeekToken(TokenKind.FromKeyword))
                {
                    parser.NextToken();

                    NameExpression tableExpr;
                    if (NameExpression.TryParseNode(parser, out tableExpr))
                        node.TableSpec = tableExpr;
                    else
                        parser.ReportSyntaxError("Invalid table name found in delete statement.");

                    if (parser.PeekToken(TokenKind.WhereKeyword))
                    {
                        parser.NextToken();

                        if(parser.PeekToken(TokenKind.CurrentKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.OfKeyword))
                            {
                                parser.NextToken();
                                NameExpression cursorName;
                                if (NameExpression.TryParseNode(parser, out cursorName))
                                    node.CursorName = cursorName;
                                else
                                    parser.ReportSyntaxError("Invalid cursor name found in delete statement.");
                            }
                            else
                                parser.ReportSyntaxError("Expecting \"of\" keyword in delete statement.");
                        }
                        else
                        {
                            ExpressionNode conditionalExpr;
                            if (ExpressionNode.TryGetExpressionNode(parser, out conditionalExpr, GeneroAst.ValidStatementKeywords.ToList()))
                                node.ConditionalExpression = conditionalExpr;
                            else
                                parser.ReportSyntaxError("Invalid conditional expression found in delete statement.");
                        }
                    }
                }
                else
                    parser.ReportSyntaxError("Expecting \"from\" keyword in delete statement.");

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    #endregion

    #region Update Statement

    public class UpdateStatement : FglStatement
    {
        public NameExpression TableSpec { get; private set; }
        public ExpressionNode SetExpression { get; private set; }
        public ExpressionNode ConditionalExpression { get; private set; }
        public NameExpression CursorName { get; private set; }

        public static bool TryParseNode(Parser parser, out UpdateStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.UpdateKeyword))
            {
                result = true;
                node = new UpdateStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression tableExpr;
                if (NameExpression.TryParseNode(parser, out tableExpr))
                    node.TableSpec = tableExpr;
                else
                    parser.ReportSyntaxError("Invalid table name found in update statement.");

                if (parser.PeekToken(TokenKind.SetKeyword))
                {
                    parser.NextToken();
                    ExpressionNode setExpr;
                    if (ExpressionNode.TryGetExpressionNode(parser, out setExpr, GeneroAst.ValidStatementKeywords.Union(new[] { TokenKind.WhereKeyword }).ToList()))
                        node.SetExpression = setExpr;
                    else
                        parser.ReportSyntaxError("Invalid expression found in update statement.");

                    if (parser.PeekToken(TokenKind.WhereKeyword))
                    {
                        parser.NextToken();

                        if (parser.PeekToken(TokenKind.CurrentKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.OfKeyword))
                            {
                                parser.NextToken();
                                NameExpression cursorName;
                                if (NameExpression.TryParseNode(parser, out cursorName))
                                    node.CursorName = cursorName;
                                else
                                    parser.ReportSyntaxError("Invalid cursor name found in update statement.");
                            }
                            else
                                parser.ReportSyntaxError("Expecting \"of\" keyword in update statement.");
                        }
                        else
                        {
                            ExpressionNode conditionalExpr;
                            if (ExpressionNode.TryGetExpressionNode(parser, out conditionalExpr, GeneroAst.ValidStatementKeywords.ToList()))
                                node.ConditionalExpression = conditionalExpr;
                            else
                                parser.ReportSyntaxError("Invalid conditional expression found in update statement.");
                        }
                    }
                }
                else
                    parser.ReportSyntaxError("Expecting \"set\" keyword in update statement.");

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    #endregion

    #region Other Statements

    public class AlterStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out AlterStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.AlterKeyword))
            {
                result = true;
                node = new AlterStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // TODO: will get back to this when needed...
            }

            return result;
        }
    }

    public class DropStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out DropStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.DropKeyword))
            {
                result = true;
                node = new DropStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // TODO: will get back to this when needed...
            }

            return result;
        }
    }

    public class RenameStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out RenameStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.RenameKeyword))
            {
                result = true;
                node = new RenameStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // TODO: will get back to this when needed...
            }

            return result;
        }
    }

    #endregion

    #region Flush Statement

    public class FlushStatement : FglStatement
    {
        public NameExpression CursorId { get; private set; }

        public static bool TryParseNode(Parser parser, out FlushStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.FlushKeyword))
            {
                result = true;
                node = new FlushStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                NameExpression cid;
                if (NameExpression.TryParseNode(parser, out cid))
                    node.CursorId = cid;
                else
                    parser.ReportSyntaxError("Invalid insert cursor identifier found in flush statement.");

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    #endregion
}
