/* ****************************************************************************
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
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.Runtime.InteropServices;
using EnvDTE80;
using EnvDTE;

namespace Microsoft.VisualStudio.VSCommon
{
    public abstract class VSCommonPackage : Package, IVsInstalledProduct, IOleComponent
    {
        protected static VSCommonPackage Instance;
        private uint _componentID;
        private static RunningDocumentTable _documentTable;
        private static IVsRunningDocumentTable _vsDocumentTable;
        private IOleComponentManager _compMgr;
        private DTEEvents _packageDTEEvents = null;

        protected VSCommonPackage()
        {
            if (_documentTable == null)
                _documentTable = new RunningDocumentTable(ServiceProvider.GlobalProvider);
        }

        protected override void Initialize()
        {
            // From PythonTools CommonPackage
            var componentManager = _compMgr = (IOleComponentManager)GetService(typeof(SOleComponentManager));
            OLECRINFO[] crinfo = new OLECRINFO[1];
            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
            crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime;
            crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
            crinfo[0].uIdleTimeInterval = 0;
            ErrorHandler.ThrowOnFailure(componentManager.FRegisterComponent(this, crinfo, out _componentID));

            base.Initialize();
            Instance = this;

            _packageDTEEvents = ApplicationObject.Events.DTEEvents;
            _packageDTEEvents.OnBeginShutdown += OnBeginShutdown;
        }

        protected virtual void OnBeginShutdown()
        {
        }

        private static readonly object _commandsLock = new object();
        private static readonly Dictionary<CommonCommand, MenuCommand> _commands = new Dictionary<CommonCommand, MenuCommand>();
        protected static Dictionary<CommonCommand, MenuCommand> Commands
        {
            get { return _commands; }
        }

        protected void RegisterCommands(IEnumerable<CommonCommand> commands, Guid cmdSet)
        {
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                lock (_commandsLock)
                {
                    foreach (var command in commands)
                    {
                        var beforeQueryStatus = command.BeforeQueryStatus;
                        CommandID toolwndCommandID = new CommandID(cmdSet, command.CommandId);
                        if (beforeQueryStatus == null)
                        {
                            MenuCommand menuToolWin = new MenuCommand(command.DoCommand, toolwndCommandID);
                            mcs.AddCommand(menuToolWin);
                            _commands[command] = menuToolWin;
                        }
                        else
                        {
                            OleMenuCommand menuToolWin = new OleMenuCommand(command.DoCommand, toolwndCommandID);
                            menuToolWin.BeforeQueryStatus += beforeQueryStatus;
                            if (command is ComboBoxCommand)
                            {
                                var cbCommand = (command as ComboBoxCommand);
                                if (cbCommand.ParameterDescription != null)
                                    menuToolWin.ParametersDescription = cbCommand.ParameterDescription;
                                CommandID getListCommandId = new CommandID(cmdSet, cbCommand.GetListCommandId);
                                OleMenuCommand getListCmd = new OleMenuCommand(cbCommand.DoGetListCommand, getListCommandId);
                                mcs.AddCommand(getListCmd);
                            }

                            mcs.AddCommand(menuToolWin);
                            _commands[command] = menuToolWin;
                        }
                    }
                }
            }
        }

        // Shows the standard Visual Studio dialog
        public void ShowDialog(string caption, string message)
        {
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(UIShell.ShowMessageBox(
                       0,
                       ref clsid,
                       caption,
                       message,
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }

        public IVsUIShell UIShell
        {
            get { return (IVsUIShell)GetService(typeof(SVsUIShell)); }
        }

        /// <summary>
        /// Gets the current IWpfTextView that is the active document.
        /// </summary>
        /// <returns></returns>
        public static IWpfTextView GetActiveTextView()
        {
            var monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            if (monitorSelection == null)
            {
                return null;
            }
            object curDocument;
            if (ErrorHandler.Failed(monitorSelection.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out curDocument)))
            {
                // TODO: Report error
                return null;
            }

            IVsWindowFrame frame = curDocument as IVsWindowFrame;
            if (frame == null)
            {
                // TODO: Report error
                return null;
            }

            object docView = null;
            if (ErrorHandler.Failed(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView)))
            {
                // TODO: Report error
                return null;
            }

            if (docView is IVsCodeWindow)
            {
                IVsTextView textView;
                if (ErrorHandler.Failed(((IVsCodeWindow)docView).GetPrimaryView(out textView)))
                {
                    // TODO: Report error
                    return null;
                }

                var model = (IComponentModel)GetGlobalService(typeof(SComponentModel));
                var adapterFactory = model.GetService<IVsEditorAdaptersFactoryService>();
                var wpfTextView = adapterFactory.GetWpfTextView(textView);
                return wpfTextView;
            }
            return null;
        }

        private static void OpenDocument(string filename, out IVsTextView viewAdapter, out IVsWindowFrame pWindowFrame)
        {
            IVsTextManager textMgr = (IVsTextManager)Instance.GetService(typeof(SVsTextManager));

            IVsUIShellOpenDocument uiShellOpenDocument = Instance.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            IVsUIHierarchy hierarchy;
            uint itemid;


            VsShellUtilities.OpenDocument(
                Instance,
                filename,
                Guid.Empty,
                out hierarchy,
                out itemid,
                out pWindowFrame,
                out viewAdapter);
        }

        public static IComponentModel ComponentModel
        {
            get
            {
                return (IComponentModel)GetGlobalService(typeof(SComponentModel));
            }
        }

        public static void NavigateTo(string filename, Guid docViewGuidType, int line, int col)
        {
            IVsTextView viewAdapter;
            IVsWindowFrame pWindowFrame;
            OpenDocument(filename, out viewAdapter, out pWindowFrame);

            ErrorHandler.ThrowOnFailure(pWindowFrame.Show());

            // Set the cursor at the beginning of the declaration.            
            ErrorHandler.ThrowOnFailure(viewAdapter.SetCaretPos(line, col));
            // Make sure that the text is visible.
            viewAdapter.CenterLines(line, 1);
        }

        public static void NavigateTo(string filename, Guid docViewGuidType, int pos)
        {
            IVsTextView viewAdapter;
            IVsWindowFrame pWindowFrame;
            OpenDocument(filename, out viewAdapter, out pWindowFrame);

            ErrorHandler.ThrowOnFailure(pWindowFrame.Show());

            // Set the cursor at the beginning of the declaration.          
            int line, col;
            ErrorHandler.ThrowOnFailure(viewAdapter.GetLineAndColumn(pos, out line, out col));
            ErrorHandler.ThrowOnFailure(viewAdapter.SetCaretPos(line, col));
            // Make sure that the text is visible.
            viewAdapter.CenterLines(line, 1);
        }

        public static ITextBuffer GetBufferForDocument(string filename, bool openDocument, string contentType)
        {
            if (openDocument)
            {

                IVsTextView viewAdapter;
                IVsWindowFrame frame;
                OpenDocument(filename, out viewAdapter, out frame);

                IVsTextLines lines;
                ErrorHandler.ThrowOnFailure(viewAdapter.GetBuffer(out lines));

                var adapter = ComponentModel.GetService<IVsEditorAdaptersFactoryService>();

                return adapter.GetDocumentBuffer(lines);
            }
            else
            {
                try
                {
                    //    IVsRunningDocumentTable rdt = Package.GetGlobalService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
                    //    if (rdt != null)
                    //    {
                    //        IEnumRunningDocuments documents;
                    //        rdt.GetRunningDocumentsEnum(out documents);
                    //        IntPtr documentData = IntPtr.Zero;
                    //        uint[] docCookie1 = new uint[1];
                    //        uint fetched;
                    //        while ((VSConstants.S_OK == documents.Next(1, docCookie1, out fetched)) && (1 == fetched))
                    //        {
                    //            uint flags;
                    //            uint editLocks;
                    //            uint readLocks;
                    //            string moniker;
                    //            IVsHierarchy docHierarchy;
                    //            uint docId;
                    //            IntPtr docData = IntPtr.Zero;
                    //            try
                    //            {
                    //                ErrorHandler.ThrowOnFailure(
                    //                    rdt.GetDocumentInfo(docCookie1[0], out flags, out readLocks, out editLocks, out moniker, out docHierarchy, out docId, out docData));
                    //                // Check if this document is the one we are looking for.
                    //                if (moniker == filename)
                    //                {
                    //                    documentData = docData;
                    //                    docData = IntPtr.Zero;
                    //                    break;
                    //                }
                    //            }
                    //            finally
                    //            {
                    //                if (IntPtr.Zero != docData)
                    //                {
                    //                    Marshal.Release(docData);
                    //                }
                    //            }
                    //        }
                    //    }

                    // TODO: occasionally this takes a LONG time to run...usually when several tool window panes are open in the main view.
                    // But not always...
                    uint docCookie;
                    var res = _documentTable.FindDocument(filename, out docCookie);

                    //IVsHierarchy pIVsHierarchy;
                    //uint itemId;
                    //IntPtr docData;
                    //uint uiVsDocCookie;
                    //var res = _vsDocumentTable.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, filename, out pIVsHierarchy, out itemId, out docData, out uiVsDocCookie);

                    if (res == null && docCookie == 0)
                    {
                        ITextDocumentFactoryService documentFactory = ComponentModel.GetService<ITextDocumentFactoryService>();
                        IContentTypeRegistryService contentRegistry = ComponentModel.GetService<IContentTypeRegistryService>();
                        ITextDocument textDoc = documentFactory.CreateAndLoadTextDocument(filename, contentRegistry.GetContentType(contentType));
                        return textDoc.TextBuffer;
                    }
                    else
                    {
                        var info = _documentTable.GetDocumentInfo(docCookie);
                        var obj = info.DocData;
                        var vsTextLines = obj as IVsTextLines;
                        var vsTextBufferProvider = obj as IVsTextBufferProvider;

                        ITextBuffer textBuffer;
                        IVsEditorAdaptersFactoryService editorAdapter = ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
                        if (vsTextLines != null)
                        {
                            textBuffer = editorAdapter.GetDataBuffer(vsTextLines);
                        }
                        else if (vsTextBufferProvider != null
                            && ErrorHandler.Succeeded(vsTextBufferProvider.GetTextBuffer(out vsTextLines))
                            && vsTextLines != null)
                        {
                            textBuffer = editorAdapter.GetDataBuffer(vsTextLines);
                        }
                        else
                        {
                            textBuffer = null;
                        }
                        return textBuffer;
                    }
                }
                catch (Exception e)
                {
                    ITextDocumentFactoryService documentFactory = ComponentModel.GetService<ITextDocumentFactoryService>();
                    IContentTypeRegistryService contentRegistry = ComponentModel.GetService<IContentTypeRegistryService>();
                    ITextDocument textDoc = documentFactory.CreateAndLoadTextDocument(filename, contentRegistry.GetContentType(contentType));
                    return textDoc.TextBuffer;
                }
            }
        }

        public virtual int IdBmpSplash(out uint pIdBmp)
        {
            pIdBmp = 300;
            return VSConstants.S_OK;
        }

        public virtual int IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = 400;
            return VSConstants.S_OK;
        }

        public virtual int OfficialName(out string pbstrName)
        {
            pbstrName = GetResourceString("@ProductName");
            return VSConstants.S_OK;
        }

        public virtual int ProductDetails(out string pbstrProductDetails)
        {
            pbstrProductDetails = GetResourceString("@ProductDetails");
            return VSConstants.S_OK;
        }

        public virtual int ProductID(out string pbstrPID)
        {
            pbstrPID = GetResourceString("@ProductID");
            return VSConstants.S_OK;
        }

        protected string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = this.GetType().GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        public event EventHandler<ComponentManagerEventArgs> OnIdle;

        public int FDoIdle(uint grfidlef)
        {
            var onIdle = OnIdle;
            if (onIdle != null)
            {
                onIdle(this, new ComponentManagerEventArgs(_compMgr));
            }

            return 0;
        }

        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
        {
        }

        public virtual void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

        public void OnEnterState(uint uStateID, int fEnter)
        {
        }

        public void OnLoseActivation()
        {
        }

        public void Terminate()
        {
        }

        private DTE2 _applicationObject = null;
        public DTE2 ApplicationObject
        {
            get
            {
                if (_applicationObject == null)
                {
                    // Get an instance of the currently running Visual Studio IDE
                    DTE dte = (DTE)GetService(typeof(DTE));
                    _applicationObject = dte as DTE2;
                }
                return _applicationObject;
            }
        }
    }

    public class ComponentManagerEventArgs : EventArgs
    {
        private readonly IOleComponentManager _compMgr;

        public ComponentManagerEventArgs(IOleComponentManager compMgr)
        {
            _compMgr = compMgr;
        }

        public IOleComponentManager ComponentManager
        {
            get
            {
                return _compMgr;
            }
        }
    }
}
