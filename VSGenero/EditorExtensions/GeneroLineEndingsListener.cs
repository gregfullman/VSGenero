using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.VSCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions
{
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class GeneroLineEndingsListener : IWpfTextViewCreationListener
    {
        private IWpfTextView _textView;
        private string _lineEnding;

        public void TextViewCreated(IWpfTextView textView)
        {
            _textView = textView;
            _textView.TextBuffer.Changed += TextBuffer_Changed;
            _textView.TextBuffer.Properties.AddProperty(typeof(GeneroLineEndingsListener), this);
        }

        public void Unregister()
        {
            _textView.TextBuffer.Changed -= TextBuffer_Changed;
        }

        void TextBuffer_Changed(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            // first determine what line endings we currently have
            if (_lineEnding == null && _textView.TextBuffer.CurrentSnapshot.LineCount > 0)
            {
                foreach(var line in _textView.TextBuffer.CurrentSnapshot.Lines)
                {
                    var lineEnding = line.GetLineBreakText();
                    if(!string.IsNullOrEmpty(lineEnding))
                    {
                        _lineEnding = lineEnding;
                        break;
                    }
                }
            }

            foreach (var change in e.Changes)
            {
                bool inconsistenciesExist = false;
                var lines = change.GetLines();
                for (int i = 0; i < lines.Count; i++)
                {
                    string currLineEnding = lines[i].GetLineEnding();
                    if (!string.IsNullOrEmpty(currLineEnding) && currLineEnding != _lineEnding)
                    {
                        lines[i] = lines[i].ReplaceLineEnding(_lineEnding);
                        inconsistenciesExist = true;
                    }
                }

                if (inconsistenciesExist)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var line in lines)
                    {
                        sb.Append(line);
                    }
                    ITextEdit edit = _textView.TextBuffer.CreateEdit();
                    edit.Replace(change.NewSpan, sb.ToString());
                    edit.Apply();
                }
            }
        }
    }
}
