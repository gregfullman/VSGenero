using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSGenero.EditorExtensions
{
    internal class GeneroProgramContentsManager
    {
        private object _lock = new object();

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
                gmc.ContentFilename = "global";
            }

            // update the global variables dictionary
            foreach (var globalVarKvp in newContents.GlobalVariables)
            {
                gmc.GlobalVariables.AddOrUpdate(globalVarKvp.Key.ToLower(), globalVarKvp.Value, (x, y) => globalVarKvp.Value);
            }

            // update the global constants dictionary
            foreach (var globalVarKvp in newContents.GlobalConstants)
            {
                gmc.GlobalConstants.AddOrUpdate(globalVarKvp.Key.ToLower(), globalVarKvp.Value, (x, y) => globalVarKvp.Value);
            }

            // update the global types dictionary
            foreach (var globalVarKvp in newContents.GlobalTypes)
            {
                gmc.GlobalTypes.AddOrUpdate(globalVarKvp.Key.ToLower(), globalVarKvp.Value, (x, y) => globalVarKvp.Value);
            }

            // Update the module functions dictionary
            foreach (var programFuncKvp in newContents.FunctionDefinitions)
            {
                gmc.FunctionDefinitions.AddOrUpdate(programFuncKvp.Key.ToLower(), programFuncKvp.Value, (x, y) => programFuncKvp.Value);
            }

            lock (_lock)
            {
                if (program != null && gmc != null)
                {
                    if (Programs.ContainsKey(program))
                        Programs[program] = gmc;
                    else
                        Programs.Add(program, gmc);
                }
            }
        }
    }
}
