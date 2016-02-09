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
    [ContentType(VSGeneroConstants.ContentTypePER)]
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
