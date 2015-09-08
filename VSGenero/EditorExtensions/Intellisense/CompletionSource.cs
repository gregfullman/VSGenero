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
                options.AnalysisType = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.AnalysisType;
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
            completionSets.Add(completions);
        }


        public void Dispose()
        {
        }
    }
}
