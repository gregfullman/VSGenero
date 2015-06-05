using Microsoft.PowerToolsEx.BlockTagger;
using Microsoft.PowerToolsEx.BlockTagger.Implementation;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.VS2013Plus.ProductivityTools
{
    internal class DisposableTagger : ITagger<IBlockTag>, IDisposable
    {
        // Fields
        private GenericBlockTagger _tagger;

        // Events
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        // Methods
        public DisposableTagger(GenericBlockTagger tagger)
        {
            this._tagger = tagger;
            this._tagger.AddRef();
            this._tagger.TagsChanged += new EventHandler<SnapshotSpanEventArgs>(this.OnTagsChanged);
        }

        public void Dispose()
        {
            if (this._tagger != null)
            {
                this._tagger.TagsChanged -= new EventHandler<SnapshotSpanEventArgs>(this.OnTagsChanged);
                this._tagger.Release();
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
