using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions
{
    public interface IGenero4GLTextViewCommandTarget
    {
        Guid PackageGuid { get; }
        bool Exec(string filepath, uint nCmdId);
    }
}
