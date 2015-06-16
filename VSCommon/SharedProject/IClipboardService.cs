using System;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudioTools
{
    public interface IClipboardService
    {
        void SetClipboard(IDataObject dataObject);

        IDataObject GetClipboard();

        void FlushClipboard();

        bool OpenClipboard();

        void EmptyClipboard();

        void CloseClipboard();
    }
}
