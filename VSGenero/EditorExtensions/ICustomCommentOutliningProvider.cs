using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;

namespace VSGenero.EditorExtensions
{
    public interface ICustomCommentOutliningProvider
    {
        IEnumerable<GeneroCodeRegion> GetRegions(TokenWithSpan[] commentLines);
    }
}
