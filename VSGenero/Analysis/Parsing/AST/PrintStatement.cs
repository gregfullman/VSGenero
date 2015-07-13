using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class PrintStatement : FglStatement
    {
        public List<ExpressionNode> ColumnExpressions { get; private set; }
        public List<ExpressionNode> WhereExpressions { get; private set; }
        public List<ExpressionNode> VariableNames { get; private set; }
        public List<ExpressionNode> Filenames { get; private set; }
        public List<ExpressionNode> OtherExpressions { get; private set; }

        public static bool TryParseNode(IParser parser, out PrintStatement node, ReportBlockNode reportNode)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.PrintKeyword))
            {
                result = true;
                node = new PrintStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.WhereExpressions = new List<ExpressionNode>();
                node.ColumnExpressions = new List<ExpressionNode>();
                node.VariableNames = new List<ExpressionNode>();
                node.Filenames = new List<ExpressionNode>();
                node.OtherExpressions = new List<ExpressionNode>();

                TokenKind nextTokKind = parser.PeekToken().Kind;

                if (ReportStatementFactory.StatementStartKeywords.Contains(nextTokKind) ||
                    GeneroAst.ValidStatementKeywords.Contains(nextTokKind) ||
                    parser.PeekToken(TokenKind.OnKeyword) ||
                   parser.PeekToken(TokenKind.EveryKeyword) ||
                   (parser.PeekToken(TokenKind.FirstKeyword) && parser.PeekToken(TokenKind.PageKeyword, 2)) ||
                   parser.PeekToken(TokenKind.PageKeyword) ||
                   parser.PeekToken(TokenKind.BeforeKeyword) ||
                   parser.PeekToken(TokenKind.AfterKeyword))
                {
                    node.EndIndex = parser.Token.Span.End;
                    return true;
                }


                List<TokenKind> breaks = new List<TokenKind> { TokenKind.Comma };
                while(true)
                {
                    if(parser.PeekToken(TokenKind.GroupKeyword))
                    {
                        parser.NextToken();
                    }

                    switch(parser.PeekToken().Kind)
                    {
                        case TokenKind.PagenoKeyword:
                        case TokenKind.LinenoKeyword:
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.UsingKeyword))
                            {
                                parser.NextToken();
                                if (parser.PeekToken(TokenCategory.StringLiteral))
                                    parser.NextToken();
                            }
                            break;
                        case TokenKind.ColumnKeyword:
                            {
                                parser.NextToken();
                                ExpressionNode colExpr;
                                if (ExpressionNode.TryGetExpressionNode(parser, out colExpr, breaks))
                                    node.ColumnExpressions.Add(colExpr);
                                break;
                            }
                        case TokenKind.CountKeyword:
                        case TokenKind.PercentKeyword:
                            {
                                parser.NextToken();
                                if(parser.PeekToken(TokenKind.LeftParenthesis))
                                {
                                    parser.NextToken();
                                    if(parser.PeekToken(TokenKind.Multiply))
                                    {
                                        parser.NextToken();
                                        if (parser.PeekToken(TokenKind.RightParenthesis))
                                            parser.NextToken();
                                        else
                                            parser.ReportSyntaxError("Expected right-paren in print statement.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Expected '*' in print statement.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected left-paren in print statement.");

                                if(parser.PeekToken(TokenKind.WhereKeyword))
                                {
                                    parser.NextToken();
                                    ExpressionNode whereClause;
                                    if (ExpressionNode.TryGetExpressionNode(parser, out whereClause, breaks))
                                        node.WhereExpressions.Add(whereClause);
                                }

                                if (parser.PeekToken(TokenKind.UsingKeyword))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenCategory.StringLiteral))
                                        parser.NextToken();
                                }
                                break;
                            }
                        case TokenKind.SumKeyword:
                        case TokenKind.MinKeyword:
                        case TokenKind.MaxKeyword:
                        case TokenKind.AvgKeyword:
                            {
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.LeftParenthesis))
                                {
                                    parser.NextToken();
                                    ExpressionNode varName;
                                    if (ExpressionNode.TryGetExpressionNode(parser, out varName))
                                    {
                                        node.VariableNames.Add(varName);
                                        if (parser.PeekToken(TokenKind.RightParenthesis))
                                            parser.NextToken();
                                        else
                                            parser.ReportSyntaxError("Expected right-paren in print statement.");
                                    }
                                    else
                                        parser.ReportSyntaxError("Invalid variable name found in print statement.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected left-paren in print statement.");

                                if (parser.PeekToken(TokenKind.WhereKeyword))
                                {
                                    parser.NextToken();
                                    ExpressionNode whereClause;
                                    if (ExpressionNode.TryGetExpressionNode(parser, out whereClause, breaks))
                                        node.WhereExpressions.Add(whereClause);
                                }

                                if(parser.PeekToken(TokenKind.UsingKeyword))
                                {
                                    parser.NextToken();
                                    if (parser.PeekToken(TokenCategory.StringLiteral))
                                        parser.NextToken();
                                }
                                break;
                            }
                        case TokenKind.FileKeyword:
                            {
                                parser.NextToken();
                                ExpressionNode filename;
                                if (ExpressionNode.TryGetExpressionNode(parser, out filename, breaks))
                                    node.Filenames.Add(filename);
                                break;
                            }
                        default:
                            {
                                ExpressionNode expr;
                                if (ExpressionNode.TryGetExpressionNode(parser, out expr, breaks, new ExpressionParsingOptions
                                {
                                    AllowStarParam = true,
                                    AdditionalExpressionParsers = new ExpressionParser[] { (x) => ReportStatementFactory.ParseAggregateReportFunction(x, reportNode) }
                                }))
                                {
                                    node.OtherExpressions.Add(expr);
                                    if (parser.PeekToken(TokenKind.SpacesKeyword))
                                    {
                                        parser.NextToken();
                                    }
                                    else if (parser.PeekToken(TokenKind.WordwrapKeyword))
                                    {
                                        parser.NextToken();
                                        if (parser.PeekToken(TokenKind.RightKeyword) && parser.PeekToken(TokenKind.MarginKeyword, 2))
                                        {
                                            parser.NextToken();
                                            parser.NextToken();
                                            if(ExpressionNode.TryGetExpressionNode(parser, out expr, breaks))
                                                node.OtherExpressions.Add(expr);
                                            else
                                                parser.ReportSyntaxError("Invalid expression found in print statement.");
                                        }
                                    }
                                }
                                else
                                    parser.ReportSyntaxError("Invalid expression found in print statement.");
                                break;
                            }
                    }

                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();
                    else if(parser.PeekToken(TokenKind.Semicolon))
                    {
                        parser.NextToken();
                        break;
                    }
                    else
                        break;
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
