using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;

namespace VSGenero.Analysis
{
    public interface IReverseTokenizer
    {
        IEnumerable<TokenInfo> GetReversedTokens();
    }
}
