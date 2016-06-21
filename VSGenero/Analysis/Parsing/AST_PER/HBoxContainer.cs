using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class HBoxContainer : AstNodePer
    {
        public TokenExpressionNode Identifier { get; private set; }

        public static bool TryParseNode(IParser parser, out HBoxContainer node)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.FormKeyword))
            {
                node = new HBoxContainer();
                result = true;
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                if(parser.PeekToken(TokenCategory.Identifier) || 
                    (parser.PeekToken(TokenCategory.Keyword) && !IsNextTokenAContainer(parser)))
                {
                    node.Identifier = new TokenExpressionNode(parser.PeekTokenWithSpan());
                    parser.NextToken();
                }

                if(parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();


                    if(parser.PeekToken(TokenKind.RightParenthesis))
                    {
                        parser.NextToken();
                    }
                    else
                    {
                        parser.ReportSyntaxError("Expected right-paren.");
                    }
                }
            }
            return result;
        }

        private static LayoutContainerType[] CanHoldTypes { get; } = new LayoutContainerType[]
        {
            LayoutContainerType.VBox,
            LayoutContainerType.HBox,
            LayoutContainerType.Group,
            LayoutContainerType.Folder,
            LayoutContainerType.Grid,
            LayoutContainerType.ScrollGrid,
            LayoutContainerType.Stack,
            LayoutContainerType.Table,
            LayoutContainerType.Tree
        };

        private static bool IsNextTokenAContainer(IParser parser)
        {
            LayoutContainerType containerType;
            var kind = parser.PeekToken().Kind;
            return LayoutContainerHelpers.TokenToContainerMapping.TryGetValue(kind, out containerType) &&
                   CanHoldTypes.Contains(containerType);
        }
    }

    //public class Hbox
}
