/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.ComponentModel;
using VSGenero.EditorExtensions;
using Microsoft.VisualStudio.Text;
using System.Timers;
using VSGenero;
using System.IO;
using Microsoft.VisualStudio.VSCommon;
using Microsoft.VisualStudio.VSCommon.Utilities;
using System.ComponentModel.Composition;

namespace VSGenero.EditorExtensions
{
    public delegate void ParseCompleteEventHandler(object sender, ParseCompleteEventArgs e);
    public delegate void ModuleContentsUpdatedEventHandler(object sender, ModuleContentsUpdatedEventArgs e);

    public class ParseCompleteEventArgs : EventArgs
    {
        private GeneroFileParserManager _fpm;
        public GeneroFileParserManager FileParserManager
        {
            get { return _fpm; }
        }

        public ParseCompleteEventArgs(GeneroFileParserManager fpm)
        {
            _fpm = fpm;
        }
    }

    public class ModuleContentsUpdatedEventArgs : EventArgs
    {
        private GeneroModuleContents _contents;
        public GeneroModuleContents ModuleContents
        {
            get { return _contents; }
        }

        public ModuleContentsUpdatedEventArgs(GeneroModuleContents moduleContents)
        {
            _contents = moduleContents;
        }
    }

    public class GeneroFunctionReturn
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class DataType
    {
        public string Name { get; set; }
        public bool DimensionRequired { get; set; }
        public bool RangeRequired { get; set; }
    }

    public enum ArrayType
    {
        None,
        Static,
        Dynamic
    }

    public class GeneroModuleContents
    {
        private string _contentFilename;
        public string ContentFilename
        {
            get { return _contentFilename; }
            set { _contentFilename = value; }
        }

        public object CollectionLock = new object();

        private ConcurrentDictionary<string, FunctionDefinition> _functionDefs;
        public ConcurrentDictionary<string, FunctionDefinition> FunctionDefinitions
        {
            get
            {
                if (_functionDefs == null)
                    _functionDefs = new ConcurrentDictionary<string, FunctionDefinition>();
                return _functionDefs;
            }
        }

        private ConcurrentDictionary<string, VariableDefinition> _globalVariables;
        public ConcurrentDictionary<string, VariableDefinition> GlobalVariables
        {
            get
            {
                if (_globalVariables == null)
                    _globalVariables = new ConcurrentDictionary<string, VariableDefinition>();
                return _globalVariables;
            }
        }

        private ConcurrentDictionary<string, ConstantDefinition> _globalConstants;
        public ConcurrentDictionary<string, ConstantDefinition> GlobalConstants
        {
            get
            {
                if (_globalConstants == null)
                    _globalConstants = new ConcurrentDictionary<string, ConstantDefinition>();
                return _globalConstants;
            }
        }

        private ConcurrentDictionary<string, TypeDefinition> _globalTypes;
        public ConcurrentDictionary<string, TypeDefinition> GlobalTypes
        {
            get
            {
                if (_globalTypes == null)
                    _globalTypes = new ConcurrentDictionary<string, TypeDefinition>();
                return _globalTypes;
            }
        }

        private ConcurrentDictionary<string, VariableDefinition> _moduleVariables;
        public ConcurrentDictionary<string, VariableDefinition> ModuleVariables
        {
            get
            {
                if (_moduleVariables == null)
                    _moduleVariables = new ConcurrentDictionary<string, VariableDefinition>();
                return _moduleVariables;
            }
        }

        private ConcurrentDictionary<string, ConstantDefinition> _moduleConstants;
        public ConcurrentDictionary<string, ConstantDefinition> ModuleConstants
        {
            get
            {
                if (_moduleConstants == null)
                    _moduleConstants = new ConcurrentDictionary<string, ConstantDefinition>();
                return _moduleConstants;
            }
        }

        private ConcurrentDictionary<string, TypeDefinition> _moduleTypes;
        public ConcurrentDictionary<string, TypeDefinition> ModuleTypes
        {
            get
            {
                if (_moduleTypes == null)
                    _moduleTypes = new ConcurrentDictionary<string, TypeDefinition>();
                return _moduleTypes;
            }
        }

        // Will always contain the same contents. Holds system variables such as sqlca, Dialog, etc.
        private Dictionary<string, VariableDefinition> _systemVariables;
        public Dictionary<string, VariableDefinition> SystemVariables
        {
            get
            {
                if (_systemVariables == null)
                    _systemVariables = new Dictionary<string, VariableDefinition>();
                return _systemVariables;
            }
        }

        private ConcurrentDictionary<string, TempTableDefinition> _tempTables;
        public ConcurrentDictionary<string, TempTableDefinition> TempTables
        {
            get
            {
                if (_tempTables == null)
                    _tempTables = new ConcurrentDictionary<string, TempTableDefinition>();
                return _tempTables;
            }
        }

        private Dictionary<string, CursorPreparation> _sqlPrepares;
        public Dictionary<string, CursorPreparation> SqlPrepares
        {
            get
            {
                if (_sqlPrepares == null)
                    _sqlPrepares = new Dictionary<string, CursorPreparation>();
                return _sqlPrepares;
            }
        }

        private Dictionary<string, CursorDeclaration> _sqlCursors;
        public Dictionary<string, CursorDeclaration> SqlCursors
        {
            get
            {
                if (_sqlCursors == null)
                    _sqlCursors = new Dictionary<string, CursorDeclaration>();
                return _sqlCursors;
            }
        }

        public void Clear()
        {
            FunctionDefinitions.Clear();
            GlobalVariables.Clear();
            ModuleVariables.Clear();
            TempTables.Clear();
            SqlPrepares.Clear();
            SqlCursors.Clear();
        }
    }

    public class GeneroFileParserManager
    {
        private ITextBuffer _buffer;
        private GeneroParser _parser;
        private BackgroundWorker _parserThread;
        private Timer _delayedParseTimer;
        private ISynchronizeInvoke _synchObj;
        private bool _parseNeeded;
        private object _parseNeededLock;
        private bool _initialParseComplete;

        public event ParseCompleteEventHandler ParseComplete;

        private GeneroModuleContents _moduleContents;
        public GeneroModuleContents ModuleContents
        {
            get
            {
                return _moduleContents;
            }
        }

        public GeneroFileParserManager(ITextBuffer buffer, string primarySibling = null)
        {
            _initialParseComplete = false;
            _parseNeeded = false;
            _parseNeededLock = new object();
            _buffer = buffer;
            _buffer.Changed += _buffer_Changed;
            _parserThread = new BackgroundWorker();
            _parser = new GeneroParser(_parserThread, primarySibling);
            _parser.ModuleContentsChanged += new ModuleContentsUpdatedEventHandler(_parser_ModuleContentsChanged);
            _parserThread.WorkerReportsProgress = true;
            _parserThread.WorkerSupportsCancellation = true;
            _parserThread.DoWork += _parser.DoWork;
            _parserThread.RunWorkerCompleted += _parserThread_RunWorkerCompleted;
            _parserThread.ProgressChanged += _parserThread_ProgressChanged;
            _parserThread.RunWorkerAsync(_buffer);

            _synchObj = new GenericSynchronizingObject();
            _delayedParseTimer = new Timer();
            _delayedParseTimer.AutoReset = true;
            _delayedParseTimer.Interval = 1000;
            _delayedParseTimer.SynchronizingObject = _synchObj;
            _delayedParseTimer.Elapsed += new ElapsedEventHandler(_delayedParseTimer_Elapsed);
            _delayedParseTimer.Start();
        }

        public void UseNewBuffer(ITextBuffer buffer)
        {
            _buffer.Changed -= _buffer_Changed;
            _buffer = buffer;
            _buffer.Changed += _buffer_Changed;
        }

        private void UpdateModuleContents(GeneroModuleContents newModuleContents)
        {
            if (_moduleContents == null)
            {
                _moduleContents = new GeneroModuleContents();
                _moduleContents.ContentFilename = _buffer.GetFilePath();
            }

            // update the global variables dictionary
            foreach (var globalVarKvp in newModuleContents.GlobalVariables)
            {
                _moduleContents.GlobalVariables.AddOrUpdate(globalVarKvp.Key.ToLower(), globalVarKvp.Value, (x, y) => globalVarKvp.Value);
            }

            // update the global constants dictionary
            foreach (var globalConst in newModuleContents.GlobalConstants)
            {
                _moduleContents.GlobalConstants.AddOrUpdate(globalConst.Key.ToLower(), globalConst.Value, (x, y) => globalConst.Value);
            }

            // update the global types dictionary
            foreach (var globalType in newModuleContents.GlobalTypes)
            {
                _moduleContents.GlobalTypes.AddOrUpdate(globalType.Key.ToLower(), globalType.Value, (x, y) => globalType.Value);
            }

            // Update the module functions dictionary
            foreach (var programFuncKvp in newModuleContents.FunctionDefinitions.Where(x => !x.Value.Private))
            {
                _moduleContents.FunctionDefinitions.AddOrUpdate(programFuncKvp.Key.ToLower(), programFuncKvp.Value, (x, y) => programFuncKvp.Value);
            }
        }

        void _parser_ModuleContentsChanged(object sender, ModuleContentsUpdatedEventArgs e)
        {
            UpdateModuleContents(e.ModuleContents);

            if (_initialParseComplete)
            {
                // pass to the caller's callback
                if (ParseComplete != null)
                {
                    ParseComplete(this, new ParseCompleteEventArgs(this));
                }
            }

            string programName = Path.GetDirectoryName(e.ModuleContents.ContentFilename);
            VSGeneroPackage.Instance.ProgramContentsManager.AddProgramContents(programName, e.ModuleContents);
        }

        void _delayedParseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_parseNeededLock)
            {
                if (_parseNeeded)
                {
                    // may need to queue up to ensure the latest change gets parsed
                    if (!_parserThread.IsBusy)
                    {
                        _parserThread.RunWorkerAsync(_buffer);
                        _parseNeeded = false;
                    }
                }
            }
        }

        void _buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            // TODO: need to look into using the TextContentChangedEventArgs to make parsing more efficient.
            // We should be able to detect where the change occurred, and then at the very least, parse the function that contains the change
            // It would help if, on first parsing, we formulate a better document map, outlining sections (i.e. global var defs, module var defs, functions, reports, etc)
            lock (_parseNeededLock)
            {
                _parseNeeded = true;
            }
        }

        private void _parserThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void MergeNewContents(GeneroModuleContents newContents)
        {
            if (string.IsNullOrWhiteSpace(_moduleContents.ContentFilename))
                _moduleContents.ContentFilename = newContents.ContentFilename;

            // Function definitions
            foreach (var rem in _moduleContents.FunctionDefinitions.Where(x => x.Value.ContainingFile == newContents.ContentFilename &&
                                                                              !newContents.FunctionDefinitions.ContainsKey(x.Key.ToLower())).ToList())
            {
                FunctionDefinition temp;
                _moduleContents.FunctionDefinitions.TryRemove(rem.Key.ToLower(), out temp);
            }
            foreach (var funcDef in newContents.FunctionDefinitions)
            {
                _moduleContents.FunctionDefinitions.AddOrUpdate(funcDef.Key.ToLower(), funcDef.Value, (x, y) => funcDef.Value);
            }

            // Global variables
            foreach (var rem in _moduleContents.GlobalVariables.Where(x => x.Value.ContainingFile == newContents.ContentFilename &&
                                                                              !newContents.GlobalVariables.ContainsKey(x.Key.ToLower())).ToList())
            {
                VariableDefinition temp;
                _moduleContents.GlobalVariables.TryRemove(rem.Key.ToLower(), out temp);
            }
            foreach (var globDef in newContents.GlobalVariables)
            {
                _moduleContents.GlobalVariables.AddOrUpdate(globDef.Key.ToLower(), globDef.Value, (x, y) => globDef.Value);
            }

            // Module variables
            foreach (var rem in _moduleContents.ModuleVariables.Where(x => x.Value.ContainingFile == newContents.ContentFilename &&
                                                                              !newContents.ModuleVariables.ContainsKey(x.Key.ToLower())).ToList())
            {
                VariableDefinition temp;
                _moduleContents.ModuleVariables.TryRemove(rem.Key.ToLower(), out temp);
            }
            foreach (var modDef in newContents.ModuleVariables)
            {
                _moduleContents.ModuleVariables.AddOrUpdate(modDef.Key.ToLower(), modDef.Value, (x, y) => modDef.Value);
            }

            // SQL cursors
            foreach (var rem in _moduleContents.SqlCursors.Where(x => x.Value.ContainingFile == newContents.ContentFilename).ToList())
            {
                _moduleContents.SqlCursors.Remove(rem.Key.ToLower());
            }
            foreach (var sqlCursor in newContents.SqlCursors)
            {
                _moduleContents.SqlCursors.Add(sqlCursor.Key.ToLower(), sqlCursor.Value);
            }

            // SQL Prepares
            foreach (var rem in _moduleContents.SqlPrepares.Where(x => x.Value.ContainingFile == newContents.ContentFilename).ToList())
            {
                _moduleContents.SqlPrepares.Remove(rem.Key.ToLower());
            }
            foreach (var sqlPrep in newContents.SqlPrepares)
            {
                _moduleContents.SqlPrepares.Add(sqlPrep.Key.ToLower(), sqlPrep.Value);
            }

            foreach (var systemVar in newContents.SystemVariables)
            {
                if (!_moduleContents.SystemVariables.ContainsKey(systemVar.Key.ToLower()))
                    _moduleContents.SystemVariables.Add(systemVar.Key.ToLower(), systemVar.Value);
                else
                    _moduleContents.SystemVariables[systemVar.Key.ToLower()] = systemVar.Value;
            }

            // Temp tables
            foreach (var rem in _moduleContents.TempTables.Where(x => x.Value.ContainingFile == newContents.ContentFilename &&
                                                                              !newContents.TempTables.ContainsKey(x.Key.ToLower())).ToList())
            {
                TempTableDefinition temp;
                _moduleContents.TempTables.TryRemove(rem.Key.ToLower(), out temp);
            }
            foreach (var tempDef in newContents.TempTables)
            {
                _moduleContents.TempTables.AddOrUpdate(tempDef.Key.ToLower(), tempDef.Value, (x, y) => tempDef.Value);
            }

            // Global Constant defs
            foreach (var rem in _moduleContents.GlobalConstants.Where(x => x.Value.ContainingFile == newContents.ContentFilename &&
                                                                              !newContents.GlobalConstants.ContainsKey(x.Key.ToLower())).ToList())
            {
                ConstantDefinition temp;
                _moduleContents.GlobalConstants.TryRemove(rem.Key.ToLower(), out temp);
            }
            foreach (var tempDef in newContents.GlobalConstants)
            {
                _moduleContents.GlobalConstants.AddOrUpdate(tempDef.Key.ToLower(), tempDef.Value, (x, y) => tempDef.Value);
            }

            // Global Type defs
            foreach (var rem in _moduleContents.GlobalTypes.Where(x => x.Value.ContainingFile == newContents.ContentFilename &&
                                                                              !newContents.GlobalTypes.ContainsKey(x.Key.ToLower())).ToList())
            {
                TypeDefinition temp;
                _moduleContents.GlobalTypes.TryRemove(rem.Key.ToLower(), out temp);
            }
            foreach (var tempDef in newContents.GlobalTypes)
            {
                _moduleContents.GlobalTypes.AddOrUpdate(tempDef.Key.ToLower(), tempDef.Value, (x, y) => tempDef.Value);
            }

            // Module Constant defs
            foreach (var rem in _moduleContents.ModuleConstants.Where(x => x.Value.ContainingFile == newContents.ContentFilename &&
                                                                              !newContents.ModuleConstants.ContainsKey(x.Key.ToLower())).ToList())
            {
                ConstantDefinition temp;
                _moduleContents.ModuleConstants.TryRemove(rem.Key.ToLower(), out temp);
            }
            foreach (var tempDef in newContents.ModuleConstants)
            {
                _moduleContents.ModuleConstants.AddOrUpdate(tempDef.Key.ToLower(), tempDef.Value, (x, y) => tempDef.Value);
            }

            // Module Type defs
            foreach (var rem in _moduleContents.ModuleTypes.Where(x => x.Value.ContainingFile == newContents.ContentFilename &&
                                                                              !newContents.ModuleTypes.ContainsKey(x.Key.ToLower())).ToList())
            {
                TypeDefinition temp;
                _moduleContents.ModuleTypes.TryRemove(rem.Key.ToLower(), out temp);
            }
            foreach (var tempDef in newContents.ModuleTypes)
            {
                _moduleContents.ModuleTypes.AddOrUpdate(tempDef.Key.ToLower(), tempDef.Value, (x, y) => tempDef.Value);
            }
        }

        private void _parserThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                _initialParseComplete = true;

                if (_moduleContents == null)
                {
                    _moduleContents = new GeneroModuleContents();
                }

                var tempModuleContents = e.Result as GeneroModuleContents;

                // merge the module contents
                MergeNewContents(tempModuleContents);

                // pass to the caller's callback
                if (ParseComplete != null)
                {
                    ParseComplete(this, new ParseCompleteEventArgs(this));
                }

                if (_parser.PrimarySibling == null)
                {
                    // this is the primary file, so add contents directly to the Content manager
                    string programName = Path.GetDirectoryName(tempModuleContents.ContentFilename);
                    VSGeneroPackage.Instance.ProgramContentsManager.AddProgramContents(programName, _moduleContents);
                }
            }
        }

        public bool IsInitialParseComplete
        {
            get { return _initialParseComplete; }
        }

        public void CancelParsing()
        {
            if (_delayedParseTimer != null && _delayedParseTimer.Enabled)
                _delayedParseTimer.Stop();
            if (_parserThread.IsBusy)
                _parserThread.CancelAsync();
            _buffer.Changed -= _buffer_Changed;
        }
    }

    /// <summary>
    /// For right now, this class will take the output of the lexer and look for function
    /// definitions. That's all for now.
    /// </summary>
    public class GeneroParser
    {
        private GeneroLexer _lexer;
        private BackgroundWorker _threadRef;
        private GeneroModuleContents _moduleContents;
        private Genero4GL_XMLSettingsLoader _languageSettings;
        private ITextBuffer _currentBuffer;
        private string _primarySibling;
        public string PrimarySibling
        {
            get { return _primarySibling; }
        }

        public GeneroParser(BackgroundWorker threadRef, string primarySibling)
        {
            _lexer = new GeneroLexer();
            _threadRef = threadRef;
            _moduleContents = new GeneroModuleContents();
            _languageSettings = GeneroSingletons.LanguageSettings;
            _primarySibling = primarySibling;
        }

        private Dictionary<string, int> existingGlobalVarsParsed = new Dictionary<string, int>();
        private Dictionary<string, int> existingModuleVarsParsed = new Dictionary<string, int>();
        private Dictionary<string, int> existingFunctionsParsed = new Dictionary<string, int>();

        private void ClearParsedVariables()
        {
            existingFunctionsParsed.Clear();
            existingModuleVarsParsed.Clear();
            existingGlobalVarsParsed.Clear();
            _moduleContents.TempTables.Clear();
            _moduleContents.SqlPrepares.Clear();
            _moduleContents.SqlCursors.Clear();
        }

        public event ModuleContentsUpdatedEventHandler ModuleContentsChanged;

        public void DoWork(object sender, DoWorkEventArgs e)
        {
            ITextBuffer buffer = e.Argument as ITextBuffer;
            _currentBuffer = buffer;
            string currentFile = _currentBuffer.GetFilePath();
            if (!currentFile.EndsWith(".4gl"))
            {
                e.Cancel = true;
                return;
            }
            _lexer.StartLexing(0, buffer.CurrentSnapshot.GetText());
            GeneroToken token = null, prevToken = null;
            _fss = FunctionSearchState.LookingForFunctionStart;
            _gss = GlobalsSearchState.LookingForGlobalsKeyword;
            _vss = VariableSearchState.LookingForDefineKeyword;
            ClearParsedVariables();


            if (_primarySibling == null)
            {
                /********************************************************************************************
                 * This buffer is being opened as the "primary" program file
                 * This means that it will hold the ModuleContents variable, from which all other siblings
                 * will get global variables and public program functions.
                 * We need to get all the sibling filenames and kick off threads to parse them.
                 *********************************************************************************************/

                string moduleFilename = buffer.GetFilePath();
                IEnumerable<string> programFilenames = (VSGeneroPackage.Instance.CurrentProgram4GLFileProvider == null) ?
                                                        EditorExtensions.GetProgramFilenames(moduleFilename) :
                                                        VSGeneroPackage.Instance.CurrentProgram4GLFileProvider.GetProgramFilenames(moduleFilename);
                foreach (var filename in programFilenames)
                {
                    var moduleBuffer = VSGeneroPackage.GetBufferForDocument(filename, false, VSGeneroConstants.ContentType4GL);
                    if (moduleBuffer != null)
                    {
                        GeneroFileParserManager fpm = VSGeneroPackage.Instance.UpdateBufferFileParserManager(moduleBuffer, currentFile);
                        fpm.ParseComplete += GeneroParser_ParseComplete;
                    }
                }
            }
            else
            {
                // TODO: need to find a way to have the globals and program functions be mapped back to the primary sibling's module content.
            }



            token = _lexer.NextToken();

            while (true)
            {
                if (_threadRef != null && _threadRef.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                // if the token is not null, we can look at it
                if (token.TokenType != GeneroTokenType.Unknown)
                {
                    // break out of loop if at end of file
                    if (token.TokenType == GeneroTokenType.Eof)
                        break;

                    if (TryParseGlobalsDefinitions(ref token, ref prevToken))
                        continue;
                    if (TryParseModuleVariableDefinitions(ref token, ref prevToken))
                        continue;
                    // TODO: this is a problem area in terms of CPU usage (~30%)
                    if (TryParseFunction(ref token, ref prevToken))
                        continue;
                }

                AdvanceToken(ref token, ref prevToken);
            }

            // TODO: need to keep track of the variables and functions that already existed in the tables, and
            // remove the ones that weren't parsed
            DiscardUnparsedVariablesAndFunctions(currentFile);
            _moduleContents.ContentFilename = currentFile;
            e.Result = _moduleContents;
        }

        void GeneroParser_ParseComplete(object sender, ParseCompleteEventArgs e)
        {
            if (ModuleContentsChanged != null)
                ModuleContentsChanged(this, new ModuleContentsUpdatedEventArgs(e.FileParserManager.ModuleContents));
        }

        private void DiscardUnparsedVariablesAndFunctions(string filename)
        {
            VariableDefinition remove;
            int temp;

            // Remove globals
            List<string> removeList = new List<string>();
            foreach (var global in _moduleContents.GlobalVariables)
            {
                if (!existingGlobalVarsParsed.TryGetValue(global.Key.ToLower(), out temp))
                    removeList.Add(global.Key.ToLower());
                else
                {
                    if (string.IsNullOrWhiteSpace(global.Value.ContainingFile))
                        global.Value.ContainingFile = filename;
                }
            }
            foreach (var global in removeList)
                _moduleContents.GlobalVariables.TryRemove(global.ToLower(), out remove);

            // Remove module variables
            removeList.Clear();
            foreach (var moduleVar in _moduleContents.ModuleVariables)
            {
                if (!existingModuleVarsParsed.TryGetValue(moduleVar.Key.ToLower(), out temp))
                    removeList.Add(moduleVar.Key.ToLower());
                else
                    moduleVar.Value.ContainingFile = filename;
            }
            foreach (var moduleVar in removeList)
                _moduleContents.ModuleVariables.TryRemove(moduleVar.ToLower(), out remove);

            // Remove functions
            removeList.Clear();
            foreach (var function in _moduleContents.FunctionDefinitions)
            {
                if (!existingFunctionsParsed.TryGetValue(function.Key.ToLower(), out temp))
                    removeList.Add(function.Key.ToLower());
                else
                {
                    function.Value.ContainingFile = filename;
                }
            }
            FunctionDefinition removeFunc;
            foreach (var function in removeList)
                _moduleContents.FunctionDefinitions.TryRemove(function.ToLower(), out removeFunc);

            foreach (var cursorPrep in _moduleContents.SqlPrepares)
                cursorPrep.Value.ContainingFile = filename;

            foreach (var cursor in _moduleContents.SqlCursors)
                cursor.Value.ContainingFile = filename;

            foreach (var tempTab in _moduleContents.TempTables)
                tempTab.Value.ContainingFile = filename;

            // TODO: not sure if we need to clear out the constants and types like we're doing above on variables...we'll see
            foreach (var constDef in _moduleContents.GlobalConstants)
                constDef.Value.ContainingFile = filename;

            foreach (var constDef in _moduleContents.ModuleConstants)
                constDef.Value.ContainingFile = filename;

            foreach (var typeDef in _moduleContents.GlobalTypes)
                typeDef.Value.ContainingFile = filename;

            foreach (var typeDef in _moduleContents.ModuleTypes)
                typeDef.Value.ContainingFile = filename;
        }

        #region Variables Parsing

        #region Global Variables
        enum GlobalsSearchState
        {
            LookingForGlobalsKeyword,
            LookingForEndKeyword
        }

        private GlobalsSearchState _gss;

        private bool TryParseGlobalsDefinitions(ref GeneroToken token, ref GeneroToken prevToken)
        {
            bool ret = false;
            // don't want to do this within the scope of a function
            if (_fss == FunctionSearchState.LookingForFunctionStart &&
                _gss == GlobalsSearchState.LookingForGlobalsKeyword)
            {
                if (token.LowercaseText == "globals" && prevToken.LowercaseText != "end")
                {
                    // we want to be in globals mode
                    _gss = GlobalsSearchState.LookingForEndKeyword;
                    AdvanceToken(ref token, ref prevToken);

                    // check to see if a globals file has been specified
                    if (token.TokenType == GeneroTokenType.String)
                    {
                        // TODO: do we not want globals from the global file showing up if they haven't been specified with an "globals" keyword?
                    }
                    else
                    {
                        // now use the general variable definition consumer
                        ret = TryParseVarConstTypeDefinitions(ref token, ref prevToken, ref _vss, _moduleContents.GlobalVariables, _moduleContents.GlobalTypes, _moduleContents.GlobalConstants, ref _currentVariableDef,
                                                            _variableBuffer, existingGlobalVarsParsed, new[] { "end", "define", "type", "constant" });
                        // TODO: need to consume "end" and "global"
                    }

                    // back out of globals mode
                    _gss = GlobalsSearchState.LookingForGlobalsKeyword;
                }
            }

            return ret;
        }

        #endregion Global Variables

        #region Module Variables

        private bool TryParseModuleVariableDefinitions(ref GeneroToken token, ref GeneroToken prevToken)
        {
            bool ret = false;
            // we only want to look for module variables outside the scope of functions
            if (_fss == FunctionSearchState.LookingForFunctionStart &&
                _vss == VariableSearchState.LookingForDefineKeyword)
            {
                // now use the general variable definition consumer
                ret = TryParseVarConstTypeDefinitions(ref token, ref prevToken, ref _vss, _moduleContents.ModuleVariables, _moduleContents.ModuleTypes, _moduleContents.ModuleConstants, ref _currentVariableDef,
                                                  _variableBuffer, existingModuleVarsParsed, new[] { "main", "function", "define", "type", "constant" });
            }
            return ret;
        }

        #endregion Module Variables

        //#region Function Variables

        ////private bool TryParseFunctionVariableDefinitions(ref GeneroToken token, ref GeneroToken prevToken)
        ////{
        ////    bool ret = false;
        ////    // we only want to look for variables inside the scope of functions
        ////    if (_fss == FunctionSearchState.LookingForFunctionEnd &&
        ////        _vss == VariableSearchState.LookingForDefineKeyword)
        ////    {
        ////        Dictionary<string, int> temp = new Dictionary<string, int>();
        ////        // now use the general variable definition consumer
        ////        ret = TryParseVariableDefinitions(ref token, ref prevToken, ref _vss, _currentFunctionDef.Variables, ref _currentVariableDef, _variableBuffer, temp, new[] { "end", "define" });
        ////    }
        ////    return ret;
        ////}

        //#endregion Function Variables

        #region General Variable Consumption

        enum VariableSearchState
        {
            LookingForDefineKeyword,
            LookingForVariableName,
            LookingForTypeName,
            LookingForConstName,
            LookingForVariableType,
            LookingForSubTypeType,
            LookingForConstValue
        }

        private VariableSearchState _vss;
        private VariableDefinition _currentVariableDef;
        private List<VariableDefinition> _variableBuffer = new List<VariableDefinition>();  // This is used since more than one variable can be defined under a type

        #region Statement Verification

        internal GeneroParser() : this(null, null)
        {
        }

        internal bool IsIncompleteVarConstTypeDefinitionStatement(string statement)
        {
            _lexer.StartLexing(0, statement);
            GeneroToken token = null, prevToken = null;
            AdvanceToken(ref token, ref prevToken);
            _vss = VariableSearchState.LookingForDefineKeyword;
            bool isComplete = TryParseVarConstTypeDefinitions(ref token, ref prevToken, ref _vss, _moduleContents.GlobalVariables, _moduleContents.GlobalTypes, _moduleContents.GlobalConstants, ref _currentVariableDef,
                                                            _variableBuffer, existingGlobalVarsParsed, new[] { "function", "end", "define", "type", "constant" });
            return !isComplete;
        }

        #endregion

        // TODO: need to have this function server two purposes.
        // 1) Its original purpose, which is to collect var, type, and const defines
        // 2) Its new additional purpose, which is to determine if a define statement is incomplete
        private bool TryParseVarConstTypeDefinitions(ref GeneroToken token,
                                                 ref GeneroToken prevToken,
                                                 ref VariableSearchState searchState,
                                                 ConcurrentDictionary<string, VariableDefinition> scopeVariables,
                                                 ConcurrentDictionary<string, TypeDefinition> scopeTypes,
                                                 ConcurrentDictionary<string, ConstantDefinition> scopeConstants,
                                                 ref VariableDefinition currentVariableDef,
                                                 List<VariableDefinition> variableBuffer,
                                                 Dictionary<string, int> variablesCollected,
                                                 string[] advanceBlacklist = null)
        {
            ConstantDefinition currentConstantDef = null;
            TypeDefinition currentTypeDef = null;

            bool valid = false;
            bool end = false;
            while (!end)
            {
                switch (searchState)
                {
                    case VariableSearchState.LookingForDefineKeyword:
                        {
                            // if we hit the define keyword, that's the start of one or more variable definitions
                            if (token.LowercaseText == "define")
                            {
                                ResetCurrentVariable(ref currentVariableDef);
                                searchState = VariableSearchState.LookingForVariableName;

                                // go to the next token
                                AdvanceToken(ref token, ref prevToken);
                            }
                            else if (token.LowercaseText == "constant")
                            {
                                searchState = VariableSearchState.LookingForConstName;
                                AdvanceToken(ref token, ref prevToken);
                            }
                            else if (token.LowercaseText == "type")
                            {
                                searchState = VariableSearchState.LookingForTypeName;
                                AdvanceToken(ref token, ref prevToken);
                            }
                            else
                            {
                                // we were unable to parse a variable definition, so exit the while loop
                                end = true;
                            }
                        }
                        break;
                    case VariableSearchState.LookingForVariableName:
                        {
                            // if the next token is an identifier or keyword, we can get a name
                            if (token.TokenType == GeneroTokenType.Identifier || token.TokenType == GeneroTokenType.Keyword)
                            {
                                currentVariableDef.Name = token.TokenText;
                                currentVariableDef.Position = token.StartPosition;
                                currentVariableDef.LineNumber = token.LineNumber;
                                currentVariableDef.ColumnNumber = token.ColumnNumber;
                                searchState = VariableSearchState.LookingForVariableType;

                                // go to the next token
                                AdvanceToken(ref token, ref prevToken);
                            }
                            else
                            {
                                // give up. If we don't have a variable name, there's not much we can do
                                ResetCurrentVariableAndBuffer(ref currentVariableDef, variableBuffer);
                                searchState = VariableSearchState.LookingForDefineKeyword;
                            }
                        }
                        break;
                    case VariableSearchState.LookingForTypeName:
                        {
                            // if the next token is an identifier or keyword, we can get a name
                            if (token.TokenType == GeneroTokenType.Identifier || token.TokenType == GeneroTokenType.Keyword)
                            {
                                currentTypeDef = new TypeDefinition
                                {
                                    Name = token.TokenText,
                                    Position = token.StartPosition,
                                    LineNumber = token.LineNumber,
                                    ColumnNumber = token.ColumnNumber,
                                };
                                searchState = VariableSearchState.LookingForSubTypeType;

                                // go to the next token
                                AdvanceToken(ref token, ref prevToken);
                            }
                            else
                            {
                                // give up. If we don't have a variable name, there's not much we can do
                                currentTypeDef = null;
                                searchState = VariableSearchState.LookingForDefineKeyword;
                            }
                        }
                        break;
                    case VariableSearchState.LookingForConstName:
                        {
                            if (token.TokenType == GeneroTokenType.Identifier || token.TokenType == GeneroTokenType.Keyword)
                            {
                                currentConstantDef = new ConstantDefinition
                                {
                                    Name = token.TokenText,
                                    Position = token.StartPosition,
                                    LineNumber = token.LineNumber,
                                    ColumnNumber = token.ColumnNumber
                                };
                                searchState = VariableSearchState.LookingForConstValue;
                                // go to the next token
                                AdvanceToken(ref token, ref prevToken);
                            }
                            else
                            {
                                currentConstantDef = null;
                                searchState = VariableSearchState.LookingForDefineKeyword;
                            }
                        }
                        break;
                    case VariableSearchState.LookingForVariableType:
                        {
                            if (token.LowercaseText == ",")
                            {
                                // we need to go back to LookingForVariableName
                                variableBuffer.Add(currentVariableDef);
                                ResetCurrentVariable(ref currentVariableDef);
                                searchState = VariableSearchState.LookingForVariableName;

                                // go to the next token
                                AdvanceToken(ref token, ref prevToken);
                            }
                            else
                            {
                                valid = GetVariableType(ref token,
                                                   ref prevToken,
                                                   ref searchState,
                                                   ref currentVariableDef,
                                                   ref variableBuffer,
                                                   scopeVariables, variablesCollected, advanceBlacklist);
                            }

                        }
                        break;
                    case VariableSearchState.LookingForSubTypeType:
                        {
                            VariableSearchState typeVss = VariableSearchState.LookingForVariableType;
                            VariableDefinition dummyVarDef = new VariableDefinition();
                            dummyVarDef.CloneContents(currentTypeDef);
                            List<VariableDefinition> dummyVarList = new List<VariableDefinition>();
                            ConcurrentDictionary<string, VariableDefinition> dummyScopeVars = new ConcurrentDictionary<string, VariableDefinition>();
                            Dictionary<string, int> dummyVarsCollected = new Dictionary<string, int>();
                            if (GetVariableType(ref token,
                                                   ref prevToken,
                                                   ref typeVss,
                                                   ref dummyVarDef,
                                                   ref dummyVarList,
                                                   dummyScopeVars, dummyVarsCollected, advanceBlacklist))
                            {
                                foreach(var scopeVar in dummyScopeVars)     // there should only be one
                                    currentTypeDef.CloneContents(scopeVar.Value);
                                if (typeVss == VariableSearchState.LookingForVariableName)
                                    searchState = VariableSearchState.LookingForTypeName;
                                else
                                    searchState = VariableSearchState.LookingForDefineKeyword;
                                scopeTypes.AddOrUpdate(currentTypeDef.Name.ToLower(), currentTypeDef, (x, y) => currentTypeDef);
                            }
                            else
                            {
                                currentTypeDef = null;
                                searchState = VariableSearchState.LookingForDefineKeyword;
                            }
                        }
                        break;
                    case VariableSearchState.LookingForConstValue:
                        {
                            if (token.TokenType == GeneroTokenType.Keyword)
                            {
                                VariableSearchState tempVss = VariableSearchState.LookingForConstValue;
                                List<VariableDefinition> dummyList = new List<VariableDefinition>();
                                VariableDefinition dummyVar = currentConstantDef;
                                // a type was specified for the constant
                                if (!TryGetActualVariableType(ref token, ref prevToken, ref tempVss, ref dummyVar, dummyList))
                                {
                                    currentConstantDef = null;
                                    searchState = VariableSearchState.LookingForDefineKeyword;
                                }
                                else
                                {
                                    currentConstantDef.Type = dummyVar.Type;
                                }
                            }

                            if (token.LowercaseText != "=")
                            {
                                currentConstantDef = null;
                                searchState = VariableSearchState.LookingForDefineKeyword;
                            }
                            else
                            {
                                AdvanceToken(ref token, ref prevToken);
                            }

                            if (!GetConstantValue(ref token, ref prevToken, ref currentConstantDef))
                            {
                                currentConstantDef = null;
                                searchState = VariableSearchState.LookingForDefineKeyword;
                            }
                            else
                            {
                                // add the current constant def to the scope given.
                                scopeConstants.AddOrUpdate(currentConstantDef.Name.ToLower(), currentConstantDef, (x, y) => currentConstantDef);
                                AdvanceToken(ref token, ref prevToken);
                                if (token.LowercaseText == ",")
                                {
                                    searchState = VariableSearchState.LookingForConstName;
                                    AdvanceToken(ref token, ref prevToken);
                                }
                                else
                                {
                                    searchState = VariableSearchState.LookingForDefineKeyword;
                                }
                            }
                        }
                        break;
                }
            }
            return valid;
        }

        private bool GetConstantValue(ref GeneroToken token, ref GeneroToken prevToken, ref ConstantDefinition constantDef)
        {
            if (token.TokenType == GeneroTokenType.String || token.TokenType == GeneroTokenType.Number)
            {
                constantDef.Value = token.TokenText;
                return true;
            }
            else
            {
                constantDef.Value = "";
                // there are some constant values that can span multiple tokens
                if (token.LowercaseText == "mdy" &&
                    GetConstantMDYValue(ref token, ref prevToken, ref constantDef))
                {
                    return true;
                }
                else if ((token.LowercaseText == "datetime" || token.LowercaseText == "interval") &&
                    GetConstantDatetimeIntervalValue(ref token, ref prevToken, ref constantDef))
                {
                    return true;
                }
            }
            return false;
        }

        private bool GetConstantMDYValue(ref GeneroToken token, ref GeneroToken prevToken, ref ConstantDefinition constantDef)
        {
            string value = token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            if (token.LowercaseText != "(")
                return false;
            value += token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            if (token.TokenType != GeneroTokenType.Number)
                return false;
            value += token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            if (token.LowercaseText != ",")
                return false;
            value += token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            if (token.TokenType != GeneroTokenType.Number)
                return false;
            value += token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            if (token.LowercaseText != ",")
                return false;
            value += token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            if (token.TokenType != GeneroTokenType.Number)
                return false;
            value += token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            if (token.LowercaseText != ")")
                return false;
            value += token.TokenText;
            constantDef.Value = value;
            return true;
        }

        private bool GetConstantDatetimeIntervalValue(ref GeneroToken token, ref GeneroToken prevToken, ref ConstantDefinition constantDef)
        {
            string value = token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            if (token.LowercaseText != "(")
                return false;
            value += token.TokenText;
            bool rightParenFound = false;
            while (AdvanceToken(ref token, ref prevToken) &&
                  (token.TokenType == GeneroTokenType.Number || token.TokenType == GeneroTokenType.Symbol))
            {
                value += token.TokenText;
                if (token.LowercaseText == ")")
                {
                    rightParenFound = true;
                    break;
                }
            }
            if (!rightParenFound)
                return false;

            AdvanceToken(ref token, ref prevToken);
            if (token.TokenType == GeneroTokenType.Keyword)
            {
                value += token.TokenText;
                AdvanceToken(ref token, ref prevToken);

                if (token.TokenText == "(")
                {
                    // handle FRACTION(1) format
                    value += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                    if (token.TokenType != GeneroTokenType.Number)
                        return false;
                    value += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                    if (token.TokenText != ")")
                        return false;
                    value += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                }

                if (token.LowercaseText == "to")
                {
                    value += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                    if (token.TokenType == GeneroTokenType.Keyword)
                    {
                        value += token.TokenText;
                        AdvanceToken(ref token, ref prevToken);
                        if (token.TokenText == "(")
                        {
                            value += token.TokenText;
                            // handle FRACTION(1) format
                            AdvanceToken(ref token, ref prevToken);
                            if (token.TokenType != GeneroTokenType.Number)
                                return false;
                            value += token.TokenText;
                            AdvanceToken(ref token, ref prevToken);
                            if (token.TokenText != ")")
                                return false;
                            value += token.TokenText;
                            AdvanceToken(ref token, ref prevToken);
                        }
                        constantDef.Value = value;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool GetVariableType(ref GeneroToken token,
                                                    ref GeneroToken prevToken,
                                                    ref VariableSearchState searchState,
                                                    ref VariableDefinition currentVariableDef,
                                                    ref List<VariableDefinition> variableBuffer,
                                                    ConcurrentDictionary<string, VariableDefinition> scope,
                                                    Dictionary<string, int> variablesCollected,
                                                    string[] advanceBlacklist = null)
        {
            VariableSearchState returnState = VariableSearchState.LookingForVariableName;

            // Note: all the stuff we do in the while loop will be done to any other variables in the variable buffer
            // try to determine the type. If it can't be done, we'll reject the variable(s) for which we were trying to find the type
            bool valid = true;
            while (true)
            {
                if (token.LowercaseText == "like")
                {
                    currentVariableDef.IsMimicType = true;
                    AdvanceToken(ref token, ref prevToken);

                    // need to get the type we are mimicking
                    valid = GetVariablesMimickingType(ref token, ref prevToken, ref searchState, ref currentVariableDef, variableBuffer);
                    // All done
                    break;
                }
                else if (token.LowercaseText == "record")
                {
                    currentVariableDef.IsRecordType = true;
                    AdvanceToken(ref token, ref prevToken);

                    // if the current token is like, let the "like" block handle the rest
                    if (token.LowercaseText == "like")
                        continue;

                    if (token.LowercaseText == "attribute")
                    {
                        AdvanceToken(ref token, ref prevToken);
                        if (token.LowercaseText == "(")
                        {
                            // TODO: get the serialization attributes
                            valid = GetSerializationAttributes(ref token, ref prevToken, ref currentVariableDef);
                        }
                        else
                            valid = false;
                    }

                    valid = GetVariablesRecordType(ref token, ref prevToken, ref searchState, ref currentVariableDef, variableBuffer);
                    break;
                }
                else if (token.LowercaseText == "dynamic")
                {
                    // advance the token to make sure we're looking at an array
                    if (AdvanceToken(ref token, ref prevToken))
                    {
                        if (token.LowercaseText == "array")
                        {
                            currentVariableDef.ArrayType = ArrayType.Dynamic;
                            if (!AdvanceToken(ref token, ref prevToken))
                            {
                                // TODO: error, break out of while loop
                                valid = false;
                                searchState = VariableSearchState.LookingForDefineKeyword;
                                break;
                            }
                            else if (token.LowercaseText == "attribute")
                            {
                                AdvanceToken(ref token, ref prevToken);
                                if (token.LowercaseText == "(")
                                {
                                    // TODO: get the serialization attributes
                                    valid = GetSerializationAttributes(ref token, ref prevToken, ref currentVariableDef);
                                }
                                else
                                    valid = false;
                            }
                        }
                        else
                        {
                            valid = false;
                            searchState = VariableSearchState.LookingForDefineKeyword;
                            break;
                        }
                    }
                    else
                    {
                        // error, break out of while loop
                        valid = false;
                        searchState = VariableSearchState.LookingForDefineKeyword;
                        break;
                    }
                }
                else if (token.LowercaseText == "array")
                {
                    bool error = false;
                    // this can only be static, so we also need to look for the dimension
                    currentVariableDef.ArrayType = ArrayType.Static;
                    if (!AdvanceToken(ref token, ref prevToken) && token.TokenText != "[")
                    {
                        // error, break out of while loop
                        error = true;
                    }
                    if (error || (!AdvanceToken(ref token, ref prevToken) && token.TokenType != GeneroTokenType.Number))
                    {
                        error = true;
                    }
                    if (!error)
                    {
                        int temp = 0;
                        if (!int.TryParse(token.TokenText, out temp))
                            error = true;
                        else
                            currentVariableDef.StaticArraySize = temp;
                    }
                    if (error || (!AdvanceToken(ref token, ref prevToken) && token.TokenText != "]"))
                    {
                        error = true;
                    }
                    if (!AdvanceToken(ref token, ref prevToken))
                        error = true;
                    if (error)
                    {
                        valid = false;
                        searchState = VariableSearchState.LookingForDefineKeyword;
                        break;
                    }
                    if (token.LowercaseText == "attribute")
                    {
                        AdvanceToken(ref token, ref prevToken);
                        if (token.LowercaseText == "(")
                        {
                            // TODO: get the serialization attributes
                            valid = GetSerializationAttributes(ref token, ref prevToken, ref currentVariableDef);
                            if (!valid)
                            {
                                searchState = VariableSearchState.LookingForDefineKeyword;
                                break;
                            }
                        }
                        else
                        {
                            valid = false;
                            searchState = VariableSearchState.LookingForDefineKeyword;
                            break;
                        }
                    }
                }
                else if (token.LowercaseText == "of")
                {
                    // skip of and go to the next token
                    if (!AdvanceToken(ref token, ref prevToken))
                    {
                        // error, break out of while loop
                        valid = false;
                        searchState = VariableSearchState.LookingForDefineKeyword;
                        break;
                    }
                }
                else if (token.LowercaseText == "with")
                {
                    // this is a multi-dimensionsal dynamic array
                    if (!AdvanceToken(ref token, ref prevToken) || token.LowercaseText != "dimension")
                    {
                        valid = false;
                        searchState = VariableSearchState.LookingForDefineKeyword;
                        break;
                    }
                    if (!AdvanceToken(ref token, ref prevToken) || token.TokenType != GeneroTokenType.Number)
                    {
                        valid = false;
                        searchState = VariableSearchState.LookingForDefineKeyword;
                        break;
                    }
                    AdvanceToken(ref token, ref prevToken); // allow the "of" case handle the rest
                }
                else
                {
                    valid = TryGetActualVariableType(ref token, ref prevToken, ref searchState, ref currentVariableDef, variableBuffer);
                    break;
                }
            }


            if (valid)
            {
                // TODO: we need some criteria for not advancing. For instance, the current token is on a "blacklist" of tokens that can't be continued past...
                if (advanceBlacklist == null ||
                   !advanceBlacklist.Contains(token.LowercaseText))
                {
                    AdvanceToken(ref token, ref prevToken);
                }

                // add the found variables to the specified scope
                AddVariablesAndReset(scope, ref currentVariableDef, variableBuffer, variablesCollected);
            }
            return valid;
        }

        private bool GetVariablesMimickingType(ref GeneroToken token,
                                               ref GeneroToken prevToken,
                                               ref VariableSearchState searchState,
                                               ref VariableDefinition currentVariableDef,
                                               List<VariableDefinition> variableBuffer)
        {
            /*
             * the mimicking format is:
             * like [database:]table.field
             */

            int i = 0;
            bool valid = true;
            while (i < 5)
            {
                // the database/table/field names occur on even numbers
                if (i == 0 || i % 2 == 0)
                {
                    if (token.TokenType == GeneroTokenType.Identifier ||
                       token.TokenType == GeneroTokenType.Keyword ||
                        token.LowercaseText == "*")
                    {
                        currentVariableDef.Type += token.TokenText;
                        i++;
                        AdvanceToken(ref token, ref prevToken);
                    }
                    else
                    {
                        valid = false;
                        break;
                    }
                }
                else
                {
                    if (token.LowercaseText == "," ||
                        token.TokenType != GeneroTokenType.Symbol)
                    {
                        valid = false;
                        break;
                    }
                    else
                    {
                        if (token.LowercaseText == ".")
                            i = 4;
                        currentVariableDef.Type += token.TokenText;
                    }
                    AdvanceToken(ref token, ref prevToken);
                }
            }

            if (!valid)
            {
                // give up on this block of variables
                searchState = VariableSearchState.LookingForDefineKeyword;
                ResetCurrentVariableAndBuffer(ref currentVariableDef, variableBuffer);
            }
            else
            {
                // determine what our search state is now...if all is good, the token has already been advanced
                searchState = token.LowercaseText == "," ? VariableSearchState.LookingForVariableName : VariableSearchState.LookingForDefineKeyword;

                // and either way, fill in the buffer's contents
                PopulateBufferedVariables(variableBuffer, currentVariableDef);
            }
            return valid;
        }

        private bool GetVariablesRecordType(ref GeneroToken token,
                                               ref GeneroToken prevToken,
                                               ref VariableSearchState searchState,
                                               ref VariableDefinition currentVariableDef,
                                               List<VariableDefinition> variableBuffer)
        {
            // We need to basically do a recursive call to TryParseVariableDefinitions
            // starting with searchState of LookingForVariableName. We do this until we hit end record
            VariableSearchState recordVss = VariableSearchState.LookingForVariableName;
            VariableDefinition recordCurrentVariableDef = new VariableDefinition
            {
                ArrayType = ArrayType.None,
                IsMimicType = false,
                IsRecordType = false,
                Type = ""
            };
            List<VariableDefinition> recordVariableBuffer = new List<VariableDefinition>();
            VariableDefinition backup = currentVariableDef.Clone();
            ConcurrentDictionary<string, VariableDefinition> elementList = new ConcurrentDictionary<string, VariableDefinition>();
            ConcurrentDictionary<string, ConstantDefinition> dummyConstList = new ConcurrentDictionary<string, ConstantDefinition>();
            ConcurrentDictionary<string, TypeDefinition> dummyTypeList = new ConcurrentDictionary<string, TypeDefinition>();

            Dictionary<string, int> temp = new Dictionary<string, int>();
            bool valid = TryParseVarConstTypeDefinitions(ref token, ref prevToken, ref recordVss, elementList, dummyTypeList, dummyConstList, ref recordCurrentVariableDef, recordVariableBuffer, temp);

            if (!valid)
            {
                // give up on this block of variables
                searchState = VariableSearchState.LookingForDefineKeyword;
                ResetCurrentVariableAndBuffer(ref currentVariableDef, variableBuffer);
            }
            else
            {
                // restore the current variable def
                currentVariableDef = backup.Clone();
                foreach (var element in elementList)
                    currentVariableDef.RecordElements.AddOrUpdate(element.Key.ToLower(), element.Value, (x, y) => element.Value);

                // the current token should be "record", so Advance past that
                AdvanceToken(ref token, ref prevToken);

                // determine what our search state is now...if all is good, the token has already been advanced
                searchState = token.LowercaseText == "," ? VariableSearchState.LookingForVariableName : VariableSearchState.LookingForDefineKeyword;

                // and either way, fill in the buffer's contents
                PopulateBufferedVariables(variableBuffer, currentVariableDef);
            }
            return valid;
        }

        private bool TryGetActualVariableType(ref GeneroToken token,
                                               ref GeneroToken prevToken,
                                               ref VariableSearchState searchState,
                                               ref VariableDefinition currentVariableDef,
                                               List<VariableDefinition> variableBuffer)
        {
            // there must be a type specifier here...if not, error
            if (token.TokenType != GeneroTokenType.Identifier && token.TokenType != GeneroTokenType.Keyword)
            {
                searchState = VariableSearchState.LookingForDefineKeyword;
                return false;
            }

            // we have a data type
            // but we have a few cases that need to be parsed out, not just using the comma (char, decimal, money, etc.)
            DataType nativeDataType = null;
            bool dimReq = false, rangeReq = false;
            if (_languageSettings.DataTypeMap.TryGetValue(token.LowercaseText, out nativeDataType))
            {
                dimReq = nativeDataType.DimensionRequired;
                rangeReq = nativeDataType.RangeRequired;
            }
            currentVariableDef.Type += token.TokenText;
            AdvanceToken(ref token, ref prevToken);
            while (token.TokenText == ".")
            {
                currentVariableDef.Type += token.TokenText;
                AdvanceToken(ref token, ref prevToken);
                if (token.TokenType == GeneroTokenType.Eof ||
                   (token.TokenType != GeneroTokenType.Identifier && token.TokenType != GeneroTokenType.Keyword))
                {
                    return false;
                }
                else
                {
                    currentVariableDef.Type += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                }
            }

            // look for a comma ahead, in case the dimension or range was skipped
            var isComma = token.TokenText == ",";

            if (isComma &&
               (dimReq || rangeReq))
            {
                searchState = VariableSearchState.LookingForDefineKeyword;
                return false;
            }

            bool valid = true;
            if (dimReq)
            {
                // parse out the dimension
                valid = GetVariableTypeDimension(ref token, ref prevToken, ref currentVariableDef);
            }
            else if (rangeReq)
            {
                // parse out the range
                valid = GetVariableTypeRange(ref token, ref prevToken, ref currentVariableDef);
            }

            if (!valid)
            {
                // give up on this block of variables
                searchState = VariableSearchState.LookingForDefineKeyword;
                ResetCurrentVariableAndBuffer(ref currentVariableDef, variableBuffer);
            }
            else
            {
                // check for the "attribute" keyword.
                if (token.LowercaseText == "attribute")
                {
                    AdvanceToken(ref token, ref prevToken);
                    if (token.LowercaseText == "(")
                    {
                        // TODO: get the serialization attributes
                        valid = GetSerializationAttributes(ref token, ref prevToken, ref currentVariableDef);
                    }
                    else
                        valid = false;
                }

                // determine what our search state is now...if all is good, the token has already been advanced
                searchState = token.LowercaseText == "," ? VariableSearchState.LookingForVariableName : VariableSearchState.LookingForDefineKeyword;

                // and either way, fill in the buffer's contents
                PopulateBufferedVariables(variableBuffer, currentVariableDef);
            }
            return valid;
        }

        private bool GetSerializationAttributes(ref GeneroToken token, ref GeneroToken prevToken, ref VariableDefinition currentVariableDefinition)
        {
            // For right now, don't do anything, just go through the attributes until the close paren
            while (AdvanceToken(ref token, ref prevToken))
            {
                if (token.LowercaseText == ")")
                {
                    AdvanceToken(ref token, ref prevToken);
                    return true;
                }
                else
                {
                    if (token.TokenType != GeneroTokenType.Identifier &&
                        token.LowercaseText != "=" &&
                        token.TokenType != GeneroTokenType.String &&
                        token.LowercaseText != ",")
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        private bool GetVariableTypeDimension(ref GeneroToken token,
                                              ref GeneroToken prevToken,
                                              ref VariableDefinition currentVariableDefinition)
        {
            if (token.LowercaseText != "(")
            {
                return false;
            }
            else
            {
                currentVariableDefinition.Type += token.TokenText;
                // can either have one number or two numbers, seperated by a comma
                AdvanceToken(ref token, ref prevToken);
                if (token.TokenType != GeneroTokenType.Number)
                {
                    return false;
                }
                currentVariableDefinition.Type += token.TokenText;
                AdvanceToken(ref token, ref prevToken);
                if (token.TokenText == ",")
                {
                    // check for a second dimension
                    var nextToken = _lexer.Lookahead(1);
                    if (nextToken.TokenType == GeneroTokenType.Number)
                    {
                        currentVariableDefinition.Type += token.TokenText;
                        AdvanceToken(ref token, ref prevToken);
                        currentVariableDefinition.Type += token.TokenText;
                        AdvanceToken(ref token, ref prevToken);
                    }
                    else
                    {
                        return false;
                    }
                }

                if (token.TokenText != ")")
                    return false;

                currentVariableDefinition.Type += token.TokenText;
                AdvanceToken(ref token, ref prevToken);
            }
            return true;
        }

        private bool GetVariableTypeRange(ref GeneroToken token,
                                          ref GeneroToken prevToken,
                                          ref VariableDefinition currentVariableDefinition)
        {
            bool valid = false;

            // For right now, let's just assume that the keywords used for start and end specifiers
            // are correct keywords.
            // TODO: at some point, I'd like to check for correct keywords in use.
            if (token.TokenType == GeneroTokenType.Keyword)
            {
                currentVariableDefinition.Type += token.TokenText;
                AdvanceToken(ref token, ref prevToken);

                if (token.TokenText == "(")
                {
                    // handle FRACTION(1) format
                    currentVariableDefinition.Type += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                    if (token.TokenType != GeneroTokenType.Number)
                        return false;
                    currentVariableDefinition.Type += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                    if (token.TokenText != ")")
                        return false;
                    currentVariableDefinition.Type += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                }

                if (token.LowercaseText == "to")
                {
                    currentVariableDefinition.Type += token.TokenText;
                    AdvanceToken(ref token, ref prevToken);
                    if (token.TokenType == GeneroTokenType.Keyword)
                    {
                        currentVariableDefinition.Type += token.TokenText;
                        AdvanceToken(ref token, ref prevToken);
                        if (token.TokenText == "(")
                        {
                            currentVariableDefinition.Type += token.TokenText;
                            // handle FRACTION(1) format
                            AdvanceToken(ref token, ref prevToken);
                            if (token.TokenType != GeneroTokenType.Number)
                                return false;
                            currentVariableDefinition.Type += token.TokenText;
                            AdvanceToken(ref token, ref prevToken);
                            if (token.TokenText != ")")
                                return false;
                            currentVariableDefinition.Type += token.TokenText;
                            AdvanceToken(ref token, ref prevToken);
                        }
                        valid = true;
                    }
                }
            }

            return valid;
        }

        private void PopulateBufferedVariables(List<VariableDefinition> buffer, VariableDefinition master)
        {
            foreach (var variable in buffer)
            {
                variable.ArrayType = master.ArrayType;
                variable.IsMimicType = master.IsMimicType;
                variable.IsRecordType = master.IsRecordType;
                foreach (var rec in master.RecordElements)
                    variable.RecordElements.AddOrUpdate(rec.Key, rec.Value, (x, y) => rec.Value);
                variable.Type = master.Type;
            }
        }

        private void AddVariablesAndReset(ConcurrentDictionary<string, VariableDefinition> scope, ref VariableDefinition currentDef, List<VariableDefinition> buffer, Dictionary<string, int> parsed)
        {
            VariableDefinition tempDef = currentDef;    // Needed so we can do update in concurrent update lambda
            scope.AddOrUpdate(currentDef.Name.ToLower(), currentDef, (x, y) => tempDef);
            if (!parsed.ContainsKey(currentDef.Name.ToLower()))
                parsed.Add(currentDef.Name.ToLower(), 1);
            foreach (var vardef in buffer)
            {
                scope.AddOrUpdate(vardef.Name.ToLower(), vardef, (x, y) => vardef);
                if (!parsed.ContainsKey(vardef.Name.ToLower()))
                    parsed.Add(vardef.Name.ToLower(), 1);
            }
            ResetCurrentVariableAndBuffer(ref currentDef, buffer);
        }

        private void ResetCurrentVariableAndBuffer(ref VariableDefinition currentDef, List<VariableDefinition> buffer)
        {
            ResetCurrentVariable(ref currentDef);
            buffer.Clear();
        }

        private void ResetCurrentVariable(ref VariableDefinition currentDef)
        {
            currentDef = null;
            currentDef = new VariableDefinition
            {
                ArrayType = ArrayType.None,
                IsMimicType = false,
                IsRecordType = false,
                Type = ""
            };
        }

        private bool AdvanceToken(ref GeneroToken token, ref GeneroToken prevToken)
        {
            prevToken = token;
            token = _lexer.NextToken();
            while (token.TokenType != GeneroTokenType.Unknown &&
                   token.TokenType == GeneroTokenType.Comment)
                token = _lexer.NextToken();
            return token.TokenType != GeneroTokenType.Unknown;
        }

        #endregion General Variable Consumption

        #endregion

        #region Function Parsing

        enum FunctionSearchState
        {
            LookingForFunctionStart,
            LookingForFunctionName,
            LookingForFunctionSignature,
            LookingForFunctionEnd
        }

        enum FunctionSignatureSearchState
        {
            LookingForOpenParen,
            LookingForParam,
            LookingForClosedParen
        }

        private FunctionSearchState _fss;
        private FunctionDefinition _currentFunctionDef = null;
        private List<FunctionDefinition> _functionList = new List<FunctionDefinition>();

        private bool TryParseFunction(ref GeneroToken token, ref GeneroToken prevToken)
        {
            FunctionSignatureSearchState fsss = FunctionSignatureSearchState.LookingForOpenParen;
            bool ret = false;
            bool end = false;
            while (!end)
            {
                switch (_fss)
                {
                    case FunctionSearchState.LookingForFunctionStart:
                        {
                            // At this point we only want to look for "private", "main", and "function" keywords
                            if (token.TokenType == GeneroTokenType.Keyword)
                            {
                                if (token.LowercaseText == "main")
                                {
                                    _currentFunctionDef = new FunctionDefinition { Main = true, Name = "main", Position = token.StartPosition, LineNumber = token.LineNumber, ColumnNumber = token.ColumnNumber };
                                    _fss = FunctionSearchState.LookingForFunctionEnd;
                                    AdvanceToken(ref token, ref prevToken);
                                }
                                else if (token.LowercaseText == "function")
                                {
                                    _currentFunctionDef = new FunctionDefinition { Main = false, Position = token.StartPosition, LineNumber = token.LineNumber, ColumnNumber = token.ColumnNumber, Report = false };
                                    // check to see if the previous token is "private"
                                    if (prevToken.LowercaseText == "private")
                                        _currentFunctionDef.Private = true;
                                    _fss = FunctionSearchState.LookingForFunctionName;
                                    AdvanceToken(ref token, ref prevToken);
                                }
                                else if (token.LowercaseText == "report" && prevToken.LowercaseText != "to")
                                {
                                    _currentFunctionDef = new FunctionDefinition { Main = false, Position = token.StartPosition, LineNumber = token.LineNumber, ColumnNumber = token.ColumnNumber, Report = true };
                                    // check to see if the previous token is "private"
                                    if (prevToken.LowercaseText == "private")
                                        _currentFunctionDef.Private = true;
                                    _fss = FunctionSearchState.LookingForFunctionName;
                                    AdvanceToken(ref token, ref prevToken);
                                }
                                else
                                {
                                    // exit this function
                                    end = true;
                                }
                            }
                            else
                            {
                                // exit this function
                                end = true;
                            }
                            break;
                        }
                    case FunctionSearchState.LookingForFunctionName:
                        {
                            // only want function name, so if the new token is not an identifier or keyword (not "end","function"), 
                            // we want to set the function def to null and move on
                            if (token.TokenType == GeneroTokenType.Identifier ||
                                token.TokenType == GeneroTokenType.Keyword)
                            {
                                if (token.LowercaseText == "end" || token.LowercaseText == "function" || token.LowercaseText == "report")
                                {
                                    _currentFunctionDef = null;
                                    _fss = FunctionSearchState.LookingForFunctionStart;
                                    end = false;
                                    break;   // we don't want to get the next token...stay on the current one
                                }
                                else if (_currentFunctionDef != null)
                                {
                                    _currentFunctionDef.Name = token.TokenText;
                                    _fss = FunctionSearchState.LookingForFunctionSignature;
                                    AdvanceToken(ref token, ref prevToken);
                                }
                            }
                            else
                            {
                                // exit this function
                                end = true;
                            }
                            break;
                        }
                    case FunctionSearchState.LookingForFunctionSignature:
                        {
                            // read function signature
                            switch (fsss)
                            {
                                case FunctionSignatureSearchState.LookingForOpenParen:
                                    {
                                        if (token.TokenType == GeneroTokenType.Symbol &&
                                           token.TokenText == "(")
                                        {
                                            fsss = FunctionSignatureSearchState.LookingForParam;
                                            AdvanceToken(ref token, ref prevToken);
                                        }
                                        else
                                        {
                                            // function not complete, move on
                                            _fss = FunctionSearchState.LookingForFunctionEnd;
                                        }
                                    }
                                    break;
                                case FunctionSignatureSearchState.LookingForParam:
                                    {
                                        if (token.TokenType == GeneroTokenType.Identifier ||
                                           token.TokenType == GeneroTokenType.Keyword)
                                        {
                                            // advance to see if we have a comma (in which case we should look for another param)
                                            _currentFunctionDef.Parameters.Add(token.TokenText);    // we'll fill in the variable definition below
                                            AdvanceToken(ref token, ref prevToken);
                                            if (token.TokenText == ",")
                                            {
                                                AdvanceToken(ref token, ref prevToken);
                                                fsss = FunctionSignatureSearchState.LookingForParam;
                                            }
                                            else
                                            {
                                                fsss = FunctionSignatureSearchState.LookingForClosedParen;
                                            }
                                        }
                                        else
                                        {
                                            // function not complete, move on
                                            fsss = FunctionSignatureSearchState.LookingForClosedParen;
                                        }
                                    }
                                    break;
                                case FunctionSignatureSearchState.LookingForClosedParen:
                                    {
                                        if (token.TokenType == GeneroTokenType.Symbol &&
                                           token.TokenText == ")")
                                        {
                                            fsss = FunctionSignatureSearchState.LookingForOpenParen;
                                            _fss = FunctionSearchState.LookingForFunctionEnd;
                                            AdvanceToken(ref token, ref prevToken);
                                        }
                                        else
                                        {
                                            // function not complete, move on
                                            _fss = FunctionSearchState.LookingForFunctionEnd;
                                        }
                                    }
                                    break;
                            }
                            break;
                        }
                    case FunctionSearchState.LookingForFunctionEnd:
                        {
                            if (token.TokenType == GeneroTokenType.Unknown ||
                                token.TokenType == GeneroTokenType.Eof)
                            {
                                end = true;
                                break;
                            }

                            //bool advanced = false;
                            // look for function variables and consume them
                            if (_vss == VariableSearchState.LookingForDefineKeyword)
                            {
                                // TODO: not sure if we want to do anything with the return value
                                Dictionary<string, int> temp = new Dictionary<string, int>();
                                bool defFound = TryParseVarConstTypeDefinitions(ref token, ref prevToken, ref _vss, _currentFunctionDef.Variables, _currentFunctionDef.Types, _currentFunctionDef.Constants, ref _currentVariableDef, _variableBuffer, temp, new[] { "end", "define", "type", "constant" });
                                if (defFound)
                                {
                                    break;
                                }

                            }

                            TryParseCreateTempTable(ref token, ref prevToken);
                            TryParseCursorPreparation(ref token, ref prevToken);
                            TryParseCursorDeclaration(ref token, ref prevToken);

                            if (_currentFunctionDef.Returns.Count == 0)
                                TryParseReturnStatement(ref token, ref prevToken);

                            if (token.TokenType == GeneroTokenType.Keyword)
                            {
                                // we've hit a new function and the one we were on wasn't complete...so scrap the current one
                                if (token.LowercaseText == "function" || token.LowercaseText == "main" ||
                                    (token.LowercaseText == "report" &&
                                    !(prevToken.LowercaseText == "start" || prevToken.LowercaseText == "to" || prevToken.LowercaseText == "finish" || prevToken.LowercaseText == "action")))
                                {
                                    _currentFunctionDef = null;
                                    _fss = FunctionSearchState.LookingForFunctionStart;
                                    // don't advance, let this bubble up to the parser while loop
                                    end = false;
                                }
                                else if (token.LowercaseText == "end")
                                {
                                    // get next token. If it's "function" we've completed the function
                                    AdvanceToken(ref token, ref prevToken);

                                    if ((_currentFunctionDef.Main && token.LowercaseText == "main") ||
                                        (_currentFunctionDef.Report && token.LowercaseText == "report") ||
                                        (token.LowercaseText == "function"))
                                    {
                                        var nextToken = _lexer.Lookahead(1);
                                        if (nextToken.TokenType == GeneroTokenType.Identifier)
                                        {
                                            _currentFunctionDef = null;
                                            _fss = FunctionSearchState.LookingForFunctionStart;
                                            // don't advance, let this bubble up to the parser while loop
                                            end = false;
                                            break;
                                        }
                                        // either way, if there's an identifier as the next token
                                        _currentFunctionDef.End = token.EndPosition;
                                        string tempName = _currentFunctionDef.Name;
                                        if (tempName == null && _currentFunctionDef.Main)    // this is the main function
                                            tempName = "main";
                                        tempName = tempName.ToLower();
                                        _moduleContents.FunctionDefinitions.AddOrUpdate(tempName, _currentFunctionDef,
                                            (x, y) =>
                                            {
                                                return _currentFunctionDef;
                                            });
                                        int i = 1;
                                        string baseName = tempName;
                                        while (existingFunctionsParsed.ContainsKey(tempName))
                                            tempName = baseName + "_copy" + i++;
                                        tempName = tempName.ToLower();
                                        existingFunctionsParsed.Add(tempName, 1);
                                        _currentFunctionDef = null;
                                        _fss = FunctionSearchState.LookingForFunctionStart;
                                        AdvanceToken(ref token, ref prevToken);
                                        end = ret = true;
                                    }
                                    else
                                    {
                                        //_currentFunctionDef = null;
                                        //_fss = FunctionSearchState.LookingForFunctionStart;
                                        //// don't advance, let this bubble up to the parser while loop
                                        //end = false;
                                    }
                                }
                                else
                                {
                                    // continue searching
                                    AdvanceToken(ref token, ref prevToken);
                                }
                            }
                            else
                            {
                                // continue searching
                                AdvanceToken(ref token, ref prevToken);
                            }
                            break;
                        }
                }
            }

            return ret;
        }

        private bool TryParseCreateTempTable(ref GeneroToken token, ref GeneroToken prevToken)
        {
            bool getVariableName = false;
            if (token.LowercaseText == "create")
            {
                var nextToken = _lexer.Lookahead(1);
                if (nextToken.LowercaseText == "temp")
                {
                    var nextNextToken = _lexer.Lookahead(2);
                    if (nextNextToken.LowercaseText == "table")
                    {
                        AdvanceToken(ref token, ref prevToken);
                        AdvanceToken(ref token, ref prevToken);
                        AdvanceToken(ref token, ref prevToken);

                        TempTableDefinition ttd = new TempTableDefinition();
                        if (token.TokenType != GeneroTokenType.Eof)
                        {
                            ttd.Name = token.TokenText;
                            ttd.Position = token.StartPosition;
                            ttd.LineNumber = token.LineNumber;
                            ttd.ColumnNumber = token.ColumnNumber;
                            AdvanceToken(ref token, ref prevToken);
                            if (token.TokenText == "(")
                            {
                                AdvanceToken(ref token, ref prevToken);
                                getVariableName = true;

                                // TODO: need to go through and set the columns
                                VariableDefinition column = null;
                                while (true)
                                {
                                    if (getVariableName)
                                    {
                                        // if the next token is an identifier or keyword, we can get a name
                                        if (token.TokenType == GeneroTokenType.Identifier || token.TokenType == GeneroTokenType.Keyword)
                                        {
                                            column = new VariableDefinition();
                                            column.Name = token.TokenText;
                                            // go to the next token
                                            AdvanceToken(ref token, ref prevToken);
                                            getVariableName = false;
                                        }
                                        else
                                        {
                                            // give up on parsing the table
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // get the column type
                                        VariableSearchState tableVss = VariableSearchState.LookingForVariableType;
                                        List<VariableDefinition> dummyVarList = new List<VariableDefinition>();
                                        bool valid = TryGetActualVariableType(ref token, ref prevToken, ref tableVss, ref column, dummyVarList);
                                        if (valid)
                                        {
                                            ttd.Columns.AddOrUpdate(column.Name.ToLower(), column, (x, y) => column);
                                            if (tableVss == VariableSearchState.LookingForDefineKeyword)
                                            {
                                                // The table definition is done
                                                break;
                                            }
                                            else
                                            {
                                                AdvanceToken(ref token, ref prevToken);
                                                getVariableName = true;
                                            }
                                        }
                                    }
                                }

                                _moduleContents.TempTables.AddOrUpdate(ttd.Name, ttd, (x, y) => ttd);
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool TryParseCursorDeclaration(ref GeneroToken token, ref GeneroToken prevToken)
        {
            int startingPosition = token.StartPosition;
            int startingLine = token.LineNumber;
            int startingColumn = token.ColumnNumber;
            if (token.LowercaseText == "declare")
            {
                AdvanceToken(ref token, ref prevToken);
                if (token.TokenType == GeneroTokenType.Identifier)
                {
                    CursorDeclaration cd = new CursorDeclaration();
                    cd.Name = token.TokenText;

                    AdvanceToken(ref token, ref prevToken);
                    if (token.LowercaseText == "scroll")
                    {
                        cd.Options.Add(token.LowercaseText);
                        AdvanceToken(ref token, ref prevToken);
                    }

                    if (token.LowercaseText != "cursor")
                        return false;

                    AdvanceToken(ref token, ref prevToken);

                    if (token.LowercaseText == "with")
                    {
                        AdvanceToken(ref token, ref prevToken);
                        if (token.LowercaseText == "hold")
                        {
                            cd.Options.Add("with hold");
                            AdvanceToken(ref token, ref prevToken);
                        }
                        else
                            return false;
                    }

                    if (token.LowercaseText == "for")
                    {
                        AdvanceToken(ref token, ref prevToken);
                        // 3 potential cases
                        // 1) static sql statement -> the next token will be "select"
                        if (token.LowercaseText == "select")
                        {
                            // TODO:
                        }
                        // 2) prepared statement -> next token will be an identifier, which should be found in _moduleContents
                        else if (token.TokenType == GeneroTokenType.Identifier)
                        {
                            CursorPreparation cp;
                            if (_moduleContents.SqlPrepares.TryGetValue(token.TokenText.ToLower(), out cp))
                            {
                                cd.PreparationVariable = cp.Name;
                                cd.Position = startingPosition;
                                cd.LineNumber = startingLine;
                                cd.ColumnNumber = startingColumn;
                                _moduleContents.SqlCursors.Add(cd.Name.ToLower(), cd);
                            }
                        }
                        // 3) a sql block -> next token will be "sql"
                        else if (token.TokenText == "sql")
                        {
                            // TODO:
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (token.LowercaseText == "from")
                    {
                        AdvanceToken(ref token, ref prevToken);
                        // The next token(s) will be one or more strings which comprise a string expression for the cursor
                        if (token.TokenType == GeneroTokenType.String)
                        {

                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool TryParseReturnStatement(ref GeneroToken token, ref GeneroToken prevToken)
        {
            bool validReturnFound = false;
            if (token.LowercaseText == "return")
            {
                List<GeneroToken> returns = new List<GeneroToken>();
                List<GeneroFunctionReturn> funcReturns = new List<GeneroFunctionReturn>();
                bool getNextReturnVar = true;
                bool checkCollectedReturns = false;
                AdvanceToken(ref token, ref prevToken);

                while (token.TokenType != GeneroTokenType.Unknown)
                {
                    if (token.LowercaseText != ",")
                    {
                        if (getNextReturnVar)
                        {
                            returns.Add(token);
                            getNextReturnVar = false;
                        }
                        else
                        {
                            checkCollectedReturns = true;
                            break;
                        }
                    }
                    else
                    {
                        getNextReturnVar = true;
                    }
                    AdvanceToken(ref token, ref prevToken);
                }

                if (checkCollectedReturns)
                {
                    // see if the returns we found a variables with types, or string literal
                    // we cannot determine the type of a number literal being passed, so it must be resolved by the user
                    bool invalid = false;
                    foreach (var retToken in returns)
                    {
                        VariableDefinition varDef;
                        if (retToken.TokenType == GeneroTokenType.String)
                        {
                            funcReturns.Add(new GeneroFunctionReturn { Type = "string" });
                        }
                        else if (_currentFunctionDef.Variables.TryGetValue(retToken.TokenText.ToLower(), out varDef))
                        {
                            funcReturns.Add(new GeneroFunctionReturn { Name = varDef.Name, Type = varDef.Type });
                        }
                        else
                        {
                            invalid = true;
                            break;
                        }
                    }
                    if (!invalid)
                    {
                        foreach (var ret in funcReturns)
                            _currentFunctionDef.Returns.Add(ret);
                        validReturnFound = true;
                    }
                }
            }

            return validReturnFound;
        }

        private bool TryParseCursorPreparation(ref GeneroToken token, ref GeneroToken prevToken)
        {
            int startingPosition = token.StartPosition;
            int startingLine = token.LineNumber;
            int startingColumn = token.ColumnNumber;
            if (token.LowercaseText == "prepare")
            {
                AdvanceToken(ref token, ref prevToken);
                if (token.TokenType == GeneroTokenType.Identifier)
                {
                    CursorPreparation cp = new CursorPreparation();
                    cp.Name = token.TokenText;

                    AdvanceToken(ref token, ref prevToken);
                    if (token.LowercaseText == "from")
                    {
                        AdvanceToken(ref token, ref prevToken);
                        if (token.TokenType == GeneroTokenType.Identifier)
                        {
                            cp.StatementVariable = token.TokenText;
                            cp.Position = startingPosition;
                            cp.LineNumber = startingLine;
                            cp.ColumnNumber = startingColumn;

                            // check and make sure the variable actually exists
                            VariableDefinition prepareDef;
                            if (_currentFunctionDef.Variables.TryGetValue(cp.StatementVariable.ToLower(), out prepareDef) ||
                                _moduleContents.ModuleVariables.TryGetValue(cp.StatementVariable.ToLower(), out prepareDef) ||
                                _moduleContents.GlobalVariables.TryGetValue(cp.StatementVariable.ToLower(), out prepareDef))
                            {
                                // need to get the Statement variable's contents
                                Stack<GeneroToken> prepareContents = new Stack<GeneroToken>();    // This will store possibly multiple lines' worth of prepare statement
                                Stack<GeneroToken> lineContents = new Stack<GeneroToken>();
                                GeneroLexer tempLexer = new GeneroLexer();
                                int prepareLineNumber = _currentBuffer.CurrentSnapshot.GetLineNumberFromPosition(startingPosition);
                                bool prepareVariableFound = false;
                                while (!prepareVariableFound)
                                {
                                    if (prepareLineNumber > 0)
                                        prepareLineNumber--;
                                    if (prepareLineNumber < _currentFunctionDef.LineNumber)
                                    {
                                        // we can't gather information on a cursor whose sql text is not set within the current function.
                                        // set the cursor statement in the prepare
                                        cp.CursorStatement = "";
                                        _moduleContents.SqlPrepares.Add(cp.Name, cp);
                                        return true;
                                    }
                                    var textSnapshotLine = _currentBuffer.CurrentSnapshot.GetLineFromLineNumber(prepareLineNumber);
                                    tempLexer.StartLexing(0, textSnapshotLine.GetText());
                                    GeneroToken currToken = null;
                                    lineContents.Clear();
                                    while ((currToken = tempLexer.NextToken()).TokenType != GeneroTokenType.Unknown &&
                                          currToken.TokenType != GeneroTokenType.Eof)
                                    {
                                        if (currToken.LowercaseText == "let")
                                        {
                                            var letTok = tempLexer.Lookahead(1);
                                            if (letTok != null && letTok.TokenText == cp.StatementVariable)
                                                prepareVariableFound = true;
                                        }
                                        lineContents.Push(currToken);
                                    }
                                    while (lineContents.Count > 0)
                                        prepareContents.Push(lineContents.Pop());
                                }

                                // Now we need to start popping off the stack, and grab the contiguous string literal after the assignment
                                GeneroToken tempTok;
                                List<string> prepareStringPieces = new List<string>();
                                bool collect = false;
                                while (prepareContents.Count > 0)
                                {
                                    tempTok = prepareContents.Pop();
                                    if (tempTok.TokenText == "=")
                                        collect = true;
                                    else
                                    {
                                        if (collect)
                                        {
                                            if (tempTok.TokenType == GeneroTokenType.String)
                                            {
                                                prepareStringPieces.Add(tempTok.TokenText);
                                            }
                                            else if (tempTok.TokenType != GeneroTokenType.Comment && tempTok.TokenText != ",")
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }

                                // now we need to put the prepareStringPieces together
                                string cursorStatement = "";
                                foreach (var cursorPiece in prepareStringPieces)
                                {
                                    cursorStatement += cursorPiece.Replace("\"", "");
                                }

                                // set the cursor statement in the prepare
                                cp.CursorStatement = cursorStatement;

                                _moduleContents.SqlPrepares.Add(cp.Name, cp);
                            }
                        }
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
