using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class SqlBlockNode : AstNode
    {
        public static bool TryParseSqlNode(Parser parser, out SqlBlockNode node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.SqlKeyword))
            {
                result = true;
                node = new SqlBlockNode();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                List<List<TokenKind>> breakSequences = new List<List<TokenKind>>() 
                    { 
                        new List<TokenKind> { TokenKind.EndKeyword, TokenKind.MainKeyword }
                    };

                SqlStatement sqlStmt;
                bool matchedBreakSequence = false;
                if (SqlStatement.TryParseNode(parser, out sqlStmt, out matchedBreakSequence, TokenKind.EndOfFile, breakSequences))
                {
                    node.Children.Add(sqlStmt.StartIndex, sqlStmt);
                }
                else
                {
                    parser.ReportSyntaxError("No SQL statement found within the SQL block.");
                }

                if (!parser.PeekToken(TokenKind.EndOfFile))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.SqlKeyword))
                    {
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                        node.IsComplete = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of SQL block.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Unexpected end of SQL block.");
                }
            }

            return result;
        }
    }
}
