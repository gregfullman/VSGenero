using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class FunctionTypeReference : TypeReference
    {
        public Dictionary<string, TypeReference> Arguments { get; private set; }
        public List<TypeReference> Returns { get; private set; }

        public static bool TryParseNode(IParser parser, out FunctionTypeReference defNode)
        {
            defNode = null;
            bool result = false;
            if (parser.PeekToken(TokenKind.FunctionKeyword))
            {
                result = true;
                defNode = new FunctionTypeReference();
                defNode.Arguments = new Dictionary<string, TypeReference>();
                defNode.Returns = new List<TypeReference>();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._location = parser.TokenLocation;

                // TODO: not sure if a function name is allowable...
                if (parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();
                    while (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                    {
                        var paramName = parser.PeekTokenWithSpan();
                        VariableDefinitionNode varDefNode = null;
                        if (VariableDefinitionNode.TryParseNode(parser, out varDefNode, null, false, false))
                        {
                            // There should only be one vardef since we're not allowing compact definition
                            foreach(var vardef in varDefNode.VariableDefinitions)
                                defNode.Arguments.Add(vardef.Name, vardef.Type);
                        }
                        else
                        {
                            parser.ReportSyntaxError("Failed getting argument type.");
                        }
                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                    }

                    if(parser.PeekToken(TokenKind.RightParenthesis))
                    {
                        parser.NextToken();
                        ReturnsStatement returnsStatement;
                        if (ReturnsStatement.TryParseNode(parser, out returnsStatement))
                        {
                            foreach (var ret in returnsStatement.ReturnTypes)
                                defNode.Returns.Add(ret);
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Missing right paren in function type signature.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("A function name cannot exist in a function type specifier");
                }
            }

            return result;
        }
    }
}
