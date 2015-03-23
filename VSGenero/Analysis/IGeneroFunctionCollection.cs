using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IGeneroFunctionCollection : IAnalysisResult
    {
        IEnumerable<IAnalysisResult> Collections { get; }
        IEnumerable<IAnalysisResult> Functions { get; }
    }
}
