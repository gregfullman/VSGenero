using System;

namespace VSGenero.External.Build
{
    public interface IBuildTaskProvider
    {
        event EventHandler<BuildTaskEventArgs> BuildTaskGenerated;
        event EventHandler<ClearBuildTasksEventArgs> ClearBuildTasks;
    }
}
