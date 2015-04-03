using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IDatabaseInformationProvider
    {
        void SetFilename(string filename);

        IEnumerable<IAnalysisResult> GetTables();
        IEnumerable<IAnalysisResult> GetColumns(string tableName);
        IAnalysisResult GetColumn(string tableName, string columnName);
    }
}
