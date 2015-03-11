using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class AttributeSpecifier : AstNode
    {
        private List<Token> _specifierTokens;
        public List<Token> SpecifierTokens
        {
            get
            {
                if(_specifierTokens == null)
                    _specifierTokens = new List<Token>();
                return _specifierTokens;
            }
        }

        public static bool TryParseNode(Parser parser, out AttributeSpecifier node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.AttributeKeyword))
            {
                result = true;
                node = new AttributeSpecifier();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if(!parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.ReportSyntaxError("Invalid attribute specifier found.");
                }
                else
                {
                    while (!parser.PeekToken(TokenKind.RightParenthesis))
                    {
                        node.SpecifierTokens.Add(parser.NextToken());
                        if(parser.PeekToken(TokenKind.EndOfFile))
                        {
                            parser.ReportSyntaxError("Unexpected end of attribute specifier.");
                        }
                    }
                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }
    }
}
