using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions
{
    public static class Genero4glPredefinedClassificationTypeNames
    {
        /// <summary>
        /// Open grouping classification.  Used for (, [, {, ), ], and }...  A subtype of the Genero
        /// operator grouping.
        /// </summary>
        public const string Grouping = "Genero grouping";

        /// <summary>
        /// Classification used for comma characters when used outside of a literal, comment, etc...
        /// </summary>
        public const string Comma = "Genero comma";

        /// <summary>
        /// Classification used for . characters when used outside of a literal, comment, etc...
        /// </summary>
        public const string Dot = "Genero dot";

        /// <summary>
        /// Classification used for all other operators
        /// </summary>
        public const string Operator = "Genero operator";
    }
}
