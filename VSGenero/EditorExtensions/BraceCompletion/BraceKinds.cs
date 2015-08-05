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

namespace VSGenero.EditorExtensions.BraceCompletion
{
    internal static class BraceKinds
    {
        // Nested Types
        public static class Bracket
        {
            // Fields
            public const char CloseCharacter = ']';
            public const char OpenCharacter = '[';
        }

        public static class CurlyBrace
        {
            // Fields
            public const char CloseCharacter = '}';
            public const char OpenCharacter = '{';
        }

        public static class DoubleQuote
        {
            // Fields
            public const char CloseCharacter = '"';
            public const char OpenCharacter = '"';
        }

        public static class Parenthese
        {
            // Fields
            public const char CloseCharacter = ')';
            public const char OpenCharacter = '(';
        }

        public static class SingleQuote
        {
            // Fields
            public const char CloseCharacter = '\'';
            public const char OpenCharacter = '\'';
        }

    }
}
