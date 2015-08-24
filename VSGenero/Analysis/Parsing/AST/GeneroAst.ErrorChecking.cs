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
            Dictionary<string, List<int>> deferredFunctionSearches = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            Body.CheckForErrors(this, errorFunc, deferredFunctionSearches);

            if(deferredFunctionSearches.Count > 0 && this._functionProvider != null)
            {
                // do the deferred search for function names
                // TODO: need to check the number of arguments and returns...no idea how that's going to happen
                var existing = this._functionProvider.GetExistingFunctionsFromSet(deferredFunctionSearches.Keys);
                if(existing != null)
                {
                    foreach(var notFound in deferredFunctionSearches.Keys.Except(existing, StringComparer.OrdinalIgnoreCase))
                    {
                        foreach(var inst in deferredFunctionSearches[notFound])
                        {
                            errorFunc(string.Format("No definition found for {0}", notFound), inst, inst + notFound.Length);
                        }
                    }
                }
            }
        }
    }
}
