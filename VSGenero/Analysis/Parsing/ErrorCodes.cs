using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public static class ErrorCodes
    {
        // The error flags
        public const int IncompleteMask = 0x000F;

        /// <summary>
        /// The error involved an incomplete statement due to an unexpected EOF.
        /// </summary>
        public const int IncompleteStatement = 0x0001;

        /// <summary>
        /// The error involved an incomplete token.
        /// </summary>
        public const int IncompleteToken = 0x0002;

        /// <summary>
        /// The mask for the actual error values 
        /// </summary>
        public const int ErrorMask = 0x7FFFFFF0;

        /// <summary>
        /// The error was a general syntax error
        /// </summary>
        public const int SyntaxError = 0x0010;
    }
}
