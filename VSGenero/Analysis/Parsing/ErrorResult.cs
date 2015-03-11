using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public class ErrorResult
    {
        private readonly string _message;
        private readonly SourceSpan _span;

        public ErrorResult(string message, SourceSpan span)
        {
            _message = message;
            _span = span;
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public SourceSpan Span
        {
            get
            {
                return _span;
            }
        }
    }
}
