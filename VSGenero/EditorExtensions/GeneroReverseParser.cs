/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
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
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace VSGenero.EditorExtensions
{
    public class GeneroReverseParser : IEnumerable<ITagSpan<ClassificationTag>>
    {
        private readonly ITextSnapshot _snapshot;
        private readonly ITextBuffer _buffer;
        private readonly ITrackingSpan _span;
        private ITextSnapshotLine _curLine;
        private GeneroClassifier _classifier;
        private readonly bool _multiLine;

        public GeneroClassifier Classifier
        {
            get { return _classifier ?? (_classifier = (GeneroClassifier)_buffer.Properties.GetProperty(typeof(GeneroClassifier))); }
        }

        public GeneroReverseParser(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span, bool multiLine = false)
        {
            _snapshot = snapshot;
            _buffer = buffer;
            _span = span;
            _multiLine = multiLine;

            var loc = span.GetSpan(snapshot);
            var line = _curLine = snapshot.GetLineFromPosition(loc.Start);

            var targetSpan = new Span(line.Start.Position, span.GetEndPoint(snapshot).Position - line.Start.Position);
        }

        public GeneroTokenType GetTokenType(IClassificationType classificationType)
        {
            return Classifier.GetTokenType(classificationType);
        }

        public IEnumerator<ITagSpan<ClassificationTag>> GetEnumerator()
        {
            return ReverseTagSpanEnumerator(Classifier, _span.GetSpan(_snapshot).End);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ReverseTagSpanEnumerator(Classifier, _span.GetSpan(_snapshot).End);
        }

        internal IEnumerator<ITagSpan<ClassificationTag>> ReverseTagSpanEnumerator(GeneroClassifier classifier, SnapshotPoint startPoint)
        {
            var startLine = startPoint.GetContainingLine();
            int curLine = startLine.LineNumber;

            for (; ; )
            {
                if (curLine == startLine.LineNumber)
                {
                    foreach (var token in classifier.GetTags(new NormalizedSnapshotSpanCollection(new SnapshotSpan(startLine.Start, startPoint))).Reverse())
                    {
                        yield return token;
                    }
                }
                else
                {
                    if (curLine >= 0)
                    {
                        var prevLine = startPoint.Snapshot.GetLineFromLineNumber(curLine);
                        foreach (var token in classifier.GetTags(new NormalizedSnapshotSpanCollection(prevLine.Extent)).Reverse())
                        {
                            yield return token;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (!_multiLine)
                {
                    // indicate the line break
                    yield return null;
                }

                curLine--;
                
            }
        }
    }
}
