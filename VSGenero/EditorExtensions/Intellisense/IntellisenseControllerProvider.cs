using Microsoft.VisualStudio.ComponentModelHost;
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
using VSGenero.Analysis;
using Microsoft.VisualStudio.VSCommon;
using VSGenero.Snippets;
using Microsoft.VisualStudio.Shell;

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(IIntellisenseControllerProvider)), ContentType(VSGeneroConstants.ContentType4GL), ContentType(VSGeneroConstants.ContentTypeINC), Order]
    class IntellisenseControllerProvider : IIntellisenseControllerProvider
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
        [Import(AllowDefault = true)]
        internal IPublicFunctionSnippetizer _PublicFunctionSnippetizer = null;  // Set view MEF

        [Import(AllowDefault = true)]
        internal IFunctionInformationProvider _PublicFunctionProvider = null;

        [Import(AllowDefault = true)]
        internal IDatabaseInformationProvider _DatabaseInfoProvider = null;

        [Import(AllowDefault = true)]
        internal IProgramFileProvider _ProgramFileProvider = null;

        [Import(AllowDefault = true)]
        internal IGeneroTextViewCommandTarget GeneroCommandTarget;

        internal IServiceProvider _ServiceProvider;

        [ImportingConstructor]
        public IntellisenseControllerProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _ServiceProvider = serviceProvider;
        }

        internal static IntellisenseControllerProvider Instance { get; private set; }

        readonly Dictionary<ITextView, Tuple<BufferParser, GeneroProjectAnalyzer>> _hookedCloseEvents =
        new Dictionary<ITextView, Tuple<BufferParser, GeneroProjectAnalyzer>>();

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            IntellisenseController controller;
            if (!textView.Properties.TryGetProperty<IntellisenseController>(typeof(IntellisenseController), out controller))
            {
                controller = new IntellisenseController(this, textView, _ServiceProvider);
            }

            var analyzer = textView.GetAnalyzer();
            if (analyzer != null)
            {
                var buffer = subjectBuffers[0];
                if(_PublicFunctionProvider != null)
                    _PublicFunctionProvider.SetFilename(buffer.GetFilePath());
                if (_DatabaseInfoProvider != null)
                    _DatabaseInfoProvider.SetFilename(buffer.GetFilePath());
                if (_ProgramFileProvider != null)
                {
                    _ProgramFileProvider.SetFilename(buffer.GetFilePath());
                    if (VSGeneroPackage.Instance.ProgramFileProvider == null)
                        VSGeneroPackage.Instance.ProgramFileProvider = _ProgramFileProvider;
                }
                foreach (var subjBuf in subjectBuffers)
                {
                    controller.PropagateAnalyzer(subjBuf);
                }

                var entry = analyzer.MonitorTextBuffer(textView, buffer);
                _hookedCloseEvents[textView] = Tuple.Create(entry.BufferParser, analyzer);
                textView.Closed += TextView_Closed;

                for (int i = 1; i < subjectBuffers.Count; i++)
                {
                    entry.BufferParser.AddBuffer(subjectBuffers[i]);
                }
                controller.SetBufferParser(entry.BufferParser);
            }
            return controller;
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            var textView = sender as ITextView;
            Tuple<BufferParser, GeneroProjectAnalyzer> tuple;
            if (textView == null || !_hookedCloseEvents.TryGetValue(textView, out tuple))
            {
                return;
            }

            textView.Closed -= TextView_Closed;
            _hookedCloseEvents.Remove(textView);

            if (tuple.Item1.AttachedViews == 0)
            {
                tuple.Item2.StopMonitoringTextBuffer(tuple.Item1);
            }
        }

        //internal static IntellisenseController GetOrCreateController(IComponentModel model, ITextView textView)
        //{
        //    IntellisenseController controller;
        //    if (!textView.Properties.TryGetProperty<IntellisenseController>(typeof(IntellisenseController), out controller))
        //    {
        //        var intellisenseControllerProvider = (
        //           from export in model.DefaultExportProvider.GetExports<IIntellisenseControllerProvider, IContentTypeMetadata>()
        //           from exportedContentType in export.Metadata.ContentTypes
        //           where (exportedContentType == VSGeneroConstants.ContentType4GL || 
        //                  exportedContentType == VSGeneroConstants.ContentTypeINC ||
        //                  exportedContentType == VSGeneroConstants.ContentTypePER) && export.Value.GetType() == typeof(IntellisenseControllerProvider)
        //           select export.Value
        //        ).First();
        //        controller = new IntellisenseController((IntellisenseControllerProvider)intellisenseControllerProvider, textView);
        //    }
        //    return controller;
        //}
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
    [ContentType(VSGeneroConstants.ContentTypeINC)]
    [ContentType(VSGeneroConstants.ContentTypePER)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class TextViewCreationListener : IVsTextViewCreationListener
    {
        internal readonly IVsEditorAdaptersFactoryService _adaptersFactory;

        [ImportingConstructor]
        public TextViewCreationListener(IVsEditorAdaptersFactoryService adaptersFactory)
        {
            _adaptersFactory = adaptersFactory;
        }

        #region IVsTextViewCreationListener Members

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = _adaptersFactory.GetWpfTextView(textViewAdapter);
            IntellisenseController controller;
            if (textView.Properties.TryGetProperty<IntellisenseController>(typeof(IntellisenseController), out controller))
            {
                controller.AttachKeyboardFilter();
            }
        }

        #endregion
    }

    /// <summary>
    /// Metadata which includes Ordering and Content Types
    /// 
    /// New in 1.1.
    /// </summary>
    public interface IOrderableContentTypeMetadata : IContentTypeMetadata, IOrderable
    {
    }

    /// <summary>
    /// New in 1.1
    /// </summary>
    public interface IContentTypeMetadata
    {
        IEnumerable<string> ContentTypes { get; }
    }
}
