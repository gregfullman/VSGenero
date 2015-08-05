/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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
    //[Export(typeof(IFunctionInformationProvider)), ContentType(VSGeneroConstants.ContentType4GL), Order]
    internal class TestFunctionProvider : IFunctionInformationProvider
    {
        private readonly Dictionary<string, TestFunctionCollection> _collections;
        private readonly Dictionary<string, string> _reverseMap;

        internal TestFunctionProvider()
        {
            _collections = new Dictionary<string, TestFunctionCollection>(StringComparer.OrdinalIgnoreCase);
            _reverseMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<TestFunction>();
            list.Add(new TestFunction("Function1", string.Format("{0}.{1}.", Name, "TestModule"), "The first test function", new List<ParameterResult>()));
            list.Add(new TestFunction("Function2", string.Format("{0}.{1}.", Name, "TestModule"), "The second test function", new List<ParameterResult>
                        {
                            new ParameterResult("param1", "The first param", "boolean")
                        }));
            foreach(var item in list)
                _reverseMap.Add(item.Name, "TestModule");
            _collections.Add("TestModule", new TestFunctionCollection("TestModule", list));
        }

        public string Name
        {
            get { return "TestFunctions"; }
        }

        public void SetFilename(string filename)
        {
            int i = 0;
        }

        public IEnumerable<IFunctionResult> GetFunction(string functionName)
        {
            string moduleName = null;
            if(_reverseMap.TryGetValue(functionName, out moduleName))
            {
                return new List<TestFunction> { _collections[moduleName].GetFunction(functionName) };
            }
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

        public LocationInfo Location { get { return null; } }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return _collections.Count > 0;
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry)
        {
            definingProject = null;
            projEntry = null;
            TestFunctionCollection collection = null;
            _collections.TryGetValue(name, out collection);
            return collection;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType)
        {
            return _collections.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Class, ast));
        }


        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public string GetImportModuleFilename(string importModule)
        {
            return null;
        }


        public IEnumerable<string> GetAvailableImportModules()
        {
            return new string[0];
        }


        public bool IsPublic
        {
            get { return true; }
        }


        public IEnumerable<IFunctionResult> GetFunctionsStartingWith(string matchText)
        {
            return new IFunctionResult[0];
        }


        public void SetOneTimeNamespace(string nameSpace)
        {
        }

        public string Typename
        {
            get { return null; }
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

        public bool IsPublic
        {
            get { return true; }
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

        public LocationInfo Location { get { return null; } }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return _functions.Count > 0;
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry)
        {
            definingProject = null;
            projEntry = null;
            return GetFunction(name);
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType)
        {
            return _functions.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, ast));
        }

        internal TestFunction GetFunction(string name)
        {
            TestFunction func = null;
            _functions.TryGetValue(name, out func);
            return func;
        }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }


        public void SetOneTimeNamespace(string nameSpace)
        {
        }

        public string Typename
        {
            get { return null; }
        }
    }

    internal class TestFunction : IFunctionResult
    {
        internal TestFunction(string name, string completionParentName, string desc, List<ParameterResult> parameters)
        {
            _parameters = parameters.ToArray();
            _name = name;
            _desc = desc;
            _completionParentName = completionParentName;
        }

        public bool IsPublic
        {
            get { return true; }
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

        public string Namespace { get { return null; } }

        private readonly string _desc;
        public string Documentation
        {
            get { return _desc; }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location { get { return null; } }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return false;
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry)
        {
            definingProject = null;
            projEntry = null;
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType)
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

        private readonly string _completionParentName;
        public string CompletionParentName
        {
            get { return _completionParentName; }
        }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
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


        public void SetOneTimeNamespace(string nameSpace)
        {
        }

        public string Typename
        {
            get { return null; }
        }


        public string[] Returns
        {
            get { return new string[0]; }
        }
    }
}
