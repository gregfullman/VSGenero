using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class FglNameExpression : NameExpression
    {
        public static bool TryParseNode(IParser parser, out FglNameExpression node, TokenKind breakToken = TokenKind.EndOfFile)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
            {
                result = true;
                node = new FglNameExpression();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                node._firstPiece = parser.Token.Token.Value.ToString();
                StringBuilder sb = new StringBuilder(node._firstPiece);
                node.EndIndex = parser.Token.Span.End;
                while (true)
                {
                    if (breakToken != TokenKind.EndOfFile &&
                       parser.PeekToken(breakToken))
                    {
                        break;
                    }

                    MemberAccessNameExpressionPiece memberAccess;
                    ArrayIndexFglNameExpressionPiece arrayIndex;
                    if (MemberAccessNameExpressionPiece.TryParse(parser, out memberAccess) && memberAccess != null)
                    {
                        node.Children.Add(memberAccess.StartIndex, memberAccess);
                        node.EndIndex = memberAccess.EndIndex;
                        node.IsComplete = true;
                        sb.Append(memberAccess.ToString());
                    }
                    else if (ArrayIndexFglNameExpressionPiece.TryParse(parser, out arrayIndex, breakToken) && arrayIndex != null)
                    {
                        node.Children.Add(arrayIndex.StartIndex, arrayIndex);
                        node.EndIndex = arrayIndex.EndIndex;
                        node.IsComplete = true;
                        sb.Append(arrayIndex.ToString());
                    }
                    else if (parser.PeekToken(TokenKind.AtSymbol))
                    {
                        parser.NextToken();
                        sb.Append("@");
                        if (parser.PeekToken(TokenCategory.Identifier) ||
                           parser.PeekToken(TokenCategory.Keyword))
                        {
                            sb.Append(parser.NextToken().Value.ToString());
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                node.Name = sb.ToString();
            }

            return result;
        }

        public override void CheckForErrors(GeneroAst ast, Action<string, int, int> errorFunc, 
                                            Dictionary<string, List<int>> deferredFunctionSearches, 
                                            FunctionProviderSearchMode searchInFunctionProvider = FunctionProviderSearchMode.NoSearch, 
                                            bool isFunctionCallOrDefinition = false)
        {
            // Check to see if the _firstPiece exists
            IGeneroProject proj;
            IProjectEntry projEntry;
            string searchStr = _firstPiece;
            if (searchInFunctionProvider != FunctionProviderSearchMode.NoSearch ||
                isFunctionCallOrDefinition)
            {
                StringBuilder sb = new StringBuilder(searchStr);
                foreach (var child in Children.Values)
                {
                    if (child is ArrayIndexFglNameExpressionPiece)
                        sb.Append(child.ToString());
                    else if (child is MemberAccessNameExpressionPiece)
                    {
                        if ((child as MemberAccessNameExpressionPiece).Text != ".*")
                            sb.Append(child.ToString());
                        else
                            break;
                    }
                }
                searchStr = sb.ToString();
            }

            bool isDeferred;
            // TODO: need to defer database lookups too
            var res = ast.GetValueByIndex(searchStr,
                                          StartIndex,
                                          ast._functionProvider,
                                          ast._databaseProvider,
                                          ast._programFileProvider,
                                          isFunctionCallOrDefinition,
                                          out isDeferred,
                                          out proj,
                                          out projEntry,
                                          searchInFunctionProvider);
            if (res == null)
            {
                if (isDeferred)
                {
                    if (deferredFunctionSearches.ContainsKey(searchStr))
                        deferredFunctionSearches[searchStr].Add(StartIndex);
                    else
                        deferredFunctionSearches.Add(searchStr, new List<int> { StartIndex });
                }
                else
                {
                    errorFunc(string.Format("No definition found for {0}", searchStr), StartIndex, StartIndex + searchStr.Length);
                }
            }
            else
            {
                if (Name.EndsWith(".*") && res is VariableDef && (res as VariableDef).ResolvedType == null)
                {
                    // need to make sure that the res has a resolved type
                    (res as VariableDef).Type.CheckForErrors(ast, errorFunc, deferredFunctionSearches);
                    //(res as VariableDef).ResolvedType = (res as VariableDef).Type.ResolvedType;
                }
                // TODO: need to check array element type
                ResolvedResult = res;
            }

            base.CheckForErrors(ast, errorFunc, deferredFunctionSearches, searchInFunctionProvider, isFunctionCallOrDefinition);
        }
    }

    public class ArrayIndexFglNameExpressionPiece : AstNode
    {
        private ExpressionNode _expression;

        public override string ToString()
        {
            if (_expression == null)
                return "[]";
            return string.Format("[{0}]", _expression.ToString());
        }

        public static bool TryParse(IParser parser, out ArrayIndexFglNameExpressionPiece node, TokenKind breakToken = TokenKind.EndOfFile)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.LeftBracket))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                result = true;
                node = new ArrayIndexFglNameExpressionPiece();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;

                // TODO: need to get an integer expression
                // for right now, we'll just check for a constant or a ident/keyword
                ExpressionNode indexExpr;
                while (FglExpressionNode.TryGetExpressionNode(parser, out indexExpr, new List<TokenKind> { TokenKind.RightBracket, TokenKind.Comma }))
                {
                    if (node._expression == null)
                        node._expression = indexExpr;
                    else
                        node._expression.AppendExpression(indexExpr);

                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();
                    else
                        break;
                }

                //if(parser.PeekToken(TokenCategory.NumericLiteral) ||
                //   parser.PeekToken(TokenCategory.Keyword) ||
                //   parser.PeekToken(TokenCategory.Identifier))
                //{
                //    parser.NextToken();
                //    sb.Append(parser.Token.Token.Value.ToString());
                //}
                //else
                //{
                //    parser.ReportSyntaxError("The parser is unable to parse a complex expression as an array index. This may not be a syntax error.");
                //}

                //// TODO: check for a nested array index access
                //ArrayIndexFglNameExpressionPiece arrayIndex;
                //if (ArrayIndexFglNameExpressionPiece.TryParse(parser, out arrayIndex, breakToken))
                //{
                //    sb.Append(arrayIndex._expression);
                //}

                //while(!parser.PeekToken(TokenKind.RightBracket))
                //{
                //    if(parser.PeekToken().Kind == breakToken)
                //    {
                //        parser.ReportSyntaxError("Unexpected end of array index expression.");
                //        break;
                //    }
                //    parser.NextToken();
                //    sb.Append(parser.Token.Token.Value.ToString());
                //}

                if (parser.PeekToken(TokenKind.RightBracket))
                {
                    parser.NextToken();
                    node.EndIndex = parser.Token.Span.End;
                    node.IsComplete = true;
                }
                else
                    parser.ReportSyntaxError("Expected right-bracket in array index.");
            }

            return result;
        }
    }
}
