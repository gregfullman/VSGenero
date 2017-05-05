/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGenero.Analysis
{
    public class GeneroProject : IGeneroProject, IAnalysisResult
    {
        public string Typename
        {
            get { return null; }
        }

        private readonly string _directory;
        public GeneroProject(string directory)
        {
            _directory = directory;
        }

        public IGeneroProject AddImportedModule(string path, IGeneroProjectEntry importer)
        {
            IGeneroProject refProj = null;
            if (!ReferencedProjects.TryGetValue(path, out refProj))
            {
                // need to tell the genero project analyzer to add a directory to the project
                refProj = VSGeneroPackage.Instance.DefaultAnalyzer.AddImportedProject(path, importer);
                if (refProj == null)
                {
                    // TODO: need to report that the project is invalid?
                }
                else
                {
                    ReferencedProjects.AddOrUpdate(path, refProj, (x, y) => y);
                }
            }
            return refProj;
        }

        public void RemoveImportedModule(string path)
        {
            VSGeneroPackage.Instance.DefaultAnalyzer.RemoveImportedProject(path);
            if (ReferencedProjects.ContainsKey(path))
            {
                IGeneroProject remEntry;
                ReferencedProjects.TryRemove(path, out remEntry);
            }
        }

        private ConcurrentDictionary<string, IGeneroProjectEntry> _projectEntries;
        public ConcurrentDictionary<string, IGeneroProjectEntry> ProjectEntries
        {
            get
            {
                if (_projectEntries == null)
                    _projectEntries = new ConcurrentDictionary<string, IGeneroProjectEntry>(StringComparer.OrdinalIgnoreCase);
                return _projectEntries;
            }
        }

        private ConcurrentDictionary<string, IGeneroProject> _referencedProjects;
        public ConcurrentDictionary<string, IGeneroProject> ReferencedProjects
        {
            get
            {
                if (_referencedProjects == null)
                    _referencedProjects = new ConcurrentDictionary<string, IGeneroProject>(StringComparer.OrdinalIgnoreCase);
                return _referencedProjects;
            }
        }

        public string Directory
        {
            get { return _directory; }
        }

        private object _refProjEntriesLock = new object();
        private HashSet<IGeneroProjectEntry> _referencingProjectEntries;
        public HashSet<IGeneroProjectEntry> ReferencingProjectEntries
        {
            get
            {
                lock (_refProjEntriesLock)
                {
                    if (_referencingProjectEntries == null)
                        _referencingProjectEntries = new HashSet<IGeneroProjectEntry>();
                    return _referencingProjectEntries;
                }
            }
        }

        public string Scope
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string Name
        {
            get { return Path.GetFileName(_directory); }
        }

        public string Documentation
        {
            get
            {
                return string.Format("Imported module {0}", Name);
            }
        }

        public int LocationIndex
        {
            get { return -1; }
        }

        public LocationInfo Location
        {
            get { return null; }
        }

        public bool HasChildFunctions(Genero4glAst ast)
        {
            return false;
        }

        public bool CanGetValueFromDebugger
        {
            get { return true; }
        }

        public bool IsPublic { get { return true; } }

        public GeneroLanguageVersion MinimumLanguageVersion
        {
            get
            {
                return GeneroLanguageVersion.None;
            }
        }

        public GeneroLanguageVersion MaximumLanguageVersion
        {
            get
            {
                return GeneroLanguageVersion.Latest;
            }
        }

        

        internal IAnalysisResult GetMemberOfType(string name, object ast, bool vars, bool types, bool consts, bool funcs, out IProjectEntry definingProjEntry)
        {
            string projNamespace = string.Format("{0}", this.Name);
            string tempStart = string.Format("{0}.", projNamespace);
            if (name.StartsWith(tempStart, StringComparison.OrdinalIgnoreCase))
                name = name.Substring(tempStart.Length);

            definingProjEntry = null;
            IAnalysisResult res = null;
            foreach (var projEntry in ProjectEntries)
            {
                if (projEntry.Value.Analysis != null &&
                   projEntry.Value.Analysis.Body != null)
                {
                    projEntry.Value.Analysis.Body.SetNamespace(projNamespace);
                    IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                    if (modRes != null)
                    {
                        // check global vars, types, and constants
                        if ((vars && modRes.GlobalVariables.TryGetValue(name, out res)) ||
                            (types && modRes.GlobalTypes.TryGetValue(name, out res)) ||
                            (consts && modRes.GlobalConstants.TryGetValue(name, out res)))
                        {
                            //found = true;
                            definingProjEntry = projEntry.Value;
                            break;
                        }

                        if (((vars && modRes.Variables.TryGetValue(name, out res)) ||
                             (types && modRes.Types.TryGetValue(name, out res)) ||
                             (consts && modRes.Constants.TryGetValue(name, out res))) && res.IsPublic)
                        {
                            definingProjEntry = projEntry.Value;
                            break;
                        }

                        // check project functions
                        IFunctionResult funcRes = null;
                        if (funcs && modRes.Functions.TryGetValue(name, out funcRes))
                        {
                            if (funcRes.AccessModifier == AccessModifier.Public)
                            {
                                res = funcRes;
                                //found = true;
                                definingProjEntry = projEntry.Value;
                                break;
                            }
                        }
                        
                        foreach(var inclFile in projEntry.Value.GetIncludedFiles())
                        {
                            if(inclFile.Analysis != null && inclFile.Analysis.Body is IModuleResult)
                            {
                                if(vars && (inclFile.Analysis.Body as IModuleResult).Variables.TryGetValue(name, out res))
                                {
                                    definingProjEntry = inclFile;
                                    break;
                                }

                                if (types && (inclFile.Analysis.Body as IModuleResult).Types.TryGetValue(name, out res))
                                {
                                    definingProjEntry = inclFile;
                                    break;
                                }

                                if (consts && (inclFile.Analysis.Body as IModuleResult).Constants.TryGetValue(name, out res))
                                {
                                    definingProjEntry = inclFile;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public IAnalysisResult GetMember(GetMemberInput input)
        {
            IProjectEntry projEntry = null;
            var res = GetMemberOfType(input.Name, input.AST, true, true, true, true, out projEntry);
            if (projEntry != null && projEntry is IGeneroProjectEntry)
            { 
                input.ProjectEntry = projEntry;
                input.DefiningProject = (projEntry as IGeneroProjectEntry).ParentProject;
            }
            return res;
        }

        public IEnumerable<MemberResult> GetMembers(GetMultipleMembersInput input)
        {
            string projNamespace = string.Format("{0}", this.Name);
            List<MemberResult> members = new List<MemberResult>();

            foreach (var projEntry in ProjectEntries)
            {
                if (projEntry.Value.Analysis != null &&
                   projEntry.Value.Analysis.Body != null)
                {
                    projEntry.Value.Analysis.Body.SetNamespace(projNamespace);
                    IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                    if (modRes != null)
                    {
                        if (input.MemberType.HasFlag(MemberType.Variables))
                        {
                            members.AddRange(modRes.GlobalVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, input.AST)));
                            members.AddRange(modRes.Variables.Where(x => x.Value.IsPublic).Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, input.AST)));
                            members.AddRange(projEntry.Value.GetIncludedFiles().Where(x => x.Analysis != null).SelectMany(x => x.Analysis.GetDefinedMembers(1, AstMemberType.Variables, input.IsMemberAccess)));
                        }

                        if (input.MemberType.HasFlag(MemberType.Types))
                        {
                            members.AddRange(modRes.GlobalTypes.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, input.AST)));
                            members.AddRange(modRes.Types.Where(x => x.Value.IsPublic).Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, input.AST)));
                            members.AddRange(projEntry.Value.GetIncludedFiles().Where(x => x.Analysis != null).SelectMany(x => x.Analysis.GetDefinedMembers(1, AstMemberType.UserDefinedTypes, input.IsMemberAccess)));
                        }

                        if (input.MemberType.HasFlag(MemberType.Constants))
                        {
                            members.AddRange(modRes.GlobalConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, input.AST)));
                            members.AddRange(modRes.Constants.Where(x => x.Value.IsPublic).Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, input.AST)));
                            members.AddRange(projEntry.Value.GetIncludedFiles().Where(x => x.Analysis != null).SelectMany(x => x.Analysis.GetDefinedMembers(1, AstMemberType.Constants, input.IsMemberAccess)));
                        }

                        if (input.MemberType.HasFlag(MemberType.Functions))
                        {
                            members.AddRange(modRes.Functions.Where(x => x.Value.IsPublic).Select(x => new MemberResult(x.Key, x.Value, x.Value.FunctionType, input.AST)));
                            members.AddRange(projEntry.Value.GetIncludedFiles().Where(x => x.Analysis != null).SelectMany(x => x.Analysis.GetDefinedMembers(1, AstMemberType.Functions, input.IsMemberAccess)));
                        }
                    }
                }
            }

            return members;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach (var projEntry in ProjectEntries)
                {
                    if (projEntry.Value != null)
                        projEntry.Value.Dispose();
                }
                ProjectEntries.Clear();
            }
        }
    }
}
