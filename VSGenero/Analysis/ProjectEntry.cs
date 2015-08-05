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
        private readonly bool _shouldAnalyzeDir;
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

        internal GeneroProjectEntry(string moduleName, string filePath, IAnalysisCookie cookie, bool shouldAnalyzeDir)
        {
            _moduleName = moduleName ?? "";
            _filePath = filePath;
            _cookie = cookie;
            _shouldAnalyzeDir = shouldAnalyzeDir;
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
                if (_cookie == null || _cookie is FileCookie || !(newCookie is FileCookie))
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

        public void UpdateIncludesAndImports(string filename, GeneroAst ast)
        {
            if (_shouldAnalyzeDir && VSGeneroPackage.Instance.ProgramFileProvider != null)
            {
                // first do imports
                if (_lastImportedModules == null)
                    _lastImportedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var modules = ast.GetImportedModules().ToList();
                HashSet<string> currentlyImportedModules = new HashSet<string>(_lastImportedModules, StringComparer.OrdinalIgnoreCase);
                VSGeneroPackage.Instance.ProgramFileProvider.SetFilename(filename);
                foreach (var mod in modules.Select(x => VSGeneroPackage.Instance.ProgramFileProvider.GetImportModuleFilename(x)).Where(y => y != null))
                {
                    if (!_lastImportedModules.Contains(mod))
                    {
                        var impProj = ParentProject.AddImportedModule(mod);
                        if (impProj != null)
                        {
                            _lastImportedModules.Add(mod);
                            try
                            {
                                if (!impProj.ReferencingProjectEntries.Contains(this))
                                    // TODO: for some reason a NRE got thrown here, but nothing was apparently wrong
                                    impProj.ReferencingProjectEntries.Add(this);
                            }
                            catch(Exception)
                            {
                                int i = 0;
                            }
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

                // next do includes
                var includes = ast.GetIncludedFiles();
                HashSet<string> currentlyIncludedFiles = new HashSet<string>(VSGeneroPackage.Instance.DefaultAnalyzer.GetIncludedFiles(this).Select(x => x.FilePath), StringComparer.OrdinalIgnoreCase);
                foreach (var incl in includes.Select(x => VSGeneroPackage.Instance.ProgramFileProvider.GetIncludeFile(x)).Where(y => y != null))
                {
                    if (!VSGeneroPackage.Instance.DefaultAnalyzer.IsIncludeFileIncludedByProjectEntry(incl, this))
                    {
                        VSGeneroPackage.Instance.DefaultAnalyzer.AddIncludedFile(incl, this);
                    }
                    else
                        currentlyIncludedFiles.Remove(incl);
                }

                // delete the leftovers
                foreach (var include in currentlyIncludedFiles)
                {
                    IGeneroProjectEntry dummy;
                    VSGeneroPackage.Instance.DefaultAnalyzer.RemoveIncludedFile(include, this);
                }
            }
        }

        public IEnumerable<IGeneroProjectEntry> GetIncludedFiles()
        {
            return VSGeneroPackage.Instance.DefaultAnalyzer.GetIncludedFiles(this);
        }

        public bool DetectCircularImports()
        {
            return false;
        }
    }

    public class GeneroProject : IGeneroProject, IAnalysisResult
    {
        public string Typename
        {
            get { return null; }
        }

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

        private object _refProjEntriesLock = new object();
        private HashSet<IGeneroProjectEntry> _referencingProjectEntries;
        public HashSet<IGeneroProjectEntry> ReferencingProjectEntries
        {
            get
            {
                lock (_refProjEntriesLock)
                {
                    if (_referencingProjectEntries == null)
                        _referencingProjectEntries = new HashSet<IGeneroProjectEntry>();
                    return _referencingProjectEntries;
                }
            }
        }

        public string Scope
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string Name
        {
            get { return Path.GetFileName(_directory); }
        }

        public string Documentation
        {
            get
            {
                return string.Format("Imported module {0}", Name);
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location
        {
            get { return null; }
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return false;
        }

        public bool CanGetValueFromDebugger
        {
            get { return true; }
        }

        public bool IsPublic { get { return true; } }

        internal IAnalysisResult GetMemberOfType(string name, GeneroAst ast, bool vars, bool types, bool consts, bool funcs, out IProjectEntry definingProjEntry)
        {
            string projNamespace = string.Format("{0}", this.Name);
            if (name.StartsWith(projNamespace, StringComparison.OrdinalIgnoreCase))
                name = name.Substring(projNamespace.Length);

            definingProjEntry = null;
            IAnalysisResult res = null;
            foreach (var projEntry in ProjectEntries)
            {
                if (projEntry.Value.Analysis != null &&
                   projEntry.Value.Analysis.Body != null)
                {
                    IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                    if (modRes != null)
                    {
                        // check global vars, types, and constants
                        if ((vars && modRes.GlobalVariables.TryGetValue(name, out res)) ||
                            (types && modRes.GlobalTypes.TryGetValue(name, out res)) ||
                            (consts && modRes.GlobalConstants.TryGetValue(name, out res)))
                        {
                            //found = true;
                            definingProjEntry = projEntry.Value;
                            break;
                        }

                        if (((vars && modRes.Variables.TryGetValue(name, out res)) ||
                             (types && modRes.Types.TryGetValue(name, out res)) ||
                             (consts && modRes.Constants.TryGetValue(name, out res))) && res.IsPublic)
                        {
                            definingProjEntry = projEntry.Value;
                            break;
                        }

                        // check project functions
                        IFunctionResult funcRes = null;
                        if (funcs && modRes.Functions.TryGetValue(name, out funcRes))
                        {
                            if (funcRes.AccessModifier == AccessModifier.Public)
                            {
                                res = funcRes;
                                //found = true;
                                definingProjEntry = projEntry.Value;
                                break;
                            }
                        }
                    }
                }
            }
            if(res != null)
                res.SetOneTimeNamespace(projNamespace);
            return res;
        }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry)
        {
            definingProject = null;
            var res = GetMemberOfType(name, ast, true, true, true, true, out projEntry);
            if (projEntry != null && projEntry is IGeneroProjectEntry)
                definingProject = (projEntry as IGeneroProjectEntry).ParentProject;
            return res;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType)
        {
            string projNamespace = string.Format("{0}", this.Name);
            List<MemberResult> members = new List<MemberResult>();

            foreach (var projEntry in ProjectEntries)
            {
                if (projEntry.Value.Analysis != null &&
                   projEntry.Value.Analysis.Body != null)
                {
                    IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                    if (modRes != null)
                    {
                        if (memberType.HasFlag(MemberType.Variables))
                        {
                            members.AddRange(modRes.GlobalVariables.Select(x =>
                                {
                                    x.Value.SetOneTimeNamespace(projNamespace);
                                    return new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, ast);
                                }));
                            members.AddRange(modRes.Variables.Where(x => x.Value.IsPublic).Select(x =>
                            {
                                x.Value.SetOneTimeNamespace(projNamespace);
                                return new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, ast);
                            }));
                        }

                        if (memberType.HasFlag(MemberType.Types))
                        {
                            members.AddRange(modRes.GlobalTypes.Select(x =>
                            {
                                x.Value.SetOneTimeNamespace(projNamespace);
                                return new MemberResult(x.Key, x.Value, GeneroMemberType.Class, ast);
                            }));
                            members.AddRange(modRes.Types.Where(x => x.Value.IsPublic).Select(x =>
                            {
                                x.Value.SetOneTimeNamespace(projNamespace);
                                return new MemberResult(x.Key, x.Value, GeneroMemberType.Class, ast);
                            }));
                        }

                        if (memberType.HasFlag(MemberType.Constants))
                        {
                            members.AddRange(modRes.GlobalConstants.Select(x =>
                            {
                                x.Value.SetOneTimeNamespace(projNamespace);
                                return new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, ast);
                            }));
                            members.AddRange(modRes.Constants.Where(x => x.Value.IsPublic).Select(x =>
                            {
                                x.Value.SetOneTimeNamespace(projNamespace);
                                return new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, ast);
                            }));
                        }

                        if (memberType.HasFlag(MemberType.Functions))
                        {
                            members.AddRange(modRes.Functions.Where(x => x.Value.IsPublic).Select(x =>
                            {
                                x.Value.SetOneTimeNamespace(projNamespace);
                                return new MemberResult(x.Key, x.Value, GeneroMemberType.Method, ast);
                            }));
                        }
                    }
                }
            }

            return members;
        }


        public void SetOneTimeNamespace(string nameSpace)
        {
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
    /// for more functionality.  See also IGeneroProjectEntry
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
        /// Returns the project entry's file path.
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
        IEnumerable<IGeneroProjectEntry> GetIncludedFiles();

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
        void UpdateIncludesAndImports(string filename, GeneroAst ast);
        bool DetectCircularImports();
        /// <summary>
        /// Returns the current tree if no parsing is currently pending, otherwise waits for the 
        /// current parse to finish and returns the up-to-date tree.
        /// </summary>
        GeneroAst WaitForCurrentTree(int timeout = -1);

        void SetProject(IGeneroProject project);
    }
}
