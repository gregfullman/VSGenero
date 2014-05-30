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
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools;

namespace Microsoft.VisualStudio.VSCommon
{
    public enum VSDialogButton
    {
        AbortRetryIgnore,
        Ok,
        OkCancel,
        RetryCancel,
        YesAllNoCancel,
        YesNo,
        YesNoCancel
    }

    public enum VSDialogDefaultButton
    {
        First,
        Second,
        Third,
        Fourth
    }

    public enum VSDialogIconMode
    {
        Critical,
        Info,
        NoIcon,
        Query,
        Warning
    }

    public enum VSDialogResult
    {
        Abort,
        Cancel,
        Ignore,
        No,
        Ok,
        Retry,
        Yes
    }

    public class TextBufferOpenStatus
    {
        public ITextBuffer Buffer { get; set; }
        public bool IsOpen { get; set; }
    }

    public abstract class VSCommonPackage : CommonPackage, IVsInstalledProduct, IOleComponent
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

        public void RegisterCommands(IEnumerable<CommonCommand> commands, Guid cmdSet)
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
            ShowDialog(caption, message, VSDialogButton.Ok, VSDialogDefaultButton.First, VSDialogIconMode.Info);
        }

        public VSDialogResult ShowDialog(string caption, string message, VSDialogButton button, VSDialogDefaultButton defaultButton, VSDialogIconMode iconMode)
        {
            OLEMSGBUTTON buttonToUse = OLEMSGBUTTON.OLEMSGBUTTON_OK;
            switch (button)
            {
                case VSDialogButton.AbortRetryIgnore: buttonToUse = OLEMSGBUTTON.OLEMSGBUTTON_ABORTRETRYIGNORE; break;
                case VSDialogButton.Ok: buttonToUse = OLEMSGBUTTON.OLEMSGBUTTON_OK; break;
                case VSDialogButton.OkCancel: buttonToUse = OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL; break;
                case VSDialogButton.RetryCancel: buttonToUse = OLEMSGBUTTON.OLEMSGBUTTON_RETRYCANCEL; break;
                case VSDialogButton.YesAllNoCancel: buttonToUse = OLEMSGBUTTON.OLEMSGBUTTON_YESALLNOCANCEL; break;
                case VSDialogButton.YesNo: buttonToUse = OLEMSGBUTTON.OLEMSGBUTTON_YESNO; break;
                case VSDialogButton.YesNoCancel: buttonToUse = OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL; break;
            }

            OLEMSGDEFBUTTON defaultButtonToUse = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
            switch (defaultButton)
            {
                case VSDialogDefaultButton.First: defaultButtonToUse = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST; break;
                case VSDialogDefaultButton.Second: defaultButtonToUse = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND; break;
                case VSDialogDefaultButton.Third: defaultButtonToUse = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_THIRD; break;
                case VSDialogDefaultButton.Fourth: defaultButtonToUse = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FOURTH; break;
            }

            OLEMSGICON msgIconToUse = OLEMSGICON.OLEMSGICON_INFO;
            switch (iconMode)
            {
                case VSDialogIconMode.Critical: msgIconToUse = OLEMSGICON.OLEMSGICON_CRITICAL; break;
                case VSDialogIconMode.Info: msgIconToUse = OLEMSGICON.OLEMSGICON_INFO; break;
                case VSDialogIconMode.NoIcon: msgIconToUse = OLEMSGICON.OLEMSGICON_NOICON; break;
                case VSDialogIconMode.Query: msgIconToUse = OLEMSGICON.OLEMSGICON_QUERY; break;
                case VSDialogIconMode.Warning: msgIconToUse = OLEMSGICON.OLEMSGICON_WARNING; break;
            }

            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(UIShell.ShowMessageBox(
                       0,
                       ref clsid,
                       caption,
                       message,
                       string.Empty,
                       0,
                       buttonToUse,
                       defaultButtonToUse,
                       msgIconToUse,
                       0,        // false
                       out result));

            switch (result)
            {
                case NativeMethods.IDABORT: return VSDialogResult.Abort;
                case NativeMethods.IDCANCEL: return VSDialogResult.Cancel;
                case NativeMethods.IDIGNORE: return VSDialogResult.Ignore;
                case NativeMethods.IDNO: return VSDialogResult.No;
                case NativeMethods.IDOK: return VSDialogResult.Ok;
                case NativeMethods.IDRETRY: return VSDialogResult.Retry;
                case NativeMethods.IDYES: return VSDialogResult.Yes;
                default: return VSDialogResult.Ok;
            }
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

        private static Dictionary<string, TextBufferOpenStatus> _bufferDictionary = new Dictionary<string, TextBufferOpenStatus>();
        public static Dictionary<string, TextBufferOpenStatus> BufferDictionary
        {
            get
            {
                if (_bufferDictionary == null)
                    _bufferDictionary = new Dictionary<string, TextBufferOpenStatus>();
                return _bufferDictionary;
            }
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
                var textBuffer = adapter.GetDocumentBuffer(lines);
                if (!BufferDictionary.ContainsKey(filename))
                    BufferDictionary.Add(filename, new TextBufferOpenStatus { Buffer = textBuffer, IsOpen = true });
                else
                    BufferDictionary[filename].IsOpen = true;
                return textBuffer;
            }
            else
            {
                TextBufferOpenStatus bufferOpen;
                if (!BufferDictionary.TryGetValue(filename, out bufferOpen))
                {
                    ITextDocumentFactoryService documentFactory = ComponentModel.GetService<ITextDocumentFactoryService>();
                    IContentTypeRegistryService contentRegistry = ComponentModel.GetService<IContentTypeRegistryService>();
                    ITextDocument textDoc = documentFactory.CreateAndLoadTextDocument(filename, contentRegistry.GetContentType(contentType));
                    if (!BufferDictionary.ContainsKey(filename))
                        BufferDictionary.Add(filename, new TextBufferOpenStatus { Buffer = textDoc.TextBuffer, IsOpen = false });
                    return textDoc.TextBuffer;
                }
                return bufferOpen.Buffer;
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
