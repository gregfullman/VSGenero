namespace VSGenero.External.Snippets
{
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
