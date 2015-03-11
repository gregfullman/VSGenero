using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;

namespace VSGenero.EditorExtensions.Intellisense
{
    class SnapshotCookie : IAnalysisCookie
    {
        private readonly ITextSnapshot _snapshot;

        public SnapshotCookie(ITextSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public ITextSnapshot Snapshot
        {
            get
            {
                return _snapshot;
            }
        }

        #region IAnalysisCookie Members

        public string GetLine(int lineNo)
        {
            return _snapshot.GetLineFromLineNumber(lineNo - 1).GetText();
        }

        #endregion
    }
}
