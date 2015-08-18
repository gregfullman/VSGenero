using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;

namespace VSGenero.Analysis
{
    public class CommentError
    {
        public string ErrorMessage { get; set; }
        public SourceSpan Span { get; set; }
    }

    public interface ICommentValidator
    {
        string ValidStartsWith { get; }
        CommentError ProcessComment(IProjectEntry projectEntry, SourceSpan span, string text);
    }
}
