using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public abstract class ExpressionNode : AstNode
    {
        protected static List<TokenKind> _preExpressionTokens = new List<TokenKind> 
        { 
            TokenKind.NotKeyword, TokenKind.ColumnKeyword, TokenKind.Subtract, TokenKind.AsciiKeyword, TokenKind.Add
        };

        public abstract void PrependExpression(ExpressionNode node);
        public abstract void AppendExpression(ExpressionNode node);
        public abstract void AppendOperator(TokenExpressionNode tokenKind);
        public abstract string ToString();
        public abstract string GetType();

        public static bool TryGetExpressionNode(IParser parser, out ExpressionNode node, List<TokenKind> breakTokens = null, bool allowStarParam = false, bool allowAnythingForFunctionParams = false)
        {
            node = null;
            bool result = false;
            bool start = true;
            bool requireExpression = false;

            TokenExpressionNode startingToken = null;

            while (true)
            {
                // First check for allowed pre-expression tokens
                if (_preExpressionTokens.Contains(parser.PeekToken().Kind))
                {
                    parser.NextToken();
                    if (node == null)
                        node = new TokenExpressionNode(parser.Token);
                    else
                        node.AppendExpression(new TokenExpressionNode(parser.Token));
                    requireExpression = true;
                }

                if (parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    ParenWrappedExpressionNode parenExpr;
                    if (ParenWrappedExpressionNode.TryParseExpression(parser, out parenExpr, allowAnythingForFunctionParams))
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
                else if (parser.PeekToken(TokenKind.LeftBracket))
                {
                    BracketWrappedExpressionNode brackNode;
                    if(BracketWrappedExpressionNode.TryParseNode(parser, out brackNode))
                    {
                        if (node == null)
                            node = brackNode;
                        else
                            node.AppendExpression(brackNode);
                        result = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Bracket-nested expression expected.");
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
                else if (parser.PeekToken(TokenKind.CurrentKeyword))
                {
                    result = true;
                    parser.NextToken();
                    string currentTypeConstraint;
                    if (TypeConstraints.VerifyValidConstraint(parser, out currentTypeConstraint, TokenKind.CurrentKeyword, true))
                    {
                        result = true;
                        StringExpressionNode strExpr = new StringExpressionNode(currentTypeConstraint);
                        if (node == null)
                            node = strExpr;
                        else
                            node.AppendExpression(strExpr);
                    }
                    else
                    {
                        if (node == null)
                            node = new TokenExpressionNode(parser.Token);
                        else
                            node.AppendExpression(new TokenExpressionNode(parser.Token));
                    }
                }
                else if(parser.PeekToken(TokenKind.IntervalKeyword))
                {
                    parser.NextToken();
                    TokenExpressionNode intervalNode = new TokenExpressionNode(parser.Token);
                    if (node == null)
                        node = intervalNode;
                    else
                        node.AppendExpression(intervalNode);
                    if (parser.PeekToken(TokenKind.LeftParenthesis))
                    {
                        parser.NextToken();
                        node.AppendExpression(new TokenExpressionNode(parser.Token));
                        while(!parser.PeekToken(TokenKind.EndOfFile))
                        {
                            parser.NextToken();
                            node.AppendExpression(new TokenExpressionNode(parser.Token));
                            if (parser.Token.Token.Kind == TokenKind.RightParenthesis)
                                break;
                        }
                        string intervalString;
                        if (TypeConstraints.VerifyValidConstraint(parser, out intervalString, TokenKind.IntervalKeyword, true))
                        {
                            result = true;
                            node.AppendExpression(new StringExpressionNode(intervalString));
                        }
                        else
                            parser.ReportSyntaxError("Invalid interval expression found.");
                    }
                }
                
                else if (parser.PeekToken(TokenCategory.Identifier) ||
                        parser.PeekToken(TokenCategory.Keyword))
                {
                    FunctionCallExpressionNode funcCall;
                    NameExpression nonFuncCallName;
                    if (FunctionCallExpressionNode.TryParseExpression(parser, out funcCall, out nonFuncCallName, false, allowStarParam, allowAnythingForFunctionParams))
                    {
                        result = true;
                        if (node == null)
                            node = funcCall;
                        else
                            node.AppendExpression(funcCall);
                    }
                    else if (nonFuncCallName != null)
                    {
                        bool isDatetime = false;
                        var dtToken = Tokens.GetToken(nonFuncCallName.Name);
                        if(dtToken != null)
                        {
                            if (TypeConstraints.DateTimeQualifiers.Contains(dtToken.Kind))
                            {
                                string dtString;
                                isDatetime = true;
                                if (TypeConstraints.VerifyValidConstraint(parser, out dtString, TokenKind.DatetimeKeyword, true, dtToken.Kind))
                                {
                                    result = true;
                                    var strExpr = new StringExpressionNode(dtString);
                                    if (node == null)
                                        node = strExpr;
                                    else
                                        node.AppendExpression(strExpr);
                                }
                            }
                        }

                        if (!isDatetime)
                        {
                            // it's a name expression
                            result = true;
                            if (node == null)
                                node = nonFuncCallName;
                            else
                                node.AppendExpression(nonFuncCallName);
                        }
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
                else if (parser.PeekToken(TokenKind.Multiply) && allowStarParam)
                {
                    result = true;
                    parser.NextToken();
                    if (node == null)
                        node = new TokenExpressionNode(parser.Token);
                    else
                        node.AppendExpression(new TokenExpressionNode(parser.Token));
                }
                else
                {
                    if (requireExpression)
                    {
                        var tok = parser.PeekToken();
                        if (breakTokens != null && !breakTokens.Contains(tok.Kind))
                            parser.ReportSyntaxError("Invalid token type found in expression.");
                        else
                            parser.ReportSyntaxError("Expression required.");
                    }
                    break;
                }
                requireExpression = false;

                Token nextTok = parser.PeekToken();
                bool isOperator = true;
                while (isOperator && !requireExpression)
                {
                    if ((breakTokens == null ||
                         (breakTokens != null && !breakTokens.Contains(nextTok.Kind))) &&
                        nextTok.Kind >= TokenKind.FirstOperator &&
                        nextTok.Kind <= TokenKind.LastOperator)
                    {
                        parser.NextToken();
                        // TODO: not sure if we want to do more analysis on what operators can start an expression
                        if (node == null)
                            node = new TokenExpressionNode(parser.Token);
                        else
                            node.AppendExpression(new TokenExpressionNode(parser.Token));

                        switch (parser.Token.Token.Kind)
                        {
                            case TokenKind.LessThan:
                                // check for '<=' or '<>'
                                if (parser.PeekToken(TokenKind.Equals) ||
                                   parser.PeekToken(TokenKind.GreaterThan))
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                break;
                            case TokenKind.GreaterThan:
                                // check for '>='
                                if (parser.PeekToken(TokenKind.Equals))
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                break;
                            case TokenKind.Exclamation:
                                // check for '!='
                                if (parser.PeekToken(TokenKind.Equals))
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Invalid token '!' found in expression.");
                                }
                                break;
                            case TokenKind.Equals:
                                // check for '=='
                                if (parser.PeekToken(TokenKind.Equals))
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                break;
                            case TokenKind.SingleBar:
                                //  check for '||'
                                if (parser.PeekToken(TokenKind.SingleBar))
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Invalid token '|' found in expression.");
                                }
                                break;
                        }
                        requireExpression = true;
                    }
                    else
                    {
                        // check for non-symbol operators
                        switch (nextTok.Kind)
                        {
                            case TokenKind.DoubleBar:
                            case TokenKind.AsKeyword:
                            case TokenKind.AndKeyword:
                            case TokenKind.OrKeyword:
                            case TokenKind.ModKeyword:
                            case TokenKind.UsingKeyword:
                            case TokenKind.InstanceOfKeyword:
                            case TokenKind.UnitsKeyword:
                            case TokenKind.LikeKeyword:
                            case TokenKind.MatchesKeyword:
                                {
                                    // require another expression
                                    requireExpression = true;
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                break;
                            case TokenKind.ClippedKeyword:
                            case TokenKind.SpacesKeyword:
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                break;
                            case TokenKind.IsKeyword:
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                    if (parser.PeekToken(TokenKind.NotKeyword))
                                    {
                                        parser.NextToken();
                                        node.AppendExpression(new TokenExpressionNode(parser.Token));
                                    }
                                    if (parser.PeekToken(TokenKind.NullKeyword))
                                    {
                                        parser.NextToken();
                                        node.AppendExpression(new TokenExpressionNode(parser.Token));
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError("NULL keyword required in expression.");
                                    }
                                }
                                break;
                            case TokenKind.NotKeyword:
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                    if (parser.PeekToken(TokenKind.LikeKeyword) ||
                                       parser.PeekToken(TokenKind.MatchesKeyword))
                                    {
                                        // require another expression
                                        requireExpression = true;
                                        parser.NextToken();
                                        node.AppendExpression(new TokenExpressionNode(parser.Token));
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError("LIKE or MATCHES keyword required in expression.");
                                    }
                                }
                                break;
                            default:
                                isOperator = false;
                                break;
                        }
                        if (!isOperator)
                            break;
                        else
                        {
                            nextTok = parser.PeekToken();
                            if (nextTok.Kind == TokenKind.EndOfFile)
                                break;
                        }
                    }
                }
                if (!requireExpression)
                {
                    break;
                }
            }

            if (result && node != null)
            {
                node.EndIndex = parser.Token.Span.End;
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

        private List<Token> _anythingParameters;
        public List<Token> AnythingParameters
        {
            get
            {
                if (_anythingParameters == null)
                    _anythingParameters = new List<Token>();
                return _anythingParameters;
            }
        }

        public static bool TryParseExpression(IParser parser, out FunctionCallExpressionNode node, out NameExpression nonFunctionCallName, bool leftParenRequired = false, bool allowStarParam = false, bool allowAnythingParam = false)
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

                    if (!allowAnythingParam)
                    {
                        // Parameters can be any expression, comma seperated
                        ExpressionNode expr;
                        while (ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }, allowStarParam))
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
                        // just 
                        int rightParenLevel = 1;
                        Token tok;
                        bool done = false;
                        while (true)
                        {
                            tok = parser.NextToken();
                            switch (tok.Kind)
                            {
                                case TokenKind.LeftParenthesis:
                                    rightParenLevel++;
                                    break;
                                case TokenKind.RightParenthesis:
                                    rightParenLevel--;
                                    if (rightParenLevel == 0)
                                        done = true;
                                    break;
                                case TokenKind.EndOfFile:
                                    done = true;
                                    break;
                                default:
                                    node.AnythingParameters.Add(tok);
                                    break;
                            }
                            if (done)
                                break;
                        }
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
            for (int i = 0; i < Parameters.Count; i++)
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

    public class BracketWrappedExpressionNode : ExpressionNode
    {
        private List<ExpressionNode> _parameters;
        public List<ExpressionNode> Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = new List<ExpressionNode>();
                return _parameters;
            }
        }

        public static bool TryParseNode(IParser parser, out BracketWrappedExpressionNode node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.LeftBracket))
            {
                result = true;
                node = new BracketWrappedExpressionNode();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                ExpressionNode expr;
                while (ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma, TokenKind.LeftBracket }))
                {
                    node.Parameters.Add(expr);
                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }

                // get the right paren
                if (parser.PeekToken(TokenKind.RightBracket))
                {
                    parser.NextToken(); // TODO: not sure if this is needed
                }
                else
                {
                    parser.ReportSyntaxError("Call statement missing right bracket.");
                }
                node.EndIndex = parser.Token.Span.End;
            }
            return result;
        }

        public override void PrependExpression(ExpressionNode node)
        {
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
            sb.Append("[");
            for (int i = 0; i < Parameters.Count; i++ )
            {
                sb.Append(Parameters[i].ToString());
                if (i + 1 < Parameters.Count)
                    sb.Append(", ");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public override string GetType()
        {
            return null;
        }
    }

    public class ParenWrappedExpressionNode : ExpressionNode
    {
        public ExpressionNode InnerExpression { get; private set; }

        private List<Token> _anythingTokens;
        public List<Token> AnythingTokens
        {
            get
            {
                if (_anythingTokens == null)
                    _anythingTokens = new List<Token>();
                return _anythingTokens;
            }
        }

        public static bool TryParseExpression(IParser parser, out ParenWrappedExpressionNode node, bool allowAnythingInParens = false)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.LeftParenthesis))
            {
                parser.NextToken();
                node = new ParenWrappedExpressionNode();
                result = true;
                node.StartIndex = parser.Token.Span.Start;

                if (!allowAnythingInParens)
                {
                    ExpressionNode exprNode;
                    if (!ExpressionNode.TryGetExpressionNode(parser, out exprNode, new List<TokenKind> { TokenKind.RightParenthesis }))
                    {
                        parser.ReportSyntaxError("Invalid expression found within parentheses.");
                    }
                    else
                    {
                        node.InnerExpression = exprNode;
                    }

                    if (parser.PeekToken(TokenKind.RightParenthesis))
                    {
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Right parenthesis not found.");
                    }
                }
                else
                {
                    // just 
                    int rightParenLevel = 1;
                    Token tok;
                    bool done = false;
                    while (true)
                    {
                        tok = parser.NextToken();
                        switch (tok.Kind)
                        {
                            case TokenKind.LeftParenthesis:
                                rightParenLevel++;
                                break;
                            case TokenKind.RightParenthesis:
                                rightParenLevel--;
                                if (rightParenLevel == 0)
                                    done = true;
                                break;
                            case TokenKind.EndOfFile:
                                done = true;
                                break;
                            default:
                                node.AnythingTokens.Add(tok);
                                break;
                        }
                        if (done)
                            break;
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
            if (InnerExpression != null)
                return string.Format("({0})", InnerExpression.ToString());
            return null;
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

        public StringExpressionNode(string value)
        {
            _literalValue = new StringBuilder(value);
        }

        public StringExpressionNode(TokenWithSpan token)
            : base(token)
        {
            _literalValue = new StringBuilder(token.Token.Value.ToString());
        }

        public override void AppendExpression(ExpressionNode node)
        {
            if (node is StringExpressionNode)
            {
                _literalValue.Append((node as StringExpressionNode).LiteralValue);

            }
            else if (node is TokenExpressionNode)
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

        protected TokenExpressionNode()
        {
        }

        public TokenExpressionNode(TokenWithSpan token)
        {
            _tokens = new List<Token>();
            StartIndex = token.Span.Start;
            _tokens.Add(token.Token);
        }

        public override void AppendExpression(ExpressionNode node)
        {
            if (node != null && node is TokenExpressionNode && _tokens != null && (node as TokenExpressionNode).Tokens != null)
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
            for (int i = 0; i < Tokens.Count; i++)
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
