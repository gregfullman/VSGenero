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
using VSGenero.Analysis.Parsing;

namespace VSGenero.EditorExtensions
{
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
        public TokenCategory TokenType { get; private set; }

        public GeneroToken(string tokenText, int start, int end, TokenCategory type, int lineNumber, int colNumber)
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

        public void SetType(TokenCategory type)
        {
            TokenType = type;
        }

        public void AddPositionOffset(int offset)
        {
            StartPosition += offset;
            EndPosition += offset;
        }
    }
}
