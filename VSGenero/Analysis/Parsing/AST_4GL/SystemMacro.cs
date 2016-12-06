using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class SystemMacro : IAnalysisResult
    {
        private readonly string _name;
        private readonly Func<string> _macroExpansion;
        private readonly GeneroLanguageVersion _minBdlVersion;
        private readonly GeneroLanguageVersion _maxBdlVersion;
        const string _scope = "system macro";

        public SystemMacro(string name, 
                           Func<string> macroExpansion,
                           GeneroLanguageVersion minimumBdlVersion = GeneroLanguageVersion.None,
                           GeneroLanguageVersion maximumBdlVersion = GeneroLanguageVersion.Latest)
        {
            _name = name;
            _macroExpansion = macroExpansion;
            _minBdlVersion = minimumBdlVersion;
            _maxBdlVersion = maximumBdlVersion;
        }

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
                if (_macroExpansion != null)
                    sb.AppendFormat(" = {0}", _macroExpansion());
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location
        {
            get { return null; }
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public bool IsPublic
        {
            get { return true; }
        }

        public IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry, bool function)
        {
            definingProject = null;
            projEntry = null;
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool function)
        {
            return null;
        }

        public string Typename
        {
            get { return null; }
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
    }
}
