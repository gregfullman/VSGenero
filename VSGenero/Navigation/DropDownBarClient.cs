/* ****************************************************************************
 * 
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
                if (newPosition < topLevel[curTopLevel].Start || newPosition > topLevel[curTopLevel].End)
                {
                    FindActiveTopLevelComboSelection(newPosition, topLevel);
                }
            }
            else
            {
                FindActiveTopLevelComboSelection(newPosition, topLevel);
            }
        }

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
                if (topLevel[i].ProjectEntry == _projectEntry && 
                    newPosition >= topLevel[i].Start && 
                    newPosition <= topLevel[i].End)
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

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // there's no associated entry, we should disable the bar
                _curTopLevelIndex = -1;
                _dropDownBar.RefreshCombo(TopLevelComboBoxId, -1);
            }
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
            _topLevelEntries = CalculateEntries();
        }

        /// <summary>
        /// Helper function for calculating all of the drop down entries that are available
        /// in the given suite statement.  Called to calculate both the members of top-level
        /// code and class bodies.
        /// </summary>
        private ReadOnlyCollection<DropDownEntryInfo> CalculateEntries()
        {
            List<DropDownEntryInfo> newEntries = new List<DropDownEntryInfo>();

            if (_projectEntry.Analysis != null &&
                _projectEntry.Analysis.Body is ModuleNode)
            {

                var moduleNode = _projectEntry.Analysis.Body as ModuleNode;
                moduleNode.SetNamespace(null);
                foreach (var node in moduleNode.Children)
                {
                    if (node.Value is FunctionBlockNode)
                    {
                        newEntries.Add(new DropDownEntryInfo(node.Value as FunctionBlockNode, _projectEntry));
                    }
                }

                // go through the other project entries in the ast.Project, and get those functions too
                if (_projectEntry.ParentProject != null)
                {
                    foreach (var otherEntry in _projectEntry.ParentProject.ProjectEntries.Values.Where(x => x != _projectEntry))
                    {
                        if (!otherEntry.IsAnalyzed)
                        {
                            // sign up for analysis complete event?
                        }
                        else if (otherEntry.Analysis != null &&
                                otherEntry.Analysis.Body is ModuleNode)
                        {
                            otherEntry.Analysis.Body.SetNamespace(null);
                            var modNode = otherEntry.Analysis.Body as ModuleNode;
                            foreach (var node in modNode.Children)
                            {
                                if (node.Value is FunctionBlockNode)
                                {
                                    newEntries.Add(new DropDownEntryInfo(node.Value as FunctionBlockNode, otherEntry));
                                }
                            }
                        }
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

        public int GetComboTipText(int iCombo, out string pbstrText)
        {
            pbstrText = null;
            return VSConstants.S_OK;
        }

        public int GetEntryAttributes(int iCombo, int iIndex, out uint pAttr)
        {
            if (_topLevelEntries[iIndex].ProjectEntry != _projectEntry)
            {
                pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_GRAY;
            }
            else
            {
                pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_PLAIN;
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
                    var topLevel = _topLevelEntries;
                    if (iIndex < topLevel.Count)
                    {
                        int oldIndex = _curTopLevelIndex;
                        _curTopLevelIndex = iIndex;
                        if (oldIndex == -1)
                        {
                            _dropDownBar.RefreshCombo(TopLevelComboBoxId, iIndex);
                        }
                        CenterAndFocus(topLevel[iIndex]);
                    }
                    break;
            }
            return VSConstants.S_OK;
        }

        private void CenterAndFocus(DropDownEntryInfo entry)
        {
            if (entry.ProjectEntry == _projectEntry)
            {
                _textView.Caret.MoveTo(new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, entry.Start));

                _textView.ViewScroller.EnsureSpanVisible(
                    new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, entry.Start, 1),
                    EnsureSpanVisibleOptions.AlwaysCenter
                );

                ((System.Windows.Controls.Control)_textView).Focus();
            }
            else if(entry.FunctionDefinition.Location != null)
            {
                entry.FunctionDefinition.Location.GotoSource();
            }
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

        class DropDownEntryInfo
        {
            private readonly FunctionBlockNode _funcDef;
            private readonly IProjectEntry _containingEntry;

            public static int CompareEntryInfo(DropDownEntryInfo x, DropDownEntryInfo y)
            {
                return string.Compare(x.Name, y.Name);
            }

            public DropDownEntryInfo(FunctionBlockNode funcDef, IProjectEntry projEntry)
            {
                _containingEntry = projEntry;
                _start = funcDef.StartIndex;
                _end = funcDef.EndIndex;
                _funcDef = funcDef;
                if (_funcDef is MainBlockNode)
                {
                    _name = "main";
                    _descName = "main";
                }
                else
                {
                    _name = _funcDef.Name;
                    _descName = _funcDef.DescriptiveName;
                }
            }

            public IProjectEntry ProjectEntry
            {
                get { return _containingEntry; }
            }

            public FunctionBlockNode FunctionDefinition
            {
                get { return _funcDef; }
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

                    if (_funcDef.AccessModifier == AccessModifier.Private)
                    {
                        overlay = ImageListOverlay.ImageListOverlayPrivate;
                    }

                    return GetImageListIndex(GetImageListKind(_funcDef), overlay);
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
