namespace VSGenero.External.Analysis
{
    public class IncludeFileLocationChangedEventArgs : LocationChangedEventArgs
    {
        public IncludeFileLocationChangedEventArgs(string newLocation)
            : base(newLocation)
        {
        }
    }
}
