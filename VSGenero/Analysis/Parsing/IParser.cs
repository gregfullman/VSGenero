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
        LocationInfo TokenLocation { get; }

        void Reset();
        void ReportSyntaxError(string message, Severity severity = Severity.Error);
        void ReportSyntaxError(int start, int end, string message, Severity severity = Severity.Error);
        void ReportSyntaxError(int start, int end, string message, int errorCode, Severity severity = Severity.Error);

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
