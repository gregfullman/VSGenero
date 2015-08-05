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

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.BraceCompletion
{
    [BracePair('"', '"'), 
     BracePair('[', ']'), 
     BracePair('(', ')'), 
     BracePair('\'', '\''), 
     ContentType(VSGeneroConstants.ContentType4GL),
     ContentType(VSGeneroConstants.ContentTypePER),
     ContentType(VSGeneroConstants.ContentTypeINC), 
     BracePair('{', '}'), /*BracePair('<', '>'),*/ 
     Export(typeof(IBraceCompletionContextProvider))]
    internal class Genero4glBraceContextProvider : IBraceCompletionContextProvider
    {
        // Fields
        private readonly ICompletionBroker completionBroker;
        private readonly ITextDocumentFactoryService documentFactory;
        //private readonly Lazy<IDECompilerHost> lazyHost;
        //private bool? roslynInstalled;
        //private static readonly Guid RoslynPackageGuid = new Guid("{6cf2e545-6109-4730-8883-cf43d7aec3e1}");

        // Methods
        [ImportingConstructor]
        public Genero4glBraceContextProvider(ITextDocumentFactoryService documentFactory, ICompletionBroker completionBroker)
        {
            this.documentFactory = documentFactory;
            this.completionBroker = completionBroker;
            //this.lazyHost = new Lazy<IDECompilerHost>(() => new IDECompilerHost(), LazyThreadSafetyMode.PublicationOnly);
        }

        private ITextDocument GetTextDocument(ITextBuffer buffer)
        {
            ITextDocument document;
            if (!this.documentFactory.TryGetTextDocument(buffer, out document))
            {
                return null;
            }
            return document;
        }

        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            context = null;
            //if (!this.roslynInstalled.HasValue)
            //{
            //    this.roslynInstalled = new bool?(this.CheckRoslyn());
            //}
            //if (!this.roslynInstalled.Value)
            //{
                ITextDocument document = null;
                if (/*!this.ServiceRunning() ||*/ !this.TryGetTextDocument(textView, out document))
                {
                    return false;
                }
                Genero4glLanguageInfo languageInfo = new Genero4glLanguageInfo(/*this.CompilationHost,*/ this.completionBroker, openingPoint.Snapshot.TextBuffer, document.FilePath);
                if (!languageInfo.IsValidContext(openingPoint))
                {
                    return false;
                }
                switch (openingBrace)
                {
                    //case '<':
                    //    if (!languageInfo.IsPossibleTypeVariableDecl(openingPoint))
                    //    {
                    //        return false;
                    //    }
                    //    context = new NormalCompletionContext(languageInfo);
                    //    return true;

                    case '[':
                    case '(':
                        context = new NormalCompletionContext(languageInfo);
                        return true;

                    case '{':
                        context = new CurlyBraceContext(languageInfo);
                        return true;

                    case '\'':
                    case '"':
                        context = new LiteralCompletionContext(languageInfo);
                        return true;
                }
            //}
            return false;
        }

        private bool TryGetTextDocument(ITextView view, out ITextDocument document)
        {
            ITextBuffer textBuffer = view.BufferGraph.GetTextBuffers(b => this.GetTextDocument(b) != null).FirstOrDefault<ITextBuffer>();
            return this.documentFactory.TryGetTextDocument(textBuffer, out document);
        }

        //// Properties
        //private IDECompilerHost CompilationHost
        //{
        //    get
        //    {
        //        return this.lazyHost.Value;
        //    }
        //}
    }
}
