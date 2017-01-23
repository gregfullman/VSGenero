using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.External.Snippets
{
    public class TriggerDynamicSnippetEventArgs : EventArgs
    {
        public DynamicSnippet Snippet { get; set; }
        public string ReplaceString { get; set; }
    }
}
