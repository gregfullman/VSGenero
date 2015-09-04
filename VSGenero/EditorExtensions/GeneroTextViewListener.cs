/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Language.StandardClassification;

namespace VSGenero.EditorExtensions
{
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [ContentType(VSGeneroConstants.ContentTypePER)]
    [ContentType(VSGeneroConstants.ContentTypeINC)]
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class GeneroTextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import(AllowDefault = true)]
        internal IGeneroTextViewChangedListener _changeListener;

        public void TextViewCreated(IWpfTextView textView)
        {
            if (_changeListener != null)
            {
                _changeListener.SetTextView(textView);
            }
            if(!textView.Properties.ContainsProperty(typeof(GeneroTextViewChangedListener)))
            {
                var listener = new GeneroTextViewChangedListener(textView);
                textView.Properties.AddProperty(typeof(GeneroTextViewChangedListener), listener);
            }
        }
    }

    internal class GeneroTextViewChangedListener
    {
        private readonly IWpfTextView _textView;
        private readonly IEditorOptions _options;
        private readonly Genero4glClassifier _classifier;
        private int _ignoreNextChange;

        public GeneroTextViewChangedListener(IWpfTextView textView)
        {
            _textView = textView;
            _options = _textView.Options;
            _textView.Closed += _textView_Closed;
            _textView.TextBuffer.Changed += TextBuffer_Changed;
            _classifier = textView.TextBuffer.GetGeneroClassifier();
        }

        void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            string filename = e.After.TextBuffer.GetFilePath();
            if (_textView != null &&
                _textView.Caret.Position.BufferPosition.Position >= 0 &&
                _textView.Caret.Position.BufferPosition.Position <= e.After.Length &&
                _ignoreNextChange != _textView.Caret.Position.BufferPosition.Position)
            {
                if(e.Changes.Count == 0 || e.Changes[0].Delta < 0)
                {
                    return;
                }

                var line = e.After.GetLineFromPosition(_textView.Caret.Position.BufferPosition.Position);
                var currLineTokens = _classifier.GetClassificationSpans(line);

                var lineStr = line.GetText();

                TokenKind alignWith = TokenKind.EndOfFile;
                bool indentAfterAlign = false;
                bool useContains = false;

                if(currLineTokens.Count > 0 && currLineTokens[0].ClassificationType.IsOfType(PredefinedClassificationTypeNames.Keyword))
                {
                    if(currLineTokens[0].Span.GetText().Equals("end", StringComparison.OrdinalIgnoreCase))
                    {
                        if(currLineTokens.Count > 1)
                        {
                            var tok = Tokens.GetToken(currLineTokens[1].Span.GetText());
                            if (tok != null && AutoIndent.BlockKeywords.ContainsKey(tok.Kind))
                            {
                                useContains = AutoIndent.BlockKeywordsContainsCheck.Contains(tok.Kind);
                                alignWith = tok.Kind;
                            }
                        }
                    }
                    else
                    {
                        var tok = Tokens.GetToken(currLineTokens[0].Span.GetText());
                        if (tok != null && AutoIndent.SubBlockKeywords.ContainsKey(tok.Kind))
                        {
                            alignWith = AutoIndent.SubBlockKeywords[tok.Kind].Item1;
                            indentAfterAlign = AutoIndent.SubBlockKeywords[tok.Kind].Item2;
                        }
                    }
                }

                if (alignWith != TokenKind.EndOfFile)
                {
                    var keyword = Tokens.TokenKinds[alignWith];
                    ITextSnapshotLine prevLine;
                    int prevLineNo = line.LineNumber - 1;
                    bool found = false;
                    string prevLineStr = null;
                    // find the line that corresponds
                    int inNestedBlockCount = 0;

                    IList<ClassificationSpan> lineTokens = null;

                    while (prevLineNo >= 0)
                    {
                        prevLine = e.After.GetLineFromLineNumber(prevLineNo);
                        prevLineStr = prevLine.GetText();
                        lineTokens = _classifier.GetClassificationSpans(prevLine);

                        if (lineTokens.Count > 0 && lineTokens[0].ClassificationType.IsOfType(PredefinedClassificationTypeNames.Keyword))
                        {
                            if (lineTokens[0].Span.GetText().Equals("end", StringComparison.OrdinalIgnoreCase) &&
                                lineTokens.Count > 1 &&
                                lineTokens[1].Span.GetText().Equals(keyword, StringComparison.OrdinalIgnoreCase))
                            {
                                inNestedBlockCount++;
                            }
                            else if((!useContains && lineTokens[0].Span.GetText().Equals(keyword, StringComparison.OrdinalIgnoreCase)) ||
                                    (useContains && lineTokens.Any(x => x.Span.GetText().Equals(keyword, StringComparison.OrdinalIgnoreCase))))
                            {
                                if (inNestedBlockCount > 0)
                                {
                                    inNestedBlockCount--;
                                }
                                else
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        prevLineNo--;
                    }

                    // get the line's indentation
                    if (found)
                    {
                        int indentSize = _options.GetIndentSize();
                        int desiredIndentation = AutoIndent.GetIndentation(prevLineStr, indentSize);
                        if (indentAfterAlign)
                            desiredIndentation += indentSize;
                        int currIndentation = AutoIndent.GetIndentation(lineStr, indentSize);
                        if (desiredIndentation != currIndentation)
                        {
                            string replacement = null;
                            if (desiredIndentation < currIndentation)
                            {
                                replacement = lineStr.Substring(currIndentation - desiredIndentation);
                            }
                            else
                            {
                                StringBuilder sb = new StringBuilder();
                                for (int i = 0; i < (desiredIndentation - currIndentation); i++)
                                    sb.Append(' ');
                                sb.Append(lineStr);
                                replacement = sb.ToString();
                            }
                            _ignoreNextChange = _textView.Caret.Position.BufferPosition.Position;
                            ITextEdit edit = _textView.TextBuffer.CreateEdit();
                            edit.Replace(new Span(line.Start, line.Length), replacement);
                            edit.Apply();
                            return;
                        }
                    }
                }
            }
            _ignoreNextChange = -1;
        }

        void _textView_Closed(object sender, EventArgs e)
        {
            var tv = sender as IWpfTextView;
            if(tv != null && tv == _textView)
            {
                tv.Properties.RemoveProperty(typeof(GeneroTextViewChangedListener));
                tv.Closed -= _textView_Closed;
                tv.TextBuffer.Changed -= TextBuffer_Changed;
            }
        }
    }
}
