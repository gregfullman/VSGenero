using Microsoft.VisualStudio.Text.Editor;

namespace VSGenero.External.Interfaces
{
    public interface IGeneroTextViewChangedListener
    {
        void SetTextView(IWpfTextView view);
    }
}
