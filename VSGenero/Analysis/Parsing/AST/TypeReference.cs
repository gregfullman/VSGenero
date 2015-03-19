using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// Format:
    /// {
    ///     datatype
    ///     |
    ///     LIKE [dbname:]tabname.colname
    /// }
    /// 
    /// For more info, see http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_user_types_003.html
    /// or http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_DEFINE.html
    /// </summary>
    public class TypeReference : AstNode
    {
        public AttributeSpecifier Attribute { get; private set; }

        private string _typeNameString;
        public string DatabaseName { get; private set; }
        public string TableName { get; private set; }
        public string ColumnName { get; private set; }

        public override string ToString()
        {
            if (Children.Count > 0)
            {
                if (Children.Count == 1)
                {
                    return Children[Children.Keys[0]].ToString();
                }
                else
                {
                    // TODO: this should never happen
                    return null;
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(TableName) && !string.IsNullOrWhiteSpace(ColumnName))
                {
                    // TODO: we have two options here...not sure what to do
                    // 1) Look up the type from a database provider
                    // 2) Just list the mimicking "table.column"
                    // For right now, we'll go with #2
                    sb.AppendFormat("like {0}.{1}", TableName, ColumnName);
                }
                else
                {
                    return _typeNameString;
                }

                return sb.ToString();
            }
        }

        public static bool TryParseNode(IParser parser, out TypeReference defNode, bool mimickingRecord = false)
        {
            defNode = null;
            bool result = false;

            ArrayTypeReference arrayType;
            RecordDefinitionNode recordDef;
            if (ArrayTypeReference.TryParseNode(parser, out arrayType))
            {
                result = true;
                defNode = new TypeReference();
                defNode.StartIndex = arrayType.StartIndex;
                defNode.EndIndex = arrayType.EndIndex;
                defNode.Children.Add(arrayType.StartIndex, arrayType);
                defNode.IsComplete = true;
            }
            else if (RecordDefinitionNode.TryParseNode(parser, out recordDef))
            {
                result = true;
                defNode = new TypeReference();
                defNode.StartIndex = recordDef.StartIndex;
                defNode.EndIndex = recordDef.EndIndex;
                defNode.Children.Add(recordDef.StartIndex, recordDef);
                defNode.IsComplete = true;
            }
            else if (parser.PeekToken(TokenKind.LikeKeyword))
            {
                result = true;
                parser.NextToken();
                defNode = new TypeReference();
                defNode.StartIndex = parser.Token.Span.Start;

                // get db info
                if (!parser.PeekToken(TokenCategory.Identifier) && parser.PeekToken(TokenKind.Colon, 2))
                {
                    parser.NextToken(); // advance to the database name
                    defNode.DatabaseName = parser.Token.Token.Value.ToString();
                    parser.NextToken(); // advance to the colon
                }
                if (!parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.ReportSyntaxError("Database table name expected.");
                }
                else if (!parser.PeekToken(TokenKind.Dot, 2) && !parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.ReportSyntaxError("A mimicking type must reference a table as follows: \"[tablename].[recordname]\".");
                }
                else
                {
                    parser.NextToken(); // advance to the table name
                    defNode.TableName = parser.Token.Token.Value.ToString();
                    parser.NextToken(); // advance to the dot
                    parser.NextToken(); // advance to the columne name (or dot)
                    defNode.ColumnName = parser.Token.Token.Value.ToString();
                    if (!mimickingRecord && defNode.ColumnName == "*")
                    {
                        parser.ReportSyntaxError("A variable cannot mimic an entire table without being a record. The variable must be defined as a mimicking record.");
                    }
                    defNode.IsComplete = true;
                    defNode.EndIndex = parser.Token.Span.End;
                }
            }
            else
            {
                var tok = parser.PeekToken();
                var cat = Tokenizer.GetTokenInfo(tok).Category;
                if (cat == TokenCategory.Keyword || cat == TokenCategory.Identifier)
                {
                    StringBuilder sb = new StringBuilder();
                    result = true;
                    parser.NextToken();
                    defNode = new TypeReference();
                    defNode.StartIndex = parser.Token.Span.Start;
                    sb.Append(parser.Token.Token.Value.ToString());
                    
                    // determine if there are any constraints on the type keyword
                    string typeString;
                    if (TypeConstraints.VerifyValidConstraint(parser, out typeString))
                    {
                        defNode._typeNameString = typeString;
                    }
                    else
                    {
                        // see if we're referencing an extension type (dotted name)
                        while (parser.PeekToken(TokenKind.Dot))
                        {
                            sb.Append(parser.NextToken().Value.ToString());
                            if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                            {
                                sb.Append(parser.NextToken().Value.ToString());
                            }
                            else
                            {
                                parser.ReportSyntaxError("Unexpected token in type reference.");
                            }
                        }
                        defNode._typeNameString = sb.ToString();
                    }

                    AttributeSpecifier attribSpec;
                    if (AttributeSpecifier.TryParseNode(parser, out attribSpec))
                    {
                        defNode.Attribute = attribSpec;
                    }

                    defNode.EndIndex = parser.Token.Span.End;
                    defNode.IsComplete = true;
                }
                else if (cat == TokenCategory.EndOfStream)
                {
                    parser.ReportSyntaxError("Unexpected end of type definition");
                    result = false;
                }
            }

            return result;
        }
    }
}
