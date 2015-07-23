using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;

namespace VSGenero.EditorExtensions.Intellisense
{
    public class ContextSensitiveCompletionAnalysis : CompletionAnalysis
    {
        private readonly IEnumerable<MemberResult> _result;

        public ContextSensitiveCompletionAnalysis(IEnumerable<MemberResult> result, ITrackingSpan span, ITextBuffer buffer, CompletionOptions options)
            : base(span, buffer, options)
        {
            _result = result;
        }

        public override CompletionSet GetCompletions(IGlyphService glyphService)
        {
            var result = new FuzzyCompletionSet(
                "Genero",
                "Genero",
                Span,
                _result.Select(m => GeneroCompletion(glyphService, m)),
                _options,
                CompletionComparer.UnderscoresLast);
            return result;
        }
    }

    public class TestCompletionAnalysis : CompletionAnalysis
    {
        private static IEnumerable<MemberResult> _result;

        public TestCompletionAnalysis(ITrackingSpan span, ITextBuffer buffer, CompletionOptions options)
            : base(span, buffer, options)
        {
        }

        public static void InitializeResults()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var list = new List<MemberResult>();

            for(int i = 0; i < 300000; i++)
            {
                list.Add(new MemberResult(new string(
    Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)])
              .ToArray()), Analysis.Parsing.AST.GeneroMemberType.Variable, null));
            }
            _result = list;
        }

        public override CompletionSet GetCompletions(IGlyphService glyphService)
        {
            var result = new FuzzyCompletionSet(
                "Genero",
                "Genero",
                Span,
                _result.Select(m => GeneroCompletion(glyphService, m)),
                _options,
                CompletionComparer.UnderscoresLast);
            return result;
        }
    }
}
