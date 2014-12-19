using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.SqlSupport
{
    public interface ISqlContextDeterminator
    {
        bool DetermineSqlContext(string filename, out string dbServer, out string database);
    }
}
