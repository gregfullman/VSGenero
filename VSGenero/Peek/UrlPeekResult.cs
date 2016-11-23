using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Peek
{
    internal class UrlPeekResult : IPeekResult
    {
        public bool CanNavigateTo
        {
            get { return true; }
        }

        public IPeekResultDisplayInfo DisplayInfo
        {
            get { return new PeekResultDisplayInfo("Help", "Help", "Help", "Help"); }
        }

#pragma warning disable 0067
        public event EventHandler Disposed;
#pragma warning restore 0067

        public string Url { get; private set; }

        public Action<IPeekResult, object, object> PostNavigationCallback
        {
            get
            {
                return null;
            }
        }

        public UrlPeekResult(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("url");
            }

            this.Url = url;
        }

        public void NavigateTo(object data)
        {
            string uri = this.Url;
            UrlPeekResultScrollState f1PeekScrollState = data as UrlPeekResultScrollState;
            if (f1PeekScrollState != null && f1PeekScrollState.BrowserUrl != null)
            {
                uri = f1PeekScrollState.BrowserUrl.AbsoluteUri;
            }

            Process.Start(uri);
        }

        public void Dispose()
        {
        }
    }
}
