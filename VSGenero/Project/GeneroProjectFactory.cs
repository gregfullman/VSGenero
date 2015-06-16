/* ****************************************************************************
 * Copyright (c) 2014 Greg Fullman 
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VSGenero.Project
{
    [Guid(VSGeneroConstants.ProjectFactoryGuid)]
    class GeneroProjectFactory : ProjectFactory
    {
        public GeneroProjectFactory(IServiceProvider package)
            : base(package) 
        {
        }

        public override ProjectNode CreateProject()
        {
            GeneroProjectNode project = new GeneroProjectNode((GeneroProjectPackage)Package);
            project.SetSite((IOleServiceProvider)((IServiceProvider)Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }

        protected override ProjectUpgradeState UpgradeProjectCheck(
            ProjectRootElement projectXml,
            ProjectRootElement userProjectXml,
            Action<__VSUL_ERRORLEVEL, string> log,
            ref Guid projectFactory,
            ref __VSPPROJECTUPGRADEVIAFACTORYFLAGS backupSupport
        )
        {
            Version version;

            if (!Version.TryParse(projectXml.ToolsVersion, out version) ||
                version < new Version(4, 0))
            {
                return ProjectUpgradeState.SafeRepair;
            }

            return ProjectUpgradeState.NotNeeded;
        }

        protected override void UpgradeProject(
            ref ProjectRootElement projectXml,
            ref ProjectRootElement userProjectXml,
            Action<__VSUL_ERRORLEVEL, string> log
        )
        {
            projectXml.ToolsVersion = "4.0";
            log(__VSUL_ERRORLEVEL.VSUL_INFORMATIONAL, SR.GetString(SR.UpgradedToolsVersion));
        }
    }
}
