using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public sealed class ParserOptions
    {
        internal static ParserOptions Default = new ParserOptions();
        public ParserOptions()
        {
            ErrorSink = ErrorSink.Null;
        }

        public ErrorSink ErrorSink { set; get; }
        public Severity IndentationInconsistencySeverity { set; get; }
        public bool Verbatim { get; set; }

        /// <summary>
        /// True if references to variables should be bound in the AST.  The root node must be
        /// held onto to access the references via GetReference/GetReferences APIs on various 
        /// nodes which reference variables.
        /// </summary>
        public bool BindReferences { get; set; }

        /// <summary>
        /// Specifies the class name the parser starts off with for name mangling name expressions.
        /// 
        /// For example __fob would turn into _C__fob if PrivatePrefix is set to C.
        /// </summary>
        public string PrivatePrefix { get; set; }

    }
}
