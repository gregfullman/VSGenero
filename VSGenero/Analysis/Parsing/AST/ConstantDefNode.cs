using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// 
    /// [PRIVATE|PUBLIC] CONSTANT <see cref="ConstantDefinitionNode"/> [,...]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Constants_003.html
    /// </summary>
    public class ConstantDefNode : AstNode
    {
        public AccessModifier AccessModifier { get; private set; }
        // TODO: instead of string, this should be the token
        public string AccessModifierToken { get; private set; }

        public IEnumerable<ConstantDefinitionNode> GetDefinitions()
        {
            return Children.Where(x => x.Value is ConstantDefinitionNode)
                           .Select(x => x.Value as ConstantDefinitionNode);
        }

        public static bool TryParseNode(IParser parser, out ConstantDefNode defNode, out bool matchedBreakSequence, List<List<TokenKind>> breakSequences = null)
        {
            defNode = null;
            bool result = false;
            matchedBreakSequence = false;
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
            if (parser.PeekToken(TokenKind.ConstantKeyword, lookAheadBy))
            {
                result = true;
                defNode = new ConstantDefNode();
                if (accMod.HasValue)
                {
                    parser.NextToken();
                    defNode.AccessModifier = accMod.Value;
                }
                else
                {
                    defNode.AccessModifier = AccessModifier.Public;
                }

                parser.NextToken(); // move past the Constant keyword
                defNode.StartIndex = parser.Token.Span.Start;

                ConstantDefinitionNode constDef;
                while (true)
                {
                    if (ConstantDefinitionNode.TryParseNode(parser, out constDef, defNode.AccessModifier == AccessModifier.Public))
                    {
                        defNode.Children.Add(constDef.StartIndex, constDef);
                    }
                    else
                    {
                        break;
                    }

                    if(parser.PeekToken(TokenKind.Comma))
                    {
                        parser.NextToken();
                    }

                    if (breakSequences != null)
                    {
                        bool matchedBreak = false;
                        foreach (var seq in breakSequences)
                        {
                            bool bsMatch = true;
                            uint peekaheadCount = 1;
                            foreach (var kind in seq)
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
                            {
                                matchedBreak = true;
                                break;
                            }
                        }

                        if (matchedBreak)
                        {
                            matchedBreakSequence = true;
                            break;
                        }
                    }
                }

                if(defNode.Children.Count > 0 &&
                   defNode.Children.All(x => x.Value.IsComplete))
                {
                    defNode.IsComplete = true;
                }
            }
            return result;
        }
    }
}
