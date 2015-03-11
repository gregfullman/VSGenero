using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IFunctionResult
    {
        string Name { get; }
        string Documentation { get; }
        ParameterResult[] Parameters { get; }
    }
}
