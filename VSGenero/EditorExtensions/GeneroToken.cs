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
