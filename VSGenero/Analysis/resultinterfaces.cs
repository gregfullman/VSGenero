using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IAnalysisResult
    {
        string Scope { get; set; }
        string Name { get; }
        string Documentation { get; }
    }

    public interface IAnalysisResultContainer
    { 
    }

    public interface IModuleResult : IAnalysisResultContainer, IGlobalsResult
    {
        IDictionary<string, IAnalysisResult> Variables { get; }
        IDictionary<string, IAnalysisResult> Types { get; }
        IDictionary<string, IAnalysisResult> Constants { get; }
        IDictionary<string, IFunctionResult> Functions { get; }
        IDictionary<string, IAnalysisResult> Cursors { get; }
    }

    public interface IGlobalsResult
    {
        IDictionary<string, IAnalysisResult> GlobalVariables { get; }
        IDictionary<string, IAnalysisResult> GlobalTypes { get; }
        IDictionary<string, IAnalysisResult> GlobalConstants { get; }
    }

    public interface IFunctionResult : IAnalysisResult, IAnalysisResultContainer, IOutlinableResult
    {
        ParameterResult[] Parameters { get; }
        AccessModifier AccessModifier { get; }
        string FunctionDocumentation { get; }
        IDictionary<string, IAnalysisResult> Variables { get; }
        IDictionary<string, IAnalysisResult> Types { get; }
        IDictionary<string, IAnalysisResult> Constants { get; }
    }

    public interface IOutlinableResult
    {
        bool CanOutline { get; }
        int StartIndex { get; set; }
        int EndIndex { get; set; }
        int DecoratorEnd { get; set; }
    }
}
