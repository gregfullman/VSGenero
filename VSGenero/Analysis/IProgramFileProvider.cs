using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public abstract class LocationChangedEventArgs : EventArgs
    {
        public string NewLocation { get; private set; }

        protected LocationChangedEventArgs(string newLocation)
        {
            NewLocation = newLocation;
        }
    }

    public class ImportModuleLocationChangedEventArgs : LocationChangedEventArgs
    {
        public string ImportModule { get; private set; }

        public ImportModuleLocationChangedEventArgs(string importModule, string newLocation)
            : base(newLocation)
        {
            ImportModule = importModule;
        }
    }

    public class IncludeFileLocationChangedEventArgs : LocationChangedEventArgs
    {
        public IncludeFileLocationChangedEventArgs(string newLocation)
            : base(newLocation)
        {
        }
    }

    public interface IProgramFileProvider
    {
        void SetFilename(string filename);
        IEnumerable<string> GetProgramFilenames(string filename);
        string GetImportModuleFilename(string importModule);
        IEnumerable<string> GetAvailableImportModules();
        string GetIncludeFile(string relativeFilename);

        event EventHandler<ImportModuleLocationChangedEventArgs> ImportModuleLocationChanged;
        event EventHandler<IncludeFileLocationChangedEventArgs> IncludeFileLocationChanged;
    }
}
