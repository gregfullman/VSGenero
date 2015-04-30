using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public abstract class ExpressionNode : AstNode
    {
        public abstract void PrependExpression(ExpressionNode node);
        public abstract void AppendExpression(ExpressionNode node);
        public abstract void AppendOperator(TokenExpressionNode tokenKind);
        public abstract string ToString();
        public abstract string GetType();

        public static bool TryGetExpressionNode(Parser parser, out ExpressionNode node, List<TokenKind> breakTokens = null)
        {
            node = null;
            bool result = false;
            bool start = true;

            Token nextTok;
            TokenExpressionNode startingToken = null;
            while (true)
            {
                // For right now, let's allow an operator in front of the expression
                nextTok = parser.PeekToken();
                // Check to see if the token is an operator, in which case we can continue gathering the expression 
                if (start &&
                   nextTok.Kind >= TokenKind.FirstOperator &&
                   nextTok.Kind <= TokenKind.LastOperator)
                {
                    parser.NextToken();
                    startingToken = new TokenExpressionNode(parser.Token);
                }
                start = false;
                if (parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    ParenWrappedExpressionNode parenExpr;
                    if (ParenWrappedExpressionNode.TryParseExpression(parser, out parenExpr))
                    {
                        if (node == null)
                            node = parenExpr;
                        else
                            node.AppendExpression(parenExpr);
                        result = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Paren-nested expression expected.");
                    }
                }
                else if (parser.PeekToken(TokenCategory.StringLiteral) ||
                        parser.PeekToken(TokenCategory.CharacterLiteral) ||
                        parser.PeekToken(TokenCategory.IncompleteMultiLineStringLiteral))
                {
                    result = true;
                    parser.NextToken();
                    if (node == null)
                        node = new StringExpressionNode(parser.Token);
                    else
                        node.AppendExpression(new StringExpressionNode(parser.Token));
                }
                else if (parser.PeekToken(TokenCategory.NumericLiteral))
                {
                    result = true;
                    parser.NextToken();
                    if (node == null)
                        node = new TokenExpressionNode(parser.Token);
                    else
                        node.AppendExpression(new TokenExpressionNode(parser.Token));
                }
                else if (parser.PeekToken(TokenCategory.Identifier) ||
                        parser.PeekToken(TokenCategory.Keyword))
                {
                    FunctionCallExpressionNode funcCall;
                    NameExpression nonFuncCallName;
                    if (FunctionCallExpressionNode.TryParseExpression(parser, out funcCall, out nonFuncCallName))
                    {
                        result = true;
                        if (node == null)
                            node = funcCall;
                        else
                            node.AppendExpression(funcCall);
                    }
                    else if (nonFuncCallName != null)
                    {
                        // it's a name expression
                        result = true;
                        if (node == null)
                            node = nonFuncCallName;
                        else
                            node.AppendExpression(nonFuncCallName);
                    }
                    else
                    {
                        result = true;
                        parser.NextToken();
                        if (node == null)
                            node = new TokenExpressionNode(parser.Token);
                        else
                            node.AppendExpression(new TokenExpressionNode(parser.Token));
                    }
                }
                else
                {
                    var tok = parser.PeekToken();
                    if (breakTokens != null && !breakTokens.Contains(tok.Kind))
                    {
                        parser.ReportSyntaxError("Invalid token type found in expression.");
                        break;
                    }
                }

                if(startingToken != null)
                {
                    node.PrependExpression(startingToken);
                }

                nextTok = parser.PeekToken();
                // Check to see if the token is an operator, in which case we can continue gathering the expression 
                if(breakTokens != null &&
                   !breakTokens.Contains(nextTok.Kind) && 
                   nextTok.Kind >= TokenKind.FirstOperator && 
                   nextTok.Kind <= TokenKind.LastOperator)
                {
                    parser.NextToken();
                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }

    public class FunctionCallExpressionNode : ExpressionNode
    {
        public NameExpression Function { get; private set; }

        private List<ExpressionNode> _params;
        public List<ExpressionNode> Parameters
        {
            get
            {
                if (_params == null)
                    _params = new List<ExpressionNode>();
                return _params;
            }
        }

        public static bool TryParseExpression(Parser parser, out FunctionCallExpressionNode node, out NameExpression nonFunctionCallName, bool leftParenRequired = false)
        {
            node = null;
            nonFunctionCallName = null;
            bool result = false;

            NameExpression name;
            if (NameExpression.TryParseNode(parser, out name, TokenKind.LeftParenthesis))
            {
                node = new FunctionCallExpressionNode();
                node.StartIndex = name.StartIndex;
                node.Function = name;

                // get the left paren
                if (parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    result = true;
                    parser.NextToken();
                    // Parameters can be any expression, comma seperated
                    ExpressionNode expr;
                    while (ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                    {
                        node.Parameters.Add(expr);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }

                    // get the right paren
                    if (parser.PeekToken(TokenKind.RightParenthesis))
                    {
                        parser.NextToken(); // TODO: not sure if this is needed
                    }
                    else
                    {
                        parser.ReportSyntaxError("Call statement missing right parenthesis.");
                    }
                }
                else
                {
                    if (leftParenRequired)
                    {
                        result = true;
                        parser.ReportSyntaxError("Call statement missing left parenthesis.");
                    }
                    else
                    {
                        nonFunctionCallName = name;
                        node = null;
                    }
                }
            }

            return result;
        }

        public override void AppendExpression(ExpressionNode node)
        {
        }

        public override void AppendOperator(TokenExpressionNode tokenKind)
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Function.Name);
            sb.Append("(");
            for (int i = 0; i < Parameters.Count; i++ )
            {
                sb.Append(Parameters[i].ToString());
                if (i + 1 < Parameters.Count)
                    sb.Append(", ");
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override string GetType()
        {
            // TODO: need to determine the return type for the function call
            return null;
        }

        public override void PrependExpression(ExpressionNode node)
        {
        }
    }

    public class ParenWrappedExpressionNode : ExpressionNode
    {
        public ExpressionNode InnerExpression { get; private set; }

        public static bool TryParseExpression(Parser parser, out ParenWrappedExpressionNode node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.LeftParenthesis))
            {
                parser.NextToken();
                node = new ParenWrappedExpressionNode();
                result = true;
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode exprNode;
                if(!ExpressionNode.TryGetExpressionNode(parser, out exprNode, new List<TokenKind> { TokenKind.RightParenthesis }))
                {
                    parser.ReportSyntaxError("Invalid expression found within parentheses.");
                }

                if(parser.PeekToken(TokenKind.RightParenthesis))
                {
                    parser.NextToken();
                    node.EndIndex = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError("Right parenthesis not found.");
                }
            }

            return result;
        }

        public override void AppendExpression(ExpressionNode node)
        {
        }

        public override void AppendOperator(TokenExpressionNode tokenKind)
        {
        }

        public override string ToString()
        {
            return string.Format("({0})", InnerExpression.ToString());
        }

        public override string GetType()
        {
            return null;
        }

        public override void PrependExpression(ExpressionNode node)
        {
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

        public override string ToString()
        {
            return LiteralValue;
        }
    }

    //public class IntegerExpressionNode : TokenExpressionNode
    //{

    //}

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

        public override void AppendOperator(TokenExpressionNode tokenKind)
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < Tokens.Count; i++)
            {
                sb.Append(Tokens[i].Value.ToString());
                if (i + 1 < Tokens.Count)
                    sb.Append(" ");
            }
            return sb.ToString();
        }

        public override string GetType()
        {
            return null;
        }

        public override void PrependExpression(ExpressionNode node)
        {
        }
    }
}
