using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
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

        private const UInt16 _defaultStaticDimOneSize = UInt16.MaxValue;
        private const UInt16 _defaultStaticDimTwoSize = UInt16.MaxValue;
        private const UInt16 _defaultStaticDimThreeSize = UInt16.MaxValue;

        private UInt16 _staticDimOneSize = 0;
        private UInt16 _staticDimTwoSize = 0;
        private UInt16 _staticDimThreeSize = 0;

        public UInt16 StaticDimOneSize
        {
            get
            {
                if (_staticDimOneSize == 0)
                    return _defaultStaticDimOneSize;
                return _staticDimOneSize;
            }
            set
            {
                _staticDimOneSize = value;
            }
        }

        public UInt16 StaticDimTwoSize
        {
            get
            {
                if (_staticDimTwoSize == 0)
                    return _defaultStaticDimTwoSize;
                return _staticDimTwoSize;
            }
            set
            {
                _staticDimTwoSize = value;
            }
        }

        public UInt16 StaticDimThreeSize
        {
            get
            {
                if (_staticDimThreeSize == 0)
                    return _defaultStaticDimThreeSize;
                return _staticDimThreeSize;
            }
            set
            {
                _staticDimThreeSize = value;
            }
        }

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
                if (StaticDimOneSize != UInt16.MaxValue)
                {
                    sb.Append(StaticDimOneSize);
                    if(StaticDimTwoSize != UInt16.MaxValue)
                    {
                        sb.AppendFormat(", {0}", StaticDimTwoSize);

                        if(StaticDimThreeSize != UInt16.MaxValue)
                        {
                            sb.AppendFormat(", {0}", StaticDimThreeSize);
                        }
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
                if ((TypeReference.TryParseNode(parser, out typeRef)))
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

                if (!parser.PeekToken(TokenKind.LeftBracket))
                    parser.ReportSyntaxError("A non-dynamic array definition must have brackets.");
                else
                    parser.NextToken();

                if (parser.PeekToken(TokenKind.RightBracket))
                {
                    // we should have a java array
                    defNode.ArrayType = ArrayType.Java;
                    parser.NextToken();

                    if (!parser.PeekToken(TokenKind.OfKeyword))
                        parser.ReportSyntaxError("Missing \"of\" keyword in array definition.");
                    else
                        parser.NextToken();

                    // now try to get the datatype
                    TypeReference typeRef;
                    if ((TypeReference.TryParseNode(parser, out typeRef)))
                    {
                        defNode.Children.Add(typeRef.StartIndex, typeRef);
                        defNode.EndIndex = typeRef.EndIndex;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid type specified for java array definition");
                    }
                }
                else if (parser.PeekToken(TokenCategory.NumericLiteral))
                {
                    defNode.ArrayType = ArrayType.Static;
                    parser.NextToken();

                    // get the first-dimension size
                    UInt16 val;
                    if (!UInt16.TryParse(parser.Token.Token.Value.ToString(), out val))
                        parser.ReportSyntaxError("Array's first dimension size is invalid");
                    else
                        defNode.StaticDimOneSize = val;

                    if(parser.PeekToken(TokenKind.Comma))
                    {
                        parser.NextToken();
                        if (!parser.PeekToken(TokenCategory.NumericLiteral))
                            parser.ReportSyntaxError("Invalid token found in static array size.");
                        else
                            parser.NextToken();

                        if (!UInt16.TryParse(parser.Token.Token.Value.ToString(), out val))
                            parser.ReportSyntaxError("Array's second dimension size is invalid");
                        else
                            defNode.StaticDimTwoSize = val;

                        if (parser.PeekToken(TokenKind.Comma))
                        {
                            parser.NextToken();
                            if (!parser.PeekToken(TokenCategory.NumericLiteral))
                                parser.ReportSyntaxError("Invalid token found in static array size.");
                            else
                                parser.NextToken();

                            if (!UInt16.TryParse(parser.Token.Token.Value.ToString(), out val))
                                parser.ReportSyntaxError("Array's third dimension size is invalid");
                            else
                                defNode.StaticDimThreeSize = val;
                        }
                    }

                    if(!parser.PeekToken(TokenKind.RightBracket))
                        parser.ReportSyntaxError("Invalid end of static array dimension specifier.");
                    else
                        parser.NextToken();

                    if(!parser.PeekToken(TokenKind.OfKeyword))
                        parser.ReportSyntaxError("Missing \"of\" keyword in array definition.");
                    else
                        parser.NextToken();

                    // now try to get the datatype
                    TypeReference typeRef;
                    if ((TypeReference.TryParseNode(parser, out typeRef)))
                    {
                        defNode.Children.Add(typeRef.StartIndex, typeRef);
                        defNode.EndIndex = typeRef.EndIndex;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid type specified for static array definition");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid size specified for array.");
                }
            }
            return result;
        }
    }
}
