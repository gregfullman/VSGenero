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

namespace VSGenero.ProductivityTools
{
    [TagType(typeof(IBlockTag)), Export(typeof(ITaggerProvider)), ContentType(VSGeneroConstants.LanguageName4GL)]
    public class Genero4glBlockTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(Microsoft.VisualStudio.Text.ITextBuffer buffer) where T : ITag
        {
            Func<Genero4glBlockTagger> creator = null;
            if (!(typeof(T) == typeof(IBlockTag)))
            {
                return null;
            }
            if (creator == null)
            {
                creator = () => new Genero4glBlockTagger(buffer, this);
            }
            return (new DisposableTagger(buffer.Properties.GetOrCreateSingletonProperty(typeof(Genero4glBlockTaggerProvider), creator)) as ITagger<T>);
        }
    }

    internal class Genero4glBlockTagger : ITagger<IBlockTag>
    {
        private readonly Genero4glBlockTaggerProvider _taggerProvider;
        private readonly ITextBuffer _buffer;
        private readonly Timer _timer;
        private bool _eventHooked;

        public Genero4glBlockTagger(ITextBuffer buffer, Genero4glBlockTaggerProvider provider)
        {
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
                Genero4glAst ast;
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

        private IEnumerable<ITagSpan<IBlockTag>> ProcessSuite(NormalizedSnapshotSpanCollection spans, Genero4glAst ast, ModuleNode moduleNode, ITextSnapshot snapshot, bool isTopLevel)
        {
            if (moduleNode != null)
            {
                List<CodeBlock> outlinables = new List<CodeBlock>();
                int level = 0;
                CodeBlock block = new CodeBlock(null, BlockType.Root, null, new SnapshotSpan(snapshot, 0, snapshot.Length), 0, level);
                GetOutlinableResults(spans, moduleNode, block, snapshot, level, ref outlinables);

                foreach (var child in outlinables)
                {
                    TagSpan<CodeBlock> tagSpan = new TagSpan<CodeBlock>(child.Span, child);
                    if (tagSpan != null)
                    {
                        yield return tagSpan;
                    }
                }
            }
        }

        private static bool ShouldInclude(IOutlinableResult result, NormalizedSnapshotSpanCollection spans)
        {
            //if (spans.Count == 1 && spans[0].Length == spans[0].Snapshot.Length)
            //{
            //    // we're processing the entire snapshot
            //    return spans[0];
            //}

            for (int i = 0; i < spans.Count; i++)
            {
                if (spans[i].IntersectsWith(Span.FromBounds(result.StartIndex, result.EndIndex)))
                {
                    return true;
                }
            }
            return false;
        }

        private static void GetOutlinableResults(NormalizedSnapshotSpanCollection spans, AstNode4gl node, CodeBlock parent, ITextSnapshot snapshot, int level, ref List<CodeBlock> outlinables)
        {
            CodeBlock curr = null;
            foreach (var child in node.Children)
            {
                if (child.Value is IOutlinableResult)
                {
                    var outRes = child.Value as IOutlinableResult;
                    if (ShouldInclude(outRes, spans))
                    {
                        curr = GetCodeBlock(outRes, snapshot, parent, level + 1);
                        if (curr != null)
                            outlinables.Add(curr);
                    }
                }

                if (child.Value.Children.Count > 0)
                {
                    if (curr == null)
                    {
                        curr = new CodeBlock(parent, BlockType.Unknown, null, new SnapshotSpan(snapshot, 0, snapshot.Length), 0, level + 1);
                    }

                    GetOutlinableResults(spans, child.Value, curr, snapshot, level + 1, ref outlinables);
                }
            }
        }

        private static CodeBlock GetCodeBlock(IOutlinableResult outlinable, ITextSnapshot snapshot, CodeBlock parent, int level)
        {
            if (outlinable.DecoratorEnd <= outlinable.DecoratorStart)
            {
                return null;
            }
            SnapshotSpan statementSpan = new SnapshotSpan(snapshot, new Span(outlinable.DecoratorStart, (outlinable.DecoratorEnd - outlinable.DecoratorStart)));
            string statement = statementSpan.GetText();
            SnapshotSpan snapSpan = new SnapshotSpan(snapshot, new Span(outlinable.StartIndex, (outlinable.EndIndex - outlinable.StartIndex)));
            return new CodeBlock(parent, GetBlockType(outlinable), statement, snapSpan, outlinable.StartIndex, level);
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
            IGeneroProjectEntry classifier;
            if (_buffer.TryGetAnalysis(out classifier))
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
    }
}
