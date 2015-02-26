using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// 
    /// identifier [ datatype] = literal
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Constants_003.html
    /// </summary>
    public class ConstantDefinitionNode : AstNode
    {
        public string Identifier { get; private set; }
        public string SpecifiedType { get; private set; }
        public string Literal { get; private set; }

        public static bool TryParseNode(Parser parser, out ConstantDefinitionNode defNode)
        {
            defNode = null;
            bool result = false;
            // parse constant definition
            if(parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
            {
                defNode = new ConstantDefinitionNode();
                result = true;
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode.Identifier = parser.Token.Token.Value.ToString();

                if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                {
                    parser.NextToken();
                    defNode.SpecifiedType = parser.Token.Token.Value.ToString();
                }

                if(!parser.PeekToken(TokenKind.Equals) && !(parser.PeekToken(2) is ConstantValueToken))
                {
                    parser.ReportSyntaxError("A constant must be defined with a value.");
                }
                else
                {
                    parser.NextToken(); // advance to equals
                    parser.NextToken(); // advance to value
                    defNode.Literal = parser.Token.Token.Value.ToString();
                }
            }
            return result;
        }
    }
}
