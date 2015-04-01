using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IFunctionInformationProvider
    {
        void SetFilename(string filename);

        IEnumerable<IAnalysisResult> GetFunctionCollections();
        IEnumerable<IFunctionResult> GetFunctions(string collectionName);
        IFunctionResult GetFunction(string functionName);
    }
}
