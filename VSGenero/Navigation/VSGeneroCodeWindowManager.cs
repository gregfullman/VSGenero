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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

using VSGenero.EditorExtensions;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VSCommon;
using System.ComponentModel.Composition;

namespace VSGenero.Navigation
{
    internal class VSGeneroCodeWindowManager : IVsCodeWindowManager, IVsCodeWindowEvents
    {
        private readonly IVsCodeWindow _window;
        private readonly ITextBuffer _textBuffer;
        private static readonly HashSet<VSGeneroCodeWindowManager> _windows = new HashSet<VSGeneroCodeWindowManager>();
        private uint _cookieVsCodeWindowEvents;
        private DropDownBarClient _client;
        private static IVsEditorAdaptersFactoryService _vsEditorAdaptersFactoryService = null;

        public VSGeneroCodeWindowManager(IVsCodeWindow codeWindow, IWpfTextView textView)
        {
            _window = codeWindow;
            _textBuffer = textView.TextBuffer;
            if (_textBuffer.ContentType.TypeName == VSGeneroConstants.ContentType4GL)
            {
                string filename = _textBuffer.GetFilePath();
                if (!VSGeneroPackage.BufferDictionary.ContainsKey(filename))
                    VSGeneroPackage.BufferDictionary.Add(filename, _textBuffer);
            }
            VSGeneroPackage.Instance.OnIdle += OnIdle;
        }

        public int AddAdornments()
        {
            _windows.Add(this);

            IVsTextView textView;

            if (ErrorHandler.Succeeded(_window.GetPrimaryView(out textView)))
            {
                ((IVsCodeWindowEvents)this).OnNewView(textView);
            }

            if (ErrorHandler.Succeeded(_window.GetSecondaryView(out textView)))
            {
                ((IVsCodeWindowEvents)this).OnNewView(textView);
            }

            return AddDropDownBar();
        }

        private int AddDropDownBar()
        {
            var cpc = (IConnectionPointContainer)_window;
            if (cpc != null)
            {
                IConnectionPoint cp;
                cpc.FindConnectionPoint(typeof(IVsCodeWindowEvents).GUID, out cp);
                if (cp != null)
                {
                    cp.Advise(this, out _cookieVsCodeWindowEvents);
                }
            }

            //var pythonProjectEntry = _textBuffer.GetAnalysis() as IPythonProjectEntry;
            //if (pythonProjectEntry == null)
            //{
            //    return VSConstants.E_FAIL;
            //}

            IWpfTextView wpfTextView = null;
            IVsTextView vsTextView;
            if (ErrorHandler.Succeeded(_window.GetLastActiveView(out vsTextView)) && vsTextView != null)
            {
                wpfTextView = VsEditorAdaptersFactoryService.GetWpfTextView(vsTextView);
            }
            if (wpfTextView == null)
            {
                return VSConstants.E_FAIL;
            }
            
            // pass on the text view
            GeneroFileParserManager fpm = VSGeneroPackage.Instance.UpdateBufferFileParserManager(_textBuffer);
            _client = new DropDownBarClient(wpfTextView);

            IVsDropdownBarManager manager = (IVsDropdownBarManager)_window;

            int instancesOpen = 0;
            if (!_textBuffer.Properties.TryGetProperty<int>("InstancesOpen", out instancesOpen))
            {
                _textBuffer.Properties.AddProperty("InstancesOpen", 1);
            }
            else
            {
                _textBuffer.Properties["InstancesOpen"] = instancesOpen + 1;
            }

            IVsDropdownBar dropDownBar;
            int hr = manager.GetDropdownBar(out dropDownBar);
            if (ErrorHandler.Succeeded(hr) && dropDownBar != null)
            {
                hr = manager.RemoveDropdownBar();
                if (!ErrorHandler.Succeeded(hr))
                {
                    return hr;
                }
            }

            int res = manager.AddDropdownBar(2, _client);
            if (ErrorHandler.Succeeded(res))
            {
                // A buffer may have multiple DropDownBarClients, given one may open multiple CodeWindows
                // over a single buffer using Window/New Window
                List<DropDownBarClient> listDropDownBarClient;
                if (!_textBuffer.Properties.TryGetProperty(typeof(DropDownBarClient), out listDropDownBarClient) || listDropDownBarClient == null)
                {
                    listDropDownBarClient = new List<DropDownBarClient>();
                    _textBuffer.Properties[typeof(DropDownBarClient)] = listDropDownBarClient;
                }
                listDropDownBarClient.Add(_client);
            }
            return res;
        }

        private static IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService
        {
            get
            {
                if (_vsEditorAdaptersFactoryService == null)
                {
                    _vsEditorAdaptersFactoryService = VSGeneroPackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
                }
                return _vsEditorAdaptersFactoryService;
            }
        }

        int IVsCodeWindowEvents.OnNewView(IVsTextView vsTextView)
        {
            var wpfTextView = VsEditorAdaptersFactoryService.GetWpfTextView(vsTextView);
            if (wpfTextView != null)
            {
                var factory = VSGeneroPackage.ComponentModel.GetService<IEditorOperationsFactoryService>();
                var editFilter = new EditFilter(wpfTextView, factory.GetEditorOperations(wpfTextView));
                editFilter.AttachKeyboardFilter(vsTextView);
//#if DEV11_OR_LATER
//                new TextViewFilter(vsTextView);
//#endif
                wpfTextView.GotAggregateFocus += OnTextViewGotAggregateFocus;
            }
            return VSConstants.S_OK;
        }

        public int RemoveAdornments()
        {
            _windows.Remove(this);

            IVsTextView textView;

            if (ErrorHandler.Succeeded(_window.GetPrimaryView(out textView)))
            {
                ((IVsCodeWindowEvents)this).OnCloseView(textView);
            }

            if (ErrorHandler.Succeeded(_window.GetSecondaryView(out textView)))
            {
                ((IVsCodeWindowEvents)this).OnCloseView(textView);
            }

            return RemoveDropDownBar();
        }

        private int RemoveDropDownBar()
        {
            var cpc = (IConnectionPointContainer)_window;
            if (cpc != null)
            {
                IConnectionPoint cp;
                cpc.FindConnectionPoint(typeof(IVsCodeWindowEvents).GUID, out cp);
                if (cp != null)
                {
                    cp.Unadvise(_cookieVsCodeWindowEvents);
                }
            }

            if (_client != null)
            {
                IVsDropdownBarManager manager = (IVsDropdownBarManager)_window;
                GeneroFileParserManager fpm;
                if (_textBuffer.Properties.TryGetProperty(typeof(GeneroFileParserManager), out fpm))
                {
                    fpm.CancelParsing();
                }
                _client.Unregister();
                // A buffer may have multiple DropDownBarClients, given one may open multiple CodeWindows
                // over a single buffer using Window/New Window
                List<DropDownBarClient> listDropDownBarClient;
                if (_textBuffer.Properties.TryGetProperty(typeof(DropDownBarClient), out listDropDownBarClient) && listDropDownBarClient != null)
                {
                    listDropDownBarClient.Remove(_client);
                    if (listDropDownBarClient.Count == 0)
                    {
                        _textBuffer.Properties.RemoveProperty(typeof(DropDownBarClient));
                    }

                    int instancesOpen = 0;
                    if (_textBuffer.Properties.TryGetProperty<int>("InstancesOpen", out instancesOpen))
                    {
                        instancesOpen--;
                        _textBuffer.Properties["InstancesOpen"] = instancesOpen;
                    }

                    if (instancesOpen == 0)
                    {
                        if (_textBuffer.Properties.ContainsProperty(typeof(GeneroFileParserManager)))
                        {
                            _textBuffer.Properties.RemoveProperty(typeof(GeneroFileParserManager));
                        }
                    }
                }

                _client = null;
                return manager.RemoveDropdownBar();
            }
            return VSConstants.S_OK;
        }

        public int OnCloseView(IVsTextView pView)
        {
            var wpfTextView = VsEditorAdaptersFactoryService.GetWpfTextView(pView);
            if (wpfTextView != null)
            {
                wpfTextView.GotAggregateFocus -= OnTextViewGotAggregateFocus;
            }
            return VSConstants.S_OK;
        }

        private void OnTextViewGotAggregateFocus(object sender, EventArgs e)
        {
            var wpfTextView = sender as IWpfTextView;
            if (wpfTextView != null)
            {
                if (_client != null)
                {
                    _client.UpdateView(wpfTextView);
                }
            }
        }

        int IVsCodeWindowManager.OnNewView(IVsTextView pView)
        {
            // NO-OP We use IVsCodeWindowEvents to track text view lifetime
            return VSConstants.S_OK;
        }

        private static void OnIdle(object sender, ComponentManagerEventArgs e)
        {
            foreach (var window in _windows)
            {
                if (e.ComponentManager.FContinueIdle() == 0)
                {
                    break;
                }

                IVsTextView vsTextView;
                if (ErrorHandler.Succeeded(window._window.GetLastActiveView(out vsTextView)) && vsTextView != null)
                {
                    var wpfTextView = VsEditorAdaptersFactoryService.GetWpfTextView(vsTextView);
                    if (wpfTextView != null)
                    {
                        EditFilter editFilter;
                        if (wpfTextView.Properties.TryGetProperty(typeof(EditFilter), out editFilter) && editFilter != null)
                        {
                            editFilter.DoIdle(e.ComponentManager);
                        }
                    }
                }
            }
        }
    }
}
