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
    public class TypeReference : AstNode, IAnalysisResult
    {
        public AttributeSpecifier Attribute { get; private set; }

        private bool _isPublic;
        public bool IsPublic { get { return _isPublic; } }

        private string _typeNameString;
        public string DatabaseName { get; private set; }
        public string TableName { get; private set; }
        public string ColumnName { get; private set; }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

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

        public static bool TryParseNode(IParser parser, out TypeReference defNode, bool mimickingRecord = false, bool isPublic = false)
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
                defNode._isPublic = isPublic;
                defNode.Children.Add(arrayType.StartIndex, arrayType);
                defNode.IsComplete = true;
            }
            else if (RecordDefinitionNode.TryParseNode(parser, out recordDef, isPublic))
            {
                result = true;
                defNode = new TypeReference();
                defNode.StartIndex = recordDef.StartIndex;
                defNode.EndIndex = recordDef.EndIndex;
                defNode._isPublic = isPublic;
                defNode.Children.Add(recordDef.StartIndex, recordDef);
                defNode.IsComplete = true;
            }
            else if (parser.PeekToken(TokenKind.LikeKeyword))
            {
                result = true;
                parser.NextToken();
                defNode = new TypeReference();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._isPublic = isPublic;

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
                    if(parser.Token.Token.Kind == TokenKind.Dot)
                    {
                        if (parser.PeekToken(TokenKind.Multiply) ||
                            parser.PeekToken(TokenCategory.Identifier) ||
                            parser.PeekToken(TokenCategory.Keyword))
                        {
                            parser.NextToken(); // advance to the column name
                            defNode.ColumnName = parser.Token.Token.Value.ToString();
                            if (!mimickingRecord && defNode.ColumnName == "*")
                            {
                                parser.ReportSyntaxError("A variable cannot mimic an entire table without being a record. The variable must be defined as a mimicking record.");
                            }
                            defNode.IsComplete = true;
                            defNode.EndIndex = parser.Token.Span.End;
                        }
                        else
                        {
                            if (mimickingRecord)
                                parser.ReportSyntaxError("A mimicking variable must use the format \"like table.*\"");
                            else
                                parser.ReportSyntaxError("A mimicking variable must use the format \"like table.column\"");
                        }
                    }
                    else
                    {
                        if (mimickingRecord)
                            parser.ReportSyntaxError("A mimicking variable must use the format \"like table.*\"");
                        else
                            parser.ReportSyntaxError("A mimicking variable must use the format \"like table.column\"");
                    }
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

        public string Name
        {
            get { return ToString(); }
        }

        public int LocationIndex
        {
            get { return StartIndex; }
        }

        public LocationInfo Location { get { return null; } }

        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            if (!string.IsNullOrWhiteSpace(TableName))
            {
                // TODO: get the specified column from the database provider
                return null;
            }
            else
            {
                // TODO: there's probably a better way to do this
                return GetAnalysisMembers(ast).Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }
        }

        internal IEnumerable<IAnalysisResult> GetAnalysisMembers(GeneroAst ast)
        {
            List<IAnalysisResult> members = new List<IAnalysisResult>();
            if (Children.Count == 1)
            {
                // we have an array type or a record type definition
                var node = Children[Children.Keys[0]];
                if (node is ArrayTypeReference)
                {
                    return (node as ArrayTypeReference).GetAnalysisResults(ast);
                }
                else if (node is RecordDefinitionNode)
                {
                    return (node as RecordDefinitionNode).GetAnalysisResults(ast);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(TableName))
                {
                    // TODO: return the table's columns
                    int i = 0;
                }
                else if (_typeNameString.Equals("string", StringComparison.OrdinalIgnoreCase))
                {
                    return GeneroAst.StringFunctions.Values;
                }
                else
                {
                    // try to determine if the _typeNameString is a user defined type, in which case we need to call its GetMembers function
                    IAnalysisResult udt = ast.TryGetUserDefinedType(_typeNameString, LocationIndex);
                    if (udt != null)
                    {
                        return udt.GetMembers(ast).Select(x => x.Var).Where(y => y != null);
                    }
                    
                    // try to get the _typeNameString from types available in imported modules
                    if(ast.ProjectEntry.ParentProject.ReferencedProjects.Count > 0)
                    {
                        foreach(var refProj in ast.ProjectEntry.ParentProject.ReferencedProjects.Values)
                        {
                            if(refProj is GeneroProject)
                            {
                                udt = (refProj as GeneroProject).GetMemberOfType(_typeNameString, ast, false, true, false, false);
                                if(udt != null)
                                {
                                    return udt.GetMembers(ast).Select(x => x.Var).Where(y => y != null);
                                }
                            }
                        }
                    }

                    // check for package class
                    udt = ast.GetValueByIndex(_typeNameString, LocationIndex, ast._functionProvider, ast._databaseProvider);
                    if (udt != null)
                    {
                        return udt.GetMembers(ast).Select(x => x.Var).Where(y => y != null);
                    }
                }
            }
            return members;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            List<MemberResult> members = new List<MemberResult>();
            if (Children.Count == 1)
            {
                // we have an array type or a record type definition
                var node = Children[Children.Keys[0]];
                if (node is ArrayTypeReference)
                {
                    return (node as ArrayTypeReference).GetMembersInternal(ast);
                }
                else if (node is RecordDefinitionNode)
                {
                    return (node as RecordDefinitionNode).GetMembers(ast);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(TableName))
                {
                    // TODO: return the table's columns
                }
                else if (_typeNameString.Equals("string", StringComparison.OrdinalIgnoreCase))
                {
                    return GeneroAst.StringFunctions.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Method, ast));
                }
                else
                {
                    // try to determine if the _typeNameString is a user defined type (or package class), in which case we need to call its GetMembers function
                    IAnalysisResult udt = ast.TryGetUserDefinedType(_typeNameString, LocationIndex);
                    if (udt != null)
                    {
                        return udt.GetMembers(ast);
                    }

                    if (ast.ProjectEntry.ParentProject.ReferencedProjects.Count > 0)
                    {
                        foreach (var refProj in ast.ProjectEntry.ParentProject.ReferencedProjects.Values)
                        {
                            if (refProj is GeneroProject)
                            {
                                udt = (refProj as GeneroProject).GetMemberOfType(_typeNameString, ast, false, true, false, false);
                                if (udt != null)
                                {
                                    return udt.GetMembers(ast);
                                }
                            }
                        }
                    }

                    // check for package class
                    udt = ast.GetValueByIndex(_typeNameString, LocationIndex, ast._functionProvider, ast._databaseProvider);
                    if (udt != null)
                    {
                        return udt.GetMembers(ast);
                    }
                }
            }
            return members;
        }


        public bool HasChildFunctions(GeneroAst ast)
        {
            if (Children.Count == 1)
            {
                var node = Children[Children.Keys[0]];
                if (node is ArrayTypeReference)
                {
                    return true;
                }
                else if (node is RecordDefinitionNode)
                {
                    return (node as RecordDefinitionNode).HasChildFunctions(ast);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(_typeNameString))
                {
                    if (_typeNameString.Equals("string", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        // try to determine if the _typeNameString is a user defined type
                        IAnalysisResult udt = ast.TryGetUserDefinedType(_typeNameString, LocationIndex);
                        if (udt != null)
                        {
                            return udt.HasChildFunctions(ast);
                        }

                        // check for package class
                        udt = ast.GetValueByIndex(_typeNameString, LocationIndex, ast._functionProvider, ast._databaseProvider);
                        if(udt != null)
                        {
                            return udt.HasChildFunctions(ast);
                        }
                    }
                }
            }
            return false;
        }
    }
}
