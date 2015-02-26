using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// This is the class on which all other AST nodes are based.
    /// It provides a SnapshotSpan of itself.
    /// </summary>
    public abstract class AstNode
    {
        public IndexSpan _span;

        public int EndIndex
        {
            get
            {
                return _span.End;
            }
            set
            {
                _span = new IndexSpan(_span.Start, value - _span.Start);
            }
        }

        public int StartIndex
        {
            get
            {
                return _span.Start;
            }
            set
            {
                _span = new IndexSpan(value, 0);
            }
        }

        private SortedList<int, AstNode> _children;
        public SortedList<int, AstNode> Children
        {
            get
            {
                if (_children == null)
                    _children = new SortedList<int, AstNode>();
                return _children;
            }
        }
    }

    public class IndexSpanComparer : IComparer<IndexSpan>
    {
        public int Compare(IndexSpan x, IndexSpan y)
        {
            return x.Start.CompareTo(y.Start);
        }
    }
}
