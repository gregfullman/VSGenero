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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class CallStatement : FglStatement
    {
        public FunctionCallExpressionNode Function { get; private set; }

        private List<NameExpression> _returns;
        public List<NameExpression> Returns
        {
            get
            {
                if (_returns == null)
                    _returns = new List<NameExpression>();
                return _returns;
            }
        }

        public static bool TryParseNode(Genero4glParser parser, out CallStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.CallKeyword))
            {
                result = true;
                node = new CallStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // get the function name
                FunctionCallExpressionNode functionCall;
                NameExpression dummy;
                if (!FunctionCallExpressionNode.TryParseExpression(parser, out functionCall, out dummy, true))
                {
                    parser.ReportSyntaxError("Unexpected token found in call statement, expecting name expression.");
                }
                else
                {
                    node.Function = functionCall;

                    if (parser.PeekToken(TokenKind.ReturningKeyword))
                    {
                        parser.NextToken();

                        NameExpression name;
                        // get return values
                        while (NameExpression.TryParseNode(parser, out name, TokenKind.Comma))
                        {
                            node.Returns.Add(name);
                            if (!parser.PeekToken(TokenKind.Comma))
                                break;
                            parser.NextToken();
                        }

                        if(node.Returns.Count == 0)
                        {
                            parser.ReportSyntaxError("One or more return variables must be specified.");
                        }
                    }
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }

        public override void CheckForErrors(GeneroAst ast, Action<string, int, int> errorFunc,
                                            Dictionary<string, List<int>> deferredFunctionSearches,
                                            FunctionProviderSearchMode searchInFunctionProvider = FunctionProviderSearchMode.NoSearch, bool isFunctionCallOrDefinition = false)
        {
            // 1) Check to make sure the function call is valid
            if(Function != null)
                Function.CheckForErrors(ast, errorFunc, deferredFunctionSearches);

            // 2) Check the return values
            if (Returns != null)
            {
                foreach (var ret in Returns)
                {
                    ret.CheckForErrors(ast, errorFunc, deferredFunctionSearches);
                    if(ret.ResolvedResult != null &&
                       !(ret.ResolvedResult is VariableDef || ret.ResolvedResult is ProgramRegister))
                    {
                        errorFunc(string.Format("Return item {0} is not a variable", ret.Name), ret.StartIndex, ret.EndIndex);
                    }
                }

                if(Function != null &&
                   Function.Function != null &&
                   Function.Function.ResolvedResult != null &&
                   Function.Function.ResolvedResult is IFunctionResult)
                {
                    var numReturns = (Function.Function.ResolvedResult as IFunctionResult).Returns.Length;
                    if(Returns.Count != numReturns)
                    {
                        errorFunc(string.Format("Unexpected number of return variables ({0}) found, expected {1} variables.", Returns.Count, numReturns), StartIndex, EndIndex);
                    }
                }
            }

            base.CheckForErrors(ast, errorFunc, deferredFunctionSearches);
        }
    }
}
