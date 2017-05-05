using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class SystemClass : IAnalysisResult
    {
        private readonly string _name;
        private readonly Dictionary<string, IFunctionResult> _memberFunctions;

        public bool IsPublic { get { return true; } }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public SystemClass(string name, IEnumerable<IFunctionResult> memberFunctions)
        {
            _name = name;
            _memberFunctions = new Dictionary<string, IFunctionResult>(StringComparer.OrdinalIgnoreCase);
            foreach (var func in memberFunctions)
                _memberFunctions.Add(func.Name, func);
        }

        private string _scope = "system class";
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
            get { return _name; }
        }

        public string Namespace { get { return null; } }

        public string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.Append(Name);
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location { get { return null; } }

        public IAnalysisResult GetMember(GetMemberInput input)
        {
            IFunctionResult funcRes = null;
            _memberFunctions.TryGetValue(input.Name, out funcRes);
            return funcRes;
        }

        public IEnumerable<MemberResult> GetMembers(GetMultipleMembersInput input)
        {
            return _memberFunctions.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, input.AST));
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return _memberFunctions.Count > 0;
        }

        public string Typename
        {
            get { return null; }
        }

        public GeneroLanguageVersion MinimumLanguageVersion
        {
            get
            {
                return GeneroLanguageVersion.None;
            }
        }

        public GeneroLanguageVersion MaximumLanguageVersion
        {
            get
            {
                return GeneroLanguageVersion.Latest;
            }
        }
    }
}
