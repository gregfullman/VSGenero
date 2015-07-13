using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class PauseStatement : FglStatement
    {
        public static bool TryParseNode(IParser parser, out PauseStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.PauseKeyword))
            {
                result = true;
                node = new PauseStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if (parser.PeekToken(TokenCategory.StringLiteral))
                    parser.NextToken();

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
