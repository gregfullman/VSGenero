using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class ReturnsStatement : FglStatement
    {
        public List<TypeReference> ReturnTypes { get; private set;}

        public static bool TryParseNode(IParser parser, out ReturnsStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.ReturnsKeyword))
            {
                parser.NextToken();
                result = true;
                node = new ReturnsStatement();

                bool multiReturn = false;
                if(parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    multiReturn = true;
                    parser.NextToken();
                }

                node.ReturnTypes = new List<TypeReference>();
                while (true)
                {
                    TypeReference typeRef;
                    if (TypeReference.TryParseNode(parser, out typeRef) && typeRef != null)
                    {
                        node.ReturnTypes.Add(typeRef);
                    }
                    if (!multiReturn)
                        break;
                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }

                if(multiReturn || node.ReturnTypes.Count > 1)
                {
                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Right paren required when defining multiple return types.");
                }
            }

            return result;
        }
    }
}
