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
                result = true;
                node = new ReturnStatement();
                while (true)
                {
                    ExpressionNode expr;
                    if (!ExpressionNode.TryGetExpressionNode(parser, out expr))
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
