using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry, bool function)
        {
            definingProject = null;
            projEntry = null;
            IFunctionResult funcRes = null;
            _memberFunctions.TryGetValue(name, out funcRes);
            return funcRes;
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool getArrayTypeMembers)
        {
            return _memberFunctions.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, ast));
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
