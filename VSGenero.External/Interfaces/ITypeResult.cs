using System.Collections.Generic;

namespace VSGenero.External.Interfaces
{
    public interface ITypeResult
    {
        bool IsRecord { get; }

        Dictionary<string, ITypeResult> RecordMemberTypes { get; }

        bool IsArray { get; }

        ITypeResult ArrayType { get; }

        string Typename { get; }

        ITypeResult UnderlyingType { get; }
    }
}
