/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSGenero.Navigation;
using Microsoft.VisualStudio.VSCommon;
using Microsoft.VisualStudioTools;
using System.IO;
using VSGenero.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Diagnostics;
using VSGenero.Analysis.Parsing.AST;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using VSGenero.Analysis.Parsing;

namespace VSGenero.EditorExtensions.Intellisense
{
    class EntryEventArgs : EventArgs
    {
        public readonly IProjectEntry Entry;

        public EntryEventArgs(IProjectEntry entry)
        {
            Entry = entry;
        }
    }

    class FileEventArgs : EventArgs
    {
        public readonly string Filename;

        public FileEventArgs(string filename)
        {
            Filename = filename;
        }
    }

    struct MonitoredBufferResult
    {
        public readonly BufferParser BufferParser;
        public readonly ITextView TextView;
        public readonly IProjectEntry ProjectEntry;

        public MonitoredBufferResult(BufferParser bufferParser, ITextView textView, IProjectEntry projectEntry)
        {
            BufferParser = bufferParser;
            TextView = textView;
            ProjectEntry = projectEntry;
        }
    }

    public class GeneroProjectEntryComparer : IEqualityComparer<IGeneroProjectEntry>
    {
        public bool Equals(IGeneroProjectEntry x, IGeneroProjectEntry y)
        {
            return string.Equals(x.FilePath, y.FilePath, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(IGeneroProjectEntry obj)
        {
            return obj.FilePath.GetHashCode();
        }
    }

    public class GeneroProjectAnalyzer : IDisposable
    {
        private static GeneroProjectEntryComparer ProjectEntryComparer = new GeneroProjectEntryComparer();
        private readonly ParseQueue _queue;
        private readonly AnalysisQueue _analysisQueue;
        private readonly Dictionary<BufferParser, IProjectEntry> _openFiles = new Dictionary<BufferParser, IProjectEntry>();
        private readonly ConcurrentDictionary<string, IGeneroProject> _projects;



        private readonly bool _implicitProject;
        private readonly AutoResetEvent _queueActivityEvent = new AutoResetEvent(false);

        private int _userCount;

        internal readonly HashSet<IProjectEntry> _hasParseErrors = new HashSet<IProjectEntry>();
        internal readonly object _hasParseErrorsLock = new object();

        private const string ParserTaskMoniker = "Parser";
        //internal const string UnresolvedImportMoniker = "UnresolvedImport";

        // Internal for tests
        private ErrorTaskProvider _errorProvider;
        //private CommentTaskProvider _commentTaskProvider;

        private readonly IServiceProvider _serviceProvider;

        private object _contentsLock = new object();

        internal GeneroProjectAnalyzer(IServiceProvider serviceProvider, bool implicitProject = true)
        {
            _serviceProvider = serviceProvider;
            _errorProvider = (ErrorTaskProvider)serviceProvider.GetService(typeof(ErrorTaskProvider));
            //_commentTaskProvider = (CommentTaskProvider)serviceProvider.GetService(typeof(CommentTaskProvider));

            _queue = new ParseQueue(this);
            _analysisQueue = new AnalysisQueue(this);

            _implicitProject = implicitProject;

            _projects = new ConcurrentDictionary<string, IGeneroProject>(StringComparer.OrdinalIgnoreCase);
            _includeFiles = new ConcurrentDictionary<string, IGeneroProjectEntry>(StringComparer.OrdinalIgnoreCase);
            _includesToIncludersMap = new ConcurrentDictionary<IGeneroProjectEntry, HashSet<IGeneroProjectEntry>>(ProjectEntryComparer);
            _includersToIncludesMap = new ConcurrentDictionary<IGeneroProjectEntry, HashSet<IGeneroProjectEntry>>(ProjectEntryComparer);

            _userCount = 1;
        }

        public void AddUser()
        {
            Interlocked.Increment(ref _userCount);
        }

        /// <summary>
        /// Reduces the number of known users by one and returns true if the
        /// analyzer should be disposed.
        /// </summary>
        public bool RemoveUser()
        {
            return Interlocked.Decrement(ref _userCount) == 0;
        }

        /// <summary>
        /// Creates a new ProjectEntry for the collection of buffers.
        /// 
        /// _openFiles must be locked when calling this function.
        /// </summary>
        internal void ReAnalyzeTextBuffers(BufferParser bufferParser)
        {
            ITextBuffer[] buffers = bufferParser.Buffers;
            if (buffers.Length > 0)
            {
                _errorProvider.ClearErrorSource(bufferParser._currentProjEntry, ParserTaskMoniker);
                //_errorProvider.ClearErrorSource(bufferParser._currentProjEntry, UnresolvedImportMoniker);
                //_commentTaskProvider.ClearErrorSource(bufferParser._currentProjEntry, ParserTaskMoniker);
                //_unresolvedSquiggles.StopListening(bufferParser._currentProjEntry as IPythonProjectEntry);

                var projEntry = CreateProjectEntry(buffers[0], new SnapshotCookie(buffers[0].CurrentSnapshot));

                //bool doSquiggles = !buffers[0].Properties.ContainsProperty(typeof(IReplEvaluator));
                //if (doSquiggles)
                //{
                //    _unresolvedSquiggles.ListenForNextNewAnalysis(projEntry as IPythonProjectEntry);
                //}

                foreach (var buffer in buffers)
                {
                    buffer.Properties.RemoveProperty(typeof(IProjectEntry));
                    buffer.Properties.AddProperty(typeof(IProjectEntry), projEntry);

                    var classifier = buffer.GetGeneroClassifier();
                    if (classifier != null)
                    {
                        classifier.NewVersion();
                    }

                    ConnectErrorList(projEntry, buffer);
                    //if (doSquiggles)
                    //{
                    //    _errorProvider.AddBufferForErrorSource(projEntry, UnresolvedImportMoniker, buffer);
                    //}
                }
                bufferParser._currentProjEntry = _openFiles[bufferParser] = projEntry;
                bufferParser._parser = this;

                foreach (var buffer in buffers)
                {
                    // A buffer may have multiple DropDownBarClients, given one may open multiple CodeWindows
                    // over a single buffer using Window/New Window
                    List<DropDownBarClient> clients;
                    if (buffer.Properties.TryGetProperty<List<DropDownBarClient>>(typeof(DropDownBarClient), out clients))
                    {
                        foreach (var client in clients)
                        {
                            client.UpdateProjectEntry(projEntry);
                        }
                    }
                }

                bufferParser.Requeue();
            }
        }

        public void ConnectErrorList(IProjectEntry projEntry, ITextBuffer buffer)
        {
            _errorProvider.AddBufferForErrorSource(projEntry, ParserTaskMoniker, buffer);
            //_commentTaskProvider.AddBufferForErrorSource(projEntry, ParserTaskMoniker, buffer);
        }

        public void DisconnectErrorList(IProjectEntry projEntry, ITextBuffer buffer)
        {
            _errorProvider.RemoveBufferForErrorSource(projEntry, ParserTaskMoniker, buffer);
            //_commentTaskProvider.RemoveBufferForErrorSource(projEntry, ParserTaskMoniker, buffer);
        }

        internal void SwitchAnalyzers(GeneroProjectAnalyzer oldAnalyzer)
        {
            lock (_openFiles)
            {
                // copy the Keys here as ReAnalyzeTextBuffers can mutuate the dictionary
                foreach (var bufferParser in oldAnalyzer._openFiles.Keys.ToArray())
                {
                    ReAnalyzeTextBuffers(bufferParser);
                }
            }
        }

        /// <summary>
        /// Starts monitoring a buffer for changes so we will re-parse the buffer to update the analysis
        /// as the text changes.
        /// </summary>
        internal MonitoredBufferResult MonitorTextBuffer(ITextView textView, ITextBuffer buffer)
        {
            IProjectEntry projEntry = CreateProjectEntry(buffer, new SnapshotCookie(buffer.CurrentSnapshot));

            //if (!buffer.Properties.ContainsProperty(typeof(IReplEvaluator)))
            //{
            ConnectErrorList(projEntry, buffer);
            //    _errorProvider.AddBufferForErrorSource(projEntry, UnresolvedImportMoniker, buffer);
            //    _unresolvedSquiggles.ListenForNextNewAnalysis(projEntry as IPythonProjectEntry);
            //}

            // kick off initial processing on the buffer
            lock (_openFiles)
            {
                var bufferParser = _queue.EnqueueBuffer(projEntry, textView, buffer);
                _openFiles[bufferParser] = projEntry;
                return new MonitoredBufferResult(bufferParser, textView, projEntry);
            }
        }

        internal void StopMonitoringTextBuffer(BufferParser bufferParser)
        {
            bufferParser.StopMonitoring();
            lock (_openFiles)
            {
                _openFiles.Remove(bufferParser);
            }

            if (ImplicitProject)
            {
                string path = bufferParser._currentProjEntry.FilePath;
                if (path != null)
                {
                    // check to see if the file is still needed
                    string dirPath = Path.GetDirectoryName(path);

                    IGeneroProject proj;
                    if (_projects.TryGetValue(dirPath, out proj))
                    {
                        IGeneroProjectEntry entry;
                        if (proj.ProjectEntries.TryGetValue(path, out entry))
                        {
                            proj.ProjectEntries[path].IsOpen = entry.IsOpen = false;
                            // are there any others that are open?
                            if (!proj.ProjectEntries.Any(x => x.Value != entry && x.Value.IsOpen))
                            {
                                if (proj.ReferencingProjectEntries.Count == 0)
                                {
                                    // before clearing the top level project's entries, we need to go through
                                    // each one and have any Referenced projects remove the entry from their Referencing set.
                                    foreach (var projEntry in proj.ProjectEntries)
                                    {
                                        foreach (var refProj in proj.ReferencedProjects)
                                        {
                                            if (refProj.Value.ReferencingProjectEntries.Contains(projEntry.Value))
                                                refProj.Value.ReferencingProjectEntries.Remove(projEntry.Value);
                                        }
                                    }

                                    // Clear the entry's included files
                                    foreach (var pEntry in proj.ProjectEntries.Values)
                                    {
                                        HashSet<IGeneroProjectEntry> includedFiles;
                                        if (_includersToIncludesMap.TryGetValue(pEntry, out includedFiles) &&
                                            includedFiles.Count > 0)
                                        {
                                            foreach (var includeFile in includedFiles.ToList())
                                                RemoveIncludedFile(includeFile.FilePath, pEntry);
                                        }
                                    }

                                    proj.ProjectEntries.Clear();

                                    // unload any import modules that are not referenced by anything else.
                                    UnloadImportedModules(proj);

                                    _projects.TryRemove(dirPath, out proj);

                                    // remove the file from the error list
                                    _errorProvider.Clear(dirPath, ParserTaskMoniker);
                                    _errorProvider.ClearErrorSource(dirPath);
                                }
                            }
                        }
                    }
                }
            }
        }


        // includes
        private readonly object _includeFilesLock = new object();
        private readonly ConcurrentDictionary<string, IGeneroProjectEntry> _includeFiles;
        private readonly object _includersLock = new object();
        private readonly ConcurrentDictionary<IGeneroProjectEntry, HashSet<IGeneroProjectEntry>> _includesToIncludersMap;
        private readonly ConcurrentDictionary<IGeneroProjectEntry, HashSet<IGeneroProjectEntry>> _includersToIncludesMap;

        internal HashSet<IGeneroProjectEntry> GetIncludedFiles(IGeneroProjectEntry includingProjectEntry)
        {
            lock (_includersLock)
            {
                HashSet<IGeneroProjectEntry> files = null;
                if (!_includersToIncludesMap.TryGetValue(includingProjectEntry, out files))
                    files = new HashSet<IGeneroProjectEntry>();
                return files;
            }
        }

        internal bool IsIncludeFileIncludedByProjectEntry(string includeFilePath, IGeneroProjectEntry includingProjectEntry)
        {
            IGeneroProjectEntry includeFileEntry;

            // check to see if the include file path even exists
            lock (_includeFilesLock)
            {
                if (_includeFiles.TryGetValue(includeFilePath, out includeFileEntry))
                {
                    lock (_includersLock)
                    {
                        HashSet<IGeneroProjectEntry> includingProjectEntries;
                        if (_includesToIncludersMap.TryGetValue(includeFileEntry, out includingProjectEntries))
                        {
                            return includingProjectEntries.Contains(includingProjectEntry);
                        }
                    }
                }
            }

            return false;
        }

        internal IGeneroProjectEntry AddIncludedFile(string includeFilePath, IGeneroProjectEntry includer)
        {
            IGeneroProjectEntry includeEntry;
            lock (_includeFilesLock)
            {
                if (!_includeFiles.TryGetValue(includeFilePath, out includeEntry))
                {
                    string moduleName = null;   // TODO: get module name from provider (if provider is null, take the file's directory name)
                    IAnalysisCookie cookie = null;
                    includeEntry = new GeneroProjectEntry(moduleName, includeFilePath, cookie, false);
                    _includeFiles.AddOrUpdate(includeFilePath, includeEntry, (x, y) => y);
                    lock (_includersLock)
                    {
                        if (_includesToIncludersMap.ContainsKey(includeEntry))
                        {
                            _includesToIncludersMap[includeEntry].Add(includer);
                        }
                        else
                        {
                            _includesToIncludersMap.AddOrUpdate(includeEntry, new HashSet<IGeneroProjectEntry>(ProjectEntryComparer) { includer }, (x, y) => y);
                        }

                        if (_includersToIncludesMap.ContainsKey(includer))
                        {
                            _includersToIncludesMap[includer].Add(includeEntry);
                        }
                        else
                        {
                            _includersToIncludesMap.AddOrUpdate(includer, new HashSet<IGeneroProjectEntry>(ProjectEntryComparer) { includeEntry }, (x, y) => y);
                        }
                    }
                    QueueFileAnalysis(includeFilePath);
                }
                else
                {
                    lock (_includersLock)
                    {
                        if (_includesToIncludersMap.ContainsKey(includeEntry))
                        {
                            _includesToIncludersMap[includeEntry].Add(includer);
                        }
                        else
                        {
                            _includesToIncludersMap.AddOrUpdate(includeEntry, new HashSet<IGeneroProjectEntry>(ProjectEntryComparer) { includer }, (x, y) => y);
                        }

                        if (_includersToIncludesMap.ContainsKey(includer))
                        {
                            _includersToIncludesMap[includer].Add(includeEntry);
                        }
                        else
                        {
                            _includersToIncludesMap.AddOrUpdate(includer, new HashSet<IGeneroProjectEntry>(ProjectEntryComparer) { includeEntry }, (x, y) => y);
                        }
                    }
                }
            }
            return includeEntry;
        }

        internal void UpdateIncludedFile(string newLocation)
        {
            IGeneroProjectEntry includeEntry;
            HashSet<IGeneroProjectEntry> includingProjectEntries = null;
            bool refill = false;

            lock (_includeFilesLock)
            {
                var filename = Path.GetFileName(newLocation);
                var includeFile = _includeFiles.Keys.FirstOrDefault(x => x.EndsWith(filename, StringComparison.OrdinalIgnoreCase));
                if (includeFile != null)
                {
                    // need to remove the current include file, since the key has now changed
                    if (_includeFiles.TryRemove(includeFile, out includeEntry))
                    {
                        // now need to remove the mappings between include files and including project entries
                        lock (_includersLock)
                        {
                            if (_includesToIncludersMap.TryRemove(includeEntry, out includingProjectEntries))
                            {
                                refill = true;
                                foreach (var includer in includingProjectEntries)
                                    _includersToIncludesMap[includer].Remove(includeEntry);
                            }
                        }
                    }
                }
            }

            if (refill && includingProjectEntries != null)
            {
                foreach (var includer in includingProjectEntries)
                {
                    AddIncludedFile(newLocation, includer);
                }
            }
        }

        internal void RemoveIncludedFile(string includeFile, IGeneroProjectEntry includer)
        {
            lock (_includeFilesLock)
            {
                IGeneroProjectEntry includeEntry;
                if (_includeFiles.TryGetValue(includeFile, out includeEntry))
                {
                    // see if this is the last includer
                    if (_includesToIncludersMap.ContainsKey(includeEntry))
                    {
                        if (_includesToIncludersMap[includeEntry].Contains(includer))
                            _includesToIncludersMap[includeEntry].Remove(includer);
                        if (_includesToIncludersMap[includeEntry].Count == 0)
                        {
                            HashSet<IGeneroProjectEntry> dummy;
                            _includesToIncludersMap.TryRemove(includeEntry, out dummy);
                            _includeFiles.TryRemove(includeFile, out includeEntry);
                        }
                    }

                    if (_includersToIncludesMap.ContainsKey(includer))
                    {
                        if (_includersToIncludesMap[includer].Contains(includeEntry))
                            _includersToIncludesMap[includer].Remove(includeEntry);
                        if (_includersToIncludesMap[includer].Count == 0)
                        {
                            HashSet<IGeneroProjectEntry> dummy;
                            _includersToIncludesMap.TryRemove(includer, out dummy);
                        }
                    }
                }
            }
        }

        internal IGeneroProject AddImportedProject(string projectPath, IGeneroProjectEntry importer = null)
        {
            IGeneroProject projEntry;
            if (!_projects.TryGetValue(projectPath, out projEntry))
            {
                GeneroProject proj = new GeneroProject(projectPath);
                _projects.AddOrUpdate(projectPath, proj, (x, y) => y);
                var files = QueueDirectoryAnalysis(projectPath);
                projEntry = proj;
                foreach (var file in files)
                    projEntry.ProjectEntries.AddOrUpdate(file, new GeneroProjectEntry(null, file, importer.Cookie, true), (x, y) => y);
            }
            return projEntry;
        }

        internal void UpdateImportedProject(string projectName, string newProjectPath)
        {
            var key = _projects.Keys.FirstOrDefault(x => x.EndsWith(projectName, StringComparison.OrdinalIgnoreCase));
            if (key != null)
            {
                IGeneroProject proj;
                if (_projects.TryGetValue(key, out proj))
                {
                    // need to update the project
                    RemoveImportedProject(proj.Directory);
                    if (newProjectPath == null)
                    {
                        newProjectPath = VSGeneroPackage.Instance.ProgramFileProvider.GetImportModuleFilename(projectName);
                    }
                    var newProj = AddImportedProject(newProjectPath);

                    foreach (var refProj in proj.ReferencingProjectEntries)
                    {
                        refProj.ParentProject.RemoveImportedModule(key);
                        refProj.ParentProject.AddImportedModule(newProjectPath, refProj);
                        newProj.ReferencingProjectEntries.Add(refProj);
                    }
                }
            }
        }

        internal void RemoveImportedProject(string projectPath)
        {
            IGeneroProject proj;
            if (_projects.TryGetValue(projectPath, out proj))
            {
                // are there any others that are open?
                if (!proj.ProjectEntries.Any(x => x.Value.IsOpen))
                {
                    // before clearing the top level project's entries, we need to go through
                    // each one and have any Referenced projects remove the entry from their Referencing set.
                    foreach (var projEntry in proj.ProjectEntries)
                    {
                        foreach (var refProj in proj.ReferencedProjects)
                        {
                            if (refProj.Value.ReferencingProjectEntries.Contains(projEntry.Value))
                                refProj.Value.ReferencingProjectEntries.Remove(projEntry.Value);
                        }
                    }
                    proj.ProjectEntries.Clear();

                    // unload any import modules that are not referenced by anything else.
                    UnloadImportedModules(proj);

                    _projects.TryRemove(projectPath, out proj);

                    // remove the file from the error list
                    _errorProvider.Clear(projectPath, ParserTaskMoniker);
                    _errorProvider.ClearErrorSource(projectPath);
                }
            }
        }

        private IGeneroProjectEntry CreateProjectEntry(string filename, IAnalysisCookie analysisCookie)
        {
            bool shouldAnalyzePath = false;
            IGeneroProjectEntry entry = null;
            if (filename != null)
            {
                string dirPath = Path.GetDirectoryName(filename);
                string extension = Path.GetExtension(filename);
                IGeneroProject projEntry;
                if (!_projects.TryGetValue(dirPath, out projEntry))
                {
                    if (extension.Equals(VSGeneroConstants.FileExtension4GL, StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(VSGeneroConstants.FileExtensionINC, StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(VSGeneroConstants.FileExtensionPER, StringComparison.OrdinalIgnoreCase))
                    {
                        shouldAnalyzePath = ShouldAnalyzePath(filename);
                        string moduleName = null;   // TODO: get module name from provider (if provider is null, take the file's directory name)
                        IAnalysisCookie cookie = null;
                        entry = new GeneroProjectEntry(moduleName, filename, cookie, shouldAnalyzePath);
                    }

                    if (entry != null)
                    {
                        GeneroProject proj = new GeneroProject(dirPath);
                        proj.ProjectEntries.AddOrUpdate(filename, entry, (x, y) => y);
                        entry.SetProject(proj);
                        _projects.AddOrUpdate(dirPath, proj, (x, y) => y);

                        if (ImplicitProject && shouldAnalyzePath)
                        { // don't analyze std lib
                            QueueDirectoryAnalysis(dirPath, filename);
                        }
                    }
                }
                else
                {
                    if (!projEntry.ProjectEntries.TryGetValue(filename, out entry))
                    {
                        if (extension.Equals(VSGeneroConstants.FileExtension4GL, StringComparison.OrdinalIgnoreCase) ||
                            extension.Equals(VSGeneroConstants.FileExtensionINC, StringComparison.OrdinalIgnoreCase) ||
                            extension.Equals(VSGeneroConstants.FileExtensionPER, StringComparison.OrdinalIgnoreCase))
                        {
                            shouldAnalyzePath = ShouldAnalyzePath(filename);
                            string moduleName = null;   // TODO: get module name from provider (if provider is null, take the file's directory name)
                            IAnalysisCookie cookie = null;
                            entry = new GeneroProjectEntry(moduleName, filename, cookie, shouldAnalyzePath);
                        }
                    }
                    if (entry != null)
                    {
                        projEntry.ProjectEntries.AddOrUpdate(filename, entry, (x, y) => y);
                        entry.SetProject(projEntry);
                        _projects.AddOrUpdate(dirPath, projEntry, (x, y) => y);

                        if (ImplicitProject && shouldAnalyzePath)
                        { // don't analyze std lib
                            QueueDirectoryAnalysis(dirPath, filename);
                        }
                    }
                }
            }

            if (entry != null)
            {
                entry.IsOpen = true;
            }
            return entry;
        }

        private IGeneroProjectEntry CreateProjectEntry(ITextBuffer buffer, IAnalysisCookie analysisCookie)
        {
            return CreateProjectEntry(buffer.GetFilePath(), analysisCookie);
        }

        private void ProjectEntryAnalyzed(string path, IGeneroProjectEntry projEntry)
        {
            lock (_includeFilesLock)
            {
                bool includeFile = false;
                if (_includeFiles.ContainsKey(path))
                {
                    _includeFiles[path] = projEntry;
                    includeFile = true;
                }

                // update the includers map
                lock (_includersLock)
                {
                    HashSet<IGeneroProjectEntry> includingProjects;
                    if (_includesToIncludersMap.TryRemove(projEntry, out includingProjects))
                    {
                        _includesToIncludersMap.AddOrUpdate(projEntry, includingProjects, (x, y) => y);

                        foreach (var includingProject in includingProjects)
                        {
                            if (_includersToIncludesMap.ContainsKey(includingProject))
                            {
                                _includersToIncludesMap[includingProject].Remove(projEntry);
                                _includersToIncludesMap[includingProject].Add(projEntry);
                            }
                        }
                    }
                }


                if (includeFile)
                    return;
            }

            string dirPath = Path.GetDirectoryName(path);
            IGeneroProject proj;
            if (_projects.TryGetValue(dirPath, out proj))
            {
                proj.ProjectEntries.AddOrUpdate(path, projEntry, (x, y) => y);
            }
        }

        private List<string> QueueDirectoryAnalysis(string path, string excludeFile = null)
        {
            string normalizedPath = CommonUtils.NormalizeDirectoryPath(path);
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(normalizedPath, "*.4gl").Where(x => string.IsNullOrWhiteSpace(excludeFile) ? true : !x.Equals(excludeFile, StringComparison.OrdinalIgnoreCase)));
            ThreadPool.QueueUserWorkItem(x => { lock (_contentsLock) { AnalyzeDirectory(normalizedPath, excludeFile, ProjectEntryAnalyzed); } });
            return files;
        }

        private void QueueFileAnalysis(string filename)
        {
            ThreadPool.QueueUserWorkItem(x => { lock (_contentsLock) { AnalyzeFile(filename, ProjectEntryAnalyzed); } });
        }

        private bool ShouldAnalyzePath(string path)
        {
            // Devart Code Compare is very resource hungry within VS, so we don't want to do a complete
            // directory analysis if the file is being opened for a compare.
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);
            if (trace.GetFrames().Any(x => x.GetMethod().Name.IndexOf("compare", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return false;
            }

            // For now, we're going to assume that the scope of the program
            // is the directory in which the specified file resides.
            return true;
        }

        private void UnloadImportedModules(IGeneroProject project)
        {
            var refList = project.ReferencedProjects.Select(x => x.Value).ToList();
            for (int i = 0; i < refList.Count; i++)
            {
                // if the project references another project, attempt to unload the referenced project (by recursing into it)
                if (refList[i].ReferencedProjects.Count > 0)
                {
                    UnloadImportedModules(refList[i]);
                }

                if (refList[i].ReferencingProjectEntries.Count != 0)
                {
                    var referrList = refList[i].ReferencingProjectEntries.ToList();
                    for (int j = 0; j < referrList.Count; j++)
                    {
                        if (referrList[j].ParentProject == project)
                            refList[i].ReferencingProjectEntries.Remove(referrList[j]);
                    }
                }

                // if no other things 
                if (refList[i].ReferencingProjectEntries.Count == 0)
                {
                    IGeneroProject remProj;
                    project.ReferencedProjects.TryRemove(refList[i].Directory, out remProj);
                    IGeneroProject remProj2;
                    if (_projects.TryRemove(refList[i].Directory, out remProj2))
                    {
                        foreach (var remProj2Entry in remProj2.ProjectEntries.Values)
                        {
                            HashSet<IGeneroProjectEntry> includedFiles;
                            if (_includersToIncludesMap.TryGetValue(remProj2Entry, out includedFiles) &&
                                includedFiles.Count > 0)
                            {
                                foreach (var includeFile in includedFiles.ToList())
                                    RemoveIncludedFile(includeFile.FilePath, remProj2Entry);
                            }
                        }
                        _errorProvider.Clear(refList[i].Directory, ParserTaskMoniker);
                        _errorProvider.ClearErrorSource(refList[i].Directory);
                    }
                }
            }
        }

        internal IGeneroProjectEntry AnalyzeFile(string path)
        {
            IGeneroProjectEntry entry = null;
            if (path != null)
            {
                string dirPath = Path.GetDirectoryName(path);
                IGeneroProject projEntry;
                if (!_projects.TryGetValue(dirPath, out projEntry))
                {
                    if (VSGeneroConstants.IsGeneroFile(path))
                    {
                        string moduleName = null;   // TODO: get module name from provider (if provider is null, take the file's directory name)
                        IAnalysisCookie cookie = null;
                        entry = new GeneroProjectEntry(moduleName, path, cookie, true);
                    }

                    if (entry != null)
                    {
                        GeneroProject proj = new GeneroProject(dirPath);
                        proj.ProjectEntries.AddOrUpdate(path, entry, (x, y) => y);
                        entry.SetProject(proj);
                        _projects.AddOrUpdate(dirPath, proj, (x, y) => y);
                    }
                }
                else
                {
                    if (!projEntry.ProjectEntries.TryGetValue(path, out entry))
                    {
                        if (VSGeneroConstants.IsGeneroFile(path))
                        {
                            string moduleName = null;   // TODO: get module name from provider (if provider is null, take the file's directory name)
                            IAnalysisCookie cookie = null;
                            entry = new GeneroProjectEntry(moduleName, path, cookie, true);
                        }
                    }
                    if (entry != null)
                    {
                        projEntry.ProjectEntries.AddOrUpdate(path, entry, (x, y) => y);
                        entry.SetProject(projEntry);
                        _projects.AddOrUpdate(dirPath, projEntry, (x, y) => y);
                    }
                }

                _queue.EnqueueFile(entry, path);
            }
            return entry;
        }

        /// <summary>
        /// Gets a ExpressionAnalysis for the expression at the provided span.  If the span is in
        /// part of an identifier then the expression is extended to complete the identifier.
        /// </summary>
        internal static ExpressionAnalysis AnalyzeExpression(ITextSnapshot snapshot, ITrackingSpan span, IFunctionInformationProvider functionProvider,
                                                             IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider, bool forCompletion = true)
        {
            var buffer = snapshot.TextBuffer;
            Genero4glReverseParser parser = new Genero4glReverseParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);
            bool isFunctionCallOrDefinition;
            var exprRange = parser.GetExpressionRange(out isFunctionCallOrDefinition, forCompletion);

            if (exprRange == null)
            {
                return ExpressionAnalysis.Empty;
            }

            string text = exprRange.Value.GetText();
            // remove any newlines in the text
            string[] lines = text.Split(new[] { '\n' });
            StringBuilder sb = new StringBuilder();
            foreach (var line in lines)
                sb.Append(line.Trim());
            text = sb.ToString();

            var applicableSpan = parser.Snapshot.CreateTrackingSpan(
                exprRange.Value.Span,
                SpanTrackingMode.EdgeExclusive
            );

            IProjectEntry analysisItem;
            if (buffer.TryGetAnalysis(out analysisItem))
            {
                var analysis = ((IGeneroProjectEntry)analysisItem).Analysis;
                if (analysis != null && text.Length > 0)
                {

                    var lineNo = parser.Snapshot.GetLineNumberFromPosition(loc.Start);
                    return new ExpressionAnalysis(
                        snapshot.TextBuffer.GetAnalyzer(),
                        text,
                        analysis,
                        loc.Start,
                        applicableSpan,
                        parser.Snapshot,
                        functionProvider,
                        databaseProvider,
                        programFileProvider,
                        isFunctionCallOrDefinition);
                }
            }

            return ExpressionAnalysis.Empty;
        }

        /// <summary>
        /// Gets a CompletionList providing a list of possible members the user can dot through.
        /// </summary>
        internal static CompletionAnalysis GetCompletions(ITextSnapshot snapshot, ITrackingSpan span, ITrackingPoint point, CompletionOptions options,
                                                          IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider,
                                                          IProgramFileProvider programFileProvider)
        {
            return TrySpecialCompletions(snapshot, span, point, options, functionProvider, databaseProvider, programFileProvider);
            //GetNormalCompletionContext(snapshot, span, point, options);
        }

        /// <summary>
        /// Gets a list of signatuers available for the expression at the provided location in the snapshot.
        /// </summary>
        internal static SignatureAnalysis GetSignatures(ITextSnapshot snapshot, ITrackingSpan span, IFunctionInformationProvider functionProvider)
        {
            var buffer = snapshot.TextBuffer;
            Genero4glReverseParser parser = new Genero4glReverseParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);

            int paramIndex;
            SnapshotPoint? sigStart;
            string lastKeywordArg;
            bool isParameterName;
            bool isFunctionCallOrDefinition;
            var exprRange = parser.GetExpressionRange(1, out paramIndex, out sigStart, out lastKeywordArg, out isParameterName, out isFunctionCallOrDefinition);
            if (exprRange == null || sigStart == null)
            {
                return new SignatureAnalysis("", 0, new ISignature[0]);
            }

            var text = new SnapshotSpan(exprRange.Value.Snapshot, new Span(exprRange.Value.Start, sigStart.Value.Position - exprRange.Value.Start)).GetText();
            var applicableSpan = parser.Snapshot.CreateTrackingSpan(exprRange.Value.Span, SpanTrackingMode.EdgeInclusive);

            var start = Stopwatch.ElapsedMilliseconds;

            var analysisItem = buffer.GetAnalysis();
            if (analysisItem != null)
            {
                var analysis = ((IGeneroProjectEntry)analysisItem).Analysis;
                if (analysis != null)
                {
                    int index = TranslateIndex(loc.Start, snapshot, analysis);

                    IEnumerable<IFunctionResult> sigs = null;
                    lock (snapshot.TextBuffer.GetAnalyzer())
                    {
                        sigs = analysis.GetSignaturesByIndex(text, index, parser, functionProvider);
                    }
                    var end = Stopwatch.ElapsedMilliseconds;

                    if (/*Logging &&*/ (end - start) > CompletionAnalysis.TooMuchTime)
                    {
                        Trace.WriteLine(String.Format("{0} lookup time {1} for signatures", text, end - start));
                    }

                    var result = new List<ISignature>();
                    if (sigs != null)
                    {
                        foreach (var sig in sigs)
                        {
                            result.Add(new Genero4glFunctionSignature(applicableSpan, sig, paramIndex, lastKeywordArg));
                        }
                    }

                    return new SignatureAnalysis(
                        text,
                        paramIndex,
                        result,
                        lastKeywordArg
                    );
                }
            }
            return new SignatureAnalysis(text, paramIndex, new ISignature[0]);
        }

        internal static int TranslateIndex(int index, ITextSnapshot fromSnapshot, GeneroAst toAnalysisSnapshot)
        {
            return index;
        }

        private static bool IsDefinition(IAnalysisVariable variable)
        {
            return variable.Type == VariableType.Definition;
        }

        internal bool IsAnalyzing
        {
            get
            {
                return _queue.IsParsing || _analysisQueue.IsAnalyzing;
            }
        }

        internal void WaitForCompleteAnalysis(Func<int, bool> itemsLeftUpdated)
        {
            if (IsAnalyzing)
            {
                while (IsAnalyzing)
                {
                    QueueActivityEvent.WaitOne(100);

                    int itemsLeft = _queue.ParsePending + _analysisQueue.AnalysisPending;

                    if (!itemsLeftUpdated(itemsLeft))
                    {
                        break;
                    }
                }
            }
            else
            {
                itemsLeftUpdated(0);
            }
        }

        internal AutoResetEvent QueueActivityEvent
        {
            get
            {
                return _queueActivityEvent;
            }
        }

        /// <summary>
        /// True if the project is an implicit project and it should model files on disk in addition
        /// to files which are explicitly added.
        /// </summary>
        internal bool ImplicitProject
        {
            get
            {
                return _implicitProject;
            }
        }

        internal GeneroAst ParseFile(ITextSnapshot snapshot)
        {
            var parser = Parser.CreateParser(
                new SnapshotSpanSourceCodeReader(
                    new SnapshotSpan(snapshot, 0, snapshot.Length)
                ),
                new ParserOptions() { Verbatim = true, BindReferences = true }
            );

            var ast = parser.ParseFile();
            return ast;

        }

        internal GeneroAst ParseSnapshot(ITextSnapshot snapshot)
        {
            using (var parser = Parser.CreateParser(
                new SnapshotSpanSourceCodeReader(
                    new SnapshotSpan(snapshot, 0, snapshot.Length)
                ),
                new ParserOptions() { Verbatim = true, BindReferences = true }
            ))
            {
                return ParseOneFile(null, parser);
            }
        }

        internal ITextSnapshot GetOpenSnapshot(IProjectEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            lock (_openFiles)
            {
                var item = _openFiles.FirstOrDefault(kv => kv.Value == entry);
                if (item.Value == null)
                {
                    return null;
                }
                var document = item.Key.Document;
                if (document == null)
                {
                    return null;
                }

                var textBuffer = document.TextBuffer;
                // TextBuffer may be null if we are racing with file close
                return textBuffer != null ? textBuffer.CurrentSnapshot : null;
            }
        }

        internal void ParseFile(IProjectEntry projectEntry, string filename, Stream content, Severity indentationSeverity)
        {
            IGeneroProjectEntry pyEntry;
            IAnalysisCookie cookie = (IAnalysisCookie)new FileCookie(filename);
            ITextSnapshot snapshot = GetOpenSnapshot(projectEntry);
            if ((pyEntry = projectEntry as IGeneroProjectEntry) != null)
            {
                GeneroAst ast;
                CollectingErrorSink errorSink;
                ParseGeneroCode(content, indentationSeverity, pyEntry, out ast, out errorSink);

                if (ast != null)
                {
                    pyEntry.UpdateTree(ast, cookie);
                }
                else
                {
                    // notify that we failed to update the existing analysis
                    pyEntry.UpdateTree(null, null);
                }

                UpdateErrorsAndWarnings(projectEntry, snapshot, errorSink, TaskLevel.Syntax);

                if (ast != null)
                {
                    _analysisQueue.Enqueue(pyEntry, AnalysisPriority.Normal);
                    pyEntry.UpdateIncludesAndImports(filename, ast);

                    List<IGeneroProjectEntry> unanalyzed = new List<IGeneroProjectEntry>();
                    unanalyzed.AddRange(pyEntry.ParentProject.GetUnanalyzedEntries());
                    unanalyzed.AddRange(pyEntry.GetIncludedFiles().Where(x => !x.IsAnalyzed));
                    if (unanalyzed.Count > 0)
                    {
                        // postpone error checking until the unanalyzed entries are analyzed
                        lock (_pendingWaitingLock)
                        {
                            // set up a map of the unanalyzed project entry to the dependent entry for which we're doing error checking.
                            // When there are no more unanalyzed project entries for the dependent entry, we can do the error checking.
                            foreach (var item in unanalyzed)
                            {
                                lock (item)
                                {
                                    if (_waitingToPendingMap.ContainsKey(pyEntry))
                                    {
                                        _waitingToPendingMap[pyEntry].Add(item);
                                    }
                                    else
                                    {
                                        _waitingToPendingMap.Add(pyEntry, new HashSet<IGeneroProjectEntry> { item });
                                    }

                                    if (_pendingToWaitingMap.ContainsKey(item))
                                    {
                                        _pendingToWaitingMap[item].Add(pyEntry);
                                    }
                                    else
                                    {
                                        _pendingToWaitingMap.Add(item, new List<IGeneroProjectEntry> { pyEntry });
                                        // and register for the event (no need to register for the event if the item is already in the pendingToWaiting map
                                        item.OnNewAnalysis += item_OnNewAnalysis;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        CheckForErrors(pyEntry);
                    }
                }
            }
        }

        internal void ParseBuffers(BufferParser bufferParser, Severity indentationSeverity, params ITextSnapshot[] snapshots)
        {
            IProjectEntry analysis = bufferParser._currentProjEntry;

            IGeneroProjectEntry pyProjEntry = analysis as IGeneroProjectEntry;
            List<GeneroAst> asts = new List<GeneroAst>();
            foreach (var snapshot in snapshots)
            {
                if (pyProjEntry != null &&
                    VSGeneroPackage.Instance.ProgramCodeContentTypes.Any(x => snapshot.TextBuffer.ContentType.IsOfType(x.TypeName)))
                {
                    GeneroAst ast;
                    CollectingErrorSink errorSink;
                    var reader = new SnapshotSpanSourceCodeReader(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                    ParseGeneroCode(reader, indentationSeverity, pyProjEntry, out ast, out errorSink);

                    if (ast != null)
                    {
                        asts.Add(ast);
                        pyProjEntry.UpdateIncludesAndImports(pyProjEntry.FilePath, ast);
                        //if (pyProjEntry.DetectCircularImports())
                        //{
                        //    // TODO: add error(s) from the (to be defined) errors returned from the function
                        //}
                    }

                    UpdateErrorsAndWarnings(analysis, snapshot, errorSink, TaskLevel.Syntax);
                }
            }

            if (pyProjEntry != null)
            {
                if (asts.Count > 0)
                {
                    GeneroAst finalAst = null;
                    if (asts.Count == 1)
                    {
                        finalAst = asts[0];
                    }

                    pyProjEntry.UpdateTree(finalAst, new SnapshotCookie(snapshots[0])); // SnapshotCookie is not entirely right, we should merge the snapshots
                    _analysisQueue.Enqueue(analysis, AnalysisPriority.High);
                }
                else
                {
                    // indicate that we are done parsing.
                    GeneroAst prevTree;
                    IAnalysisCookie prevCookie;
                    pyProjEntry.GetTreeAndCookie(out prevTree, out prevCookie);
                    pyProjEntry.UpdateTree(prevTree, prevCookie);
                }

                List<IGeneroProjectEntry> unanalyzed = new List<IGeneroProjectEntry>();
                unanalyzed.AddRange(pyProjEntry.ParentProject.GetUnanalyzedEntries());
                unanalyzed.AddRange(pyProjEntry.GetIncludedFiles().Where(x => !x.IsAnalyzed));
                if (unanalyzed.Count > 0)
                {
                    // postpone error checking until the unanalyzed entries are analyzed
                    lock (_pendingWaitingLock)
                    {
                        // set up a map of the unanalyzed project entry to the dependent entry for which we're doing error checking.
                        // When there are no more unanalyzed project entries for the dependent entry, we can do the error checking.
                        foreach (var item in unanalyzed)
                        {
                            lock (item)
                            {
                                if (_waitingToPendingMap.ContainsKey(pyProjEntry))
                                {
                                    _waitingToPendingMap[pyProjEntry].Add(item);
                                }
                                else
                                {
                                    _waitingToPendingMap.Add(pyProjEntry, new HashSet<IGeneroProjectEntry> { item });
                                }

                                if (_pendingToWaitingMap.ContainsKey(item))
                                {
                                    _pendingToWaitingMap[item].Add(pyProjEntry);
                                }
                                else
                                {
                                    _pendingToWaitingMap.Add(item, new List<IGeneroProjectEntry> { pyProjEntry });
                                    // and register for the event (no need to register for the event if the item is already in the pendingToWaiting map
                                    item.OnNewAnalysis += item_OnNewAnalysis;
                                }
                            }
                        }
                    }
                }
                else
                {
                    CheckForErrors(pyProjEntry);
                }
            }
        }

        private void CheckForErrors(IGeneroProjectEntry projEntry)
        {
            ITextSnapshot snapshot = GetOpenSnapshot(projEntry);

            // check for errors in the syntax tree
            CollectingErrorSink astErrors = new CollectingErrorSink();
            if (projEntry.Analysis._functionProvider == null && VSGeneroPackage.Instance.GlobalFunctionProvider != null)
            {
                projEntry.Analysis._functionProvider = VSGeneroPackage.Instance.GlobalFunctionProvider;
                projEntry.Analysis._functionProvider.SetFilename(projEntry.FilePath);
            }
            if (projEntry.Analysis._databaseProvider == null && VSGeneroPackage.Instance.GlobalDatabaseProvider != null)
            {
                projEntry.Analysis._databaseProvider = VSGeneroPackage.Instance.GlobalDatabaseProvider;
                projEntry.Analysis._databaseProvider.SetFilename(projEntry.FilePath);
            }
            projEntry.Analysis.CheckForErrors((msg, start, end) =>
            {
                astErrors.Add(msg, projEntry.Analysis._lineLocations, start, end, ErrorCodes.SyntaxError, Severity.Error);
            });

            // update any errors found
            if (astErrors.Errors.Count > 0 || astErrors.Warnings.Count > 0)
            {
                UpdateErrorsAndWarnings(projEntry, snapshot, astErrors, TaskLevel.Semantics);
            }
        }

        void item_OnNewAnalysis(object sender, EventArgs e)
        {
            lock (_pendingWaitingLock)
            {
                IGeneroProjectEntry pendingItem = sender as IGeneroProjectEntry;
                List<IGeneroProjectEntry> waitingItems;
                if (_pendingToWaitingMap.TryGetValue(pendingItem, out waitingItems))
                {
                    lock (pendingItem)
                    {
                        foreach (var waiting in waitingItems.ToArray())
                        {
                            // remove pendingItem from the waiting item's pending items
                            if (_waitingToPendingMap.ContainsKey(waiting))
                            {
                                if (_waitingToPendingMap[waiting].Contains(pendingItem))
                                {
                                    _waitingToPendingMap[waiting].Remove(pendingItem);
                                }

                                if (_waitingToPendingMap[waiting].Count == 0)
                                {
                                    // the waiting item is no longer waiting for any other pending items
                                    // so we can trigger the error checking

                                    
                                    // TODO: do this on a thread
                                    CheckForErrors(waiting);

                                    waitingItems.Remove(waiting);
                                }
                            }
                        }

                        if (waitingItems.Count == 0)
                        {
                            // the pending item has no more items awaiting it.
                            // so we can unregister from this item's event
                            pendingItem.OnNewAnalysis -= item_OnNewAnalysis;
                            _pendingToWaitingMap.Remove(pendingItem);
                        }
                    }
                }
            }
        }

        private Dictionary<IGeneroProjectEntry, HashSet<IGeneroProjectEntry>> _waitingToPendingMap = new Dictionary<IGeneroProjectEntry, HashSet<IGeneroProjectEntry>>();
        private Dictionary<IGeneroProjectEntry, List<IGeneroProjectEntry>> _pendingToWaitingMap = new Dictionary<IGeneroProjectEntry, List<IGeneroProjectEntry>>();
        private object _pendingWaitingLock = new object();


        private void UpdateErrorsAndWarnings(
            IProjectEntry entry,
            ITextSnapshot snapshot,
            CollectingErrorSink errorSink,
            TaskLevel level
        )
        {
            // Update the warn-on-launch state for this entry
            bool changed = false;
            lock (_hasParseErrorsLock)
            {
                changed = errorSink.Errors.Any() ? _hasParseErrors.Add(entry) : _hasParseErrors.Remove(entry);
            }
            if (changed)
            {
                OnShouldWarnOnLaunchChanged(entry);
            }

            // Update the parser warnings/errors.
            var factory = new TaskProviderItemFactory(snapshot);
            if (errorSink.Warnings.Any() || errorSink.Errors.Any())
            {
                if (!_errorProvider.HasErrorSource(entry, ParserTaskMoniker))
                    _errorProvider.AddBufferForErrorSource(entry, ParserTaskMoniker, null);

                _errorProvider.ReplaceItems(
                    entry,
                    ParserTaskMoniker,
                    errorSink.Warnings
                        .Select(er => factory.FromErrorResult(_serviceProvider, er, VSTASKPRIORITY.TP_NORMAL, VSTASKCATEGORY.CAT_BUILDCOMPILE, level))
                        .Concat(errorSink.Errors.Select(er => factory.FromErrorResult(_serviceProvider, er, VSTASKPRIORITY.TP_HIGH, VSTASKCATEGORY.CAT_BUILDCOMPILE, level)))
                        .ToList(),
                    level
                );
            }
            else
            {
                _errorProvider.Clear(entry, ParserTaskMoniker);
            }
        }

        internal void ClearParserTasks(IProjectEntry entry)
        {
            if (entry != null)
            {
                _errorProvider.Clear(entry, ParserTaskMoniker);
                //_commentTaskProvider.Clear(entry, ParserTaskMoniker);
                //_unresolvedSquiggles.StopListening(entry as IPythonProjectEntry);

                bool removed = false;
                lock (_hasParseErrorsLock)
                {
                    removed = _hasParseErrors.Remove(entry);
                }
                if (removed)
                {
                    OnShouldWarnOnLaunchChanged(entry);
                }
            }
        }

        internal void ClearAllTasks()
        {
            _errorProvider.ClearAll();
            //_commentTaskProvider.ClearAll();

            lock (_hasParseErrorsLock)
            {
                _hasParseErrors.Clear();
            }
        }

        internal bool ShouldWarnOnLaunch(IProjectEntry entry)
        {
            lock (_hasParseErrorsLock)
            {
                return _hasParseErrors.Contains(entry);
            }
        }

        private void OnShouldWarnOnLaunchChanged(IProjectEntry entry)
        {
            var evt = ShouldWarnOnLaunchChanged;
            if (evt != null)
            {
                evt(this, new EntryEventArgs(entry));
            }
        }

        internal event EventHandler<EntryEventArgs> ShouldWarnOnLaunchChanged;


        private void ParseGeneroCode(Stream content, Severity indentationSeverity, IProjectEntry entry, out GeneroAst ast, out CollectingErrorSink errorSink)
        {
            ast = null;
            errorSink = new CollectingErrorSink();

            using (var parser = Parser.CreateParser(content,
                                                    new ParserOptions() { Verbatim = true, ErrorSink = errorSink, IndentationInconsistencySeverity = indentationSeverity, BindReferences = true },
                                                    entry))
            {
                ast = ParseOneFile(ast, parser);
            }
        }

        private void ParseGeneroCode(TextReader content, Severity indentationSeverity, IProjectEntry entry, out GeneroAst ast, out CollectingErrorSink errorSink)
        {
            ast = null;
            errorSink = new CollectingErrorSink();

            using (var parser = Parser.CreateParser(content, new ParserOptions() { Verbatim = true, ErrorSink = errorSink, IndentationInconsistencySeverity = indentationSeverity, BindReferences = true }, entry))
            {
                ast = ParseOneFile(ast, parser);
            }
        }

        private static GeneroAst ParseOneFile(GeneroAst ast, Parser parser)
        {
            if (parser != null)
            {
                try
                {
                    ast = parser.ParseFile();
                }
                catch (Exception e)
                {
                    Debug.Assert(false, String.Format("Failure in Genero parser: {0}", e.ToString()));
                }

            }
            return ast;
        }

        private static ITrackingSpan CreateSpan(ITextSnapshot snapshot, SourceSpan span)
        {
            Debug.Assert(span.Start.Index >= 0);
            var newSpan = new Span(
                span.Start.Index,
                Math.Min(span.End.Index - span.Start.Index, Math.Max(snapshot.Length - span.Start.Index, 0))
            );
            Debug.Assert(newSpan.End <= snapshot.Length);
            return snapshot.CreateTrackingSpan(newSpan, SpanTrackingMode.EdgeInclusive);
        }

        private static CompletionAnalysis TrySpecialCompletions(ITextSnapshot snapshot, ITrackingSpan span, ITrackingPoint point, CompletionOptions options,
                                                                IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider,
                                                                IProgramFileProvider programFileProvider)
        {
            int currPos = point.GetPosition(snapshot);
            var snapSpan = span.GetSpan(snapshot);
            var buffer = snapshot.TextBuffer;
            var classifier = buffer.GetGeneroClassifier();
            if (classifier == null)
            {
                return null;
            }
            var start = snapSpan.Start;
            bool includePublicFunctions = snapSpan.Length > 2;
            var parser = new Genero4glReverseParser(snapshot, buffer, span);
            //if (parser.IsInGrouping())
            //{
            //    var range = parser.GetExpressionRange(nesting: 1);
            //    if (range != null)
            //    {
            //        start = range.Value.Start;
            //    }
            //}

            // TODO: need to figure out how to get a statement that spans more than one line
            var tokens = classifier.GetClassificationSpans(new SnapshotSpan(start.GetContainingLine().Start, snapSpan.Start));
            if (tokens.Count > 0)
            {
                // Check for context-sensitive intellisense
                var lastClass = tokens[tokens.Count - 1];

                if (currPos > lastClass.Span.Start.Position &&
                   currPos <= lastClass.Span.End.Position)
                {
                    if (lastClass.ClassificationType == classifier.Provider.Comment)
                    {
                        // No completions in comments
                        return CompletionAnalysis.EmptyCompletionContext;
                    }
                    else if (lastClass.ClassificationType == classifier.Provider.StringLiteral)
                    {

                        // String completion
                        //if (lastClass.Span.Start.GetContainingLine().LineNumber == lastClass.Span.End.GetContainingLine().LineNumber)
                        //{
                        //    return new StringLiteralCompletionList(span, buffer, options);
                        //}
                        //else
                        //{
                        // multi-line string, no string completions.
                        return CompletionAnalysis.EmptyCompletionContext;
                        //}
                    }
                }
            }
            else if ((tokens = classifier.GetClassificationSpans(snapSpan.Start.GetContainingLine().ExtentIncludingLineBreak)).Count > 0 &&
             tokens[0].ClassificationType == classifier.Provider.StringLiteral)
            {
                // multi-line string, no string completions.
                //return CompletionAnalysis.EmptyCompletionContext;
                int i = 0;
            }

            var entry = (IGeneroProjectEntry)buffer.GetAnalysis();
            if (entry != null && entry.Analysis != null)
            {
                //var members = entry.Analysis.GetContextMembersByIndex(start, parser, functionProvider, databaseProvider, programFileProvider);
                var members = entry.Analysis.GetContextMembers(start, parser, functionProvider, databaseProvider, programFileProvider, includePublicFunctions, span.GetText(snapshot));
                if (members != null)
                {
                    return new ContextSensitiveCompletionAnalysis(members, span, buffer, options);
                }
            }

            return CompletionAnalysis.EmptyCompletionContext; ;
        }

        private static CompletionAnalysis GetNormalCompletionContext(ITextSnapshot snapshot, ITrackingSpan applicableSpan, ITrackingPoint point, CompletionOptions options)
        {
            return new TestCompletionAnalysis(applicableSpan, snapshot.TextBuffer, options);
        }

        private static bool IsSpaceCompletion(ITextSnapshot snapshot, ITrackingPoint loc)
        {
            var pos = loc.GetPosition(snapshot);
            if (pos > 0)
            {
                return snapshot.GetText(pos - 1, 1) == " ";
            }
            return false;
        }

        /// <summary>
        /// Analyzes a complete directory including all of the contained files and packages.
        /// </summary>
        /// <param name="dir">Directory to analyze.</param>
        /// <param name="onFileAnalyzed">If specified, this callback is invoked for every <see cref="IProjectEntry"/>
        /// that is analyzed while analyzing this directory.</param>
        /// <remarks>The callback may be invoked on a thread different from the one that this function was originally invoked on.</remarks>
        public void AnalyzeDirectory(string dir, string excludeFile = null, Action<string, IGeneroProjectEntry> onFileAnalyzed = null)
        {
            _analysisQueue.Enqueue(new AddDirectoryAnalysis(dir, excludeFile, onFileAnalyzed, this), AnalysisPriority.High);
        }

        public void AnalyzeFile(string filename, Action<string, IGeneroProjectEntry> onFileAnalyzed = null)
        {
            _analysisQueue.Enqueue(new AddFileAnalysis(filename, onFileAnalyzed, this), AnalysisPriority.High);
        }

        class AddDirectoryAnalysis : IAnalyzable
        {
            private readonly string _dir;
            private readonly string _excludeFile;
            private readonly Action<string, IGeneroProjectEntry> _onFileAnalyzed;
            private readonly GeneroProjectAnalyzer _analyzer;

            public AddDirectoryAnalysis(string dir, string excludeFile, Action<string, IGeneroProjectEntry> onFileAnalyzed, GeneroProjectAnalyzer analyzer)
            {
                _dir = dir;
                _excludeFile = excludeFile;
                _onFileAnalyzed = onFileAnalyzed;
                _analyzer = analyzer;
            }

            #region IAnalyzable Members

            public void Analyze(CancellationToken cancel)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }

                _analyzer.AnalyzeDirectoryWorker(_dir, _excludeFile, _onFileAnalyzed, cancel);
            }

            #endregion
        }

        class AddFileAnalysis : IAnalyzable
        {
            private readonly string _filename;
            private readonly Action<string, IGeneroProjectEntry> _onFileAnalyzed;
            private readonly GeneroProjectAnalyzer _analyzer;

            public AddFileAnalysis(string filename, Action<string, IGeneroProjectEntry> onFileAnalyzed, GeneroProjectAnalyzer analyzer)
            {
                _filename = filename;
                _onFileAnalyzed = onFileAnalyzed;
                _analyzer = analyzer;
            }

            #region IAnalyzable Members

            public void Analyze(CancellationToken cancel)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }

                _analyzer.AnalyzeFileWorker(_filename, _onFileAnalyzed);
            }

            #endregion
        }

        private void AnalyzeFileWorker(string filename, Action<string, IGeneroProjectEntry> onFileAnalyzed)
        {
            if (string.IsNullOrEmpty(filename))
            {
                Debug.Assert(false, "Unexpected empty filename");
                return;
            }

            try
            {
                IGeneroProjectEntry entry = AnalyzeFile(filename);
                if (onFileAnalyzed != null)
                {
                    onFileAnalyzed(filename, entry);
                }
            }
            catch (IOException)
            {
                // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private void AnalyzeDirectoryWorker(string dir, string _excludeFile, Action<string, IGeneroProjectEntry> onFileAnalyzed, CancellationToken cancel)
        {
            if (string.IsNullOrEmpty(dir))
            {
                Debug.Assert(false, "Unexpected empty dir");
                return;
            }

            try
            {
                // TODO: need to handle .per and .inc files as well.
                foreach (string filename in Directory.GetFiles(dir, "*.4gl"))
                {
                    if (_excludeFile != null && filename.Equals(_excludeFile, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (cancel.IsCancellationRequested)
                    {
                        break;
                    }
                    IGeneroProjectEntry entry = AnalyzeFile(filename);
                    if (onFileAnalyzed != null)
                    {
                        onFileAnalyzed(filename, entry);
                    }
                }
            }
            catch (IOException)
            {
                // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        internal void Cancel()
        {
            _analysisQueue.Stop();
        }

        private static Stopwatch _stopwatch = MakeStopWatch();

        private static Stopwatch MakeStopWatch()
        {
            var res = new Stopwatch();
            res.Start();
            return res;
        }

        internal static Stopwatch Stopwatch
        {
            get
            {
                return _stopwatch;
            }
        }

        public void Dispose()
        {
            foreach (var proj in _projects.Values)
            {
                foreach (var entry in proj.ProjectEntries.Values)
                {
                    _errorProvider.Clear(entry, ParserTaskMoniker);
                    //_errorProvider.Clear(entry, UnresolvedImportMoniker);
                    //_commentTaskProvider.Clear(entry, ParserTaskMoniker);
                }

                // TODO: dispose of error providers for referenced projects
                int i = 0;
            }

            _analysisQueue.Stop();
        }
    }
}
