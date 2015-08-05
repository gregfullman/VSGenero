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

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.BraceCompletion
{
   internal static class Extensions
    {
        // Methods
        public static bool ContainsOnlyWhitespace(this IBraceCompletionSession session)
        {
            SnapshotSpan sessionSpan = session.GetSessionSpan();
            ITextSnapshot snapshot = sessionSpan.Snapshot;
            int position = sessionSpan.Start.Position;
            position = (snapshot[position] == session.OpeningBrace) ? (position + 1) : position;
            int num2 = sessionSpan.End.Position - 1;
            num2 = (snapshot[num2] == session.ClosingBrace) ? (num2 - 1) : num2;
            if (!position.PositionInSnapshot(snapshot) || !num2.PositionInSnapshot(snapshot))
            {
                return false;
            }
            for (int i = position; i <= num2; i++)
            {
                if (!char.IsWhiteSpace(snapshot[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static SnapshotPoint? GetCaretPosition(this IBraceCompletionSession session)
        {
            return session.TextView.Caret.Position.Point.GetPoint(session.SubjectBuffer, PositionAffinity.Successor);
        }

        //public static ParseTreeNode GetEnclosingNode(this ParseTreeNode node, NodeGroup group)
        //{
        //    ParseTreeNode node2;
        //    ParseTreeNode node3;
        //    if (!node.GetEnclosingNode(group, out node2, out node3))
        //    {
        //        return null;
        //    }
        //    return node2;
        //}

        //public static bool GetEnclosingNode(this ParseTreeNode node, NodeGroup group, out ParseTreeNode ppEnclosingNode, out ParseTreeNode ppChild)
        //{
        //    ParseTreeNode node2 = null;
        //    while ((node != null) && !node.InGroup(group))
        //    {
        //        node2 = node;
        //        node = node.Parent;
        //    }
        //    if (node == null)
        //    {
        //        ppEnclosingNode = null;
        //        ppChild = null;
        //        return false;
        //    }
        //    ppEnclosingNode = node;
        //    ppChild = node2;
        //    return true;
        //}

        public static SnapshotSpan GetSessionSpan(this IBraceCompletionSession session)
        {
            ITextSnapshot currentSnapshot = session.SubjectBuffer.CurrentSnapshot;
            SnapshotPoint start = session.OpeningPoint.GetPoint(currentSnapshot);
            return new SnapshotSpan(start, session.ClosingPoint.GetPoint(currentSnapshot));
        }

        //public static Token GetTokenAtPosition(this ParseTree tree, Position position)
        //{
        //    int num = tree.FindNearestPosition(position);
        //    if ((num >= 0) && (num < tree.LexData.Tokens.Count))
        //    {
        //        return tree.LexData.Tokens[num];
        //    }
        //    return null;
        //}

        public static int GetValueInValidRange(this int value, int smallest, int largest)
        {
            return Math.Max(smallest, Math.Min(value, largest));
        }

        //public static bool IsPreprocessorLine(this ParseTree tree, int lineNumber)
        //{
        //    PPTokenId id;
        //    int num;
        //    int num2;
        //    return tree.TryGetPreprocessorDirective(lineNumber, out id, out num, out num2);
        //}

        //public static bool IsRegionEndLine(this ParseTree tree, int lineNumber)
        //{
        //    LexData lexData = tree.LexData;
        //    if (lexData.RegionCount > 0)
        //    {
        //        foreach (int num in lexData.RegionEnds)
        //        {
        //            if (num == lineNumber)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        //public static bool IsRegionStartLine(this ParseTree tree, int lineNumber)
        //{
        //    LexData lexData = tree.LexData;
        //    if (lexData.RegionCount > 0)
        //    {
        //        foreach (int num in lexData.RegionStarts)
        //        {
        //            if (num == lineNumber)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        //public static bool IsTransitionLine(this ParseTree tree, int lineNumber)
        //{
        //    LexData lexData = tree.LexData;
        //    if (lexData.TransitionLineCount > 0)
        //    {
        //        foreach (int num in lexData.TransitionLines)
        //        {
        //            if (num == lineNumber)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        public static void MoveCaretTo(this IBraceCompletionSession session, SnapshotPoint point, int virtualSpaces = 0)
        {
            ITextView textView = session.TextView;
            SnapshotPoint? nullable = textView.BufferGraph.MapUpToBuffer(point, PointTrackingMode.Negative, PositionAffinity.Successor, textView.TextBuffer);
            if (nullable.HasValue)
            {
                if (virtualSpaces <= 0)
                {
                    textView.Caret.MoveTo(nullable.Value);
                }
                else
                {
                    VirtualSnapshotPoint bufferPosition = new VirtualSnapshotPoint(nullable.Value, virtualSpaces);
                    textView.Caret.MoveTo(bufferPosition);
                }
                textView.Caret.EnsureVisible();
            }
        }

        public static bool PositionInSnapshot(this int position, ITextSnapshot snapshot)
        {
            return (position.GetValueInValidRange(0, Math.Max(0, snapshot.Length - 1)) == position);
        }

        //public static Position ToCSharpPosition(this SnapshotPoint corePoint, ITextSnapshot snapshot = null)
        //{
        //    SnapshotPoint point = corePoint.TranslateTo(snapshot ?? corePoint.Snapshot, PointTrackingMode.Positive);
        //    ITextSnapshotLine containingLine = point.GetContainingLine();
        //    return new Position(containingLine.LineNumber, Math.Max(point.Position - containingLine.Start.Position, 0));
        //}

        //public static SnapshotPoint ToSnapshotPoint(this Position position, ITextSnapshot snapshot)
        //{
        //    int lineNumber = position.Line.GetValueInValidRange(0, snapshot.LineCount - 1);
        //    ITextSnapshotLine lineFromLineNumber = snapshot.GetLineFromLineNumber(lineNumber);
        //    int num2 = position.Character.GetValueInValidRange(0, lineFromLineNumber.LengthIncludingLineBreak - 1);
        //    return new SnapshotPoint(snapshot, ((int) lineFromLineNumber.Start) + num2);
        //}

        internal static IVsTextBuffer ToIVsTextBuffer(this ITextBuffer textBuffer)
        {
            IVsTextBuffer buffer;
            if (!textBuffer.Properties.TryGetProperty<IVsTextBuffer>(typeof(IVsTextBuffer), out buffer))
            {
                return null;
            }
            return buffer;
        }
    }

 


 

 

}
