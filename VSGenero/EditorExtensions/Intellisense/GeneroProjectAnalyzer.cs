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
        public readonly IProjectEntry ProjectEntry;

        public MonitoredBufferResult(BufferParser bufferParser, IProjectEntry projectEntry)
        {
            BufferParser = bufferParser;
            ProjectEntry = projectEntry;
        }
    }

    public class GeneroProjectAnalyzer : IDisposable
    {
        private readonly ParseQueue _queue;
        private readonly AnalysisQueue _analysisQueue;
        private readonly Dictionary<BufferParser, IProjectEntry> _openFiles = new Dictionary<BufferParser, IProjectEntry>();
        private readonly ConcurrentDictionary<string, IGeneroProject> _projects;
        private readonly bool _implicitProject;
        private readonly AutoResetEvent _queueActivityEvent = new AutoResetEvent(false);

        private int _userCount;

        // Internal for tests
        internal readonly IErrorProviderFactory _errorProvider;

        private static readonly Lazy<TaskProvider> _taskProvider = new Lazy<TaskProvider>(() =>
        {
            var _errorList = VSGeneroPackage.GetGlobalService(typeof(SVsErrorList)) as IVsTaskList;
            return new TaskProvider(_errorList);
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        private object _contentsLock = new object();

        internal GeneroProjectAnalyzer(IErrorProviderFactory errorProvider, bool implicitProject = true)
        {
            _errorProvider = errorProvider;

            _queue = new ParseQueue(this);
            _analysisQueue = new AnalysisQueue(this);

            _implicitProject = implicitProject;

            _projects = new ConcurrentDictionary<string, IGeneroProject>(StringComparer.OrdinalIgnoreCase);

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
        /// Starts monitoring a buffer for changes so we will re-parse the buffer to update the analysis
        /// as the text changes.
        /// </summary>
        internal MonitoredBufferResult MonitorTextBuffer(ITextView textView, ITextBuffer buffer)
        {
            IGeneroProjectEntry projEntry = CreateProjectEntry(buffer, new SnapshotCookie(buffer.CurrentSnapshot));
            var bufferParser = _queue.EnqueueBuffer(projEntry, textView, buffer);
            return new MonitoredBufferResult(bufferParser, projEntry);
        }

        private IGeneroProjectEntry CreateProjectEntry(ITextBuffer buffer, IAnalysisCookie analysisCookie)
        {
            IGeneroProjectEntry entry = null;
            string path = buffer.GetFilePath();
            if (path != null)
            {
                string dirPath = Path.GetDirectoryName(path);
                IGeneroProject projEntry;
                if (!_projects.TryGetValue(dirPath, out projEntry))
                {
                    if (buffer.ContentType.IsOfType(VSGeneroConstants.ContentType4GL) ||
                        buffer.ContentType.IsOfType(VSGeneroConstants.ContentTypeINC) ||
                        buffer.ContentType.IsOfType(VSGeneroConstants.ContentTypePER))
                    {
                        string moduleName = null;   // TODO: get module name from provider (if provider is null, take the file's directory name)
                        IAnalysisCookie cookie = null;
                        entry = new ProjectEntry(moduleName, path, cookie);
                    }

                    if (entry != null)
                    {
                        GeneroProject proj = new GeneroProject();
                        proj.ProjectEntries.AddOrUpdate(path, entry, (x, y) => y);
                        entry.SetProject(proj);
                        _projects.AddOrUpdate(dirPath, proj, (x, y) => y);

                        if (ImplicitProject && ShouldAnalyzePath(path))
                        { // don't analyze std lib
                            QueueDirectoryAnalysis(dirPath, path);
                        }
                    }
                }
                else
                {
                    if (!projEntry.ProjectEntries.TryGetValue(path, out entry))
                    {
                        if (buffer.ContentType.IsOfType(VSGeneroConstants.ContentType4GL) ||
                            buffer.ContentType.IsOfType(VSGeneroConstants.ContentTypeINC) ||
                            buffer.ContentType.IsOfType(VSGeneroConstants.ContentTypePER))
                        {
                            string moduleName = null;   // TODO: get module name from provider (if provider is null, take the file's directory name)
                            IAnalysisCookie cookie = null;
                            entry = new ProjectEntry(moduleName, path, cookie);
                        }
                    }
                    if (entry != null)
                    {
                        projEntry.ProjectEntries.AddOrUpdate(path, entry, (x, y) => y);
                        entry.SetProject(projEntry);
                        _projects.AddOrUpdate(dirPath, projEntry, (x, y) => y);

                        if (ImplicitProject && ShouldAnalyzePath(path))
                        { // don't analyze std lib
                            QueueDirectoryAnalysis(dirPath, path);
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

        private void ProjectEntryAnalyzed(string path, IGeneroProjectEntry projEntry)
        {
            string dirPath = Path.GetDirectoryName(path);
            IGeneroProject proj;
            if (_projects.TryGetValue(dirPath, out proj))
            {
                proj.ProjectEntries.AddOrUpdate(path, projEntry, (x, y) => y);
            }
        }

        private void QueueDirectoryAnalysis(string path, string excludeFile = null)
        {
            ThreadPool.QueueUserWorkItem(x => { lock (_contentsLock) { AnalyzeDirectory(CommonUtils.NormalizeDirectoryPath(path), excludeFile, ProjectEntryAnalyzed); } });
        }

        private bool ShouldAnalyzePath(string path)
        {
            // For now, we're going to assume that the scope of the program
            // is the directory in which the specified file resides.
            return true;
        }

        internal void StopMonitoringTextBuffer(BufferParser bufferParser)
        {
            bufferParser.StopMonitoring();
            string path = bufferParser._currentProjEntry.FilePath;
            if (ImplicitProject && _taskProvider.IsValueCreated && path != null)
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
                            proj.ProjectEntries.Clear();
                            _projects.TryRemove(dirPath, out proj);

                            // remove the file from the error list
                            _taskProvider.Value.Clear(Path.GetDirectoryName(bufferParser._currentProjEntry.FilePath));
                        }
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
                        entry = new ProjectEntry(moduleName, path, cookie);
                    }

                    if (entry != null)
                    {
                        GeneroProject proj = new GeneroProject();
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
                            entry = new ProjectEntry(moduleName, path, cookie);
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
                                                             IDatabaseInformationProvider databaseProvider, bool forCompletion = true)
        {
            var buffer = snapshot.TextBuffer;
            Genero4glReverseParser parser = new Genero4glReverseParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);
            var exprRange = parser.GetExpressionRange(forCompletion);

            if (exprRange == null)
            {
                return ExpressionAnalysis.Empty;
            }

            string text = exprRange.Value.GetText();

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
                        databaseProvider);
                }
            }

            return ExpressionAnalysis.Empty;
        }

        /// <summary>
        /// Gets a CompletionList providing a list of possible members the user can dot through.
        /// </summary>
        internal static CompletionAnalysis GetCompletions(ITextSnapshot snapshot, ITrackingSpan span, ITrackingPoint point, CompletionOptions options,
                                                          IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider)
        {
            return TrySpecialCompletions(snapshot, span, point, options, functionProvider, databaseProvider); /*??
                   GetNormalCompletionContext(snapshot, span, point, options)*/
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
            var exprRange = parser.GetExpressionRange(1, out paramIndex, out sigStart, out lastKeywordArg, out isParameterName);
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

        internal void ParseFile(IProjectEntry projectEntry, string filename, Stream content, Severity indentationSeverity)
        {
            IGeneroProjectEntry pyEntry;
            IAnalysisCookie cookie = (IAnalysisCookie)new FileCookie(filename);
            if ((pyEntry = projectEntry as IGeneroProjectEntry) != null)
            {
                GeneroAst ast;
                CollectingErrorSink errorSink;
                ParseGeneroCode(content, indentationSeverity, pyEntry, out ast, out errorSink);

                if (ast != null)
                {
                    pyEntry.UpdateTree(ast, cookie);
                    _analysisQueue.Enqueue(pyEntry, AnalysisPriority.Normal);
                }
                else
                {
                    // notify that we failed to update the existing analysis
                    pyEntry.UpdateTree(null, null);
                }

                if (errorSink.Warnings.Count > 0 || errorSink.Errors.Count > 0)
                {
                    TaskProvider provider = GetTaskProviderAndClearProjectItems(projectEntry);
                    if (provider != null)
                    {
                        provider.ReplaceWarnings(projectEntry.FilePath, errorSink.Warnings);
                        provider.ReplaceErrors(projectEntry.FilePath, errorSink.Errors);

                        UpdateErrorList(errorSink, projectEntry.FilePath, provider);
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
                    }

                    // update squiggles for the buffer

                    // SimpleTagger says it's thread safe (http://msdn.microsoft.com/en-us/library/dd885186.aspx), but it's buggy...  
                    // Post the removing of squiggles to the UI thread so that we don't crash when we're racing with 
                    // updates to the buffer.  http://pytools.codeplex.com/workitem/142
                    var dispatcher = bufferParser.Dispatcher;

                    if (dispatcher != null)
                    {
                        var entry = bufferParser._currentProjEntry;
                        dispatcher.BeginInvoke((Action)(() =>
                        {
                            UpdateSquiggles(snapshot, entry, errorSink, false);
                        }));
                    }
                }
                else
                {
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
            }
        }

        private void UpdateSquiggles(
            ITextSnapshot snapshot,
            IProjectEntry entry,
            CollectingErrorSink errorSink,
            bool unresolvedImportWarning
        )
        {
            var squiggles = _errorProvider.GetErrorTagger(snapshot.TextBuffer);
            var provider = GetTaskProviderAndClearProjectItems(entry);

            squiggles.RemoveTagSpans(x => true);
            if (entry.FilePath != null)
            {
                foreach (ErrorResult warning in errorSink.Warnings)
                {
                    squiggles.CreateTagSpan(
                        CreateSpan(snapshot, warning.Span),
                        new ErrorTag(PredefinedErrorTypeNames.Warning, warning.Message)
                    );
                }

                foreach (ErrorResult error in errorSink.Errors)
                {
                    squiggles.CreateTagSpan(
                        CreateSpan(snapshot, error.Span),
                        new ErrorTag(PredefinedErrorTypeNames.SyntaxError, error.Message)
                    );
                }

                if (provider != null)
                {
                    provider.ReplaceWarnings(entry.FilePath, errorSink.Warnings);
                    provider.ReplaceErrors(entry.FilePath, errorSink.Errors);
                    UpdateErrorList(errorSink, entry.FilePath, provider);
                }
            }
        }

        private TaskProvider GetTaskProviderAndClearProjectItems(IProjectEntry projEntry)
        {
            if (VSGeneroPackage.Instance != null)
            {
                if (projEntry.FilePath != null)
                {
                    _taskProvider.Value.Clear(projEntry.FilePath);
                }
            }
            return _taskProvider.Value;
        }

        private void UpdateErrorList(CollectingErrorSink errorSink, string filepath, TaskProvider provider)
        {
            if (errorSink.Warnings.Count > 0)
            {
                OnWarningAdded(filepath);
            }
            else
            {
                OnWarningRemoved(filepath);
            }
            if (errorSink.Errors.Count > 0)
            {
                OnErrorAdded(filepath);
            }
            else
            {
                OnErrorRemoved(filepath);
            }

            if (provider != null && (errorSink.Errors.Count > 0 || errorSink.Warnings.Count > 0))
            {
                provider.UpdateTasks();
            }
        }

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
                                                                IFunctionInformationProvider functionProvider, IDatabaseInformationProvider databaseProvider)
        {
            var snapSpan = span.GetSpan(snapshot);
            var buffer = snapshot.TextBuffer;
            var classifier = buffer.GetGeneroClassifier();
            if (classifier == null)
            {
                return null;
            }
            var start = snapSpan.Start;

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
            else if ((tokens = classifier.GetClassificationSpans(snapSpan.Start.GetContainingLine().ExtentIncludingLineBreak)).Count > 0 &&
             tokens[0].ClassificationType == classifier.Provider.StringLiteral)
            {
                // multi-line string, no string completions.
                return CompletionAnalysis.EmptyCompletionContext;
            }

            var entry = (IGeneroProjectEntry)buffer.GetAnalysis();
            if (entry != null && entry.Analysis != null)
            {
                var members = entry.Analysis.GetContextMembersByIndex(start, parser, functionProvider, databaseProvider);
                if (members != null)
                {
                    return new ContextSensitiveCompletionAnalysis(members, span, buffer, options);
                }
            }

            return CompletionAnalysis.EmptyCompletionContext; ;
        }

        private static CompletionAnalysis GetNormalCompletionContext(ITextSnapshot snapshot, ITrackingSpan applicableSpan, ITrackingPoint point, CompletionOptions options)
        {
            var span = applicableSpan.GetSpan(snapshot);

            if (IsSpaceCompletion(snapshot, point) && !IntellisenseController.ForceCompletions)
            {
                return CompletionAnalysis.EmptyCompletionContext;
            }

            var parser = new Genero4glReverseParser(snapshot, snapshot.TextBuffer, applicableSpan);
            if (parser.IsInGrouping())
            {
                options = options.Clone();
                options.IncludeStatementKeywords = false;
            }

            return new NormalCompletionAnalysis(
                snapshot.TextBuffer.GetAnalyzer(),
                snapshot,
                applicableSpan,
                snapshot.TextBuffer,
                options
            );
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

        private void AnalyzeDirectoryWorker(string dir, string _excludeFile, Action<string, IGeneroProjectEntry> onFileAnalyzed, CancellationToken cancel)
        {
            if (string.IsNullOrEmpty(dir))
            {
                Debug.Assert(false, "Unexpected empty dir");
                return;
            }

            try
            {
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

            //try
            //{
            //    foreach (string filename in Directory.GetFiles(dir, "*.per"))
            //    {
            //        if (cancel.IsCancellationRequested)
            //        {
            //            break;
            //        }
            //        IProjectEntry entry = AnalyzeFile(filename);
            //        if (onFileAnalyzed != null)
            //        {
            //            onFileAnalyzed(entry);
            //        }
            //    }
            //}
            //catch (IOException)
            //{
            //    // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
            //}
            //catch (UnauthorizedAccessException)
            //{
            //}
        }

        internal void Cancel()
        {
            _analysisQueue.Stop();
        }

        internal void RemoveErrors(IProjectEntry entry, bool suppressUpdate)
        {
            if (entry != null && entry.FilePath != null)
            {
                if (_taskProvider.IsValueCreated)
                {
                    // _taskProvider may not be created if we've never opened a Python file and
                    // none of the project files have errors
                    _taskProvider.Value.Clear(entry.FilePath, !suppressUpdate);
                }
                OnWarningRemoved(entry.FilePath);
                OnErrorRemoved(entry.FilePath);
            }
        }

        private void OnWarningAdded(string path)
        {
            var evt = WarningAdded;
            if (evt != null)
            {
                evt(this, new FileEventArgs(path));
            }
        }

        private void OnWarningRemoved(string path)
        {
            var evt = WarningRemoved;
            if (evt != null)
            {
                evt(this, new FileEventArgs(path));
            }
        }

        private void OnErrorAdded(string path)
        {
            var evt = ErrorAdded;
            if (evt != null)
            {
                evt(this, new FileEventArgs(path));
            }
        }

        private void OnErrorRemoved(string path)
        {
            var evt = ErrorRemoved;
            if (evt != null)
            {
                evt(this, new FileEventArgs(path));
            }
        }

        internal EventHandler<FileEventArgs> WarningAdded;
        internal EventHandler<FileEventArgs> WarningRemoved;
        internal EventHandler<FileEventArgs> ErrorAdded;
        internal EventHandler<FileEventArgs> ErrorRemoved;

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

        class TaskProvider : IVsTaskProvider
        {
            private readonly Dictionary<string, List<ErrorResult>> _warnings = new Dictionary<string, List<ErrorResult>>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, List<ErrorResult>> _errors = new Dictionary<string, List<ErrorResult>>(StringComparer.OrdinalIgnoreCase);
            private readonly uint _cookie;
            private readonly IVsTaskList _errorList;
            private readonly object _contentsLock = new object();

            private class WorkerMessage
            {
                public enum MessageType { Clear, Warnings, Errors, Update }
                public MessageType Type;
                public string Filename;
                public List<ErrorResult> Errors;

                public readonly static WorkerMessage Update = new WorkerMessage { Type = MessageType.Update };
            }
            private bool _hasWorker;
            private readonly BlockingCollection<WorkerMessage> _workerQueue;

            public TaskProvider(IVsTaskList errorList)
            {
                _errorList = errorList;
                if (_errorList != null)
                {
                    ErrorHandler.ThrowOnFailure(_errorList.RegisterTaskProvider(this, out _cookie));
                }
                _workerQueue = new BlockingCollection<WorkerMessage>();
            }

            private void Worker(object param)
            {
                bool changed = false;
                WorkerMessage msg;
                var lastUpdateTime = DateTime.Now;

                for (; ; )
                {
                    // Give queue up to 1 second to have a message in it before exiting loop
                    while (_workerQueue.TryTake(out msg, 1000))
                    {
                        switch (msg.Type)
                        {
                            case WorkerMessage.MessageType.Clear:
                                lock (_contentsLock)
                                {
                                    if (Path.HasExtension(msg.Filename))
                                    {
                                        changed = _errors.Remove(msg.Filename) || changed;
                                        changed = _warnings.Remove(msg.Filename) || changed;
                                    }
                                    else
                                    {
                                        foreach(var key in _errors.Keys.Where(x => x.StartsWith(msg.Filename, StringComparison.OrdinalIgnoreCase)).ToList())
                                            changed = _errors.Remove(key) || changed;
                                        foreach (var key in _warnings.Keys.Where(x => x.StartsWith(msg.Filename, StringComparison.OrdinalIgnoreCase)).ToList())
                                            changed = _warnings.Remove(key) || changed;
                                    }
                                }
                                break;
                            case WorkerMessage.MessageType.Warnings:
                                lock (_contentsLock)
                                {
                                    _warnings[msg.Filename] = msg.Errors;
                                }
                                changed = true;
                                break;
                            case WorkerMessage.MessageType.Errors:
                                lock (_contentsLock)
                                {
                                    _errors[msg.Filename] = msg.Errors;
                                }
                                changed = true;
                                break;
                            case WorkerMessage.MessageType.Update:
                                changed = true;
                                break;
                        }

                        // Batch refreshes over 1 second
                        if (changed && _errorList != null)
                        {
                            var currentTime = DateTime.Now;
                            if ((currentTime - lastUpdateTime).TotalMilliseconds > 1000)
                            {
                                RefreshTasks();
                                lastUpdateTime = currentTime;
                                changed = false;
                            }
                        }
                    }

                    lock (_workerQueue)
                    {
                        if (_workerQueue.Count == 0)
                        {
                            _hasWorker = false;
                            break;
                        }
                    }
                }

                // Handle refresh not handled in loop
                if (changed && _errorList != null)
                {
                    RefreshTasks();
                }
            }

            private void RefreshTasks()
            {
                try
                {
                    _errorList.RefreshTasks(_cookie);
                }
                catch (InvalidComObjectException)
                {
                    // DevDiv2 759317 - Watson bug, COM object can go away...
                }
            }

            private void SendMessage(WorkerMessage msg)
            {
                lock (_workerQueue)
                {
                    _workerQueue.Add(msg);
                    if (!_hasWorker)
                    {
                        _hasWorker = true;
                        ThreadPool.QueueUserWorkItem(Worker);
                    }
                }
            }

            public void UpdateTasks()
            {
                if (_errorList != null)
                {
                    SendMessage(WorkerMessage.Update);
                }
            }

            public uint Cookie
            {
                get
                {
                    return _cookie;
                }
            }

            #region IVsTaskProvider Members

            public int EnumTaskItems(out IVsEnumTaskItems ppenum)
            {
                lock (_contentsLock)
                {
                    ppenum = new TaskEnum(CopyErrorList(_warnings), CopyErrorList(_errors));
                }
                return VSConstants.S_OK;
            }

            private static Dictionary<string, ErrorResult[]> CopyErrorList(Dictionary<string, List<ErrorResult>> input)
            {
                Dictionary<string, ErrorResult[]> errors = new Dictionary<string, ErrorResult[]>(input.Count);
                foreach (var keyvalue in input)
                {
                    errors[keyvalue.Key] = keyvalue.Value.ToArray();
                }
                return errors;
            }

            public int ImageList(out IntPtr phImageList)
            {
                // not necessary if we report our category as build compile.
                phImageList = IntPtr.Zero;
                return VSConstants.E_NOTIMPL;
            }

            public int OnTaskListFinalRelease(IVsTaskList pTaskList)
            {
                return VSConstants.S_OK;
            }

            public int ReRegistrationKey(out string pbstrKey)
            {
                pbstrKey = null;
                return VSConstants.E_NOTIMPL;
            }

            public int SubcategoryList(uint cbstr, string[] rgbstr, out uint pcActual)
            {
                pcActual = 0;
                return VSConstants.S_OK;
            }

            #endregion

            /// <summary>
            /// Replaces the errors for the specified filename with the new set of errors.
            /// </summary>
            internal void ReplaceErrors(string filename, List<ErrorResult> errors)
            {
                if (errors.Count > 0)
                {
                    SendMessage(new WorkerMessage { Type = WorkerMessage.MessageType.Errors, Filename = filename, Errors = errors });
                }
            }

            /// <summary>
            /// Replaces the warnings for the specified filename with the new set of errors.
            /// </summary>
            internal void ReplaceWarnings(string filename, List<ErrorResult> warnings)
            {
                if (warnings.Count > 0)
                {
                    SendMessage(new WorkerMessage { Type = WorkerMessage.MessageType.Warnings, Filename = filename, Errors = warnings });
                }
            }

            internal void Clear(string filename)
            {
                Clear(filename, true);
            }

            internal void Clear(string filename, bool updateList)
            {
                SendMessage(new WorkerMessage { Type = WorkerMessage.MessageType.Clear, Filename = filename });
                if (updateList)
                {
                    SendMessage(WorkerMessage.Update);
                }
            }

            class TaskEnum : IVsEnumTaskItems
            {
                private readonly Dictionary<string, ErrorResult[]> _warnings;
                private readonly Dictionary<string, ErrorResult[]> _errors;
                private IEnumerator<ErrorInfo> _enum;

                public TaskEnum(Dictionary<string, ErrorResult[]> warnings, Dictionary<string, ErrorResult[]> errors)
                {
                    _warnings = warnings;
                    _errors = errors;
                    _enum = Enumerator(warnings, errors);
                }

                struct ErrorInfo
                {
                    public readonly string Filename;
                    public readonly ErrorResult Error;
                    public readonly bool IsError;

                    public ErrorInfo(string filename, ErrorResult error, bool isError)
                    {
                        Filename = filename;
                        Error = error;
                        IsError = isError;
                    }
                }

                IEnumerator<ErrorInfo> Enumerator(Dictionary<string, ErrorResult[]> warnings, Dictionary<string, ErrorResult[]> errors)
                {
                    foreach (var fileAndErrorList in warnings)
                    {
                        foreach (var error in fileAndErrorList.Value)
                        {
                            yield return new ErrorInfo(fileAndErrorList.Key, error, false);
                        }
                    }

                    foreach (var fileAndErrorList in errors)
                    {
                        foreach (var error in fileAndErrorList.Value)
                        {
                            yield return new ErrorInfo(fileAndErrorList.Key, error, true);
                        }
                    }
                }

                #region IVsEnumTaskItems Members

                public int Clone(out IVsEnumTaskItems ppenum)
                {
                    ppenum = new TaskEnum(_warnings, _errors);
                    return VSConstants.S_OK;
                }

                public int Next(uint celt, IVsTaskItem[] rgelt, uint[] pceltFetched = null)
                {
                    for (int i = 0; i < celt && _enum.MoveNext(); i++)
                    {
                        var next = _enum.Current;
                        pceltFetched[0] = (uint)i + 1;
                        rgelt[i] = new TaskItem(next.Error, next.Filename, next.IsError);
                    }

                    return VSConstants.S_OK;
                }

                public int Reset()
                {
                    _enum = Enumerator(_warnings, _errors);
                    return VSConstants.S_OK;
                }

                public int Skip(uint celt)
                {
                    while (celt != 0 && _enum.MoveNext())
                    {
                        celt--;
                    }
                    return VSConstants.S_OK;
                }

                #endregion

                class TaskItem : IVsTaskItem
                {
                    private readonly ErrorResult _error;
                    private readonly string _path;
                    private readonly bool _isError;

                    public TaskItem(ErrorResult error, string path, bool isError)
                    {
                        _error = error;
                        _path = path;
                        _isError = isError;
                    }

                    public SourceSpan Span
                    {
                        get
                        {
                            return _error.Span;
                        }
                    }

                    public string Message
                    {
                        get
                        {
                            return _error.Message;
                        }
                    }

                    #region IVsTaskItem Members

                    public int CanDelete(out int pfCanDelete)
                    {
                        pfCanDelete = 0;
                        return VSConstants.S_OK;
                    }

                    public int Category(VSTASKCATEGORY[] pCat)
                    {
                        pCat[0] = VSTASKCATEGORY.CAT_BUILDCOMPILE;
                        return VSConstants.S_OK;
                    }

                    public int Column(out int piCol)
                    {
                        if (Span.Start.Line == 1 && Span.Start.Column == 1 && Span.Start.Index != 0)
                        {
                            // we don't have the column number calculated
                            piCol = 0;
                            return VSConstants.E_FAIL;
                        }
                        piCol = Span.Start.Column - 1;
                        return VSConstants.S_OK;
                    }

                    public int Document(out string pbstrMkDocument)
                    {
                        pbstrMkDocument = _path;
                        return VSConstants.S_OK;
                    }

                    public int HasHelp(out int pfHasHelp)
                    {
                        pfHasHelp = 0;
                        return VSConstants.S_OK;
                    }

                    public int ImageListIndex(out int pIndex)
                    {
                        pIndex = 0;
                        return VSConstants.E_NOTIMPL;
                    }

                    public int IsReadOnly(VSTASKFIELD field, out int pfReadOnly)
                    {
                        pfReadOnly = 1;
                        return VSConstants.S_OK;
                    }

                    public int Line(out int piLine)
                    {
                        if (Span.Start.Line == 1 && Span.Start.Column == 1 && Span.Start.Index != 0)
                        {
                            // we don't have the line number calculated
                            piLine = 0;
                            return VSConstants.E_FAIL;
                        }
                        piLine = Span.Start.Line - 1;
                        return VSConstants.S_OK;
                    }

                    public int NavigateTo()
                    {
                        try
                        {
                            if (Span.Start.Line == 1 && Span.Start.Column == 1 && Span.Start.Index != 0)
                            {
                                // we have just an absolute index, use that to naviagte
                                VSGeneroPackage.NavigateTo(_path, Guid.Empty, Span.Start.Index);
                            }
                            else
                            {
                                VSGeneroPackage.NavigateTo(_path, Guid.Empty, Span.Start.Line - 1, Span.Start.Column - 1);
                            }
                            return VSConstants.S_OK;
                        }
                        catch (DirectoryNotFoundException)
                        {
                            // This may happen when the error was in a file that's located inside a .zip archive.
                            // Let's walk the path and see if it is indeed the case.
                            string path = _path;
                            while (path != null)
                            {
                                if (File.Exists(path))
                                {
                                    var ext = Path.GetExtension(path);
                                    if (string.Equals(ext, ".zip", StringComparison.OrdinalIgnoreCase) || string.Equals(ext, ".egg", StringComparison.OrdinalIgnoreCase))
                                    {
                                        MessageBox.Show("Opening source files contained in .zip archives is not supported", "Cannot open file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        return VSConstants.S_FALSE;
                                    }
                                }
                                path = Path.GetDirectoryName(path);
                            }
                            // If it failed for some other reason, let caller handle it.
                            throw;
                        }
                    }

                    public int NavigateToHelp()
                    {
                        return VSConstants.E_NOTIMPL;
                    }

                    public int OnDeleteTask()
                    {
                        return VSConstants.E_NOTIMPL;
                    }

                    public int OnFilterTask(int fVisible)
                    {
                        return VSConstants.E_NOTIMPL;
                    }

                    public int SubcategoryIndex(out int pIndex)
                    {
                        pIndex = 0;
                        return VSConstants.E_NOTIMPL;
                    }

                    public int get_Checked(out int pfChecked)
                    {
                        pfChecked = 0;
                        return VSConstants.S_OK;
                    }

                    public int get_Priority(VSTASKPRIORITY[] ptpPriority)
                    {
                        ptpPriority[0] = _isError ? VSTASKPRIORITY.TP_HIGH : VSTASKPRIORITY.TP_NORMAL;
                        return VSConstants.S_OK;
                    }

                    public int get_Text(out string pbstrName)
                    {
                        pbstrName = Message;
                        return VSConstants.S_OK;
                    }

                    public int put_Checked(int fChecked)
                    {
                        return VSConstants.E_NOTIMPL;
                    }

                    public int put_Priority(VSTASKPRIORITY tpPriority)
                    {
                        return VSConstants.E_NOTIMPL;
                    }

                    public int put_Text(string bstrName)
                    {
                        return VSConstants.E_NOTIMPL;
                    }

                    #endregion
                }
            }
        }


        public void Dispose()
        {
            if (_taskProvider.IsValueCreated)
            {
                _taskProvider.Value.UpdateTasks();
            }

            _analysisQueue.Stop();
        }
    }
}
