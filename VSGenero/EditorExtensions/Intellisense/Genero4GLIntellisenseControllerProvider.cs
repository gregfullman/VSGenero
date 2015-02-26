using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Snippets;

namespace VSGenero.EditorExtensions.Intellisense
{
    //[Export(typeof(IIntellisenseControllerProvider)), ContentType(VSGeneroConstants.ContentType4GL), Order]
    class Genero4GLIntellisenseControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        internal ICompletionBroker _CompletionBroker = null; // Set via MEF
        [Import]
        internal IEditorOperationsFactoryService _EditOperationsFactory = null; // Set via MEF
        [Import]
        internal IVsEditorAdaptersFactoryService _adaptersFactory { get; set; }
        [Import]
        internal ISignatureHelpBroker _SigBroker = null; // Set via MEF
        [Import]
        internal IQuickInfoBroker _QuickInfoBroker = null; // Set via MEF
        [Import]
        internal IIncrementalSearchFactoryService _IncrementalSearch = null; // Set via MEF
        [Import]
        internal IPublicFunctionSnippetizer _PublicFunctionSnippetizer = null;  // Set view MEF

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            Genero4GLIntellisenseController controller = null;
            if (!textView.TextBuffer.Properties.TryGetProperty<Genero4GLIntellisenseController>(typeof(Genero4GLIntellisenseController), out controller))
            {
                controller = new Genero4GLIntellisenseController(this, textView);
            }
            return controller;
        }
    }

    /// <summary>
    /// Monitors creation of text view adapters for Python code so that we can attach
    /// our keyboard filter.  This enables not using a keyboard pre-preprocessor
    /// so we can process all keys for text views which we attach to.  We cannot attach
    /// our command filter on the text view when our intellisense controller is created
    /// because the adapter does not exist.
    /// </summary>
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class Genero4GLTextViewCreationListener : IVsTextViewCreationListener
    {
        internal readonly IVsEditorAdaptersFactoryService _adaptersFactory;

        [ImportingConstructor]
        public Genero4GLTextViewCreationListener(IVsEditorAdaptersFactoryService adaptersFactory)
        {
            _adaptersFactory = adaptersFactory;
        }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = _adaptersFactory.GetWpfTextView(textViewAdapter);
            Genero4GLIntellisenseController controller;
            if (textView.TextBuffer.Properties.TryGetProperty<Genero4GLIntellisenseController>(typeof(Genero4GLIntellisenseController), out controller))
            {
                controller.AttachKeyboardFilter();
            }
        }

        #endregion
    }
}
