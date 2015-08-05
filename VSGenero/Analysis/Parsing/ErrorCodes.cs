/* ****************************************************************************
*
* Copyright (c) Microsoft Corporation. 
*
* This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
* copy of the license can be found in the License.html file at the root of this distribution. If 
* you cannot locate the Apache License, Version 2.0, please send an email to 
* vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
* by the terms of the Apache License, Version 2.0.
*
* You must not remove this notice, or any other, from this software.
*
* ***************************************************************************/

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
