/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGenero.Analysis
{
    public enum MemberType
    {
        None = 0,
        Variables = 1,
        Types = 2,
        Constants = 4,
        Functions = 8,
        All = 15
    }

    public interface IAnalysisResult
    {
        string Scope { get; set; }
        string Name { get; }
        string Documentation { get; }
        int LocationIndex { get; }
        LocationInfo Location { get; }
        bool HasChildFunctions(Genero4glAst ast);
        bool CanGetValueFromDebugger { get; }
        bool IsPublic { get; }
        string Typename { get; }
        GeneroLanguageVersion MinimumLanguageVersion { get; }
        GeneroLanguageVersion MaximumLanguageVersion { get; }

        IAnalysisResult GetMember(string name, Genero4glAst ast, out IGeneroProject definingProject, out IProjectEntry projectEntry, bool function);
        IEnumerable<MemberResult> GetMembers(Genero4glAst ast, MemberType memberType, bool function);
    }

    public enum DatabaseTableType
    {
        Table,
        View
    }

    public interface IDbTableResult : IAnalysisResult
    {
        DatabaseTableType TableType { get; }
    }

    public interface IAnalysisResultContainer
    { 
    }

    public interface IModuleResult : IAnalysisResultContainer
    {
        string ProgramName { get; }

        List<string> CExtensionImports { get; }
        HashSet<string> FglImports { get; }
        IDictionary<string, IAnalysisResult> Variables { get; }
        IDictionary<string, IAnalysisResult> Types { get; }
        IDictionary<string, IAnalysisResult> Constants { get; }
        IDictionary<string, IFunctionResult> Functions { get; }
        IDictionary<string, IAnalysisResult> Cursors { get; }
        IDictionary<string, IAnalysisResult> Tables { get; }
        IDictionary<string, IAnalysisResult> GlobalVariables { get; }
        IDictionary<string, IAnalysisResult> GlobalTypes { get; }
        IDictionary<string, IAnalysisResult> GlobalConstants { get; }

        PrepareStatement PreparedCursorResolver(string prepIdent);
        void BindCursorResult(IAnalysisResult cursorResult, IParser parser);
        void BindTableResult(IAnalysisResult tableResult, IParser parser);
    }

    public interface IFunctionResult : IAnalysisResult, IAnalysisResultContainer, IOutlinableResult
    {
        ParameterResult[] Parameters { get; }
        string[] Returns { get; }
        AccessModifier AccessModifier { get; }
        string FunctionDocumentation { get; }
        IDictionary<string, IAnalysisResult> Variables { get; }
        IDictionary<string, IAnalysisResult> Types { get; }
        IDictionary<string, IAnalysisResult> Constants { get; }
        IDictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> LimitedScopeVariables { get; }
        string CompletionParentName { get; }

        void SetCommentDocumentation(string commentDoc);

        GeneroMemberType FunctionType { get; }
    }

    public interface IOutlinableResult
    {
        bool CanOutline { get; }
        int StartIndex { get; set; }
        int EndIndex { get; set; }
        int DecoratorStart { get; set; }
        int DecoratorEnd { get; set; }

        SortedList<int, int> AdditionalDecoratorRanges { get; }
    }
}
