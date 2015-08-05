using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Refactoring
{
    struct ExtractMethodResult
    {
        public readonly string Method;
        public readonly string Call;

        public ExtractMethodResult(string newMethod, string newCall)
        {
            Method = newMethod;
            Call = newCall;
        }
    }
}
