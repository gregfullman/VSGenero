using System;

namespace VSGenero.External.Analysis
{
    public abstract class LocationChangedEventArgs : EventArgs
    {
        public string NewLocation { get; private set; }

        protected LocationChangedEventArgs(string newLocation)
        {
            NewLocation = newLocation;
        }
    }
}
