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

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(IQuickInfoSourceProvider)), ContentType(VSGeneroConstants.ContentType4GL), ContentType(VSGeneroConstants.ContentTypeINC), Order, Name("Genero 4GL Quick Info Source")]
    class QuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import(AllowDefault = true)]
        internal IFunctionInformationProvider _PublicFunctionProvider = null;

        [Import(AllowDefault = true)]
        internal IDatabaseInformationProvider _DatabaseInfoProvider = null;

        [Import(AllowDefault = true)]
        internal IProgramFileProvider _ProgramFileProvider = null;

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource(this, textBuffer);
        }
    }
}
