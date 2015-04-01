using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IDatabaseInformationProvider
    {
        IEnumerable<IAnalysisResult> GetTables();
        IEnumerable<IAnalysisResult> GetColumns(string tableName);
    }
}
