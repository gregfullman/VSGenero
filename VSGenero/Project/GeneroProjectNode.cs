/* ****************************************************************************
 *
 * Copyright (c) 2014, Greg Fullman
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Build.Execution;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Navigation;
using NativeMethods = Microsoft.VisualStudioTools.Project.NativeMethods;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using Task = System.Threading.Tasks.Task;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
using VsMenus = Microsoft.VisualStudioTools.Project.VsMenus;
using VSGenero.Navigation;

namespace VSGenero.Project
{
    [Guid(VSGeneroConstants.ProjectNodeGuid)]
    internal class GeneroProjectNode : CommonProjectNode
    {
        public GeneroProjectNode(CommonProjectPackage package)
            : base(package, Utilities.GetImageList(typeof(GeneroProjectNode).Assembly.GetManifestResourceStream(VSGeneroConstants.ProjectImageList)))
        {
            Type projectNodePropsType = typeof(GeneroProjectNodeProperties);
            AddCATIDMapping(projectNodePropsType, projectNodePropsType.GUID);
        }

        public override Type GetProjectFactoryType()
        {
            return typeof(GeneroProjectFactory);
        }

        public override Type GetEditorFactoryType()
        {
            return typeof(GeneroEditorFactory);
        }

        public override string GetProjectName()
        {
            return "GeneroProject";
        }

        public override string GetFormatList()
        {
            return VSGeneroConstants.ProjectFileFilter;
        }

        public override Type GetGeneralPropertyPageType()
        {
            return typeof(CommonPropertyPage);  // TODO: need to create GeneroGeneralPropertyPage
        }

        public override Type GetLibraryManagerType()
        {
            return typeof(IGeneroLibraryManager);     // TODO: need to create an implementation of ILibraryManager
        }

        public override IProjectLauncher GetLauncher()
        {
            return null;    // TODO: I suppose this could somehow link up with the Run mechanism...will need to expose something like that
        }
    }
}
