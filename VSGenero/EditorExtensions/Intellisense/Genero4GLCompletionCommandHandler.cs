/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 * 
 * Contents of this file are based on the MSDN walkthrough here:
 * http://msdn.microsoft.com/en-us/library/ee372314.aspx
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Reflection;
using System.Collections.Generic;

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Genero 4GL Completion controller")]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class Genero4GLCompletionHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }
        [Import]
        internal ISignatureHelpBroker SignatureBroker = null; // Set via MEF

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Func<Genero4GLCompletionCommandHandler> createCommandHandler = delegate() { return new Genero4GLCompletionCommandHandler(textViewAdapter, textView, this); };
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }

    internal class Genero4GLCompletionCommandHandler : IOleCommandTarget
    {
        private IOleCommandTarget m_nextCommandHandler;
        private ITextView m_textView;
        private Genero4GLCompletionHandlerProvider m_provider;
        private ICompletionSession m_session;
        private ISignatureHelpSession _sigHelpSession;
        private readonly char[] _commitChars = { ' ', '.', ',', '(' };

        internal Genero4GLCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, Genero4GLCompletionHandlerProvider provider)
        {
            this.m_textView = textView;
            this.m_provider = provider;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out m_nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return m_nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(m_provider.ServiceProvider))
            {
                return m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands 
            uint commandID = nCmdID;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it 
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //check for a commit character 
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar) || 
                    (char.IsPunctuation(typedChar) && typedChar != '_')))   // don't want to commit on '_'
            {
                //check for a a selection 
                if (m_session != null && !m_session.IsDismissed)
                {
                    //if the selection is fully selected, commit the current session 
                    if (m_session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        m_session.Commit();
                        //also, don't add the character to the buffer 
                        if(!_commitChars.Contains(typedChar))
                            return VSConstants.S_OK;
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        m_session.Dismiss();
                    }
                }
            }

            //pass along the command so the char is added to the buffer 
            int retVal = m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            if ((!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar)) ||
                 typedChar == '.')
            {
                if (m_session == null || m_session.IsDismissed) // If there is no active session, bring up completion
                {
                    var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                    this.TriggerCompletion();
                    // This seems to be null when an empty completion set is returned
                    if (m_session != null)
                    {
                        m_session.Filter();
                    }
                }
                else     //the completion session is already active, so just filter
                {
                    m_session.Filter();
                }
                handled = true;
            }
            //else if (typedChar == '(')
            //{
            //    if (m_session != null)
            //    {
            //        m_session.Dismiss();
            //    }
            //    if (_sigHelpSession != null)
            //    {
            //        _sigHelpSession.Dismiss();
            //    }
            //}
            //else if (typedChar == ')')
            //{


            //}
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (m_session != null && !m_session.IsDismissed)
                    m_session.Filter();
                handled = true;
            }
            if (handled) return VSConstants.S_OK;
            return retVal;
        }

        internal void TriggerSignatureHelp()
        {
            if (_sigHelpSession != null)
            {
                _sigHelpSession.Dismiss();
            }

            _sigHelpSession = m_provider.SignatureBroker.TriggerSignatureHelp(m_textView);


        }

        private bool TriggerCompletion()
        {
            //the caret must be in a non-projection location 
            SnapshotPoint? caretPoint =
            m_textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }

            m_session = m_provider.CompletionBroker.CreateCompletionSession
         (m_textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            //subscribe to the Dismissed event on the session 
            m_session.Dismissed += this.OnSessionDismissed;
            m_session.Start();

            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            m_session.Dismissed -= this.OnSessionDismissed;
            m_session = null;
        }
    }
}
