namespace VSGenero.External.Analysis
{
    public class ImportModuleLocationChangedEventArgs : LocationChangedEventArgs
    {
        public string ImportModule { get; private set; }

        public ImportModuleLocationChangedEventArgs(string importModule, string newLocation)
            : base(newLocation)
        {
            ImportModule = importModule;
        }
    }
}
