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
        public NameExpression MimicTableName { get; private set; }
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

            if (MimicTableName != null)
            {
                sb.AppendFormat(" like {0}", MimicTableName.Name);
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
                    NameExpression tableName;
                    if (NameExpression.TryParseNode(parser, out tableName))
                    {
                        defNode.MimicTableName = tableName;
                        if(!tableName.Name.EndsWith(".*"))
                            parser.ReportSyntaxError("A mimicking record must reference a table as follows: \"[tablename].*\".");
                    }
                    else
                        parser.ReportSyntaxError("Invalid database table name found in record definition.");
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
                                parser.ReportSyntaxError(string.Format("Record field {0} defined more than once.", tok.Token.Value.ToString()), Severity.Error);
                        }

                        AttributeSpecifier.TryParseNode(parser, out attribSpec);

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

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry, bool function)
        {
            definingProject = null;
            projEntry = null;
            if (MemberDictionary.Count == 0 && MimicTableName != null)
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
            if (MemberDictionary.Count == 0 && MimicTableName != null && ast._databaseProvider != null)
            {
                // get the table's columns
                return ast._databaseProvider.GetColumns(MimicTableName.Name);
            }
            else
            {
                return MemberDictionary.Values;
            }
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType, bool function)
        {
            if (MemberDictionary.Count == 0 && MimicTableName != null)
            {
                // get the table's columns
                if (ast._databaseProvider != null)
                {
                    return ast._databaseProvider.GetColumns(MimicTableName.Name).Select(x => new MemberResult(x.Name, x, GeneroMemberType.DbColumn, ast));
                }
            }
            else
            {
                return MemberDictionary.Values.Select(x => new MemberResult(x.Name, x, GeneroMemberType.Variable, ast));
            }
            return new MemberResult[0];
        }


        public bool HasChildFunctions(GeneroAst ast)
        {
            return MemberDictionary.Values.Any(x => x.Type.HasChildFunctions(ast));
        }

        public string Typename
        {
            get { return null; }
        }

        public override void CheckForErrors(GeneroAst ast, Action<string, int, int> errorFunc, Dictionary<string, List<int>> deferredFunctionSearches, GeneroAst.FunctionProviderSearchMode searchInFunctionProvider = GeneroAst.FunctionProviderSearchMode.NoSearch, bool isFunctionCallOrDefinition = false)
        {
            if(MemberDictionary.Count > 0)
            {
                foreach (var recChild in MemberDictionary.Values)
                    recChild.Type.CheckForErrors(ast, errorFunc, deferredFunctionSearches, searchInFunctionProvider, isFunctionCallOrDefinition);
            }
            else
            {
                // TODO: check the database and table (deferred?)
            }

            // we don't store any children off, so this does nothing
            base.CheckForErrors(ast, errorFunc, deferredFunctionSearches, searchInFunctionProvider, isFunctionCallOrDefinition);
        }
    }
}
