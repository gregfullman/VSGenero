using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    /// <summary>
    /// Base class for token-based expresssions
    /// </summary>
    public class TokenExpressionNode : ExpressionNode
    {
        protected List<Token> _tokens;
        public List<Token> Tokens
        {
            get { return _tokens; }
        }

        protected TokenExpressionNode()
        {
        }

        public TokenExpressionNode(TokenWithSpan token)
        {
            _tokens = new List<Token>();
            StartIndex = token.Span.Start;
            _tokens.Add(token.Token);
            EndIndex = token.Span.End;
        }

        protected override string GetStringForm()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Tokens.Count; i++)
            {
                sb.Append(Tokens[i].Value.ToString());
                if (i + 1 < Tokens.Count)
                    sb.Append(" ");
            }
            return sb.ToString();
        }

        public override string GetExpressionType(GeneroAst ast)
        {
            // TODO: determine the type from the token we have
            return null;
        }
    }
}
