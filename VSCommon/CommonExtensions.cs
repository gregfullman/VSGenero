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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Language.Intellisense;
using System.IO;
using Microsoft.VisualStudio.Language.StandardClassification;

namespace Microsoft.VisualStudio.VSCommon
{
    public static class VSCommonExtensions
    {
        public static string GetFilePath(this ITextView textView)
        {
            return textView.TextBuffer.GetFilePath();
        }

        public static string GetFilePath(this ITextBuffer textBuffer)
        {
            ITextDocument textDocument;
            if (textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument))
            {
                return textDocument.FilePath;
            }
            else
            {
                IVsTextBuffer bufferAdapter;
                textBuffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out bufferAdapter);
                if (bufferAdapter != null)
                {
                    var persistFileFormat = bufferAdapter as IPersistFileFormat;
                    string filename = null;
                    uint i;
                    if (persistFileFormat != null)
                    {
                        persistFileFormat.GetCurFile(out filename, out i);
                        return filename;
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the span to use for the provided intellisense session.
        /// </summary>
        /// <returns>A tracking span. The span may be of length zero if there
        /// is no suitable token at the trigger point.</returns>
        internal static ITrackingSpan GetApplicableSpan(this IIntellisenseSession session, ITextBuffer buffer)
        {
            var snapshot = buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(buffer);

            var span = snapshot.GetApplicableSpan(triggerPoint);
            if (span != null)
            {
                return span;
            }
            return snapshot.CreateTrackingSpan(triggerPoint.GetPosition(snapshot), 0, SpanTrackingMode.EdgeInclusive);
        }

        /// <summary>
        /// Returns the applicable span at the provided position.
        /// </summary>
        /// <returns>A tracking span, or null if there is no token at the
        /// provided position.</returns>
        public static ITrackingSpan GetApplicableSpan(this ITextSnapshot snapshot, ITrackingPoint point)
        {
            return snapshot.GetApplicableSpan(point.GetPosition(snapshot));
        }

        /// <summary>
        /// Returns the applicable span at the provided position.
        /// </summary>
        /// <returns>A tracking span, or null if there is no token at the
        /// provided position.</returns>
        public static ITrackingSpan GetApplicableSpan(this ITextSnapshot snapshot, int position)
        {
            //var classifier = snapshot.TextBuffer.GetPythonClassifier();
            IClassifier classifier = null;
            var line = snapshot.GetLineFromPosition(position);
            if (classifier == null || line == null)
            {
                return null;
            }

            var spanLength = position - line.Start.Position;
            // Increase position by one to include 'fob' in: "abc.|fob"
            if (spanLength < line.Length)
            {
                spanLength += 1;
            }

            var classifications = classifier.GetClassificationSpans(new SnapshotSpan(line.Start, spanLength));
            // Handle "|"
            if (classifications == null || classifications.Count == 0)
            {
                return null;
            }

            var lastToken = classifications[classifications.Count - 1];
            // Handle "fob |"
            if (lastToken == null || position > lastToken.Span.End)
            {
                return null;
            }

            if (position > lastToken.Span.Start)
            {
                if (lastToken.CanComplete())
                {
                    // Handle "fo|o"
                    return snapshot.CreateTrackingSpan(lastToken.Span, SpanTrackingMode.EdgeInclusive);
                }
                else
                {
                    // Handle "<|="
                    return null;
                }
            }

            var secondLastToken = classifications.Count >= 2 ? classifications[classifications.Count - 2] : null;
            if (lastToken.Span.Start == position && lastToken.CanComplete() &&
                (secondLastToken == null ||             // Handle "|fob"
                 position > secondLastToken.Span.End || // Handle "if |fob"
                 !secondLastToken.CanComplete()))
            {     // Handle "abc.|fob"
                return snapshot.CreateTrackingSpan(lastToken.Span, SpanTrackingMode.EdgeInclusive);
            }

            // Handle "abc|."
            // ("ab|c." would have been treated as "ab|c")
            if (secondLastToken != null && secondLastToken.Span.End == position && secondLastToken.CanComplete())
            {
                return snapshot.CreateTrackingSpan(secondLastToken.Span, SpanTrackingMode.EdgeInclusive);
            }

            return null;
        }

        internal static bool CanComplete(this ClassificationSpan token)
        {
            return token.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Keyword) |
                token.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Identifier);
        }

        /// <summary>
        /// Return true if both paths represent the same location.
        /// </summary>
        public static bool IsSamePath(string file1, string file2)
        {
            if (string.IsNullOrEmpty(file1))
            {
                return string.IsNullOrEmpty(file2);
            }
            else if (string.IsNullOrEmpty(file2))
            {
                return false;
            }

            if (String.Equals(file1, file2, StringComparison.Ordinal))
            {
                // Quick return, but will only work where the paths are already normalized and
                // have matching case.
                return true;
            }

            Uri uri1, uri2;
            return
                TryMakeUri(file1, false, UriKind.Absolute, out uri1) &&
                TryMakeUri(file2, false, UriKind.Absolute, out uri2) &&
                uri1 == uri2;
        }

        internal static bool TryMakeUri(string path, bool isDirectory, UriKind kind, out Uri uri)
        {
            if (isDirectory && !string.IsNullOrEmpty(path) && !HasEndSeparator(path))
            {
                path += Path.DirectorySeparatorChar;
            }

            return Uri.TryCreate(path, kind, out uri);
        }

        /// <summary>
        /// Returns true if the path has a directory separator character at the end.
        /// </summary>
        public static bool HasEndSeparator(string path)
        {
            return (!string.IsNullOrEmpty(path) &&
                (path[path.Length - 1] == Path.DirectorySeparatorChar ||
                 path[path.Length - 1] == Path.AltDirectorySeparatorChar));
        }

        public static void CopyStream(Stream input, Stream output)
        {
            // Insert null checking here for production
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
