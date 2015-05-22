using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class LoadStatement : FglStatement
    {
        public ExpressionNode Filename { get; private set; }
        public ExpressionNode DelimiterChar { get; private set; }
        public NameExpression TableName { get; private set; }
        public List<NameExpression> ColumnNames { get; private set; }
        public ExpressionNode InsertString { get; private set; }

        public static bool TryParseNode(Parser parser, out LoadStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.LoadKeyword))
            {
                result = true;
                node = new LoadStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.ColumnNames = new List<NameExpression>();

                if (parser.PeekToken(TokenKind.FromKeyword))
                    parser.NextToken();
                else
                    parser.ReportSyntaxError("Expected \"from\" keyword in load statement.");

                ExpressionNode filenameExpr;
                if (ExpressionNode.TryGetExpressionNode(parser, out filenameExpr))
                    node.Filename = filenameExpr;
                else
                    parser.ReportSyntaxError("Invalid filename found in load statement.");

                if(parser.PeekToken(TokenKind.DelimiterKeyword))
                {
                    parser.NextToken();
                    if (ExpressionNode.TryGetExpressionNode(parser, out filenameExpr))
                        node.DelimiterChar = filenameExpr;
                    else
                        parser.ReportSyntaxError("Invalid delimiter character found in load statement.");
                }

                if(parser.PeekToken(TokenKind.InsertKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.IntoKeyword))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expected \"into\" keyword in load statement.");

                    NameExpression tableName;
                    if (NameExpression.TryParseNode(parser, out tableName))
                        node.TableName = tableName;
                    else
                        parser.ReportSyntaxError("Invalid table name found in load statement.");

                    if(parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();

                        while(NameExpression.TryParseNode(parser, out tableName, TokenKind.Comma))
                        {
                            node.ColumnNames.Add(tableName);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                        }

                        if (parser.PeekToken(TokenKind.RightParenthesis))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expected right-paren in load statement.");
                    }
                }
                else
                {
                    if (ExpressionNode.TryGetExpressionNode(parser, out filenameExpr, GeneroAst.ValidStatementKeywords.ToList()))
                        node.InsertString = filenameExpr;
                    else
                        parser.ReportSyntaxError("Invalid insert string found in load statement.");
                }
            }

            return result;
        }
    }
}
