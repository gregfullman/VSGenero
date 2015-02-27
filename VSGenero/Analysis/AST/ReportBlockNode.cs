using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// [PUBLIC|PRIVATE] REPORT report-name (argument-list)
    ///  [ define-section ]
    ///  [ output-section ]
    ///  [  sort-section ]
    ///  [ format-section ] 
    /// END REPORT
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_reports_Report_Definition.html
    /// </summary>
    public class ReportBlockNode : FunctionBlockNode
    {
        public static bool TryParseNode(Parser parser, out ReportBlockNode defNode)
        {
            defNode = null;
            bool result = false;
            AccessModifier? accMod = null;
            string accModToken = null;

            if (parser.PeekToken(TokenKind.PublicKeyword))
            {
                accMod = AccessModifier.Public;
                accModToken = parser.PeekToken().Value.ToString();
            }
            else if (parser.PeekToken(TokenKind.PrivateKeyword))
            {
                accMod = AccessModifier.Private;
                accModToken = parser.PeekToken().Value.ToString();
            }

            uint lookAheadBy = (uint)(accMod.HasValue ? 2 : 1);
            if (parser.PeekToken(TokenKind.FunctionKeyword, lookAheadBy))
            {
                result = true;
                defNode = new ReportBlockNode();
                if (accMod.HasValue)
                {
                    parser.NextToken();
                    defNode.AccessModifier = accMod.Value;
                }
                else
                {
                    defNode.AccessModifier = AccessModifier.Public;
                }

                parser.NextToken(); // move past the Function keyword
                defNode.StartIndex = parser.Token.Span.Start;

                // get the name
                if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    defNode.Name = parser.Token.Token.Value.ToString();
                }
                else
                {
                    parser.ReportSyntaxError("A function must have a name.");
                }

                if (!parser.PeekToken(TokenKind.LeftParenthesis))
                    parser.ReportSyntaxError("A function must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                // get the parameters
                while (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    defNode.Arguments.Add(parser.Token.Token.Value.ToString());
                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();

                    // TODO: probably need to handle "end" "function" case...won't right now
                }

                if (!parser.PeekToken(TokenKind.RightParenthesis))
                    parser.ReportSyntaxError("A function must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                List<TokenKind> breakSequence = new List<TokenKind>() { TokenKind.EndKeyword, TokenKind.FunctionKeyword };
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
                    else
                    {
                        // TODO: not sure whether to break or keep going...for right now, let's keep going until we hit the end keyword
                        parser.NextToken();
                    }
                }

                if (!parser.PeekToken(TokenKind.EndOfFile))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.FunctionKeyword))
                    {
                        parser.NextToken();
                        defNode.EndIndex = parser.Token.Span.End;
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of report definition.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Unexpected end of report definition");
                }
            }
            return result;
        }
    }
}
