using System;
using System.Collections.Generic;
using System.Text;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class BuiltinFunction : IFunctionResult
    {
        private readonly string _name;
        private readonly List<ParameterResult> _parameters;
        private readonly List<string> _returns;
        private readonly string _description;
        private readonly string _namespace;
        private readonly string _documentationUrl;
        private readonly GeneroLanguageVersion _minBdlVersion;
        private readonly GeneroLanguageVersion _maxBdlVersion;

        public bool IsPublic { get { return true; } }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public BuiltinFunction(string name, 
                               string nameSpace, 
                               IEnumerable<ParameterResult> parameters, 
                               IEnumerable<string> returns, 
                               string description,
                               string documentationUrl = null, 
                               GeneroLanguageVersion minimumBdlVersion = GeneroLanguageVersion.None,
                               GeneroLanguageVersion maximumBdlVersion = GeneroLanguageVersion.Latest)
        {
            _name = name;
            _namespace = nameSpace;
            _description = description;
            _documentationUrl = documentationUrl;
            _parameters = new List<ParameterResult>(parameters);
            _returns = new List<string>(returns);
            _minBdlVersion = minimumBdlVersion;
            _maxBdlVersion = maximumBdlVersion;
        }

        public string DefinitionUrl { get { return _documentationUrl; } }

        public ParameterResult[] Parameters
        {
            get { return _parameters.ToArray(); }
        }

        public AccessModifier AccessModifier
        {
            get { return Analysis.AccessModifier.Public; }
        }

        public string FunctionDocumentation
        {
            get { return _description; }
        }

        private Dictionary<string, IAnalysisResult> _dummyDict = new Dictionary<string, IAnalysisResult>();
        public IDictionary<string, IAnalysisResult> Variables
        {
            get { return _dummyDict; }
        }

        public IDictionary<string, IAnalysisResult> Types
        {
            get { return _dummyDict; }
        }

        public IDictionary<string, IAnalysisResult> Constants
        {
            get { return _dummyDict; }
        }

        private string _scope = "system function";
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }

                if (_returns.Count == 0)
                {
                    sb.Append("void ");
                }
                else if (_returns.Count == 1)
                {
                    sb.AppendFormat("{0} ", _returns[0]);
                }

                if (!string.IsNullOrWhiteSpace(_namespace))
                    sb.AppendFormat("{0}.", _namespace);
                sb.Append(Name);
                sb.Append('(');

                // if there are any parameters put them in
                int total = _parameters.Count;
                int i = 0;
                foreach (var varDef in _parameters)
                {
                    sb.AppendFormat("{0} {1}", varDef.Type, varDef.Name);
                    if (i + 1 < total)
                    {
                        sb.Append(", ");
                    }
                    i++;
                }

                sb.Append(')');

                if (_returns.Count > 1)
                {
                    sb.AppendLine();
                    sb.Append("returning ");
                    foreach (var ret in _returns)
                    {
                        sb.Append(ret);
                        if (i + 1 < total)
                        {
                            sb.Append(", ");
                        }
                        i++;
                    }
                }
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        private LocationInfo _location;
        public LocationInfo Location
        {
            get
            {
                if(_location == null && !string.IsNullOrWhiteSpace(DefinitionUrl))
                {
                    _location = new LocationInfo(DefinitionUrl);
                }
                return _location;
            }
        }

        public IAnalysisResult GetMember(GetMemberInput input)
        {
            if (_returns != null && _returns.Count == 1)
            {
                var typeRef = new TypeReference(_returns[0]);
                return typeRef.GetMember(input);
            }
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GetMultipleMembersInput input)
        {
            if (_returns != null && _returns.Count == 1)
            {
                var typeRef = new TypeReference(_returns[0]);
                return typeRef.GetMembers(input);
            }
            return new MemberResult[0];
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public void SetCommentDocumentation(string commentDoc)
        {
        }

        public bool CanOutline
        {
            get { return false; }
        }

        public int StartIndex
        {
            get
            {
                return -1;
            }
            set
            {
            }
        }

        public int EndIndex
        {
            get
            {
                return -1;
            }
            set
            {
            }
        }

        public int DecoratorEnd
        {
            get
            {
                return -1;
            }
            set
            {
            }
        }

        public string CompletionParentName
        {
            get { return null; }
        }


        public int DecoratorStart
        {
            get
            {
                return StartIndex;
            }
            set
            {
            }
        }

        public string Typename
        {
            get
            {
                if (_returns != null && _returns.Count == 1)
                {
                    var typeRef = new TypeReference(_returns[0]);
                    return typeRef.ToString();
                }
                return null;
            }
        }


        public string[] Returns
        {
            get { return _returns.ToArray(); }
        }

        private Dictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> _dummyLimitDict = new Dictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>>();
        public IDictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> LimitedScopeVariables
        {
            get { return _dummyLimitDict; }
        }

        private SortedList<int, int> _additionalDecoratorRanges;
        public SortedList<int, int> AdditionalDecoratorRanges
        {
            get
            {
                if (_additionalDecoratorRanges == null)
                    _additionalDecoratorRanges = new SortedList<int, int>();
                return _additionalDecoratorRanges;
            }
        }

        public GeneroLanguageVersion MinimumLanguageVersion
        {
            get
            {
                return _minBdlVersion;
            }
        }

        public GeneroLanguageVersion MaximumLanguageVersion
        {
            get
            {
                return _maxBdlVersion;
            }
        }

        public GeneroMemberType FunctionType
        {
            get
            {
                return GeneroMemberType.Function;
            }
        }
    }
}
