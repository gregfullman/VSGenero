using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.Analysis.Parsing
{
    public class Parser : IDisposable, IParser
    {
        // immutable properties:
        private readonly Tokenizer _tokenizer;

        // mutable properties:
        private ErrorSink _errors;

        // state:
        private TokenWithSpan _token;
        private TokenWithSpan _lookahead;
        private List<TokenWithSpan> _lookaheads = new List<TokenWithSpan>();

        private readonly ParserOptions _options;
        //private Stack<FunctionDefinition> _functions;
        private int _classDepth;
        private bool _fromFutureAllowed;
        private string _privatePrefix;
        private bool _parsingStarted, _allowIncomplete;
        private bool _inLoop, _inFinally, _isGenerator;
        private List<IndexSpan> _returnsWithValue;
        private TextReader _sourceReader;
        private int _errorCode;
        private readonly bool _verbatim;                            // true if we're in verbatim mode and the ASTs can be turned back into source code, preserving white space / comments
        private readonly bool _bindReferences;                      // true if we should bind the references in the ASTs
        private string _tokenWhiteSpace, _lookaheadWhiteSpace;      // the whitespace for the current and lookahead tokens as provided from the parser
        private List<string> _lookaheadWhiteSpaces = new List<string>();
        private Dictionary<AstNode, Dictionary<object, object>> _attributes = new Dictionary<AstNode, Dictionary<object, object>>();  // attributes for each node, currently just round tripping information

        private IProjectEntry _projectEntry;
        private string _filename;
        private List<TokenWithSpan> _codeRegions;

        private Parser _includeParser;
        private TextReader _includeParserReader;

        public readonly FglStatementFactory StatementFactory;

        public string Filename 
        { 
            get 
            { 
                if(string.IsNullOrWhiteSpace(_filename) &&
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
                if (_includeParser != null)
                    return _includeParser.Token;
                else
                    return _token; 
            }
        }

        public Tokenizer Tokenizer
        {
            get { return _tokenizer; }
        }

        #region Construction

        private Parser(Tokenizer tokenizer, ErrorSink errorSink, bool verbatim, bool bindRefs, string privatePrefix, ParserOptions options)
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
            StatementFactory = new FglStatementFactory();
            _privatePrefix = privatePrefix;
            _codeRegions = new List<TokenWithSpan>();
        }

        public static Parser CreateParser(TextReader reader, IProjectEntry projEntry = null, string filename = null)
        {
            return CreateParser(reader, null, projEntry, filename);
        }

        public static Parser CreateParser(TextReader reader, ParserOptions parserOptions, IProjectEntry projEntry = null, string filename = null)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var options = parserOptions ?? ParserOptions.Default;

            Tokenizer tokenizer = new Tokenizer(options.ErrorSink, (options.Verbatim ? TokenizerOptions.Verbatim : TokenizerOptions.None) | TokenizerOptions.GroupingRecovery);

            tokenizer.Initialize(null, reader, SourceLocation.MinValue);
            tokenizer.IndentationInconsistencySeverity = options.IndentationInconsistencySeverity;

            Parser result = new Parser(tokenizer,
                options.ErrorSink ?? ErrorSink.Null,
                options.Verbatim,
                options.BindReferences,
                options.PrivatePrefix,
                options
            );
            result._projectEntry = projEntry;
            result._filename = filename;

            result._sourceReader = reader;
            return result;
        }

        //public static Parser CreateParser(Stream stream, IProjectEntry projEntry = null, string filename = null)
        //{
        //    if (stream == null)
        //    {
        //        throw new ArgumentNullException("stream");
        //    }

        //    return CreateParser(stream, null, projEntry, filename);
        //}

        /// <summary>
        /// Creates a new parser from a seekable stream including scanning the BOM or looking for a # coding: comment to detect the appropriate coding.
        /// </summary>
        public static Parser CreateParser(Stream stream, ParserOptions parserOptions = null, IProjectEntry projEntry = null)
        {
            var options = parserOptions ?? ParserOptions.Default;
            var reader = new StreamReader(stream, true);

            return CreateParser(reader, options, projEntry);
        }


        public Parser CreateIncludeParser(string filename)
        {
            _includeParserReader = new StreamReader(filename);
            _includeParser = CreateParser(_includeParserReader, _options, null, filename);
            return _includeParser;
        }

        #endregion

        public void Reset()
        {
            //_languageFeatures = languageFeatures;
            _token = new TokenWithSpan();
            //_lookahead = new TokenWithSpan();
            _lookaheads = new List<TokenWithSpan>();
            _fromFutureAllowed = true;
            _classDepth = 0;
            //_functions = null;
            _privatePrefix = null;

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
            if (_includeParser != null)
                _includeParser.ReportSyntaxError(t, span, errorCode, allowIncomplete);
            else
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
            if (_includeParser != null)
                _includeParser.ReportSyntaxError(message, severity);
            else
            {
                if (_lookaheads.Count > 0)
                {
                    ReportSyntaxError(_lookaheads[0].Span.Start, _lookaheads[0].Span.End, message, severity);
                }
            }
        }

        public void ReportSyntaxError(int start, int end, string message, Severity severity = Severity.Error)
        {
            if (_includeParser != null)
                _includeParser.ReportSyntaxError(start, end, message, severity);
            else
                ReportSyntaxError(start, end, message, ErrorCodes.SyntaxError, severity);
        }

        public void ReportSyntaxError(int start, int end, string message, int errorCode, Severity severity = Severity.Error)
        {
            if (_includeParser != null)
                _includeParser.ReportSyntaxError(start, end, message, errorCode, severity);
            else
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
        }

        #endregion

        //public void ParseSingleStatement()
        //{
        //    StartParsing();

        //    MaybeEatNewLine();
        //    //Statement statement = ParseStmt();
        //    // TODO: parse the statement
        //    EatEndOfInput();
        //    //return CreateAst(statement);
        //}

        public GeneroAst ParseFile()
        {
            return ParseFileWorker();
        }

        private GeneroAst CreateAst(AstNode node)
        {
            var ast = new GeneroAst(node, _tokenizer.GetLineLocations(), GeneroLanguageVersion.None, _projectEntry, _filename);
            node.PropagateSyntaxTree(ast);
            if (_verbatim)
            {
                if (_lookahead.Token != null)
                {
                    AddExtraVerbatimText(node, _lookaheadWhiteSpace + _lookahead.Token.VerbatimImage);
                }
                AddCodeRegions(node);
                _codeRegions.Clear();
            }
            foreach (var keyValue in _attributes)
            {
                foreach (var nodeAttr in keyValue.Value)
                {
                    ast.SetAttribute(keyValue.Key, nodeAttr.Key, nodeAttr.Value);
                }
            }
            return ast;
        }

        public bool ParseSingleFunction(out FunctionBlockNode functionNode, bool abbreviatedParse, out string remainingReadText, out int[] lineLocations)
        {
            StartParsing();
            functionNode = null;
            remainingReadText = null;
            int bufPos;
            bool result = FunctionBlockNode.TryParseNode(this, out functionNode, out bufPos, null, abbreviatedParse, false);
            remainingReadText = _tokenizer.GetRemainingReadText(bufPos);
            lineLocations = _tokenizer.GetLineLocations();
            return result;
        }

        private GeneroAst ParseFileWorker()
        {
            StartParsing();

            ModuleNode moduleNode = null;
            if(ModuleNode.TryParseNode(this, out moduleNode))
            {
                return CreateAst(moduleNode);
            }
            _codeRegions.Clear();
            return null;

            //List<Statement> l = new List<Statement>();

            //
            // A future statement must appear near the top of the module. 
            // The only lines that can appear before a future statement are: 
            // - the module docstring (if any), 
            // - comments, 
            // - blank lines, and 
            // - other future statements. 
            // 

            //MaybeEatNewLine();

            //if (PeekToken(TokenKind.Constant))
            //{
            //    //Statement s = ParseStmt();
            //    //l.Add(s);
            //    _fromFutureAllowed = false;
            //    //ExpressionStatement es = s as ExpressionStatement;
            //    //if (es != null)
            //    //{
            //    //    ConstantExpression ce = es.Expression as ConstantExpression;
            //    //    if (ce != null && IsString(ce))
            //    //    {
            //    //        // doc string
            //    //        _fromFutureAllowed = true;
            //    //    }
            //    //}
            //}

            //MaybeEatNewLine();

            //// the end of from __future__ sequence
            //_fromFutureAllowed = false;

            //while (true)
            //{
            //    if (MaybeEatEof()) break;
            //    if (MaybeEatNewLine()) continue;

            //    Statement s = ParseStmt();
            //    l.Add(s);
            //}

            //Statement[] stmts = l.ToArray();

            //SuiteStatement ret = new SuiteStatement(stmts);
            //AddIsAltForm(ret);
            //if (_token.Token != null)
            //{
            //    ret.SetLoc(0, GetEnd());
            //}
            //return CreateAst(ret);
        }

        private void StartParsing()
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
            if (_includeParser != null)
                return _includeParser.GetEnd();
            else
            {
                Debug.Assert(_token.Token != null, "No token fetched");
                return _token.Span.End;
            }
        }

        private int GetStart()
        {
            if (_includeParser != null)
                return _includeParser.GetStart();
            else
            {
                Debug.Assert(_token.Token != null, "No token fetched");
                return _token.Span.Start;
            }
        }

        public Token NextToken()
        {
            if (_includeParser != null)
            {
                var token = _includeParser.NextToken();
                if (token.Kind == TokenKind.EndOfFile)
                {
                    _includeParser.Dispose();
                    _includeParser = null;
                }
                return token;
            }
            else
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
        }

        public Token PeekToken(uint aheadBy = 1)
        {
            if (_includeParser != null)
            {
                var tok = _includeParser.PeekToken(aheadBy);
                if(tok.Kind == TokenKind.EndOfFile)
                {
                    _includeParser.Dispose();
                    _includeParser = null;
                }
                return tok;
            }
            else
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
        }

        public TokenWithSpan PeekTokenWithSpan(uint aheadBy = 1)
        {
            if (_includeParser != null)
            {
                var tok = _includeParser.PeekTokenWithSpan(aheadBy);
                if (tok.Token.Kind == TokenKind.EndOfFile)
                {
                    _includeParser.Dispose();
                    _includeParser = null;
                }
                return tok;
            }
            else
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
                    foreach(var commentTok in _tokenizer.CurrentState.SingleLineComments)
                    {
                        str = commentTok.Token.Value.ToString();
                        if(str.StartsWith("#region", StringComparison.OrdinalIgnoreCase) ||
                           str.StartsWith("#endregion", StringComparison.OrdinalIgnoreCase))
                        {
                            _codeRegions.Add(commentTok);
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
            if (_includeParser != null)
            {
                var tok = _includeParser.PeekToken(aheadBy);
                if(tok.Kind == TokenKind.EndOfFile)
                {
                    _includeParser.Dispose();
                    _includeParser = null;
                }
                return tok.Kind == kind;
            }
            else
                return PeekToken(aheadBy).Kind == kind;
        }

        public bool PeekToken(Token check, uint aheadBy = 1)
        {
            if (_includeParser != null)
            {
                var tok = _includeParser.PeekToken(aheadBy);
                if (tok.Kind == TokenKind.EndOfFile)
                {
                    _includeParser.Dispose();
                    _includeParser = null;
                }
                return tok == check;
            }
            else
                return PeekToken(aheadBy) == check;
        }

        public bool PeekToken(TokenCategory category, uint aheadBy = 1)
        {
            if (_includeParser != null)
            {
                var tok = _includeParser.PeekToken(aheadBy);
                if (tok.Kind == TokenKind.EndOfFile)
                {
                    _includeParser.Dispose();
                    _includeParser = null;
                }
                return Tokenizer.GetTokenInfo(tok).Category == category;
            }
            else
            {
                var tok = PeekToken(aheadBy);
                return Tokenizer.GetTokenInfo(tok).Category == category;
            }
        }

        public bool Eat(TokenKind kind)
        {
            if (_includeParser != null)
            {
                return _includeParser.Eat(kind);
            }
            else
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
        }

        internal bool EatNoEof(TokenKind kind)
        {
            if (_includeParser != null)
            {
                return _includeParser.EatNoEof(kind);
            }
            else
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
        }

        public bool MaybeEat(TokenKind kind)
        {
            if (_includeParser != null)
            {
                return _includeParser.MaybeEat(kind);
            }
            else
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
        }

        internal bool MaybeEatName(string name)
        {
            if (_includeParser != null)
            {
                return _includeParser.MaybeEatName(name);
            }
            else
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
        }

        internal bool MaybeEatEof()
        {
            if (_includeParser != null)
            {
                return _includeParser.MaybeEatEof();
            }
            else
            {
                if (PeekToken().Kind == TokenKind.EndOfFile)
                {
                    return true;
                }

                return false;
            }
        }

        internal NameToken ReadName()
        {
            if (_includeParser != null)
            {
                return _includeParser.ReadName();
            }
            else
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
        }

        /// <summary>
        /// Maybe eats a new line token returning true if the token was
        /// eaten.
        /// </summary>
        internal bool MaybeEatNewLine()
        {
            if (_includeParser != null)
            {
                return _includeParser.MaybeEatNewLine();
            }
            else
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
        }

        internal bool MaybeEatNewLine(out string whitespace)
        {
            if (_includeParser != null)
            {
                return _includeParser.MaybeEatNewLine(out whitespace);
            }
            else
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
        }

        /// <summary>
        /// Eats a new line token throwing if the next token isn't a new line.  
        /// </summary>
        internal bool EatNewLine(out string whitespace)
        {
            if (_includeParser != null)
            {
                return _includeParser.EatNewLine(out whitespace);
            }
            else
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
        }

        internal Token EatEndOfInput()
        {
            if (_includeParser != null)
            {
                return _includeParser.EatEndOfInput();
            }
            else
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
        }

        private class TokenizerErrorSink : ErrorSink
        {
            private readonly Parser _parser;

            public TokenizerErrorSink(Parser parser)
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

        #region Verbatim AST support

        private void AddPreceedingWhiteSpace(AstNode ret)
        {
            AddPreceedingWhiteSpace(ret, _tokenWhiteSpace);
        }

        private Dictionary<object, object> GetNodeAttributes(AstNode node)
        {
            Dictionary<object, object> attrs;
            if (!_attributes.TryGetValue(node, out attrs))
            {
                _attributes[node] = attrs = new Dictionary<object, object>();
            }
            return attrs;
        }

        private void AddVerbatimName(Name name, AstNode ret)
        {
            if (_verbatim && name.RealName != name.VerbatimName)
            {
                GetNodeAttributes(ret)[NodeAttributes.VerbatimImage] = name.VerbatimName;
            }
        }

        private void AddVerbatimImage(AstNode ret, string image)
        {
            if (_verbatim)
            {
                GetNodeAttributes(ret)[NodeAttributes.VerbatimImage] = image;
            }
        }

        private List<string> MakeWhiteSpaceList()
        {
            return _verbatim ? new List<string>() : null;
        }

        private void AddPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.PreceedingWhiteSpace] = whiteSpace;
        }

        private void AddSecondPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            if (_verbatim)
            {
                Debug.Assert(_verbatim);
                GetNodeAttributes(ret)[NodeAttributes.SecondPreceedingWhiteSpace] = whiteSpace;
            }
        }

        private void AddThirdPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ThirdPreceedingWhiteSpace] = whiteSpace;
        }

        private void AddFourthPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.FourthPreceedingWhiteSpace] = whiteSpace;
        }

        private void AddFifthPreceedingWhiteSpace(AstNode ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.FifthPreceedingWhiteSpace] = whiteSpace;
        }

        private void AddExtraVerbatimText(AstNode ret, string text)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ExtraVerbatimText] = text;
        }

        private void AddCodeRegions(AstNode ret)
        {
            Debug.Assert(_verbatim);
            TokenWithSpan[] arr = new TokenWithSpan[_codeRegions.Count];
            _codeRegions.CopyTo(arr);
            GetNodeAttributes(ret)[NodeAttributes.CodeRegions] = arr;
        }

        private void AddListWhiteSpace(AstNode ret, string[] whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ListWhiteSpace] = whiteSpace;
        }

        private void AddNamesWhiteSpace(AstNode ret, string[] whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.NamesWhiteSpace] = whiteSpace;
        }

        private void AddVerbatimNames(AstNode ret, string[] names)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.VerbatimNames] = names;
        }

        private void AddIsAltForm(AstNode expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.IsAltFormValue] = NodeAttributes.IsAltFormValue;
        }

        private void AddErrorMissingCloseGrouping(AstNode expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.ErrorMissingCloseGrouping] = NodeAttributes.ErrorMissingCloseGrouping;
        }

        private void AddErrorIsIncompleteNode(AstNode expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.ErrorIncompleteNode] = NodeAttributes.ErrorIncompleteNode;
        }

        #endregion

        struct Name
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
            if (_includeParserReader != null)
            {
                _includeParserReader.Close();
                _includeParserReader.Dispose();
                _includeParserReader = null;
            }
        }


        public LocationInfo TokenLocation
        {
            get
            {
                if (_includeParser != null)
                {
                    return _includeParser.TokenLocation;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(_filename))
                        return new LocationInfo(_filename, _token.Span.Start);
                    return null;
                }
            }
        }
    }
}
