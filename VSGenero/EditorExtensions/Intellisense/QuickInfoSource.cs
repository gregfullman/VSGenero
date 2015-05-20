using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing.AST;
using Microsoft.VisualStudio.VSCommon;
using EnvDTE;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio;

namespace VSGenero.EditorExtensions.Intellisense
{
    internal class QuickInfoSource : IQuickInfoSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly QuickInfoSourceProvider _provider;
        private IQuickInfoSession _curSession;

        public QuickInfoSource(QuickInfoSourceProvider provider, ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
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
            if (_provider._ProgramFileProvider != null)
                _provider._ProgramFileProvider.SetFilename(_textBuffer.GetFilePath());
            var vars = _textBuffer.CurrentSnapshot.AnalyzeExpression(
                session.CreateTrackingSpan(_textBuffer),
                false,
                _provider._PublicFunctionProvider,
                _provider._DatabaseInfoProvider,
                _provider._ProgramFileProvider
            );

            AugmentQuickInfoWorker(session, _textBuffer, vars, quickInfoContent, out applicableToSpan);
        }

        private void CurSessionDismissed(object sender, EventArgs e)
        {
            _curSession = null;
        }

        internal static void AugmentQuickInfoWorker(IQuickInfoSession session, ITextBuffer subjectBuffer, ExpressionAnalysis exprAnalysis, System.Collections.Generic.IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
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
                    string qiText;
                    if(TryGetQuickInfoFromDebugger(session, applicableToSpan.GetSpan(subjectBuffer.CurrentSnapshot), out qiText))
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

        private static TextSpan GetTextSpan(ITextBuffer subjectBuffer, SnapshotSpan span, out IVsTextView vsTextView)
        {
            IWpfTextView textView;
            vsTextView = null;
            if (subjectBuffer.Properties.TryGetProperty<IWpfTextView>(typeof(IWpfTextView), out textView))
            {
                if (subjectBuffer.Properties.TryGetProperty<IVsTextView>(typeof(IVsTextView), out vsTextView))
                {
                    var adapterFactory = VSGeneroPackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
                    if (adapterFactory != null)
                    {
                        var viewAdapter = adapterFactory.GetViewAdapter(textView);
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
                    }
                }
            }
            return default(TextSpan);
        }

        private static bool TryGetQuickInfoFromDebugger(IQuickInfoSession session, SnapshotSpan span, out string tipText)
        {
            IVsTextLines lines;
            tipText = null;
            IVsDebugger debuggerService = GetDebuggerService(session.TextView);
            if (debuggerService == null)
            {
                return false;
            }

            IVsTextView vsTextView;
            var txtSpan = GetTextSpan(session.TextView.TextBuffer, span, out vsTextView);
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

    }

    [ComImport, Guid("80DD0557-F6FE-48e3-9651-398C5E7D8D78"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
    internal interface IVsCustomDataTip
    {
        [PreserveSig]
        int DisplayDataTip();
    }
}
