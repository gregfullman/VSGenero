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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VSGenero.Snippets;
using VSGenero.EditorExtensions;
using VSGenero.Analysis;
using IServiceProvider = System.IServiceProvider;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.EditorExtensions.Intellisense
{
    internal sealed class IntellisenseController : IIntellisenseController, IOleCommandTarget
    {
        private readonly ITextView _textView;
        private IVsTextView _vsTextView;
        private IVsExpansionSession _expansionSession;
        private readonly IServiceProvider _serviceProvider;
        private readonly IVsExpansionManager _expansionMgr;
        private readonly ExpansionClient _expansionClient;
        private readonly IntellisenseControllerProvider _provider;
        private readonly IIncrementalSearch _incSearch;
        private BufferParser _bufferParser;
        private ICompletionSession _activeSession;
        private ISignatureHelpSession _sigHelpSession;
        private IQuickInfoSession _quickInfoSession;
        private IOleCommandTarget _oldTarget;
        private IEditorOperations _editOps;
        private static string[] _allStandardSnippetTypes = { ExpansionClient.Expansion, ExpansionClient.SurroundsWith };
        private static string[] _surroundsWithSnippetTypes = { ExpansionClient.SurroundsWith };

        /// <summary>
        /// Attaches events for invoking Statement completion 
        /// </summary>
        public IntellisenseController(IntellisenseControllerProvider provider, ITextView textView, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _textView = textView;
            _provider = provider;
            _editOps = provider._EditOperationsFactory.GetEditorOperations(textView);
            _incSearch = provider._IncrementalSearch.GetIncrementalSearch(textView);
            //_textView.MouseHover += TextViewMouseHover;
            textView.Properties.AddProperty(typeof(IntellisenseController), this);  // added so our key processors can get back to us

            if (_textView.TextBuffer.ContentType.IsOfType(VSGeneroConstants.ContentType4GL) ||
                _textView.TextBuffer.ContentType.IsOfType(VSGeneroConstants.ContentTypeINC))
            {
                try
                {
                    _expansionClient = new ExpansionClient(textView, provider._adaptersFactory, provider._ServiceProvider);
                    var textMgr = (IVsTextManager2)_serviceProvider.GetService(typeof(SVsTextManager));
                    textMgr.GetExpansionManager(out _expansionMgr);
                }
                catch (ArgumentException ex)
                {
                    // No expansion client for this buffer, but we can continue without it
                    Debug.Fail(ex.ToString());
                }
            }
        }

        internal void SetBufferParser(BufferParser bufferParser)
        {
            Utilities.CheckNotNull(bufferParser, "Cannot set buffer parser multiple times");
            _bufferParser = bufferParser;
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

        internal void TriggerQuickInfo()
        {
            if (_quickInfoSession != null && !_quickInfoSession.IsDismissed)
            {
                _quickInfoSession.Dismiss();
            }
            _quickInfoSession = _provider._QuickInfoBroker.TriggerQuickInfo(_textView);
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            PropagateAnalyzer(subjectBuffer);

            Debug.Assert(_bufferParser != null, "SetBufferParser has not been called");
            BufferParser existingParser;
            if (!subjectBuffer.Properties.TryGetProperty(typeof(BufferParser), out existingParser))
            {
                _bufferParser.AddBuffer(subjectBuffer);
            }
            else
            {
                // already connected to a buffer parser, we should have the same project entry
                Debug.Assert(_bufferParser._currentProjEntry == existingParser._currentProjEntry);
            }
        }

        public void PropagateAnalyzer(ITextBuffer subjectBuffer)
        {
            int i = 0;
            //PythonReplEvaluator replEvaluator;
            //if (_textView.Properties.TryGetProperty<PythonReplEvaluator>(typeof(PythonReplEvaluator), out replEvaluator))
            //{
            //    subjectBuffer.Properties.AddProperty(typeof(VsProjectAnalyzer), replEvaluator.ReplAnalyzer);
            //}
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            // only disconnect if we own the buffer parser
            Debug.Assert(_bufferParser != null, "SetBufferParser has not been called");
            BufferParser existingParser;
            if (subjectBuffer.Properties.TryGetProperty<BufferParser>(typeof(BufferParser), out existingParser) &&
                --existingParser.AttachedViews == 0)
            {
                _bufferParser.RemoveBuffer(subjectBuffer);
            }
        }

        /// <summary>
        /// Detaches the events
        /// </summary>
        /// <param name="textView"></param>
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
            _textView.Properties.RemoveProperty(typeof(IntellisenseController));

            DetachKeyboardFilter();

            _bufferParser = null;
        }

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
                    case ' ':
                        if (VSGeneroPackage.Instance.LangPrefs.SpaceTriggersCompletion &&
                            VSGeneroPackage.Instance.LangPrefs.AutoListMembers && AreSurroundingCharactersWhitespace(ch, true))
                        {
                            TriggerCompletionSession(false);
                        }
                        break;
                    case '.':
                        if (VSGeneroPackage.Instance.LangPrefs.AutoListMembers && AreSurroundingCharactersWhitespace(ch, true))
                        {
                            TriggerCompletionSession(false);
                        }
                        break;
                    case '(':
                        if (VSGeneroPackage.Instance.LangPrefs.AutoListParams)
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

                        if (VSGeneroPackage.Instance.LangPrefs.AutoListParams)
                        {
                            // trigger help for outer call if there is one
                            TriggerSignatureHelp();
                        }
                        break;
                    //case '=':
                    case ',':
                        if (_sigHelpSession == null)
                        {
                            if (VSGeneroPackage.Instance.LangPrefs.AutoListParams)
                            {
                                CommaStartSignatureSession();
                            }
                        }
                        else
                        {
                            UpdateCurrentParameter();
                        }
                        break;
                    default:
                        if (VSGeneroPackage.Instance.LangPrefs.AutoListMembers && IsIdentifierChar(ch) && _activeSession == null && AreSurroundingCharactersWhitespace(ch))
                        {
                            TriggerCompletionSession(false);
                        }
                        break;
                }
            }
        }

        private bool AreSurroundingCharactersWhitespace(char currChar, bool onlyNextChar = false)
        {
            var point = _textView.GetCaretPosition();
            if (point.HasValue)
            {
                if (point.Value.Position <= 1) 
                    return true;
                string prevChar = _textView.TextSnapshot.GetText(new Span(point.Value.Position - 2, 1));
                string nextChar = _textView.TextSnapshot.GetText(new Span(point.Value.Position, 1));
                if ((prevChar == "(" || prevChar == "[") && (string.IsNullOrWhiteSpace(nextChar) || nextChar == "," || nextChar == ")" || nextChar == "]"))
                    return true;
                // Handle member completion
                if (onlyNextChar && currChar == '.' && nextChar == "(")
                    return true;
                return (string.IsNullOrWhiteSpace(prevChar) || onlyNextChar) && (string.IsNullOrWhiteSpace(nextChar) || nextChar == "," || nextChar == ")" || nextChar == "]");
            }
            return false;
        }

        private bool Backspace()
        {
            if (_sigHelpSession != null)
            {
                if (_textView.Selection.IsActive && !_textView.Selection.IsEmpty)
                {
                    // when deleting a selection don't do anything to pop up signature help again
                    _sigHelpSession.Dismiss();
                    return false;
                }

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
                        if (VSGeneroPackage.Instance.LangPrefs.AutoListParams)
                        {
                            TriggerSignatureHelp();
                        }

                        return true;
                    }
                    else
                    {
                        _sigHelpSession.Dismiss();
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
            Genero4glFunctionSignature sig = _sigHelpSession.SelectedSignature as Genero4glFunctionSignature;
            if (sig != null)
            {
                var prevBuffer = sig.ApplicableToSpan.TextBuffer;
                var textBuffer = _textView.TextBuffer;

                var targetPt = _textView.BufferGraph.MapDownToFirstMatch(
                    new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, position),
                    PointTrackingMode.Positive,
                    VSGeneroConstants.IsGenero4GLContent,
                    PositionAffinity.Successor
                );

                if (targetPt != null)
                {
                    var span = targetPt.Value.Snapshot.CreateTrackingSpan(targetPt.Value.Position, 0, SpanTrackingMode.EdgeInclusive);

                    var sigs = targetPt.Value.Snapshot.GetSignatures(span, _provider._PublicFunctionProvider);
                    bool retrigger = false;
                    if (sigs.Signatures.Count == _sigHelpSession.Signatures.Count)
                    {
                        for (int i = 0; i < sigs.Signatures.Count && !retrigger; i++)
                        {
                            var leftSig = sigs.Signatures[i];
                            var rightSig = _sigHelpSession.Signatures[i];

                            if (leftSig.Parameters.Count == rightSig.Parameters.Count)
                            {
                                for (int j = 0; j < leftSig.Parameters.Count; j++)
                                {
                                    var leftParam = leftSig.Parameters[j];
                                    var rightParam = rightSig.Parameters[j];

                                    if (leftParam.Name != rightParam.Name || leftParam.Documentation != rightParam.Documentation)
                                    {
                                        retrigger = true;
                                        break;
                                    }
                                }
                            }

                            if (leftSig.Content != rightSig.Content || leftSig.Documentation != rightSig.Documentation)
                            {
                                retrigger = true;
                            }
                        }
                    }
                    else
                    {
                        retrigger = true;
                    }

                    if (retrigger)
                    {
                        _sigHelpSession.Dismiss();
                        TriggerSignatureHelp();
                    }
                    else
                    {
                        int curParam = sigs.ParameterIndex;
                        if (sigs.LastKeywordArgument != null)
                        {
                            curParam = Int32.MaxValue;
                            for (int i = 0; i < sig.Parameters.Count; i++)
                            {
                                if (sig.Parameters[i].Name == sigs.LastKeywordArgument)
                                {
                                    curParam = i;
                                    break;
                                }
                            }
                        }

                        if (curParam < sig.Parameters.Count)
                        {
                            sig.SetCurrentParameter(sig.Parameters[curParam]);
                        }
                        else if (sigs.LastKeywordArgument == "")
                        {
                            sig.SetCurrentParameter(null);
                        }
                        else
                        {
                            CommaFindBestSignature(curParam, sigs.LastKeywordArgument);
                        }
                    }
                }
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

                            Genero4glFunctionSignature sig = availableSig as Genero4glFunctionSignature;
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

                    Genero4glFunctionSignature sig = availableSig as Genero4glFunctionSignature;
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

        Span? _parentSpan = null;

        internal void TriggerCompletionSession(bool completeWord)
        {
            Dismiss();

            _activeSession = CompletionBroker.TriggerCompletion(_textView);

            if (_activeSession != null)
            {
                FuzzyCompletionSet set;
                if (completeWord &&
                    _activeSession.CompletionSets.Count == 1 &&
                    (set = _activeSession.CompletionSets[0] as FuzzyCompletionSet) != null &&
                    set.SelectSingleBest())
                {
                    // prevent the changed handler from triggering
                    _blockChangeTrigger = true;

                    if(_parentSpan == null)
                        _parentSpan = GetCompletionParentSpan();
                    _activeSession.Commit();
                    if (_parentSpan != null)
                    {
                        RemoveParentSpan(_parentSpan);
                        _parentSpan = null;
                    }
                    _activeSession = null;
                }
                else
                {
                    _activeSession.Filter();
                    _activeSession.Dismissed += OnCompletionSessionDismissed;
                    _activeSession.Committed += OnCompletionSessionCommitted;
                    _textView.TextBuffer.Changed += TextBuffer_Changed;
                }
            }
        }

        private bool _blockChangeTrigger;
        void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (_activeSession == null || _blockChangeTrigger)
            {
                _textView.TextBuffer.Changed -= TextBuffer_Changed;
                _blockChangeTrigger = false;
            }
            else
            {
                if (e.After.Length - e.Before.Length == 1)
                {
                    if (_activeSession.SelectedCompletionSet.ApplicableTo.GetText(_textView.TextSnapshot).Length == 2 &&
                        _provider._PublicFunctionProvider != null)
                    {
                        SnapshotSpan? span = GetPrecedingExpression();
                        if(span.HasValue && span.Value.GetText().EndsWith("."))
                        {
                            return;
                        }

                        Dismiss();
                        _activeSession = CompletionBroker.TriggerCompletion(_textView);
                        _activeSession.Filter();
                        _activeSession.Dismissed += OnCompletionSessionDismissed;
                        _activeSession.Committed += OnCompletionSessionCommitted;
                    }
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
                if (_sigHelpSession.Properties.TryGetProperty(typeof(Genero4glFunctionSignature), out sig))
                {
                    _sigHelpSession.SelectedSignature = sig;

                    IParameter param;
                    if (_sigHelpSession.Properties.TryGetProperty(typeof(GeneroParameter), out param))
                    {
                        ((Genero4glFunctionSignature)sig).SetCurrentParameter(param);
                    }
                }
            }
        }

        private void OnCompletionSessionDismissed(object sender, EventArgs e)
        {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            if (_activeSession != null)
            {
                _activeSession.Dismissed -= OnCompletionSessionDismissed;
                _textView.TextBuffer.Changed -= TextBuffer_Changed;
                _activeSession = null;
            }
        }

        private void OnCompletionSessionCommitted(object sender, EventArgs e)
        {
            if (_activeSession != null)
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

                        if (_parentSpan == null)
                        {
                            _parentSpan = GetCompletionParentSpan();
                            if (_parentSpan != null)
                            {
                                RemoveParentSpan(_parentSpan);
                                _parentSpan = null;
                            }
                        }
                    }
                }
                if (_activeSession != null)
                {
                    _activeSession.Committed -= OnCompletionSessionCommitted;
                    _activeSession = null;
                    _textView.TextBuffer.Changed -= TextBuffer_Changed;
                }
            }
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

        internal ICompletionBroker CompletionBroker
        {
            get
            {
                return _provider._CompletionBroker;
            }
        }

        internal IVsEditorAdaptersFactoryService AdaptersFactory
        {
            get
            {
                return _provider._adaptersFactory;
            }
        }

        internal ISignatureHelpBroker SignatureBroker
        {
            get
            {
                return _provider._SigBroker;
            }
        }

        #region IOleCommandTarget Members

        // we need this because VS won't give us certain keyboard events as they're handled before our key processor.  These
        // include enter and tab both of which we want to complete.

        internal void AttachKeyboardFilter()
        {
            if (_oldTarget == null)
            {
                _vsTextView = AdaptersFactory.GetViewAdapter(_textView);
                if (_vsTextView != null)
                {
                    ErrorHandler.ThrowOnFailure(_vsTextView.AddCommandFilter(this, out _oldTarget));
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

        private Span? GetCompletionParentSpan()
        {
            Span? retSpan = null;
            if (_activeSession != null &&
               _activeSession.SelectedCompletionSet != null &&
               _activeSession.SelectedCompletionSet.SelectionStatus != null)
            {
                var compl = _activeSession.SelectedCompletionSet.SelectionStatus.Completion;
                string complParentName;
                if (compl.Properties.TryGetProperty<string>(CompletionAnalysis.CompletionParentPropertyName, out complParentName))
                {
                    var srcSpan = GetPrecedingExpression();
                    if (srcSpan.HasValue && srcSpan.Value.Length > 0)
                    {
                        string expr = srcSpan.Value.GetText();
                        if (complParentName.Equals(expr, StringComparison.OrdinalIgnoreCase))
                        {
                            // remove the span from the textview
                            retSpan = srcSpan.Value.Span;
                        }
                    }
                }
            }
            return retSpan;
        }

        private void RemoveParentSpan(Span? span)
        {
            if (span.HasValue && span.Value.Length > 0)
            {
                var edit = _textView.TextBuffer.CreateEdit();
                if (edit != null)
                {
                    if (edit.Delete(span.Value))
                    {
                        edit.Apply();
                    }
                }
            }
        }

        private bool IsPreviousCharacterWhitespace()
        {
            int position = _textView.Caret.Position.BufferPosition.Position;
            if (position == 0)
                return true;
            else
            {
                var span = new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, new Span(position - 1, 1));
                return string.IsNullOrWhiteSpace(span.GetText());
            }
        }

        private SnapshotSpan? GetPrecedingExpression()
        {
            ITrackingSpan startSpan = null;
            if (_activeSession != null)
            {
                var span = _activeSession.GetApplicableSpan(_textView.TextBuffer);
                startSpan = _textView.TextSnapshot.CreateTrackingSpan(span.GetSpan(_textView.TextSnapshot).Start.Position, 0, SpanTrackingMode.EdgeInclusive);
            }
            else
            {
                startSpan = _textView.TextSnapshot.CreateTrackingSpan(_textView.Caret.Position.BufferPosition.Position, 0, SpanTrackingMode.EdgeInclusive);
            }
            var parser = new Genero4glReverseParser(_textView.TextSnapshot, _textView.TextBuffer, startSpan);
            bool isFunctionCallOrDefinition;
            return parser.GetExpressionRange(out isFunctionCallOrDefinition);
        }

        private IVsTextView GetViewAdapter()
        {
            return _provider._adaptersFactory.GetViewAdapter(_textView);
        }

        private void TriggerSnippet(uint nCmdID)
        {
            if (_expansionMgr != null)
            {
                string prompt;
                string[] snippetTypes;
                if ((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.SURROUNDWITH)
                {
                    prompt = "SurroundWith";
                    snippetTypes = _surroundsWithSnippetTypes;
                }
                else
                {
                    prompt = "InsertSnippet";
                    snippetTypes = _allStandardSnippetTypes;
                }

                _expansionMgr.InvokeInsertionUI(
                    GetViewAdapter(),
                    _expansionClient,
                    VSGeneroConstants.guidGenero4glLanguageServiceGuid,
                    snippetTypes,
                    snippetTypes.Length,
                    0,
                    null,
                    0,
                    0,
                    prompt,
                    ">"
                );
            }
        }

        private bool TryTriggerExpansion()
        {
            if (_expansionMgr != null)
            {
                var snapshot = _textView.TextBuffer.CurrentSnapshot;
                var span = new SnapshotSpan(snapshot, new Span(_textView.Caret.Position.BufferPosition.Position - 1, 1));
                var classification = _textView.TextBuffer.GetGeneroClassifier().GetClassificationSpans(span);
                if (classification.Count == 1)
                {
                    var clsSpan = classification.First().Span;
                    var text = classification.First().Span.GetText();

                    TextSpan[] textSpan = new TextSpan[1];
                    textSpan[0].iStartLine = clsSpan.Start.GetContainingLine().LineNumber;
                    textSpan[0].iStartIndex = clsSpan.Start.Position - clsSpan.Start.GetContainingLine().Start;
                    textSpan[0].iEndLine = clsSpan.End.GetContainingLine().LineNumber;
                    textSpan[0].iEndIndex = clsSpan.End.Position - clsSpan.End.GetContainingLine().Start;

                    string expansionPath, title;
                    int hr = _expansionMgr.GetExpansionByShortcut(
                        _expansionClient,
                        VSGeneroConstants.guidGenero4glLanguageServiceGuid,
                        text,
                        GetViewAdapter(),
                        textSpan,
                        1,
                        out expansionPath,
                        out title
                    );
                    if (ErrorHandler.Succeeded(hr))
                    {
                        // hr may be S_FALSE if there are multiple expansions,
                        // so we don't want to InsertNamedExpansion yet. VS will
                        // pop up a selection dialog in this case.
                        if (hr == VSConstants.S_OK)
                        {
                            return ErrorHandler.Succeeded(_expansionClient.InsertNamedExpansion(title, expansionPath, textSpan[0]));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (int)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);

                if (_activeSession != null && !_activeSession.IsDismissed)
                {
                    if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected &&
                        (VSGeneroPackage.Instance.IntellisenseOptions4GLPage.CompletionCommittedBy.IndexOf(ch) != -1 ||
                         (ch == ' ' && VSGeneroPackage.Instance.IntellisenseOptions4GLPage.SpaceCommitsIntellisense)))
                    {
                        // prevent the changed handler from triggering
                        _blockChangeTrigger = true;

                        if (_parentSpan == null)
                            _parentSpan = GetCompletionParentSpan();
                        _activeSession.Commit();
                        if (_parentSpan != null)
                        {
                            RemoveParentSpan(_parentSpan);
                            _parentSpan = null;
                        }
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

            if(_activeSession != null)
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

                                // prevent the changed handler from triggering
                                _blockChangeTrigger = true;

                                if (_parentSpan == null)
                                    _parentSpan = GetCompletionParentSpan();
                                _activeSession.Commit();
                                if (_parentSpan != null)
                                {
                                    RemoveParentSpan(_parentSpan);
                                    _parentSpan = null;
                                }

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
                                // prevent the changed handler from triggering
                                _blockChangeTrigger = true;

                                if (_parentSpan == null)
                                    _parentSpan = GetCompletionParentSpan();
                                _activeSession.Commit();
                                if (_parentSpan != null)
                                {
                                    RemoveParentSpan(_parentSpan);
                                    _parentSpan = null;
                                }
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.BACKSPACE:
                        case VSConstants.VSStd2KCmdID.DELETE:
                        case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                        case VSConstants.VSStd2KCmdID.DELETEWORDRIGHT:
                            int res = _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            if (_activeSession != null)
                            {
                                if (!_activeSession.IsDismissed)
                                {
                                    _activeSession.Filter();
                                }
                                //else
                                //{
                                //    _activeSession = null;
                                //    if((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.BACKSPACE)
                                //    {
                                //        Backspace();
                                //    }
                                //}
                            }
                            return res;
                    }
                }
            }
            else if(_sigHelpSession != null)
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
            else
            {
                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID)
                    {
                        case VSConstants.VSStd2KCmdID.RETURN:
                            if (_expansionMgr != null && _expansionClient.InSession && ErrorHandler.Succeeded(_expansionClient.EndCurrentExpansion(false)))
                            {
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.TAB:
                            if(IsPreviousCharacterWhitespace())
                            {
                                return _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            }
                            if (_expansionMgr != null && _expansionClient.InSession && ErrorHandler.Succeeded(_expansionClient.NextField()))
                            {
                                return VSConstants.S_OK;
                            }
                            if (_textView.Selection.IsEmpty && _textView.Caret.Position.BufferPosition > 0)
                            {
                                if (TryTriggerExpansion())
                                {
                                    return VSConstants.S_OK;
                                }
                                else
                                {
                                    // get the token directly behind the cursor (or maybe on it?)
                                    SnapshotSpan? span = GetPrecedingExpression();
                                    //_textView.Caret.Position.BufferPosition.GetCurrentMemberOrMemberAccess(out span);

                                    if (!span.HasValue)
                                    {
                                        return _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                                    }

                                    string spanText = span.Value.GetText();
                                    if(spanText == null || spanText.Length == 0)
                                        return _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                                    // see if the text is a keyword...if so, don't bother with the public function snippetizer
                                    var tok = Tokens.GetToken(spanText);
                                    if(tok != null || !(char.IsLetter(spanText[0]) || spanText[0] == '_'))
                                        return _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                                    DynamicSnippet dynSnippet = null;
                                    // 1) TODO: first do a lookup internally (i.e. within the VSGenero symbols). We're not doing that right now

                                    var vars = _textView.TextBuffer.CurrentSnapshot.AnalyzeExpression(
                                        _textView.TextBuffer.CurrentSnapshot.CreateTrackingSpan(span.Value.Span, SpanTrackingMode.EdgeInclusive),
                                        false,
                                        _provider._PublicFunctionProvider,
                                        _provider._DatabaseInfoProvider,
                                        _provider._ProgramFileProvider
                                    );
                                    if (vars != null &&
                                       vars.Value != null &&
                                       vars.Value is IFunctionResult)
                                    {
                                        dynSnippet = (vars.Value as IFunctionResult).GetSnippet(spanText);
                                    }
                                    if (dynSnippet == null)
                                    {
                                        // 2) Do a lookup using the PublicFunctionSnippetizer
                                        if (_provider._PublicFunctionSnippetizer != null)
                                            dynSnippet = _provider._PublicFunctionSnippetizer.GetSnippet(spanText, _textView.TextBuffer);
                                    }
                                    if (dynSnippet != null)
                                    {
                                        // Set the location where the snippet will be inserted
                                        int startLine, startColumn, endLine, endColumn;
                                        _vsTextView.GetCaretPos(out startLine, out endColumn);
                                        startColumn = endColumn - spanText.Length;
                                        endLine = startLine;

                                        MSXML.DOMDocument domDoc = new MSXML.DOMDocument();
                                        domDoc.loadXML(SnippetGenerator.GenerateSnippetXml(dynSnippet));
                                        MSXML.IXMLDOMNode node = domDoc;
                                        InsertCodeExpansion(node, startLine, startColumn, endLine, endColumn);
                                        return VSConstants.S_OK;
                                    }
                                }
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.BACKTAB:
                            if (_expansionMgr != null && _expansionClient.InSession && ErrorHandler.Succeeded(_expansionClient.PreviousField()))
                            {
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.SURROUNDWITH:
                        case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
                            TriggerSnippet(nCmdID);
                            return VSConstants.S_OK;
                    }
                }
            }

            return _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private static bool IsIdentifierChar(char ch)
        {
            return ch == '_' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');// || (ch >= '0' && ch <= '9');
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
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.SURROUNDWITH:
                        case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
                            if (_expansionMgr != null)
                            {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                                return VSConstants.S_OK;
                            }
                            break;
                    }
                }
            }
            return _oldTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private void InsertCodeExpansion(MSXML.IXMLDOMNode customSnippet, int startLine, int startColumn, int endLine, int endColumn)
        {
            // Insert the selected code snippet and start an expansion session
            IVsTextLines buffer;
            _vsTextView.GetBuffer(out buffer);

            // Get the IVsExpansion from the current IVsTextLines
            IVsExpansion vsExpansion = (IVsExpansion)buffer;

            vsExpansion.InsertSpecificExpansion(
                customSnippet,
                new TextSpan { iStartIndex = startColumn, iEndIndex = endColumn, iEndLine = endLine, iStartLine = startLine },
                _expansionClient,
                VSGeneroConstants.guidGenero4glLanguageServiceGuid,
                null,
                out _expansionSession);
        }

        #endregion
    }
}
