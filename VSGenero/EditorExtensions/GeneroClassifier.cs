/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Language.StandardClassification;
using VSGenero.Navigation;

namespace VSGenero.EditorExtensions
{
    #region Provider definition
    [Export(typeof(ITaggerProvider))]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [ContentType(VSGeneroConstants.ContentTypePER)]
    [TagType(typeof(ClassificationTag))]
    internal sealed class GeneroClassifierProvider : ITaggerProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        [Import(AllowDefault = true)]
        internal IPublicFunctionNavigator PublicFunctionNavigator;

        // We're using this to allow access to the PublicFunctioNavigator import without having to do the complex MEF stuff to resolve imports.
        // Since the classifier is (one of) the first exports that gets resolved, this is a fairly safe place for it.
        internal static GeneroClassifierProvider Instance { get; private set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            Instance = this;
            GeneroClassifier classifier;
            if (!buffer.Properties.TryGetProperty<GeneroClassifier>(typeof(GeneroClassifier), out classifier))
            {
                classifier = new GeneroClassifier(buffer, ClassificationTypeRegistry);
                buffer.Properties.AddProperty(typeof(GeneroClassifier), classifier);
            }
            return classifier as ITagger<T>;
        }
    }
    #endregion //provider def

    public static class GeneroClassificationTypeNames
    {
        public const string MultiLineString = "multi-line string";
        public const string MultiLineComment = "multi-line comment";
    }

    #region Tagger
    public sealed class GeneroClassifier : ITagger<ClassificationTag>
    {
        private ITextBuffer _buffer;
        private IDictionary<GeneroTokenType, ClassificationTag> _4glTags;
        private IDictionary<IClassificationType, GeneroTokenType> _classificationTypes;
        private IDictionary<IClassificationType, GeneroTokenType> _multiLineClassificationTypes;
        private GeneroLexer _lexer;

        private Dictionary<int, GeneroToken> _incompleteMultiLineTokens;

        public GeneroClassifier(ITextBuffer buffer,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _buffer.Changed += new EventHandler<TextContentChangedEventArgs>(_buffer_Changed);
            _lexer = new GeneroLexer();
            _4glTags = new Dictionary<GeneroTokenType, ClassificationTag>();
            _classificationTypes = new Dictionary<IClassificationType, GeneroTokenType>();
            _multiLineClassificationTypes = new Dictionary<IClassificationType, GeneroTokenType>();
            _incompleteMultiLineTokens = new Dictionary<int, GeneroToken>();
            _4glTags[GeneroTokenType.Keyword] = BuildTag(typeService, PredefinedClassificationTypeNames.Keyword, GeneroTokenType.Keyword, _classificationTypes);
            _4glTags[GeneroTokenType.Identifier] = BuildTag(typeService, PredefinedClassificationTypeNames.Identifier, GeneroTokenType.Identifier, _classificationTypes);
            _4glTags[GeneroTokenType.String] = BuildTag(typeService, PredefinedClassificationTypeNames.String, GeneroTokenType.String, _classificationTypes);
            _4glTags[GeneroTokenType.Symbol] = BuildTag(typeService, PredefinedClassificationTypeNames.Operator, GeneroTokenType.Symbol, _classificationTypes);
            _4glTags[GeneroTokenType.Comment] = BuildTag(typeService, PredefinedClassificationTypeNames.Comment, GeneroTokenType.Comment, _classificationTypes);
            _4glTags[GeneroTokenType.MultiLineComment] = BuildTag(typeService, PredefinedClassificationTypeNames.Comment, GeneroTokenType.MultiLineComment, _multiLineClassificationTypes);
            _4glTags[GeneroTokenType.MultiLineString] = BuildTag(typeService, PredefinedClassificationTypeNames.String, GeneroTokenType.MultiLineString, _multiLineClassificationTypes);
        }

        void _buffer_Changed(object sender, TextContentChangedEventArgs e)
        {

            // if a change was made before an incomplete multi-line token, the token needs to be updated
            // so that the GetTags call will have an updated set of tokens.
            foreach (var change in e.Changes)
            {
                // first see if this change has actually removed one or more multi-line tokens
                foreach (var rem in _incompleteMultiLineTokens.Where(x => change.OldSpan.Contains(x.Value.StartPosition) &&
                                                                         change.NewSpan.End <= x.Value.StartPosition).ToList())
                {
                    _incompleteMultiLineTokens.Remove(rem.Key);
                }

                // get the incomplete multi-line tokens found after the change.Before line
                // get line number of the change (before)
                int line = e.Before.GetLineNumberFromPosition(change.OldPosition);
                // do ToList to prevent InvalidOperation while enumerating
                foreach (var keyLine in _incompleteMultiLineTokens.Keys.Where(x => x > line).ToList())
                {
                    int newLineIndex = keyLine;
                    if (change.LineCountDelta != 0)
                    {
                        // now update the keys to reflect the line delta
                        newLineIndex = keyLine + change.LineCountDelta;
                        _incompleteMultiLineTokens.ChangeKey(keyLine, newLineIndex);
                    }
                    // now update the token's positioning
                    _incompleteMultiLineTokens[newLineIndex].AddPositionOffset(change.Delta);
                }

                // apply any changes after (or on the same line) as the start line of a multi-line token to the MultiLineEndingPosition (if it's set)
                foreach (var keyLine in _incompleteMultiLineTokens.Keys.Where(x => x <= line).ToList())
                {
                    GeneroToken temp = _incompleteMultiLineTokens[keyLine];
                    // get the start position of the multi-line token...if it's less than the start of the change, then we're within the multi-line token
                    if (temp.StartPosition < change.NewPosition &&
                        temp.MultiLineEndingPosition > 0 &&
                        change.NewEnd <= temp.MultiLineEndingPosition + 1)
                    {
                        temp.MultiLineEndingPosition += change.Delta;
                        _incompleteMultiLineTokens[keyLine] = temp;
                    }
                }
            }
        }

        private ClassificationTag BuildTag(IClassificationTypeRegistryService typeService, string type, GeneroTokenType tokenType, IDictionary<IClassificationType, GeneroTokenType> dict)
        {
            IClassificationType classType = typeService.GetClassificationType(type);
            dict.Add(classType, tokenType);
            return new ClassificationTag(classType);
        }

        public GeneroTokenType GetTokenType(IClassificationType classificationType)
        {
            GeneroTokenType tokenType;
            if (!_classificationTypes.TryGetValue(classificationType, out tokenType))
            {
                _multiLineClassificationTypes.TryGetValue(classificationType, out tokenType);
            }
            return tokenType;
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            foreach (var span in spans)
            {
                var snapshot = span.Snapshot;
                int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
                // In general, each span is a line. Until we see differently, we'll use the logic below
                //int lastLine = snapshot.GetLineNumberFromPosition(span.Length > 0 ? span.End - 1 : span.End);

                // Lex the span
                _lexer.StartLexing(0, span.GetText());
                GeneroToken token = null;
                do
                {
                    token = _lexer.NextToken();
                    if (token.TokenType != GeneroTokenType.Eof && token.TokenType != GeneroTokenType.Unknown)
                    {
                        ClassificationTag tag;
                        // if the piece of text was recognizable, let's classify it
                        if (_4glTags.TryGetValue(token.TokenType, out tag))
                        {
                            if ((token.TokenType == GeneroTokenType.MultiLineComment ||
                                token.TokenType == GeneroTokenType.MultiLineString) &&
                                token.IsIncomplete)
                            {
                                // record this as an incomplete, if it isn't already there
                                if (!_incompleteMultiLineTokens.ContainsKey(firstLine))
                                {
                                    _incompleteMultiLineTokens.Add(firstLine, token);
                                }
                            }
                            else
                            {
                                int mlTokenLineNumber = 0;
                                // determine if we are after an incomplete multi-line token
                                var beginningMultiLineTok = FindClosestIncompleteMultiLineTokenBefore(token, firstLine, span.Start.Position, out mlTokenLineNumber);
                                if (beginningMultiLineTok != null)
                                {
                                    // see if this token is an ending match for the incomplete multi-line token we found
                                    if (beginningMultiLineTok.MultiLineEndingPosition <= 0 && 
                                        beginningMultiLineTok.IsIncompleteCompletingToken(token))
                                    {
                                        beginningMultiLineTok.MultiLineEndingPosition = token.StartPosition + span.Start.Position;
                                        _incompleteMultiLineTokens[mlTokenLineNumber] = beginningMultiLineTok;
                                    }

                                    _4glTags.TryGetValue(GeneroTokenType.MultiLineComment, out tag);
                                }
                            }

                            token.AddPositionOffset(span.Start.Position);
                            var location = new SnapshotSpan(span.Snapshot, token.StartPosition, token.EndPosition - token.StartPosition);
                            yield return new TagSpan<ClassificationTag>(location, tag);
                        }
                    }
                }
                while (token.TokenType != GeneroTokenType.Unknown && token.TokenType != GeneroTokenType.Eof);
            }
        }

        private GeneroToken FindClosestIncompleteMultiLineTokenBefore(GeneroToken token, int lineNumber, int offset, out int foundLineNumber)
        {
            GeneroToken mlToken = null;
            foundLineNumber = 0;
            if (_incompleteMultiLineTokens.Count > 0)
            {
                int closestLineNumber = _incompleteMultiLineTokens.Keys.Max(x =>
                {
                    return x < lineNumber + (token.LineNumber - 1) ? x : -1;
                });
                if (closestLineNumber >= 0 &&
                    (_incompleteMultiLineTokens[closestLineNumber].MultiLineEndingPosition <= 0 ||
                     _incompleteMultiLineTokens[closestLineNumber].MultiLineEndingPosition > token.StartPosition + offset))
                {
                    mlToken = _incompleteMultiLineTokens[closestLineNumber];
                    foundLineNumber = closestLineNumber;
                }
            }
            return mlToken;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
    #endregion //Tagger
}
