using Microsoft.VisualStudio.Text.BraceCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.BraceCompletion
{
    internal class LiteralCompletionContext : NormalCompletionContext
    {
        // Methods
        public LiteralCompletionContext(Genero4glLanguageInfo languageInfo)
            : base(languageInfo)
        {
        }

        public override bool AllowOverType(IBraceCompletionSession session)
        {
            return true;
        }
    }
}
