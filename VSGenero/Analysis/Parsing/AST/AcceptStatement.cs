using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class AcceptStatement : FglStatement
    {
        public TokenKind AcceptType { get; private set; }

        public static bool TryParseNode(Parser parser, out AcceptStatement node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.AcceptKeyword))
            {
                result = true;
                node = new AcceptStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                TokenKind tokKind = parser.PeekToken().Kind;
                switch (tokKind)
                {
                    case TokenKind.ConstructKeyword:
                    case TokenKind.InputKeyword:
                    case TokenKind.DialogKeyword:
                    case TokenKind.DisplayKeyword:
                        node.AcceptType = tokKind;
                        parser.NextToken();
                        node.EndIndex = parser.Token.Span.End;
                        break;
                    default:
                        parser.ReportSyntaxError("Accept statement must be of form: accept { CONSTRUCT | INPUT | DIALOG | DISPLAY }");
                        break;
                }
            }

            return result;
        }
    }
}
