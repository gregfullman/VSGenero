using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing;

namespace VSGenero.EditorExtensions.Intellisense
{
    public class HighlightReferencesTagger : ITagger<TextMarkerTag>
    {
        // Fields
        private readonly Timer _timer;
        private readonly ITextBuffer buffer;
        private static readonly NormalizedSnapshotSpanCollection emptyCollection = new NormalizedSnapshotSpanCollection();
        private NormalizedSnapshotSpanCollection highlightReferencesSpans;
        private bool isActive;
        private readonly ITextView textView;
        private readonly HighlightReferencesTaggerProvider _provider;

        private object _notificationLock = new object();
        private bool _registeredForNotify = false;

        private const int Delay = 1000;

        // Events
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        // Methods
        private HighlightReferencesTagger(HighlightReferencesTaggerProvider provider, ITextBuffer buffer, ITextView textView)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (textView == null)
                throw new ArgumentNullException("textView");
            _timer = new Timer(UpdateAtCaretPosition, null, Timeout.Infinite, Timeout.Infinite);
            _provider = provider;
            this.buffer = buffer;
            this.textView = textView;
            this.highlightReferencesSpans = emptyCollection;
            this.textView.Closed += new EventHandler(this.OnTextView_Closed);
            this.textView.Caret.PositionChanged += Caret_PositionChanged;
            this.textView.LayoutChanged += textView_LayoutChanged;
            this.isActive = true;
        }

        void textView_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot)
            {
                _lastCaretPosition = textView.Caret.Position;
                _timer.Change(Delay, Timeout.Infinite);
            }
        }

        void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            _lastCaretPosition = e.NewPosition;
            _timer.Change(Delay, Timeout.Infinite);
        }

        private CaretPosition _lastCaretPosition;

        void UpdateAtCaretPosition(object state)
        {
            string filename = buffer.GetFilePath();
            if (_provider._PublicFunctionProvider != null)
                _provider._PublicFunctionProvider.SetFilename(filename);
            if (_provider._DatabaseInfoProvider != null)
                _provider._DatabaseInfoProvider.SetFilename(filename);
            if (_provider._ProgramFileProvider != null)
                _provider._ProgramFileProvider.SetFilename(filename);

            IGeneroProjectEntry analysisItem;
            if (buffer.TryGetAnalysis(out analysisItem))
            {
                lock (_notificationLock)
                {
                    if (!analysisItem.IsAnalyzed)
                    {
                        if (!_registeredForNotify)
                        {
                            _registeredForNotify = true;
                            // sign up for notification
                            analysisItem.OnNewAnalysis += analysisItem_OnNewAnalysis;
                        }
                        return;
                    }
                }
            }

            AnalyzeExpression(_lastCaretPosition);
        }

        private void AnalyzeExpression(CaretPosition position)
        {
            var vars = buffer.CurrentSnapshot.AnalyzeExpression(
                position.CreateTrackingSpan(buffer),
                false,
                _provider._PublicFunctionProvider,
                _provider._DatabaseInfoProvider,
                _provider._ProgramFileProvider
            );

            if (String.IsNullOrWhiteSpace(vars.Expression))
            {
                Clear();
                return;
            }

            if (vars.Value != null && vars.Variables != null)
            {
                IProjectEntry currProj = buffer.GetAnalysis();
                if (currProj != null)
                {
                    var references = vars.Variables
                                         .Where(x =>
                                             {
                                                 if (x.Location != null)
                                                 {
                                                     if ((x.Location.ProjectEntry != null && x.Location.ProjectEntry != currProj) ||
                                                        (!string.IsNullOrWhiteSpace(x.Location.FilePath) && !x.Location.FilePath.Equals(currProj.FilePath, StringComparison.OrdinalIgnoreCase)))
                                                     {
                                                         return false;
                                                     }
                                                 }
                                                 return true;
                                             })
                                         .Select(x => new IndexSpan(x.Location.Index, vars.Value.Name.Length));
                    if (references != null)
                    {
                        ITextSnapshot currentSnapshot = this.buffer.CurrentSnapshot;
                        NormalizedSnapshotSpanCollection spans = new NormalizedSnapshotSpanCollection(references.Select(x => x.ToSnapshotSpan(currentSnapshot)));
                        if (this.highlightReferencesSpans != spans)
                        {
                            this.highlightReferencesSpans = spans;
                            this.FireTagsChangedEvent();
                        }
                        return;
                    }
                }
            }
            Clear();
        }

        void analysisItem_OnNewAnalysis(object sender, EventArgs e)
        {
            // Well, maybe not...
            lock (_notificationLock)
            {
                _registeredForNotify = true;
                (sender as IGeneroProjectEntry).OnNewAnalysis -= analysisItem_OnNewAnalysis;
            }

            AnalyzeExpression(textView.Caret.Position);
        }

        public void Clear()
        {
            if (this.isActive)
            {
                this.ClearReferences();
            }
        }

        private void ClearReferences()
        {
            if (this.highlightReferencesSpans.Count > 0)
            {
                this.highlightReferencesSpans = emptyCollection;
                this.FireTagsChangedEvent();
            }
        }

        internal static HighlightReferencesTagger CreateInstance(HighlightReferencesTaggerProvider provider, ITextBuffer buffer, ITextView textView)
        {
            HighlightReferencesTagger tagger;
            if (textView == null)
                throw new ArgumentNullException("textView");
            Type key = typeof(HighlightReferencesTagger);
            if (!textView.Properties.TryGetProperty<HighlightReferencesTagger>(key, out tagger))
            {
                tagger = new HighlightReferencesTagger(provider, buffer, textView);
                textView.Properties.AddProperty(key, tagger);
            }
            return tagger;
        }

        private void FireTagsChangedEvent()
        {
            EventHandler<SnapshotSpanEventArgs> tagsChanged = this.TagsChanged;
            if (tagsChanged != null)
            {
                var snapSpan = new SnapshotSpan(textView.TextBuffer.CurrentSnapshot, 0, textView.TextBuffer.CurrentSnapshot.Length);
                tagsChanged(this, new SnapshotSpanEventArgs(snapSpan));
            }
        }

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            Func<SnapshotSpan, SnapshotSpan> selector = null;
            NormalizedSnapshotSpanCollection highlightReferencesSpans = this.highlightReferencesSpans;
            if ((this.isActive && (highlightReferencesSpans != null)) && ((highlightReferencesSpans.Count != 0) && (spans.Count != 0)))
            {
                SnapshotSpan span = spans[0];
                SnapshotSpan span2 = highlightReferencesSpans[0];
                if (span.Snapshot.TextBuffer == span2.Snapshot.TextBuffer)
                {
                    //SnapshotSpan span3 = highlightReferencesSpans[0];
                    //SnapshotSpan span4 = spans[0];
                    //if (span3.Snapshot != span4.Snapshot)
                    //{
                    //    if (selector == null)
                    //    {
                    //        selector = x => x.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive);
                    //    }
                    //    highlightReferencesSpans = new NormalizedSnapshotSpanCollection(highlightReferencesSpans.Select<SnapshotSpan, SnapshotSpan>(selector));
                    //}
                    //NormalizedSnapshotSpanCollection iteratorVariable1 = NormalizedSnapshotSpanCollection.Overlap(highlightReferencesSpans, spans);
                    //foreach (SnapshotSpan iteratorVariable2 in iteratorVariable1)
                    //{
                    //    yield return new TagSpan<TextMarkerTag>(iteratorVariable2, new HighlightReferencesTag());
                    //}
                    foreach (var snapSpan in highlightReferencesSpans)
                    {
                        yield return new TagSpan<TextMarkerTag>(snapSpan, new HighlightReferencesTag());
                    }
                }
            }
        }

        private void OnHighlightReferencesEngineShutdown()
        {
            if (this.isActive)
            {
                this.ClearReferences();
            }
        }

        private void OnTextView_Closed(object sender, EventArgs e)
        {
            this.textView.Closed -= new EventHandler(this.OnTextView_Closed);
            this.textView.Caret.PositionChanged -= Caret_PositionChanged;
            this.textView.LayoutChanged -= textView_LayoutChanged;
            Type key = typeof(HighlightReferencesTagger);
            this.textView.Properties.RemoveProperty(key);
            OnHighlightReferencesEngineShutdown();
            this.isActive = false;
            _timer.Dispose();
        }

        private void VerifyTaggerIsActive()
        {
            if (!this.isActive)
            {
                throw new InvalidOperationException("Tagger has already been shut down!");
            }
        }

    }
}
