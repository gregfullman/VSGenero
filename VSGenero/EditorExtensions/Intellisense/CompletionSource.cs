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
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.VSCommon;

namespace VSGenero.EditorExtensions.Intellisense
{
    public static class CompletionSessionExtensions
    {
        public static CompletionOptions GetOptions(this ICompletionSession session)
        {
            var options = new CompletionOptions
            {
                ConvertTabsToSpaces = session.TextView.Options.IsConvertTabsToSpacesEnabled(),
                IndentSize = session.TextView.Options.GetIndentSize(),
                TabSize = session.TextView.Options.GetTabSize()
            };

            if (VSGeneroPackage.Instance != null)
            {
                options.IntersectMembers = false;// VSGeneroPackage.Instance.AdvancedOptions4GLPage.IntersectMembers;
                options.HideAdvancedMembers = VSGeneroPackage.Instance.LangPrefs.HideAdvancedMembers;
                // TODO: make this an option
                options.FilterCompletions = true; // false; // VSGeneroPackage.Instance.AdvancedOptions4GLPage.FilterCompletions;
                options.SearchMode = FuzzyMatchMode.Default;// VSGeneroPackage.Instance.AdvancedOptions4GLPage.SearchMode;
            }
            return options;
        }
    }

    class CompletionSource : ICompletionSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly CompletionSourceProvider _provider;
        private ICompletionSession _completionSession;

        public CompletionSource(CompletionSourceProvider provider, ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _provider = provider;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            _completionSession = session;
            var textBuffer = _textBuffer;
            if (_provider._PublicFunctionProvider != null)
                _provider._PublicFunctionProvider.SetFilename(textBuffer.GetFilePath());
            if (_provider._DatabaseInfoProvider != null)
                _provider._DatabaseInfoProvider.SetFilename(textBuffer.GetFilePath());
            if (_provider._ProgramFileProvider != null)
                _provider._ProgramFileProvider.SetFilename(textBuffer.GetFilePath());
            
            var span = session.GetApplicableSpan(textBuffer);
            var triggerPoint = session.GetTriggerPoint(textBuffer);
            var options = session.GetOptions();
            var provider = textBuffer.CurrentSnapshot.GetCompletions(span, triggerPoint, options, _provider._PublicFunctionProvider, _provider._DatabaseInfoProvider, _provider._ProgramFileProvider);
            
            provider.GlyphService = _provider._glyphService;
            var completions = provider.GetCompletions(_provider._glyphService);

            if (completions == null || completions.Completions.Count == 0)
            {
                //if (PythonToolsPackage.Instance != null &&
                //    !session.TextView.GetAnalyzer().InterpreterFactory.IsAnalysisCurrent())
                //{
                //    // no completions, inform the user via the status bar that the analysis is not yet complete.
                //    var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
                //    statusBar.SetText(Resources.WarningAnalysisNotCurrent);
                //}
                return;
            }
            //_textBuffer.Changed += _textBuffer_Changed;
            completionSets.Add(completions);
        }

        //async void _textBuffer_Changed(object sender, TextContentChangedEventArgs e)
        //{
        //    if (_completionSession != null &&
        //        _completionSession.SelectedCompletionSet != null)
        //    {
        //        var text = _completionSession.SelectedCompletionSet.ApplicableTo.GetText(_textBuffer.CurrentSnapshot);
        //        if (text.Length == 2 && e.Before.Length < e.After.Length)   // only trigger after having added a character
        //        {
        //            if (_completionSession.SelectedCompletionSet is FuzzyCompletionSet)
        //            {
        //                // give the completion set a callback that can be run asynchronously to get additional completions
        //                Func<IEnumerable<DynamicallyVisibleCompletion>> callback = () =>
        //                    {
        //                        if(_provider._PublicFunctionProvider != null)
        //                        {
        //                            _provider._PublicFunctionProvider.SetFilename(_textBuffer.GetFilePath());
        //                            var results = _provider._PublicFunctionProvider.GetFunctionsStartingWith(text);
        //                            if(results != null)
        //                            {
        //                                return results.Select(x => CompletionAnalysis.GeneroCompletion(_provider._glyphService, new Analysis.MemberResult(x.Name, Analysis.Parsing.AST.GeneroMemberType.Function, null)));
        //                            }
        //                        }
        //                        return new DynamicallyVisibleCompletion[0];
        //                    };
        //                await (_completionSession.SelectedCompletionSet as FuzzyCompletionSet).RecalculateAsync(callback);
        //            }
        //        }
        //    }
        //    _textBuffer.Changed -= _textBuffer_Changed;
        //}


        public void Dispose()
        {
            //if (_completionSession != null && _textBuffer != null)
            //{
            //    try
            //    {
            //        _textBuffer.Changed -= _textBuffer_Changed;
            //    }
            //    catch (Exception)
            //    { }
            //}
        }
    }
}
