using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class ScreenNode : AstNodePer
    {
        // default height is 24 characters
        private int _lines = 24;
        public int Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }

        public int Chars { get; private set; }

        public string Title { get; private set; }

        public static bool TryParseNode(IParser parser, out ScreenNode node)
        {
            bool result = false;
            node = null;

            if (parser.PeekToken(TokenKind.ScreenKeyword))
            {
                result = true;
                node = new ScreenNode();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if(parser.PeekToken(TokenKind.SizeKeyword))
                {
                    parser.NextToken();
                    var linesVal = parser.NextToken().Value.ToString();
                    int tempLines;
                    if(int.TryParse(linesVal, out tempLines))
                    {
                        node.Lines = tempLines;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid value found for screen height.");
                    }

                    if(parser.PeekToken(TokenKind.ByKeyword))
                    {
                        parser.NextToken();
                        var charsVal = parser.NextToken().Value.ToString();
                        if (int.TryParse(linesVal, out tempLines))
                        {
                            node.Chars = tempLines;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Invalid value found for screen width.");
                        }
                    }
                }

                if(parser.PeekToken(TokenKind.TitleKeyword))
                {
                    parser.NextToken();
                    StringExpressionNode strExpr;
                    if(StringExpressionNode.TryGetExpressionNode(parser, out strExpr))
                    {
                        node.Title = strExpr.LiteralValue;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid screen title found.");
                    }
                }
                
                if(parser.PeekToken(TokenKind.LeftBrace))
                {
                    parser.NextToken();

                    // TODO: screen content;
                    // TODO: also check for other keywords that might appear if the screen is not complete.
                    while (!parser.PeekToken(TokenKind.RightBrace))
                        parser.NextToken();

                    if(parser.PeekToken(TokenKind.RightBrace))
                    {
                        parser.ReportSyntaxError("Expected right-brace.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Expected left-brace.");
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
