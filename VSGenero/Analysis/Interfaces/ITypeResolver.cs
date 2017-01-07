using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Interfaces
{
    public interface ITypeResolver
    {
        ITypeResult GetGeneroType(string variableName, string filename, int lineNumber);
    }
}
