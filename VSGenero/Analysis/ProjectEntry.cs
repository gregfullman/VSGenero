using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.Analysis
{
    /// <summary>
    /// Provides interactions to analysis a single file in a project and get the results back.
    /// 
    /// To analyze a file the tree should be updated with a call to UpdateTree and then PreParse
    /// should be called on all files.  Finally Parse should then be called on all files.
    /// </summary>
    internal sealed class GeneroProjectEntry : IGeneroProjectEntry
    {
        private readonly string _moduleName;
        private readonly string _filePath;
        //private readonly ModuleInfo _myScope;
        private IAnalysisCookie _cookie;
        private GeneroAst _tree;
        //private AnalysisUnit _unit;
        private int _analysisVersion;
        private Dictionary<object, object> _properties = new Dictionary<object, object>();
        private ManualResetEventSlim _curWaiter;
        private int _updatesPending, _waiters;

        // we expect to have at most 1 waiter on updated project entries, so we attempt to share the event.
        private static ManualResetEventSlim _sharedWaitEvent = new ManualResetEventSlim(false);

        internal GeneroProjectEntry(string moduleName, string filePath, IAnalysisCookie cookie)
        {
            _moduleName = moduleName ?? "";
            _filePath = filePath;
            _cookie = cookie;
            //_myScope = new ModuleInfo(_moduleName, this, state.Interpreter.CreateModuleContext());
            //_unit = new AnalysisUnit(_tree, _myScope.Scope);
            //AnalysisLog.NewUnit(_unit);
        }

        public event EventHandler<EventArgs> OnNewParseTree;
        public event EventHandler<EventArgs> OnNewAnalysis;

        public void UpdateTree(GeneroAst newAst, IAnalysisCookie newCookie)
        {
            lock (this)
            {
                if (_updatesPending > 0)
                {
                    _updatesPending--;
                }
                if (newAst == null)
                {
                    // there was an error in parsing, just let the waiter go...
                    if (_curWaiter != null)
                    {
                        _curWaiter.Set();
                    }
                    _tree = null;
                    return;
                }

                _tree = newAst;
                if(_cookie == null || _cookie is FileCookie || !(newCookie is FileCookie))
                    _cookie = newCookie;

                if (_curWaiter != null)
                {
                    _curWaiter.Set();
                }
            }

            var newParse = OnNewParseTree;
            if (newParse != null)
            {
                newParse(this, EventArgs.Empty);
            }
        }

        public void GetTreeAndCookie(out GeneroAst tree, out IAnalysisCookie cookie)
        {
            lock (this)
            {
                tree = _tree;
                cookie = _cookie;
            }
        }

        public void BeginParsingTree()
        {
            lock (this)
            {
                _updatesPending++;
            }
        }

        public GeneroAst WaitForCurrentTree(int timeout = -1)
        {
            lock (this)
            {
                if (_updatesPending == 0)
                {
                    return _tree;
                }

                _waiters++;
                if (_curWaiter == null)
                {
                    _curWaiter = Interlocked.Exchange(ref _sharedWaitEvent, null);
                    if (_curWaiter == null)
                    {
                        _curWaiter = new ManualResetEventSlim(false);
                    }
                    else
                    {
                        _curWaiter.Reset();
                    }
                }
            }

            bool gotNewTree = _curWaiter.Wait(timeout);

            lock (this)
            {
                _waiters--;
                if (_waiters == 0 &&
                    Interlocked.CompareExchange(ref _sharedWaitEvent, _curWaiter, null) != null)
                {
                    _curWaiter.Dispose();
                }
                _curWaiter = null;
            }

            return gotNewTree ? _tree : null;
        }

        public void Analyze(CancellationToken cancel)
        {
            Analyze(cancel, false);
        }

        public void Analyze(CancellationToken cancel, bool enqueueOnly)
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }
            lock (this)
            {
                _analysisVersion++;

                Parse(enqueueOnly, cancel);
            }

            var newAnalysis = OnNewAnalysis;
            if (newAnalysis != null)
            {
                newAnalysis(this, EventArgs.Empty);
            }
        }

        public int AnalysisVersion
        {
            get
            {
                return _analysisVersion;
            }
        }

        public bool IsAnalyzed
        {
            get
            {
                return _tree != null;
            }
        }

        private void Parse(bool enqueOnly, CancellationToken cancel)
        {
            GeneroAst tree;
            IAnalysisCookie cookie;
            GetTreeAndCookie(out tree, out cookie);
            if (tree == null)
            {
                return;
            }
        }

        public string GetLine(int lineNo)
        {
            return _cookie.GetLine(lineNo);
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public IAnalysisCookie Cookie
        {
            get { return _cookie; }
        }

        public string ModuleName
        {
            get
            {
                return _moduleName;
            }
        }

        public Dictionary<object, object> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<object, object>();
                }
                return _properties;
            }
        }

        public void RemovedFromProject()
        {
            _analysisVersion = -1;
        }

        public GeneroAst Analysis
        {
            get { return _tree; }
        }

        public void SetProject(IGeneroProject project)
        {
            _parentProject = project;
        }

        private IGeneroProject _parentProject;
        public IGeneroProject ParentProject
        {
            get { return _parentProject; }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                _isOpen = value;
            }
        }

        private HashSet<string> _lastImportedModules;
        public void UpdateImportedProjects(string filename, GeneroAst ast)
        {
            if (VSGeneroPackage.Instance.GlobalFunctionProvider != null)
            {
                if (_lastImportedModules == null)
                    _lastImportedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var modules = ast.GetImportedModules().ToList();
                HashSet<string> currentlyImportedModules = new HashSet<string>(_lastImportedModules, StringComparer.OrdinalIgnoreCase);
                _lastImportedModules.Clear();
                VSGeneroPackage.Instance.GlobalFunctionProvider.SetFilename(filename);
                foreach (var mod in modules.Select(x => VSGeneroPackage.Instance.GlobalFunctionProvider.GetImportModuleFilename(x)).Where(y => y != null))
                {
                    if (!_lastImportedModules.Contains(mod))
                    {
                        var impProj = ParentProject.AddImportedModule(mod);
                        if (impProj != null)
                        {
                            _lastImportedModules.Add(mod);
                            impProj.ReferencingProjectEntries.Add(this);
                        }
                    }
                    else
                        currentlyImportedModules.Remove(mod);
                }

                // delete the leftovers
                foreach (var mod in currentlyImportedModules)
                {
                    ParentProject.RemoveImportedModule(mod);
                    _lastImportedModules.Remove(mod);
                }
            }
        }

        public bool DetectCircularImports()
        {
            return false;
        }
    }

    public class GeneroProject : IGeneroProject
    {
        private readonly string _directory;
        public GeneroProject(string directory)
        {
            _directory = directory;
        }

        public IGeneroProject AddImportedModule(string path)
        {
            IGeneroProject refProj = null;
            if (!ReferencedProjects.TryGetValue(path, out refProj))
            {
                // need to tell the genero project analyzer to add a directory to the project
                refProj = VSGeneroPackage.Instance.DefaultAnalyzer.AddImportedProject(path);
                if (refProj == null)
                {
                    // TODO: need to report that the project is invalid?
                }
                else
                {
                    ReferencedProjects.AddOrUpdate(path, refProj, (x, y) => y);
                }
            }
            return refProj;
        }

        public void RemoveImportedModule(string path)
        {
            VSGeneroPackage.Instance.DefaultAnalyzer.RemoveImportedProject(path);
            if (ReferencedProjects.ContainsKey(path))
            {
                IGeneroProject remEntry;
                ReferencedProjects.TryRemove(path, out remEntry);
            }
        }

        private ConcurrentDictionary<string, IGeneroProjectEntry> _projectEntries;
        public ConcurrentDictionary<string, IGeneroProjectEntry> ProjectEntries
        {
            get
            {
                if (_projectEntries == null)
                    _projectEntries = new ConcurrentDictionary<string, IGeneroProjectEntry>(StringComparer.OrdinalIgnoreCase);
                return _projectEntries;
            }
        }

        private ConcurrentDictionary<string, IGeneroProject> _referencedProjects;
        public ConcurrentDictionary<string, IGeneroProject> ReferencedProjects
        {
            get
            {
                if (_referencedProjects == null)
                    _referencedProjects = new ConcurrentDictionary<string, IGeneroProject>(StringComparer.OrdinalIgnoreCase);
                return _referencedProjects;
            }
        }

        public string Directory
        {
            get { return _directory; }
        }

        private HashSet<IGeneroProjectEntry> _referencingProjectEntries;
        public HashSet<IGeneroProjectEntry> ReferencingProjectEntries
        {
            get
            {
                if (_referencingProjectEntries == null)
                    _referencingProjectEntries = new HashSet<IGeneroProjectEntry>();
                return _referencingProjectEntries;
            }
        }
    }

    /// <summary>
    /// Represents a unit of work which can be analyzed.
    /// </summary>
    public interface IAnalyzable
    {
        void Analyze(CancellationToken cancel);
    }

    /// <summary>
    /// Represents a file which is capable of being analyzed.  Can be cast to other project entry types
    /// for more functionality.  See also IPythonProjectEntry and IXamlProjectEntry.
    /// </summary>
    public interface IProjectEntry : IAnalyzable
    {
        /// <summary>
        /// Returns true if the project entry has been parsed and analyzed.
        /// </summary>
        bool IsAnalyzed { get; }

        /// <summary>
        /// Returns the current analysis version of the project entry.
        /// </summary>
        int AnalysisVersion
        {
            get;
        }

        /// <summary>
        /// Returns the project entries file path.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets the specified line of text from the project entry.
        /// </summary>
        string GetLine(int lineNo);

        /// <summary>
        /// Provides storage of arbitrary properties associated with the project entry.
        /// </summary>
        Dictionary<object, object> Properties
        {
            get;
        }

        /// <summary>
        /// Called when the project entry is removed from the project.
        /// 
        /// Implementors of this method must ensure this method is thread safe.
        /// </summary>
        void RemovedFromProject();
    }

    /// <summary>
    /// Used to track information about where the analysis came from and
    /// get back the original content.
    /// </summary>
    public interface IAnalysisCookie
    {
        string GetLine(int lineNo);
    }

    /// <summary>
    /// Represents a group of files that can be analyzed
    /// </summary>
    public interface IGeneroProject
    {
        IGeneroProject AddImportedModule(string path);
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

    public interface IGeneroProjectEntry : IProjectEntry
    {
        bool IsOpen { get; set; }

        IGeneroProject ParentProject { get; }

        string ModuleName { get; }

        /// <summary>
        /// Returns the last parsed AST.
        /// </summary>
        GeneroAst Analysis
        {
            get;
        }

        event EventHandler<EventArgs> OnNewParseTree;
        event EventHandler<EventArgs> OnNewAnalysis;

        /// <summary>
        /// Informs the project entry that a new tree will soon be available and will be provided by
        /// a call to UpdateTree.  Calling this method will cause WaitForCurrentTree to block until
        /// UpdateTree has been called.
        /// 
        /// Calls to BeginParsingTree should be balanced with calls to UpdateTree.
        /// 
        /// This method is thread safe.
        /// </summary>
        void BeginParsingTree();

        void UpdateTree(GeneroAst ast, IAnalysisCookie fileCookie);
        void GetTreeAndCookie(out GeneroAst ast, out IAnalysisCookie cookie);
        void UpdateImportedProjects(string filename, GeneroAst ast);
        bool DetectCircularImports();
        /// <summary>
        /// Returns the current tree if no parsing is currently pending, otherwise waits for the 
        /// current parse to finish and returns the up-to-date tree.
        /// </summary>
        GeneroAst WaitForCurrentTree(int timeout = -1);

        void SetProject(IGeneroProject project);
    }
}
