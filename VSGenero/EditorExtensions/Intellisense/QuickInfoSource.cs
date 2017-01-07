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

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing.AST_4GL;
using Microsoft.VisualStudio.VSCommon;
using EnvDTE;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio;
using VSGenero.Analysis.Interfaces;
using VSGenero.Analysis.Parsing;

namespace VSGenero.EditorExtensions.Intellisense
{
    internal class QuickInfoSource : IQuickInfoSource, ITypeResolver
    {
        private readonly ITextBuffer _textBuffer;
        private readonly IVsTextView _viewAdapter;
        private readonly QuickInfoSourceProvider _provider;
        private IQuickInfoSession _curSession;

        public QuickInfoSource(QuickInfoSourceProvider provider, ITextBuffer textBuffer)
        {
            var adapterFactory = VSGeneroPackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
            _textBuffer = textBuffer;
            IWpfTextView wpfTextView;
            if (textBuffer.Properties.TryGetProperty<IWpfTextView>(typeof(IWpfTextView), out wpfTextView))
            {
                _viewAdapter = adapterFactory.GetViewAdapter(wpfTextView);
            }
            _provider = provider;
        }

        #region IQuickInfoSource Members

        public void AugmentQuickInfoSession(IQuickInfoSession session, System.Collections.Generic.IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            if (_curSession != null && !_curSession.IsDismissed)
            {
                _curSession.Dismiss();
                _curSession = null;
            }

            _curSession = session;
            _curSession.Dismissed += CurSessionDismissed;
            if (_provider._PublicFunctionProvider != null)
                _provider._PublicFunctionProvider.SetFilename(_textBuffer.GetFilePath());
            if (_provider._DatabaseInfoProvider != null)
                _provider._DatabaseInfoProvider.SetFilename(_textBuffer.GetFilePath());
            var vars = _textBuffer.CurrentSnapshot.AnalyzeExpression(
                session.CreateTrackingSpan(_textBuffer),
                false,
                _provider._PublicFunctionProvider,
                _provider._DatabaseInfoProvider,
                _provider._ProgramFileProvider
            );

            AugmentQuickInfoWorker(_provider, session, _textBuffer, _viewAdapter, vars, quickInfoContent, out applicableToSpan);
        }

        private void CurSessionDismissed(object sender, EventArgs e)
        {
            _curSession = null;
        }

        internal static void AugmentQuickInfoWorker(QuickInfoSourceProvider provider, IQuickInfoSession session, ITextBuffer subjectBuffer, IVsTextView viewAdapter, ExpressionAnalysis exprAnalysis, System.Collections.Generic.IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = exprAnalysis.Span;
            if (applicableToSpan == null || String.IsNullOrWhiteSpace(exprAnalysis.Expression))
            {
                return;
            }

            bool first = true;
            var result = new StringBuilder();
            int count = 0;
            IAnalysisResult val = exprAnalysis.Value;
            if(val != null)
            {
                DTE dte = (DTE)VSGeneroPackage.Instance.GetPackageService(typeof(DTE));
                if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode ||
                    !val.CanGetValueFromDebugger)
                {
                    quickInfoContent.Add(val.Documentation);
                }
                else
                {
                    //if(provider != null && provider._GeneroDebugger != null)
                    //{
                    //    provider._GeneroDebugger.SetDataTipContext(val.GetVariableType());
                    //}
                    string qiText;
                    if (TryGetQuickInfoFromDebugger(session, applicableToSpan.GetSpan(subjectBuffer.CurrentSnapshot), viewAdapter, out qiText))
                    {
                        quickInfoContent.Add(qiText);
                    }
                }
            }
        }

        private static IVsDebugger GetDebuggerService(ITextView textView)
        {
            IntPtr ptr;
            Guid gUID = typeof(SVsShellDebugger).GUID;
            Guid iid = typeof(IVsDebugger).GUID;
            VSGeneroPackage.Instance.GeneroEditorFactory.QueryService(ref gUID, ref iid, out ptr);
            if (ptr != IntPtr.Zero)
            {
                IVsDebugger objectForIUnknown = (IVsDebugger)Marshal.GetObjectForIUnknown(ptr);
                Marshal.Release(ptr);
                return objectForIUnknown;
            }
            return null;
        }

        private static TextSpan GetTextSpan(IVsTextView viewAdapter, SnapshotSpan span)
        {
            if (viewAdapter != null)
            {
                int startLine, startCol;
                int endLine, endCol;

                viewAdapter.GetLineAndColumn(span.Start.Position, out startLine, out startCol);
                viewAdapter.GetLineAndColumn(span.End.Position, out endLine, out endCol);

                TextSpan newSpan = new TextSpan()
                {
                    iStartLine = startLine,
                    iStartIndex = startCol,
                    iEndLine = endLine,
                    iEndIndex = endCol
                };

                return newSpan;
            }
            return default(TextSpan);
        }

        private static bool TryGetQuickInfoFromDebugger(IQuickInfoSession session, SnapshotSpan span, IVsTextView viewAdapter, out string tipText)
        {
            IVsTextLines lines;
            tipText = null;
            IVsDebugger debuggerService = GetDebuggerService(session.TextView);
            if (debuggerService == null)
            {
                return false;
            }

            IVsTextView vsTextView = viewAdapter;
            var txtSpan = GetTextSpan(viewAdapter, span); //GetTextSpan(session.TextView.TextBuffer, span, out vsTextView);
            TextSpan[] dataBufferTextSpan = new TextSpan[] { txtSpan };

            int hr = -2147467259;
            if ((dataBufferTextSpan[0].iStartLine == dataBufferTextSpan[0].iEndLine) && (dataBufferTextSpan[0].iStartIndex == dataBufferTextSpan[0].iEndIndex))
            {
                int iStartIndex = dataBufferTextSpan[0].iStartIndex;
                int iStartLine = dataBufferTextSpan[0].iStartLine;
                //if (ErrorHandler.Failed(textViewWindow.GetWordExtent(iStartLine, iStartIndex, 0, dataBufferTextSpan)))
                //{
                //    return false;
                //}
                if ((iStartLine < dataBufferTextSpan[0].iStartLine) || (iStartLine > dataBufferTextSpan[0].iEndLine))
                {
                    return false;
                }
                if ((iStartLine == dataBufferTextSpan[0].iStartLine) && (iStartIndex < dataBufferTextSpan[0].iStartIndex))
                {
                    return false;
                }
                if ((iStartLine == dataBufferTextSpan[0].iEndLine) && (iStartIndex >= dataBufferTextSpan[0].iEndIndex))
                {
                    return false;
                }
            }
            if (ErrorHandler.Failed(vsTextView.GetBuffer(out lines)))
            {
                return false;
            }
            hr = debuggerService.GetDataTipValue(lines, dataBufferTextSpan, null, out tipText);
            if (hr == 0x45001)
            {
                HandoffNoDefaultTipToDebugger(session);
                session.Dismiss();
                tipText = null;
                return true;
            }
            if (ErrorHandler.Failed(hr))
            {
                return false;
            }
            return true;
        }

        private static void HandoffNoDefaultTipToDebugger(IQuickInfoSession session)
        {
            IVsDebugger debuggerService = GetDebuggerService(session.TextView);
            if (debuggerService != null)
            {
                IVsCustomDataTip tip = debuggerService as IVsCustomDataTip;
                if (tip != null)
                {
                    tip.DisplayDataTip();
                }
            }
        }

        #endregion

        public void Dispose()
        {
        }

        #region ITypeResolver Members

        public ITypeResult GetGeneroType(string variableName, string filename, int lineNumber)
        {
            // TODO: need to
            // 1) Retrieve the GeneroAst based on the filename
            // 2) 

            if (_provider._PublicFunctionProvider != null)
                _provider._PublicFunctionProvider.SetFilename(filename);
            if (_provider._DatabaseInfoProvider != null)
                _provider._DatabaseInfoProvider.SetFilename(filename);

            if(VSGeneroPackage.Instance.DefaultAnalyzer != null)
            {
                var projectEntry = VSGeneroPackage.Instance.DefaultAnalyzer.AnalyzeFile(filename);  // This file should already be "analyzed"
                if (projectEntry != null && projectEntry.Analysis != null)
                {
                    IGeneroProject dummyProj;
                    IProjectEntry projEntry;
                    bool dummyDef;
                    var analysisResult = projectEntry.Analysis.GetValueByIndex(variableName,
                                                                                projectEntry.Analysis.LineNumberToIndex(lineNumber),
                                                                                _provider._PublicFunctionProvider,
                                                                                _provider._DatabaseInfoProvider,
                                                                                _provider._ProgramFileProvider,
                                                                                false,
                                                                                out dummyDef, out dummyProj, out projEntry, FunctionProviderSearchMode.Search);
                    if (analysisResult != null && analysisResult is IVariableResult)
                    {
                        return (analysisResult as IVariableResult).GetGeneroType();
                    }
                }
            }
            return null;
        }

        #endregion
    }

    [ComImport, Guid("80DD0557-F6FE-48e3-9651-398C5E7D8D78"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
    internal interface IVsCustomDataTip
    {
        [PreserveSig]
        int DisplayDataTip();
    }
}
