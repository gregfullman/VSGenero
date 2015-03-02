using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    public class PreprocessorNode : AstNode
    {
        private List<Token> _preprocessorTokens;
        public List<Token> PreprocessorTokens
        {
            get
            {
                if (_preprocessorTokens == null)
                    _preprocessorTokens = new List<Token>();
                return _preprocessorTokens;
            }
        }

        public static bool TryParseNode(Parser parser, out PreprocessorNode node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.Ampersand))
            {
                parser.NextToken();
                result = true;
                node = new PreprocessorNode();
                node.StartIndex = parser.Token.Span.Start;
                StringBuilder sb = new StringBuilder();
                
                while(!parser.PeekToken(TokenKind.NewLine) && !parser.PeekToken(TokenKind.EndOfFile))
                {
                    node.PreprocessorTokens.Add(parser.NextToken());
                }
                if (parser.PeekToken(TokenKind.NewLine))
                    parser.NextToken();
            }

            return result;
        }
    }
}
