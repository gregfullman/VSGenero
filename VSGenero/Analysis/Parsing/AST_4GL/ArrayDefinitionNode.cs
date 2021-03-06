﻿/* ****************************************************************************
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

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    /// <summary>
    /// Static array definition:
    /// ARRAY [ size [,size  [,size] ] ] OF datatype
    /// 
    /// Dynamic array definition:
    /// DYNAMIC ARRAY [ WITH DIMENSION rank ] OF datatype
    /// 
    /// Java array definition:
    /// ARRAY [ ] OF javatype
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Arrays_002.html
    /// </summary>
    public class ArrayTypeReference : TypeReference
    {

        public AttributeSpecifier Attribute { get; private set; }

        public ArrayType ArrayType { get; private set; }
        private const uint _defaultDynamicArrayDim = 1;
        private uint _dynamicArrayDimension = 0;
        public uint DynamicArrayDimension
        {
            get 
            {
                if (_dynamicArrayDimension == 0) 
                    return _defaultDynamicArrayDim;
                return _dynamicArrayDimension;
            }
            private set
            {
                _dynamicArrayDimension = value;
            }
        }

        public override ITypeResult GetGeneroType()
        {
            if (Children.Count == 1 && Children[Children.Keys[0]] is TypeReference)
            {
                return (Children[Children.Keys[0]] as TypeReference).GetGeneroType();
            }
            return null;
        }

        public override IAnalysisResult ResolvedType
        {
            get
            {
                if(Children.Count == 1 && Children[Children.Keys[0]] is TypeReference)
                {
                    return (Children[Children.Keys[0]] as TypeReference).ResolvedType;
                }
                return null;
            }
        }

        public override string SimpleTypeName
        {
            get
            {
                if (Children.Count == 1 && Children[Children.Keys[0]] is TypeReference)
                {
                    return (Children[Children.Keys[0]] as TypeReference).Name;
                }
                return null;
            }
        }

        public ExpressionNode StaticDimOneSize { get; private set; }

        public ExpressionNode StaticDimTwoSize { get; private set; }

        public ExpressionNode StaticDimThreeSize { get; private set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if(ArrayType == ArrayType.Dynamic)
            {
                sb.Append("dynamic array ");
                if(DynamicArrayDimension != 1)
                {
                    sb.AppendFormat("with dimension {0}", DynamicArrayDimension);
                }
            }
            else if(ArrayType == ArrayType.Static)
            {
                sb.Append("array [");
                sb.Append(StaticDimOneSize.ToString());
                if (StaticDimTwoSize != null)
                {
                    sb.AppendFormat(", {0}", StaticDimTwoSize.ToString());
                    if (StaticDimThreeSize != null)
                    {
                        sb.AppendFormat(", {0}", StaticDimThreeSize.ToString());
                    }
                }
                sb.Append("]");
            }
            else
            {
                sb.Append("array []");
            }

            sb.Append(" of ");

            if (Children.Count == 1)
            {
                sb.Append(Children[Children.Keys[0]].ToString());
            }

            return sb.ToString();
        }

        public static bool TryParseNode(IParser parser, out ArrayTypeReference defNode)
        {
            defNode = null;
            bool result = false;
            if (parser.PeekToken(TokenKind.DynamicKeyword))
            {
                result = true;
                defNode = new ArrayTypeReference();
                defNode.ArrayType = ArrayType.Dynamic;
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._location = parser.TokenLocation;

                if (!parser.PeekToken(TokenKind.ArrayKeyword))
                    parser.ReportSyntaxError("Missing \"array\" keyword after \"dynamic\" array keyword.");
                else
                    parser.NextToken();

                AttributeSpecifier attribSpec;
                if (AttributeSpecifier.TryParseNode(parser, out attribSpec))
                {
                    defNode.Attribute = attribSpec;
                }

                // [WITH DIMENSION rank] is optional
                if (parser.PeekToken(TokenKind.WithKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.DimensionKeyword))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenCategory.NumericLiteral))
                        {
                            var tok = parser.NextToken() as ConstantValueToken;
                            if (tok != null)
                            {
                                uint intVal;
                                string val = tok.Value.ToString();
                                if (uint.TryParse(val, out intVal))
                                {
                                    if (intVal >= 1 && intVal <= 3)
                                    {
                                        defNode.DynamicArrayDimension = intVal;
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError("A dynamic array's dimension can be 1, 2, or 3.");
                                    }
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Invalid array dimension found.");
                                }
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid token found.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("Invalid dimension specifier for dynamic array.");
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("\"dimension\" keyword not specified for dynamic array");
                    }
                }

                if (!parser.PeekToken(TokenKind.OfKeyword))
                    parser.ReportSyntaxError("Missing \"of\" keyword in array definition.");
                else
                    parser.NextToken();

                // now try to get the datatype
                TypeReference typeRef;
                if ((TypeReference.TryParseNode(parser, out typeRef) && typeRef != null))
                {
                    defNode.Children.Add(typeRef.StartIndex, typeRef);
                    defNode.EndIndex = typeRef.EndIndex;
                }
                else
                {
                    parser.ReportSyntaxError("Invalid type specified for dynamic array definition");
                }
            }
            else if (parser.PeekToken(TokenKind.ArrayKeyword))
            {
                result = true;
                defNode = new ArrayTypeReference();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._location = parser.TokenLocation;

                if (!parser.PeekToken(TokenKind.LeftBracket))
                    parser.ReportSyntaxError("A non-dynamic array definition must have brackets.");
                else
                    parser.NextToken();

                if (parser.PeekToken(TokenKind.RightBracket))
                {
                    // we should have a java array
                    defNode.ArrayType = ArrayType.Java;
                    parser.NextToken();

                    AttributeSpecifier attribSpec;
                    if (AttributeSpecifier.TryParseNode(parser, out attribSpec))
                    {
                        defNode.Attribute = attribSpec;
                    }

                    if (!parser.PeekToken(TokenKind.OfKeyword))
                        parser.ReportSyntaxError("Missing \"of\" keyword in array definition.");
                    else
                        parser.NextToken();

                    // now try to get the datatype
                    TypeReference typeRef;
                    if ((TypeReference.TryParseNode(parser, out typeRef) && typeRef != null))
                    {
                        defNode.Children.Add(typeRef.StartIndex, typeRef);
                        defNode.EndIndex = typeRef.EndIndex;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid type specified for java array definition");
                    }
                }
                else
                {
                    defNode.ArrayType = ArrayType.Static;
                    List<TokenKind> breakToks = new List<TokenKind> { TokenKind.Comma, TokenKind.RightBracket };
                    // get the first-dimension size
                    ExpressionNode dimensionExpr;
                    if (!FglExpressionNode.TryGetExpressionNode(parser, out dimensionExpr, breakToks))
                        parser.ReportSyntaxError("Array's first dimension size is invalid");
                    else
                        defNode.StaticDimOneSize = dimensionExpr;

                    if(parser.PeekToken(TokenKind.Comma))
                    {
                        parser.NextToken();

                        if (!FglExpressionNode.TryGetExpressionNode(parser, out dimensionExpr, breakToks))
                            parser.ReportSyntaxError("Array's second dimension size is invalid");
                        else
                            defNode.StaticDimTwoSize = dimensionExpr;

                        if (parser.PeekToken(TokenKind.Comma))
                        {
                            parser.NextToken();

                            if (!FglExpressionNode.TryGetExpressionNode(parser, out dimensionExpr, breakToks))
                                parser.ReportSyntaxError("Array's third dimension size is invalid");
                            else
                                defNode.StaticDimThreeSize = dimensionExpr;
                        }
                    }

                    if(!parser.PeekToken(TokenKind.RightBracket))
                        parser.ReportSyntaxError("Invalid end of static array dimension specifier.");
                    else
                        parser.NextToken();

                    AttributeSpecifier attribSpec;
                    if (AttributeSpecifier.TryParseNode(parser, out attribSpec))
                    {
                        defNode.Attribute = attribSpec;
                    }

                    if (!parser.PeekToken(TokenKind.OfKeyword))
                        parser.ReportSyntaxError("Missing \"of\" keyword in array definition.");
                    else
                        parser.NextToken();

                    // now try to get the datatype
                    TypeReference typeRef;
                    if ((TypeReference.TryParseNode(parser, out typeRef) && typeRef != null))
                    {
                        defNode.Children.Add(typeRef.StartIndex, typeRef);
                        defNode.EndIndex = typeRef.EndIndex;
                        defNode.IsComplete = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid type specified for static array definition");
                    }
                }
            }
            return result;
        }

        internal IEnumerable<IAnalysisResult> GetAnalysisResults(MemberType memberType, GetMemberInput input)
        {
            List<IAnalysisResult> results = new List<IAnalysisResult>();
            if (Children.Count == 1)
            {
                // get the table's columns
                var node = Children[Children.Keys[0]];
                if(node is TypeReference)
                {
                    results.AddRange((node as TypeReference).GetAnalysisMembers(memberType, input));
                }
            }
            results.AddRange(Genero4glAst.ArrayFunctions.Values.Where(x => input.AST.LanguageVersion >= x.MinimumLanguageVersion && input.AST.LanguageVersion <= x.MaximumLanguageVersion));
            return results;
        }

        internal IEnumerable<MemberResult> GetMembersInternal(GetMultipleMembersInput input)
        {
            List<MemberResult> results = new List<MemberResult>();
            if (input.GetArrayTypeMembers)
            {
                if (Children.Count == 1)
                {
                    var node = Children[Children.Keys[0]];
                    if (node is TypeReference)
                    {
                        var newInput = new GetMultipleMembersInput
                        {
                            AST = input.AST,
                            MemberType = input.MemberType
                            // We don't want to continue getting array type members further down
                        };
                        results.AddRange((node as TypeReference).GetMembers(newInput));   
                    }
                }
            }
            else
            {
                results.AddRange(Genero4glAst.ArrayFunctions.Values.Where(x => input.AST.LanguageVersion >= x.MinimumLanguageVersion && input.AST.LanguageVersion <= x.MaximumLanguageVersion)
                                                                       .Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, input.AST)));
            }

            return results;
        }
    }
}
