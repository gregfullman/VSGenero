/* ****************************************************************************
 * 
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


using Microsoft.VisualStudio;
using System;

namespace VSGenero
{
    public static class VSGeneroConstants
    {
        internal const string ProjectFileFilter = "Genero Project File (*.glproj)\n*.glproj\nAll Files (*.*)\n*.*\n";

        public const string GeneroEditorFactoryGuid = "F26A6EE9-3CE6-4885-B62D-382F4DD3A50E";
        public const string GeneroProjectFactoryGuid = "888888a0-9f3d-457c-b088-3a5042f75d53";
        internal const string EditorFactoryPromptForEncodingGuid = "CA887E0B-55C6-4AE9-B5CF-A2EEFBA90A3F";
        internal const string LibraryManagerServiceGuid = "26E07811-23A9-4E72-B64D-141461371D55";
        internal const string LibraryManagerGuid = "66973468-8AB4-4410-A8EE-9E36BCC7ED21";
        internal const string ProjectNodeGuid = "6EC824C5-356D-4446-9A42-D80E2D17C14B";

//#if VS120
//        internal const string ProjectImageList = "VSGenero.Resources.GeneroImageList.png";
//#else
        internal const string ProjectImageList = "VSGenero.Resources.GeneroImageList.bmp";
//#endif
        // Do not change below info without re-requesting PLK:
        internal const string ProjectSystemPackageGuid = "15490272-3C6B-4129-8E1D-795C8B6D8E9A"; //matches PLK

        public const string BaseRegistryKey = "VSGenero";

        //These are VS internal constants - don't change them
        public static Guid Std97CmdGroupGuid = typeof(VSConstants.VSStd97CmdID).GUID;
        public static Guid Std2KCmdGroupGuid = typeof(VSConstants.VSStd2KCmdID).GUID;

        internal const int IconIfForSplashScreen = 300;
        internal const int IconIdForAboutBox = 400;
    }
}
