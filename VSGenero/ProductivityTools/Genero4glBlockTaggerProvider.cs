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

using Microsoft.PowerToolsEx.BlockTagger;
using Microsoft.PowerToolsEx.BlockTagger.Implementation;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.ProductivityTools
{
    [TagType(typeof(IBlockTag)), Export(typeof(ITaggerProvider)), ContentType(VSGeneroConstants.LanguageName4GL)]
    public class Genero4glBlockTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(Microsoft.VisualStudio.Text.ITextBuffer buffer) where T : ITag
        {
            Func<GenericBlockTagger> creator = null;
            if(!(typeof(T) == typeof(IBlockTag)))
            {
                return null;
            }
            if(creator == null)
            {
                creator = () => new GenericBlockTagger(buffer, new Genero4glParser());
            }
            return (new DisposableTagger(buffer.Properties.GetOrCreateSingletonProperty<GenericBlockTagger>(typeof(Genero4glBlockTaggerProvider), creator)) as ITagger<T>);
        }
    }
}
