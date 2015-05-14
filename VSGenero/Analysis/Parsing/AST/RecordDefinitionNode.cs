using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// identifier RECORD [<see cref="AttributeSpecifier"/>]
    ///   member <see cref="TypeReference"/> [<see cref="AttributeSpecifier"/>]
    ///   [,...] 
    /// END RECORD
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_records_002.html
    /// </summary>
    public class RecordDefinitionNode : AstNode, IAnalysisResult
    {
        public AttributeSpecifier Attribute { get; private set; }
        public string MimicTableName { get; private set; }
        public string MimicDatabaseName { get; private set; }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        private bool _isPublic;
        public bool IsPublic { get { return _isPublic; } }

        private Dictionary<string, VariableDef> _memberDictionary;
        public Dictionary<string, VariableDef> MemberDictionary
        {
            get
            {
                if (_memberDictionary == null)
                    _memberDictionary = new Dictionary<string, VariableDef>(StringComparer.OrdinalIgnoreCase);
                return _memberDictionary;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("record");

            if (!string.IsNullOrWhiteSpace(MimicTableName))
            {
                sb.AppendFormat(" like {0}.*", MimicTableName);
            }

            return sb.ToString();
        }

        public new static bool TryParseNode(IParser parser, out RecordDefinitionNode defNode, bool isPublic = false)
        {
            defNode = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.RecordKeyword))
            {
                result = true;
                defNode = new RecordDefinitionNode();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._isPublic = isPublic;
                parser.NextToken();     // move past the record keyword
                if (parser.PeekToken(TokenKind.LikeKeyword))
                {
                    // get db info
                    parser.NextToken();
                    if (!parser.PeekToken(TokenCategory.Identifier) && parser.PeekToken(TokenKind.Colon, 2))
                    {
                        parser.NextToken(); // advance to the database name
                        defNode.MimicDatabaseName = parser.Token.Token.Value.ToString();
                        parser.NextToken(); // advance to the colon
                    }
                    if (!parser.PeekToken(TokenCategory.Identifier))
                    {
                        parser.ReportSyntaxError("Database table name expected.");
                    }
                    else if (!parser.PeekToken(TokenKind.Dot, 2) && !parser.PeekToken(TokenKind.Multiply))
                    {
                        parser.ReportSyntaxError("A mimicking record must reference a table as follows: \"[tablename].*\".");
                    }
                    else
                    {
                        parser.NextToken(); // advance to the table name
                        defNode.MimicTableName = parser.Token.Token.Value.ToString();
                        parser.NextToken(); // advance to the dot
                        parser.NextToken(); // advance to the star
                    }
                }
                else
                {
                    AttributeSpecifier attribSpec;
                    if (AttributeSpecifier.TryParseNode(parser, out attribSpec))
                    {
                        defNode.Attribute = attribSpec;
                    }

                    bool advance = true;
                    TokenWithSpan tok = default(TokenWithSpan);
                    while (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                    {
                        //if (advance)
                        //{

                        parser.NextToken();
                        tok = parser.Token;
                        //}
                        //else
                        //{
                        //    // reset
                        //    tok = parser.Token.Token;
                        //    advance = true;
                        //}

                        TypeReference tr;
                        if (TypeReference.TryParseNode(parser, out tr, true))
                        {
                            if (!defNode.MemberDictionary.ContainsKey(tok.Token.Value.ToString()))
                                defNode.MemberDictionary.Add(tok.Token.Value.ToString(), new VariableDef(tok.Token.Value.ToString(), tr, tok.Span.Start, true));
                            else
                                parser.ReportSyntaxError(string.Format("Record field {0} defined more than once.", tok.Token.Value.ToString()));
                        }

                        if (parser.MaybeEat(TokenKind.Comma))
                        {
                            //advance = false;
                            continue;
                        }
                        else if (parser.MaybeEat(TokenKind.EndKeyword))
                        {
                            if (!parser.MaybeEat(TokenKind.RecordKeyword))
                            {
                                parser.ReportSyntaxError("Invalid end token in record definition");
                            }
                            else
                            {
                                defNode.EndIndex = parser.Token.Span.End;
                                defNode.IsComplete = true;
                            }
                            break;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Invalid token within record definition");
                            break;
                        }
                    }
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
            get { return null; }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location { get { return null; } }

        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            if (MemberDictionary.Count == 0 && !string.IsNullOrEmpty(MimicTableName))
            {
                return null;
            }
            else
            {
                VariableDef varDef = null;
                MemberDictionary.TryGetValue(name, out varDef);
                return varDef;
            }
        }

        internal IEnumerable<IAnalysisResult> GetAnalysisResults(GeneroAst ast)
        {
            if (MemberDictionary.Count == 0 && !string.IsNullOrEmpty(MimicTableName))
            {
                // get the table's columns
                return ast._databaseProvider.GetColumns(MimicTableName);
            }
            else
            {
                return MemberDictionary.Values;
            }
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            if (MemberDictionary.Count == 0 && !string.IsNullOrEmpty(MimicTableName))
            {
                // get the table's columns
                return ast._databaseProvider.GetColumns(MimicTableName).Select(x => new MemberResult(x.Name, x, GeneroMemberType.DbColumn, ast));
            }
            else
            {
                return MemberDictionary.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Variable, ast));
            }
        }


        public bool HasChildFunctions(GeneroAst ast)
        {
            return MemberDictionary.Values.Any(x => x.Type.HasChildFunctions(ast));
        }
    }
}
