using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Peek
{
    internal class UrlPeekResultScrollState : IPeekResultScrollState
    {
        private UrlPeekResultPresentation _presentation;

        public UrlPeekResultScrollState(UrlPeekResultPresentation presentation)
        {
            if (presentation == null)
            {
                throw new ArgumentNullException("presentation");
            }
            _presentation = presentation;
        }

        public Uri BrowserUrl
        {
            get
            {
                if (_presentation.Browser != null)
                {
                    return _presentation.Browser.Url;
                }
                return null;
            }
        }

        public void RestoreScrollState(IPeekResultPresentation presentation)
        {
        }

        public void Dispose()
        {
        }
    }
}
