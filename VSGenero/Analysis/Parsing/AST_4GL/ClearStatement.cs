using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class ClearStatement : FglStatement
    {
        public List<FglNameExpression> FieldList { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out ClearStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.ClearKeyword))
            {
                result = true;
                node = new ClearStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.FieldList = new List<FglNameExpression>();

                // get the bynamefields
                FglNameExpression nameExpr;
                while (FglNameExpression.TryParseNode(parser, out nameExpr))
                {
                    node.FieldList.Add(nameExpr);
                    if (!parser.PeekToken(TokenKind.Comma))
                        break;
                    parser.NextToken();
                }

                if(node.FieldList.Count == 0)
                {
                    parser.ReportSyntaxError("Incomplete clear statement found.");
                }
                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
