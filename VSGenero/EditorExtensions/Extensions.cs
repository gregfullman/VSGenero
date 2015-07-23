using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing.AST;
using VSGenero.EditorExtensions.Intellisense;
using Microsoft.VisualStudio.VSCommon;
using VSGenero.Analysis.Parsing;
using VSGenero.Snippets;
using System.Net;

namespace VSGenero.EditorExtensions
{
    public static class Extensions
    {
        internal static void GotoSource(this LocationInfo location)
        {
            if (location.Line > 0 && location.Column > 0)
            {
                VSGeneroPackage.NavigateTo(
                    location.FilePath,
                    Guid.Empty,
                    location.Line - 1,
                    location.Column - 1);
            }
            else
            {
                VSGeneroPackage.NavigateTo(location.FilePath, Guid.Empty, location.Index);
            }
        }

        internal static bool IsOpenGrouping(this ClassificationSpan span)
        {
            return span.ClassificationType.IsOfType(Genero4glPredefinedClassificationTypeNames.Grouping) &&
                span.Span.Length == 1 &&
                (span.Span.GetText() == "{" || span.Span.GetText() == "[" || span.Span.GetText() == "(");
        }

        internal static bool IsOpenGrouping(this TokenKind tokKind)
        {
            return tokKind == TokenKind.LeftBracket || tokKind == TokenKind.LeftBrace || tokKind == TokenKind.LeftParenthesis;
        }

        internal static bool IsCloseGrouping(this ClassificationSpan span)
        {
            return span.ClassificationType.IsOfType(Genero4glPredefinedClassificationTypeNames.Grouping) &&
                span.Span.Length == 1 &&
                (span.Span.GetText() == "}" || span.Span.GetText() == "]" || span.Span.GetText() == ")");
        }

        internal static bool IsCloseGrouping(this TokenKind tokKind)
        {
            return tokKind == TokenKind.RightBracket || tokKind == TokenKind.RightBrace || tokKind == TokenKind.RightParenthesis;
        }

        internal static ExpressionAnalysis GetExpressionAnalysis(this ITextView view, IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider,
                                                                 IProgramFileProvider programFileProvider)
        {
            ITrackingSpan span = GetCaretSpan(view);
            if (span == null)
                return null;
            // TODO: get the function provider and database provider from EditFilter class.
            return span.TextBuffer.CurrentSnapshot.AnalyzeExpression(span, false, functionProvider, databaseProvider, programFileProvider);
        }

        internal static ITrackingSpan GetCaretSpan(this ITextView view)
        {
            var caretPoint = view.GetCaretPosition();
            if (caretPoint == null)
                return null;
            var snapshot = caretPoint.Value.Snapshot;
            var caretPos = caretPoint.Value.Position;

            // fob(
            //    ^
            //    +---  Caret here
            //
            // We want to lookup fob, not fob(
            //
            ITrackingSpan span;
            if (caretPos != snapshot.Length)
            {
                string curChar = snapshot.GetText(caretPos, 1);
                if (!IsIdentifierChar(curChar[0]) && caretPos > 0)
                {
                    string prevChar = snapshot.GetText(caretPos - 1, 1);
                    if (IsIdentifierChar(prevChar[0]))
                    {
                        caretPos--;
                    }
                }
                span = snapshot.CreateTrackingSpan(
                    caretPos,
                    1,
                    SpanTrackingMode.EdgeInclusive
                );
            }
            else
            {
                span = snapshot.CreateTrackingSpan(
                    caretPos,
                    0,
                    SpanTrackingMode.EdgeInclusive
                );
            }

            return span;
        }

        private static bool IsIdentifierChar(char curChar)
        {
            return Char.IsLetterOrDigit(curChar) || curChar == '_';
        }

        internal static SnapshotPoint? GetCaretPosition(this ITextView view)
        {
            return view.BufferGraph.MapDownToFirstMatch(
               new SnapshotPoint(view.TextBuffer.CurrentSnapshot, view.Caret.Position.BufferPosition),
               PointTrackingMode.Positive,
               (x) => VSGeneroConstants.IsGenero4GLContent(x) || VSGeneroConstants.IsGeneroPERContent(x),
               PositionAffinity.Successor
            );
        }

        internal static bool TryGetAnalysis(this ITextBuffer buffer, out IProjectEntry analysis)
        {
            return buffer.Properties.TryGetProperty<IProjectEntry>(typeof(IProjectEntry), out analysis);
        }

        internal static bool TryGetPythonAnalysis(this ITextBuffer buffer, out IGeneroProjectEntry analysis)
        {
            IProjectEntry entry;
            if (buffer.TryGetAnalysis(out entry) && (analysis = entry as IGeneroProjectEntry) != null)
            {
                return true;
            }
            analysis = null;
            return false;
        }


        internal static GeneroProjectAnalyzer GetAnalyzer(this ITextBuffer buffer)
        {
            GeneroProjectAnalyzer analyzer;

            // exists for tests where we don't run in VS and for the existing changes preview
            if (buffer.Properties.TryGetProperty<GeneroProjectAnalyzer>(typeof(GeneroProjectAnalyzer), out analyzer))
            {
                return analyzer;
            }

            return VSGeneroPackage.Instance.DefaultAnalyzer;
        }

        public static IProjectEntry GetAnalysis(this ITextBuffer buffer)
        {
            IProjectEntry res;
            buffer.TryGetAnalysis(out res);
            return res;
        }

        internal static string LimitLines(
            this string str,
            int maxLines = 30,
            int charsPerLine = 200,
            bool ellipsisAtEnd = true,
            bool stopAtFirstBlankLine = false
        )
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            int lineCount = 0;
            var prettyPrinted = new StringBuilder();
            bool wasEmpty = true;

            using (var reader = new StringReader(str))
            {
                for (var line = reader.ReadLine(); line != null && lineCount < maxLines; line = reader.ReadLine())
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (wasEmpty)
                        {
                            continue;
                        }
                        wasEmpty = true;
                        if (stopAtFirstBlankLine)
                        {
                            lineCount = maxLines;
                            break;
                        }
                        lineCount += 1;
                        prettyPrinted.AppendLine();
                    }
                    else
                    {
                        wasEmpty = false;
                        lineCount += (line.Length / charsPerLine) + 1;
                        prettyPrinted.AppendLine(line);
                    }
                }
            }
            if (ellipsisAtEnd && lineCount >= maxLines)
            {
                prettyPrinted.AppendLine("...");
            }
            return prettyPrinted.ToString().Trim();
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
        internal static ITrackingSpan GetApplicableSpan(this ITextSnapshot snapshot, ITrackingPoint point)
        {
            return snapshot.GetApplicableSpan(point.GetPosition(snapshot));
        }

        /// <summary>
        /// Returns the applicable span at the provided position.
        /// </summary>
        /// <returns>A tracking span, or null if there is no token at the
        /// provided position.</returns>
        internal static ITrackingSpan GetApplicableSpan(this ITextSnapshot snapshot, int position)
        {
            var classifier = snapshot.TextBuffer.GetGeneroClassifier();
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
        /// Gets a CompletionAnalysis providing a list of possible members the user can dot through.
        /// </summary>
        public static CompletionAnalysis GetCompletions(this ITextSnapshot snapshot, ITrackingSpan span, ITrackingPoint point, CompletionOptions options, 
                                                        IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider,
                                                        IProgramFileProvider programFileProvider)
        {
            return GeneroProjectAnalyzer.GetCompletions(snapshot, span, point, options, functionProvider, databaseProvider, programFileProvider);
        }

        /// <summary>
        /// Gets a list of signatuers available for the expression at the provided location in the snapshot.
        /// </summary>
        public static SignatureAnalysis GetSignatures(this ITextSnapshot snapshot, ITrackingSpan span, IFunctionInformationProvider functionProvider)
        {
            return GeneroProjectAnalyzer.GetSignatures(snapshot, span, functionProvider);
        }

        /// <summary>
        /// Gets a ExpressionAnalysis for the expression at the provided span.  If the span is in
        /// part of an identifier then the expression is extended to complete the identifier.
        /// </summary>
        public static ExpressionAnalysis AnalyzeExpression(this ITextSnapshot snapshot, ITrackingSpan span, bool forCompletion, 
                                                           IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider,
                                                           IProgramFileProvider programFileProvider)
        {
            return GeneroProjectAnalyzer.AnalyzeExpression(snapshot, span, functionProvider, databaseProvider, programFileProvider, forCompletion);
        }

        internal static GeneroProjectAnalyzer GetAnalyzer(this ITextView textView)
        {
            return textView.TextBuffer.GetAnalyzer();
        }

        internal static ITrackingSpan CreateTrackingSpan(this IIntellisenseSession session, ITextBuffer buffer)
        {
            var triggerPoint = session.GetTriggerPoint(buffer);
            var position = triggerPoint.GetPosition(buffer.CurrentSnapshot);
            if (position == buffer.CurrentSnapshot.Length)
            {
                return session.GetApplicableSpan(buffer);
            }

            return buffer.CurrentSnapshot.CreateTrackingSpan(position, 1, SpanTrackingMode.EdgeInclusive);
        }

        public static StandardGlyphGroup ToGlyphGroup(this GeneroMemberType objectType)
        {
            StandardGlyphGroup group;
            switch (objectType)
            {
                case GeneroMemberType.Namespace: group = StandardGlyphGroup.GlyphGroupNamespace; break;
                case GeneroMemberType.Class: group = StandardGlyphGroup.GlyphGroupClass; break;
                case GeneroMemberType.Module: group = StandardGlyphGroup.GlyphGroupModule; break;
                case GeneroMemberType.Instance: group = StandardGlyphGroup.GlyphGroupVariable; break;
                case GeneroMemberType.Constant: group = StandardGlyphGroup.GlyphGroupConstant; break;
                case GeneroMemberType.Keyword: group = StandardGlyphGroup.GlyphKeyword; break;
                case GeneroMemberType.Variable: group = StandardGlyphGroup.GlyphGroupVariable; break;
                case GeneroMemberType.DbTable: group = StandardGlyphGroup.GlyphLibrary; break;
                case GeneroMemberType.DbView: group = StandardGlyphGroup.GlyphGroupMap; break;
                case GeneroMemberType.DbColumn: group = StandardGlyphGroup.GlyphGroupMapItem; break;
                case GeneroMemberType.Function:
                case GeneroMemberType.Method:
                default:
                    group = StandardGlyphGroup.GlyphGroupMethod;
                    break;
            }
            return group;
        }

        internal static DynamicSnippet GetSnippet(this IFunctionResult functionResult, string optionalNameOverride = null)
        {
            string name = optionalNameOverride ?? functionResult.Name;
            string desc = name;
            //if (publicFunction.XmlInformation != null && !string.IsNullOrWhiteSpace(publicFunction.XmlInformation.Summary))
            //    desc = WebUtility.HtmlDecode(publicFunction.XmlInformation.Summary);


            // Generate the code
            List<DynamicSnippetReplacement> replacements = new List<DynamicSnippetReplacement>();
            StringBuilder codeBuilder = new StringBuilder();

            codeBuilder.AppendFormat("{0}(", name);
            for (int i = 0; i < functionResult.Parameters.Length; i++)
            {
                var param = functionResult.Parameters[i];
                string pName = string.Format("param{0}", i);
                replacements.Add(new DynamicSnippetReplacement(pName, string.Format("{0}: {1}", param.Name, param.Type), param.Name));

                // TODO: maybe format things a bit, breaking up lines and such

                codeBuilder.AppendFormat("${0}$", pName);
                if (i + 1 < functionResult.Parameters.Length)
                {
                    codeBuilder.Append(", ");
                }
            }
            codeBuilder.Append(")");

            //if (publicFunction.SourceDocInformation.Returns.Count > 0)
            //{
            //    codeBuilder.Append("\n");
            //    codeBuilder.Append("returning ");
            //    for (int i = 0; i < publicFunction.SourceDocInformation.Returns.Count; i++)
            //    {
            //        var ret = publicFunction.SourceDocInformation.Returns.ElementAt(i);
            //        string rName = string.Format("ret{0}", i);
            //        replacements.Add(new DynamicSnippetReplacement(rName, "", ret.Name));   // TODO: get tooltip from xml if available

            //        // TODO: maybe format things a bit, breaking up lines and such

            //        codeBuilder.AppendFormat("${0}$", rName);
            //        if (i + 1 < publicFunction.SourceDocInformation.Returns.Count)
            //        {
            //            codeBuilder.Append(", ");
            //        }
            //    }
            //}

            codeBuilder.Append("$end$");

            DynamicSnippet snippet = new DynamicSnippet(name, name, desc, "VSGenero", codeBuilder.ToString());
            snippet.Replacements.AddRange(replacements);
            return snippet;
        }
    }
}
