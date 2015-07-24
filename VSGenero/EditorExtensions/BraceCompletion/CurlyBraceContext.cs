using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.BraceCompletion
{
    internal class CurlyBraceContext : NormalCompletionContext
    {
        // Methods
        public CurlyBraceContext(Genero4glLanguageInfo languageInfo)
            : base(languageInfo)
        {
        }

        private void Format(IBraceCompletionSession session)
        {
            SnapshotSpan sessionSpan = session.GetSessionSpan();
            if (!base.LanguageInfo.TryFormat(sessionSpan))
            {
                string str;
                ITextSnapshotLine containingLine = sessionSpan.End.GetContainingLine();
                if (base.LanguageInfo.TryGetLineIndentation(containingLine, session.TextView.Options, out str))
                {
                    using (ITextEdit edit = session.SubjectBuffer.CreateEdit())
                    {
                        edit.Replace(Span.FromBounds((int)containingLine.Start, sessionSpan.End.Position - 1), str);
                        edit.Apply();
                    }
                }
            }
        }

        public override void OnReturn(IBraceCompletionSession session)
        {
            if (session.ContainsOnlyWhitespace())
            {
                using (ITextEdit edit = session.SubjectBuffer.CreateEdit(EditOptions.DefaultMinimalChange, null, null))
                {
                    edit.Insert(session.ClosingPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot) - 1, Environment.NewLine);
                    edit.Apply();
                }
                this.Format(session);
                this.SetCaretPosition(session);
            }
        }

        private void SetCaretPosition(IBraceCompletionSession session)
        {
            int num;
            ITextSnapshot currentSnapshot = session.SubjectBuffer.CurrentSnapshot;
            ITextSnapshotLine containingLine = session.OpeningPoint.GetPoint(currentSnapshot).GetContainingLine();
            ITextSnapshotLine lineFromLineNumber = currentSnapshot.GetLineFromLineNumber(containingLine.LineNumber + 1);
            if (!base.LanguageInfo.TryGetLineIndentation(lineFromLineNumber, out num))
            {
                session.MoveCaretTo(lineFromLineNumber.End, 0);
            }
            else
            {
                session.MoveCaretTo(lineFromLineNumber.End, Math.Max(0, num - lineFromLineNumber.Length));
            }
        }

        public override void Start(IBraceCompletionSession session)
        {
            base.LanguageInfo.TryFormat(session.GetSessionSpan());
        }
    }


}
