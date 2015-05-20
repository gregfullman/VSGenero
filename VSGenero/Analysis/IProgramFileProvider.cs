using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IProgramFileProvider
    {
        void SetFilename(string filename);
        IEnumerable<string> GetProgramFilenames(string filename);
        string GetImportModuleFilename(string importModule);
        IEnumerable<string> GetAvailableImportModules();
        string GetIncludeFile(string relativeFilename);
    }
}
