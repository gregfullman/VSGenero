using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class ProgramRegister : IVariableResult, ITypeResult
    {
        private ProgramRegister _parentRegister;
        private readonly string _name;
        private readonly string _typeName;
        private readonly Dictionary<string, ProgramRegister> _childRegisters;
        private readonly GeneroLanguageVersion _minBdlVersion;
        private readonly GeneroLanguageVersion _maxBdlVersion;

        public Dictionary<string, ProgramRegister> ChildRegisters {  get { return _childRegisters; } }

        public bool CanGetValueFromDebugger
        {
            get { return true; }
        }

        public bool IsPublic { get { return true; } }

        public ProgramRegister(string name, 
                               string typeName, 
                               IEnumerable<ProgramRegister> childRegisters = null,
                               bool isArray = false,
                               int arrayDimension = 0,
                               string arrayType = null,
                               GeneroLanguageVersion minimumBdlVersion = GeneroLanguageVersion.None,
                               GeneroLanguageVersion maximumBdlVersion = GeneroLanguageVersion.Latest)
        {
            _parentRegister = null;
            _name = name;
            _typeName = typeName;
            _minBdlVersion = minimumBdlVersion;
            _maxBdlVersion = maximumBdlVersion;
            _isArray = isArray;
            _arrayDimension = arrayDimension;
            if(arrayType != null)
            {
                _arrayType = new VariableTypeResult
                {
                    Typename = arrayType
                };
            }
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

        public IAnalysisResult GetMember(GetMemberInput input)
        {
            ProgramRegister progReg = null;
            if (_childRegisters != null)
            {
                _childRegisters.TryGetValue(input.Name, out progReg);
            }
            else
            {

            }
            return progReg;
        }

        public IEnumerable<MemberResult> GetMembers(GetMultipleMembersInput input)
        {
            if (_childRegisters != null)
            {
                return _childRegisters.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Variable, input.AST));
            }
            return null;
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public ITypeResult GetGeneroType()
        {
            // TODO:
            return this;
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

        public bool IsRecord
        {
            get
            {
                return _childRegisters.Count > 0;
            }
        }

        public Dictionary<string, ITypeResult> RecordMemberTypes
        {
            get
            {
                return _childRegisters.ToDictionary(x => x.Key, x => x.Value as ITypeResult);
            }
        }

        private readonly bool _isArray;
        public bool IsArray
        {
            get
            {
                return _isArray;
            }
        }

        private readonly int _arrayDimension;
        public int ArrayDimension
        {
            get
            {
                return _arrayDimension;
            }
        }

        private readonly ITypeResult _arrayType;
        public ITypeResult ArrayType
        {
            get
            {
                return _arrayType;
            }
        }

        public ITypeResult UnderlyingType
        {
            get
            {
                return null;
            }
        }
    }
}
