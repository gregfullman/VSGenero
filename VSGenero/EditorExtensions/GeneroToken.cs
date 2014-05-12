/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
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

namespace VSGenero.EditorExtensions
{
    public enum GeneroTokenType
    {
        Eof,
        Number,
        Identifier,
        String,
        MultiLineString,
        Comment,
        MultiLineComment,
        Symbol,
        Keyword,
        Unknown
    }

    public class GeneroToken
    {
        public string TokenText { get; private set; }

        private string _lower = null;
        public string LowercaseText
        {
            get
            {
                if (_lower == null && TokenText != null)
                    _lower = TokenText.ToLower();
                return _lower;
            }
        }

        public int LineNumber { get; private set; }
        public int ColumnNumber { get; private set; }
        public int StartPosition { get; private set; }
        public int EndPosition { get; private set; }
        public bool IsIncomplete { get; set; }
        public int MultiLineEndingPosition { get; set; }
        public GeneroTokenType TokenType { get; private set; }

        public GeneroToken(string tokenText, int start, int end, GeneroTokenType type, int lineNumber, int colNumber)
        {
            TokenType = type;
            TokenSetup(tokenText, start, end, lineNumber, colNumber);
        }

        private void TokenSetup(string tokenText, int start, int end, int lineNumber, int colNumber)
        {
            TokenText = tokenText;
            StartPosition = start;
            EndPosition = end;
            LineNumber = lineNumber;
            ColumnNumber = colNumber;
        }

        public void SetType(GeneroTokenType type)
        {
            TokenType = type;
        }

        public void AddPositionOffset(int offset)
        {
            StartPosition += offset;
            EndPosition += offset;
            // increment the multi-line ending position if set
            if (MultiLineEndingPosition > 0)
                MultiLineEndingPosition += offset;
        }

        public bool IsIncompleteCompletingToken(GeneroToken token)
        {
            // Handle multi-line comment for right now
            if (TokenText[0] == '{')
            {
                return token.LowercaseText == "}";
            }
            // TODO: handle multi-line string
            return false;
        }
    }
}
