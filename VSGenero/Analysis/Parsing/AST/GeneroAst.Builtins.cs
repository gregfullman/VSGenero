using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public partial class GeneroAst
    {
        private static bool _initialized = false;
        private static object _initLock = new object();

        private static Dictionary<string, IAnalysisResult> _systemVariables;
        public static IDictionary<string, IAnalysisResult> SystemVariables
        {
            get
            {
                if (_systemVariables == null)
                    _systemVariables = new Dictionary<string, IAnalysisResult>();
                return _systemVariables;
            }
        }

        private static Dictionary<string, IAnalysisResult> _systemConstants;
        public static IDictionary<string, IAnalysisResult> SystemConstants
        {
            get
            {
                if (_systemConstants == null)
                    _systemConstants = new Dictionary<string, IAnalysisResult>();
                return _systemConstants;
            }
        }

        private static void InitializeBuiltins()
        {
            lock (_initLock)
            {
                if (!_initialized)
                {
                    // System variables
                    SystemVariables.Add("status", new ProgramRegister("status", "int"));
                    SystemVariables.Add("int_flag", new ProgramRegister("int_flag", "boolean"));
                    SystemVariables.Add("quit_flag", new ProgramRegister("quit_flag", "boolean"));

                    // System constants
                    SystemConstants.Add("null", new SystemConstant("null", null, null));
                    SystemConstants.Add("true", new SystemConstant("true", "int", 1));
                    SystemConstants.Add("false", new SystemConstant("false", "int", 2));
                    SystemConstants.Add("notfound", new SystemConstant("notfound", "int", 100));

                    _initialized = true;
                }
            }
        }
    }

    public class ProgramRegister : IAnalysisResult
    {
        private readonly string _name;
        private readonly string _typeName;

        public ProgramRegister(string name, string typeName)
        {
            _name = name;
            _typeName = typeName;
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

        public string Documentation
        {
            get 
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.AppendFormat("{0} {1}", Name, _typeName);
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }
    }

    public class SystemConstant : IAnalysisResult
    {
        private readonly string _name;
        private readonly string _typeName;
        private readonly object _value;

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
                if(_value != null)
                    sb.AppendFormat(" = {0}", _value);
                return sb.ToString();
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }
    }
}
