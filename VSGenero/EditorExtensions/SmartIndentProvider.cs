using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [ContentType(VSGeneroConstants.ContentTypeINC)]
    public sealed class SmartIndentProvider : ISmartIndentProvider
    {
        private sealed class Indent : ISmartIndent
        {
            private readonly ITextView _textView;

            public Indent(ITextView view)
            {
                _textView = view;
                AutoIndent.Initialize();
            }

            /// <summary>
            /// This is called when the enter key is pressed or when navigating to an empty line.
            /// </summary>
            /// <param name="line"></param>
            /// <returns></returns>
            public int? GetDesiredIndentation(ITextSnapshotLine line)
            {
                if (VSGeneroPackage.Instance.LangPrefs.IndentMode == vsIndentStyle.vsIndentStyleSmart)
                {
                    return AutoIndent.GetLineIndentation(line, _textView);
                }
                else
                {
                    return null;
                }
            }

            public void Dispose()
            {
            }
        }

        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new Indent(textView);
        }
    }
}
