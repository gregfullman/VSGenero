using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    public enum AccessModifier
    {
        Private,
        Public
    }

    public enum ImportModuleType
    {
        C,
        FGL,
        Java
    }

    public enum ArrayType
    {
        Static,
        Dynamic,
        Java
    }
}
