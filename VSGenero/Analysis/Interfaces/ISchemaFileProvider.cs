using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing.Schema;

namespace VSGenero.Analysis.Interfaces
{
    public interface ISchemaFileProvider
    {
        event EventHandler<SchemaFileChangedEventArgs> SchemaFileChanged;

        string GetSchemaFilename(string currentFile);
    }
}
