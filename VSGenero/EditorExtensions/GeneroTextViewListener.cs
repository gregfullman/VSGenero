using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions
{
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [ContentType(VSGeneroConstants.ContentTypePER)]
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class Genero4GLTextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import(AllowDefault = true)]
        internal IGeneroTextViewChangedListener _changeListener;

        public Genero4GLTextViewCreationListener()
        {
            int i = 0;
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (_changeListener != null)
            {
                _changeListener.SetTextView(textView);
            }
        }
    }
}
