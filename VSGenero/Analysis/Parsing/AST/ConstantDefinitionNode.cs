using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// 
    /// identifier [ datatype] = literal
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Constants_003.html
    /// </summary>
    public class ConstantDefinitionNode : AstNode, IAnalysisResult
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

        private string _scope;
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
                _scope = value;
            }
        }

        public string Name
        {
            get { return Identifier; }
        }

        public override string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.Append(Name);
                if(!string.IsNullOrWhiteSpace(SpecifiedType))
                {
                    sb.AppendFormat(" ({0})", SpecifiedType);
                }
                sb.AppendFormat(" = {0}", Literal);
                return sb.ToString();
            }
        }


        public int LocationIndex
        {
            get { return StartIndex; }
        }
    }
}
