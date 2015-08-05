/* ****************************************************************************
*
* Copyright (c) Microsoft Corporation. 
*
* This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
* copy of the license can be found in the License.html file at the root of this distribution. If 
* you cannot locate the Apache License, Version 2.0, please send an email to 
* vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.Analysis.Parsing
{
    public class StubErrorSink : ErrorSink
    {
        public int ErrorCount { get; set; }

        public override void Add(string message, int[] lineLocations, int startIndex, int endIndex, int errorCode, Severity severity)
        {
            ErrorCount++;
        }
    }

    public class TokenListParser : IParser
    {
        private readonly List<TokenWithSpan> _tokenList;
        private int _currIndex;

        public TokenListParser(List<TokenWithSpan> tokenList)
        {
            _tokenList = tokenList;
            _currIndex = 0;
        }

        private ErrorSink _errorSink = new StubErrorSink();
        public ErrorSink ErrorSink
        {
            get
            {
                return _errorSink;
            }
            set
            {
                _errorSink = value;
            }
        }

        public TokenWithSpan Token
        {
            get 
            { 
                if(_currIndex < _tokenList.Count)
                {
                    return _tokenList[_currIndex];
                }
                return new TokenWithSpan(Tokens.EndOfFileToken, new IndexSpan(0, 0), 0);
            }
        }

        private Tokenizer _tokenizer;
        public Tokenizer Tokenizer
        {
            get
            {
                if (_tokenizer == null)
                    _tokenizer = new Tokenizer();
                return _tokenizer;
            }
        }

        public void Reset()
        {
            _currIndex = 0;
            (ErrorSink as StubErrorSink).ErrorCount = 0;
        }

        public void ReportSyntaxError(string message, Severity severity = Severity.Error)
        {
            ErrorSink.Add(message, null, 0, 0, 0, severity);
        }

        public void ReportSyntaxError(int start, int end, string message, Severity severity = Severity.Error)
        {
            ErrorSink.Add(message, null, start, end, 0, severity);
        }

        public void ReportSyntaxError(int start, int end, string message, int errorCode, Severity severity = Severity.Error)
        {
            ErrorSink.Add(message, null, start, end, errorCode, severity);
        }

        public GeneroAst ParseFile()
        {
            return null;
        }

        public Token NextToken()
        {
            _currIndex++;
            return Token.Token;
        }

        public Token PeekToken(uint aheadBy = 1)
        {
            if (_currIndex + aheadBy < _tokenList.Count)
                return _tokenList[_currIndex + (int)aheadBy].Token;
            return Tokens.EndOfFileToken;
        }

        public TokenWithSpan PeekTokenWithSpan(uint aheadBy = 1)
        {
            if (_currIndex + aheadBy < _tokenList.Count)
                return _tokenList[_currIndex + (int)aheadBy];
            return new TokenWithSpan(Tokens.EndOfFileToken, new IndexSpan(0, 0), 0);
        }

        public bool PeekToken(TokenKind kind, uint aheadBy = 1)
        {
            return PeekToken(aheadBy).Kind == kind;
        }

        public bool PeekToken(Token check, uint aheadBy = 1)
        {
            return PeekToken(aheadBy) == check;
        }

        public bool PeekToken(TokenCategory category, uint aheadBy = 1)
        {
            var tok = PeekToken(aheadBy);
            return Tokenizer.GetTokenInfo(tok).Category == category;
        }

        public bool Eat(TokenKind kind)
        {
            Token next = PeekToken();
            if (next.Kind != kind)
            {
                return false;
            }
            else
            {
                NextToken();
                return true;
            }
        }

        public bool MaybeEat(TokenKind kind)
        {
            if (PeekToken().Kind == kind)
            {
                NextToken();
                return true;
            }
            else
            {
                return false;
            }
        }


        public LocationInfo TokenLocation
        {
            get { return null; }
        }
    }
}
