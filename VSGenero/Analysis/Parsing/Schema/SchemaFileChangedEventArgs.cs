using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.Schema
{
    public class SchemaFileChangedEventArgs : EventArgs
    {
        public string CurrentFilename { get; set; }
        public string NewFilename { get; set; }
    }
}
