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
    /// This class encapsulates the logic for variables definitions
    /// [PUBLIC|PRIVATE] DEFINE 
    /// {
    ///     <see cref="VariableDefinitionNode"/>
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

        public IEnumerable<VariableDefinitionNode> GetDefinitions()
        {
            return Children.Where(x => x.Value is VariableDefinitionNode)
                           .Select(x => x.Value as VariableDefinitionNode);
        }

        public static bool TryParseDefine(IParser parser, out DefineNode defNode, out bool matchedBreakSequence, List<List<TokenKind>> breakSequences = null, Action<VariableDef> binder = null)
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
                        VariableDefinitionNode varDef;
                        if (VariableDefinitionNode.TryParseNode(parser, out varDef, binder) && varDef != null)
                        {
                            defNode.Children.Add(varDef.StartIndex, varDef);
                        }
                        else
                        {
                            tryAgain = false;
                        }

                        if (tryAgain)
                        {
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
                        if(defNode.Children.All(x => x.Value.IsComplete))
                        {
                            defNode.IsComplete = true;
                        }
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
