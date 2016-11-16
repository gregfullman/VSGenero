using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public enum AstMemberType
    {
        Variables = 1,
        Constants = 2,
        Functions = 4,
        Dialogs = 8,
        Reports = 16,
        PreparedCursors = 32,
        DeclaredCursors = 64,
        Tables = 128,
        SystemTypes = 256,
        UserDefinedTypes = 512,

        Types = SystemTypes | UserDefinedTypes,
        Cursors = PreparedCursors | DeclaredCursors,
        All = Variables | Constants | Types | Functions | Dialogs | Reports | Cursors | Tables
    }

    public enum FunctionProviderSearchMode
    {
        NoSearch,
        Search,
        Deferred
    }
}
