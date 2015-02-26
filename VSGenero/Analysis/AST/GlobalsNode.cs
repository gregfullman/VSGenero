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
    /// Syntax 1:
    /// GLOBALS
    ///     declaration-statement (variable, constant, or type definition)
    ///     [,...]
    /// END GLOBALS
    /// 
    /// or
    /// 
    /// Syntax 2:
    /// GLOBALS "filename"
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Globals_003.html
    /// </summary>
    public class GlobalsNode : AstNode
    {
        public string GlobalsFilename { get; private set; }

        public static bool TryParseNode(Parser parser, out GlobalsNode defNode)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.GlobalsKeyword))
            {
                result = true;
                parser.NextToken();
                defNode = new GlobalsNode();
                defNode.StartIndex = parser.Token.Span.Start;

                var tok = parser.PeekToken();
                if(Tokenizer.GetTokenInfo(tok).Category == TokenCategory.StringLiteral)
                {
                    parser.NextToken();
                    defNode.GlobalsFilename = parser.Token.Token.Value.ToString();
                }
                else
                {
                    List<TokenKind> breakSequence = new List<TokenKind>() { TokenKind.EndKeyword, TokenKind.GlobalsKeyword };
                    // try to parse one or more declaration statements
                    while(!parser.PeekToken(TokenKind.EndKeyword) &&
                          !parser.PeekToken(TokenKind.EndOfFile))
                    {
                        DefineNode defineNode;
                        TypeDefNode typeNode;
                        ConstantDefNode constNode;
                        if (DefineNode.TryParseDefine(parser, out defineNode, breakSequence))
                        {
                            defNode.Children.Add(defineNode.StartIndex, defineNode);
                        }
                        else if(TypeDefNode.TryParseNode(parser, out typeNode))
                        {
                            defNode.Children.Add(defineNode.StartIndex, typeNode);
                        }
                        else if(ConstantDefNode.TryParseNode(parser, out constNode))
                        {
                            defNode.Children.Add(defineNode.StartIndex, constNode);
                        }
                    }

                    if (!parser.PeekToken(TokenKind.EndOfFile))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.GlobalsKeyword))
                        {
                            parser.NextToken();
                            defNode.EndIndex = parser.Token.Span.End;
                        }
                        else
                        {
                            parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of globals definition.");
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Unexpected end of globals definition");
                    }
                }
            }

            return result;
        }
    }
}
