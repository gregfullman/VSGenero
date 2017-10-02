using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class DictionaryDefinitionNode : TypeReference
    {
        public static bool TryParseNode(IParser parser, out DictionaryDefinitionNode defNode)
        {
            defNode = null;
            bool result = false;
            if (parser.PeekToken(TokenKind.DictionaryKeyword))
            {
                result = true;
                defNode = new DictionaryDefinitionNode();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._location = parser.TokenLocation;

                if (!parser.PeekToken(TokenKind.OfKeyword))
                    parser.ReportSyntaxError("Missing \"of\" keyword in dictionary definition.");
                else
                    parser.NextToken();

                // now try to get the datatype
                TypeReference typeRef;
                if ((TypeReference.TryParseNode(parser, out typeRef) && typeRef != null))
                {
                    defNode.Children.Add(typeRef.StartIndex, typeRef);
                    defNode.EndIndex = typeRef.EndIndex;
                }
                else
                {
                    parser.ReportSyntaxError("Invalid type specified for dictionary definition");
                }
            }
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("dictionary of ");
            if (Children.Count == 1)
            {
                sb.Append(Children[Children.Keys[0]].ToString());
            }
            return sb.ToString();
        }

        internal IEnumerable<IAnalysisResult> GetAnalysisResults(MemberType memberType, GetMemberInput input)
        {
            List<IAnalysisResult> results = new List<IAnalysisResult>();
            if (Children.Count == 1)
            {
                // get the table's columns
                var node = Children[Children.Keys[0]];
                if (node is TypeReference)
                {
                    results.AddRange((node as TypeReference).GetAnalysisMembers(memberType, input));
                }
            }
            results.AddRange(Genero4glAst.DictionaryFunctions.Values.Where(x => input.AST.LanguageVersion >= x.MinimumLanguageVersion && input.AST.LanguageVersion <= x.MaximumLanguageVersion));
            return results;
        }

        internal IEnumerable<MemberResult> GetMembersInternal(GetMultipleMembersInput input)
        {
            List<MemberResult> results = new List<MemberResult>();
            if (input.GetArrayTypeMembers)
            {
                if (Children.Count == 1)
                {
                    var node = Children[Children.Keys[0]];
                    if (node is TypeReference)
                    {
                        var newInput = new GetMultipleMembersInput
                        {
                            AST = input.AST,
                            MemberType = input.MemberType
                            // We don't want to continue getting array type members further down
                        };
                        results.AddRange((node as TypeReference).GetMembers(newInput));
                    }
                }
            }
            else
            {
                results.AddRange(Genero4glAst.DictionaryFunctions.Values.Where(x => input.AST.LanguageVersion >= x.MinimumLanguageVersion && input.AST.LanguageVersion <= x.MaximumLanguageVersion)
                                                                       .Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, input.AST)));
            }

            return results;
        }
    }
}
