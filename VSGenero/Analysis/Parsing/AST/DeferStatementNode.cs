using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// DEFER { INTERRUPT | QUIT }
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_DEFER.html
    /// </summary>
    public class DeferStatementNode : FglStatement
    {
        public static bool TryParseNode(Parser parser, out DeferStatementNode defNode)
        {
            defNode = null;
            bool result = false;
            
            if(parser.PeekToken(TokenKind.DeferKeyword))
            {
                result = true;
                defNode = new DeferStatementNode();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                if(parser.PeekToken(TokenKind.InterruptKeyword))
                {
                    parser.NextToken();
                }
                else if(parser.PeekToken(TokenKind.QuitKeyword))
                {
                    parser.NextToken();
                }
                else
                {
                    parser.ReportSyntaxError("Invalid token found in defer statement.");
                }
                defNode.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
