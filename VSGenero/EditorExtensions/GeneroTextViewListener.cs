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

        public GeneroTextViewCreationListener()
        {
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (_changeListener != null)
            {
                _changeListener.SetTextView(textView);
            }
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
            if (_ignoreNextChange != _textView.Caret.Position.BufferPosition.Position &&
                !string.IsNullOrWhiteSpace(e.Changes[0].NewText))
            {
                var line = e.After.GetLineFromPosition(_textView.Caret.Position.BufferPosition.Position);
                var lineStr = line.GetText();
                var trimmed = lineStr.Trim();
                if ((trimmed.StartsWith("end", StringComparison.OrdinalIgnoreCase) /* and ends with a block keyword */ &&
                    AutoIndent.BlockKeywords.Any(x => trimmed.EndsWith(Tokens.TokenKinds[x], StringComparison.OrdinalIgnoreCase))) ||
                    trimmed.Equals("else", StringComparison.OrdinalIgnoreCase))
                {
                    // shift the line over
                    // TODO: need to use better logic than this...need to find the corresponding statement start and determine its indentation
                    int startInd = 0;
                    int indentSize = _options.GetIndentSize();
                    while (startInd < indentSize)
                    {
                        if (lineStr[startInd] == ' ')
                            startInd++;
                        else
                            break;
                    }

                    _ignoreNextChange = _textView.Caret.Position.BufferPosition.Position;
                    var repl = lineStr.Substring(startInd);
                    ITextEdit edit = _textView.TextBuffer.CreateEdit();
                    edit.Replace(new Span(line.Start, line.Length), repl);
                    edit.Apply();
                    return;
                }
            }
            _ignoreNextChange = -1;
        }
    }
}
