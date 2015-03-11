using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public class OverloadResult : IFunctionResult
    {
        private readonly ParameterResult[] _parameters;
        private readonly string _name;

        public OverloadResult(ParameterResult[] parameters, string name)
        {
            _parameters = parameters;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
        public virtual string Documentation
        {
            get { return null; }
        }
        public virtual ParameterResult[] Parameters
        {
            get { return _parameters; }
        }
    }
}
