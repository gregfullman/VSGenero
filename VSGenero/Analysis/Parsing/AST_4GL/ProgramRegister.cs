using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class ProgramRegister : IAnalysisResult
    {
        private ProgramRegister _parentRegister;
        private readonly string _name;
        private readonly string _typeName;
        private readonly Dictionary<string, ProgramRegister> _childRegisters;
        private readonly GeneroLanguageVersion _minBdlVersion;
        private readonly GeneroLanguageVersion _maxBdlVersion;

        public bool CanGetValueFromDebugger
        {
            get { return true; }
        }

        public bool IsPublic { get { return true; } }

        public ProgramRegister(string name, 
                               string typeName, 
                               IEnumerable<ProgramRegister> childRegisters = null,
                               GeneroLanguageVersion minimumBdlVersion = GeneroLanguageVersion.None,
                               GeneroLanguageVersion maximumBdlVersion = GeneroLanguageVersion.Latest)
        {
            _parentRegister = null;
            _name = name;
            _typeName = typeName;
            _minBdlVersion = minimumBdlVersion;
            _maxBdlVersion = maximumBdlVersion;
            _childRegisters = new Dictionary<string, ProgramRegister>(StringComparer.OrdinalIgnoreCase);
            if (childRegisters != null)
            {
                foreach (var reg in childRegisters)
                {
                    reg._parentRegister = this;
                    _childRegisters.Add(reg._name, reg);
                }
            }
        }

        const string _scope = "program register";
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
                if (_parentRegister != null)
                {
                    sb.AppendFormat("{0}.", _parentRegister.Name);
                }
                sb.AppendFormat("{0} {1}", Name, _typeName);
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
            ProgramRegister progReg = null;
            if (_childRegisters != null)
            {
                _childRegisters.TryGetValue(name, out progReg);
            }
            else
            {

            }
            return progReg;
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool function)
        {
            if (_childRegisters != null)
            {
                return _childRegisters.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Variable, ast));
            }
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
