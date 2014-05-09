/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 * 
 * Contents of this file are based on the MSDN walkthrough here:
 * http://msdn.microsoft.com/en-us/library/ee197665.aspx
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.VSCommon;

namespace VSGenero.EditorExtensions
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    internal sealed class Genero4GLOutliningTaggerProvider : ITaggerProvider
    {
        [Import(AllowDefault = true)]
        private IProgram4GLFileProvider _program4glFileProvider;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (_program4glFileProvider != null &&
               VSGeneroPackage.Instance.CurrentProgram4GLFileProvider == null)
            {
                VSGeneroPackage.Instance.CurrentProgram4GLFileProvider = _program4glFileProvider;
            }

            //create a single tagger for each buffer.
            Func<ITagger<T>> sc = delegate() { return new Genero4GLOutliner(buffer) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(sc);
        }
    }

    public sealed class Genero4GLOutliner : ITagger<IOutliningRegionTag>
    {
        string ellipsis = "...";    //the characters that are displayed when the region is collapsed
        ITextBuffer buffer;
        ITextSnapshot snapshot;
        GeneroModuleContents _moduleContents;
        List<Region> regions;

        public Genero4GLOutliner(ITextBuffer buffer)
        {
            this.buffer = buffer;
            this.buffer.Changed += BufferChanged;
            this.snapshot = buffer.CurrentSnapshot;
            this.regions = new List<Region>();

            GeneroFileParserManager fpm = VSGeneroPackage.Instance.UpdateBufferFileParserManager(buffer);
            fpm.ParseComplete += Genero4GLOutliner_ParseComplete;
            ForceReoutline(fpm);
        }

        void Genero4GLOutliner_ParseComplete(object sender, ParseCompleteEventArgs e)
        {
            ForceReoutline(sender as GeneroFileParserManager);
        }

        private void ForceReoutline(GeneroFileParserManager fpm)
        {
            _moduleContents = fpm.ModuleContents;
            ReOutline();
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // TODO: ok, what I want to do is queue up a re-outline request to basically wait for idleness
            // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).
            //if (e.After != buffer.CurrentSnapshot)
            //    return;

            //ReOutline();

            // TODO: we can use this as a spot to insert the xml doc snippet?
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;
            List<Region> currentRegions = this.regions;
            ITextSnapshot currentSnapshot = spans[0].Snapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);
            int startPos = entire.Start.Position;
            int endPos = entire.End.Position;
            foreach (var region in currentRegions)
            {
                if (region.Start <= endPos &&
                    region.End >= startPos)
                {
                    int end = region.End;
                    if (end >= currentSnapshot.Length)
                        end = currentSnapshot.Length - 1;
                    if (currentSnapshot.Length == 0)
                        end = 0;
                    //the region starts at the beginning of the "[", and goes until the *end* of the line that contains the "]".
                    if (end > region.Start)
                    {
                        yield return new TagSpan<IOutliningRegionTag>(
                            new SnapshotSpan(currentSnapshot,
                                region.Start, end - region.Start),
                            new OutliningRegionTag(false, false, (region.CollapsedText + ellipsis), String.Empty));
                    }
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        void ReOutline()
        {
            if (_moduleContents != null)
            {
                ITextSnapshot newSnapshot = buffer.CurrentSnapshot;
                List<Region> newRegions = new List<Region>();

                if (_moduleContents.FunctionDefinitions != null)
                {
                    FindHiddenRegions(newSnapshot, ref newRegions, buffer.GetFilePath());
                }

                //determine the changed span, and send a changed event with the new spans
                List<Span> oldSpans =
                    new List<Span>(this.regions.Select(r => AsSnapshotSpan(r, this.snapshot)
                        .TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive)
                        .Span));
                List<Span> newSpans =
                        new List<Span>(newRegions.Select(r => AsSnapshotSpan(r, newSnapshot).Span));

                NormalizedSpanCollection oldSpanCollection = new NormalizedSpanCollection(oldSpans);
                NormalizedSpanCollection newSpanCollection = new NormalizedSpanCollection(newSpans);

                //the changed regions are regions that appear in one set or the other, but not both.
                NormalizedSpanCollection removed =
                NormalizedSpanCollection.Difference(oldSpanCollection, newSpanCollection);

                int changeStart = int.MaxValue;
                int changeEnd = -1;

                if (removed.Count > 0)
                {
                    changeStart = removed[0].Start;
                    changeEnd = removed[removed.Count - 1].End;
                }

                if (newSpans.Count > 0)
                {
                    changeStart = Math.Min(changeStart, newSpans[0].Start);
                    changeEnd = Math.Max(changeEnd, newSpans[newSpans.Count - 1].End);
                }

                this.snapshot = newSnapshot;
                this.regions = newRegions;

                if (changeStart <= changeEnd)
                {
                    ITextSnapshot snap = this.snapshot;
                    if (this.TagsChanged != null)
                        this.TagsChanged(this, new SnapshotSpanEventArgs(
                            new SnapshotSpan(this.snapshot, Span.FromBounds(changeStart, changeEnd))));
                }
            }
        }

        void FindHiddenRegions(ITextSnapshot snapShot, ref List<Region> regions, string currentFilename)
        {
            foreach (var function in _moduleContents.FunctionDefinitions.Where(x => x.Value.ContainingFile == currentFilename))
            {
                var region = new Region();
                region.Start = function.Value.Position;
                region.End = function.Value.End;
                string collapsedText = "";
                if (function.Value.Main)
                {
                    collapsedText = "main";
                }
                else if (function.Value.Report)
                {
                    if (function.Value.Private)
                        collapsedText = "private ";
                    collapsedText += "report " + function.Value.Name;
                }
                else
                {
                    if (function.Value.Private)
                        collapsedText = "private ";
                    collapsedText += "function " + function.Value.Name;
                }
                region.CollapsedText = collapsedText;
                regions.Add(region);
                
            }
        }

        static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
        {
            int end = region.End;
            if (end > snapshot.Length)
                end = snapshot.Length;  // TODO: not sure if this should be -1
            int start = region.Start;
            if (start > end)
                start = end;
            if (start < 0 || end < 0)
                return new SnapshotSpan(snapshot, 0, 0);
            return new SnapshotSpan(snapshot, start, end - start);
        }

        class Region
        {
            public int Start { get; set; }
            public int End { get; set; }
            public string CollapsedText { get; set; }
        } 
    }
}
