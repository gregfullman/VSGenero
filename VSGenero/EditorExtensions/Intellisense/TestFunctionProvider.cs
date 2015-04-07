using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(IFunctionInformationProvider)), ContentType(VSGeneroConstants.ContentType4GL), Order]
    internal class TestFunctionProvider : IFunctionInformationProvider
    {
        private readonly Dictionary<string, TestFunctionCollection> _collections;

        internal TestFunctionProvider()
        {
            _collections = new Dictionary<string, TestFunctionCollection>(StringComparer.OrdinalIgnoreCase);
            _collections.Add("TestModule", new TestFunctionCollection("TestModule", new List<TestFunction>
                {
                    new TestFunction("Function1", "The first test function", new List<ParameterResult>()),
                    new TestFunction("Function2", "The second test function", new List<ParameterResult>
                        {
                            new ParameterResult("param1", "The first param", "boolean")
                        }),
                }));
        }

        public string Name
        {
            get { return "TestFunctions"; }
        }

        public void SetFilename(string filename)
        {
            int i = 0;
        }

        public IFunctionResult GetFunction(string functionName)
        {
            return null;
        }

        public string Scope
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string Documentation
        {
            get { return "A test repository of public functions."; }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return _collections.Count > 0;
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            TestFunctionCollection collection = null;
            _collections.TryGetValue(name, out collection);
            return collection;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            return _collections.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Class, ast));
        }
    }

    internal class TestFunctionCollection : IAnalysisResult
    {
        private readonly Dictionary<string, TestFunction> _functions;

        internal TestFunctionCollection(string name, IEnumerable<TestFunction> functions)
        {
            _name = name;
            _functions = new Dictionary<string, TestFunction>(StringComparer.OrdinalIgnoreCase);
            foreach (var func in functions)
                _functions.Add(func.Name, func);
        }

        public string Scope
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        private readonly string _name;
        public string Name
        {
            get { return _name; }
        }

        public string Documentation
        {
            get { return string.Format("Function repository for collection {0}.", _name); }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return _functions.Count > 0;
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            TestFunction func = null;
            _functions.TryGetValue(name, out func);
            return func;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            return _functions.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, ast));
        }
    }

    internal class TestFunction : IFunctionResult
    {
        internal TestFunction(string name, string desc, List<ParameterResult> parameters)
        {
            _parameters = parameters.ToArray();
            _name = name;
            _desc = desc;
        }

        private readonly ParameterResult[] _parameters;
        public ParameterResult[] Parameters
        {
            get { return _parameters; }
        }

        public AccessModifier AccessModifier
        {
            get { return Analysis.AccessModifier.Public; }
        }

        public string FunctionDocumentation
        {
            get { return ""; }
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

        public string Scope
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        private readonly string _name;
        public string Name
        {
            get { return _name; }
        }

        private readonly string _desc;
        public string Documentation
        {
            get { return _desc; }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return false;
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            return new List<MemberResult>();
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
    }
}
