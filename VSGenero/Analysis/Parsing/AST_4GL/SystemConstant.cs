using System.Collections.Generic;
using System.Text;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class SystemConstant : IAnalysisResult
    {
        private readonly string _name;
        private readonly string _typeName;
        private readonly object _value;

        public bool IsPublic { get { return true; } }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public SystemConstant(string name, string typeName, object value)
        {
            _name = name;
            _typeName = typeName;
            _value = value;
        }

        const string _scope = "system constant";
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
                if (!string.IsNullOrWhiteSpace(_typeName))
                {
                    sb.AppendFormat(" ({0})", _typeName);
                }
                if (_value != null)
                    sb.AppendFormat(" = {0}", _value);
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
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GetMultipleMembersInput input)
        {
            return null;
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public string Typename
        {
            get { return _typeName; }
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
