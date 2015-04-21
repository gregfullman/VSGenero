using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public interface IProgramFileProvider
    {
        IEnumerable<string> GetProgramFilenames(string filename);
    }
}
