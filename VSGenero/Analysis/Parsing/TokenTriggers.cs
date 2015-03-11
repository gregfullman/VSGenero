using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    [Flags]
    public enum TokenTriggers
    {
        None = 0,
        MemberSelect = 1,
        MatchBraces = 2,
        ParameterStart = 16,
        ParameterNext = 32,
        ParameterEnd = 64,
        Parameter = 128,
        MethodTip = 240,
    }
}
