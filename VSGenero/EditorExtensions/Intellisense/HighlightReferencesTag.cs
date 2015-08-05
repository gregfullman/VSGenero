using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.Intellisense
{
    public class HighlightReferencesTag : TextMarkerTag
    {
        // Methods
        public HighlightReferencesTag()
            : base("MarkerFormatDefinition/HighlightedReference")
        {
        }
    }
}
