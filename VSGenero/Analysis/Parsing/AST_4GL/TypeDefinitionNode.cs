/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/ 

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    /// <summary>
    /// identifier 
    ///     <see cref="TypeReference"/> [<see cref="AttributeSpecifier"/>]
    ///     |
    ///     <see cref="RecordDefinitionNode"/>
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_user_types_003.html
    /// </summary>
    public class TypeDefinitionNode : AstNode4gl, IAnalysisResult
    {
        public string Identifier { get; private set; }

        private bool _isPublic;
        public bool IsPublic { get { return _isPublic; } }

        public TypeReference TypeRef
        {
            get
            {
                if(Children.Count == 1 && Children[Children.Keys[0]] is TypeReference)
                {
                    return Children[Children.Keys[0]] as TypeReference;
                }
                return null;
            }
        }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public static bool TryParseDefine(IParser parser, out TypeDefinitionNode defNode, bool isPublic = false)
        {
            defNode = null;
            bool result = false;
            if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
            {
                defNode = new TypeDefinitionNode();
                result = true;
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._location = parser.TokenLocation;
                defNode.Identifier = parser.Token.Token.Value.ToString();
                defNode._isPublic = isPublic;

                TypeReference typeRef;
                if (TypeReference.TryParseNode(parser, out typeRef, false, isPublic) && typeRef != null)
                {
                    defNode.Children.Add(typeRef.StartIndex, typeRef);
                    if (defNode.Children.All(x => x.Value.IsComplete))
                    {
                        defNode.EndIndex = defNode.Children.Last().Value.EndIndex;
                        defNode.IsComplete = true;
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid syntax found in type definition.");
                }
            }
            return result;
        }

        private string _scope;
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
                _scope = value;
            }
        }

        public string Name
        {
            get 
            {
                if (!string.IsNullOrWhiteSpace(_namespace))
                    return string.Format("{0}.{1}", _namespace, Identifier);
                return Identifier; 
            }
        }

        private string _namespace;

        public override string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.Append(Name);
                if (Children.Count == 1 && Children[Children.Keys[0]] is TypeReference)
                {
                    sb.AppendFormat(" {0}", (Children[Children.Keys[0]] as TypeReference).ToString());
                }
                return sb.ToString();
            }
        }


        public int LocationIndex
        {
            get { return StartIndex; }
        }

        private LocationInfo _location;
        public LocationInfo Location { get { return _location; } }

        public IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry, bool function)
        {
            definingProject = null;
            projEntry = null;
            if (Children.Count == 1 &&
               Children[Children.Keys[0]] is TypeReference)
            {
                return (Children[Children.Keys[0]] as TypeReference).GetMember(name, ast, out definingProject, out projEntry, function);
            }
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool function)
        {
            if(Children.Count == 1 &&
               Children[Children.Keys[0]] is TypeReference)
            {
                return (Children[Children.Keys[0]] as TypeReference).GetMembers(ast, memberType, function);
            }
            return null;
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            if (Children.Count == 1 &&
               Children[Children.Keys[0]] is TypeReference)
            {
                return (Children[Children.Keys[0]] as TypeReference).HasChildFunctions(ast);
            }
            return false;
        }

        public override void SetNamespace(string ns)
        {
            _namespace = ns;
            base.SetNamespace(ns);
        }

        public string Typename
        {
            get 
            { 
                return Name; 
            }
        }
    }
}
