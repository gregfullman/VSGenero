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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class VariableDef : IVariableResult
    {
        private string _name;
        public string Name
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_namespace))
                    return string.Format("{0}.{1}", _namespace, _name);
                return _name;
            }
            private set { _name = value; }
        }

        public TypeReference Type { get; internal set; }

        public IAnalysisResult ResolvedType {  get { return Type; } }

        private string _filename;
        private string _namespace;
        private bool _isPublic = false;
        public bool IsPublic
        {
            get { return _isPublic; }
        }

        public void SetIsPublic(bool value)
        {
            _isPublic = value;
        }

        public VariableDef(string name, TypeReference type, int location, bool isPublic = false, string filename = null)
        {
            Name = name;
            Type = type;
            _locationIndex = location;
            _isPublic = isPublic;
            _filename = filename;
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
                sb.AppendFormat("{0} {1}", Name, Type.ToString());
                return sb.ToString();
            }
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

        private int _locationIndex;
        public int LocationIndex
        {
            get { return _locationIndex; }
        }

        public LocationInfo Location
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_filename))
                {
                    return new LocationInfo(_filename, _locationIndex);
                }
                return null;
            }
        }

        public IAnalysisResult GetMember(GetMemberInput input)
        {
            return Type.GetMember(input);
        }

        public IEnumerable<MemberResult> GetMembers(GetMultipleMembersInput input)
        {
            return Type.GetMembers(input);
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return Type.HasChildFunctions(ast);
        }

        public bool CanGetValueFromDebugger
        {
            get { return true; }
        }

        public void SetNamespace(string ns)
        {
            _namespace = ns;
        }

        public ITypeResult GetGeneroType()
        {
            return Type?.GetGeneroType();
        }

        public string Typename
        {
            get { return Type.ToString(); }
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

        public void PropagateSyntaxTree(GeneroAst ast)
        {
            Type?.PropagateSyntaxTree(ast);
        }
    }

    /// <summary>
    /// Format:
    /// identifier [,...] <see cref="TypeReference"/> [<see cref="AttributeSpecifier"/>]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_DEFINE.html
    /// </summary>
    public class VariableDefinitionNode : AstNode4gl
    {
        private List<Tuple<string, int>> _identifiers;
        public List<Tuple<string, int>> Identifiers
        {
            get
            {
                if (_identifiers == null)
                {
                    _identifiers = new List<Tuple<string, int>>();
                }
                return _identifiers;
            }
            set
            {
                _identifiers = value;
            }
        }

        public LocationInfo Location { get; private set; }

        private List<VariableDef> _varDefs;
        public List<VariableDef> VariableDefinitions
        {
            get
            {
                if (_varDefs == null)
                    _varDefs = new List<VariableDef>();
                return _varDefs;
            }
        }

        public static bool TryParseNode(IParser parser, out VariableDefinitionNode defNode, Action<VariableDef> binder = null, bool reportSyntaxError = true, bool allowShorthandDefinition = true)
        {
            defNode = null;
            bool result = false;
            uint peekaheadCount = 1;
            List<Tuple<string, int>> identifiers = new List<Tuple<string, int>>();

            var tok = parser.PeekTokenWithSpan(peekaheadCount);
            var tokInfo = Tokenizer.GetTokenInfo(tok.Token);
            while (tokInfo.Category == TokenCategory.Identifier || tokInfo.Category == TokenCategory.Keyword)
            {
                identifiers.Add(new Tuple<string, int>(tok.Token.Value.ToString(), tok.Span.Start));
                peekaheadCount++;
                if (!parser.PeekToken(TokenKind.Comma, peekaheadCount) || !allowShorthandDefinition)
                    break;
                peekaheadCount++;
                tok = parser.PeekTokenWithSpan(peekaheadCount);
                tokInfo = Tokenizer.GetTokenInfo(tok.Token);
            }

            if (identifiers.Count > 0)
            {
                result = true;
                defNode = new VariableDefinitionNode();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode.Location = parser.TokenLocation;
                defNode.Identifiers = new List<Tuple<string, int>>(identifiers);
                while (peekaheadCount > 1)
                {
                    parser.NextToken();
                    peekaheadCount--;
                }

                TypeReference typeRef;
                if (TypeReference.TryParseNode(parser, out typeRef) && typeRef != null)
                {
                    defNode.EndIndex = typeRef.EndIndex;
                    defNode.Children.Add(typeRef.StartIndex, typeRef);
                    if (defNode.Children.Last().Value.IsComplete)
                    {
                        defNode.IsComplete = true;
                    }
                }
                else
                {
                    if(reportSyntaxError)
                        parser.ReportSyntaxError("No type defined for variable(s).");
                    result = false;
                }

                if (typeRef != null)
                {
                    foreach (var ident in defNode.Identifiers)
                    {
                        var varDef = new VariableDef(ident.Item1, typeRef, ident.Item2, false, (defNode.Location != null ? defNode.Location.FilePath : null));
                        defNode.VariableDefinitions.Add(varDef);
                        if (binder != null)
                        {
                            binder(varDef);
                        }
                    }
                }
            }

            return result;
        }

        public override void CheckForErrors(GeneroAst ast, Action<string, int, int> errorFunc, Dictionary<string, List<int>> deferredFunctionSearches, FunctionProviderSearchMode searchInFunctionProvider = FunctionProviderSearchMode.NoSearch, bool isFunctionCallOrDefinition = false)
        {
            // This will do error checking on the type reference
            base.CheckForErrors(ast, errorFunc, deferredFunctionSearches, searchInFunctionProvider, isFunctionCallOrDefinition);

            if (VariableDefinitions != null)
            {
                if (Children.Count == 1 &&
                    Children[Children.Keys[0]] is TypeReference)
                {
                    var resolvedType = (Children[Children.Keys[0]] as TypeReference).ResolvedType;
                    if (resolvedType != null)
                    {
                        foreach (var vardef in VariableDefinitions)
                        {
                            // apply the resolved type definition to the variable
                            // TODO:
                            //vardef.ResolvedType = resolvedType;
                        }
                    }
                }
            }
        }

        public override void SetNamespace(string ns)
        {
            foreach (var def in VariableDefinitions)
                def.SetNamespace(ns);
        }
    }

    //internal static class VariableTypeFactory
    //{
    //    internal static ITypeResult GetVariableType(this IAnalysisResult analysisResult)
    //    {
    //        ITypeResult typeResult = null;
    //        var varDef = analysisResult as VariableDef;
    //        if(varDef != null && varDef.Type != null)
    //        {
    //            typeResult = new VariableType
    //            {
    //                IsArray =  varDef.Type.IsArray,
    //                IsRecord = varDef.Type.IsRecord
    //            };
    //            var resolvedType = varDef.Type.ResolvedType;
    //            while(resolvedType != null)
    //            {
    //                if (resolvedType is TypeDefinitionNode)
    //                {
    //                    var typeRef = (resolvedType as TypeDefinitionNode).TypeRef;
    //                    if (typeRef != null)
    //                    {
    //                        if (typeRef.ResolvedType != null)
    //                        {
    //                            resolvedType = typeRef.ResolvedType;
    //                            continue;
    //                        }
    //                        else
    //                        {
    //                            (typeResult as VariableType).Typename = typeRef.Name;
    //                            (typeResult as VariableType).IsArray = typeRef.IsArray;
    //                            (typeResult as VariableType).IsRecord = typeRef.IsRecord;
    //                        }
    //                    }
    //                }
    //                else if (resolvedType is GeneroPackageClass)
    //                {
    //                    (typeResult as VariableType).Typename = (resolvedType as GeneroPackageClass).Name;
    //                    break;
    //                }

    //                break;
    //            }
    //            if(typeResult.Typename == null)
    //            {
    //                (typeResult as VariableType).Typename = varDef.Type.SimpleTypeName;
    //            }
    //            return typeResult;
    //            //if(resolvedType.)
    //            //while(resolvedType != null)
    //            //{
    //            //    if (resolvedType is TypeDefinitionNode)
    //            //    {
    //            //        resolvedType = (resolvedType as TypeDefinitionNode).TypeRef;
    //            //    }
    //            //    else if(resolvedType is GeneroPackageClass)
    //            //    {
    //            //            (resolvedType as GeneroPackageClass).
    //            //    }
    //            //}
    //        }
    //        else if(analysisResult is ProgramRegister)
    //        {
    //            var progReg = analysisResult as ProgramRegister;
    //            typeResult = new VariableType
    //            {
    //                IsArray = false,
    //                IsRecord = progReg.ChildRegisters.Count > 0,
    //                Typename = progReg.Typename
    //            };
    //        }
    //        return typeResult;
    //    }

    internal class VariableTypeResult : ITypeResult
    {
        public ITypeResult ArrayType { get; internal set; }

        public bool IsArray { get; internal set; }

        public bool IsRecord { get; internal set; }

        public Dictionary<string, ITypeResult> RecordMemberTypes { get; internal set; }

        public string Typename { get; internal set; }

        public ITypeResult UnderlyingType { get; internal set; }
    }
}
