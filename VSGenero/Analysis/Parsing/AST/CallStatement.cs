using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class CallStatement : FglStatement
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

        private List<NameExpression> _returns;
        public List<NameExpression> Returns
        {
            get
            {
                if (_returns == null)
                    _returns = new List<NameExpression>();
                return _returns;
            }
        }

        public static bool TryParseNode(Parser parser, out CallStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.CallKeyword))
            {
                result = true;
                node = new CallStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // get the function name
                NameExpression name;
                if(!NameExpression.TryParseNode(parser, out name, TokenKind.LeftParenthesis))
                {
                    parser.ReportSyntaxError("Unexpected token found in call statement, expecting name expression.");
                }
                else
                {
                    node.Function = name;
                }

                // get the left paren
                if(parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();
                    // Parameters can be any expression, comma seperated
                    ExpressionNode expr;
                    while(ExpressionNode.TryGetExpressionNode(parser, out expr, new List<TokenKind> { TokenKind.Comma, TokenKind.RightParenthesis }))
                    {
                        node.Parameters.Add(expr);
                        if (!parser.PeekToken(TokenKind.Comma))
                            break;
                        parser.NextToken();
                    }

                    // get the right paren
                    if(parser.PeekToken(TokenKind.RightParenthesis))
                    {
                        parser.NextToken();

                        if (parser.PeekToken(TokenKind.ReturningKeyword))
                        {
                            parser.NextToken();

                            // get return values
                            while(NameExpression.TryParseNode(parser, out name, TokenKind.Comma))
                            {
                                node.Returns.Add(name);
                                if (!parser.PeekToken(TokenKind.Comma))
                                    break;
                                parser.NextToken();
                            }
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Call statement missing right parenthesis.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Call statement missing left parenthesis.");
                }
            }

            return result;
        }
    }
}
