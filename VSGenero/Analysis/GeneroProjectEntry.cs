using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;

namespace VSGenero.Analysis
{
    /// <summary>
    /// Provides interactions to analysis a single file in a project and get the results back.
    /// 
    /// To analyze a file the tree should be updated with a call to UpdateTree and then PreParse
    /// should be called on all files.  Finally Parse should then be called on all files.
    /// </summary>
    internal class GeneroProjectEntry : IGeneroProjectEntry
    {
        private readonly string _moduleName;
        private readonly string _filePath;
        protected readonly bool _shouldAnalyzeDir;
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

        public bool IsErrorChecked { get; set; }

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

        public virtual void UpdateIncludesAndImports(string filename, GeneroAst ast)
        {
        }

        public IEnumerable<IGeneroProjectEntry> GetIncludedFiles()
        {
            return VSGeneroPackage.Instance.DefaultAnalyzer.GetIncludedFiles(this).ToList();
        }

        public bool DetectCircularImports()
        {
            return false;
        }

        public bool CanErrorCheck
        {
            get
            {
                return this.ParentProject.ProjectEntries.Select(x => x.Value).All(y =>
                {
                    if (!y.IsAnalyzed)
                    {
                        return false;
                    }

                    if (y.ParentProject.ReferencedProjects.Count > 0)
                    {
                        // check to see if all entries in each of the referenced project have been error checked
                        return y.ParentProject.ReferencedProjects.SelectMany(a => a.Value.ProjectEntries).All(x => x.Value.IsErrorChecked);
                    }

                    if (GetIncludedFiles().Any(x => !x.IsAnalyzed))
                        return false;
                    return true;
                });
            }
        }

        private bool _preventErrorCheck;
        private object _preventErrorCheckLock = new object();
        public bool PreventErrorCheck
        {
            get
            {
                lock (_preventErrorCheckLock)
                {
                    return _preventErrorCheck;
                }
            }
            set
            {
                lock (_preventErrorCheckLock)
                {
                    _preventErrorCheck = value;
                }
            }
        }
    }
}
