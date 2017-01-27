using System;

namespace VSGenero.External.Build
{
    public class ClearBuildTasksEventArgs : EventArgs
    {
        public string Path { get; set; }
    }
}
