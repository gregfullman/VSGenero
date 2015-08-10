using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public partial class GeneroAst
    {
        public void CheckForErrors(Action<string, int, int> errorFunc)
        {
            Body.CheckForErrors(this, errorFunc);
        }
    }
}
