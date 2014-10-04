using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Snippets
{
    public class DynamicSnippet
    {
        public string Title { get; private set; }
        public string Shortcut { get; private set; }
        public string Description { get; private set; }
        public string Author { get; private set; }

        public string Code { get; private set; }

        private List<DynamicSnippetReplacement> _replacements;
        public List<DynamicSnippetReplacement> Replacements
        {
            get
            {
                if (_replacements == null)
                    _replacements = new List<DynamicSnippetReplacement>();
                return _replacements;
            }
        }

        public DynamicSnippet(string title, string shortcut, string description, string author, string code)
        {
            Title = title;
            Shortcut = shortcut;
            Description = description;
            Author = author;
            Code = code;
        }
    }

    public class DynamicSnippetReplacement
    {
        public string ID { get; private set; }
        public string ToolTip { get; private set; }
        public string Default { get; private set; }

        public DynamicSnippetReplacement(string id, string tooltip, string defaultValue)
        {
            ID = id;
            ToolTip = tooltip;
            Default = defaultValue;
        }
    }


}
