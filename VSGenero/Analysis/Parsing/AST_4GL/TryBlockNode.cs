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

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    /// <summary>
    /// TRY
    ///     instruction
    ///     [...]
    /// CATCH
    ///     instruction
    ///     [...]
    /// END TRY
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Exceptions_007.html
    /// </summary>
    public class TryCatchStatement : FglStatement
    {
        public static bool TryParseNode(Genero4glParser parser, out TryCatchStatement defNode,
                                 IModuleResult containingModule,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.TryKeyword))
            {
                result = true;
                defNode = new TryCatchStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode.DecoratorEnd = parser.Token.Span.End;

                HashSet<TokenKind> newEndKeywords = new HashSet<TokenKind>();
                if (endKeywords != null)
                    newEndKeywords.AddRange(endKeywords);
                newEndKeywords.Add(TokenKind.TryKeyword);

                prepStatementBinders.Insert(0, defNode.BindPrepareCursorFromIdentifier);

                TryBlock tryBlock;
                if (TryBlock.TryParseNode(parser, out tryBlock, containingModule, prepStatementBinders, returnStatementBinder,
                                          limitedScopeVariableAdder, validExitKeywords, contextStatementFactories, newEndKeywords) && tryBlock != null)
                {
                    defNode.Children.Add(tryBlock.StartIndex, tryBlock);
                }

                if(parser.PeekToken(TokenKind.CatchKeyword))
                {
                    parser.NextToken();
                    CatchBlock catchBlock;
                    if (CatchBlock.TryParseNode(parser, out catchBlock, containingModule, prepStatementBinders, returnStatementBinder,
                                                limitedScopeVariableAdder, validExitKeywords, contextStatementFactories, newEndKeywords) && catchBlock != null)
                    {
                        defNode.Children.Add(catchBlock.StartIndex, catchBlock);

                        // add the catch block to the additional decorators
                        defNode.AdditionalDecoratorRanges.Add(catchBlock.StartIndex, catchBlock.StartIndex + 5);
                    }
                }

                prepStatementBinders.RemoveAt(0);

                if(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.TryKeyword, 2))
                {
                    parser.NextToken();
                    parser.NextToken();
                }
                else
                {
                    parser.ReportSyntaxError("Invalid end of try-catch block found.");
                }
                defNode.EndIndex = parser.Token.Span.End;
            }

            return result;
        }

        public override bool CanOutline
        {
            get { return true; }
        }

        public override int DecoratorEnd { get; set; }
    }

    public class TryBlock : AstNode4gl
    {
        public static bool TryParseNode(Genero4glParser parser, out TryBlock node,
                                 IModuleResult containingModule,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = new TryBlock();
            node.StartIndex = parser.Token.Span.Start;
            prepStatementBinders.Insert(0, node.BindPrepareCursorFromIdentifier);
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                   !parser.PeekToken(TokenKind.CatchKeyword) &&
                   !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.TryKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinders, returnStatementBinder, 
                                                         limitedScopeVariableAdder, false, validExitKeywords, contextStatementFactories, null, endKeywords) && statement != null)
                {
                    AstNode4gl stmtNode = statement as AstNode4gl;
                    node.Children.Add(stmtNode.StartIndex, stmtNode);
                }
                else if (parser.PeekToken(TokenKind.EndKeyword) && endKeywords != null && endKeywords.Contains(parser.PeekToken(2).Kind))
                {
                    break;
                }
                else
                {
                    parser.NextToken();
                }
            }
            prepStatementBinders.RemoveAt(0);
            node.EndIndex = parser.Token.Span.End;
            return true;
        }
    }

    public class CatchBlock : AstNode4gl
    {
        public static bool TryParseNode(Genero4glParser parser, out CatchBlock node,
                                 IModuleResult containingModule,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null,
                                 HashSet<TokenKind> endKeywords = null)
        {
            node = new CatchBlock();
            node.StartIndex = parser.Token.Span.Start;
            prepStatementBinders.Insert(0, node.BindPrepareCursorFromIdentifier);
            while (!parser.PeekToken(TokenKind.EndOfFile) &&
                   !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.TryKeyword, 2)))
            {
                FglStatement statement;
                if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, prepStatementBinders, 
                                                         returnStatementBinder, limitedScopeVariableAdder, false, validExitKeywords, 
                                                         contextStatementFactories, null, endKeywords) && statement != null)
                {
                    AstNode4gl stmtNode = statement as AstNode4gl;
                    node.Children.Add(stmtNode.StartIndex, stmtNode);
                }
                else if (parser.PeekToken(TokenKind.EndKeyword) && endKeywords != null && endKeywords.Contains(parser.PeekToken(2).Kind))
                {
                    break;
                }
                else
                {
                    parser.NextToken();
                }
            }
            prepStatementBinders.RemoveAt(0);
            node.EndIndex = parser.Token.Span.End;
            return true;
        }
    }
}
