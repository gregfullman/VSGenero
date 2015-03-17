using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.Analysis.Parsing
{
    public interface IParser
    {
        ErrorSink ErrorSink { get; set; }
        TokenWithSpan Token { get; }
        Tokenizer Tokenizer { get; }

        void Reset();
        void ReportSyntaxError(string message);
        void ReportSyntaxError(int start, int end, string message);
        void ReportSyntaxError(int start, int end, string message, int errorCode);

        GeneroAst ParseFile();
        Token NextToken();
        Token PeekToken(uint aheadBy = 1);
        TokenWithSpan PeekTokenWithSpan(uint aheadBy = 1);
        bool PeekToken(TokenKind kind, uint aheadBy = 1);
        bool PeekToken(Token check, uint aheadBy = 1);
        bool PeekToken(TokenCategory category, uint aheadBy = 1);
        bool Eat(TokenKind kind);
        bool MaybeEat(TokenKind kind);
    }
}
