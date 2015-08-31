using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGenero.Analysis.Parsing
{
    public abstract class AstNode
    {
        public AstNode Parent { get; protected set; }
        public IndexSpan _span;
        public bool IsComplete { get; protected set; }

        internal Genero4glAst SyntaxTree { get; set; }

        public virtual GeneroMemberType MemberType
        {
            get
            {
                return GeneroMemberType.Unknown;
            }
        }

        public virtual string Description
        {
            get
            {
                return null;
            }
        }

        public virtual string ShortDescription
        {
            get
            {
                return null;
            }
        }

        public virtual string Documentation
        {
            get
            {
                return null;
            }
        }

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

        public virtual string NodeName
        {
            get
            {
                return GetType().Name;
            }
        }

        public virtual void SetNamespace(string ns)
        {
            foreach (var child in Children.Values)
                child.SetNamespace(ns);
        }
        //public string ToCodeString(GeneroAst ast)
        //{
        //    return ToCodeString(ast, CodeFormattingOptions.Default);
        //}

        //public string ToCodeString(GeneroAst ast, CodeFormattingOptions format)
        //{
        //    StringBuilder res = new StringBuilder();
        //    AppendCodeString(res, ast, format);
        //    return res.ToString();
        //}

        public abstract void PropagateSyntaxTree(IGeneroAst ast);

        //public SourceLocation GetStart(Genero4glAst parent)
        //{
        //    return parent.IndexToLocation(StartIndex);
        //}

        //public SourceLocation GetEnd(Genero4glAst parent)
        //{
        //    return parent.IndexToLocation(EndIndex);
        //}

        //public SourceSpan GetSpan(Genero4glAst parent)
        //{
        //    return new SourceSpan(GetStart(parent), GetEnd(parent));
        //}

        //public static void CopyLeadingWhiteSpace(Genero4glAst parentNode, AstNode fromNode, AstNode toNode)
        //{
        //    parentNode.SetAttribute(toNode, NodeAttributes.PreceedingWhiteSpace, fromNode.GetLeadingWhiteSpace(parentNode));
        //}

        /// <summary>
        /// Returns the proceeeding whitespace (newlines and comments) that
        /// shows up before this node.
        /// 
        /// New in 1.1.
        /// </summary>
        //public virtual string GetLeadingWhiteSpace(Genero4glAst ast)
        //{
        //    return this.GetProceedingWhiteSpaceDefaultNull(ast) ?? "";
        //}

        /// <summary>
        /// Sets the proceeding whitespace (newlines and comments) that shows up
        /// before this node.
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="whiteSpace"></param>
        //public virtual void SetLeadingWhiteSpace(Genero4glAst ast, string whiteSpace)
        //{
        //    ast.SetAttribute(this, NodeAttributes.PreceedingWhiteSpace, whiteSpace);
        //}

        /// <summary>
        /// Gets the indentation level for the current statement.  Requires verbose
        /// mode when parsing the trees.
        /// </summary>
        //public string GetIndentationLevel(Genero4glAst parentNode)
        //{
        //    var leading = GetLeadingWhiteSpace(parentNode);
        //    // we only want the trailing leading space for the current line...
        //    for (int i = leading.Length - 1; i >= 0; i--)
        //    {
        //        if (leading[i] == '\r' || leading[i] == '\n')
        //        {
        //            leading = leading.Substring(i + 1);
        //            break;
        //        }
        //    }
        //    return leading;
        //}

        #region Internal APIs

        /// <summary>
        /// Appends the code representation of the node to the string builder.
        /// </summary>
        //internal abstract void AppendCodeString(StringBuilder res, GeneroAst ast, CodeFormattingOptions format);

        /// <summary>
        /// Appends the code representation of the node to the string builder, replacing the initial whitespace.
        /// 
        /// If initialWhiteSpace is null then the default whitespace is used.
        /// </summary>
        //internal virtual void AppendCodeString(StringBuilder res, GeneroAst ast, CodeFormattingOptions format, string leadingWhiteSpace)
        //{
        //    if (leadingWhiteSpace == null)
        //    {
        //        AppendCodeString(res, ast, format);
        //        return;
        //    }
        //    res.Append(leadingWhiteSpace);
        //    StringBuilder tmp = new StringBuilder();
        //    AppendCodeString(tmp, ast, format);
        //    for (int curChar = 0; curChar < tmp.Length; curChar++)
        //    {
        //        if (!char.IsWhiteSpace(tmp[curChar]))
        //        {
        //            res.Append(tmp.ToString(curChar, tmp.Length - curChar));
        //            break;
        //        }
        //    }
        //}

        //internal void SetLoc(int start, int end)
        //{
        //    _span = new IndexSpan(start, end >= start ? end - start : start);
        //}

        //internal void SetLoc(IndexSpan span)
        //{
        //    _span = span;
        //}

        internal IndexSpan IndexSpan
        {
            get
            {
                return _span;
            }
            set
            {
                _span = value;
            }
        }

        //internal virtual string GetDocumentation(AstNode node)
        //{
        //    return node.Documentation;
        //}

        #endregion

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

        public virtual void FindAllReferences(IAnalysisResult item, List<IndexSpan> referenceList)
        {
        }

        /// <summary>
        /// The default method calls CheckForErrors on all AstNode children of this node.
        /// To do specific error checking, override this method and call the base method at the end.
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="errors"></param>
        public virtual void CheckForErrors(IGeneroAst ast, Action<string, int, int> errorFunc,
                                           Dictionary<string, List<int>> deferredFunctionSearches,
                                           Genero4glAst.FunctionProviderSearchMode searchInFunctionProvider = Genero4glAst.FunctionProviderSearchMode.NoSearch,
                                           bool isFunctionCallOrDefinition = false)
        {
            foreach (var child in Children)
                child.Value.CheckForErrors(ast, errorFunc, deferredFunctionSearches, searchInFunctionProvider, isFunctionCallOrDefinition);
        }
    }
}
