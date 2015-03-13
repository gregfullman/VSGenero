using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public class AnalysisVariable : IAnalysisVariable
    {
        public AnalysisVariable(LocationInfo locInfo, VariableType type)
        {
            _location = locInfo;
            _type = type;
        }

        private LocationInfo _location;
        public LocationInfo Location
        {
            get { return _location; }
        }

        private VariableType _type;
        public VariableType Type
        {
            get { return _type; }
        }
    }
}
