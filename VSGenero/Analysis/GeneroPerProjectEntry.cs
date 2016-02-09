using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    internal class GeneroPerProjectEntry : GeneroProjectEntry
    {
        public GeneroPerProjectEntry(string moduleName, string filePath, IAnalysisCookie cookie, bool shouldAnalyzeDir)
            : base(moduleName, filePath, cookie, shouldAnalyzeDir)
        {
        }
    }
}
