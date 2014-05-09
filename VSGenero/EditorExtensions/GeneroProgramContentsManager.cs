using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSGenero.EditorExtensions
{
    internal class GeneroProgramContentsManager
    {
        private Dictionary<string, GeneroModuleContents> _programs;
        public Dictionary<string, GeneroModuleContents> Programs
        {
            get
            {
                if (_programs == null)
                    _programs = new Dictionary<string, GeneroModuleContents>();
                return _programs;
            }
        }

        public void AddProgramContents(string program, GeneroModuleContents newContents)
        {
            GeneroModuleContents gmc;
            if (!Programs.TryGetValue(program, out gmc))
            {
                gmc = new GeneroModuleContents();
            }

            // update the global variables dictionary
            foreach (var globalVarKvp in newContents.GlobalVariables)
            {
                gmc.GlobalVariables.AddOrUpdate(globalVarKvp.Key, globalVarKvp.Value, (x, y) => globalVarKvp.Value);
            }

            // Update the module functions dictionary
            foreach (var programFuncKvp in newContents.FunctionDefinitions.Where(x => !x.Value.Private))
            {
                gmc.FunctionDefinitions.AddOrUpdate(programFuncKvp.Key, programFuncKvp.Value, (x, y) => programFuncKvp.Value);
            }

            Programs[program] = gmc;
        }
    }
}
