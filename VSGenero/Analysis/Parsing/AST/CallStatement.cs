using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class CallStatement : FglStatement
    {
        public FunctionCallExpressionNode Function { get; private set; }

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
                FunctionCallExpressionNode functionCall;
                NameExpression dummy;
                if (!FunctionCallExpressionNode.TryParseExpression(parser, out functionCall, out dummy, true))
                {
                    parser.ReportSyntaxError("Unexpected token found in call statement, expecting name expression.");
                }
                else
                {
                    node.Function = functionCall;

                    if (parser.PeekToken(TokenKind.ReturningKeyword))
                    {
                        parser.NextToken();

                        NameExpression name;
                        // get return values
                        while (NameExpression.TryParseNode(parser, out name, TokenKind.Comma))
                        {
                            node.Returns.Add(name);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }
                    }
                }
            }

            return result;
        }
    }
}
