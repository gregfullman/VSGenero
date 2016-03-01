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
    public class ReturnStatement : FglStatement
    {
        private List<ExpressionNode> _returns;
        public List<ExpressionNode> Returns
        {
            get
            {
                if (_returns == null)
                    _returns = new List<ExpressionNode>();
                return _returns;
            }
        }

        public static bool TryParseNode(Genero4glParser parser, out ReturnStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.ReturnKeyword))
            {
                parser.NextToken();
                result = true;
                node = new ReturnStatement();
                node.StartIndex = parser.Token.Span.Start;

                while (true)
                {
                    // TODO: not sure about this....it was in here for a reason, right?
                    var tok = parser.PeekToken();
                    if (Genero4glAst.ValidStatementKeywords.Contains(tok.Kind) &&
                        !Genero4glAst.Acceptable_ReturnVariableName_StatementKeywords.Contains(tok.Kind))
                    {
                        // TODO: need to check and see if there are any variables defined with the same name as the statement keyword?
                        break;
                    }

                    ExpressionNode expr;
                    if (!FglExpressionNode.TryGetExpressionNode(parser, out expr ))
                    {
                        break;
                    }
                    node.Returns.Add(expr);

                    if (!parser.PeekToken(TokenKind.Comma))
                    {
                        break;
                    }
                    parser.NextToken();
                }
            }

            return result;
        }
    }
}
