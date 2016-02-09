/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
