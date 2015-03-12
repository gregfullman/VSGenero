using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing.AST;

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

            var vars = _textBuffer.CurrentSnapshot.AnalyzeExpression(
                session.CreateTrackingSpan(_textBuffer),
                false
            );

            AugmentQuickInfoWorker(vars, quickInfoContent, out applicableToSpan);
        }

        private void CurSessionDismissed(object sender, EventArgs e)
        {
            _curSession = null;
        }

        internal static void AugmentQuickInfoWorker(ExpressionAnalysis exprAnalysis, System.Collections.Generic.IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
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
                quickInfoContent.Add(val.Documentation);
            }
        }

        #endregion

        public void Dispose()
        {
        }

    }
}
