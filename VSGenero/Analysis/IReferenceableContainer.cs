using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    interface IReferenceableContainer
    {
        IEnumerable<IReferenceable> GetDefinitions(string name);
    }

    interface IReferenceable
    {
        IEnumerable<KeyValuePair<IProjectEntry, LocationInfo>> Definitions
        {
            get;
        }
        IEnumerable<KeyValuePair<IProjectEntry, LocationInfo>> References
        {
            get;
        }
    }
}
