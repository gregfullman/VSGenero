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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.ProductivityTools
{
    internal class DisposableTagger : ITagger<IBlockTag>, IDisposable
    {
        // Fields
        private ITagger<IBlockTag> _tagger;

        // Events
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        // Methods
        public DisposableTagger(ITagger<IBlockTag> tagger)
        {
            this._tagger = tagger;
            //this._tagger.AddRef();
            this._tagger.TagsChanged += new EventHandler<SnapshotSpanEventArgs>(this.OnTagsChanged);
        }

        public void Dispose()
        {
            if (this._tagger != null)
            {
                this._tagger.TagsChanged -= new EventHandler<SnapshotSpanEventArgs>(this.OnTagsChanged);
                //this._tagger.Release();
                if(this._tagger is IDisposable)
                {
                    (this._tagger as IDisposable).Dispose();
                }
                this._tagger = null;
            }
        }

        public IEnumerable<ITagSpan<IBlockTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return this._tagger.GetTags(spans);
        }

        private void OnTagsChanged(object sender, SnapshotSpanEventArgs e)
        {
            EventHandler<SnapshotSpanEventArgs> tagsChanged = this.TagsChanged;
            if (tagsChanged != null)
            {
                tagsChanged(sender, e);
            }
        }
    }

}
