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
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using System.Threading;
using VSGenero.Analysis;
using VSGenero.EditorExtensions;
using VSGenero.Analysis.Parsing.AST_4GL;
using VSGenero.EditorExtensions.Intellisense;
using System.Diagnostics;
using Microsoft.VisualStudio.Text.Editor;
using System.Windows.Input;
using VSGenero.Analysis.Parsing;
using VSGenero.External;

namespace VSGenero.ProductivityTools
{
    [TagType(typeof(IBlockTag)), Export(typeof(IViewTaggerProvider)), ContentType(External.GeneroConstants.LanguageName4GL)]
    public class Genero4glBlockTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            Func<Genero4glBlockTagger> creator = null;
            if (!(typeof(T) == typeof(IBlockTag)))
            {
                return null;
            }
            if (creator == null)
            {
                creator = () => new Genero4glBlockTagger(textView, buffer, this);
            }
            return (new DisposableTagger(buffer.Properties.GetOrCreateSingletonProperty(typeof(Genero4glBlockTaggerProvider), creator)) as ITagger<T>);
        }
    }

    public class Genero4glBlockTagger : ITagger<IBlockTag>, IDisposable
    {
        private readonly Genero4glBlockTaggerProvider _taggerProvider;
        private readonly ITextBuffer _buffer;
        private readonly ITextView _textview;
        private Timer _timer;
        private bool _eventHooked;
        private bool _disposed;
        private object _disposedLock = new object();

        public Genero4glBlockTagger(ITextView textview, ITextBuffer buffer, Genero4glBlockTaggerProvider provider)
        {
            _textview = textview;
            _taggerProvider = provider;
            _buffer = buffer;
            _buffer.Properties[typeof(Genero4glBlockTagger)] = this;
            _timer = new Timer(TagUpdate, null, Timeout.Infinite, Timeout.Infinite);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IBlockTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            IGeneroProjectEntry classifier;
            if (_buffer.TryGetAnalysis(out classifier))
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

            return new ITagSpan<IBlockTag>[0];
        }

        private IEnumerable<ITagSpan<IBlockTag>> ProcessSuite(NormalizedSnapshotSpanCollection spans, GeneroAst ast, ModuleNode moduleNode, ITextSnapshot snapshot, bool isTopLevel)
        {
            if (moduleNode != null)
            {
                List<Genero4glCodeBlock> outlinables = new List<Genero4glCodeBlock>();
                int level = 0;
                Genero4glCodeBlock block = new Genero4glCodeBlock(this, _textview, null, BlockType.Root, null, level);
                GetOutlinableResults(this, _textview, spans, moduleNode, block, snapshot, level, ref outlinables);

                foreach (var child in outlinables)
                {
                    TagSpan<Genero4glCodeBlock> tagSpan = new TagSpan<Genero4glCodeBlock>(child.Span, child);
                    if (tagSpan != null)
                    {
                        yield return tagSpan;
                    }
                }
            }
        }

        private static bool ShouldInclude(IOutlinableResult result, NormalizedSnapshotSpanCollection spans)
        {
            for (int i = 0; i < spans.Count; i++)
            {
                if (spans[i].IntersectsWith(Span.FromBounds(result.StartIndex, result.EndIndex)))
                {
                    return true;
                }
            }
            return false;
        }

        private static void GetOutlinableResults(Genero4glBlockTagger tagger, ITextView textView, NormalizedSnapshotSpanCollection spans, AstNode node, Genero4glCodeBlock parent, ITextSnapshot snapshot, int level, ref List<Genero4glCodeBlock> outlinables)
        {
            Genero4glCodeBlock curr = null;
            foreach (var child in node.Children)
            {
                if (child.Value != null)
                {
                    var outRes = child.Value as IOutlinableResult;
                    if (outRes.CanOutline /*&& ShouldInclude(outRes, spans)*/
                        && outRes.EndIndex < textView.TextSnapshot.Length)
                    {
                        curr = new Genero4glCodeBlock(tagger, textView, parent, GetBlockType(outRes), outRes,
                            level + 1);
                        if (curr != null)
                            outlinables.Add(curr);
                    }

                    if (child.Value.Children.Count > 0)
                    {
                        if (curr == null)
                        {
                            curr = new Genero4glCodeBlock(tagger, textView, parent, BlockType.Unknown, null, level + 1);
                        }

                        GetOutlinableResults(tagger, textView, spans, child.Value, curr, snapshot, level + 1,
                            ref outlinables);
                    }
                }
            }
        }

        private static Dictionary<Type, BlockType> _statementToBlockMap = new Dictionary<Type, BlockType>()
        {
            {typeof(CaseStatement), BlockType.Conditional},
            {typeof(WhenStatement), BlockType.Conditional},
            {typeof(OtherwiseStatement), BlockType.Conditional},
            {typeof(ConstructBlock), BlockType.Other},
            {typeof(DialogBlock), BlockType.Other},
            {typeof(DisplayBlock), BlockType.Other},
            {typeof(DisplayControlBlock), BlockType.Other},
            {typeof(ForeachStatement), BlockType.Loop},
            {typeof(ForStatement), BlockType.Loop},
            {typeof(GlobalsNode), BlockType.Other},
            {typeof(IfStatement), BlockType.Conditional},
            {typeof(InputBlock), BlockType.Other},
            {typeof(InputControlBlock), BlockType.Other},
            {typeof(MenuBlock), BlockType.Other},
            {typeof(MenuOption), BlockType.Other},
            {typeof(ReportFormatSection), BlockType.Other},
            {typeof(SqlBlockNode), BlockType.Other},
            {typeof(TryCatchStatement), BlockType.Conditional},
            {typeof(WhileStatement), BlockType.Loop}
        };

        private static BlockType GetBlockType(IOutlinableResult outlinable)
        {
            BlockType blockType = BlockType.Unknown;
            var type = outlinable.GetType();
            if (!_statementToBlockMap.TryGetValue(type, out blockType))
            {
                if (outlinable is IFunctionResult)
                {
                    blockType = BlockType.Method;
                }
            }
            return blockType;
        }

        private void OnNewParseTree(object sender, EventArgs e)
        {
            lock(_disposedLock)
            {
                if (!_disposed)
                {
                    IGeneroProjectEntry classifier;
                    if (_buffer.TryGetAnalysis(out classifier))
                    {
                        if (_timer != null)
                            _timer.Change(300, Timeout.Infinite);
                    }
                }
            }
        }

        private void TagUpdate(object unused)
        {
            lock(_disposedLock)
            {
                if (!_disposed)
                {
                    if (_timer != null)
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    var snapshot = _buffer.CurrentSnapshot;
                    var tagsChanged = TagsChanged;
                    if (tagsChanged != null)
                    {
                        tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
                    }
                }
            }
        }

        public void Dispose()
        {
            lock(_disposedLock)
            {
                _disposed = true;
                IGeneroProjectEntry classifier;
                if (_buffer.TryGetAnalysis(out classifier))
                {
                    classifier.OnNewAnalysis -= OnNewParseTree;
                }
                if (_buffer.Properties.ContainsProperty(typeof(Genero4glBlockTagger)))
                    _buffer.Properties.RemoveProperty(typeof(Genero4glBlockTagger));
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }
    }
}
