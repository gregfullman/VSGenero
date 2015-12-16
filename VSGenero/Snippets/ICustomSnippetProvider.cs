using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Snippets
{
    public class TriggerDynamicSnippetEventArgs : EventArgs
    {
        public DynamicSnippet Snippet { get; set; }
        public string ReplaceString { get; set; }
    }

    public interface ICustomSnippetProvider
    {
        TriggerDynamicSnippetEventArgs GetCustomSnippet(ITextView textView, char c);
    }
}
