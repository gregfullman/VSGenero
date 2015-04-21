using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions
{
    public interface IGeneroTextViewCommandTarget
    {
        Guid PackageGuid { get; }
        bool Exec(string filepath, uint nCmdId);
    }
}
