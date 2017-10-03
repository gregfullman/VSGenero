using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class FunctionTypeReference : TypeReference, IFunctionResult
    {
        private Dictionary<string, IAnalysisResult> _dummyDict = new Dictionary<string, IAnalysisResult>();
        private Dictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> _dummyLimitDict = new Dictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>>();
        private bool _isPublic;
        private List<string> _orderedArgs;
        public Dictionary<string, TypeReference> Arguments { get; private set; }
        public List<TypeReference> Returns { get; private set; }

        #region IFunctionResult Implementation

        public ParameterResult[] Parameters
        {
            get
            {
                List<ParameterResult> paramList = new List<ParameterResult>();
                for (int i = 0; i < _orderedArgs.Count; i++)
                {
                    var typeRef = Arguments[_orderedArgs[i]];
                    paramList.Add(new ParameterResult(_orderedArgs[i], "", typeRef.ToString()));
                }
                return paramList.ToArray();
            }
        }

        string[] IFunctionResult.Returns
        {
            get
            {
                return Returns.Select(x => x.ToString()).ToArray();
            }
        }

        public AccessModifier AccessModifier => _isPublic ? AccessModifier.Public : AccessModifier.Private;

        public string FunctionDocumentation => _commentDocumentation ?? "";

        public IDictionary<string, IAnalysisResult> Variables => _dummyDict;

        public IDictionary<string, IAnalysisResult> Types => _dummyDict;

        public IDictionary<string, IAnalysisResult> Constants => _dummyDict;

        public IDictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> LimitedScopeVariables => _dummyLimitDict;

        public string CompletionParentName => null;

        public GeneroMemberType FunctionType => GeneroMemberType.Function;

        private string _commentDocumentation;
        public void SetCommentDocumentation(string commentDoc)
        {
            _commentDocumentation = commentDoc;
        }

        #endregion

        public override string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("FUNCTION(");
                for (int i = 0; i < _orderedArgs.Count; i++)
                {
                    var typeRef = Arguments[_orderedArgs[i]];
                    sb.AppendFormat("{0} {1}", _orderedArgs[i], typeRef.ToString());
                    if (i + 1 < _orderedArgs.Count)
                        sb.Append(", ");
                }
                sb.Append(")");

                if (Returns.Count > 0)
                {
                    sb.Append(" RETURNS ");
                    if (Returns.Count > 1)
                        sb.Append("(");

                    for (int i = 0; i < Returns.Count; i++)
                    {
                        sb.Append(Returns[i].ToString());
                        if (i + 1 < Returns.Count)
                            sb.Append(", ");
                    }

                    if (Returns.Count > 1)
                        sb.Append(")");
                }

                return sb.ToString();
            }
        }

        

        public override string ToString()
        {
            return Documentation;
        }

        public override string Name => "FUNCTION";

        public static bool TryParseNode(IParser parser, out FunctionTypeReference defNode, bool isPublic)
        {
            defNode = null;
            bool result = false;
            if (parser.PeekToken(TokenKind.FunctionKeyword))
            {
                result = true;
                defNode = new FunctionTypeReference();
                defNode.Arguments = new Dictionary<string, TypeReference>();
                defNode.Returns = new List<TypeReference>();
                defNode._orderedArgs = new List<string>();
                defNode._isPublic = isPublic;
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._location = parser.TokenLocation;

                // TODO: not sure if a function name is allowable...
                if (parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();
                    while (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                    {
                        VariableDefinitionNode varDefNode = null;
                        if (VariableDefinitionNode.TryParseNode(parser, out varDefNode, null, false, false))
                        {
                            // There should only be one vardef since we're not allowing compact definition
                            foreach (var vardef in varDefNode.VariableDefinitions)
                            {
                                defNode.Arguments.Add(vardef.Name, vardef.Type);
                                defNode._orderedArgs.Add(vardef.Name);
                            }
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
