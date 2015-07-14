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
            switch (tokenKind)
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

        public static bool TryParseSqlStatement(IParser parser, out FglStatement node, out bool matchedBreakSequence, TokenKind limitTo = TokenKind.EndOfFile, List<List<TokenKind>> breakSequences = null)
        {
            matchedBreakSequence = false;
            node = null;
            bool result = false;

            if (limitTo != TokenKind.EndOfFile)
            {
                if (!parser.PeekToken(limitTo))
                    return result;
            }

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.SelectKeyword:
                    {
                        SelectStatement selStmt;
                        if ((result = SelectStatement.TryParseNode(parser, out selStmt, out matchedBreakSequence)))
                        {
                            node = selStmt;
                        }
                        break;
                    }
                case TokenKind.UpdateKeyword:
                    {
                        UpdateStatement updStmt;
                        if ((result = UpdateStatement.TryParseNode(parser, out updStmt)))
                        {
                            node = updStmt;
                        }
                        break;
                    }
                case TokenKind.InsertKeyword:
                    {
                        InsertStatement insStmt;
                        if ((result = InsertStatement.TryParseNode(parser, out insStmt)))
                        {
                            node = insStmt;
                        }
                        break;
                    }
                case TokenKind.DeleteKeyword:
                    {
                        DeleteStatement delStmt;
                        if ((result = DeleteStatement.TryParseNode(parser, out delStmt)))
                        {
                            node = delStmt;
                        }
                        break;
                    }
            }

            if (node != null)
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

        public Dictionary<object, ExpressionNode> Tables { get; private set; }

        public ExpressionNode ConditionalExpression { get; private set; }

        public List<NameExpression> GroupByList { get; private set; }
        public ExpressionNode GroupByCondition { get; private set; }

        public List<ExpressionNode> OrderByList { get; private set; }

        public static bool TryParseNode(IParser parser, out SelectStatement node, out bool matchedBreakSequence, List<List<TokenKind>> breakSequences = null)
        {
            node = null;
            matchedBreakSequence = false;
            bool result = false;

            if (parser.PeekToken(TokenKind.SelectKeyword))
            {
                result = true;
                node = new SelectStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // get the subset clause, if available
                if (parser.PeekToken(TokenKind.SkipKeyword))
                {
                    node.SubsetSkip = true;
                    parser.NextToken();

                    ExpressionNode skipExpr;
                    if (ExpressionNode.TryGetExpressionNode(parser, out skipExpr, new List<TokenKind> 
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
                switch (parser.PeekToken().Kind)
                {
                    case TokenKind.FirstKeyword: node.SubsetType = SelectSubsetType.First; break;
                    case TokenKind.MiddleKeyword: node.SubsetType = SelectSubsetType.Middle; break;
                    case TokenKind.LimitKeyword: node.SubsetType = SelectSubsetType.Limit; break;
                }
                if (node.SubsetType != SelectSubsetType.None)
                {
                    parser.NextToken();
                    string subsetTypeStr = parser.Token.Token.Value.ToString();
                    ExpressionNode skipExpr;
                    if (ExpressionNode.TryGetExpressionNode(parser, out skipExpr, new List<TokenKind> 
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
                if (node.DuplicatesOption != SelectDuplicatesOption.None)
                    parser.NextToken();

                // get the select list
                node.SelectList = new List<ExpressionNode>();
                bool isStarOnly = false;
                if (parser.PeekToken(TokenKind.Multiply))
                {
                    isStarOnly = true;
                    parser.NextToken();
                    node.SelectAll = true;
                    if (parser.PeekToken(TokenKind.Comma))
                    {
                        parser.NextToken();
                        isStarOnly = false;
                    }
                }
                if (!isStarOnly)
                {
                    node.SelectAll = false;
                    while (true)
                    {
                        if (parser.PeekToken(TokenKind.CaseKeyword))
                        {
                            // this is a pretty kludgy way of handling a case statement within a select statement, but for now it will do.
                            parser.NextToken();
                            TokenExpressionNode tokenExpression = new TokenExpressionNode(parser.Token);
                            while (!parser.PeekToken(TokenKind.IntoKeyword) &&
                                  !parser.PeekToken(TokenKind.FromKeyword) &&
                                  !parser.PeekToken(TokenKind.Comma))
                            {
                                parser.NextToken();
                                tokenExpression.AppendExpression(new TokenExpressionNode(parser.Token));
                                if (parser.PeekToken(TokenKind.EndKeyword))
                                {
                                    parser.NextToken();
                                    tokenExpression.AppendExpression(new TokenExpressionNode(parser.Token));
                                    break;
                                }
                            }
                            node.SelectList.Add(tokenExpression);
                        }
                        else
                        {
                            ExpressionNode name;
                            if (ExpressionNode.TryGetExpressionNode(parser, out name, new List<TokenKind>
                                {
                                    TokenKind.Comma, TokenKind.IntoKeyword, TokenKind.FromKeyword
                                }, new ExpressionParsingOptions { AllowStarParam = true, AllowAnythingForFunctionParams = true }))
                            {
                                node.SelectList.Add(name);
                                // TODO: there may be other sql select functions that should be allowed in the select list...
                                if (name is FunctionCallExpressionNode &&
                                   (name as FunctionCallExpressionNode).Function.Name.Equals("top", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }
                        }
                        if (!parser.PeekToken(TokenKind.Comma))
                        {
                            if (!parser.PeekToken(TokenKind.IntoKeyword) &&
                               !parser.PeekToken(TokenKind.FromKeyword))
                            {
                                // we may have a column alias
                                ExpressionNode alias;
                                if (ExpressionNode.TryGetExpressionNode(parser, out alias, new List<TokenKind>
                                {
                                    TokenKind.Comma, TokenKind.IntoKeyword, TokenKind.FromKeyword
                                }))
                                {
                                    if (!parser.PeekToken(TokenKind.Comma))
                                        break;
                                }
                            }
                            else
                                break;
                        }
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.IntoKeyword))
                            break;
                    }
                }

                node.IntoVariables = new List<NameExpression>();
                if (parser.PeekToken(TokenKind.IntoKeyword))
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

                if (!parser.PeekToken(TokenKind.FromKeyword))
                {
                    parser.ReportSyntaxError("Select statement is missing \"from\" keyword.");
                }
                else
                {
                    parser.NextToken();
                }

                node.Tables = new Dictionary<object, ExpressionNode>();
                // get the tables
                ExpressionNode tableName;
                ExpressionNode aliasName;
                while (true)
                {
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.SelectKeyword))
                        {
                            SelectStatement subSelStmt;
                            bool dummy;
                            if (SelectStatement.TryParseNode(parser, out subSelStmt, out dummy))
                            {
                                node.Tables.Add(subSelStmt, null);
                                if (!parser.PeekToken(TokenKind.Comma))
                                {
                                    var peek = parser.PeekToken();
                                    var cat = Tokenizer.GetTokenInfo(peek).Category;
                                    if ((cat == TokenCategory.Keyword ||
                                       cat == TokenCategory.Identifier) &&
                                        (!GeneroAst.ValidStatementKeywords.Contains(peek.Kind) &&
                                        peek.Kind != TokenKind.WhereKeyword &&
                                        peek.Kind != TokenKind.GroupKeyword &&
                                        peek.Kind != TokenKind.OrderKeyword))
                                    {
                                        if (!ExpressionNode.TryGetExpressionNode(parser, out aliasName, null, new ExpressionParsingOptions { AllowAnythingForFunctionParams = true }))
                                            parser.ReportSyntaxError("Invalid table alias name found.");
                                        else
                                            node.Tables[subSelStmt] = aliasName;

                                        if (!parser.PeekToken(TokenKind.Comma))
                                            break;
                                        else
                                            parser.NextToken();
                                    }
                                    else
                                        break;
                                }
                                else
                                    parser.NextToken();
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Invalid table name found in select statement.");
                            break;
                        }
                    }
                    else
                    {
                        if (ExpressionNode.TryGetExpressionNode(parser, out tableName, new List<TokenKind>
                        {
                            TokenKind.Comma
                        }))
                        {
                            node.Tables.Add(tableName, null);
                            if (!parser.PeekToken(TokenKind.Comma))
                            {
                                var peek = parser.PeekToken();
                                var cat = Tokenizer.GetTokenInfo(peek).Category;
                                if ((cat == TokenCategory.Keyword ||
                                   cat == TokenCategory.Identifier) &&
                                    (!GeneroAst.ValidStatementKeywords.Contains(peek.Kind) &&
                                    peek.Kind != TokenKind.WhereKeyword &&
                                    peek.Kind != TokenKind.GroupKeyword &&
                                    peek.Kind != TokenKind.OrderKeyword))
                                {
                                    if (!ExpressionNode.TryGetExpressionNode(parser, out aliasName))
                                        parser.ReportSyntaxError("Invalid table alias name found.");
                                    else
                                        node.Tables[tableName] = aliasName;

                                    if (!parser.PeekToken(TokenKind.Comma))
                                        break;
                                    else
                                        parser.NextToken();
                                }
                                else
                                    break;
                            }
                            else
                                parser.NextToken();
                        }
                        else
                        {
                            parser.ReportSyntaxError("Invalid table name found in select statement.");
                            break;
                        }
                    }
                }

                // get the where clause
                if (parser.PeekToken(TokenKind.WhereKeyword))
                {
                    parser.NextToken();
                    ExpressionNode conditionalExpr;
                    if (ExpressionNode.TryGetExpressionNode(parser, out conditionalExpr, GeneroAst.ValidStatementKeywords.Union(new List<TokenKind>
                        {
                            TokenKind.GroupKeyword, TokenKind.OrderKeyword
                        }).ToList(), new ExpressionParsingOptions { AllowStarParam = true, AllowAnythingForFunctionParams = true, AllowQuestionMark = true }))
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
                if (parser.PeekToken(TokenKind.GroupKeyword) && parser.PeekToken(TokenKind.ByKeyword, 2))
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

                    if (parser.PeekToken(TokenKind.HavingKeyword))
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
                if (parser.PeekToken(TokenKind.OrderKeyword) && parser.PeekToken(TokenKind.ByKeyword, 2))
                {
                    parser.NextToken();
                    parser.NextToken();
                    ExpressionNode colName;
                    while (ExpressionNode.TryGetExpressionNode(parser, out colName, GeneroAst.ValidStatementKeywords.Union(new List<TokenKind> { TokenKind.Comma }).ToList()))
                    {
                        node.OrderByList.Add(colName);
                        if(parser.PeekToken(TokenKind.AscKeyword) || 
                            parser.PeekToken(TokenKind.DescKeyword))
                        {
                            parser.NextToken();
                        }
                        if (parser.PeekToken(TokenKind.Comma))
                        {
                            parser.NextToken();
                        }
                        else
                            break;
                    }
                }

                if (parser.PeekToken(TokenKind.ForKeyword) &&
                   parser.PeekToken(TokenKind.UpdateKeyword, 2))
                {
                    parser.NextToken();
                    parser.NextToken();
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

        public static bool TryParseNode(IParser parser, out InsertStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.InsertKeyword))
            {
                result = true;
                node = new InsertStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.Values = new List<ExpressionNode>();

                if (parser.PeekToken(TokenKind.IntoKeyword))
                {
                    parser.NextToken();

                    NameExpression tableExpr;
                    if (NameExpression.TryParseNode(parser, out tableExpr))
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

                    if (parser.PeekToken(TokenKind.ValuesKeyword))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
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
                        if (SelectStatement.TryParseNode(parser, out selStmt, out dummy) && selStmt != null)
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

        public static bool TryParseNode(IParser parser, out DeleteStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.DeleteKeyword))
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
        public enum SyntaxType
        {
            None,
            Syntax1,
            Syntax2,
            Syntax3,
            Syntax4
        }

        public NameExpression TableSpec { get; private set; }
        public ExpressionNode ConditionalExpression { get; private set; }
        public NameExpression CursorName { get; private set; }

        public SyntaxType Type { get; private set; }
        public List<NameExpression> ColumnList { get; private set; }
        public bool WholeTable { get; private set; }
        public NameExpression WholeTableName { get; private set; }
        public List<object> Variables { get; set; }

        /// <summary>
        /// Used for syntax 4
        /// </summary>
        public NameExpression RecordName { get; private set; }

        public static bool TryParseNode(IParser parser, out UpdateStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.UpdateKeyword))
            {
                result = true;
                node = new UpdateStatement();
                node.ColumnList = new List<NameExpression>();
                node.Variables = new List<object>();
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
                    // there are various constructs that can be used.
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        // this can handle syntax 2 or 4
                        // Collect the column list in either case
                        parser.NextToken();
                        NameExpression colName;
                        while (NameExpression.TryParseNode(parser, out colName, TokenKind.Comma))
                        {
                            node.ColumnList.Add(colName);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }
                        if (parser.PeekToken(TokenKind.RightParenthesis))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.Equals))
                            {
                                parser.NextToken();

                                if (parser.PeekToken(TokenKind.LeftParenthesis))
                                {
                                    // we have syntax 2, let the code below get the variable list
                                    node.Type = SyntaxType.Syntax2;
                                    // continued below...
                                }
                                else if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                                {
                                    node.Type = SyntaxType.Syntax4;
                                    if (NameExpression.TryParseNode(parser, out colName) && colName.Name.EndsWith(".*"))
                                    {
                                        node.RecordName = colName;
                                    }
                                    else
                                        parser.ReportSyntaxError("Invalid record name found in update statement.");
                                }
                                else
                                    parser.ReportSyntaxError("Invalid form of update statement found.");
                            }
                            else
                                parser.ReportSyntaxError("Expected '=' token in update statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expected right-paren token in update statement.");
                    }
                    else if (parser.PeekToken(TokenKind.Multiply))
                    {
                        // this can handle syntax 3 or 4
                        parser.NextToken();
                        node.WholeTable = true;
                        node.Type = SyntaxType.Syntax3;
                        if (parser.PeekToken(TokenKind.Equals))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.LeftParenthesis))
                            {
                                node.Type = SyntaxType.Syntax3;
                                // continued below...
                            }
                            else if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                            {
                                node.Type = SyntaxType.Syntax4;
                                NameExpression colName;
                                if (NameExpression.TryParseNode(parser, out colName) && colName.Name.EndsWith(".*"))
                                {
                                    node.RecordName = colName;
                                }
                                else
                                    parser.ReportSyntaxError("Invalid record name found in update statement.");
                            }
                            else
                                parser.ReportSyntaxError("Invalid form of update statement found.");
                        }
                        else
                            parser.ReportSyntaxError("Expected '=' token in update statement.");
                    }
                    else if (parser.PeekToken(TokenCategory.Identifier) ||
                            parser.PeekToken(TokenCategory.Keyword))
                    {
                        // this can handle syntax 1, 3, and 4
                        NameExpression nameExpr;
                        if (NameExpression.TryParseNode(parser, out nameExpr))
                        {
                            if (nameExpr.Name.EndsWith(".*"))
                            {
                                node.WholeTableName = nameExpr;
                                if (parser.PeekToken(TokenKind.Equals))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                                    {
                                        node.Type = SyntaxType.Syntax3;
                                        // continued below...
                                    }
                                    else if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                                    {
                                        node.Type = SyntaxType.Syntax4;
                                        NameExpression colName;
                                        if (NameExpression.TryParseNode(parser, out colName) && colName.Name.EndsWith(".*"))
                                        {
                                            node.RecordName = colName;
                                        }
                                        else
                                            parser.ReportSyntaxError("Invalid record name found in update statement.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Invalid form of update statement found.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected '=' token in update statement.");
                            }
                            else if (parser.PeekToken(TokenKind.Equals))
                            {
                                parser.NextToken();
                                node.Type = SyntaxType.Syntax1;

                                // continue parsing here, breaking the column names into the column list, and the variables/expression into the variable list
                                node.ColumnList.Add(nameExpr);

                                ExpressionNode expr;
                                if (ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma }, new ExpressionParsingOptions { AllowNestedSelectStatement = true }))
                                    node.Variables.Add(expr);
                                else
                                    parser.ReportSyntaxError("Invalid expression found in update statement.");

                                while (parser.PeekToken(TokenKind.Comma))
                                {
                                    parser.NextToken();
                                    if (NameExpression.TryParseNode(parser, out nameExpr))
                                    {
                                        node.ColumnList.Add(nameExpr);
                                        if (parser.PeekToken(TokenKind.Equals))
                                        {
                                            parser.NextToken();
                                            if (ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma }, new ExpressionParsingOptions { AllowNestedSelectStatement = true }))
                                                node.Variables.Add(expr);
                                            else
                                                parser.ReportSyntaxError("Invalid expression found in update statement.");
                                        }
                                        else
                                            parser.ReportSyntaxError("Expected '=' token in update statement.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Invalid name expression found in update statement.");
                                }
                            }
                        }
                        else
                            parser.ReportSyntaxError("Invalid name expression found in update statement.");
                    }
                    else
                        parser.ReportSyntaxError("Unexpected token found in update statement.");

                    if (node.Type == SyntaxType.Syntax2 || node.Type == SyntaxType.Syntax3)
                    {
                        // get the paren-wrapped list of variables/expressions
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            // for now, we'll just use ExpressionNode to get the variables/expressions. Not sure what else can show up here...
                            ExpressionNode expr;
                            while (ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                            {
                                node.Variables.Add(expr);
                                if (!parser.PeekToken(TokenKind.Comma))
                                    break;
                                parser.NextToken();
                            }
                            if (parser.PeekToken(TokenKind.RightParenthesis))
                                parser.NextToken();
                            else
                                parser.ReportSyntaxError("Expected right-paren in update statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expected left-paren in update statement.");
                    }

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
                            if (ExpressionNode.TryGetExpressionNode(parser, out conditionalExpr, GeneroAst.ValidStatementKeywords.ToList(), new ExpressionParsingOptions { AllowStarParam = true, AllowAnythingForFunctionParams = true }))
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

            if (parser.PeekToken(TokenKind.FlushKeyword))
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
