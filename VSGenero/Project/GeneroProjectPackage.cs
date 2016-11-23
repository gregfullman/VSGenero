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

    // This attribute controls what shows up in the New Item dialog (not Project's Add Item dialog)
    [ProvideEditorExtension2(typeof(EditorFactory), VSGeneroConstants.FileExtension4GL, 50, ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3750, DefaultName = "program", TemplateDir = "Templates\\NewItem")]
    [ProvideEditorExtension2(typeof(EditorFactory), VSGeneroConstants.FileExtension4GL, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSGeneroConstants.GeneroProjectFactoryGuid, NameResourceID = 3750, EditorNameResourceId = 3751, DefaultName = "program", TemplateDir = ".\\NullPath")]

    [ProvideEditorExtension2(typeof(EditorFactory), VSGeneroConstants.FileExtensionPER, 50, ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3750, DefaultName = "program", TemplateDir = "Templates\\NewItem")]
    [ProvideEditorExtension2(typeof(EditorFactory), VSGeneroConstants.FileExtensionPER, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSGeneroConstants.GeneroProjectFactoryGuid, NameResourceID = 3750, EditorNameResourceId = 3751, DefaultName = "program", TemplateDir = ".\\NullPath")]

    [ProvideFileFilter(VSGeneroConstants.GeneroProjectFactoryGuid, "/1", "Genero Files;*.4gl,*.per", 101)]
    [ProvideEditorLogicalView(typeof(EditorFactory), VSConstants.LOGVIEWID.TextView_string)]

    [Guid(VSGeneroConstants.ProjectSystemPackageGuid)]
    public class GeneroProjectPackage : CommonProjectPackage
    {
        internal const string GeneroProjectFileFilter = "Genero Project Files (*.glproj);*.glproj";

        public override ProjectFactory CreateProjectFactory()
        {
            return new GeneroProjectFactory(this);
        }

        public override IVsEditorFactory CreateEditorFactory()
        {
            return VSGeneroPackage.Instance.GeneroEditorFactory;
        }

        public override IVsEditorFactory CreateEditorFactoryPromptForEncoding()
        {
            return VSGeneroPackage.Instance.GeneroEditorFactory;
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
