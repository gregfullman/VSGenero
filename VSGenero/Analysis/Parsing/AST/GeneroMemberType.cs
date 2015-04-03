using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public enum GeneroMemberType
    {
        Unknown,
        /// <summary>
        /// The result is a user defined or built-in class.
        /// </summary>
        Class,
        /// <summary>
        /// An instance of a user defined or built-in class.
        /// </summary>
        Instance,
        /// <summary>
        /// An instance of a user defined or built-in function.
        /// </summary>
        Function,
        /// <summary>
        /// An instance of a user defined or built-in method.
        /// </summary>
        Method,
        /// <summary>
        /// An instance of a built-in or user defined module.
        /// </summary>
        Module,
        /// <summary>
        /// A constant defined in source code.
        /// </summary>
        Constant,

        /// <summary>
        /// The member represents a keyword
        /// </summary>
        Keyword,

        Variable,

        DbTable,
        DbColumn
    }
}
