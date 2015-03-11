using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

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
                options.FilterCompletions = false; // VSGeneroPackage.Instance.AdvancedOptions4GLPage.FilterCompletions;
                options.SearchMode = FuzzyMatchMode.Default;// VSGeneroPackage.Instance.AdvancedOptions4GLPage.SearchMode;
            }
            return options;
        }
    }

    class CompletionSource : ICompletionSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly CompletionSourceProvider _provider;

        public CompletionSource(CompletionSourceProvider provider, ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _provider = provider;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var textBuffer = _textBuffer;
            var span = session.GetApplicableSpan(textBuffer);
            var triggerPoint = session.GetTriggerPoint(textBuffer);
            var options = session.GetOptions();
            var provider = textBuffer.CurrentSnapshot.GetCompletions(span, triggerPoint, options);

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
