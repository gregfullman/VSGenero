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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VSGenero.Analysis;
using VSGenero.EditorExtensions;
using VSGenero.EditorExtensions.Intellisense;
using Microsoft.VisualStudio.VSCommon;
using VSGenero.Refactoring;
using VSGenero.Analysis.Parsing;

namespace VSGenero.Navigation
{
    public sealed class EditFilter : IOleCommandTarget, IDisposable
    {
        private readonly ITextView _textView;
        private readonly IEditorOperations _editorOps;
        private readonly System.IServiceProvider _serviceProvider;
        private IFunctionInformationProvider _functionProvider;
        private IDatabaseInformationProvider _databaseProvider;
        private IProgramFileProvider _programFileProvider;
        private IOleCommandTarget _next;

        public EditFilter(ITextView textView, IEditorOperations editorOps, System.IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _textView = textView;
            _editorOps = editorOps;

            Genero4glClassifier classifier;
            if (_textView.TextBuffer.Properties.TryGetProperty(typeof(Genero4glClassifier), out classifier))
            {
                _functionProvider = classifier.Provider._PublicFunctionProvider;
                _databaseProvider = classifier.Provider._DatabaseInfoProvider;
                _programFileProvider = classifier.Provider._ProgramFileProvider;
            }

            BraceMatcher.WatchBraceHighlights(textView, VSGeneroPackage.ComponentModel);
        }

        internal void AttachKeyboardFilter(IVsTextView vsTextView)
        {
            if (_next == null)
            {
                ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out _next));
            }
        }

        public void DetachKeyboardFilter(IVsTextView vsTextView)
        {
            vsTextView.RemoveCommandFilter(this);
        }

        public void Dispose()
        {
            // Nothing to clean up right now...
        }

        public enum GetLocationOptions
        {
            Definitions = 1,
            References = 2,
            Values = 4
        }

        public static IEnumerable<LocationInfo> GetLocations(ITextView textView, GeneroLanguageVersion languageVersion, GetLocationOptions options = GetLocationOptions.Definitions | GetLocationOptions.References | GetLocationOptions.Values)
        {
            List<LocationInfo> locations = new List<LocationInfo>();

            Genero4glClassifier classifier;
            IFunctionInformationProvider funcProvider = null;
            IDatabaseInformationProvider dbProvider = null;
            IProgramFileProvider progfileProvider = null;
            if (textView.TextBuffer.Properties.TryGetProperty(typeof(Genero4glClassifier), out classifier))
            {
                funcProvider = classifier.Provider._PublicFunctionProvider;
                dbProvider = classifier.Provider._DatabaseInfoProvider;
                progfileProvider = classifier.Provider._ProgramFileProvider;
            }

            var analysis = textView.GetExpressionAnalysis(funcProvider, dbProvider, progfileProvider);
            if (analysis != null)
            {

                Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
                GetDefsRefsAndValues(analysis, languageVersion, out definitions, out references, out values);

                if (options.HasFlag(GetLocationOptions.Values))
                {
                    foreach (var location in values.Keys)
                    {
                        locations.Add(location);
                    }
                }

                if (options.HasFlag(GetLocationOptions.Definitions))
                {
                    foreach (var location in definitions.Keys)
                    {
                        locations.Add(location);
                    }
                }

                if (options.HasFlag(GetLocationOptions.References))
                {
                    foreach (var location in references.Keys)
                    {
                        locations.Add(location);
                    }
                }
            }

            return locations;
        }

        public static IAnalysisResult GetAnalysisItemUnderCaret(ITextView textView)
        {
            IAnalysisResult result = null;
            // don't need the various providers, at least right now
            var analysis = textView.GetExpressionAnalysis(null, null, null);
            if(analysis != null)
            {
                result = analysis.Value;
            }
            return result;
        }

        /// <summary>
        /// Implements Goto Definition.  Called when the user selects Goto Definition from the 
        /// context menu or hits the hotkey associated with Goto Definition.
        /// 
        /// If there is 1 and only one definition immediately navigates to it.  If there are
        /// no references displays a dialog box to the user.  Otherwise it opens the find
        /// symbols dialog with the list of results.
        /// </summary>
        private int GotoDefinition()
        {
            UpdateStatusForIncompleteAnalysis();

            var analysis = _textView.GetExpressionAnalysis(_functionProvider, _databaseProvider, _programFileProvider);
            if (analysis == null)
            {
                MessageBox.Show(String.Format("Cannot go to definition.  The cursor is not on a symbol."), "VSGenero");
                return VSConstants.S_OK;
            }
            else
            {
                Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
                GetDefsRefsAndValues(analysis, _textView.GetLanguageVersion(_programFileProvider), out definitions, out references, out values);

                if ((values.Count + definitions.Count) == 1)
                {
                    if (values.Count != 0)
                    {
                        foreach (var location in values.Keys)
                        {
                            GotoLocation(location);
                            break;
                        }
                    }
                    else
                    {
                        foreach (var location in definitions.Keys)
                        {
                            GotoLocation(location);
                            break;
                        }
                    }
                }
                else if (values.Count + definitions.Count == 0)
                {
                    if (String.IsNullOrWhiteSpace(analysis.Expression))
                    {
                        MessageBox.Show(String.Format("Cannot go to definition.  The cursor is not on a symbol."), "VSGenero");
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Cannot go to definition \"{0}\"", analysis.Expression), "VSGenero");
                    }
                }
                else if (definitions.Count == 0)
                {
                    ShowFindSymbolsDialog(analysis, new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values));
                }
                else if (values.Count == 0)
                {
                    ShowFindSymbolsDialog(analysis, new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values));
                }
                else
                {
                    ShowFindSymbolsDialog(analysis,
                        new LocationCategory("Goto Definition",
                            new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values),
                            new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values)
                        )
                    );
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Moves the caret to the specified location, staying in the current text view 
        /// if possible.
        /// 
        /// https://pytools.codeplex.com/workitem/1649
        /// </summary>
        private void GotoLocation(LocationInfo location)
        {
            if (CommonUtils.IsSamePath(location.FilePath, _textView.GetFilePath()))
            {
                var adapterFactory = VSGeneroPackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
                var viewAdapter = adapterFactory.GetViewAdapter(_textView);
                var lineNum = location.Line - 1;
                if(lineNum <= 0)
                {
                    lineNum = _textView.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(location.Index);
                }
                var character = location.Column - 1;  // start index
                if (character < 0)
                    character = 0;
                viewAdapter.SetCaretPos(lineNum, character);
                viewAdapter.CenterLines(lineNum, 1);
            }
            else
            {
                location.GotoSource(_serviceProvider, _textView.GetLanguageVersion(_programFileProvider));
            }
        }

        /// <summary>
        /// Implements Find All References.  Called when the user selects Find All References from
        /// the context menu or hits the hotkey associated with find all references.
        /// 
        /// Always opens the Find Symbol Results box to display the results.
        /// </summary>
        private int FindAllReferences()
        {
            UpdateStatusForIncompleteAnalysis();

            var analysis = _textView.GetExpressionAnalysis(_functionProvider, _databaseProvider, _programFileProvider);
            LocationCategory locations = null;
            if (analysis != null)
            {
                locations = GetFindRefLocations(analysis, _textView.GetLanguageVersion(_programFileProvider));
            }
            ShowFindSymbolsDialog(analysis, locations);

            return VSConstants.S_OK;
        }

        internal static LocationCategory GetFindRefLocations(ExpressionAnalysis analysis, GeneroLanguageVersion languageVersion)
        {
            Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
            GetDefsRefsAndValues(analysis, languageVersion, out definitions, out references, out values);

            var locations = new LocationCategory("Find All References",
                    new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values),
                    new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values),
                    new SymbolList("References", StandardGlyphGroup.GlyphReference, references.Values)
                );
            return locations;
        }

        private static void GetDefsRefsAndValues(ExpressionAnalysis provider, GeneroLanguageVersion languageVersion, out Dictionary<LocationInfo, SimpleLocationInfo> definitions, out Dictionary<LocationInfo, SimpleLocationInfo> references, out Dictionary<LocationInfo, SimpleLocationInfo> values)
        {
            references = new Dictionary<LocationInfo, SimpleLocationInfo>();
            definitions = new Dictionary<LocationInfo, SimpleLocationInfo>();
            values = new Dictionary<LocationInfo, SimpleLocationInfo>();

            var priorityVariables = new Dictionary<string, IAnalysisVariable>(StringComparer.OrdinalIgnoreCase);

            foreach (var v in provider.Variables.Where(x => x.Location != null))
            {
                switch (v.Type)
                {
                    case VariableType.Definition:
                        values.Remove(v.Location);

                        if(!string.IsNullOrWhiteSpace(v.Name) && v.Priority != 0)
                        {
                            IAnalysisVariable existing;
                            if(priorityVariables.TryGetValue(v.Name, out existing))
                            {
                                if(existing.Priority <= v.Priority)
                                {
                                    break;
                                }
                                else
                                {
                                    priorityVariables.Remove(v.Name);
                                }
                            }
                            priorityVariables.Add(v.Name, v);
                        }

                        definitions[v.Location] = new SimpleLocationInfo(null, provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField, languageVersion);
                        break;
                    case VariableType.Reference:
                        references[v.Location] = new SimpleLocationInfo(null, provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField, languageVersion);
                        break;
                    case VariableType.Value:
                        if (!definitions.ContainsKey(v.Location))
                        {
                            values[v.Location] = new SimpleLocationInfo(null, provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField, languageVersion);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Opens the find symbols dialog with a list of results.  This is done by requesting
        /// that VS does a search against our library GUID.  Our library then responds to
        /// that request by extracting the prvoided symbol list out and using that for the
        /// search results.
        /// </summary>
        private static void ShowFindSymbolsDialog(ExpressionAnalysis provider, IVsNavInfo symbols)
        {
            // ensure our library is loaded so find all references will go to our library
            //VSGeneroPackage.GetGlobalService(typeof(IGeneroLibraryManager));
            // For some reason, using the GetGlobalService call was calling resulting in a call to an external package function, if it existed.
            // We want to keep the call to within this package.
            VSGeneroPackage.Instance.LoadLibraryManager();

            if (provider != null && provider.Expression != "")
            {
                var findSym = (IVsFindSymbol)VSGeneroPackage.GetGlobalService(typeof(SVsObjectSearch));
                VSOBSEARCHCRITERIA2 searchCriteria = new VSOBSEARCHCRITERIA2();
                searchCriteria.eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD;
                searchCriteria.pIVsNavInfo = symbols;
                searchCriteria.grfOptions = (uint)_VSOBSEARCHOPTIONS2.VSOBSO_LISTREFERENCES;
                searchCriteria.szName = provider.Expression;

                Guid guid = Guid.Empty;
                //  new Guid("{a5a527ea-cf0a-4abf-b501-eafe6b3ba5c6}")
                int hResult = findSym.DoSearch(new Guid(CommonConstants.LibraryGuid), new VSOBSEARCHCRITERIA2[] { searchCriteria });
                ErrorHandler.ThrowOnFailure(hResult);
            }
            else
            {
                var statusBar = (IVsStatusbar)VSGeneroPackage.GetGlobalService(typeof(SVsStatusbar));
                statusBar.SetText("The caret must be on valid expression to find all references.");
            }
        }



        internal class LocationCategory : SimpleObjectList<SymbolList>, IVsNavInfo, ICustomSearchListProvider
        {
            private readonly string _name;

            internal LocationCategory(string name, params SymbolList[] locations)
            {
                _name = name;

                foreach (var location in locations)
                {
                    if (location.Children.Count > 0)
                    {
                        Children.Add(location);
                    }
                }
            }

            public override uint CategoryField(LIB_CATEGORY lIB_CATEGORY)
            {
                return (uint)(_LIB_LISTTYPE.LLT_HIERARCHY | _LIB_LISTTYPE.LLT_MEMBERS | _LIB_LISTTYPE.LLT_PACKAGE);
            }

            #region IVsNavInfo Members

            public int EnumCanonicalNodes(out IVsEnumNavInfoNodes ppEnum)
            {
                ppEnum = new NodeEnumerator<SymbolList>(Children);
                return VSConstants.S_OK;
            }

            public int EnumPresentationNodes(uint dwFlags, out IVsEnumNavInfoNodes ppEnum)
            {
                ppEnum = new NodeEnumerator<SymbolList>(Children);
                return VSConstants.S_OK;
            }

            public int GetLibGuid(out Guid pGuid)
            {
                pGuid = Guid.Empty;
                return VSConstants.S_OK;
            }

            public int GetSymbolType(out uint pdwType)
            {
                pdwType = (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion

            #region ICustomSearchListProvider Members

            public IVsSimpleObjectList2 GetSearchList()
            {
                return this;
            }

            #endregion
        }

        internal class SimpleLocationInfo : SimpleObject, IVsNavInfoNode
        {
            private readonly LocationInfo _locationInfo;
            private readonly StandardGlyphGroup _glyphType;
            private readonly string _pathText, _lineText;
            private readonly System.IServiceProvider _serviceProvider;
            private readonly GeneroLanguageVersion _languageVersion;

            public SimpleLocationInfo(System.IServiceProvider serviceProvider, string searchText, LocationInfo locInfo, StandardGlyphGroup glyphType, GeneroLanguageVersion languageVersion)
            {
                _serviceProvider = serviceProvider;
                _locationInfo = locInfo;
                _glyphType = glyphType;
                _languageVersion = languageVersion;
                _pathText = GetSearchDisplayText();
                if (_locationInfo.ProjectEntry != null)
                    _lineText = _locationInfo.ProjectEntry.GetLine(_locationInfo.Line);
                else
                    _lineText = "";
            }

            public override string Name
            {
                get
                {
                    return _locationInfo.FilePath;
                }
            }

            public override string GetTextRepresentation(VSTREETEXTOPTIONS options)
            {
                if (options == VSTREETEXTOPTIONS.TTO_DEFAULT)
                {
                    return _pathText + _lineText.Trim();
                }
                return String.Empty;
            }

            private string GetSearchDisplayText()
            {
                return String.Format("{0} - ({1}, {2}): ",
                    _locationInfo.FilePath,
                    _locationInfo.Line,
                    _locationInfo.Column);
            }

            public override string UniqueName
            {
                get
                {
                    return _locationInfo.FilePath;
                }
            }

            public override bool CanGoToSource
            {
                get
                {
                    return true;
                }
            }

            public override VSTREEDISPLAYDATA DisplayData
            {
                get
                {
                    var res = new VSTREEDISPLAYDATA();
                    res.Image = res.SelectedImage = (ushort)_glyphType;
                    res.State = (uint)_VSTREEDISPLAYSTATE.TDS_FORCESELECT;

                    // This code highlights the text but it gets the wrong region.  This should be re-enabled
                    // and highlight the correct region.

                    //res.ForceSelectStart = (ushort)(_pathText.Length + _locationInfo.Column - 1);
                    //res.ForceSelectLength = (ushort)_locationInfo.Length;
                    return res;
                }
            }

            public override void GotoSource(VSOBJGOTOSRCTYPE SrcType)
            {
                _locationInfo.GotoSource(_serviceProvider, _languageVersion);
            }

            #region IVsNavInfoNode Members

            public int get_Name(out string pbstrName)
            {
                pbstrName = _locationInfo.FilePath;
                return VSConstants.S_OK;
            }

            public int get_Type(out uint pllt)
            {
                pllt = 16; // (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion
        }

        internal class SymbolList : SimpleObjectList<SimpleLocationInfo>, IVsNavInfo, IVsNavInfoNode, ICustomSearchListProvider, ISimpleObject
        {
            private readonly string _name;
            private readonly StandardGlyphGroup _glyphGroup;

            internal SymbolList(string description, StandardGlyphGroup glyphGroup, IEnumerable<SimpleLocationInfo> locations)
            {
                _name = description;
                _glyphGroup = glyphGroup;
                Children.AddRange(locations);
            }

            public override uint CategoryField(LIB_CATEGORY lIB_CATEGORY)
            {
                return (uint)(_LIB_LISTTYPE.LLT_MEMBERS | _LIB_LISTTYPE.LLT_PACKAGE);
            }

            #region ISimpleObject Members

            public bool CanDelete
            {
                get { return false; }
            }

            public bool CanGoToSource
            {
                get { return false; }
            }

            public bool CanRename
            {
                get { return false; }
            }

            public string Name
            {
                get { return _name; }
            }

            public string UniqueName
            {
                get { return _name; }
            }

            public string FullName
            {
                get
                {
                    return _name;
                }
            }

            public string GetTextRepresentation(VSTREETEXTOPTIONS options)
            {
                switch (options)
                {
                    case VSTREETEXTOPTIONS.TTO_DISPLAYTEXT:
                        return _name;
                }
                return null;
            }

            public string TooltipText
            {
                get { return null; }
            }

            public object BrowseObject
            {
                get { return null; }
            }

            public System.ComponentModel.Design.CommandID ContextMenuID
            {
                get { return null; }
            }

            public VSTREEDISPLAYDATA DisplayData
            {
                get
                {
                    var res = new VSTREEDISPLAYDATA();
                    res.Image = res.SelectedImage = (ushort)_glyphGroup;
                    return res;
                }
            }

            public void Delete()
            {
            }

            public void DoDragDrop(OleDataObject dataObject, uint grfKeyState, uint pdwEffect)
            {
            }

            public void Rename(string pszNewName, uint grfFlags)
            {
            }

            public void GotoSource(VSOBJGOTOSRCTYPE SrcType)
            {
            }

            public void SourceItems(out IVsHierarchy ppHier, out uint pItemid, out uint pcItems)
            {
                ppHier = null;
                pItemid = 0;
                pcItems = 0;
            }

            public uint EnumClipboardFormats(_VSOBJCFFLAGS _VSOBJCFFLAGS, VSOBJCLIPFORMAT[] rgcfFormats)
            {
                return VSConstants.S_OK;
            }

            public void FillDescription(_VSOBJDESCOPTIONS _VSOBJDESCOPTIONS, IVsObjectBrowserDescription3 pobDesc)
            {
            }

            public IVsSimpleObjectList2 FilterView(uint ListType)
            {
                return this;
            }

            #endregion

            #region IVsNavInfo Members

            public int EnumCanonicalNodes(out IVsEnumNavInfoNodes ppEnum)
            {
                ppEnum = new NodeEnumerator<SimpleLocationInfo>(Children);
                return VSConstants.S_OK;
            }

            public int EnumPresentationNodes(uint dwFlags, out IVsEnumNavInfoNodes ppEnum)
            {
                ppEnum = new NodeEnumerator<SimpleLocationInfo>(Children);
                return VSConstants.S_OK;
            }

            public int GetLibGuid(out Guid pGuid)
            {
                pGuid = Guid.Empty;
                return VSConstants.S_OK;
            }

            public int GetSymbolType(out uint pdwType)
            {
                pdwType = (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion

            #region ICustomSearchListProvider Members

            public IVsSimpleObjectList2 GetSearchList()
            {
                return this;
            }

            #endregion

            #region IVsNavInfoNode Members

            public int get_Name(out string pbstrName)
            {
                pbstrName = "name";
                return VSConstants.S_OK;
            }

            public int get_Type(out uint pllt)
            {
                pllt = 16; // (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion
        }

        class NodeEnumerator<T> : IVsEnumNavInfoNodes where T : IVsNavInfoNode
        {
            private readonly List<T> _locations;
            private IEnumerator<T> _locationEnum;

            public NodeEnumerator(List<T> locations)
            {
                _locations = locations;
                Reset();
            }

            #region IVsEnumNavInfoNodes Members

            public int Clone(out IVsEnumNavInfoNodes ppEnum)
            {
                ppEnum = new NodeEnumerator<T>(_locations);
                return VSConstants.S_OK;
            }

            public int Next(uint celt, IVsNavInfoNode[] rgelt, out uint pceltFetched)
            {
                pceltFetched = 0;
                while (celt-- != 0 && _locationEnum.MoveNext())
                {
                    rgelt[pceltFetched++] = _locationEnum.Current;
                }
                return VSConstants.S_OK;
            }

            public int Reset()
            {
                _locationEnum = _locations.GetEnumerator();
                return VSConstants.S_OK;
            }

            public int Skip(uint celt)
            {
                while (celt-- != 0)
                {
                    _locationEnum.MoveNext();
                }
                return VSConstants.S_OK;
            }

            #endregion
        }

        private void UpdateStatusForIncompleteAnalysis()
        {
            var statusBar = (IVsStatusbar)VSGeneroPackage.GetGlobalService(typeof(SVsStatusbar));
            var analyzer = _textView.GetAnalyzer();
            if (analyzer != null && analyzer.IsAnalyzing)
            {
                statusBar.SetText("Python source analysis is not up to date");
            }
        }

        #region IOleCommandTarget Members

        /// <summary>
        /// Called from VS when we should handle a command or pass it on.
        /// </summary>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (IntellisenseControllerProvider.Instance != null &&
                IntellisenseControllerProvider.Instance.GeneroCommandTarget != null &&
                pguidCmdGroup == IntellisenseControllerProvider.Instance.GeneroCommandTarget.PackageGuid)
            {
                if (_textView != null && _textView.TextBuffer != null)
                {
                    string path = _textView.TextBuffer.GetFilePath();
                    if (IntellisenseControllerProvider.Instance.GeneroCommandTarget.Exec(path, nCmdID))
                    {
                        return VSConstants.S_OK;
                    }
                }
            }

            // preprocessing
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                switch ((VSConstants.VSStd97CmdID)nCmdID)
                {
                    case VSConstants.VSStd97CmdID.Paste:
                        //PythonReplEvaluator eval;
                        //if (_textView.Properties.TryGetProperty(typeof(PythonReplEvaluator), out eval))
                        //{
                        //    string pasting = eval.FormatClipboard() ?? Clipboard.GetText();
                        //    if (pasting != null)
                        //    {
                        //        PasteReplCode(eval, pasting);

                        //        return VSConstants.S_OK;
                        //    }
                        //}
                        //else
                        //{
                        //    string updated = RemoveReplPrompts(_textView.Options.GetNewLineCharacter());
                        //    if (updated != null)
                        //    {
                        //        _editorOps.ReplaceSelection(updated);
                        //        return VSConstants.S_OK;
                        //    }
                        //}
                        break;
                    case VSConstants.VSStd97CmdID.GotoDefn: return GotoDefinition();
                    case VSConstants.VSStd97CmdID.FindReferences: return FindAllReferences();
                }
            }
            else if (pguidCmdGroup == VSGeneroConstants.Std2KCmdGroupGuid)
            {
                OutliningTaggerProvider.OutliningTagger tagger;
                IntellisenseController controller = null;

                switch ((VSConstants.VSStd2KCmdID)nCmdID)
                {
                    case (VSConstants.VSStd2KCmdID)147: // ECMD_SMARTTASKS  defined in stdidcmd.h, but not in MPF
                        // if the user is typing to fast for us to update the smart tags on the idle event
                        // then we want to update them before VS pops them up.
                        UpdateSmartTags();
                        break;
                    //case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                    //    FormatCode(new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, 0, _textView.TextBuffer.CurrentSnapshot.Length), false);
                    //    return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                        if(_textView != null)
                            FormatCode(_textView.Selection.StreamSelectionSpan.SnapshotSpan, true);
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        if (_textView != null)
                        {
                            controller =
                                _textView.Properties.GetProperty<IntellisenseController>(typeof (IntellisenseController));

                            if (controller != null)
                            {
                                IntellisenseController.ForceCompletions = true;
                                try
                                {
                                    controller.TriggerCompletionSession((VSConstants.VSStd2KCmdID) nCmdID ==
                                                                        VSConstants.VSStd2KCmdID.COMPLETEWORD);
                                }
                                finally
                                {
                                    IntellisenseController.ForceCompletions = false;
                                }
                                return VSConstants.S_OK;
                            }
                        }
                        break;

                    case VSConstants.VSStd2KCmdID.QUICKINFO:
                        if (_textView != null)
                        {
                            controller =
                                _textView.Properties.GetProperty<IntellisenseController>(typeof (IntellisenseController));
                            if (controller != null)
                            {
                                controller.TriggerQuickInfo();
                                return VSConstants.S_OK;
                            }
                        }
                        break;

                    case VSConstants.VSStd2KCmdID.PARAMINFO:
                        if (_textView != null)
                        {
                            controller =
                                _textView.Properties.GetProperty<IntellisenseController>(typeof (IntellisenseController));
                            if (controller != null)
                            {
                                controller.TriggerSignatureHelp();
                                return VSConstants.S_OK;
                            }
                        }
                        break;

                    case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                        tagger = _textView.GetOutliningTagger();
                        tagger?.Disable();
                        // let VS get the event as well
                        break;

                    case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                        tagger = _textView.GetOutliningTagger();
                        tagger?.Enable();
                        // let VS get the event as well
                        break;

                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        if (VSGenero.EditorExtensions.EditorExtensions.CommentOrUncommentBlock(_textView, comment: true))
                        {
                            return VSConstants.S_OK;
                        }
                        break;

                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        if (VSGenero.EditorExtensions.EditorExtensions.CommentOrUncommentBlock(_textView, comment: false))
                        {
                            return VSConstants.S_OK;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.EXTRACTMETHOD:
                        ExtractMethod();
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.RENAME:
                        RefactorRename();
                        return VSConstants.S_OK;
                }
            }

            return _next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private bool ExtractMethod()
        {
            return new MethodExtractor(_textView).ExtractMethod(new ExtractMethodUserInput(_serviceProvider));
        }

        private void FormatCode(SnapshotSpan span, bool selectResult)
        {
            // get the line numbers for the start and end of the span
            int startLine = _textView.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(span.Start.Position);
            int endLine = _textView.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(span.End.Position);

            AutoIndent.Format(_textView, startLine, endLine);
        }

        internal void RefactorRename()
        {
            var analyzer = _textView.GetAnalyzer();
            if (analyzer.IsAnalyzing)
            {
                var dialog = new WaitForCompleteAnalysisDialog(analyzer);

                var res = dialog.ShowModal();
                if (res != true)
                {
                    // user cancelled dialog before analysis completed...
                    return;
                }
            }

            new VariableRenamer(_textView, _functionProvider, _databaseProvider, _programFileProvider, _serviceProvider).RenameVariable(
                new RenameVariableUserInput(_serviceProvider),
                (IVsPreviewChangesService)VSGeneroPackage.GetGlobalService(typeof(SVsPreviewChangesService))
            );
        }

        private const uint CommandDisabledAndHidden = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU);
        /// <summary>
        /// Called from VS to see what commands we support.  
        /// </summary>
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd97CmdID.FindReferences:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == VSGeneroConstants.Std2KCmdGroupGuid)
            {
                OutliningTaggerProvider.OutliningTagger tagger;
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                        case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                        case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        case VSConstants.VSStd2KCmdID.QUICKINFO:
                        case VSConstants.VSStd2KCmdID.PARAMINFO:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;

                        case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                            tagger = _textView.GetOutliningTagger();
                            if (tagger != null && tagger.Enabled)
                            {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            }
                            return VSConstants.S_OK;

                        case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                            tagger = _textView.GetOutliningTagger();
                            if (tagger != null && !tagger.Enabled)
                            {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            }
                            return VSConstants.S_OK;

                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.EXTRACTMETHOD:
                            QueryStatusExtractMethod(prgCmds, i);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.RENAME:
                            QueryStatusRename(prgCmds, i);
                            return VSConstants.S_OK;
                    }
                }
            }
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private void QueryStatusExtractMethod(OLECMD[] prgCmds, int i)
        {
            var activeView = VSGeneroPackage.GetActiveTextView();

            if (_textView.TextBuffer.ContentType.IsOfType(VSGeneroConstants.ContentType4GL) ||
                _textView.TextBuffer.ContentType.IsOfType(VSGeneroConstants.ContentTypeINC) ||
                _textView.TextBuffer.ContentType.IsOfType(VSGeneroConstants.ContentTypePER))
            {
                if (_textView.Selection.IsEmpty ||
                    _textView.Selection.Mode == TextSelectionMode.Box ||
                    String.IsNullOrWhiteSpace(_textView.Selection.StreamSelectionSpan.GetText()))
                {
                    prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                }
                else
                {
                    prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                }
            }
            else
            {
                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE);
            }
        }

        private void QueryStatusRename(OLECMD[] prgCmds, int i)
        {
            IWpfTextView activeView = VSGeneroPackage.GetActiveTextView();
            if (_textView.TextBuffer.ContentType.IsOfType(VSGeneroConstants.ContentType4GL) ||
                _textView.TextBuffer.ContentType.IsOfType(VSGeneroConstants.ContentTypeINC) ||
                _textView.TextBuffer.ContentType.IsOfType(VSGeneroConstants.ContentTypePER))
            {
                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
            }
            else
            {
                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE);
            }
        }

        #endregion

        internal void DoIdle(IOleComponentManager compMgr)
        {
            UpdateSmartTags(compMgr);
        }

        private void UpdateSmartTags(IOleComponentManager compMgr = null)
        {
            //SmartTagController controller;
            //if (_textView.Properties.TryGetProperty<SmartTagController>(typeof(SmartTagController), out controller))
            //{
            //    controller.ShowSmartTag(compMgr);
            //}
        }
    }
}
