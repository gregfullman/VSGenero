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

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    /// <summary>
    /// Types of create statements:
    /// 1) sequence
    /// 2) table
    /// 3) index
    /// 4) view
    /// 5) synonym
    /// </summary>
    public class CreateStatement : FglStatement
    {
        public static bool TryParseNode(Genero4glParser parser, out CreateStatement node, IModuleResult containingModule)
        {
            node = null;
            bool result = true;

            if(parser.PeekToken(TokenKind.CreateKeyword))
            {
                parser.NextToken();
                switch(parser.PeekToken().Kind)
                {
                    case TokenKind.SequenceKeyword:
                        CreateSequenceStatement sequenceNode;
                        result = CreateSequenceStatement.TryParseNode(parser, out sequenceNode);
                        node = sequenceNode;
                        break;
                    case TokenKind.TableKeyword:
                    case TokenKind.TempKeyword:
                        CreateTableStatement tableNode;
                        result = CreateTableStatement.TryParseNode(parser, out tableNode, containingModule);
                        node = tableNode;
                        break;
                    default:
                        result = false;
                        break;
                }
            }

            return result;
        }
    }

    public class CreateTableStatement : CreateStatement, IAnalysisResult
    {
        public FglNameExpression TableName { get; private set; }
        public bool TempTable { get; private set; }
        public FglNameExpression TablespaceName { get; private set; }
        public ExpressionNode ExtentSize { get; private set; }
        public ExpressionNode NextSize { get; private set; }

        internal static bool TryParseNode(Genero4glParser parser, out CreateTableStatement node, IModuleResult containingModule)
        {
            node = null;
            bool result = false;
            bool temp = false;

            int tempIndex = -1;
            if(parser.PeekToken(TokenKind.TempKeyword))
            {
                tempIndex = parser.Token.Span.Start;
                temp = true;
                parser.NextToken();
            }

            if (parser.PeekToken(TokenKind.TableKeyword))
            {
                result = true;
                node = new CreateTableStatement();
                node.TempTable = temp;
                if (tempIndex >= 0)
                    node.StartIndex = tempIndex;
                else
                    node.StartIndex = parser.Token.Span.Start;
                parser.NextToken();

                if (parser.PeekToken(TokenKind.IfKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.NotKeyword))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.ExistsKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting \"exists\" keyword in create table statement.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting \"not\" keyword in create table statement.");
                }

                FglNameExpression nameExpr;
                if (FglNameExpression.TryParseNode(parser, out nameExpr))
                    node.TableName = nameExpr;
                else
                    parser.ReportSyntaxError("Invalid name found for create table statement.");

                if (parser.PeekToken(TokenKind.LeftParenthesis))
                {
                    parser.NextToken();
                    CreatedTableColumn tableCol;
                    while(CreatedTableColumn.TryParseNode(parser, out tableCol) && tableCol != null)
                    {
                        node.Children.Add(tableCol.StartIndex, tableCol);
                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                        else
                            break;
                    }

                    if (parser.PeekToken(TokenKind.RightParenthesis))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expected right-paren in create table statement.");

                    if(parser.PeekToken(TokenKind.WithKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.NoKeyword))
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.LogKeyword))
                                parser.NextToken();
                            else
                                parser.ReportSyntaxError("Expecting \"log\" keyword in create table statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"no\" keyword in create table statement.");
                    }

                    if(parser.PeekToken(TokenKind.InKeyword))
                    {
                        parser.NextToken();
                        if (FglNameExpression.TryParseNode(parser, out nameExpr))
                            node.TablespaceName = nameExpr;
                        else
                            parser.ReportSyntaxError("Invalid name found for create table statement.");
                    }

                    if(parser.PeekToken(TokenKind.ExtentKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.SizeKeyword))
                        {
                            parser.NextToken();
                            ExpressionNode extSize;
                            if (FglExpressionNode.TryGetExpressionNode(parser, out extSize))
                                node.ExtentSize = extSize;
                            else
                                parser.ReportSyntaxError("Invalid expression found for extent size in create table statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"size\" keyword in create table statement.");
                    }

                    if (parser.PeekToken(TokenKind.NextKeyword))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.SizeKeyword))
                        {
                            parser.NextToken();
                            ExpressionNode extSize;
                            if (FglExpressionNode.TryGetExpressionNode(parser, out extSize))
                                node.NextSize = extSize;
                            else
                                parser.ReportSyntaxError("Invalid expression found for next size in create table statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"size\" keyword in create table statement.");
                    }

                    if(parser.PeekToken(TokenKind.LockKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.ModeKeyword))
                        {
                            parser.NextToken();
                            switch(parser.PeekToken().Kind)
                            {
                                case TokenKind.PageKeyword:
                                    parser.NextToken();
                                    break;
                                case TokenKind.RowKeyword:
                                    parser.NextToken();
                                    break;
                                default:
                                    parser.ReportSyntaxError("Expecting \"page\" or \"row\" keyword in create table statement.");
                                    break;
                            }
                        }
                        else
                            parser.ReportSyntaxError("Expecting \"mode\" keyword in create table statement.");
                    }
                }
                else
                    parser.ReportSyntaxError("Expected left-paren in create table statement.");

                containingModule.BindTableResult(node, parser);
            }
            return result;
        }

        public string Scope
        {
            get
            {
                return string.Format("{0}table", TempTable ? "temp " : "");
            }
            set
            {
            }
        }

        public string Name
        {
            get 
            {
                if (TableName == null)
                    return null;
                return TableName.Name; 
            }
        }

        public int LocationIndex
        {
            get { return StartIndex; }
        }

        private LocationInfo _location = null;
        public LocationInfo Location
        {
            get { return _location; }
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public bool IsPublic
        {
            get { return true; }
        }

        public string Typename
        {
            get { return null; }
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

        public IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projectEntry, bool function)
        {
            definingProject = null;
            projectEntry = null;
            return this.Children.Values.Cast<CreatedTableColumn>().FirstOrDefault(x => x.ColumnName.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool getArrayTypeMembers)
        {
            return this.Children.Values.Cast<CreatedTableColumn>().Select(x => new MemberResult(x.ColumnName.Name, x, GeneroMemberType.DbColumn, SyntaxTree));
        }

        public override void PropagateSyntaxTree(GeneroAst ast)
        {
            _location = ast.ResolveLocation(this);
            base.PropagateSyntaxTree(ast);
        }
    }

    public class CreatedTableColumn : AstNode4gl, IAnalysisResult
    {
        public FglNameExpression ColumnName { get; private set; }
        public TypeReference DataType { get; private set; }

        public ExpressionNode DefaultValue { get; private set; }
        public bool NotNull { get; private set; }
        public FglNameExpression ConstraintName { get; private set; }
        public FglNameExpression ReferencingTableName { get; private set; }
        public List<FglNameExpression> ReferencingTableColumnNames { get; private set; }

        public List<FglNameExpression> UniqueColumns { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out CreatedTableColumn node)
        {
            node = new CreatedTableColumn();
            node.StartIndex = parser.Token.Span.Start;
            node.UniqueColumns = new List<FglNameExpression>();
            node.ReferencingTableColumnNames = new List<FglNameExpression>();

            bool result = true;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.UniqueKeyword:
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            FglNameExpression nameExpr;
                            while(FglNameExpression.TryParseNode(parser, out nameExpr))
                            {
                                node.UniqueColumns.Add(nameExpr);
                                if (parser.PeekToken(TokenKind.Comma))
                                    parser.NextToken();
                                else
                                    break;
                            }
                            if (parser.PeekToken(TokenKind.RightParenthesis))
                                parser.NextToken();
                            else
                                parser.ReportSyntaxError("Expected right-paren in create table statement.");
                        }
                        else
                            parser.ReportSyntaxError("Expected left-paren in create table statement.");
                        break;
                    }
                case TokenKind.PrimaryKeyword:
                case TokenKind.CheckKeyword:
                case TokenKind.ForeignKeyword:
                    result = false;
                    parser.ReportSyntaxError("Token not supported at this time for create table statements.");
                    break;
                default:
                    {
                        FglNameExpression colName;
                        if (FglNameExpression.TryParseNode(parser, out colName))
                            node.ColumnName = colName;
                        else
                            parser.ReportSyntaxError("Invalid name found for table column in create table statement.");

                        TypeReference dataType;
                        if (TypeReference.TryParseNode(parser, out dataType))
                        {
                            // TODO: should probably ensure that only SQL compatible types are used here...
                            node.DataType = dataType;
                        }
                        else
                            parser.ReportSyntaxError("Invalid data-type found for table column in create table statement.");

                        if(parser.PeekToken(TokenKind.DefaultKeyword))
                        {
                            parser.NextToken();
                            ExpressionNode defVal;
                            if (FglExpressionNode.TryGetExpressionNode(parser, out defVal))
                                node.DefaultValue = defVal;
                            else
                                parser.ReportSyntaxError("Invalid expression found for default value in create table statement.");
                        }

                        if(parser.PeekToken(TokenKind.NotKeyword))
                        {
                            parser.NextToken();
                            if(parser.PeekToken(TokenKind.NullKeyword))
                            {
                                parser.NextToken();
                                node.NotNull = true;
                            }
                            else
                                parser.ReportSyntaxError("Expecting \"null\" keyword in create table statement.");
                        }

                        // TODO: do the other modifiers (primary key, unique, check, references)
                    }
                    break;
            }

            return result;
        }

        public string Scope
        {
            get
            {
                return "table column";
            }
            set
            {
            }
        }

        public string Name
        {
            get { return ColumnName.Name; }
        }

        public int LocationIndex
        {
            get { return StartIndex; }
        }

        private LocationInfo _location = null;
        public LocationInfo Location
        {
            get { return _location; }
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public bool IsPublic
        {
            get { return true; }
        }

        public string Typename
        {
            get { return null; }
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

        public IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projectEntry, bool function)
        {
            definingProject = null;
            projectEntry = null;
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool getArrayTypeMembers)
        {
            return new MemberResult[0];
        }
    }

    public class CreateSequenceStatement : CreateStatement
    {
        public FglNameExpression SequenceName { get; private set; }

        internal static bool TryParseNode(Genero4glParser parser, out CreateSequenceStatement node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.SequenceKeyword))
            {
                result = true;
                node = new CreateSequenceStatement();
                node.StartIndex = parser.Token.Span.Start; // want to get the start of the create token
                parser.NextToken();

                if(parser.PeekToken(TokenKind.IfKeyword))
                {
                    parser.NextToken();
                    if(parser.PeekToken(TokenKind.NotKeyword))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.ExistsKeyword))
                            parser.NextToken();
                        else
                            parser.ReportSyntaxError("Expecting \"exists\" keyword in create sequence statement.");
                    }
                    else
                        parser.ReportSyntaxError("Expecting \"not\" keyword in create sequence statement.");
                }

                FglNameExpression nameExpr;
                if (FglNameExpression.TryParseNode(parser, out nameExpr))
                    node.SequenceName = nameExpr;
                else
                    parser.ReportSyntaxError("Invalid name found for create sequence statement.");

                // TODO: finish getting the modifiers
            }

            return result;
        }
    }
}
