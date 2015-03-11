using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public enum ParseResult
    {
        /// <summary>
        /// Source code is a syntactically correct.
        /// </summary>
        Complete,

        /// <summary>
        /// Source code represents an empty statement/expression.
        /// </summary>
        Empty,

        /// <summary>
        /// Source code is already invalid and no suffix can make it syntactically correct.
        /// </summary>
        Invalid,

        /// <summary>
        /// Last token is incomplete. Source code can still be completed correctly.
        /// </summary>
        IncompleteToken,

        /// <summary>
        /// Last statement is incomplete. Source code can still be completed correctly.
        /// </summary>
        IncompleteStatement,
    }
}
