using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// TRY
    ///     instruction
    ///     [...]
    /// CATCH
    ///     instruction
    ///     [...]
    /// END TRY
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Exceptions_007.html
    /// </summary>
    public class TryCatchStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out TryCatchStatement defNode,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.TryKeyword))
            {
                result = true;
                defNode = new TryCatchStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                TryBlock tryBlock;
                if(TryBlock.TryParseNode(parser, out tryBlock, prepStatementResolver, prepStatementBinder))
                {
                    defNode.Children.Add(tryBlock.StartIndex, tryBlock);
                }

                if(parser.PeekToken(TokenKind.CatchKeyword))
                {
                    parser.NextToken();
                    CatchBlock catchBlock;
                    if(CatchBlock.TryParseNode(parser, out catchBlock, prepStatementResolver, prepStatementBinder))
                    {
                        defNode.Children.Add(catchBlock.StartIndex, catchBlock);
                    }
                }

                if(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.TryKeyword, 2))
                {
                    parser.NextToken();
                    parser.NextToken();
                }
                else
                {
                    parser.ReportSyntaxError("Invalid end of try-catch block found.");
                }
                defNode.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }

    public class TryBlock : AstNode
    {
        public static bool TryParseNode(Parser parser, out TryBlock node,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            node = new TryBlock();
            node.StartIndex = parser.Token.Span.Start;
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                   !parser.PeekToken(TokenKind.CatchKeyword) &&
                   !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.TryKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, prepStatementResolver, prepStatementBinder))
                {
                    AstNode stmtNode = statement as AstNode;
                    node.Children.Add(stmtNode.StartIndex, stmtNode);
                }
                else
                {
                    parser.NextToken();
                }
            }
            node.EndIndex = parser.Token.Span.End;
            return true;
        }
    }

    public class CatchBlock : AstNode
    {
        public static bool TryParseNode(Parser parser, out CatchBlock node,
                                 Func<string, PrepareStatement> prepStatementResolver = null,
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            node = new CatchBlock();
            node.StartIndex = parser.Token.Span.Start;
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                   !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.TryKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, prepStatementResolver, prepStatementBinder))
                {
                    AstNode stmtNode = statement as AstNode;
                    node.Children.Add(stmtNode.StartIndex, stmtNode);
                }
                else
                {
                    parser.NextToken();
                }
            }
            node.EndIndex = parser.Token.Span.End;
            return true;
        }
    }
}
