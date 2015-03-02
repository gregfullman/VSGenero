using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// This class encapsulates the logic for variables definitions
    /// [PUBLIC|PRIVATE] DEFINE 
    /// {
    ///     <see cref="VariableDefinitionNode"/> |
    ///     <see cref="RecordDefinitionNode"/>
    /// }[,...]
    /// 
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_DEFINE.html
    /// </summary>
    public class DefineNode : AstNode
    {
        public AccessModifier AccessModifier { get; private set; }
        // TODO: instead of string, this should be the token
        public string AccessModifierToken { get; private set; }

        public static bool TryParseDefine(Parser parser, out DefineNode defNode, List<TokenKind> breakSequence = null)
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
            if (parser.PeekToken(TokenKind.DefineKeyword, lookAheadBy))
            {
                result = true;
                defNode = new DefineNode();
                if (accMod.HasValue)
                {
                    parser.NextToken();
                    defNode.AccessModifier = accMod.Value;
                }
                else
                {
                    defNode.AccessModifier = AccessModifier.Public;
                }

                parser.NextToken(); // move past the Define keyword
                defNode.StartIndex = parser.Token.Span.Start;

                if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    bool tryAgain = true;
                    do
                    {
                        RecordDefinitionNode recDef;
                        if (RecordDefinitionNode.TryParseNode(parser, out recDef))
                        {
                            defNode.Children.Add(recDef.StartIndex, recDef);
                        }
                        else
                        {
                            VariableDefinitionNode varDef;
                            if (VariableDefinitionNode.TryParseNode(parser, out varDef))
                            {
                                defNode.Children.Add(varDef.StartIndex, varDef);
                            }
                            else
                            {
                                tryAgain = false;
                            }
                        }

                        if (tryAgain)
                        {
                            if (breakSequence != null)
                            {
                                bool bsMatch = true;
                                uint peekaheadCount = 1;
                                foreach (var kind in breakSequence)
                                {
                                    if (parser.PeekToken(kind, peekaheadCount))
                                    {
                                        peekaheadCount++;
                                    }
                                    else
                                    {
                                        bsMatch = false;
                                        break;
                                    }
                                }

                                if (bsMatch)
                                    tryAgain = false;
                            }

                            if (tryAgain)
                            {
                                if (parser.PeekToken(TokenKind.Comma))
                                {
                                    parser.NextToken();
                                    continue;
                                }
                                else
                                {
                                    tryAgain = false;
                                }
                            }

                            
                        }
                    }
                    while (tryAgain);

                    if (defNode.Children.Count > 0)
                    {
                        defNode.EndIndex = parser.Token.Span.End;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Nothing defined in define block");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid token following define.");
                }
            }

            return result;
        }
    }
}
