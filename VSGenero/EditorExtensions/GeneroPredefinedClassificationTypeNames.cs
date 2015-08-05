/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
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
