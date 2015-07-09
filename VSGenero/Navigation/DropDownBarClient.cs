/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio;

using VSGenero.EditorExtensions.Intellisense;
using VSGenero.EditorExtensions;
using Microsoft.VisualStudio.Text;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.VSCommon;
using Microsoft.VisualStudio.VSCommon.Utilities;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.Navigation
{
    class DropDownBarClient : IVsDropdownBarClient
    {
        private IGeneroProjectEntry _projectEntry;
        private ReadOnlyCollection<DropDownEntryInfo> _topLevelEntries; // entries for top-level members of the file
        private ReadOnlyCollection<DropDownEntryInfo> _nestedEntries;   // entries for nested members in the file
        private readonly Dispatcher _dispatcher;                        // current dispatcher so we can get back to our thread
        private IWpfTextView _textView;                                 // text view we're drop downs for
        private IVsDropdownBar _dropDownBar;                            // drop down bar - used to refresh when changes occur
        private int _curTopLevelIndex = -1, _curNestedIndex = -1;       // currently selected indices for each oar
        private GenericSynchronizingObject _synchObj = new GenericSynchronizingObject();

        private static readonly ImageList _imageList = GetImageList();
        private static readonly ReadOnlyCollection<DropDownEntryInfo> EmptyEntries = new ReadOnlyCollection<DropDownEntryInfo>(new DropDownEntryInfo[0]);

        private const int TopLevelComboBoxId = 0;
        //private const int NestedComboBoxId = 1;
        public DropDownBarClient(IWpfTextView textView, IGeneroProjectEntry pythonProjectEntry)
        {
            _projectEntry = pythonProjectEntry;
            _projectEntry.OnNewParseTree += ParserOnNewParseTree;
            _textView = textView;
            _topLevelEntries = _nestedEntries = EmptyEntries;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _textView.Caret.PositionChanged += CaretPositionChanged;
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            int newPosition = e.NewPosition.BufferPosition.Position;

            var topLevel = _topLevelEntries;
            int curTopLevel = _curTopLevelIndex;

            if (curTopLevel != -1 && curTopLevel < topLevel.Count)
            {
                // TODO: need to mark the end of the functions too
                if (newPosition >= topLevel[curTopLevel].Start && newPosition <= topLevel[curTopLevel].End)
                {
                    //UpdateNestedComboSelection(newPosition);
                }
                else
                {
                    FindActiveTopLevelComboSelection(newPosition, topLevel);
                }
            }
            else
            {
                FindActiveTopLevelComboSelection(newPosition, topLevel);
            }
        }

        //private void UpdateNestedComboSelection(int newPosition)
        //{
        //    // left side has not changed, check rhs
        //    int curNested = _curNestedIndex;
        //    var nested = _nestedEntries;

        //    if (curNested != -1 && curNested < nested.Count)
        //    {
        //        if (newPosition < nested[curNested].Start || newPosition > nested[curNested].End)
        //        {
        //            // right hand side has changed
        //            FindActiveNestedSelection(newPosition, nested);
        //        }
        //    }
        //    else
        //    {
        //        FindActiveNestedSelection(newPosition, nested);
        //    }
        //}

        //private void FindActiveNestedSelection(int newPosition, ReadOnlyCollection<DropDownEntryInfo> nested)
        //{
        //    if (_dropDownBar == null)
        //    {
        //        return;
        //    }

        //    int oldNested = _curNestedIndex;

        //    bool found = false;

        //    if (_curTopLevelIndex != -1)
        //    {
        //        for (int i = 0; i < nested.Count; i++)
        //        {
        //            if (newPosition >= nested[i].Start && newPosition <= nested[i].End)
        //            {
        //                _curNestedIndex = i;

        //                if (oldNested == -1)
        //                {
        //                    // we've selected something new, we need to refresh the combo to
        //                    // to remove the grayed out entry
        //                    _dropDownBar.RefreshCombo(NestedComboBoxId, i);
        //                }
        //                else
        //                {
        //                    // changing from one nested to another, just update the selection
        //                    _dropDownBar.SetCurrentSelection(NestedComboBoxId, i);
        //                }

        //                found = true;
        //                break;
        //            }
        //        }
        //    }

        //    if (!found)
        //    {
        //        // there's no associated entry, we should disable the bar
        //        _curNestedIndex = -1;

        //        // we need to refresh to clear the nested combo box since there is no associated nested entry
        //        _dropDownBar.RefreshCombo(NestedComboBoxId, -1);
        //    }
        //}

        private void ForceTopLevelRefresh()
        {
            if (_dropDownBar != null && _topLevelEntries != null)
            {
                for (int i = 0; i < _topLevelEntries.Count; i++)
                {
                    _dropDownBar.RefreshCombo(TopLevelComboBoxId, i);
                }
            }
        }

        private void FindActiveTopLevelComboSelection(int newPosition, ReadOnlyCollection<DropDownEntryInfo> topLevel)
        {
            if (_dropDownBar == null)
            {
                return;
            }

            int oldTopLevel = _curTopLevelIndex;

            // left side has changed
            bool found = false;
            for (int i = 0; i < topLevel.Count; i++)
            {
                if (newPosition >= topLevel[i].Start && newPosition <= topLevel[i].End)
                {
                    _curTopLevelIndex = i;

                    // we found a new left hand side
                    if (oldTopLevel == -1)
                    {
                        // we've selected something new, we need to refresh the combo to
                        // to remove the grayed out entry
                        _dropDownBar.RefreshCombo(TopLevelComboBoxId, i);
                    }
                    else
                    {
                        // changing from one top-level to another, just update the selection
                        _dropDownBar.SetCurrentSelection(TopLevelComboBoxId, i);
                    }

                    // update the nested entries
                    //CalculateNestedEntries();
                    //_dropDownBar.RefreshCombo(NestedComboBoxId, 0);
                    //UpdateNestedComboSelection(newPosition);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // there's no associated entry, we should disable the bar
                _curTopLevelIndex = -1;
                //_curNestedIndex = -1;

                // Commented this out because it was preventing the function list from populating when the document first displayed
                //if (oldTopLevel != -1)
                //{
                    // we need to refresh to clear both combo boxes since there is no associated entry
                    _dropDownBar.RefreshCombo(TopLevelComboBoxId, -1);
                    //_dropDownBar.RefreshCombo(NestedComboBoxId, -1);
                //}
            }

            //UpdateNestedComboSelection(newPosition);
        }

        public int GetComboAttributes(int iCombo, out uint pcEntries, out uint puEntryType, out IntPtr phImageList)
        {
            uint count = 0;

            switch (iCombo)
            {
                case TopLevelComboBoxId:
                    CalculateTopLevelEntries();
                    count = (uint)_topLevelEntries.Count;
                    break;
                //case NestedComboBoxId:
                //    CalculateNestedEntries();
                //    count = (uint)_nestedEntries.Count;
                //    break;
            }

            pcEntries = count;
            puEntryType = (uint)(DROPDOWNENTRYTYPE.ENTRY_TEXT | DROPDOWNENTRYTYPE.ENTRY_IMAGE | DROPDOWNENTRYTYPE.ENTRY_ATTR);
            phImageList = _imageList.Handle;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Wired to parser event for when the parser has completed parsing a new tree and we need
        /// to update the navigation bar with the new data.
        /// </summary>
        private void ParserOnNewParseTree(object sender, EventArgs e)
        {
            var dropDownBar = _dropDownBar;
            if (dropDownBar != null)
            {
                _curNestedIndex = -1;
                _curTopLevelIndex = -1;
                Action callback = () =>
                {
                    CalculateTopLevelEntries();
                    CaretPositionChanged(this, new CaretPositionChangedEventArgs(null, _textView.Caret.Position, _textView.Caret.Position));
                };
                _dispatcher.BeginInvoke(callback, DispatcherPriority.Background);
            }
        }

        private void CalculateTopLevelEntries()
        {
            //bool forceRefresh = false;
            //// intialize the top level entries with a transform from function defs to drop down entries
            //if (_moduleContents != null)
            //{
            //    string bufferFilename = _textView.TextBuffer.GetFilePath();
            //    var list = _moduleContents.FunctionDefinitions.Where(y => y.Value.ContainingFile == bufferFilename).Select(x => new DropDownEntryInfo(x.Value)).ToList();
            //    list.Sort(DropDownEntryInfo.CompareEntryInfo);
            //    forceRefresh = DifferencesExist(_topLevelEntries, list);
            //    _topLevelEntries = new ReadOnlyCollection<DropDownEntryInfo>(list);
            //}
            //return forceRefresh;
            GeneroAst ast = _projectEntry.Analysis;
            if (ast != null)
            {
                _topLevelEntries = CalculateEntries(ast.Body as ModuleNode);
            }
        }

        /// <summary>
        /// Helper function for calculating all of the drop down entries that are available
        /// in the given suite statement.  Called to calculate both the members of top-level
        /// code and class bodies.
        /// </summary>
        private static ReadOnlyCollection<DropDownEntryInfo> CalculateEntries(ModuleNode moduleNode)
        {
            List<DropDownEntryInfo> newEntries = new List<DropDownEntryInfo>();

            if (moduleNode != null)
            {
                foreach (var node in moduleNode.Children)
                {
                    if (node.Value is FunctionBlockNode || node.Value is MainBlockNode)
                    {
                        newEntries.Add(new DropDownEntryInfo(node.Value));
                    }
                }
            }

            newEntries.Sort(ComparisonFunction);
            return new ReadOnlyCollection<DropDownEntryInfo>(newEntries);
        }

        private static int ComparisonFunction(DropDownEntryInfo x, DropDownEntryInfo y)
        {
            return CompletionComparer.UnderscoresLast.Compare(x.Name, y.Name);
        }

        //private void CalculateNestedEntries()
        //{
        //    //var entries = _topLevelEntries;
        //    //int topLevelIndex = _curTopLevelIndex;
        //    //if (entries.Count == 0)
        //    //{
        //     //   _nestedEntries = EmptyEntries;
        //    //}
        //    //else if (topLevelIndex < entries.Count)
        //    //{
        //    //    var info = entries[topLevelIndex == -1 ? 0 : topLevelIndex];

        //    //    ClassDefinition klass = info.Body as ClassDefinition;
        //    //    if (klass != null)
        //    //    {
        //    //        _nestedEntries = CalculateEntries(klass.Body as SuiteStatement);
        //    //    }
        //    //    else
        //    //    {
        //    //        _nestedEntries = EmptyEntries;
        //    //    }
        //    //}
        //}

        public int GetComboTipText(int iCombo, out string pbstrText)
        {
            pbstrText = null;
            return VSConstants.S_OK;
        }

        public int GetEntryAttributes(int iCombo, int iIndex, out uint pAttr)
        {
            pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_PLAIN;

            if (iIndex == 0)
            {
                switch (iCombo)
                {
                    case TopLevelComboBoxId:
                        if (_curTopLevelIndex == -1)
                        {
                            pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_GRAY;
                        }
                        break;
                    //case NestedComboBoxId:
                    //    if (_curNestedIndex == -1)
                    //    {
                    //        pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_GRAY;
                    //    }
                    //    break;
                }
            }

            return VSConstants.S_OK;
        }

        public int GetEntryImage(int iCombo, int iIndex, out int piImageIndex)
        {
            piImageIndex = 0;

            switch (iCombo)
            {
                case TopLevelComboBoxId:
                    var topLevel = _topLevelEntries;
                    if (iIndex < topLevel.Count)
                    {
                        piImageIndex = topLevel[iIndex].ImageListIndex;
                    }
                    break;
                //case NestedComboBoxId:
                //    var nested = _nestedEntries;
                //    if (iIndex < nested.Count)
                //    {
                //        piImageIndex = nested[iIndex].ImageListIndex;
                //    }
                //    break;
            }

            return VSConstants.S_OK;
        }

        public int GetEntryText(int iCombo, int iIndex, out string ppszText)
        {
            ppszText = String.Empty;
            switch (iCombo)
            {
                case TopLevelComboBoxId:
                    var topLevel = _topLevelEntries;
                    if (iIndex < topLevel.Count)
                    {
                        if (VSGeneroPackage.Instance.AdvancedOptions4GLPage.ShowFunctionParametersInList)
                            ppszText = topLevel[iIndex].DescriptiveName;
                        else
                            ppszText = topLevel[iIndex].Name;
                    }
                    break;
            }

            return VSConstants.S_OK;
        }

        public int OnComboGetFocus(int iCombo)
        {
            return VSConstants.S_OK;
        }

        public int OnItemChosen(int iCombo, int iIndex)
        {
            if (_dropDownBar == null)
            {
                return VSConstants.E_UNEXPECTED;
            }
            switch (iCombo)
            {
                case TopLevelComboBoxId:
                    //_dropDownBar.RefreshCombo(NestedComboBoxId, 0);
                    var topLevel = _topLevelEntries;
                    if (iIndex < topLevel.Count)
                    {
                        int oldIndex = _curTopLevelIndex;
                        _curTopLevelIndex = iIndex;
                        if (oldIndex == -1)
                        {
                            _dropDownBar.RefreshCombo(TopLevelComboBoxId, iIndex);
                        }
                        CenterAndFocus(topLevel[iIndex].Start);
                    }
                    break;
                //case NestedComboBoxId:
                //    var nested = _nestedEntries;
                //    if (iIndex < nested.Count)
                //    {
                //        int oldIndex = _curNestedIndex;
                //        _curNestedIndex = iIndex;
                //        if (oldIndex == -1)
                //        {
                //            _dropDownBar.RefreshCombo(NestedComboBoxId, iIndex);
                //        }
                //        CenterAndFocus(nested[iIndex].Start);
                //    }
                //    break;
            }
            return VSConstants.S_OK;
        }

        private void CenterAndFocus(int index)
        {
            _textView.Caret.MoveTo(new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, index));

            _textView.ViewScroller.EnsureSpanVisible(
                new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, index, 1),
                EnsureSpanVisibleOptions.AlwaysCenter
            );

            ((System.Windows.Controls.Control)_textView).Focus();
        }

        public int OnItemSelected(int iCombo, int iIndex)
        {
            return VSConstants.S_OK;
        }

        public int SetDropdownBar(IVsDropdownBar pDropdownBar)
        {
            _dropDownBar = pDropdownBar;
            if (_dropDownBar != null)
            {
                CaretPositionChanged(this, new CaretPositionChangedEventArgs(null, _textView.Caret.Position, _textView.Caret.Position));
            }

            return VSConstants.S_OK;
        }

        internal void Unregister()
        {
            _projectEntry.OnNewParseTree -= ParserOnNewParseTree;
            _textView.Caret.PositionChanged -= CaretPositionChanged;
        }

        public void UpdateView(IWpfTextView textView)
        {
            if (_textView != textView)
            {
                _textView.Caret.PositionChanged -= CaretPositionChanged;
                _textView = textView;
                _textView.Caret.PositionChanged += CaretPositionChanged;
                CaretPositionChanged(this, new CaretPositionChangedEventArgs(null, _textView.Caret.Position, _textView.Caret.Position));
            }
        }

        //private void UpdateFunctionList(object sender, ParseCompleteEventArgs e)
        //{
        //    if (!_synchObj.InvokeRequired)
        //    {
        //        ForceFunctionListUpdate(sender as GeneroFileParserManager);
        //    }
        //    else
        //    {
        //        _synchObj.Invoke(new Action<GeneroFileParserManager>(ForceFunctionListUpdate), new object[] { sender as GeneroFileParserManager });
        //    }
        //}

        //private void ForceFunctionListUpdate(GeneroFileParserManager fpm)
        //{
        //    if (fpm != null)
        //    {
        //        _moduleContents = fpm.ModuleContents;
        //    }
        //    if (CalculateTopLevelEntries())
        //        ForceTopLevelRefresh();
        //    CaretPositionChanged(this, new CaretPositionChangedEventArgs(null, _textView.Caret.Position, _textView.Caret.Position));
        //}

        //public void ForceRefresh()
        //{
        //    ForceFunctionListUpdate(null);
        //}

        /// <summary>
        /// An enum which is synchronized with our image list for the various
        /// kinds of images which are available.  This can be combined with the 
        /// ImageListOverlay to select an image for the appropriate member type
        /// and indicate the appropiate visiblity.  These can be combined with
        /// GetImageListIndex to get the final index.
        /// 
        /// Most of these are unused as we're just using an image list shipped
        /// by the VS SDK.
        /// </summary>
        enum ImageListKind
        {
            Class,          // 0
            Unknown1,       // 1
            Unknown2,       // 2
            Enum,           // 3
            Unknown3,       // 4
            Lightning,      // 5
            Unknown4,       // 6
            BlueBox,        // 7
            Key,            // 8
            BlueStripe,     // 9
            ThreeDashes,    // 10
            TwoBoxes,       // 11
            Method,         // 12
            StaticMethod,   // 13
            Unknown6,       // 14
            Namespace,      // 15
            Unknown7,       // 16
            Property,       // 17
            Unknown8,       // 18
            Unknown9,       // 19
            Report,         // 20
            Unknown11,      // 21
            Unknown12,      // 22
            Unknown13,      // 23
            ClassMethod,    // 24
            Unknown25,      // 25
            Unknown26,      // 26
            Unknown27,      // 27
            Unknown28,      // 28
            Unknown29,      // 29
            Unknown30,      // 30
            Dialog = 200
        }

        /// <summary>
        /// Indicates the overlay kind which should be used for a drop down members
        /// image.  The overlay kind typically indicates visibility.
        /// 
        /// Most of these are unused as we're just using an image list shipped
        /// by the VS SDK.
        /// </summary>
        enum ImageListOverlay
        {
            ImageListOverlayNone,
            ImageListOverlayLetter,
            ImageListOverlayBlue,
            ImageListOverlayKey,
            ImageListOverlayPrivate,
            ImageListOverlayArrow,
        }

        /// <summary>
        /// Turns an image list kind / overlay into the proper index in the image list.
        /// </summary>
        private static int GetImageListIndex(ImageListKind kind, ImageListOverlay overlay)
        {
            if ((int)kind <= 30)
            {
                int groupBase = (int)kind * 6;
                return groupBase + (int)overlay;
            }
            return (int)kind;
        }

        /// <summary>
        /// Reads our image list from our DLLs resource stream.
        /// </summary>
        private static ImageList GetImageList()
        {
            ImageList list = new ImageList();
            list.ImageSize = new Size(0x10, 0x10);
            list.TransparentColor = Color.FromArgb(0xff, 0, 0xff);
            Stream manifestResourceStream = typeof(DropDownBarClient).Assembly.GetManifestResourceStream("VSGenero.Resources.completionset.bmp");
            if (manifestResourceStream == null)
                manifestResourceStream = typeof(DropDownBarClient).Assembly.GetManifestResourceStream("VSGenero.Resources.completionset2013.bmp");
            list.Images.AddStrip(new Bitmap(manifestResourceStream));
            return list;
        }

        internal void UpdateProjectEntry(IProjectEntry newEntry)
        {
            if (newEntry is IGeneroProjectEntry)
            {
                _projectEntry.OnNewParseTree -= ParserOnNewParseTree;
                _projectEntry = (IGeneroProjectEntry)newEntry;
                _projectEntry.OnNewParseTree += ParserOnNewParseTree;
            }
        }

        public void ForceRefresh()
        {
            ForceTopLevelRefresh();
        }

        struct DropDownEntryInfo
        {
            private FunctionBlockNode _funcDef;
            private MainBlockNode _mainDef;

            public static int CompareEntryInfo(DropDownEntryInfo x, DropDownEntryInfo y)
            {
                return string.Compare(x.Name, y.Name);
            }

            public DropDownEntryInfo(AstNode def)
            {
                _start = def.StartIndex;
                _end = def.EndIndex;
                if(def is FunctionBlockNode)
                {
                    _mainDef = null;
                    _funcDef = def as FunctionBlockNode;
                    _name = _funcDef.Name;
                    _descName = _funcDef.DescriptiveName;
                }
                else if(def is MainBlockNode)
                {
                    _funcDef = null;
                    _mainDef = def as MainBlockNode;
                    _name = "main";
                    _descName = "main";
                }
                else
                {
                    _mainDef = null;
                    _funcDef = null;
                    _name = null;
                    _descName = null;
                    throw new ArgumentException("Invalid AstNode for DropDownEntry");
                }
            }

            private string _name;
            public string Name
            {
                get { return _name; }
            }

            private string _descName;
            public string DescriptiveName
            {
                get { return _descName; }
            }

            private int _start;
            public int Start
            {
                get { return _start; }
            }

            private int _end;
            public int End
            {
                get { return _end; }
            }

            /// <summary>
            /// Gets the index in our image list which should be used for the icon to display
            /// next to the drop down element.
            /// </summary>
            public int ImageListIndex
            {
                get
                {
                    ImageListOverlay overlay = ImageListOverlay.ImageListOverlayNone;
                    string name = Name;
                    if (name != null && name.StartsWith("_") && !(name.StartsWith("__") && name.EndsWith("__")))
                    {
                        overlay = ImageListOverlay.ImageListOverlayPrivate;
                    }

                    if (_funcDef != null)
                    {
                        if (_funcDef.AccessModifier == AccessModifier.Private)
                        {
                            overlay = ImageListOverlay.ImageListOverlayPrivate;
                        }

                        return GetImageListIndex(GetImageListKind(_funcDef), overlay);
                    }
                    else
                    {
                        return GetImageListIndex(GetImageListKind(_mainDef), overlay);
                    }
                }
            }

            private static ImageListKind GetImageListKind(AstNode def)
            {
                if (def is ReportBlockNode)
                    return ImageListKind.Report;
                else if (def is DeclarativeDialogBlock)
                    return ImageListKind.Dialog;
                else
                    return ImageListKind.Method;
            }
        }
    }
}
