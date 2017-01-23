using System.Collections.Generic;

namespace VSGenero.External.Snippets
{
    public class DynamicSnippet
    {
        public string Title { get; private set; }
        public string Shortcut { get; private set; }
        public string Description { get; private set; }
        public string Author { get; private set; }

        public string Code { get; private set; }

        public string StaticSnippetString { get; set; }

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
}
