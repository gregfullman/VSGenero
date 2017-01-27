using Microsoft.VisualStudio.Shell;
using System;

namespace VSGenero.External.Build
{
    public class BuildTaskEventArgs : EventArgs
    {
        public TaskPriority Priority { get; set; }
        public TaskErrorCategory Category { get; set; }
        public string Filename { get; set; }
        public uint Line { get; set; }
        public uint Column { get; set; }
        public string Message { get; set; }
    }
}
