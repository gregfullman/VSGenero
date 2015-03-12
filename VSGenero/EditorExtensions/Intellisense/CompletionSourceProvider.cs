using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(ICompletionSourceProvider)), ContentType(VSGeneroConstants.ContentType4GL), Order, Name("CompletionProvider")]
    internal class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal IGlyphService _glyphService = null; // Assigned from MEF

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new CompletionSource(this, textBuffer);
        }
    }
}
