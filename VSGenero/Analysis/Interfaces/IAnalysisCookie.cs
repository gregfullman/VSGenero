using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    /// <summary>
    /// Used to track information about where the analysis came from and
    /// get back the original content.
    /// </summary>
    public interface IAnalysisCookie
    {
        string GetLine(int lineNo);
    }
}
