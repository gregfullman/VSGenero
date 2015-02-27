using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
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
        public ArrayType ArrayType { get; private set; }
        public uint DynamicArrayDimension { get; private set; }
        public uint StaticDimOneSize { get; private set; }
        public uint StaticDimTwoSize { get; private set; }
        public uint StaticDimThreeSize { get; private set; }

        public static bool TryParseNode(Parser parser, out ArrayTypeReference defNode)
        {
            defNode = null;
            bool result = false;
            if(parser.PeekToken(TokenKind.DynamicKeyword))
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

                // [WITH DIMENSION rank] is optional
                if(parser.PeekToken(TokenKind.WithKeyword))
                {
                    parser.NextToken();
                    if(parser.PeekToken(TokenKind.DimensionKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenCategory.NumericLiteral))
                        {
                            var tok = parser.NextToken() as ConstantValueToken;
                            if(tok != null)
                            {
                                uint intVal;
                                string val = tok.Value.ToString();
                                if(uint.TryParse(val, out intVal))
                                {
                                    if(intVal >= 1 && intVal <= 3)
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

                if(!parser.PeekToken(TokenKind.OfKeyword))
                    parser.ReportSyntaxError("Missing \"of\" keyword in array definition.");
                else
                    parser.NextToken();

                // now try to get the datatype
                TypeReference typeRef;
                if((TypeReference.TryParseNode(parser, out typeRef)))
                {
                    defNode.Children.Add(typeRef.StartIndex, typeRef);
                    defNode.EndIndex = typeRef.EndIndex;
                }
                else
                {
                    parser.ReportSyntaxError("Invalid type specified for dynamic array definition");
                }
            }
            else if(parser.PeekToken(TokenKind.ArrayKeyword))
            {
                result = true;
                defNode = new ArrayTypeReference();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                if (!parser.PeekToken(TokenKind.LeftBracket))
                    parser.ReportSyntaxError("A non-dynamic array definition must have brackets.");
                else
                    parser.NextToken();

                if(parser.PeekToken(TokenKind.RightBracket))
                {
                    // we should have a java array
                    defNode.ArrayType = ArrayType.Java;
                }
                else if(parser.PeekToken(TokenCategory.NumericLiteral))
                {
                    defNode.ArrayType = ArrayType.Static;
                    parser.NextToken();


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
