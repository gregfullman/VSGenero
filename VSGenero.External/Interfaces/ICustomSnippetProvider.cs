using Microsoft.VisualStudio.Text.Editor;
using VSGenero.External.Snippets;

namespace VSGenero.External.Interfaces
{
    public interface ICustomSnippetProvider
    {
        TriggerDynamicSnippetEventArgs GetCustomSnippet(ITextView textView, char c);
    }
}
