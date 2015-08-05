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
        string GetExpressionText(out int startIndex, out int endIndex, out bool isFunctionCallOrDefinition);
    }
}
