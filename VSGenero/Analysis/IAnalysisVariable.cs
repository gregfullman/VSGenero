using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IAnalysisVariable
    {
        /// <summary>
        /// Returns the location of where the variable is defined.
        /// </summary>
        LocationInfo Location
        {
            get;
        }

        VariableType Type
        {
            get;
        }
    }
}
