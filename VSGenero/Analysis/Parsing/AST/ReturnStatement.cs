using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class ReturnStatement : FglStatement
    {
        private List<ExpressionNode> _returns;
        public List<ExpressionNode> Returns
        {
            get
            {
                if (_returns == null)
                    _returns = new List<ExpressionNode>();
                return _returns;
            }
        }

        public static bool TryParseNode(Parser parser, out ReturnStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.ReturnKeyword))
            {
                parser.NextToken();
                result = true;
                node = new ReturnStatement();
                node.StartIndex = parser.Token.Span.Start;

                while (true)
                {
                    // TODO: not sure about this....it was in here for a reason, right?
                    var tok = parser.PeekToken();
                    if (GeneroAst.ValidStatementKeywords.Contains(tok.Kind) &&
                        !GeneroAst.Acceptable_ReturnVariableName_StatementKeywords.Contains(tok.Kind))
                    {
                        // TODO: need to check and see if there are any variables defined with the same name as the statement keyword?
                        break;
                    }

                    ExpressionNode expr;
                    if (!ExpressionNode.TryGetExpressionNode(parser, out expr ))
                    {
                        break;
                    }
                    node.Returns.Add(expr);

                    if (!parser.PeekToken(TokenKind.Comma))
                    {
                        break;
                    }
                    parser.NextToken();
                }
            }

            return result;
        }
    }
}
