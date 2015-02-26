using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// Format:
    /// identifier [,...] <see cref="TypeReference"/> [<see cref="AttributeSpecifier"/>]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_DEFINE.html
    /// </summary>
    public class VariableDefinitionNode : AstNode
    {
        private HashSet<string> _identifiers;
        public HashSet<string> Identifiers
        {
            get
            {
                if(_identifiers == null)
                {
                    _identifiers = new HashSet<string>();
                }
                return _identifiers;
            }
            set
            {
                _identifiers = value;
            }
        }

        public static bool TryParseNode(Parser parser, out VariableDefinitionNode defNode)
        {
            defNode = null;
            bool result = false;
            uint peekaheadCount = 1;
            List<string> identifiers = new List<string>();

            var tok = parser.PeekToken(peekaheadCount);
            var cat = Tokenizer.GetTokenInfo(tok).Category;
            while (cat == TokenCategory.Identifier || cat == TokenCategory.Keyword)
            {
                identifiers.Add(tok.Value.ToString());
                peekaheadCount++;
                if (!parser.PeekToken(TokenKind.Comma, peekaheadCount))
                    break;
                peekaheadCount++;
                tok = parser.PeekToken(peekaheadCount);
                cat = Tokenizer.GetTokenInfo(tok).Category;
            }

            if (identifiers.Count > 0)
            {
                result = true;
                defNode = new VariableDefinitionNode();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode.Identifiers = new HashSet<string>(identifiers);
                while (peekaheadCount > 1)
                {
                    parser.NextToken();
                    peekaheadCount--;
                }

                TypeReference typeRef;
                if(TypeReference.TryParseNode(parser, out typeRef))
                {
                    defNode.EndIndex = typeRef.EndIndex;
                    defNode.Children.Add(typeRef.StartIndex, typeRef);
                }
                else
                {
                    parser.ReportSyntaxError("No type defined for variable(s).");
                    result = false;
                }
            }

            return result;
        }
    }
}
