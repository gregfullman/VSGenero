using System;
using System.Collections.Generic;
using VSGenero.External.Analysis;
using VSGenero.External.Analysis.Parsing;

namespace VSGenero.External.Interfaces
{
    public interface IProgramFileProvider
    {
        IEnumerable<string> GetProgramFilenames(string filename);
        string GetImportModuleFilename(string importModule, string currentFilename);
        IEnumerable<string> GetAvailableImportModules(string currentFilename);
        string GetIncludeFile(string relativeFilename, string currentFilename);
        GeneroLanguageVersion GetLanguageVersion(string filename);

        event EventHandler<ImportModuleLocationChangedEventArgs> ImportModuleLocationChanged;
        event EventHandler<IncludeFileLocationChangedEventArgs> IncludeFileLocationChanged;
    }
}
