using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public abstract class GeneroParser : IDisposable, IParser
    {
        protected Dictionary<AstNode, Dictionary<object, object>> _attributes = new Dictionary<AstNode, Dictionary<object, object>>();  // attributes for each node, currently just round tripping information

        // immutable properties:
        protected readonly Tokenizer _tokenizer;

        // mutable properties:
        protected ErrorSink _errors;

        // state:
        protected TokenWithSpan _token;
        protected TokenWithSpan _lookahead;
        protected List<TokenWithSpan> _lookaheads = new List<TokenWithSpan>();

        private readonly ParserOptions _options;
        private bool _parsingStarted;
        protected internal TextReader _sourceReader;
        protected int _errorCode;
        protected readonly bool _verbatim;                            // true if we're in verbatim mode and the ASTs can be turned back into source code, preserving white space / comments
        private readonly bool _bindReferences;                      // true if we should bind the references in the ASTs
        protected string _tokenWhiteSpace, _lookaheadWhiteSpace;      // the whitespace for the current and lookahead tokens as provided from the parser
        protected List<string> _lookaheadWhiteSpaces = new List<string>();

        protected internal IProjectEntry _projectEntry;
        protected internal string _filename;
        protected List<TokenWithSpan> _codeRegions;
        protected List<TokenWithSpan> _nonCodeRegionComments;

        public GeneroAst ParseFile()
        {
            return ParseFileWorker();
        }

        private GeneroAst ParseFileWorker()
        {
            StartParsing();
            var ast = CreateAst();
            _codeRegions.Clear();
            _nonCodeRegionComments.Clear();
            return ast;
        }

        protected abstract GeneroAst CreateAst();

        protected void UpdateNodeAndTree(AstNode node, GeneroAst ast)
        {
            node.PropagateSyntaxTree(ast);
            if (_verbatim)
            {
                if (_lookahead.Token != null)
                {
                    AddExtraVerbatimText(node, _lookaheadWhiteSpace + _lookahead.Token.VerbatimImage);
                }
                AddCodeRegions(node);
                AddNonCodeRegionComments(node);
                _codeRegions.Clear();
                _nonCodeRegionComments.Clear();
            }
            foreach (var keyValue in _attributes)
            {
                foreach (var nodeAttr in keyValue.Value)
                {
                    ast.SetAttribute(keyValue.Key, nodeAttr.Key, nodeAttr.Value);
                }
            }
        }

        public string Filename
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_filename) &&
                    _projectEntry != null)
                {
                    return _projectEntry.FilePath;
                }
                else
                    return _filename;
            }
        }

        public ErrorSink ErrorSink
        {
            get
            {
                return _errors;
            }
            set
            {
                Contract.Assert(value != null);
                _errors = value;
            }
        }

        public TokenWithSpan Token
        {
            get
            {
                return _token;
            }
        }

        public Tokenizer Tokenizer
        {
            get { return _tokenizer; }
        }

        #region Construction

        protected GeneroParser(Tokenizer tokenizer, ErrorSink errorSink, bool verbatim, bool bindRefs, ParserOptions options)
        {
            Contract.Assert(tokenizer != null);
            Contract.Assert(errorSink != null);

            tokenizer.ErrorSink = new TokenizerErrorSink(this);

            _tokenizer = tokenizer;
            _errors = errorSink;
            //_langVersion = langVersion;
            _verbatim = verbatim;
            _bindReferences = bindRefs;
            _options = options;

            //if (langVersion.Is3x())
            //{
            //    // 3.x always does true division and absolute import
            //    _languageFeatures |= FutureOptions.TrueDivision | FutureOptions.AbsoluteImports;
            //}

            Reset();
            //StatementFactory = new FglStatementFactory();
            _codeRegions = new List<TokenWithSpan>();
            _nonCodeRegionComments = new List<TokenWithSpan>();
        }

        #endregion

        public void Reset()
        {
            _token = new TokenWithSpan();
            _lookaheads = new List<TokenWithSpan>();

            _parsingStarted = false;
            _errorCode = 0;
        }

        #region Error Reporting

        private void ReportSyntaxError(TokenWithSpan t)
        {
            ReportSyntaxError(t, ErrorCodes.SyntaxError);
        }

        private void ReportSyntaxError(TokenWithSpan t, int errorCode)
        {
            ReportSyntaxError(t.Token, t.Span, errorCode, true);
        }

        private void ReportSyntaxError(Token t, IndexSpan span, int errorCode, bool allowIncomplete)
        {
            var start = span.Start;
            var end = span.End;

            if (allowIncomplete && (t.Kind == TokenKind.EndOfFile || (_tokenizer.IsEndOfFile && (t.Kind == TokenKind.Dedent || t.Kind == TokenKind.NLToken))))
            {
                errorCode |= ErrorCodes.IncompleteStatement;
            }

            string msg = String.Format(System.Globalization.CultureInfo.InvariantCulture, GetErrorMessage(t, errorCode), t.Image);

            ReportSyntaxError(start, end, msg, errorCode);
        }

        private static string GetErrorMessage(Token t, int errorCode)
        {
            string msg;
            if (t.Kind != TokenKind.EndOfFile)
            {
                msg = "unexpected token '{0}'";
            }
            else
            {
                msg = "unexpected EOF while parsing";
            }

            return msg;
        }

        public void ReportSyntaxError(string message, Severity severity = Severity.Error)
        {
            if (_lookaheads.Count > 0)
            {
                ReportSyntaxError(_lookaheads[0].Span.Start, _lookaheads[0].Span.End, message, severity);
            }
        }

        public void ReportSyntaxError(int start, int end, string message, Severity severity = Severity.Error)
        {
            ReportSyntaxError(start, end, message, ErrorCodes.SyntaxError, severity);
        }

        public void ReportSyntaxError(int start, int end, string message, int errorCode, Severity severity = Severity.Error)
        {
            // save the first one, the next error codes may be induced errors:
            if (_errorCode == 0)
            {
                _errorCode = errorCode;
            }
            _errors.Add(
                message,
                _tokenizer.GetLineLocations(),
                start, end,
                errorCode,
                severity);
        }

        #endregion

        protected void StartParsing()
        {
            if (_parsingStarted)
                throw new InvalidOperationException("Parsing already started. Use Restart to start again.");

            _parsingStarted = true;

            FetchLookahead();

            string whitespace = _verbatim ? "" : null;
            while (PeekToken().Kind == TokenKind.NLToken)
            {
                NextToken();

                if (whitespace != null)
                {
                    whitespace += _tokenWhiteSpace + _token.Token.VerbatimImage;
                }
            }
            _lookaheadWhiteSpaces[0] = whitespace + _lookaheadWhiteSpaces[0];
        }

        private int GetEnd()
        {
            Debug.Assert(_token.Token != null, "No token fetched");
            return _token.Span.End;
        }

        private int GetStart()
        {
            Debug.Assert(_token.Token != null, "No token fetched");
            return _token.Span.Start;
        }

        public Token NextToken()
        {
            _token = _lookaheads[0];
            _tokenWhiteSpace = _lookaheadWhiteSpaces[0];
            _lookaheads.RemoveAt(0);
            _lookaheadWhiteSpaces.RemoveAt(0);
            if (_lookaheads.Count == 0)
            {
                FetchLookahead();
            }
            return _token.Token;
        }

        public Token PeekToken(uint aheadBy = 1)
        {
            if (aheadBy == 0)
            {
                throw new InvalidOperationException("Cannot peek at the current token");
            }
            while (_lookaheads.Count < aheadBy)
            {
                FetchLookahead();
            }
            return _lookaheads[(int)aheadBy - 1].Token;
        }

        public TokenWithSpan PeekTokenWithSpan(uint aheadBy = 1)
        {
            if (aheadBy == 0)
            {
                throw new InvalidOperationException("Cannot peek at the current token");
            }
            while (_lookaheads.Count < aheadBy)
            {
                FetchLookahead();
            }
            return _lookaheads[(int)aheadBy - 1];
        }

        private void FetchLookahead()
        {
            // for right now we don't want to see whitespace chars
            var tok = _tokenizer.GetNextToken();
            int tokenBufferPosition = _tokenizer.TokenBufferPosition;
            while ((!_tokenizer.CurrentOptions.HasFlag(TokenizerOptions.VerbatimCommentsAndLineJoins) &&
                    (Tokenizer.GetTokenInfo(tok).Category == TokenCategory.WhiteSpace ||
                     Tokenizer.GetTokenInfo(tok).Category == TokenCategory.Comment)) ||
                  tok is DentToken)
            {
                if (_tokenizer.CurrentState.SingleLineComments != null && _tokenizer.CurrentState.SingleLineComments.Count > 0)
                {
                    string str;
                    foreach (var commentTok in _tokenizer.CurrentState.SingleLineComments)
                    {
                        str = commentTok.Token.Value.ToString();
                        if (str.StartsWith("#region", StringComparison.OrdinalIgnoreCase) ||
                           str.StartsWith("#endregion", StringComparison.OrdinalIgnoreCase))
                        {
                            _codeRegions.Add(commentTok);
                        }
                        else
                        {
                            _nonCodeRegionComments.Add(commentTok);
                        }
                    }
                }
                tok = _tokenizer.GetNextToken();
            }
            _lookaheads.Add(new TokenWithSpan(tok, _tokenizer.TokenSpan, tokenBufferPosition));
            _lookaheadWhiteSpaces.Add(_tokenizer.PreceedingWhiteSpace);
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
                ReportSyntaxError(_lookaheads[0]);
                return false;
            }
            else
            {
                NextToken();
                return true;
            }
        }

        internal bool EatNoEof(TokenKind kind)
        {
            Token next = PeekToken();
            if (next.Kind != kind)
            {
                ReportSyntaxError(_lookaheads[0].Token, _lookaheads[0].Span, ErrorCodes.SyntaxError, false);
                return false;
            }
            NextToken();
            return true;
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

        internal bool MaybeEatName(string name)
        {
            var peeked = PeekToken();
            if (peeked.Kind == TokenKind.Name && ((NameToken)peeked).Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                NextToken();
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool MaybeEatEof()
        {
            if (PeekToken().Kind == TokenKind.EndOfFile)
            {
                return true;
            }

            return false;
        }

        internal NameToken ReadName()
        {
            NameToken n = PeekToken() as NameToken;
            if (n == null)
            {
                ReportSyntaxError(_lookaheads[0]);
                return n;
            }
            NextToken();
            return n;
        }

        /// <summary>
        /// Maybe eats a new line token returning true if the token was
        /// eaten.
        /// </summary>
        internal bool MaybeEatNewLine()
        {
            string curWhiteSpace = "";
            string newWhiteSpace;
            if (MaybeEatNewLine(out newWhiteSpace))
            {
                if (_verbatim)
                {
                    _lookaheadWhiteSpaces[0] = curWhiteSpace + newWhiteSpace + _lookaheadWhiteSpaces[0];
                }
                return true;
            }
            return false;
        }

        internal bool MaybeEatNewLine(out string whitespace)
        {
            whitespace = _verbatim ? "" : null;
            if (MaybeEat(TokenKind.NewLine))
            {
                if (whitespace != null)
                {
                    whitespace += _tokenWhiteSpace + _token.Token.VerbatimImage;
                }
                while (MaybeEat(TokenKind.NLToken))
                {
                    if (whitespace != null)
                    {
                        whitespace += _tokenWhiteSpace + _token.Token.VerbatimImage;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Eats a new line token throwing if the next token isn't a new line.  
        /// </summary>
        internal bool EatNewLine(out string whitespace)
        {
            whitespace = _verbatim ? "" : null;
            if (Eat(TokenKind.NewLine))
            {
                if (whitespace != null)
                {
                    whitespace += _tokenWhiteSpace + _token.Token.VerbatimImage;
                }

                while (MaybeEat(TokenKind.NLToken))
                {
                    if (whitespace != null)
                    {
                        whitespace += _tokenWhiteSpace + _token.Token.VerbatimImage;
                    }
                }
                return true;
            }
            return false;
        }

        internal Token EatEndOfInput()
        {
            while (MaybeEatNewLine() || MaybeEat(TokenKind.Dedent))
            {
                ;
            }

            Token t = NextToken();
            if (t.Kind != TokenKind.EndOfFile)
            {
                ReportSyntaxError(_token);
            }
            return t;
        }

        private class TokenizerErrorSink : ErrorSink
        {
            private readonly GeneroParser _parser;

            public TokenizerErrorSink(GeneroParser parser)
            {
                _parser = parser;
            }

            public override void Add(string message, int[] lineLocations, int startIndex, int endIndex, int errorCode, Severity severity)
            {
                if (_parser._errorCode == 0 && (severity == Severity.Error || severity == Severity.FatalError))
                {
                    _parser._errorCode = errorCode;
                }

                _parser.ErrorSink.Add(message, lineLocations, startIndex, endIndex, errorCode, severity);
            }
        }

        protected struct Name
        {
            public readonly string RealName;
            public readonly string VerbatimName;

            public Name(string name, string verbatimName)
            {
                RealName = name;
                VerbatimName = verbatimName;
            }

            public bool HasName
            {
                get
                {
                    return RealName != null;
                }
            }
        }

        public void Dispose()
        {
        }

        public LocationInfo TokenLocation
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_filename))
                    return new LocationInfo(_filename, _token.Span.Start);
                return null;
            }
        }

        protected Dictionary<object, object> GetNodeAttributes(AstNode node)
        {
            Dictionary<object, object> attrs;
            if (!_attributes.TryGetValue(node, out attrs))
            {
                _attributes[node] = attrs = new Dictionary<object, object>();
            }
            return attrs;
        }

        #region Verbatim AST support

        protected void AddPreceedingWhiteSpace(AstNode ret)
        {
            AddPreceedingWhiteSpace(ret, _tokenWhiteSpace);
        }

        protected void AddVerbatimName(Name name, AstNode ret)
        {
            if (_verbatim && name.RealName != name.VerbatimName)
            {
                GetNodeAttributes(ret)[NodeAttributes.VerbatimImage] = name.VerbatimName;
            }
        }

        protected void AddVerbatimImage(AstNode ret, string image)
        {
            if (_verbatim)
            {
                GetNodeAttributes(ret)[NodeAttributes.VerbatimImage] = image;
            }
        }

        protected List<string> MakeWhiteSpaceList()
        {
            return _verbatim ? new List<string>() : null;
        }

        protected void AddPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.PreceedingWhiteSpace] = whiteSpace;
        }

        protected void AddSecondPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            if (_verbatim)
            {
                Debug.Assert(_verbatim);
                GetNodeAttributes(ret)[NodeAttributes.SecondPreceedingWhiteSpace] = whiteSpace;
            }
        }

        protected void AddThirdPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ThirdPreceedingWhiteSpace] = whiteSpace;
        }

        protected void AddFourthPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.FourthPreceedingWhiteSpace] = whiteSpace;
        }

        protected void AddFifthPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.FifthPreceedingWhiteSpace] = whiteSpace;
        }

        protected void AddExtraVerbatimText(AstNode ret, string text)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ExtraVerbatimText] = text;
        }

        protected void AddCodeRegions(AstNode ret)
        {
            Debug.Assert(_verbatim);
            TokenWithSpan[] arr = new TokenWithSpan[_codeRegions.Count];
            _codeRegions.CopyTo(arr);
            GetNodeAttributes(ret)[NodeAttributes.CodeRegions] = arr;
        }

        protected void AddNonCodeRegionComments(AstNode ret)
        {
            Debug.Assert(_verbatim);
            TokenWithSpan[] arr = new TokenWithSpan[_nonCodeRegionComments.Count];
            _nonCodeRegionComments.CopyTo(arr);
            GetNodeAttributes(ret)[NodeAttributes.NonCodeRegionComments] = arr;
        }

        protected void AddListWhiteSpace(AstNode ret, string[] whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ListWhiteSpace] = whiteSpace;
        }

        protected void AddNamesWhiteSpace(AstNode ret, string[] whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.NamesWhiteSpace] = whiteSpace;
        }

        protected void AddVerbatimNames(AstNode ret, string[] names)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.VerbatimNames] = names;
        }

        protected void AddIsAltForm(AstNode expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.IsAltFormValue] = NodeAttributes.IsAltFormValue;
        }

        protected void AddErrorMissingCloseGrouping(AstNode expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.ErrorMissingCloseGrouping] = NodeAttributes.ErrorMissingCloseGrouping;
        }

        protected void AddErrorIsIncompleteNode(AstNode expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.ErrorIncompleteNode] = NodeAttributes.ErrorIncompleteNode;
        }

        #endregion
    }
}
