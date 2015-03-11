using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing.AST;
using VSGenero.EditorExtensions.Intellisense;

namespace VSGenero.EditorExtensions
{
    [Export(typeof(ITaggerProvider)), ContentType(VSGeneroConstants.ContentType4GL)]
    [TagType(typeof(IOutliningRegionTag))]
    class OutliningTaggerProvider : ITaggerProvider
    {
        #region ITaggerProvider Members

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return (ITagger<T>)(buffer.GetOutliningTagger() ?? new OutliningTagger(buffer));
        }

        #endregion

        internal class OutliningTagger : ITagger<IOutliningRegionTag>
        {
            private readonly ITextBuffer _buffer;
            private readonly Timer _timer;
            private bool _enabled, _eventHooked;

            public OutliningTagger(ITextBuffer buffer)
            {
                _buffer = buffer;
                _buffer.Properties[typeof(OutliningTagger)] = this;
                if (VSGeneroPackage.Instance != null)
                {
                    _enabled = true;//VSGeneroPackage.Instance.IntellisenseOptions4GLPage.EnterOutliningModeOnOpen;
                }
                _timer = new Timer(TagUpdate, null, Timeout.Infinite, Timeout.Infinite);
            }

            public bool Enabled
            {
                get
                {
                    return _enabled;
                }
            }

            public void Enable()
            {
                _enabled = true;
                var snapshot = _buffer.CurrentSnapshot;
                var tagsChanged = TagsChanged;
                if (tagsChanged != null)
                {
                    tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
                }
            }

            public void Disable()
            {
                _enabled = false;
                var snapshot = _buffer.CurrentSnapshot;
                var tagsChanged = TagsChanged;
                if (tagsChanged != null)
                {
                    tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
                }
            }

            #region ITagger<IOutliningRegionTag> Members

            public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
            {
                IGeneroProjectEntry classifier;
                if (_enabled && _buffer.TryGetPythonAnalysis(out classifier))
                {
                    if (!_eventHooked)
                    {
                        classifier.OnNewParseTree += OnNewParseTree;
                        _eventHooked = true;
                    }
                    GeneroAst ast;
                    IAnalysisCookie cookie;
                    classifier.GetTreeAndCookie(out ast, out cookie);
                    SnapshotCookie snapCookie = cookie as SnapshotCookie;

                    if (ast != null &&
                        snapCookie != null &&
                        snapCookie.Snapshot.TextBuffer == spans[0].Snapshot.TextBuffer)
                    {   // buffer could have changed if file was closed and re-opened
                        return ProcessSuite(spans, ast, ast.Body as ModuleNode, snapCookie.Snapshot, true);
                    }
                }

                return new ITagSpan<IOutliningRegionTag>[0];
            }

            private void OnNewParseTree(object sender, EventArgs e)
            {
                IGeneroProjectEntry classifier;
                if (_buffer.TryGetPythonAnalysis(out classifier))
                {
                    _timer.Change(300, Timeout.Infinite);
                }
            }

            private void TagUpdate(object unused)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                var snapshot = _buffer.CurrentSnapshot;
                var tagsChanged = TagsChanged;
                if (tagsChanged != null)
                {
                    tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
                }
            }

            private IEnumerable<ITagSpan<IOutliningRegionTag>> ProcessSuite(NormalizedSnapshotSpanCollection spans, GeneroAst ast, ModuleNode moduleNode, ITextSnapshot snapshot, bool isTopLevel)
            {
                if (moduleNode != null)
                {
                    // TODO: Binary search the statements?  The perf of this seems fine for the time being
                    // w/ a 5000+ line file though.                    
                    foreach (var child in moduleNode.Children)
                    {
                        SnapshotSpan? span = ShouldInclude(child.Value, spans);
                        if (span == null)
                        {
                            continue;
                        }

                        FunctionBlockNode funcDef = child.Value as FunctionBlockNode;
                        if (funcDef != null)
                        {
                            TagSpan tagSpan = GetFunctionSpan(ast, snapshot, funcDef);

                            if (tagSpan != null)
                            {
                                yield return tagSpan;
                            }

                            // recurse into the class definition and outline it's members
                            //foreach (var v in ProcessSuite(spans, ast, funcDef.Body as SuiteStatement, snapshot, false))
                            //{
                            //    yield return v;
                            //}
                        }

                        MainBlockNode mainDef = child.Value as MainBlockNode;
                        if(mainDef != null)
                        {
                            TagSpan tagSpan = GetMainSpan(ast, snapshot, mainDef);

                            if (tagSpan != null)
                            {
                                yield return tagSpan;
                            }
                        }

                        //ClassDefinition classDef = statement as ClassDefinition;
                        //if (classDef != null)
                        //{
                        //    TagSpan tagSpan = GetClassSpan(ast, snapshot, classDef);

                        //    if (tagSpan != null)
                        //    {
                        //        yield return tagSpan;
                        //    }

                        //    // recurse into the class definition and outline it's members
                        //    foreach (var v in ProcessSuite(spans, ast, classDef.Body as SuiteStatement, snapshot, false))
                        //    {
                        //        yield return v;
                        //    }
                        //}

                        //if (isTopLevel)
                        //{
                        //    IfStatement ifStmt = statement as IfStatement;
                        //    if (ifStmt != null)
                        //    {
                        //        TagSpan tagSpan = GetIfSpan(ast, snapshot, ifStmt);

                        //        if (tagSpan != null)
                        //        {
                        //            yield return tagSpan;
                        //        }
                        //    }

                        //    WhileStatement whileStatment = statement as WhileStatement;
                        //    if (whileStatment != null)
                        //    {
                        //        TagSpan tagSpan = GetWhileSpan(ast, snapshot, whileStatment);

                        //        if (tagSpan != null)
                        //        {
                        //            yield return tagSpan;
                        //        }
                        //    }

                        //    ForStatement forStatement = statement as ForStatement;
                        //    if (forStatement != null)
                        //    {
                        //        TagSpan tagSpan = GetForSpan(ast, snapshot, forStatement);

                        //        if (tagSpan != null)
                        //        {
                        //            yield return tagSpan;
                        //        }
                        //    }
                        //}
                    }
                }
            }

            //private static TagSpan GetForSpan(GeneroAst ast, ITextSnapshot snapshot, ForStatement forStmt)
            //{
            //    if (forStmt.List != null)
            //    {
            //        return GetTagSpan(snapshot, forStmt.StartIndex, forStmt.EndIndex, forStmt.List.EndIndex);
            //    }
            //    return null;
            //}

            //private static TagSpan GetWhileSpan(GeneroAst ast, ITextSnapshot snapshot, WhileStatement whileStmt)
            //{
            //    return GetTagSpan(
            //        snapshot,
            //        whileStmt.StartIndex,
            //        whileStmt.EndIndex,
            //        whileStmt.Test.EndIndex
            //    );
            //}

            //private static TagSpan GetIfSpan(GeneroAst ast, ITextSnapshot snapshot, IfStatement ifStmt)
            //{
            //    return GetTagSpan(snapshot, ifStmt.StartIndex, ifStmt.EndIndex, ifStmt.Tests[0].HeaderIndex);
            //}

            private static TagSpan GetFunctionSpan(GeneroAst ast, ITextSnapshot snapshot, FunctionBlockNode funcDef)
            {
                return GetTagSpan(snapshot, funcDef.StartIndex, funcDef.EndIndex, funcDef.DecoratorEnd);
            }

            private static TagSpan GetMainSpan(GeneroAst ast, ITextSnapshot snapshot, MainBlockNode funcDef)
            {
                return GetTagSpan(snapshot, funcDef.StartIndex, funcDef.EndIndex, funcDef.DecoratorEnd);
            }

            //private static TagSpan GetClassSpan(GeneroAst ast, ITextSnapshot snapshot, ClassDefinition classDef)
            //{
            //    return GetTagSpan(snapshot, classDef.StartIndex, classDef.EndIndex, classDef.HeaderIndex, classDef.Decorators);
            //}

            private static TagSpan GetTagSpan(ITextSnapshot snapshot, int start, int end, int decoratorEnd)
            {
                TagSpan tagSpan = null;
                try
                {
                    int length = end - start;
                    if(length > 0)
                    {
                        var headerLength = decoratorEnd - start;
                        Span headerSpan = new Span(start, headerLength);

                        var span = GetFinalSpan(snapshot, start, length);

                        tagSpan = new TagSpan(
                                new SnapshotSpan(snapshot, span),
                                new OutliningTag(snapshot, headerSpan, span, true)
                            );
                    }
                }
                catch (ArgumentException)
                {
                    // sometimes Python's parser gives us bad spans, ignore those and fix the parser
                    Debug.Assert(false, "bad argument when making span/tag");
                }

                return tagSpan;
            }

            private static Span GetFinalSpan(ITextSnapshot snapshot, int start, int length)
            {
                Debug.Assert(start + length <= snapshot.Length);
                int cnt = 0;
                var text = snapshot.GetText(start, length);

                // remove up to 2 \r\n's if we just end with these, this will leave a space between the methods
                while (length > 0 && ((Char.IsWhiteSpace(text[length - 1])) || ((text[length - 1] == '\r' || text[length - 1] == '\n') && cnt++ < 4)))
                {
                    length--;
                }
                return new Span(start, length);
            }

            private SnapshotSpan? ShouldInclude(AstNode statement, NormalizedSnapshotSpanCollection spans)
            {
                if (spans.Count == 1 && spans[0].Length == spans[0].Snapshot.Length)
                {
                    // we're processing the entire snapshot
                    return spans[0];
                }

                for (int i = 0; i < spans.Count; i++)
                {
                    if (spans[i].IntersectsWith(Span.FromBounds(statement.StartIndex, statement.EndIndex)))
                    {
                        return spans[i];
                    }
                }
                return null;
            }

            public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

            #endregion
        }

        class TagSpan : ITagSpan<IOutliningRegionTag>
        {
            private readonly SnapshotSpan _span;
            private readonly OutliningTag _tag;

            public TagSpan(SnapshotSpan span, OutliningTag tag)
            {
                _span = span;
                _tag = tag;
            }

            #region ITagSpan<IOutliningRegionTag> Members

            public SnapshotSpan Span
            {
                get { return _span; }
            }

            public IOutliningRegionTag Tag
            {
                get { return _tag; }
            }

            #endregion
        }

        class OutliningTag : IOutliningRegionTag
        {
            private readonly ITextSnapshot _snapshot;
            private readonly Span _headerSpan, _bodySpan;
            private readonly bool _isImplementation;

            public OutliningTag(ITextSnapshot iTextSnapshot, Span headerSpan, Span bodySpan, bool isImplementation)
            {
                _snapshot = iTextSnapshot;
                _headerSpan = headerSpan;
                _bodySpan = bodySpan;
                _isImplementation = isImplementation;
            }

            #region IOutliningRegionTag Members

            public object CollapsedForm
            {
                get { return _snapshot.GetText(_headerSpan) + "..."; }
            }

            public object CollapsedHintForm
            {
                get
                {
                    string collapsedHint = _snapshot.GetText(_bodySpan);

                    string[] lines = collapsedHint.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    // remove any leading white space for the preview
                    if (lines.Length > 0)
                    {
                        int smallestWhiteSpace = Int32.MaxValue;
                        for (int i = 0; i < lines.Length; i++)
                        {
                            string curLine = lines[i];

                            for (int j = 0; j < curLine.Length; j++)
                            {
                                if (curLine[j] != ' ')
                                {
                                    smallestWhiteSpace = Math.Min(j, smallestWhiteSpace);
                                }
                            }
                        }

                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].Length >= smallestWhiteSpace)
                            {
                                lines[i] = lines[i].Substring(smallestWhiteSpace);
                            }
                        }

                        return String.Join("\r\n", lines);
                    }
                    return collapsedHint;
                }
            }

            public bool IsDefaultCollapsed
            {
                get { return false; }
            }

            public bool IsImplementation
            {
                get { return _isImplementation; }
            }

            #endregion
        }
    }

    static class OutliningTaggerProviderExtensions
    {
        public static OutliningTaggerProvider.OutliningTagger GetOutliningTagger(this ITextView self)
        {
            return self.TextBuffer.GetOutliningTagger();
        }

        public static OutliningTaggerProvider.OutliningTagger GetOutliningTagger(this ITextBuffer self)
        {
            OutliningTaggerProvider.OutliningTagger res;
            if (self.Properties.TryGetProperty<OutliningTaggerProvider.OutliningTagger>(typeof(OutliningTaggerProvider.OutliningTagger), out res))
            {
                return res;
            }
            return null;
        }
    }
}
