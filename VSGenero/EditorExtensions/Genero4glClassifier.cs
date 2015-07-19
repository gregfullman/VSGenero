using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing;

namespace VSGenero.EditorExtensions
{
    public class Genero4glClassifier : IClassifier
    {
        private readonly TokenCache _tokenCache;
        private readonly Genero4glClassifierProvider _provider;
        private readonly ITextBuffer _buffer;

        [ThreadStatic]
        private static Tokenizer _tokenizer;    // tokenizer for each version, shared between all buffers

        internal Genero4glClassifier(Genero4glClassifierProvider provider, ITextBuffer buffer) {
            if (buffer != null)
            {
                buffer.Changed += BufferChanged;
                buffer.ContentTypeChanged += BufferContentTypeChanged;
            }

            _tokenCache = new TokenCache();
            _provider = provider;
            _buffer = buffer;
        }

        internal void NewVersion() {
            _tokenCache.Clear();
            var changed = ClassificationChanged;
            if (changed != null) {
                var snapshot = _buffer.CurrentSnapshot;

                changed(this, new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
            }
        }

        #region IDlrClassifier

        // This event gets raised if the classification of existing test changes.
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        /// <summary>
        /// This method classifies the given snapshot span.
        /// </summary>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
            return GetClassificationSpans(span, false);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span, bool resetTokenizer)
        {
            var classifications = new List<ClassificationSpan>();
            var snapshot = span.Snapshot;


            if (span.Length > 0)
            {
                AddClassifications(GetTokenizer(resetTokenizer), classifications, span);
            }

            return classifications;
        }

        public IList<TokenInfo> GetTokens(SnapshotSpan span)
        {
            var tokens = new List<TokenInfo>();
            var snapshot = span.Snapshot;


            if (span.Length > 0)
            {
                AddTokens(GetTokenizer(false), tokens, span);
            }

            return tokens;
        }

        private Tokenizer GetTokenizer(bool reset) {
            if (_tokenizer == null) {
                _tokenizer = new Tokenizer(options: TokenizerOptions.Verbatim | TokenizerOptions.VerbatimCommentsAndLineJoins);
            }
            else if(reset)
            {
                _tokenCache.Clear();
            }
            return _tokenizer;
        }

        public Genero4glClassifierProvider Provider {
            get {
                return _provider;
            }
        }

        #endregion

        #region Private Members

        private Dictionary<TokenCategory, IClassificationType> CategoryMap {
            get {
                return _provider.CategoryMap;
            }
        }

        private void BufferContentTypeChanged(object sender, ContentTypeChangedEventArgs e) {
            _tokenCache.Clear();
            _buffer.Changed -= BufferChanged;
            _buffer.ContentTypeChanged -= BufferContentTypeChanged;
            _buffer.Properties.RemoveProperty(typeof(Genero4glClassifier));
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e) {
            var snapshot = e.After;

            _tokenCache.EnsureCapacity(snapshot.LineCount);

            var tokenizer = GetTokenizer(false);
            foreach (var change in e.Changes) {
                if (change.LineCountDelta > 0) {
                    _tokenCache.InsertLines(snapshot.GetLineNumberFromPosition(change.NewEnd) + 1 - change.LineCountDelta, change.LineCountDelta);
                } else if (change.LineCountDelta < 0) {
                    _tokenCache.DeleteLines(snapshot.GetLineNumberFromPosition(change.NewEnd) + 1, -change.LineCountDelta);
                }

                ApplyChange(tokenizer, snapshot, change.NewSpan);
            }
        }

        private void AddTokens(Tokenizer tokenizer, List<TokenInfo> tokens, SnapshotSpan span)
        {
            Debug.Assert(span.Length > 0);

            var snapshot = span.Snapshot;
            int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
            int lastLine = snapshot.GetLineNumberFromPosition(span.End - 1);

            Contract.Assert(firstLine >= 0);

            _tokenCache.EnsureCapacity(snapshot.LineCount);

            // find the closest line preceding firstLine for which we know categorizer state, stop at the codeStartLine:
            LineTokenization lineTokenization;
            int currentLine = _tokenCache.IndexOfPreviousTokenization(firstLine, 0, out lineTokenization) + 1;
            object state = lineTokenization.State;

            while (currentLine <= lastLine)
            {
                if (!_tokenCache.TryGetTokenization(currentLine, out lineTokenization))
                {
                    lineTokenization = TokenizeLine(tokenizer, snapshot, state, currentLine);
                    _tokenCache[currentLine] = lineTokenization;
                }

                foreach(var token in lineTokenization.Tokens)
                {
                    tokens.Add(token);
                }

                currentLine++;
            }
        }

        /// <summary>
        /// Adds classification spans to the given collection.
        /// Scans a contiguous sub-<paramref name="span"/> of a larger code span which starts at <paramref name="codeStartLine"/>.
        /// </summary>
        private void AddClassifications(Tokenizer tokenizer, List<ClassificationSpan> classifications, SnapshotSpan span) {
            Debug.Assert(span.Length > 0);

            var snapshot = span.Snapshot;
            int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
            int lastLine = snapshot.GetLineNumberFromPosition(span.End - 1);

            Contract.Assert(firstLine >= 0);

            _tokenCache.EnsureCapacity(snapshot.LineCount);

            // find the closest line preceding firstLine for which we know categorizer state, stop at the codeStartLine:
            LineTokenization lineTokenization;
            int currentLine = _tokenCache.IndexOfPreviousTokenization(firstLine, 0, out lineTokenization) + 1;
            object state = lineTokenization.State;

            while (currentLine <= lastLine) {
                if (!_tokenCache.TryGetTokenization(currentLine, out lineTokenization)) {
                    lineTokenization = TokenizeLine(tokenizer, snapshot, state, currentLine);
                    _tokenCache[currentLine] = lineTokenization;
                }

                state = lineTokenization.State;

                for (int i = 0; i < lineTokenization.Tokens.Length; i++) {
                    var token = lineTokenization.Tokens[i];
                    if (token.Category == TokenCategory.IncompleteMultiLineStringLiteral) 
                    {
                        // we need to walk backwards to find the start of this multi-line string...

                        TokenInfo startToken = token;
                        int validPrevLine;
                        int length = startToken.SourceSpan.Length;
                        if (i == 0) {
                            length += GetLeadingMultiLineStrings(tokenizer, snapshot, firstLine, currentLine, out validPrevLine, ref startToken);
                        } else {
                            validPrevLine = currentLine;
                        }

                        if (i == lineTokenization.Tokens.Length - 1) {
                            length += GetTrailingMultiLineStrings(tokenizer, snapshot, currentLine, state);
                        }

                        int start = SnapshotSpanToSpan(snapshot, startToken, validPrevLine).Start;
                        if (start + length > snapshot.Length)
                            length -= 1;
                        var multiStrSpan = new Span(start, length);
                        classifications.Add(
                            new ClassificationSpan(
                                new SnapshotSpan(snapshot, multiStrSpan),
                                _provider.StringLiteral
                            )
                        );
                    } 
                    else if(token.Category == TokenCategory.IncompleteMultiLineComment)
                    {
                        TokenInfo startToken = token;
                        int validPrevLine;
                        int length = startToken.SourceSpan.Length;
                        if (i == 0)
                        {
                            length += GetLeadingMultiLineComments(tokenizer, snapshot, firstLine, currentLine, out validPrevLine, ref startToken);
                        }
                        else
                        {
                            validPrevLine = currentLine;
                        }

                        if (i == lineTokenization.Tokens.Length - 1)
                        {
                            length += GetTrailingMultiLineComments(tokenizer, snapshot, currentLine, state);
                        }

                        int start = SnapshotSpanToSpan(snapshot, startToken, validPrevLine).Start;
                        if(start + length > snapshot.Length)
                            length -= 1;
                        var multiStrSpan = new Span(start, length);
                        classifications.Add(
                            new ClassificationSpan(
                                new SnapshotSpan(snapshot, multiStrSpan),
                                _provider.Comment
                            )
                        );
                    }
                    else {
                        var classification = ClassifyToken(span, token, currentLine);

                        if (classification != null) {
                            classifications.Add(classification);
                        }
                    }
                }

                currentLine++;
            }
        }

        private int GetLeadingMultiLineStrings(Tokenizer tokenizer, ITextSnapshot snapshot, int firstLine, int currentLine, out int validPrevLine, ref TokenInfo startToken) {
            validPrevLine = currentLine;
            int prevLine = currentLine - 1;
            int length = 0;

            while (prevLine >= 0) {
                LineTokenization prevLineTokenization;
                if (!_tokenCache.TryGetTokenization(prevLine, out prevLineTokenization)) {
                    LineTokenization lineTokenizationTemp;
                    int currentLineTemp = _tokenCache.IndexOfPreviousTokenization(firstLine, 0, out lineTokenizationTemp) + 1;
                    object stateTemp = lineTokenizationTemp.State;

                    while (currentLineTemp <= snapshot.LineCount) {
                        if (!_tokenCache.TryGetTokenization(currentLineTemp, out lineTokenizationTemp)) {
                            lineTokenizationTemp = TokenizeLine(tokenizer, snapshot, stateTemp, currentLineTemp);
                            _tokenCache[currentLineTemp] = lineTokenizationTemp;
                        }

                        stateTemp = lineTokenizationTemp.State;
                    }

                    prevLineTokenization = TokenizeLine(tokenizer, snapshot, stateTemp, prevLine);
                    _tokenCache[prevLine] = prevLineTokenization;
                }

                if (prevLineTokenization.Tokens.Length != 0) {
                    if (prevLineTokenization.Tokens[prevLineTokenization.Tokens.Length - 1].Category != TokenCategory.IncompleteMultiLineStringLiteral) {
                        break;
                    }

                    startToken = prevLineTokenization.Tokens[prevLineTokenization.Tokens.Length - 1];
                    length += startToken.SourceSpan.Length;
                }

                validPrevLine = prevLine;
                prevLine--;

                if (prevLineTokenization.Tokens.Length > 1) {
                    // http://pytools.codeplex.com/workitem/749
                    // if there are multiple tokens on this line then our multi-line string
                    // is terminated.
                    break;
                }
            }
            return length;
        }

        private int GetLeadingMultiLineComments(Tokenizer tokenizer, ITextSnapshot snapshot, int firstLine, int currentLine, out int validPrevLine, ref TokenInfo startToken)
        {
            validPrevLine = currentLine;
            int prevLine = currentLine - 1;
            int length = 0;

            while (prevLine >= 0)
            {
                LineTokenization prevLineTokenization;
                if (!_tokenCache.TryGetTokenization(prevLine, out prevLineTokenization))
                {
                    LineTokenization lineTokenizationTemp;
                    int currentLineTemp = _tokenCache.IndexOfPreviousTokenization(firstLine, 0, out lineTokenizationTemp) + 1;
                    object stateTemp = lineTokenizationTemp.State;

                    while (currentLineTemp <= snapshot.LineCount)
                    {
                        if (!_tokenCache.TryGetTokenization(currentLineTemp, out lineTokenizationTemp))
                        {
                            lineTokenizationTemp = TokenizeLine(tokenizer, snapshot, stateTemp, currentLineTemp);
                            _tokenCache[currentLineTemp] = lineTokenizationTemp;
                        }

                        stateTemp = lineTokenizationTemp.State;
                    }

                    prevLineTokenization = TokenizeLine(tokenizer, snapshot, stateTemp, prevLine);
                    _tokenCache[prevLine] = prevLineTokenization;
                }

                if (prevLineTokenization.Tokens.Length != 0)
                {
                    if (prevLineTokenization.Tokens[prevLineTokenization.Tokens.Length - 1].Category != TokenCategory.IncompleteMultiLineComment)
                    {
                        break;
                    }

                    startToken = prevLineTokenization.Tokens[prevLineTokenization.Tokens.Length - 1];
                    length += startToken.SourceSpan.Length;
                }

                validPrevLine = prevLine;
                prevLine--;

                if (prevLineTokenization.Tokens.Length > 1)
                {
                    // http://pytools.codeplex.com/workitem/749
                    // if there are multiple tokens on this line then our multi-line string
                    // is terminated.
                    break;
                }
            }
            return length;
        }

        private int GetTrailingMultiLineStrings(Tokenizer tokenizer, ITextSnapshot snapshot, int currentLine, object state) {
            int nextLine = currentLine + 1;
            var prevState = state;
            int length = 0;
            while (nextLine < snapshot.LineCount) {
                LineTokenization nextLineTokenization;
                if (!_tokenCache.TryGetTokenization(nextLine, out nextLineTokenization)) {
                    nextLineTokenization = TokenizeLine(tokenizer, snapshot, prevState, nextLine);
                    prevState = nextLineTokenization.State;
                    _tokenCache[nextLine] = nextLineTokenization;
                }

                if (nextLineTokenization.Tokens.Length != 0) {
                    if (nextLineTokenization.Tokens[0].Category != TokenCategory.IncompleteMultiLineStringLiteral) {
                        break;
                    }

                    length += nextLineTokenization.Tokens[0].SourceSpan.Length;
                }
                nextLine++;
            }
            return length;
        }

        private int GetTrailingMultiLineComments(Tokenizer tokenizer, ITextSnapshot snapshot, int currentLine, object state)
        {
            int nextLine = currentLine + 1;
            var prevState = state;
            int length = 0;
            while (nextLine < snapshot.LineCount)
            {
                LineTokenization nextLineTokenization;
                if (!_tokenCache.TryGetTokenization(nextLine, out nextLineTokenization))
                {
                    nextLineTokenization = TokenizeLine(tokenizer, snapshot, prevState, nextLine);
                    prevState = nextLineTokenization.State;
                    _tokenCache[nextLine] = nextLineTokenization;
                }

                if (nextLineTokenization.Tokens.Length != 0)
                {
                    if (nextLineTokenization.Tokens[0].Category != TokenCategory.IncompleteMultiLineComment)
                    {
                        break;
                    }

                    length += nextLineTokenization.Tokens[0].SourceSpan.Length;
                }
                nextLine++;
            }
            return length;
        }

        /// <summary>
        /// Rescans the part of the buffer affected by a change. 
        /// Scans a contiguous sub-<paramref name="span"/> of a larger code span which starts at <paramref name="codeStartLine"/>.
        /// </summary>
        private void ApplyChange(Tokenizer tokenizer, ITextSnapshot snapshot, Span span) {
            int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
            int lastLine = snapshot.GetLineNumberFromPosition(span.Length > 0 ? span.End - 1 : span.End);

            Contract.Assert(firstLine >= 0);

            // find the closest line preceding firstLine for which we know categorizer state, stop at the codeStartLine:
            LineTokenization lineTokenization;
            firstLine = _tokenCache.IndexOfPreviousTokenization(firstLine, 0, out lineTokenization) + 1;
            object state = lineTokenization.State;

            int currentLine = firstLine;
            object previousState;
            while (currentLine < snapshot.LineCount) {
                previousState = _tokenCache.TryGetTokenization(currentLine, out lineTokenization) ? lineTokenization.State : null;
                _tokenCache[currentLine] = lineTokenization = TokenizeLine(tokenizer, snapshot, state, currentLine);
                state = lineTokenization.State;

                // stop if we visted all affected lines and the current line has no tokenization state or its previous state is the same as the new state:
                if (currentLine > lastLine && (previousState == null || previousState.Equals(state))) {
                    break;
                }

                currentLine++;
            }

            // classification spans might have changed between the start of the first and end of the last visited line:
            int changeStart = snapshot.GetLineFromLineNumber(firstLine).Start;
            int changeEnd = (currentLine < snapshot.LineCount) ? snapshot.GetLineFromLineNumber(currentLine).End : snapshot.Length;
            if (changeStart < changeEnd) {
                var classificationChanged = ClassificationChanged;
                if (classificationChanged != null) {
                    var args = new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, new Span(changeStart, changeEnd - changeStart)));
                    classificationChanged(this, args);
                }
            }
        }

        private LineTokenization TokenizeLine(Tokenizer tokenizer, ITextSnapshot snapshot, object previousLineState, int lineNo) {
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNo);
            SnapshotSpan lineSpan = new SnapshotSpan(snapshot, line.Start, line.LengthIncludingLineBreak);

            var tcp = new SnapshotSpanSourceCodeReader(lineSpan);

            tokenizer.Initialize(previousLineState, tcp, new SourceLocation(line.Start.Position, lineNo + 1, 1));
            try {
                var tokens = tokenizer.ReadTokens(lineSpan.Length).ToArray();
                return new LineTokenization(tokens, tokenizer.CurrentState);
            } finally {
                tokenizer.Uninitialize();
            }
        }

        private ClassificationSpan ClassifyToken(SnapshotSpan span, TokenInfo token, int lineNumber) {
            IClassificationType classification = null;

            if (token.Category == TokenCategory.Operator) {
                if (token.Trigger == TokenTriggers.MemberSelect) {
                    classification = _provider.DotClassification;
                }
            } else if (token.Category == TokenCategory.Grouping) {
                if ((token.Trigger & TokenTriggers.MatchBraces) != 0) {
                    classification = _provider.GroupingClassification;
                }
            } else if (token.Category == TokenCategory.Delimiter) {
                if (token.Trigger == TokenTriggers.ParameterNext) {
                    classification = _provider.CommaClassification;
                }
            }

            if (classification == null) {
                CategoryMap.TryGetValue(token.Category, out classification);
            }

            if (classification != null) {
                var tokenSpan = SnapshotSpanToSpan(span.Snapshot, token, lineNumber);
                var intersection = span.Intersection(tokenSpan);

                if (intersection != null && intersection.Value.Length > 0 ||
                    (span.Length == 0 && tokenSpan.Contains(span.Start))) { // handle zero-length spans which Intersect and Overlap won't return true on ever.
                    return new ClassificationSpan(new SnapshotSpan(span.Snapshot, tokenSpan), classification);
                }
            }

            return null;
        }

        private static Span SnapshotSpanToSpan(ITextSnapshot snapshot, TokenInfo token, int lineNumber) {
            var line = snapshot.GetLineFromLineNumber(lineNumber);
            var index = line.Start.Position + token.SourceSpan.Start.Column - 1;
            var tokenSpan = new Span(index, token.SourceSpan.Length);
            return tokenSpan;
        }

        #endregion
    }

    internal static class ClassifierExtensions {
        public static Genero4glClassifier GetGeneroClassifier(this ITextBuffer buffer) {
            Genero4glClassifier res;
            if (buffer.Properties.TryGetProperty<Genero4glClassifier>(typeof(Genero4glClassifier), out res)) {
                return res;
            }
            return null;
        }
    }
}
