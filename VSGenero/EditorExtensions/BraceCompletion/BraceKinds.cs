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
