using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Peek
{
    [Export(typeof(IPeekResultPresenter))]
    [Name("Peek URL Presenter")]
    internal class F1PeekResultPresenter : IPeekResultPresenter
    {
        public IPeekResultPresentation TryCreatePeekResultPresentation(IPeekResult result)
        {
            UrlPeekResult f1Result = result as UrlPeekResult;
            if (f1Result != null)
            {
                return new UrlPeekResultPresentation(f1Result);
            }

            return null;
        }
    }
}
