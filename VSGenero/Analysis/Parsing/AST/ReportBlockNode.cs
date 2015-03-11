using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
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
            if (parser.PeekToken(TokenKind.ReportKeyword, lookAheadBy))
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
                    defNode.DecoratorEnd = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError("A report must have a name.");
                }

                if (!parser.PeekToken(TokenKind.LeftParenthesis))
                    parser.ReportSyntaxError("A report must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                // get the parameters
                while (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    string errMsg;
                    if (!defNode.AddArgument(parser.Token, out errMsg))
                    {
                        parser.ReportSyntaxError(errMsg);
                    }
                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();

                    // TODO: probably need to handle "end" "function" case...won't right now
                }

                if (!parser.PeekToken(TokenKind.RightParenthesis))
                    parser.ReportSyntaxError("A report must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                List<List<TokenKind>> breakSequences = new List<List<TokenKind>>() 
                    { 
                        new List<TokenKind> { TokenKind.EndKeyword, TokenKind.ReportKeyword },
                        new List<TokenKind> { TokenKind.ConstantKeyword },
                        new List<TokenKind> { TokenKind.DefineKeyword },
                        new List<TokenKind> { TokenKind.TypeKeyword }
                    };
                // try to parse one or more declaration statements
                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                          !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ReportKeyword, 2)))
                {
                    DefineNode defineNode;
                    TypeDefNode typeNode;
                    ConstantDefNode constNode;
                    bool matchedBreakSequence = false;
                    switch (parser.PeekToken().Kind)
                    {
                        case TokenKind.TypeKeyword:
                            {
                                if (TypeDefNode.TryParseNode(parser, out typeNode, out matchedBreakSequence, breakSequences))
                                {
                                    defNode.Children.Add(typeNode.StartIndex, typeNode);
                                }
                                break;
                            }
                        case TokenKind.ConstantKeyword:
                            {
                                if (ConstantDefNode.TryParseNode(parser, out constNode, out matchedBreakSequence, breakSequences))
                                {
                                    defNode.Children.Add(constNode.StartIndex, constNode);
                                }
                                break;
                            }
                        case TokenKind.DefineKeyword:
                            {
                                if (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences, defNode.BindArgument))
                                {
                                    defNode.Children.Add(defineNode.StartIndex, defineNode);
                                }
                                break;
                            }
                        default:
                            {
                                FglStatement statement;
                                if (parser.StatementFactory.TryParseNode(parser, out statement))
                                {
                                    AstNode stmtNode = statement as AstNode;
                                    defNode.Children.Add(stmtNode.StartIndex, stmtNode);
                                }
                                break;
                            }
                    }
                    // if a break sequence was matched, we don't want to advance the token
                    if (!matchedBreakSequence)
                    {
                        // TODO: not sure whether to break or keep going...for right now, let's keep going until we hit the end keyword
                        parser.NextToken();
                    }
                }

                if (!parser.PeekToken(TokenKind.EndOfFile))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.ReportKeyword))
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
