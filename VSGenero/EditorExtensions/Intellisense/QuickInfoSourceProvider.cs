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
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Interfaces;
using VSGenero.External;
using VSGenero.External.Interfaces;

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(IQuickInfoSourceProvider)),
     ContentType(External.GeneroConstants.ContentType4GL),
     ContentType(External.GeneroConstants.ContentTypeINC),
     ContentType(External.GeneroConstants.ContentTypePER),
     Order, 
     Name("Genero Quick Info Source")]
    class QuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import(AllowDefault = true)]
        internal IFunctionInformationProvider _PublicFunctionProvider = null;

        [Import(AllowDefault = true)]
        internal IDatabaseInformationProvider _DatabaseInfoProvider = null;

        [Import(AllowDefault = true)]
        internal IProgramFileProvider _ProgramFileProvider = null;

        [Import(AllowDefault = true)]
        internal IGeneroDebugger _GeneroDebugger = null;

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            var infoSource = new QuickInfoSource(this, textBuffer);
            if(_GeneroDebugger != null)
            {
                _GeneroDebugger.SetTypeResolver(infoSource);
            }
            return infoSource;
        }
    }
}
