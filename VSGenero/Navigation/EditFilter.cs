/* ****************************************************************************
 * Copyright (c) 2014 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;

using VSGenero.EditorExtensions.Intellisense;
using VSGenero.EditorExtensions;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.VSCommon;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace VSGenero.Navigation
{
    public class GoToDefinitionLocation
    {
        public string Filename { get; set; }
        public int Position { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
    }

    internal sealed class EditFilter : IOleCommandTarget
    {
        private readonly ITextView _textView;
        private readonly IEditorOperations _editorOps;
        private IOleCommandTarget _next;
        private GeneroLexer _lexer;
        internal static EditFilter Instance { get; private set; }

        public EditFilter(ITextView textView, IEditorOperations editorOps)
        {
            _textView = textView;
            _textView.Properties[typeof(EditFilter)] = this;
            _editorOps = editorOps;
            _lexer = new GeneroLexer();
            Instance = this;
        }

        internal void AttachKeyboardFilter(IVsTextView vsTextView)
        {
            if (_next == null)
            {
                ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out _next));
            }
        }

        public GoToDefinitionLocation GetGoToLocationDefinition(ITextView textView, GeneroLexer lexer)
        {
            GoToDefinitionLocation goToLocation = null;
            int absoluteCaretPos = textView.Caret.Position.BufferPosition.Position;
            int lineStartPos = textView.Caret.ContainingTextViewLine.Start.Position;
            int relativeCaretPos = absoluteCaretPos - lineStartPos;
            if (relativeCaretPos >= 0)
            {
                string lineText = textView.Caret.ContainingTextViewLine.Extent.GetText();

                // Have the lexer process this line
                bool tokenFound = false;
                lexer.StartLexing(0, lineText);
                GeneroToken currentTok = null, prevTok = null;
                do
                {
                    prevTok = currentTok;
                    currentTok = lexer.NextToken();

                    // TODO: need to account for the caret being in whitespace between two tokens
                    if (currentTok.StartPosition <= relativeCaretPos &&
                       currentTok.EndPosition >= relativeCaretPos)
                    {
                        tokenFound = true;
                        break;
                    }
                }
                while (currentTok.TokenType != GeneroTokenType.Unknown && currentTok.TokenType != GeneroTokenType.Eof);

                if (tokenFound)
                {
                    string tokenText = currentTok.TokenText.ToLower();
                    // ok, now the currentTok has the token the caret was on. Now find it!
                    GeneroFileParserManager fpm;
                    if (textView.TextBuffer.Properties.TryGetProperty<GeneroFileParserManager>(typeof(GeneroFileParserManager), out fpm))
                    {
                        GeneroModuleContents programContents;
                        VSGeneroPackage.Instance.ProgramContentsManager.Programs.TryGetValue(textView.TextBuffer.GetProgram(), out programContents);

                        EditorExtensions.VariableDefinition varDef = null;
                        FunctionDefinition funcDef = null;
                        CursorPreparation cursorPrep = null;
                        ConstantDefinition constDef = null;
                        TypeDefinition typeDef = null;
                        // 1) first see if the caret is on a function name                                                                                                                 
                        if (!fpm.ModuleContents.FunctionDefinitions.TryGetValue(tokenText, out funcDef) &&
                            (programContents != null && !programContents.FunctionDefinitions.TryGetValue(tokenText, out funcDef)))
                        {
                            // 1a) look for a cursor
                            CursorDeclaration cursorDecl;
                            string searchName = tokenText;
                            if (fpm.ModuleContents.SqlCursors.TryGetValue(searchName, out cursorDecl))
                            {
                                // TODO: will have to rework this a bit when we support other types of cursor declarations
                                searchName = cursorDecl.PreparationVariable.ToLower();
                            }

                            if (!fpm.ModuleContents.SqlPrepares.TryGetValue(searchName, out cursorPrep))
                            {
                                // 2) determine what function we're in, and see if we're on a local variable
                                FunctionDefinition tmpFunc = IntellisenseExtensions.DetermineContainingFunction(absoluteCaretPos, fpm);
                                if (tmpFunc != null)
                                {
                                    if (!tmpFunc.Variables.TryGetValue(tokenText, out varDef))
                                        if (!tmpFunc.Constants.TryGetValue(tokenText, out constDef))
                                            tmpFunc.Types.TryGetValue(tokenText, out typeDef);
                                }
                                if (varDef == null && constDef == null && typeDef == null)
                                {
                                    // look at module variables
                                    if (!fpm.ModuleContents.ModuleVariables.TryGetValue(tokenText, out varDef))
                                        // look at global variables
                                        if (!fpm.ModuleContents.GlobalVariables.TryGetValue(tokenText, out varDef) &&
                                            (programContents != null && !programContents.GlobalVariables.TryGetValue(tokenText, out varDef)))
                                            // look at module constants
                                            if (!fpm.ModuleContents.ModuleConstants.TryGetValue(tokenText, out constDef))
                                                // look at global constants
                                                if (!fpm.ModuleContents.GlobalConstants.TryGetValue(tokenText, out constDef) &&
                                                    (programContents != null && !programContents.GlobalConstants.TryGetValue(tokenText, out constDef)))
                                                    // look at module types
                                                    if (!fpm.ModuleContents.ModuleTypes.TryGetValue(tokenText, out typeDef))
                                                        // look at global types
                                                        if (!fpm.ModuleContents.GlobalTypes.TryGetValue(tokenText, out typeDef))
                                                            if (programContents != null)
                                                                programContents.GlobalTypes.TryGetValue(tokenText, out typeDef);
                                }
                            }
                        }

                        if (varDef != null)
                        {
                            goToLocation = new GoToDefinitionLocation { Filename = varDef.ContainingFile, Position = varDef.Position, ColumnNumber = varDef.ColumnNumber, LineNumber = varDef.LineNumber };
                            return goToLocation;
                        }
                        else if (constDef != null)
                        {
                            goToLocation = new GoToDefinitionLocation { Filename = constDef.ContainingFile, Position = constDef.Position, ColumnNumber = constDef.ColumnNumber, LineNumber = constDef.LineNumber };
                            return goToLocation;
                        }
                        else if (typeDef != null)
                        {
                            goToLocation = new GoToDefinitionLocation { Filename = typeDef.ContainingFile, Position = typeDef.Position, ColumnNumber = typeDef.ColumnNumber, LineNumber = typeDef.LineNumber };
                            return goToLocation;
                        }
                        else if (funcDef != null)
                        {
                            goToLocation = new GoToDefinitionLocation { Filename = funcDef.ContainingFile, Position = funcDef.Position, ColumnNumber = funcDef.ColumnNumber, LineNumber = funcDef.LineNumber };
                            return goToLocation;
                        }
                        else if (cursorPrep != null)
                        {
                            goToLocation = new GoToDefinitionLocation { Filename = cursorPrep.ContainingFile, Position = cursorPrep.Position, ColumnNumber = cursorPrep.ColumnNumber, LineNumber = cursorPrep.LineNumber };
                            return goToLocation;
                        }
                    }
                    if (GeneroClassifierProvider.Instance.PublicFunctionNavigator != null)
                    {
                        goToLocation = GeneroClassifierProvider.Instance.PublicFunctionNavigator.GetPublicFunctionLocation(tokenText, textView.TextBuffer);
                        if (goToLocation != null)
                            return goToLocation;
                    }
                    VSGeneroPackage.Instance.ShowDialog("Definition not found", string.Format("The definition of \"{0}\" could not be found.", tokenText));
                    return goToLocation;
                }
                VSGeneroPackage.Instance.ShowDialog("Definition not found", "Unable to determine what token the cursor is on.");
            }
            return goToLocation;
        }

        /// <summary>
        /// Implements Goto Definition.  Called when the user selects Goto Definition from the 
        /// context menu or hits the hotkey associated with Goto Definition.
        /// 
        /// If there is 1 and only one definition immediately navigates to it.  If there are
        /// no references displays a dialog box to the user.  Otherwise it opens the find
        /// symbols dialog with the list of results.
        /// </summary>
        private int GotoDefinition()
        {
            GoToDefinitionLocation location = GetGoToLocationDefinition(_textView, _lexer);
            if(location != null)
            {
                GotoLocation(location);
            }
            return VSConstants.S_OK;

            //UpdateStatusForIncompleteAnalysis();

            //var analysis = _textView.GetExpressionAnalysis();

            //Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
            //GetDefsRefsAndValues(analysis, out definitions, out references, out values);

            //if ((values.Count + definitions.Count) == 1)
            //{
            //    if (values.Count != 0)
            //    {
            //        foreach (var location in values.Keys)
            //        {
            //            GotoLocation(location);
            //            break;
            //        }
            //    }
            //    else
            //    {
            //        foreach (var location in definitions.Keys)
            //        {
            //            GotoLocation(location);
            //            break;
            //        }
            //    }
            //}
            //else if (values.Count + definitions.Count == 0)
            //{
            //    if (String.IsNullOrWhiteSpace(analysis.Expression))
            //    {
            //        MessageBox.Show(String.Format("Cannot go to definition.  The cursor is not on a symbol."), "Python Tools for Visual Studio");
            //    }
            //    else
            //    {
            //        MessageBox.Show(String.Format("Cannot go to definition \"{0}\"", analysis.Expression), "Python Tools for Visual Studio");
            //    }
            //}
            //else if (definitions.Count == 0)
            //{
            //    ShowFindSymbolsDialog(analysis, new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values));
            //}
            //else if (values.Count == 0)
            //{
            //    ShowFindSymbolsDialog(analysis, new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values));
            //}
            //else
            //{
            //    ShowFindSymbolsDialog(analysis,
            //        new LocationCategory("Goto Definition",
            //            new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values),
            //            new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values)
            //        )
            //    );
            //}
        }

        /// <summary>
        /// Moves the caret to the specified location, staying in the current text view 
        /// if possible.
        /// </summary>
        private void GotoLocation(GoToDefinitionLocation location)
        {
            //Debug.Assert(location != null);
            //Debug.Assert(location.Line > 0);
            //Debug.Assert(location.Column > 0);

            if (VSCommonExtensions.IsSamePath(location.Filename, _textView.GetFilePath()))
            {
                int line, col;
                var adapterFactory = VSGeneroPackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
                var viewAdapter = adapterFactory.GetViewAdapter(_textView);
                viewAdapter.GetLineAndColumn(location.Position, out line, out col);
                viewAdapter.SetCaretPos(line, col);
                viewAdapter.CenterLines(line, 1);
            }
            else
            {
                VSGeneroPackage.NavigateTo(location.Filename, Guid.Empty, location.Position);
            }
        }

        /// <summary>
        /// Implements Find All References.  Called when the user selects Find All References from
        /// the context menu or hits the hotkey associated with find all references.
        /// 
        /// Always opens the Find Symbol Results box to display the results.
        /// </summary>
        private int FindAllReferences()
        {
            //UpdateStatusForIncompleteAnalysis();

            //var analysis = _textView.GetExpressionAnalysis();

            //var locations = GetFindRefLocations(analysis);

            //ShowFindSymbolsDialog(analysis, locations);

            return VSConstants.S_OK;
        }

        //internal static LocationCategory GetFindRefLocations(ExpressionAnalysis analysis)
        //{
        //    Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
        //    GetDefsRefsAndValues(analysis, out definitions, out references, out values);

        //    var locations = new LocationCategory("Find All References",
        //            new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values),
        //            new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values),
        //            new SymbolList("References", StandardGlyphGroup.GlyphReference, references.Values)
        //        );
        //    return locations;
        //}

        /// <summary>
        /// Opens the find symbols dialog with a list of results.  This is done by requesting
        /// that VS does a search against our library GUID.  Our library then responds to
        /// that request by extracting the prvoided symbol list out and using that for the
        /// search results.
        /// </summary>
        //private static void ShowFindSymbolsDialog(ExpressionAnalysis provider, IVsNavInfo symbols)
        //{
        //    // ensure our library is loaded so find all references will go to our library
        //    Package.GetGlobalService(typeof(IPythonLibraryManager));

        //    if (provider.Expression != "")
        //    {
        //        var findSym = (IVsFindSymbol)PythonToolsPackage.GetGlobalService(typeof(SVsObjectSearch));
        //        VSOBSEARCHCRITERIA2 searchCriteria = new VSOBSEARCHCRITERIA2();
        //        searchCriteria.eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD;
        //        searchCriteria.pIVsNavInfo = symbols;
        //        searchCriteria.grfOptions = (uint)_VSOBSEARCHOPTIONS2.VSOBSO_LISTREFERENCES;
        //        searchCriteria.szName = provider.Expression;

        //        Guid guid = Guid.Empty;
        //        //  new Guid("{a5a527ea-cf0a-4abf-b501-eafe6b3ba5c6}")
        //        ErrorHandler.ThrowOnFailure(findSym.DoSearch(new Guid(CommonConstants.LibraryGuid), new VSOBSEARCHCRITERIA2[] { searchCriteria }));
        //    }
        //    else
        //    {
        //        var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
        //        statusBar.SetText("The caret must be on valid expression to find all references.");
        //    }
        //}

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // preprocessing
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                switch ((VSConstants.VSStd97CmdID)nCmdID)
                {
                    case VSConstants.VSStd97CmdID.Paste:
                        //PythonReplEvaluator eval;
                        //if (_textView.Properties.TryGetProperty(typeof(PythonReplEvaluator), out eval))
                        //{
                        //    string pasting = eval.FormatClipboard() ?? Clipboard.GetText();
                        //    if (pasting != null)
                        //    {
                        //        PasteReplCode(eval, pasting);

                        //        return VSConstants.S_OK;
                        //    }
                        //}
                        //else
                        //{
                        //    string updated = RemoveReplPrompts(_textView.Options.GetNewLineCharacter());
                        //    if (updated != null)
                        //    {
                        //        _editorOps.ReplaceSelection(updated);
                        //        return VSConstants.S_OK;
                        //    }
                        //}
                        break;
                    case VSConstants.VSStd97CmdID.GotoDefn:
                        //VSGeneroPackage.Instance.ShowDialog("Functionality Not Supported", "Go to definition is not yet supported.");
                        return GotoDefinition();
                    case VSConstants.VSStd97CmdID.FindReferences:
                        VSGeneroPackage.Instance.ShowDialog("Functionality Not Supported", "Find all references is not yet supported.");
                        return FindAllReferences();
                }
            }
            else if (pguidCmdGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                Genero4GLOutliner tagger;
                switch ((VSConstants.VSStd2KCmdID)nCmdID)
                {
                    case (VSConstants.VSStd2KCmdID)147: // ECMD_SMARTTASKS  defined in stdidcmd.h, but not in MPF
                        // if the user is typing to fast for us to update the smart tags on the idle event
                        // then we want to update them before VS pops them up.
                        //UpdateSmartTags();
                        break;
                    case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                        VSGeneroPackage.Instance.ShowDialog("Functionality Not Supported", "Document formatting is not yet supported.");
                        //FormatCode(new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, 0, _textView.TextBuffer.CurrentSnapshot.Length), false);
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                        VSGeneroPackage.Instance.ShowDialog("Functionality Not Supported", "Selection formatting is not yet supported.");
                        //FormatCode(_textView.Selection.StreamSelectionSpan.SnapshotSpan, true);
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                        VSGeneroPackage.Instance.ShowDialog("Functionality Not Supported", "Member list is not yet supported.");
                        break;
                    //case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                    //    var controller = _textView.Properties.GetProperty<IntellisenseController>(typeof(IntellisenseController));
                    //    if (controller != null)
                    //    {
                    //        IntellisenseController.ForceCompletions = true;
                    //        try
                    //        {
                    //            controller.TriggerCompletionSession((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.COMPLETEWORD);
                    //        }
                    //        finally
                    //        {
                    //            IntellisenseController.ForceCompletions = false;
                    //        }
                    //        return VSConstants.S_OK;
                    //    }
                    //    break;

                    //case VSConstants.VSStd2KCmdID.QUICKINFO:
                    //    controller = _textView.Properties.GetProperty<IntellisenseController>(typeof(IntellisenseController));
                    //    if (controller != null)
                    //    {
                    //        controller.TriggerQuickInfo();
                    //        return VSConstants.S_OK;
                    //    }
                    //    break;

                    //case VSConstants.VSStd2KCmdID.PARAMINFO:
                    //    controller = _textView.Properties.GetProperty<IntellisenseController>(typeof(IntellisenseController));
                    //    if (controller != null)
                    //    {
                    //        controller.TriggerSignatureHelp();
                    //        return VSConstants.S_OK;
                    //    }
                    //    break;

                    case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                        //tagger = _textView.GetOutliningTagger();
                        //if (tagger != null)
                        //{
                        //    tagger.Disable();
                        //}
                        // let VS get the event as well
                        break;

                    case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                        //tagger = _textView.GetOutliningTagger();
                        //if (tagger != null)
                        //{
                        //    tagger.Enable();
                        //}
                        // let VS get the event as well
                        break;

                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        if (EditorExtensions.EditorExtensions.CommentOrUncommentBlock(_textView, true))
                        {
                            return VSConstants.S_OK;
                        }
                        break;

                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        if (EditorExtensions.EditorExtensions.CommentOrUncommentBlock(_textView, false))
                        {
                            return VSConstants.S_OK;
                        }
                        break;
                    //case VSConstants.VSStd2KCmdID.EXTRACTMETHOD:
                    //    //ExtractMethod();
                    //    return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.RENAME:
                        VSGeneroPackage.Instance.ShowDialog("Functionality Not Supported", "Renaming is not yet supported.");
                        //RefactorRename();
                        return VSConstants.S_OK;
                }
            }
//#if DEV12
//            else if (pguidCmdGroup == typeof(VSConstants.VSStd12CmdID).GUID)
//            {
//                switch ((VSConstants.VSStd12CmdID)nCmdID)
//                {
//                    case VSConstants.VSStd12CmdID.PeekDefinition:
//                        {
//                            VSGeneroPackage.Instance.ShowDialog("Functionality Not Supported", "Peek definition is not yet supported.");
//                            return VSConstants.S_OK;
//                        }
//                }
//            }
//#endif
            //else if (pguidCmdGroup == GuidList.guidPythonToolsCmdSet)
            //{
            //    switch (nCmdID)
            //    {
            //        case PkgCmdIDList.cmdidRefactorRenameIntegratedShell:
            //            RefactorRename();
            //            return VSConstants.S_OK;
            //        case PkgCmdIDList.cmdidExtractMethodIntegratedShell:
            //            ExtractMethod();
            //            return VSConstants.S_OK;
            //        default:
            //            lock (PythonToolsPackage.CommandsLock)
            //            {
            //                foreach (var command in PythonToolsPackage.Commands.Keys)
            //                {
            //                    if (command.CommandId == nCmdID)
            //                    {
            //                        command.DoCommand(this, EventArgs.Empty);
            //                        return VSConstants.S_OK;
            //                    }
            //                }
            //            }
            //            break;
            //    }

            //}

            return _next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private const uint CommandDisabledAndHidden = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU);

        /// <summary>
        /// Called from VS to see what commands we support.  
        /// </summary>
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd97CmdID.FindReferences:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            //            else if (pguidCmdGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            //            {
            //                for (int i = 0; i < cCmds; i++)
            //                {
            //                    switch (prgCmds[i].cmdID)
            //                    {
            //                        case PkgCmdIDList.cmdidRefactorRenameIntegratedShell:
            //                            // C# provides the refactor context menu for the main VS command outside
            //                            // of the integrated shell.  In the integrated shell we provide our own
            //                            // command for it so these still show up.
            ////#if DEV10
            ////                                        if (!IsCSharpInstalled()) {
            ////                                            QueryStatusRename(prgCmds, i);
            ////                                        } else 
            ////#endif
            ////                            {
            ////                                prgCmds[i].cmdf = CommandDisabledAndHidden;
            ////                            }
            //                            QueryStatusRename(prgCmds, i);
            //                            return VSConstants.S_OK;
            //                    }
            //                }
            //            }
            //                        case PkgCmdIDList.cmdidRefactorRenameIntegratedShell:
            //                            // C# provides the refactor context menu for the main VS command outside
            //                            // of the integrated shell.  In the integrated shell we provide our own
            //                            // command for it so these still show up.
            //#if DEV10
            //                            if (!IsCSharpInstalled()) {
            //                                QueryStatusRename(prgCmds, i);
            //                            } else 
            //#endif
            //                            {
            //                                prgCmds[i].cmdf = CommandDisabledAndHidden;
            //                            }
            //                            return VSConstants.S_OK;
            //                        case PkgCmdIDList.cmdidExtractMethodIntegratedShell:
            //                            // C# provides the refactor context menu for the main VS command outside
            //                            // of the integrated shell.  In the integrated shell we provide our own
            //                            // command for it so these still show up.
            //#if DEV10
            //                            if (!IsCSharpInstalled()) {
            //                                QueryStatusExtractMethod(prgCmds, i);
            //                            } else 
            //#endif
            //                            {
            //                                prgCmds[i].cmdf = CommandDisabledAndHidden;
            //                            }
            //                            return VSConstants.S_OK;
            //                        default:
            //                            lock (PythonToolsPackage.CommandsLock)
            //                            {
            //                                foreach (var command in PythonToolsPackage.Commands.Keys)
            //                                {
            //                                    if (command.CommandId == prgCmds[i].cmdID)
            //                                    {
            //                                        int? res = command.EditFilterQueryStatus(ref prgCmds[i], pCmdText);
            //                                        if (res != null)
            //                                        {
            //                                            return res.Value;
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                            break;
            //                    }
            //                }
            //            }
            else if (pguidCmdGroup == VSGeneroConstants.Std2KCmdGroupGuid)
            {
                Genero4GLOutliner tagger;
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                        case VSConstants.VSStd2KCmdID.FORMATSELECTION:

                        case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                        case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        case VSConstants.VSStd2KCmdID.QUICKINFO:
                        case VSConstants.VSStd2KCmdID.PARAMINFO:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;

                        //case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
                        //    tagger = _textView.GetOutliningTagger();
                        //    if (tagger != null && tagger.Enabled)
                        //    {
                        //        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                        //    }
                        //    return VSConstants.S_OK;

                        //case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
                        //    tagger = _textView.GetOutliningTagger();
                        //    if (tagger != null && !tagger.Enabled)
                        //    {
                        //        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                        //    }
                        //    return VSConstants.S_OK;

                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        //case VSConstants.VSStd2KCmdID.EXTRACTMETHOD:
                        //    QueryStatusExtractMethod(prgCmds, i);
                        //    return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.RENAME:
                            QueryStatusRename(prgCmds, i);
                            return VSConstants.S_OK;
                    }
                }
            }
#if DEV12
            else if (pguidCmdGroup == typeof(VSConstants.VSStd12CmdID).GUID)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd12CmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd12CmdID.PeekDefinition:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
#endif
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private void QueryStatusRename(OLECMD[] prgCmds, int i)
        {
            IWpfTextView activeView = VSGeneroPackage.GetActiveTextView();
            if (VSGeneroConstants.IsGenero4GLContent(_textView.TextBuffer))
            {
                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
            }
            else
            {
                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE);
            }
        }

        internal void DoIdle(IOleComponentManager compMgr)
        {
            // TODO:
            //UpdateSmartTags(compMgr);
        }
    }
}
