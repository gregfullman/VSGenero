/* ****************************************************************************
 * Copyright (c) 2014 Greg Fullman 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using VSGenero.EditorExtensions;
using VSGenero.Navigation;
using VSGenero.Analysis;

namespace VSGenero.VS2013Plus
{
    [SupportsStandaloneFiles(true), Export(typeof(IPeekableItemSourceProvider)), Name("GeneroPeekableItemSourceProvider"), ContentType(VSGeneroConstants.ContentType4GL), ContentType(VSGeneroConstants.ContentTypeINC)]
    public class PeekableItemSourceProvider : IPeekableItemSourceProvider
    {
        [Import]
        public IPeekResultFactory PeekResultFactory { get; set;}

        [Import(AllowDefault=true)]
        public SVsServiceProvider ServiceProvider;

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        public IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        public ITextBuffer CurrentBuffer { get; private set; }

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            CurrentBuffer = textBuffer;
            return new PeekableItemSource(textBuffer, this);
        }
    }

    public class PeekableItemSource : IPeekableItemSource
    {
        private ITextBuffer _buffer;
        private PeekableItemSourceProvider _factory;
        private ITextDocumentFactoryService _textDocumentFactoryService;

        public PeekableItemSource(ITextBuffer buffer, PeekableItemSourceProvider factory)
        {
            _buffer = buffer;
            _factory = factory;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            if(session == null)
            {
                throw new ArgumentNullException("session");
            }
            if(peekableItems == null)
            {
                throw new ArgumentNullException("peekableItems");
            }
            if(session.RelationshipName == PredefinedPeekRelationships.Definitions.Name)
            {
                ITextDocument document;
                SnapshotPoint? triggerPoint = session.GetTriggerPoint(this._buffer.CurrentSnapshot);
                if(triggerPoint.HasValue && this.TryGetTextDocument(_buffer, out document))
                {
                    if(!session.TextView.TextBuffer.Properties.ContainsProperty(typeof(ITextDocument)))
                    {
                        session.TextView.TextBuffer.Properties.AddProperty(typeof(ITextDocument), document);
                    }

                    peekableItems.Add(new PeekableItem(EditFilter.GetLocations(session.TextView, EditFilter.GetLocationOptions.Definitions), _factory));
                }
            }
        }

        private bool TryGetTextDocument(ITextBuffer buffer, out ITextDocument textDocument)
        {
            if(this._textDocumentFactoryService == null)
            {
                this._textDocumentFactoryService = (ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel).GetService<ITextDocumentFactoryService>();
            }
            ITextDocument document = null;
            if(this._textDocumentFactoryService.TryGetTextDocument(buffer, out document))
            {
                textDocument = document;
                return true;
            }
            textDocument = null;
            return false;
        }

        public void Dispose()
        {
        }
    }
}
