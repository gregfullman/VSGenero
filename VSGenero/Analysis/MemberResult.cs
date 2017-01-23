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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGenero.Analysis
{
    public struct MemberResult
    {
        private readonly string _name;
        private string _completion;
        private readonly Func<GeneroMemberType> _type;
        private readonly Func<IAnalysisResult> _var;
        private readonly GeneroAst _ast;
        private readonly string _documentation;

        public MemberResult(string name, IAnalysisResult var, GeneroMemberType type, GeneroAst ast)
        {
            _documentation = null;
            _name = _completion = name;
            _var = () => var;
            _type = null;
            _ast = ast;
            _type = () => type;
        }

        public MemberResult(string name, GeneroMemberType type, GeneroAst ast)
        {
            _documentation = null;
            _name = _completion = name;
            _type = () => type;
            _var = null;
            _ast = ast;
        }

        public MemberResult(string name, string documentation, GeneroMemberType type, GeneroAst ast)
        {
            _documentation = documentation;
            _name = _completion = name;
            _type = () => type;
            _var = null;
            _ast = ast;
        }
        
        public IAnalysisResult Var
        {
            get
            {
                return _var == null ? null : _var();
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Completion
        {
            get { return _completion; }
        }

        public string Documentation
        {
            get
            {
                if (_documentation != null)
                    return _documentation;

                var docSeen = new HashSet<string>();
                var typeSeen = new HashSet<string>();
                var docs = new List<string>();
                var types = new List<string>();

                var doc = new StringBuilder();
                if (_var != null)
                {
                    var ns = _var();
                    var docString = ns == null ? "" : ns.Documentation;
                    if (docSeen.Add(docString))
                    {
                        docs.Add(docString);
                    }
                    
                    foreach (var str in docs.OrderBy(s => s))
                    {
                        doc.AppendLine(str);
                        doc.AppendLine();
                    }
                }
                return Utils.CleanDocumentation(doc.ToString());
            }
        }

        public GeneroMemberType MemberType
        {
            get
            {
                return _type();
            }
        }
        
        /// <summary>
        /// Gets the location(s) for the member(s) if they are available.
        /// 
        /// New in 1.5.
        /// </summary>
        public LocationInfo Location
        {
            get
            {
                if (_var == null)
                    return null;
                var ns = _var();
                if(_ast != null)
                {
                    return _ast.ResolveLocation(ns);
                }
                return null;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MemberResult))
            {
                return false;
            }

            return Name == ((MemberResult)obj).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
