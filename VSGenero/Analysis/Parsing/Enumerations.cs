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
        Types = 4,
        Functions = 8,
        Dialogs = 16,
        Reports = 32,
        PreparedCursors = 64,
        DeclaredCursors = 128,
        Tables = 256,

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
