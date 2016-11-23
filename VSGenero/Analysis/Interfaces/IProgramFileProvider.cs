/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;

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
        IEnumerable<string> GetProgramFilenames(string filename);
        string GetImportModuleFilename(string importModule, string currentFilename);
        IEnumerable<string> GetAvailableImportModules(string currentFilename);
        string GetIncludeFile(string relativeFilename, string currentFilename);
        GeneroLanguageVersion GetLanguageVersion(string filename); 

        event EventHandler<ImportModuleLocationChangedEventArgs> ImportModuleLocationChanged;
        event EventHandler<IncludeFileLocationChangedEventArgs> IncludeFileLocationChanged;
    }
}
