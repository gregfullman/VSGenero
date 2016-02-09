using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGenero.Analysis
{
    internal class Genero4glProjectEntry : GeneroProjectEntry
    {
        private HashSet<string> _lastImportedModules;

        public Genero4glProjectEntry(string moduleName, string filePath, IAnalysisCookie cookie, bool shouldAnalyzeDir)
            : base(moduleName, filePath, cookie, shouldAnalyzeDir)
        {
        }

        public override void UpdateIncludesAndImports(string filename, GeneroAst ast)
        {
            if (_shouldAnalyzeDir && VSGeneroPackage.Instance.ProgramFileProvider != null)
            {
                var fglAst = ast as Genero4glAst;
                if (fglAst != null)
                {
                    // first do imports
                    if (_lastImportedModules == null)
                        _lastImportedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var modules = fglAst.GetImportedModules().ToList();
                    HashSet<string> currentlyImportedModules = new HashSet<string>(_lastImportedModules, StringComparer.OrdinalIgnoreCase);
                    foreach (var mod in modules.Select(x => VSGeneroPackage.Instance.ProgramFileProvider.GetImportModuleFilename(x, FilePath)).Where(y => y != null))
                    {
                        if (!_lastImportedModules.Contains(mod))
                        {
                            var impProj = ParentProject.AddImportedModule(mod, this);
                            if (impProj != null)
                            {
                                _lastImportedModules.Add(mod);
                                try
                                {
                                    if (!impProj.ReferencingProjectEntries.Contains(this))
                                        // TODO: for some reason a NRE got thrown here, but nothing was apparently wrong
                                        impProj.ReferencingProjectEntries.Add(this);
                                }
                                catch (Exception)
                                {
                                    int i = 0;
                                }
                            }
                        }
                        else
                            currentlyImportedModules.Remove(mod);
                    }

                    // delete the leftovers
                    foreach (var mod in currentlyImportedModules)
                    {
                        ParentProject.RemoveImportedModule(mod);
                        _lastImportedModules.Remove(mod);
                    }

                    // next do includes
                    var includes = fglAst.GetIncludedFiles();
                    HashSet<string> currentlyIncludedFiles = new HashSet<string>(VSGeneroPackage.Instance.DefaultAnalyzer.GetIncludedFiles(this).Select(x => x.FilePath), StringComparer.OrdinalIgnoreCase);
                    foreach (var incl in includes.Select(x => VSGeneroPackage.Instance.ProgramFileProvider.GetIncludeFile(x, FilePath)).Where(y => y != null))
                    {
                        if (!VSGeneroPackage.Instance.DefaultAnalyzer.IsIncludeFileIncludedByProjectEntry(incl, this))
                        {
                            VSGeneroPackage.Instance.DefaultAnalyzer.AddIncludedFile(incl, this);
                        }
                        else
                            currentlyIncludedFiles.Remove(incl);
                    }

                    // delete the leftovers
                    foreach (var include in currentlyIncludedFiles)
                    {
                        IGeneroProjectEntry dummy;
                        VSGeneroPackage.Instance.DefaultAnalyzer.RemoveIncludedFile(include, this);
                    }
                }
            }
        }
    }
}
