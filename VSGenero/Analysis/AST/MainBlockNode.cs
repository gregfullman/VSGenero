using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// MAIN
    ///     [ <see cref="DefineNode"/>
    ///     | <see cref="ConstantDefNode"/>
    ///     | <see cref="TypeDefNode"/>
    ///     ]
    ///     { [<see cref="DeferStatementNode"/>]
    ///     | fgl-statement
    ///     | sql-statement
    ///     }
    ///     [...]
    /// END MAIN
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_MAIN.html
    /// </summary>
    public class MainBlockNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out MainBlockNode defNode)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.MainKeyword))
            {
                result = true;
                defNode = new MainBlockNode();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                List<TokenKind> breakSequence = new List<TokenKind>() { TokenKind.EndKeyword, TokenKind.MainKeyword };
                // try to parse one or more declaration statements
                while (!parser.PeekToken(TokenKind.EndKeyword) &&
                      !parser.PeekToken(TokenKind.EndOfFile))
                {
                    DefineNode defineNode;
                    TypeDefNode typeNode;
                    ConstantDefNode constNode;
                    if (DefineNode.TryParseDefine(parser, out defineNode, breakSequence))
                    {
                        defNode.Children.Add(defineNode.StartIndex, defineNode);
                    }
                    else if (TypeDefNode.TryParseNode(parser, out typeNode))
                    {
                        defNode.Children.Add(defineNode.StartIndex, typeNode);
                    }
                    else if (ConstantDefNode.TryParseNode(parser, out constNode))
                    {
                        defNode.Children.Add(defineNode.StartIndex, constNode);
                    }
                }

                if (!parser.PeekToken(TokenKind.EndOfFile))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.MainKeyword))
                    {
                        parser.NextToken();
                        defNode.EndIndex = parser.Token.Span.End;
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of main definition.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Unexpected end of main definition");
                }
            }

            return result;
        }
    }
}
