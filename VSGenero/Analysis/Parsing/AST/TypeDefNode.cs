/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/ 

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// [PUBLIC|PRIVATE] TYPE <see cref="TypeDefinitionNode"/> [,...]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_user_types_003.html
    /// </summary>
    public class TypeDefNode : AstNode
    {
        public AccessModifier AccessModifier { get; private set; }
        // TODO: instead of string, this should be the token
        public string AccessModifierToken { get; private set; }

        public IEnumerable<TypeDefinitionNode> GetDefinitions()
        {
            return Children.Where(x => x.Value is TypeDefinitionNode)
                           .Select(x => x.Value as TypeDefinitionNode);
        }

        public static bool TryParseNode(IParser parser, out TypeDefNode defNode, out bool matchedBreakSequence, List<List<TokenKind>> breakSequences = null)
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
            if (parser.PeekToken(TokenKind.TypeKeyword, lookAheadBy))
            {
                result = true;
                defNode = new TypeDefNode();
                if (accMod.HasValue)
                {
                    parser.NextToken();
                    defNode.AccessModifier = accMod.Value;
                }
                else
                {
                    defNode.AccessModifier = AccessModifier.Public;
                }

                parser.NextToken(); // move past the Type keyword
                defNode.StartIndex = parser.Token.Span.Start;

                TypeDefinitionNode constDef;
                while (true)
                {
                    if (TypeDefinitionNode.TryParseDefine(parser, out constDef, defNode.AccessModifier == AccessModifier.Public) && constDef != null)
                    {
                        defNode.Children.Add(constDef.StartIndex, constDef);
                    }
                    else
                    {
                        break;
                    }

                    if (parser.PeekToken(TokenKind.Comma))
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

                if(defNode.Children.Count > 0 && defNode.Children.All(x => x.Value.IsComplete))
                {
                    defNode.EndIndex = defNode.Children.Last().Value.EndIndex;
                    defNode.IsComplete = true;
                }
            }
            return result;
        }
    }
}
