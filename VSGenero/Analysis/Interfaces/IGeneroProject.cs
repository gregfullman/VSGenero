using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    /// <summary>
    /// Represents a group of files that can be analyzed
    /// </summary>
    public interface IGeneroProject
    {
        IGeneroProject AddImportedModule(string path, IGeneroProjectEntry importer);
        void RemoveImportedModule(string path);

        string Directory { get; }
        /// <summary>
        /// Enumerable of the project's immediate entries
        /// </summary>
        ConcurrentDictionary<string, IGeneroProjectEntry> ProjectEntries { get; }

        /// <summary>
        /// Enumerable of projects that are referenced from this project
        /// </summary>
        ConcurrentDictionary<string, IGeneroProject> ReferencedProjects { get; }

        HashSet<IGeneroProjectEntry> ReferencingProjectEntries { get; }
    }
}
