using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    /// <summary>
    /// This interface allows an external extension to provide additional functions for the analysis context.
    /// Functions are provided by dotted access (i.e. {Name}.{Collection}.{FunctionName})
    /// </summary>
    public interface IFunctionInformationProvider : IAnalysisResult
    {
        void SetFilename(string filename);
        IFunctionResult GetFunction(string functionName);
        string GetImportModuleFilename(string importModule);
        IEnumerable<string> GetAvailableImportModules();
    }
}
