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

        internal static void AugmentQuickInfoWorker(ExpressionAnalysis vars, System.Collections.Generic.IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = vars.Span;
            if (applicableToSpan == null || String.IsNullOrWhiteSpace(vars.Expression))
            {
                return;
            }

            bool first = true;
            var result = new StringBuilder();
            int count = 0;
            List<AstNode> listVars = new List<AstNode>(vars.Values);
            HashSet<string> descriptions = new HashSet<string>();
            bool multiline = false;
            foreach (var v in listVars)
            {
                string description = null;
                if (listVars.Count == 1)
                {
                    if (!String.IsNullOrWhiteSpace(v.Description))
                    {
                        description = v.Description;
                    }
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(v.ShortDescription))
                    {
                        description = v.ShortDescription;
                    }
                }

                description = description.LimitLines();

                if (description != null && descriptions.Add(description))
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        if (result.Length == 0 || result[result.Length - 1] != '\n')
                        {
                            result.Append(", ");
                        }
                        else
                        {
                            multiline = true;
                        }
                    }
                    result.Append(description);
                    count++;
                }
            }

            string expr = vars.Expression;
            if (expr.Length > 4096)
            {
                expr = expr.Substring(0, 4093) + "...";
            }
            if (multiline)
            {
                result.Insert(0, expr + ": " + Environment.NewLine);
            }
            else if (result.Length > 0)
            {
                result.Insert(0, expr + ": ");
            }
            else
            {
                result.Append(expr);
                result.Append(": ");
                result.Append("<unknown type>");
            }

            quickInfoContent.Add(result.ToString());
        }

        #endregion

        public void Dispose()
        {
        }

    }
}
