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
            if (_ignoreNextChange != _textView.Caret.Position.BufferPosition.Position &&
                !string.IsNullOrWhiteSpace(e.Changes[0].NewText))
            {
                var line = e.After.GetLineFromPosition(_textView.Caret.Position.BufferPosition.Position);
                var lineStr = line.GetText();
                var trimmed = lineStr.Trim();

                TokenKind alignWith = TokenKind.EndOfFile;
                if (trimmed.StartsWith("end", StringComparison.OrdinalIgnoreCase))
                {
                    // get the last word in the line
                    var word = trimmed.Split(new[] { ' ' });
                    if (word.Length > 1)
                    {
                        var tok = Tokens.GetToken(word[word.Length - 1]);
                        if (tok != null && AutoIndent.BlockKeywords.Contains(tok.Kind))
                        {
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
                    while (prevLineNo > 0)
                    {
                        prevLine = e.After.GetLineFromLineNumber(prevLineNo);
                        if ((prevLineStr = prevLine.GetText()).TrimStart().StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
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
                                replacement = lineStr.PadLeft(desiredIndentation - currIndentation);
                            }
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
