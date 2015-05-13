using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    [Serializable]
    public struct TokenInfo : IEquatable<TokenInfo>
    {
        private TokenCategory _category;
        private TokenTriggers _trigger;
        private SourceSpan _span;
        private Token _token;

        public Token Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public TokenCategory Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public TokenTriggers Trigger
        {
            get { return _trigger; }
            set { _trigger = value; }
        }

        public SourceSpan SourceSpan
        {
            get { return _span; }
            set { _span = value; }
        }

        internal TokenInfo(SourceSpan span, TokenCategory category, TokenTriggers trigger, Token token)
        {
            _category = category;
            _trigger = trigger;
            _span = span;
            _token = token;
        }

        #region IEquatable<TokenInfo> Members

        public bool Equals(TokenInfo other)
        {
            return _category == other._category && _trigger == other._trigger && _span == other._span && _token == other._token;
        }

        #endregion

        public override string ToString()
        {
            return String.Format("TokenInfo: {0}, {1}, {2}", _span, _category, _trigger);
        }

        public TokenWithSpan ToTokenWithSpan()
        {
            return new TokenWithSpan(Token, new IndexSpan(SourceSpan.Start.Index, SourceSpan.Length), 0);
        }
    }
}