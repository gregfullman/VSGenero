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
using System.Threading.Tasks;
using VSGenero.SqlSupport;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public class DeclareStatement : FglStatement, IAnalysisResult
    {
        public string Identifier { get; private set; }
        public bool Scroll { get; private set; }
        public bool WithHold { get; private set; }

        public string PreparedStatementId { get; private set; }
        private Func<string, PrepareStatement> _prepStatementResolver;

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public bool IsPublic { get { return false; } }

        public static bool TryParseNode(Genero4glParser parser, out DeclareStatement defNode, IModuleResult containingModule)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.DeclareKeyword))
            {
                result = true;
                defNode = new DeclareStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._prepStatementResolver = containingModule.PreparedCursorResolver;

                if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                {
                    parser.NextToken();
                    defNode.Identifier = parser.Token.Token.Value.ToString();

                    if(parser.PeekToken(TokenKind.ScrollKeyword))
                    {
                        parser.NextToken();
                        defNode.Scroll = true;
                    }

                    if(parser.PeekToken(TokenKind.CursorKeyword))
                    {
                        parser.NextToken();

                        if(parser.PeekToken(TokenKind.WithKeyword))
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.HoldKeyword))
                            {
                                parser.NextToken();
                                defNode.WithHold = true;
                            }
                            else
                            {
                                parser.ReportSyntaxError("SQL declare statement missing \"hold\" keyword.");
                            }
                        }

                        if(parser.PeekToken(TokenKind.FromKeyword))
                        {
                            parser.NextToken();
                            // We have a string expression declare
                            ExpressionNode exprNode;
                            if(FglExpressionNode.TryGetExpressionNode(parser, out exprNode)/* && exprNode is StringExpressionNode*/ && exprNode != null)
                            {
                                defNode.Children.Add(exprNode.StartIndex, exprNode);
                                defNode.EndIndex = exprNode.EndIndex;

                                containingModule.BindCursorResult(defNode, parser);
                            }
                            else
                            {
                                parser.ReportSyntaxError("String expression not found for SQl declare statement");
                            }
                        }
                        else if(parser.PeekToken(TokenKind.ForKeyword))
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.SqlKeyword))
                            {
                                // we have a sql block declare
                                SqlBlockNode sqlBlock;
                                if(SqlBlockNode.TryParseSqlNode(parser, out sqlBlock) && sqlBlock != null)
                                {
                                    defNode.Children.Add(sqlBlock.StartIndex, sqlBlock);
                                    defNode.EndIndex = sqlBlock.EndIndex;
                                    defNode.IsComplete = true;
                                    containingModule.BindCursorResult(defNode, parser);
                                }
                            }
                            else if(parser.PeekToken(TokenKind.SelectKeyword))
                            {
                                // we have a static sql select statement
                                FglStatement sqlStmt;
                                bool dummy;
                                if (SqlStatementFactory.TryParseSqlStatement(parser, out sqlStmt, out dummy, TokenKind.SelectKeyword) && sqlStmt != null)
                                {
                                    defNode.Children.Add(sqlStmt.StartIndex, sqlStmt);
                                    defNode.EndIndex = sqlStmt.EndIndex;
                                    defNode.IsComplete = true;
                                    containingModule.BindCursorResult(defNode, parser);
                                }
                                else
                                {
                                    parser.ReportSyntaxError("Static SQL declare statement must specify a SELECT statement.");
                                }
                            }
                            else if(parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                            {
                                // we have a prepared statment
                                parser.NextToken();
                                defNode.PreparedStatementId = parser.Token.Token.Value.ToString();
                                defNode.EndIndex = parser.Token.Span.End;
                                defNode.IsComplete = true;
                                containingModule.BindCursorResult(defNode, parser);
                            }
                            else
                            {
                                parser.ReportSyntaxError("Invalid token found in SQL declare statment.");
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError("SQL declare statement must have either \"for\" or \"from\" keyword.");
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("SQL declare statement missing \"cursor\" keyword.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("SQL declare statement must specify an identifier to declare.");
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
            get { return Identifier; }
        }
        public override string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("declared cursor {0}", Name);
                if (!string.IsNullOrWhiteSpace(PreparedStatementId) && _prepStatementResolver != null)
                {
                    sb.AppendLine();
                    PrepareStatement prepStmt = _prepStatementResolver(PreparedStatementId);
                    if(prepStmt != null)
                    {
                        sb.Append(prepStmt.Documentation);
                    }
                    else
                    {
                        sb.Append("(unable to resolve prepared statement)");
                    }
                }
                else if(Children.Count == 1)
                {
                    StringExpressionNode strExpr = Children[Children.Keys[0]] as StringExpressionNode;
                    if(strExpr != null)
                    {
                        sb.AppendLine(" from:");
                        sb.AppendLine();
                        string formatted = SqlStatementExtractor.FormatSqlStatement(strExpr.LiteralValue);
                        if (formatted != null)
                            sb.Append(formatted);
                        else
                            sb.Append(strExpr.LiteralValue);
                    }
                }
                else
                {
                    sb.AppendLine();
                    sb.Append("(Documentation not supported at this time)");
                }
                return sb.ToString();
            }
        }


        public int LocationIndex
        {
            get { return StartIndex; }
        }

        public LocationInfo Location { get { return null; } }

        public IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry, bool function)
        {
            definingProject = null;
            projEntry = null;
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool function)
        {
            return null;
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public string Typename
        {
            get { return null; }
        }
    }
}
