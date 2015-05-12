using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class FreeStatement : FglStatement
    {
        public NameExpression Target { get; private set; }

        public static bool TryParseNode(Parser parser, out FreeStatement defNode)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.FreeKeyword))
            {
                result = true;
                defNode = new FreeStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                NameExpression expr;
                if(!NameExpression.TryParseNode(parser, out expr))
                {
                    parser.ReportSyntaxError("Invalid name found in free statement.");
                }
                else
                {
                    defNode.Target = expr;
                }

                defNode.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
