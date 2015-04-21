using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions
{
    public interface IGeneroTextViewChangedListener
    {
        void SetTextView(IWpfTextView view);
    }
}
