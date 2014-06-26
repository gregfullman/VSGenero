using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio;
using System.Diagnostics;
using System.Windows.Input;

namespace VSGenero.EditorExtensions.Intellisense
{
    class Genero4GLIntellisenseController : IIntellisenseController, IOleCommandTarget
    {
        private readonly ITextView _textView;
        private readonly Genero4GLIntellisenseControllerProvider _provider;
        private IOleCommandTarget _oldTarget; 
        private IEditorOperations _editOps;
        private readonly IIncrementalSearch _incSearch;
        private ICompletionSession _activeSession;
        private ISignatureHelpSession _sigHelpSession;
        private IQuickInfoSession _quickInfoSession;

        internal IVsEditorAdaptersFactoryService AdaptersFactory
        {
            get
            {
                return _provider._adaptersFactory;
            }
        }

        internal ICompletionBroker CompletionBroker
        {
            get
            {
                return _provider._CompletionBroker;
            }
        }

        internal ISignatureHelpBroker SignatureBroker
        {
            get
            {
                return _provider._SigBroker;
            }
        }

        public Genero4GLIntellisenseController(Genero4GLIntellisenseControllerProvider provider, ITextView textView)
        {
            _textView = textView;
            _provider = provider;
            _editOps = provider._EditOperationsFactory.GetEditorOperations(textView);
            _incSearch = provider._IncrementalSearch.GetIncrementalSearch(textView);
            //_textView.MouseHover += TextViewMouseHover;
            textView.Properties.AddProperty(typeof(Genero4GLIntellisenseController), this);  // added so our key processors can get back to us
        }

        //private void TextViewMouseHover(object sender, MouseHoverEventArgs e)
        //{
        //    if (_quickInfoSession != null && !_quickInfoSession.IsDismissed)
        //    {
        //        _quickInfoSession.Dismiss();
        //    }
        //    var pt = e.TextPosition.GetPoint(VSGeneroConstants.IsGenero4GLContent, PositionAffinity.Successor);
        //    if (pt != null)
        //    {
        //        _quickInfoSession = _provider._QuickInfoBroker.TriggerQuickInfo(
        //            _textView,
        //            pt.Value.Snapshot.CreateTrackingPoint(pt.Value.Position, PointTrackingMode.Positive),
        //            true);
        //    }
        //}

        //internal void TriggerQuickInfo()
        //{
        //    if (_quickInfoSession != null && !_quickInfoSession.IsDismissed)
        //    {
        //        _quickInfoSession.Dismiss();
        //    }
        //    _quickInfoSession = _provider._QuickInfoBroker.TriggerQuickInfo(_textView);
        //}

        /// <summary>
        /// Triggers Statement completion when appropriate keys are pressed
        /// The key combination is CTRL-J or "."
        /// The intellisense window is dismissed when one presses ESC key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreprocessKeyDown(object sender, TextCompositionEventArgs e)
        {
            // We should only receive pre-process events from our text view
            Debug.Assert(sender == _textView);

            // TODO: We should handle = for signature completion of keyword arguments

            string text = e.Text;
            if (text.Length == 1)
            {
                HandleChar(text[0]);
            }
        }

        private void HandleChar(char ch)
        {
            // We trigger completions when the user types . or space.  Called via our IOleCommandTarget filter
            // on the text view.
            //
            // We trigger signature help when we receive a "(".  We update our current sig when 
            // we receive a "," and we close sig help when we receive a ")".

            if (!_incSearch.IsActive)
            {
                switch (ch)
                {
                    case '(':
                        if (/*PythonToolsPackage.Instance.LangPrefs.AutoListParams*/true)
                        {
                            OpenParenStartSignatureSession();
                        }
                        break;
                    case ')':
                        if (_sigHelpSession != null)
                        {
                            _sigHelpSession.Dismiss();
                            _sigHelpSession = null;
                        }

                        if (/*PythonToolsPackage.Instance.LangPrefs.AutoListParams*/true)
                        {
                            // trigger help for outer call if there is one
                            TriggerSignatureHelp();
                        }
                        break;
                    case '=':
                    case ',':
                        if (_sigHelpSession == null)
                        {
                            if (/*PythonToolsPackage.Instance.LangPrefs.AutoListParams*/true)
                            {
                                CommaStartSignatureSession();
                            }
                        }
                        else
                        {
                            UpdateCurrentParameter();
                        }
                        break;
                    case '@':
                    case '.':
                        if(VSGeneroPackage.Instance.IntellisenseOptions4GLPage.ShowCompletionList && _activeSession == null)
                        {
                            TriggerCompletionSession(false);
                        }
                        break;
                    default:
                        if(IsIdentifierChar(ch) && _activeSession == null)
                        {
                            TriggerCompletionSession(false);
                        }
                        break;
                }
            }
        }

        private bool Backspace()
        {
            if (_sigHelpSession != null)
            {
                //if (_textView.Selection.IsActive && !_textView.Selection.IsEmpty)
                //if (_sigHelpSession != null && !_sigHelpSession.IsDismissed)
                //{
                //    // when deleting a selection don't do anything to pop up signature help again
                //    _sigHelpSession.Dismiss();
                //    return false;
                //}

                SnapshotPoint? caretPoint = _textView.BufferGraph.MapDownToFirstMatch(
                    _textView.Caret.Position.BufferPosition,
                    PointTrackingMode.Positive,
                    VSGeneroConstants.IsGenero4GLContent,
                    PositionAffinity.Predecessor
                );

                if (caretPoint != null && caretPoint.Value.Position != 0)
                {
                    var deleting = caretPoint.Value.Snapshot[caretPoint.Value.Position - 1];
                    if (deleting == ',')
                    {
                        caretPoint.Value.Snapshot.TextBuffer.Delete(new Span(caretPoint.Value.Position - 1, 1));
                        UpdateCurrentParameter();
                        return true;
                    }
                    else if (deleting == '(' || deleting == ')')
                    {
                        _sigHelpSession.Dismiss();
                        // delete the ( before triggering help again
                        caretPoint.Value.Snapshot.TextBuffer.Delete(new Span(caretPoint.Value.Position - 1, 1));

                        // Pop to an outer nesting of signature help
                        //if (/*PythonToolsPackage.Instance.LangPrefs.AutoListParams*/true)
                        //{
                        //    TriggerSignatureHelp();
                        //}

                        return true;
                    }
                }
            }
            return false;
        }

        private void OpenParenStartSignatureSession()
        {
            if (_activeSession != null)
            {
                _activeSession.Dismiss();
            }
            if (_sigHelpSession != null)
            {
                _sigHelpSession.Dismiss();
            }

            TriggerSignatureHelp();
        }

        private void CommaStartSignatureSession()
        {
             TriggerSignatureHelp();
        }

        /// <summary>
        /// Updates the current parameter for the caret's current position.
        /// 
        /// This will analyze the buffer for where we are currently located, find the current
        /// parameter that we're entering, and then update the signature.  If our current
        /// signature does not have enough parameters we'll find a signature which does.
        /// </summary>
        private void UpdateCurrentParameter()
        {
            if (_sigHelpSession == null)
            {
                // we moved out of the original span for sig help, re-trigger based upon the position
                TriggerSignatureHelp();
                return;
            }

            int position = _textView.Caret.Position.BufferPosition.Position;
            // we advance to the next parameter
            // TODO: Take into account params arrays
            // TODO: need to parse and see if we have keyword arguments entered into the current signature yet
            GeneroFunctionSignature sig = _sigHelpSession.SelectedSignature as GeneroFunctionSignature;
            if (sig != null)
            {
                int startPos = sig.ApplicableToSpan.GetStartPoint(sig.ApplicableToSpan.TextBuffer.CurrentSnapshot).Position;
                SnapshotSpan subSpan = new SnapshotSpan(sig.ApplicableToSpan.TextBuffer.CurrentSnapshot,
                                                        startPos,
                                                        position - startPos);
                var currText = subSpan.GetText();
                int currParam = currText.Count(x => x == ',');

                if (currParam < sig.Parameters.Count)
                {
                    sig.SetCurrentParameter(sig.Parameters[currParam]);
                }
                else
                {
                    sig.SetCurrentParameter(null);
                }


                //var prevBuffer = sig.ApplicableToSpan.TextBuffer;
                //var textBuffer = _textView.TextBuffer;

                //var targetPt = _textView.BufferGraph.MapDownToFirstMatch(
                //    new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, position),
                //    PointTrackingMode.Positive,
                //    VSGeneroConstants.IsGenero4GLContent,
                //    PositionAffinity.Successor
                //);

                //if (targetPt != null)
                //{
                    // check to see where in the current signature we are

                    //var span = targetPt.Value.Snapshot.CreateTrackingSpan(targetPt.Value.Position, 0, SpanTrackingMode.EdgeInclusive);

                    //var sigs = targetPt.Value.Snapshot.GetSignatures(span);
                    //bool retrigger = false;
                    // TODO: do we need to support this?
                    //if (sigs.Signatures.Count == _sigHelpSession.Signatures.Count)
                    //{
                    //    for (int i = 0; i < sigs.Signatures.Count && !retrigger; i++)
                    //    {
                    //        var leftSig = sigs.Signatures[i];
                    //        var rightSig = _sigHelpSession.Signatures[i];

                    //        if (leftSig.Parameters.Count == rightSig.Parameters.Count)
                    //        {
                    //            for (int j = 0; j < leftSig.Parameters.Count; j++)
                    //            {
                    //                var leftParam = leftSig.Parameters[j];
                    //                var rightParam = rightSig.Parameters[j];

                    //                if (leftParam.Name != rightParam.Name || leftParam.Documentation != rightParam.Documentation)
                    //                {
                    //                    retrigger = true;
                    //                    break;
                    //                }
                    //            }
                    //        }

                    //        if (leftSig.Content != rightSig.Content || leftSig.Documentation != rightSig.Documentation)
                    //        {
                    //            retrigger = true;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //retrigger = true;
                    //}

                    //if (retrigger)
                    //{
                    //    _sigHelpSession.Dismiss();
                    //    TriggerSignatureHelp();
                    //}
                    //else
                    //{
                    //int curParam = sigs.ParameterIndex;
                    //if (sigs.LastKeywordArgument != null)
                    //{
                    //    curParam = Int32.MaxValue;
                    //    for (int i = 0; i < sig.Parameters.Count; i++)
                    //    {
                    //        if (sig.Parameters[i].Name == sigs.LastKeywordArgument)
                    //        {
                    //            curParam = i;
                    //            break;
                    //        }
                    //    }
                    //}


       
                    //if (curParam < sig.Parameters.Count)
                    //{
                    //    sig.SetCurrentParameter(sig.Parameters[curParam]);
                    //}
                    //else if (sigs.LastKeywordArgument == "")
                    //{
                    //    sig.SetCurrentParameter(null);
                    //}
                    //else
                    //{
                    //    CommaFindBestSignature(curParam, sigs.LastKeywordArgument);
                    //}
                    //}
                //}
            }
        }

        private void CommaFindBestSignature(int curParam, string lastKeywordArg)
        {
            // see if we have a signature which accomodates this...

            // TODO: We should also take into account param arrays
            // TODO: We should also get the types of the arguments and use that to
            // pick the best signature when the signature includes types.
            foreach (var availableSig in _sigHelpSession.Signatures)
            {
                if (lastKeywordArg != null)
                {
                    for (int i = 0; i < availableSig.Parameters.Count; i++)
                    {
                        if (availableSig.Parameters[i].Name == lastKeywordArg)
                        {
                            _sigHelpSession.SelectedSignature = availableSig;

                            GeneroFunctionSignature sig = availableSig as GeneroFunctionSignature;
                            if (sig != null)
                            {
                                sig.SetCurrentParameter(sig.Parameters[i]);
                            }
                            break;

                        }
                    }
                }
                else if (availableSig.Parameters.Count > curParam)
                {
                    _sigHelpSession.SelectedSignature = availableSig;

                    GeneroFunctionSignature sig = availableSig as GeneroFunctionSignature;
                    if (sig != null)
                    {
                        sig.SetCurrentParameter(sig.Parameters[curParam]);
                    }
                    break;
                }
            }
        }

        [ThreadStatic]
        internal static bool ForceCompletions;

        internal void TriggerCompletionSession(bool completeWord)
        {
            Dismiss();

            _activeSession = CompletionBroker.TriggerCompletion(_textView);

            if (_activeSession != null)
            {
                MemberCompletionSet set;
                if (completeWord &&
                    _activeSession.CompletionSets.Count == 1 &&
                    (set = _activeSession.CompletionSets[0] as MemberCompletionSet) != null &&
                    set.SelectSingleBest())
                {
                    _activeSession.Commit();
                    _activeSession = null;
                }
                else
                {
                    _activeSession.Filter();
                    _activeSession.Dismissed += OnCompletionSessionDismissed;
                    _activeSession.Committed += OnCompletionSessionCommitted;
                }
            }
        }

        

        internal void TriggerSignatureHelp()
        {
            if (_sigHelpSession != null)
            {
                _sigHelpSession.Dismiss();
            }

            _sigHelpSession = SignatureBroker.TriggerSignatureHelp(_textView);

            if (_sigHelpSession != null)
            {
                _sigHelpSession.Dismissed += OnSignatureSessionDismissed;

                ISignature sig;
                if (_sigHelpSession.Properties.TryGetProperty(typeof(GeneroFunctionSignature), out sig))
                {
                    _sigHelpSession.SelectedSignature = sig;


                    IParameter param = _sigHelpSession.SelectedSignature.CurrentParameter;
                    if(param != null)
                    //if (_sigHelpSession.Properties.TryGetProperty(typeof(GeneroFunctionParameter), out param))
                    {
                        ((GeneroFunctionSignature)sig).SetCurrentParameter(param);
                    }
                }
            }
        }

        void OnCompletionSessionCommitted(object sender, EventArgs e)
        {
            if (VSGeneroPackage.Instance.IntellisenseOptions4GLPage.PreSelectMRU)
            {
                if (_activeSession.SelectedCompletionSet != null &&
                   _activeSession.SelectedCompletionSet.SelectionStatus != null)
                {
                    // find the new completion in the list of MRU completions. If found, move it to the front.
                    // If not found, add it.
                    var firstMRU = IntellisenseExtensions.LastCommittedCompletions.FirstOrDefault(x => x.DisplayText == _activeSession.SelectedCompletionSet.SelectionStatus.Completion.DisplayText);
                    if (firstMRU != null)
                    {
                        var indexOfMRU = IntellisenseExtensions.LastCommittedCompletions.IndexOf(firstMRU);
                        IntellisenseExtensions.LastCommittedCompletions.RemoveAt(indexOfMRU);
                    }
                    IntellisenseExtensions.LastCommittedCompletions.Insert(0, _activeSession.SelectedCompletionSet.SelectionStatus.Completion);
                }
            }
            _activeSession.Committed -= OnCompletionSessionCommitted;
        }

        private void OnCompletionSessionDismissed(object sender, System.EventArgs e)
        {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            _activeSession.Dismissed -= OnCompletionSessionDismissed;
            _activeSession = null;
        }

        private void OnSignatureSessionDismissed(object sender, System.EventArgs e)
        {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            _sigHelpSession.Dismissed -= OnSignatureSessionDismissed;
            _sigHelpSession = null;
        }

        private void DeleteSelectedSpans()
        {
            if (!_textView.Selection.IsEmpty)
            {
                _editOps.Delete();
            }
        }

        private void Dismiss()
        {
            if (_activeSession != null)
            {
                _activeSession.Dismiss();
            }
        }

        public void ConnectSubjectBuffer(Microsoft.VisualStudio.Text.ITextBuffer subjectBuffer)
        {
        }

        public void Detach(ITextView textView)
        {
            if (_textView == null)
            {
                throw new InvalidOperationException("Already detached from text view");
            }
            if (textView != _textView)
            {
                throw new ArgumentException("Not attached to specified text view", "textView");
            }

            //_textView.MouseHover -= TextViewMouseHover;
            _textView.Properties.RemoveProperty(typeof(Genero4GLIntellisenseController));

            DetachKeyboardFilter();
        }

        public void DisconnectSubjectBuffer(Microsoft.VisualStudio.Text.ITextBuffer subjectBuffer)
        {
        }

        private static bool IsIdentifierChar(char ch)
        {
            return ch == '_' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9');
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (int)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);

                if (_activeSession != null && !_activeSession.IsDismissed)
                {
                    if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected &&
                        VSGeneroPackage.Instance.IntellisenseOptions4GLPage.CompletionCommittedBy.IndexOf(ch) != -1)
                    {
                        _activeSession.Commit();
                    }
                    else if (!IsIdentifierChar(ch))
                    {
                        _activeSession.Dismiss();
                    }
                }

                int res = _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                HandleChar((char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn));

                if (_activeSession != null && !_activeSession.IsDismissed)
                {
                    _activeSession.Filter();
                }

                return res;
            }

            if (_activeSession != null)
            {
                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID)
                    {
                        case VSConstants.VSStd2KCmdID.RETURN:
                            if (VSGeneroPackage.Instance.IntellisenseOptions4GLPage.EnterCommitsIntellisense &&
                                !_activeSession.IsDismissed &&
                                _activeSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                            {

                                // If the user has typed all of the characters as the completion and presses
                                // enter we should dismiss & let the text editor receive the enter.  For example 
                                // when typing "import sys[ENTER]" completion starts after the space.  After typing
                                // sys the user wants a new line and doesn't want to type enter twice.

                                bool enterOnComplete = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.AddNewLineAtEndOfFullyTypedWord &&
                                         EnterOnCompleteText();

                                _activeSession.Commit();

                                if (!enterOnComplete)
                                {
                                    return VSConstants.S_OK;
                                }
                            }
                            else
                            {
                                _activeSession.Dismiss();
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.TAB:
                            if (!_activeSession.IsDismissed)
                            {
                                _activeSession.Commit();
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.BACKSPACE:
                        case VSConstants.VSStd2KCmdID.DELETE:
                        case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                        case VSConstants.VSStd2KCmdID.DELETEWORDRIGHT:
                            int res = _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            if (_activeSession != null && !_activeSession.IsDismissed)
                            {
                                _activeSession.Filter();
                            }
                            return res;
                    }
                }
            }
            else if (_sigHelpSession != null)
            {
                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID)
                    {
                        case VSConstants.VSStd2KCmdID.BACKSPACE:
                            bool fDeleted = Backspace();
                            if (fDeleted)
                            {
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.LEFT:
                            _editOps.MoveToPreviousCharacter(false);
                            UpdateCurrentParameter();
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.RIGHT:
                            _editOps.MoveToNextCharacter(false);
                            UpdateCurrentParameter();
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.HOME:
                        case VSConstants.VSStd2KCmdID.BOL:
                        case VSConstants.VSStd2KCmdID.BOL_EXT:
                        case VSConstants.VSStd2KCmdID.EOL:
                        case VSConstants.VSStd2KCmdID.EOL_EXT:
                        case VSConstants.VSStd2KCmdID.END:
                        case VSConstants.VSStd2KCmdID.WORDPREV:
                        case VSConstants.VSStd2KCmdID.WORDPREV_EXT:
                        case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                            _sigHelpSession.Dismiss();
                            _sigHelpSession = null;
                            break;
                    }
                }
            }

            return _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private bool EnterOnCompleteText()
        {
            SnapshotPoint? point = _activeSession.GetTriggerPoint(_textView.TextBuffer.CurrentSnapshot);
            if (point.HasValue)
            {
                int chars = _textView.Caret.Position.BufferPosition.Position - point.Value.Position;
                var selectionStatus = _activeSession.SelectedCompletionSet.SelectionStatus;
                if (chars == selectionStatus.Completion.InsertionText.Length)
                {
                    string text = _textView.TextSnapshot.GetText(point.Value.Position, chars);

                    if (String.Compare(text, selectionStatus.Completion.InsertionText, true) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _oldTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        // we need this because VS won't give us certain keyboard events as they're handled before our key processor.  These
        // include enter and tab both of which we want to complete.

        internal void AttachKeyboardFilter()
        {
            if (_oldTarget == null)
            {
                var viewAdapter = AdaptersFactory.GetViewAdapter(_textView);
                if (viewAdapter != null)
                {
                    ErrorHandler.ThrowOnFailure(viewAdapter.AddCommandFilter(this, out _oldTarget));
                }
            }
        }

        private void DetachKeyboardFilter()
        {
            if (_oldTarget != null)
            {
                ErrorHandler.ThrowOnFailure(AdaptersFactory.GetViewAdapter(_textView).RemoveCommandFilter(this));
                _oldTarget = null;
            }
        }
    }
}
