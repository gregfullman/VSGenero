using System;

namespace VSGenero.External.Interfaces
{
    public interface IGeneroTextViewCommandTarget
    {
        Guid PackageGuid { get; }
        bool Exec(string filepath, uint nCmdId);
    }
}
