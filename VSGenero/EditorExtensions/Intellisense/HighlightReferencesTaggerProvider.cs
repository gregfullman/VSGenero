using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;

namespace VSGenero.EditorExtensions.Intellisense
{
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [ContentType(VSGeneroConstants.ContentTypeINC)]
    [Name("Genero Highlight References Tagger Provider")]
    [TagType(typeof(TextMarkerTag))]
    [Export(typeof(IViewTaggerProvider))]
    public class HighlightReferencesTaggerProvider : IViewTaggerProvider
    {
        // Fields
        [Import(AllowDefault = true)]
        public SVsServiceProvider serviceProvider;
        private readonly Dictionary<ITextView, HighlightReferencesTagger> taggerToViewMapping = new Dictionary<ITextView, HighlightReferencesTagger>();

        [Import(AllowDefault = true)]
        internal IFunctionInformationProvider _PublicFunctionProvider = null;

        [Import(AllowDefault = true)]
        internal IDatabaseInformationProvider _DatabaseInfoProvider = null;

        [Import(AllowDefault = true)]
        internal IProgramFileProvider _ProgramFileProvider = null;

        // Methods
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return (HighlightReferencesTagger.CreateInstance(this, buffer, textView) as ITagger<T>);
        }

    }
}
