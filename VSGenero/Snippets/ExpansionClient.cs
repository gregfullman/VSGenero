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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using VSGenero.EditorExtensions;

namespace VSGenero.Snippets
{
    class ExpansionClient : IVsExpansionClient
    {
        private readonly IVsTextLines _lines;
        private readonly IVsExpansion _expansion;
        private readonly IVsTextView _view;
        private readonly ITextView _textView;
        private readonly IVsEditorAdaptersFactoryService _adapterFactory;
        private readonly IServiceProvider _serviceProvider;
        private IVsExpansionSession _session;
        private bool _sessionEnded, _selectEndSpan;
        private ITrackingPoint _selectionStart, _selectionEnd;
        private IEditorOptions _options;
        private ITrackingPoint _endCaretPoint;

        public const string SurroundsWith = "SurroundsWith";
        public const string Expansion = "Expansion";

        public ExpansionClient(ITextView textView, IVsEditorAdaptersFactoryService adapterFactory, IServiceProvider serviceProvider)
        {
            _textView = textView;
            _serviceProvider = serviceProvider;
            _adapterFactory = adapterFactory;
            _view = _adapterFactory.GetViewAdapter(_textView);
            _lines = (IVsTextLines)_adapterFactory.GetBufferAdapter(_textView.TextBuffer);
            _expansion = _lines as IVsExpansion;
            _options = textView.Options;
            if (_expansion == null)
            {
                throw new ArgumentException("TextBuffer does not support expansions");
            }
        }

        public bool InSession
        {
            get
            {
                return _session != null;
            }
        }

        public int EndExpansion()
        {
            _session = null;
            _sessionEnded = true;
            _selectionStart = _selectionEnd = null;
            return VSConstants.S_OK;
        }

        public int FormatSpan(IVsTextLines pBuffer, TextSpan[] ts)
        {
            MSXML.IXMLDOMNode codeNode, snippetTypes;

            int hr = VSConstants.S_OK;
            if (ErrorHandler.Failed(hr = _session.GetSnippetNode("CodeSnippet:Code", out codeNode)) || codeNode == null)
            {
                return hr;
            }

            if (ErrorHandler.Failed(hr = _session.GetHeaderNode("CodeSnippet:SnippetTypes", out snippetTypes)) || snippetTypes == null)
            {
                return hr;
            }

            bool surroundsWith = false;
            foreach (MSXML.IXMLDOMNode snippetType in snippetTypes.childNodes)
            {
                if (snippetType.nodeName == "SnippetType")
                {
                    if (snippetType.text == SurroundsWith)
                    {
                        surroundsWith = true;
                        break;
                    }
                }
            }

            using (var edit = _textView.TextBuffer.CreateEdit())
            {
                Span? endSpan = null;
                if (surroundsWith)
                {
                    var templateText = codeNode.text.Replace("\n", _textView.Options.GetNewLineCharacter());
                    templateText = templateText.Replace("$end$", "");

                    // we can finally figure out where the selected text began witin the original template...
                    int selectedIndex = templateText.IndexOf("$selected$");
                    if (selectedIndex != -1)
                    {
                        var start = _selectionStart.GetPosition(_textView.TextBuffer.CurrentSnapshot);
                        var end = _selectionEnd.GetPosition(_textView.TextBuffer.CurrentSnapshot);
                        if (end < start)
                        {
                            end = start;
                        }
                        endSpan = Span.FromBounds(start, end);
                    }
                }

                _endCaretPoint = null;
                bool setCaretPoint = endSpan.HasValue && endSpan.Value.Start != endSpan.Value.End;
                if (setCaretPoint)
                    _endCaretPoint = _textView.TextBuffer.CurrentSnapshot.CreateTrackingPoint(endSpan.Value.End, PointTrackingMode.Positive);

                // we now need to update any code which was not selected  that we just inserted.
                AutoIndent.Format(_textView, edit, ts[0].iStartLine, ts[0].iEndLine, false, !setCaretPoint);

                edit.Apply();
            }
            return hr;
        }

        public int GetExpansionFunction(MSXML.IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction pFunc)
        {
            pFunc = null;
            return VSConstants.S_OK;
        }

        public int IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind)
        {
            pfIsValidKind = 1;
            return VSConstants.S_OK;
        }

        public int IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType)
        {
            pfIsValidType = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeInsertion(IVsExpansionSession pSession)
        {
            _session = pSession;
            return VSConstants.S_OK;
        }

        public int OnItemChosen(string pszTitle, string pszPath)
        {
            int caretLine, caretColumn;
            GetCaretPosition(out caretLine, out caretColumn);

            var textSpan = new TextSpan() { iStartLine = caretLine, iStartIndex = caretColumn, iEndLine = caretLine, iEndIndex = caretColumn };
            return InsertNamedExpansion(pszTitle, pszPath, textSpan);
        }

        public int InsertNamedExpansion(string pszTitle, string pszPath, TextSpan textSpan)
        {
            if (_session != null)
            {
                // if the user starts an expansion session while one is in progress
                // then abort the current expansion session
                _session.EndCurrentExpansion(1);
                _session = null;
            }

            var selection = _textView.Selection;
            var snapshot = selection.Start.Position.Snapshot;

            _selectionStart = snapshot.CreateTrackingPoint(selection.Start.Position, Microsoft.VisualStudio.Text.PointTrackingMode.Positive);
            _selectionEnd = snapshot.CreateTrackingPoint(selection.End.Position, Microsoft.VisualStudio.Text.PointTrackingMode.Negative);
            _selectEndSpan = _sessionEnded = false;

            int hr = _expansion.InsertNamedExpansion(
                pszTitle,
                pszPath,
                textSpan,
                this,
                VSGeneroConstants.guidGenero4glLanguageServiceGuid,
                0,
                out _session
            );

            if (ErrorHandler.Succeeded(hr))
            {
                if (_sessionEnded)
                {
                    _session = null;
                }
            }
            return hr;
        }

        public int NextField()
        {
            return _session.GoToNextExpansionField(0);
        }

        public int PreviousField()
        {
            return _session.GoToPreviousExpansionField();
        }

        public int EndCurrentExpansion(bool leaveCaret)
        {
            if (_endCaretPoint != null)
            {
                _textView.Selection.Clear();
                _textView.Caret.MoveTo(_endCaretPoint.GetPoint(_textView.TextBuffer.CurrentSnapshot));
                return _session.EndCurrentExpansion(1);
            }
            return _session.EndCurrentExpansion(leaveCaret ? 1 : 0);
        }

        public int PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        private void GetCaretPosition(out int caretLine, out int caretColumn)
        {
            ErrorHandler.ThrowOnFailure(_view.GetCaretPos(out caretLine, out caretColumn));

            // Handle virtual space
            int lineLength;
            ErrorHandler.ThrowOnFailure(_lines.GetLengthOfLine(caretLine, out lineLength));

            if (caretColumn > lineLength)
            {
                caretColumn = lineLength;
            }
        }
    }
}
