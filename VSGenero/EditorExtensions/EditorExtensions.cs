/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using System.IO;
using Microsoft.VisualStudio.VSCommon;

namespace VSGenero.EditorExtensions
{
    public static class EditorExtensions
    {
        public static string ReplaceLineEnding(this string line, string lineEnding)
        {
            StringBuilder sb = new StringBuilder(line);
            var currLineEnding = line.GetLineEnding();
            sb.Remove(line.Length - currLineEnding.Length, currLineEnding.Length);
            sb.Append(lineEnding);
            return sb.ToString();
        }

        public static string GetLineEnding(this string line)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = line.Length - 1; i >= 0; i--)
            {
                if(line[i] == '\n' || line[i] == '\r')
                {
                    sb.Insert(0, line[i]);
                }
                else
                {
                    break;
                }
            }
            return sb.ToString();
        }

        public static List<string> GetLines(this ITextChange change)
        {
            List<string> lines = new List<string>();
            var changeLength = change.NewText.Length;
            if (changeLength > 0)
            {
                StringBuilder sb = new StringBuilder();
                int currInd = 0;
                do
                {
                    sb.Append(change.NewText[currInd]);
                    if(change.NewText[currInd] == '\n')
                    {
                        lines.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                while (++currInd < changeLength);

                if (sb.Length > 0)
                    lines.Add(sb.ToString());
            }
            return lines;
        }

        public static bool CommentOrUncommentBlock(ITextView view, bool comment)
        {
            SnapshotPoint start, end;
            SnapshotPoint? mappedStart, mappedEnd;

            if (view.Selection.IsActive && !view.Selection.IsEmpty)
            {
                // comment every line in the selection
                start = view.Selection.Start.Position;
                end = view.Selection.End.Position;
                mappedStart = MapPoint(view, start);

                var endLine = end.GetContainingLine();
                if (endLine.Start == end)
                {
                    // http://pytools.codeplex.com/workitem/814
                    // User selected one extra line, but no text on that line.  So let's
                    // back it up to the previous line.  It's impossible that we're on the
                    // 1st line here because we have a selection, and we end at the start of
                    // a line.  In normal selection this is only possible if we wrapped onto the
                    // 2nd line, and it's impossible to have a box selection with a single line.
                    end = end.Snapshot.GetLineFromLineNumber(endLine.LineNumber - 1).End;
                }

                mappedEnd = MapPoint(view, end);
            }
            else
            {
                // comment the current line
                start = end = view.Caret.Position.BufferPosition;
                mappedStart = mappedEnd = MapPoint(view, start);
            }

            if (mappedStart != null && mappedEnd != null &&
                mappedStart.Value <= mappedEnd.Value)
            {
                if (comment)
                {
                    CommentRegion(view, mappedStart.Value, mappedEnd.Value);
                }
                else
                {
                    UncommentRegion(view, mappedStart.Value, mappedEnd.Value);
                }

                // TODO: select multiple spans?
                // Select the full region we just commented, do not select if in projection buffer 
                // (the selection might span non-language buffer regions)
                if (VSGeneroConstants.IsGenero4GLContent(view.TextBuffer))
                {
                    UpdateSelection(view, start, end);
                }
                return true;
            }

            return false;
        }

        private static SnapshotPoint? MapPoint(ITextView view, SnapshotPoint point)
        {
            return view.BufferGraph.MapDownToFirstMatch(
               point,
               PointTrackingMode.Positive,
               VSGeneroConstants.IsGenero4GLContent,
               PositionAffinity.Successor
            );
        }

        private static void UpdateSelection(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            view.Selection.Select(
                new SnapshotSpan(
                // translate to the new snapshot version:
                    start.GetContainingLine().Start.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Negative),
                    end.GetContainingLine().End.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Positive)
                ),
                false
            );
        }

        /// <summary>
        /// Adds comment characters (#) to the start of each line.  If there is a selection the comment is applied
        /// to each selected line.  Otherwise the comment is applied to the current line.
        /// </summary>
        /// <param name="view"></param>
        private static void CommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            //Debug.Assert(start.Snapshot == end.Snapshot);
            var snapshot = start.Snapshot;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                int minColumn = Int32.MaxValue;
                // first pass, determine the position to place the comment
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);
                    var text = curLine.GetText();

                    int firstNonWhitespace = IndexOfNonWhitespaceCharacter(text);
                    if (firstNonWhitespace >= 0 && firstNonWhitespace < minColumn)
                    {
                        // ignore blank lines
                        minColumn = firstNonWhitespace;
                    }
                }

                // second pass, place the comment
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);
                    if (String.IsNullOrWhiteSpace(curLine.GetText()))
                    {
                        continue;
                    }

                    //Debug.Assert(curLine.Length >= minColumn);

                    edit.Insert(curLine.Start.Position + minColumn, "#");
                }

                edit.Apply();
            }
        }

        /// <summary>
        /// Removes a comment character (#) from the start of each line.  If there is a selection the character is
        /// removed from each selected line.  Otherwise the character is removed from the current line.  Uncommented
        /// lines are ignored.
        /// </summary>
        private static void UncommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            //Debug.Assert(start.Snapshot == end.Snapshot);
            var snapshot = start.Snapshot;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {

                // first pass, determine the position to place the comment
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);

                    DeleteFirstCommentChar(edit, curLine);
                }

                edit.Apply();
            }
        }

        private static int IndexOfNonWhitespaceCharacter(string text)
        {
            for (int j = 0; j < text.Length; j++)
            {
                if (!Char.IsWhiteSpace(text[j]))
                {
                    return j;
                }
            }
            return -1;
        }

        private static void DeleteFirstCommentChar(ITextEdit edit, ITextSnapshotLine curLine)
        {
            var text = curLine.GetText();
            for (int j = 0; j < text.Length; j++)
            {
                if (!Char.IsWhiteSpace(text[j]))
                {
                    if (text[j] == '#')
                    {
                        edit.Delete(curLine.Start.Position + j, 1);
                    }
                    break;
                }
            }
        }

        //public static GeneroReverseParser GetReverseParser(IWpfTextView textView, int startPosition = -1, bool multiLine = false)
        //{
        //    SnapshotPoint currentPoint = (textView.Caret.Position.BufferPosition) - 1;
        //    return GetReverseParser(currentPoint, startPosition, multiLine);
        //}

        //public static GeneroReverseParser GetReverseParser(SnapshotPoint triggerPoint, int startPosition = -1, bool multiLine = false)
        //{
        //    ITextSnapshotLine currentLine = triggerPoint.GetContainingLine();
        //    int lineNumber = currentLine.LineNumber - 1;

        //    int tempStartPos = startPosition;
        //    if (tempStartPos < 0)
        //        tempStartPos = currentLine.End.Position;
        //    //if (tempStartPos < currentLine.Start.Position &&
        //    //    lineNumber > 0)
        //    //{
        //    //    currentLine.Snapshot.
        //    //}

        //    ITrackingSpan span = currentLine.Snapshot.CreateTrackingSpan(currentLine.Start.Position, tempStartPos - currentLine.Start.Position, SpanTrackingMode.EdgeInclusive);
        //    return new GeneroReverseParser(currentLine.Snapshot, triggerPoint.Snapshot.TextBuffer, span, multiLine);
        //}

        public static string GetLineString(IWpfTextView textView, int startPosition)
        {
            SnapshotPoint currentPoint = (textView.Caret.Position.BufferPosition) - 1;
            return EditorExtensions.GetLineString(currentPoint, startPosition);
        }

        public static string GetLineString(SnapshotPoint triggerPoint, int startPosition)
        {
            ITextSnapshotLine currentLine = triggerPoint.GetContainingLine();

            int tempStartPos = startPosition;
            if (tempStartPos < 0)
                tempStartPos = currentLine.End.Position;
            ITrackingSpan span = currentLine.Snapshot.CreateTrackingSpan(currentLine.Start.Position, tempStartPos - currentLine.Start.Position, SpanTrackingMode.EdgeInclusive);
            return span.GetText(currentLine.Snapshot);
        }

        public static void GetLineAndColumnOfFile(string filename, int charPosition, out int line, out int column)
        {
            line = 0;
            column = 0;

            int currentLineStart = 0;
            if(File.Exists(filename))
            {
                using(StreamReader sr = new StreamReader(filename))
                {
                    string lineStr;
                    while(true)
                    {
                        lineStr = sr.ReadLine();
                        if(lineStr == null)
                            break;
                        line++;
                        if(currentLineStart + lineStr.Length > charPosition)
                        {
                            column = (currentLineStart + lineStr.Length) - charPosition;
                            break;
                        }
                        else
                        {
                            currentLineStart = currentLineStart + lineStr.Length;
                        }
                    }
                }
            }
        }

        public static void ChangeKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
                                      TKey fromKey, TKey toKey)
        {
            TValue value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }

        /// <summary>
        /// This is the default function for getting program filenames. It simply retreives filenames for all files
        /// with a .4gl extension in the same directory as <paramref name="moduleFilename"/>. 
        /// </summary>
        /// <param name="moduleFilename"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetProgramFilenames(string moduleFilename)
        {
            string filepath = Path.GetDirectoryName(moduleFilename);
            return Directory.GetFiles(filepath, "*.4gl").Where(x => x.EndsWith(".4gl") && !string.Equals(x, moduleFilename, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetProgram(this ITextBuffer buffer)
        {
            return Path.GetDirectoryName(buffer.GetFilePath());
        }
    }
}
