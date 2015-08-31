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

namespace VSGenero.Analysis.Parsing
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
        DbView,
        DbColumn,
        Namespace,
        Dialog,
        Report
    }
}
