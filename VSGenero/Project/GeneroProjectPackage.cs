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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
#if DEV11_OR_LATER
using Microsoft.VisualStudio.Shell.Interop;
#endif

namespace VSGenero.Project
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Description("Genero Project Package")]
    [ProvideProjectFactory(typeof(GeneroProjectFactory), VSGeneroConstants.LanguageName4GL, GeneroProjectFileFilter, "glproj", "glproj", ".\\NullPath", LanguageVsTemplate = VSGeneroConstants.LanguageName4GL)]
    // TODO: not sure how to get two language extensions registered under the same editor...we'll see
    [ProvideEditorExtension2(typeof(GeneroEditorFactory), VSGeneroConstants.FileExtension4GL, 50, ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3750, DefaultName = "program", TemplateDir = "Templates\\NewItem")]
#if DEV12
    // TODO: look at Python Tools, there's something different for this
#else
    [ProvideEditorExtension2(typeof(GeneroEditorFactory), VSGeneroConstants.FileExtension4GL, 50, "*:1", ProjectGuid = VSGeneroConstants.ProjectFactoryGuid, NameResourceID = 3750, EditorNameResourceId = 3751, DefaultName = "program", TemplateDir = ".\\NullPath")]
#endif
    [ProvideFileFilter(VSGeneroConstants.ProjectFactoryGuid, "/1", "Genero Files;*.4gl,*.per", 101)]
    [ProvideEditorLogicalView(typeof(GeneroEditorFactory), VSConstants.LOGVIEWID.TextView_string)]

    [Guid(VSGeneroConstants.ProjectSystemPackageGuid)]
    public class GeneroProjectPackage : CommonProjectPackage
    {
        internal const string GeneroProjectFileFilter = "Genero Project Files (*.glproj);*.glproj";

        public override ProjectFactory CreateProjectFactory()
        {
            return new GeneroProjectFactory(this);
        }

        public override CommonEditorFactory CreateEditorFactory()
        {
            return new GeneroEditorFactory(this);
        }

        public override CommonEditorFactory CreateEditorFactoryPromptForEncoding()
        {
            return new GeneroEditorFactory(this);
        }

        public override uint GetIconIdForAboutBox()
        {
            return VSGeneroConstants.IconIdForAboutBox;
        }

        public override uint GetIconIdForSplashScreen()
        {
            return VSGeneroConstants.IconIfForSplashScreen;
        }

        public override string GetProductName()
        {
            return VSGeneroConstants.LanguageName4GL;
        }

        public override string GetProductDescription()
        {
            return VSGeneroConstants.LanguageName4GL;
        }

        public override string GetProductVersion()
        {
            return "1.0";
        }
    }
}
