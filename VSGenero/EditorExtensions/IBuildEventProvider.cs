using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions
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

    public class ClearBuildTasksEventArgs : EventArgs
    {
        public string Path { get; set; }
    }

    public interface IBuildTaskProvider
    {
        event EventHandler<BuildTaskEventArgs> BuildTaskGenerated;
        event EventHandler<ClearBuildTasksEventArgs> ClearBuildTasks;
    }
}
