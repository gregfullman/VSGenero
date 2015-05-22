using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;

namespace VSGenero.EditorExtensions
{
    public class GeneroLexer
    {
        private string _source;       // the source text buffer
        protected int _currentPosition; // an index into _source
        protected int _currentLineNumber;
        protected int _currentColumn;
        protected int _tokenStart;      // the start of the current token

        public GeneroLexer()
        {
            CommonSetup();
        }

        protected void CommonSetup()
        {
            _currentPosition = 0;
            _currentLineNumber = 1;
            _currentColumn = 1;
        }

        public int CurrentPosition
        {
            get { return _currentPosition; }
        }

        public void StartLexing(int startPos, string contents)
        {
            _source = contents;
            _currentPosition = startPos;
            // TODO: if we ever start at a point later than zero, this will have to change!
            _currentColumn = 1;
            _currentLineNumber = 1;
        }

        protected virtual bool IsNumberCharacter()
        {
            if (char.IsDigit(_source[_currentPosition]))
                return true;

            if (_source[_currentPosition] == '.' &&
                _source.Length > _currentPosition + 1)
                return char.IsDigit(_source[_currentPosition + 1]);

            return false;
        }

        protected bool IsSymbol(char c)
        {
            return (c != 34 && c != 95 && c != 96 &&
                   ((c >= 33 && c <= 47) ||
                    (c >= 58 && c <= 64) ||
                    (c >= 91 && c <= 96) ||
                    (c >= 123 && c <= 126)));
        }

        public GeneroToken Lookahead(int numTokens)
        {
            int backupPos = _currentPosition;
            int backupLine = _currentLineNumber;
            int backupCol = _currentColumn;

            GeneroToken token = null;
            for (int i = 0; i < numTokens; i++)
            {
                token = NextToken();
                if (token.TokenType == TokenCategory.None)
                    break;
            }

            _currentPosition = backupPos;
            _currentLineNumber = backupLine;
            _currentColumn = backupCol;
            return token;
        }

        protected virtual bool IsEof
        {
            get { return _currentPosition >= _source.Length; }
        }

        protected virtual GeneroToken EnsureNotEndOfFile()
        {
            if (IsEof)
            {
                var token = new GeneroToken("", _currentPosition, _currentPosition, TokenCategory.EndOfStream, _currentLineNumber, _currentColumn);
                return token;
            }
            return null;
        }

        protected virtual bool CanGetTokens()
        {
            return !string.IsNullOrWhiteSpace(_source) && _currentPosition < _source.Length;
        }

        protected virtual void AdvancePastWhitespace()
        {
            while (!IsEof && char.IsWhiteSpace(_source, _currentPosition))
            {
                IncrementCurrentPosition();
            }
        }

        protected virtual void IncrementCurrentPosition()
        {
            if (_source[_currentPosition] == '\n')
            {
                _currentLineNumber++;
                _currentColumn = 1;
            }
            else
            {
                _currentColumn++;
            }
            _currentPosition++;
        }

        protected virtual GeneroToken GetCommentToken()
        {
            GeneroToken retToken = null;
            // Test for comment
            if (_source[_currentPosition] == '#' ||
                (_source[_currentPosition] == '-' &&
                    (_source.Length > _currentPosition + 1 && _source[_currentPosition + 1] == '-')))
            {
                // go all the way to the end of the line
                while (_currentPosition < _source.Length &&
                       (_source[_currentPosition] != '\r' &&
                       _source[_currentPosition] != '\n'))
                    IncrementCurrentPosition();
                string temp = _source.Substring(_tokenStart, _currentPosition - _tokenStart);
                retToken = new GeneroToken(temp,
                                           _tokenStart,
                                           _currentPosition,
                                           TokenCategory.Comment,
                                           _currentLineNumber,
                                           _currentColumn - temp.Length);
                //_currentPosition++;
            }
            return retToken;
        }

        protected virtual GeneroToken GetNumberToken()
        {
            GeneroToken retToken = null;
            // Test for number
            if (IsNumberCharacter())
            {
                IncrementCurrentPosition();
                while (_currentPosition < _source.Length &&
                       IsNumberCharacter())
                    IncrementCurrentPosition();
                string temp = _source.Substring(_tokenStart, _currentPosition - _tokenStart);
                retToken = new GeneroToken(temp,
                                               _tokenStart,
                                               _currentPosition,
                                               TokenCategory.NumericLiteral,
                                                _currentLineNumber,
                                                _currentColumn - temp.Length);
                //_currentPosition++;
            }
            return retToken;
        }

        protected virtual GeneroToken GetStringToken()
        {
            GeneroToken retToken = null;
            // Test for string
            bool doubleQuote = _source[_currentPosition] == '"';
            bool singleQuote = _source[_currentPosition] == '\'';
            if (doubleQuote || singleQuote)
            {
                IncrementCurrentPosition();
                while (_currentPosition < _source.Length &&
                       (
                        (doubleQuote && _source[_currentPosition] != '"') ||
                        (singleQuote && _source[_currentPosition] != '\'')))
                {
                    if (_source[_currentPosition] == '\\' &&
                        _currentPosition + 1 < _source.Length)
                    {
                        // skip the escaped character
                        IncrementCurrentPosition();
                    }
                    // TODO: at some point it would be nice to support multi-line strings...but not yet
                    if (_source[_currentPosition] == '\r' || _source[_currentPosition] == '\n')
                        break;
                    IncrementCurrentPosition();
                }
                if (_currentPosition < _source.Length)
                    IncrementCurrentPosition();
                // if there's nothing beyond the string, we need to decrease the current position so we don't throw an exception
                while (_currentPosition > _source.Length)
                {
                    _currentPosition--;
                }

                string temp = "";
                if (_currentPosition >= _tokenStart)
                    temp = _source.Substring(_tokenStart, _currentPosition - _tokenStart);
                retToken = new GeneroToken(temp,
                                           _tokenStart,
                                           _currentPosition,
                                           TokenCategory.StringLiteral,
                                           _currentLineNumber,
                                           _currentColumn - temp.Length);
            }
            return retToken;
        }

        protected virtual GeneroToken GetSymbolToken()
        {
            GeneroToken retToken = null;

            // Look for symbols
            int backupPos = _currentPosition;   // need to save the current position, in case of false initial matches

            if (IsSymbol(_source[_currentPosition]))
            {
                IncrementCurrentPosition();
                // need to handle tokens that have 2 (or more) symbols
                //if (_currentPosition < _source.Length &&
                //    IsSymbol(_source[_currentPosition]))
                //{
                //    Tokens.GetToken( _lexerDefinition.SymbolMap.ContainsKey())
                //{
                //    var oneAhead = _source.Substring(_tokenStart, _currentPosition + 1 - _tokenStart);
                //    var tok = Tokens.GetToken(oneAhead);
                //    IncrementCurrentPosition();
                //}
                //// look in the symbol map
                //if (_lexerDefinition.SymbolMap.ContainsKey(_source.Substring(_tokenStart, _currentPosition - _tokenStart)))
                //{
                    string temp = _source.Substring(_tokenStart, _currentPosition - _tokenStart);
                    retToken = new GeneroToken(temp,
                                               _tokenStart,
                                               _currentPosition,
                                               TokenCategory.Operator,
                                                _currentLineNumber,
                                                _currentColumn - temp.Length);
                    //_currentPosition++;
                    return retToken;
                //}
            }
            _currentPosition = backupPos;

            return retToken;
        }

        protected virtual GeneroToken GetKeywordOrIdentifierToken()
        {
            GeneroToken retToken = null;

            if (_source[_currentPosition] == '_' ||
                       char.IsLetter(_source[_currentPosition]))
            {
                IncrementCurrentPosition();
                while (_currentPosition < _source.Length &&
                       (_source[_currentPosition] == '_' ||
                       char.IsLetter(_source[_currentPosition]) ||
                       char.IsDigit(_source[_currentPosition])))
                    IncrementCurrentPosition();

                // look for it in the keyword map
                string normalCase = _source.Substring(_tokenStart, _currentPosition - _tokenStart);
                string lowercase = normalCase.ToLower();
                if (Tokens.GetToken(lowercase) != null)
                {
                    retToken = new GeneroToken(normalCase,
                                               _tokenStart,
                                               _currentPosition,
                                               TokenCategory.Keyword,
                                                _currentLineNumber,
                                                _currentColumn - normalCase.Length);
                    //_currentPosition++;
                    return retToken;
                }

                retToken = new GeneroToken(normalCase,
                                               _tokenStart,
                                               _currentPosition,
                                               TokenCategory.Identifier,
                                                _currentLineNumber,
                                                _currentColumn - normalCase.Length);
                //_currentPosition++;   
                return retToken;
            }

            return retToken;
        }

        public GeneroToken NextToken()
        {
            GeneroToken retToken = null;
            retToken = EnsureNotEndOfFile();
            if (retToken != null)
                return retToken;

            if (CanGetTokens())
            {
                AdvancePastWhitespace();

                retToken = EnsureNotEndOfFile();
                if (retToken != null)
                    return retToken;

                _tokenStart = _currentPosition;

                if ((retToken = GetCommentToken()) != null)
                    return retToken;

                if ((retToken = GetNumberToken()) != null)
                    return retToken;

                if ((retToken = GetStringToken()) != null)
                    return retToken;

                if ((retToken = GetSymbolToken()) != null)
                    return retToken;

                if ((retToken = GetKeywordOrIdentifierToken()) != null)
                    return retToken;

                // if we didn't find anything, we'll want to advance
                if (retToken == null)
                {
                    IncrementCurrentPosition();
                }
                return new GeneroToken("", 0, 0, TokenCategory.None, 0, 0);
            }
            return new GeneroToken("", 0, 0, TokenCategory.None, 0, 0); ;
        }
    }
}
