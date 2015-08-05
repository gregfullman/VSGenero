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
        private IWpfTextView _textView;
        private int _ignoreNextChange;
        private IEditorOptions _options;
        private string _filename;

        public GeneroTextViewCreationListener()
        {
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (_changeListener != null)
            {
                _changeListener.SetTextView(textView);
            }
            if(_textView != null && _textView.TextBuffer != null)
                _filename = _textView.TextBuffer.GetFilePath();
            _textView = textView;
            _options = textView.Options;
            textView.Closed += textView_Closed;
            textView.TextBuffer.Changed += TextBuffer_Changed;
        }

        void textView_Closed(object sender, EventArgs e)
        {
            (sender as IWpfTextView).TextBuffer.Changed -= TextBuffer_Changed;
            (sender as IWpfTextView).Closed -= textView_Closed;
        }

        void TextBuffer_Changed(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            string filename = e.After.TextBuffer.GetFilePath();
            if (_textView.Caret.Position.BufferPosition.Position >= 0 &&
                _textView.Caret.Position.BufferPosition.Position <= e.After.Length &&
                _ignoreNextChange != _textView.Caret.Position.BufferPosition.Position &&
                !string.IsNullOrWhiteSpace(e.Changes[0].NewText))
            {
                var line = e.After.GetLineFromPosition(_textView.Caret.Position.BufferPosition.Position);
                var lineStr = line.GetText();
                var trimmed = lineStr.Trim();

                TokenKind alignWith = TokenKind.EndOfFile;
                bool useContains = false;
                if (trimmed.StartsWith("end", StringComparison.OrdinalIgnoreCase))
                {
                    // get the last word in the line
                    var word = trimmed.Split(new[] { ' ' });
                    if (word.Length > 1)
                    {
                        var tok = Tokens.GetToken(word[word.Length - 1]);
                        if (tok != null && AutoIndent.BlockKeywords.ContainsKey(tok.Kind))
                        {
                            useContains = AutoIndent.BlockKeywordsContainsCheck.Contains(tok.Kind);
                            alignWith = tok.Kind;
                        }
                    }
                }
                else
                {
                    if (trimmed.Length >= 4)
                    {
                        var word = trimmed.Split(new[] { ' ' });
                        if (word.Length >= 1)
                        {
                            var tok = Tokens.GetToken(word[0]);
                            if (tok != null && AutoIndent.SubBlockKeywords.ContainsKey(tok.Kind))
                            {
                                alignWith = AutoIndent.SubBlockKeywords[tok.Kind];
                            }
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
                    bool inNestedBlock = false;
                    while (prevLineNo > 0)
                    {
                        prevLine = e.After.GetLineFromLineNumber(prevLineNo);
                        prevLineStr = prevLine.GetText();
                        var prevTrimmed = prevLineStr.Trim();
                        if (prevTrimmed.StartsWith("#") || prevTrimmed.StartsWith("--"))
                        {
                            prevLineNo--;
                            continue;
                        }
                        else if (prevTrimmed.StartsWith("end", StringComparison.OrdinalIgnoreCase) && prevLineStr.TrimEnd().EndsWith(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            inNestedBlock = true;
                        }
                        else if ((!useContains && prevLineStr.TrimStart().StartsWith(keyword, StringComparison.OrdinalIgnoreCase)) ||
                            (useContains && prevLineStr.Trim().Contains(keyword)))
                        {
                            if (inNestedBlock)
                            {
                                inNestedBlock = false;
                            }
                            else
                            {
                                found = true;
                                break;
                            }
                        }
                        prevLineNo--;
                    }

                    // get the line's indentation
                    if (found)
                    {
                        int indentSize = _options.GetIndentSize();
                        int desiredIndentation = AutoIndent.GetIndentation(prevLineStr, indentSize);
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
    }
}
