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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSGenero.Project
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Description("Genero Project Package")]
    [ProvideProjectFactory(typeof(GeneroProjectFactory), VSGeneroConstants.LanguageName, GeneroProjectFileFilter, "glproj", "glproj", @".\\NullPath", LanguageVsTemplate = VSGeneroConstants.LanguageName)]
    // TODO: not sure how to get two language extensions registered under the same editor...we'll see
    
    // This attribute controls what shows up in the New Item dialog (not Project's Add Item dialog)
    [ProvideEditorExtension2(typeof(GeneroEditorFactory), VSGeneroConstants.FileExtension4GL, 50, ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3750, DefaultName = "program", TemplateDir = "Templates\\NewItem")]
#if DEV12_OR_LATER
    // TODO: look at Python Tools, there's something different for this
    [ProvideEditorExtension2(typeof(GeneroEditorFactory), VSGeneroConstants.FileExtension4GL, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSGeneroConstants.ProjectFactoryGuid, NameResourceID = 3750, EditorNameResourceId = 3751, DefaultName = "program", TemplateDir = ".\\NullPath")]
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
