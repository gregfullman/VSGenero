using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public abstract class ExpressionNode : AstNode
    {
        public abstract void AppendExpression(ExpressionNode node);

        public static bool TryGetExpressionNode(Parser parser, out ExpressionNode node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenCategory.StringLiteral) || 
               parser.PeekToken(TokenCategory.CharacterLiteral) ||
               parser.PeekToken(TokenCategory.IncompleteMultiLineStringLiteral))
            {
                result = true;
                parser.NextToken();
                node = new StringExpressionNode(parser.Token);
            }
            else
            {
                result = true;
                parser.NextToken();
                node = new TokenExpressionNode(parser.Token);
            }

            return result;
        }
    }

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

        public StringExpressionNode(TokenWithSpan token)
            : base(token)
        {
            _literalValue = new StringBuilder(token.Token.Value.ToString());
        }

        public override void AppendExpression(ExpressionNode node)
        {
            if(node is StringExpressionNode)
            {
                _literalValue.Append((node as StringExpressionNode).LiteralValue);
                
            }
            else if(node is TokenExpressionNode)
            {
                // TODO: we should be able to follow the coversion rules to append to the literal value
            }
            base.AppendExpression(node);
        }
    }

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

        public TokenExpressionNode(TokenWithSpan token)
        {
            _tokens = new List<Token>();
            StartIndex = token.Span.Start;
            _tokens.Add(token.Token);
        }

        public override void AppendExpression(ExpressionNode node)
        {
            if (node is TokenExpressionNode)
            {
                _tokens.AddRange((node as TokenExpressionNode).Tokens);
            }
            EndIndex = node.EndIndex;
        }
    }
}
