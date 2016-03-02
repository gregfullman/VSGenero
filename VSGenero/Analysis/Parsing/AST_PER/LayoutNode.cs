using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class LayoutNode : AstNodePer
    {
        public static bool TryParseNode(IParser parser, out LayoutNode node)
        {
            bool result = false;
            node = null;

            if (parser.PeekToken(TokenKind.LayoutKeyword))
            {
                result = true;
                node = new LayoutNode();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // TODO: layout


                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
