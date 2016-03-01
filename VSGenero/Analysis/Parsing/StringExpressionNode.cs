using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    /// <summary>
    /// Encapsulates expressions based on string-type literals
    /// </summary>
    public class StringExpressionNode : TokenExpressionNode
    {
        private StringBuilder _literalValue;
        public string LiteralValue
        {
            get { return _literalValue.ToString(); }
        }

        public StringExpressionNode(string value)
        {
            _literalValue = new StringBuilder(value);
        }

        public StringExpressionNode(TokenWithSpan token)
            : base(token)
        {
            _literalValue = new StringBuilder(token.Token.Value.ToString());
        }

        protected override string GetStringForm()
        {
            return LiteralValue;
        }

        public override string GetExpressionType(GeneroAst ast)
        {
            return "string";
        }

        public static bool TryGetExpressionNode(IParser parser, out StringExpressionNode node)
        {
            bool result = false;
            node = null;

            if (parser.PeekToken(TokenCategory.StringLiteral) ||
                parser.PeekToken(TokenCategory.CharacterLiteral) ||
                parser.PeekToken(TokenCategory.IncompleteMultiLineStringLiteral))
            {
                parser.NextToken();
                node = new StringExpressionNode(parser.Token);
                node.StartIndex = parser.Token.Span.Start;
                node.EndIndex = parser.Token.Span.End;
                result = true;
            }

            return result;
        }
    }
}
