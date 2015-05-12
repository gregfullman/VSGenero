using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// OPTIONS
    /// { SHORT CIRCUIT
    /// } [,...]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_OPTIONS_compiler.html
    /// </summary>
    public class CompilerOptionsNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out CompilerOptionsNode defNode)
        {
            defNode = null;
            if(parser.PeekToken(TokenKind.OptionsKeyword))
            {
                parser.NextToken();
                defNode = new CompilerOptionsNode();
                defNode.StartIndex = parser.Token.Span.Start;

                // read options
                if (parser.PeekToken(TokenKind.ShortKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.CircuitKeyword))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("The only compiler options are: short circuit");
                }
                else
                {
                    parser.ReportSyntaxError("The only compiler options are: short circuit");
                }

                defNode.EndIndex = parser.Token.Span.End;
                defNode.IsComplete = true;

                return true;
            }
            return false;
        }
    }
}
