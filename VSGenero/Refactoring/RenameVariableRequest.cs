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

namespace VSGenero.Refactoring
{
    /// <summary>
    /// Encapsulates all of the possible knobs which can be flipped when renaming a variable.
    /// </summary>
    class RenameVariableRequest
    {
        public readonly string Name;
        public readonly bool Preview, SearchInComments, SearchInStrings;

        public RenameVariableRequest(string name, bool preview, bool searchInComments, bool searchInStrings)
        {
            Name = name;
            Preview = preview;
            SearchInComments = searchInComments;
            SearchInStrings = searchInStrings;
        }
    }
}
