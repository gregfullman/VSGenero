using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class TablesNode : AstNodePer
    {
        public static bool TryParseNode(IParser parser, out TablesNode node)
        {
            bool result = false;
            node = null;

            if(parser.PeekToken(TokenKind.TablesKeyword))
            {
                result = true;
                node = new TablesNode();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // TODO: tables


                node.EndIndex = parser.Token.Span.End;
            }    

            return result;
        }
    }
}
