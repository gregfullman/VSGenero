using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Snippets
{
    public interface IPublicFunctionSnippetizer
    {
        DynamicSnippet GetSnippet(string functionName, ITextBuffer currentBuffer);
    }
}
