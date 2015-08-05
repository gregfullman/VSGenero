using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Refactoring
{
    /// <summary>
    /// Provides inputs/UI to the extract method refactoring.  Enables driving of the refactoring programmatically
    /// or via UI.
    /// </summary>
    interface IRenameVariableInput
    {
        RenameVariableRequest GetRenameInfo(string originalName);

        void CannotRename(string message);

        void ClearRefactorPane();

        void OutputLog(string message);

        ITextBuffer GetBufferForDocument(string filename);

        IVsLinkedUndoTransactionManager BeginGlobalUndo();

        void EndGlobalUndo(IVsLinkedUndoTransactionManager undo);

    }
}
