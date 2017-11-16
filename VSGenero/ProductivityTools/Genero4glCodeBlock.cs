using Microsoft.PowerToolsEx.BlockTagger;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using VSGenero.Analysis;

namespace VSGenero.ProductivityTools
{
    public class Genero4glCodeBlock : IBlockTag, ITag
    {
        // Fields
        private readonly Genero4glBlockTagger _tagger;
        private readonly ITextView _textView;
        private readonly IList<Genero4glCodeBlock> children = new List<Genero4glCodeBlock>();
        private readonly int level;
        private readonly Genero4glCodeBlock parent;
        private readonly BlockType type;
        private readonly IOutlinableResult _outlinable;

        // Methods
        public Genero4glCodeBlock(Genero4glBlockTagger tagger, ITextView textView, Genero4glCodeBlock parent, BlockType type, IOutlinableResult outlinable, int level)
        {
            _tagger = tagger;
            _textView = textView;
            this.parent = parent;
            if (parent != null)
            {
                parent.children.Add(this);
            }
            this.type = type;
            this.level = level;
            _outlinable = outlinable;
        }

        private static int ContainsWord(string text, string p, int index)
        {
            index = text.IndexOf(p, index, StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                return -1;
            }
            if (((index == 0) || !char.IsLetterOrDigit(text[index - 1])) && (((index + p.Length) == text.Length) || !char.IsLetterOrDigit(text[index + p.Length])))
            {
                return index;
            }
            return ContainsWord(text, p, index + 1);
        }

        public FrameworkElement Context(BlockColoring coloring, TextRunProperties properties)
        {
            Genero4glCodeBlock item = this;
            Stack<Genero4glCodeBlock> stack = new Stack<Genero4glCodeBlock>();
            do
            {
                if (item.type == BlockType.Root)
                {
                    break;
                }
                if (item.type != BlockType.Unknown)
                {
                    stack.Push(item);
                }
                item = item.parent;
            }
            while (item.type != BlockType.Namespace);
            int repeatCount = 0;
            StringBuilder builder = new StringBuilder();
            while (stack.Count != 0)
            {
                item = stack.Pop();
                builder.Append(item.Statement(repeatCount));

                repeatCount += 2;
                if (stack.Count != 0)
                {
                    builder.Append('\r');
                    builder.Append(' ', repeatCount);
                }
            }
            return new TextBlob(FormatStatements(builder.ToString(), coloring, properties));
        }

        private static FormattedText FormatStatements(string tipText, BlockColoring coloring, TextRunProperties properties)
        {
            FormattedText formattedText = new FormattedText(tipText, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, properties.Typeface, properties.FontRenderingEmSize, properties.ForegroundBrush);
            if (coloring != null)
            {
                string[] keywords = new string[] { "for", "while", "foreach" };
                string[] strArray2 = new string[] { "if", "then", "else", "case", "when", "otherwise" };
                string[] strArray3 = new string[] { "function", "main", "report", "dialog" };
                SetColors(coloring.GetToolTipBrush(BlockType.Loop), keywords, tipText, formattedText);
                SetColors(coloring.GetToolTipBrush(BlockType.Conditional), strArray2, tipText, formattedText);
                SetColors(coloring.GetToolTipBrush(BlockType.Method), strArray3, tipText, formattedText);
            }
            return formattedText;
        }

        private static void SetColors(Brush brush, string[] keywords, string tipText, FormattedText formattedText)
        {
            foreach (string str in keywords)
            {
                int startIndex = -1;
                while (true)
                {
                    startIndex = ContainsWord(tipText, str, startIndex + 1);
                    if (startIndex == -1)
                    {
                        break;
                    }
                    formattedText.SetForegroundBrush(brush, startIndex, str.Length);
                }
            }
        }

        // Properties
        public IList<Genero4glCodeBlock> Children
        {
            get
            {
                return this.children;
            }
        }

        public int Level
        {
            get
            {
                return this.level;
            }
        }

        IBlockTag IBlockTag.Parent
        {
            get
            {
                return this.parent;
            }
        }

        public Genero4glCodeBlock Parent
        {
            get
            {
                return this.parent;
            }
        }

        private SnapshotSpan _span;
        public SnapshotSpan Span
        {
            get
            {
                try
                {
                    if (_outlinable != null && _span == default(SnapshotSpan))
                    {
                        _span = new SnapshotSpan(_textView.TextSnapshot, _outlinable.StartIndex, (_outlinable.EndIndex - _outlinable.StartIndex));
                    }
                }
                catch(Exception e)
                {
                }
                return _span;
            }
        }

        public string Statement(int repeatCount)
        {
            if (_outlinable == null)
                return "";

            StringBuilder sb = new StringBuilder();

            // Get the normal text
            if (_outlinable.DecoratorEnd >= _outlinable.DecoratorStart)
            {
                var decSpan = new SnapshotSpan(_textView.TextSnapshot, _outlinable.DecoratorStart, (_outlinable.DecoratorEnd - _outlinable.DecoratorStart));
                sb.Append(decSpan.GetText());

                if (_outlinable.AdditionalDecoratorRanges.Count > 0)
                {
                    // determine the correct statement depending on where the mouse is
                    Genero4glMouseProcessor mouseProcessor;
                    if (_textView.Properties.TryGetProperty(typeof(Genero4glMouseProcessor), out mouseProcessor))
                    {
                        int mousePos = mouseProcessor.MousePosition;

                        List<int> keys = _outlinable.AdditionalDecoratorRanges.Select(x => x.Key).ToList();
                        int searchIndex = keys.BinarySearch(mousePos);
                        if (searchIndex < 0)
                        {
                            searchIndex = ~searchIndex;
                            if (searchIndex > 0)
                                searchIndex--;
                        }

                        while (searchIndex >= 0 && searchIndex < keys.Count)
                        {
                            int startInd = keys[searchIndex];    // This is our decorator start index
                            if (mousePos <= startInd)
                                break;

                            int endInd = _outlinable.AdditionalDecoratorRanges[startInd];

                            decSpan = new SnapshotSpan(_textView.TextSnapshot, new Span(startInd, (endInd - startInd)));
                            sb.Append("\r");
                            sb.Append(' ', repeatCount);
                            sb.Append(decSpan.GetText());

                            searchIndex++;
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public SnapshotPoint StatementStart
        {
            get
            {
                if (_outlinable == null)
                    return default(SnapshotPoint);
                return new SnapshotPoint(this.Span.Snapshot, _outlinable.StartIndex);
            }
        }

        public BlockType Type
        {
            get
            {
                return this.type;
            }
        }

        // Nested Types
        public class TextBlob : FrameworkElement
        {
            // Fields
            private FormattedText text;

            // Methods
            public TextBlob(FormattedText text)
            {
                this.text = text;
                base.Width = text.Width;
                base.Height = text.Height;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                drawingContext.DrawText(this.text, new Point(0.0, 0.0));
            }
        }
    }
}
