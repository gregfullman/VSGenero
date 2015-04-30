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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Project;
using VSGenero.Navigation;

namespace VSGenero.Project
{
    [Guid(VSGeneroConstants.EditorFactoryGuid)]
    public class GeneroEditorFactory : CommonEditorFactory
    {
        public GeneroEditorFactory(CommonProjectPackage package) : base(package) { }

        public GeneroEditorFactory(CommonProjectPackage package, bool promptForEncoding) : base(package, promptForEncoding) { }

        protected override void InitializeLanguageService(IVsTextLines textLines)
        {
            IVsUserData userData = textLines as IVsUserData;
            if (userData != null)
            {
                Guid langSid = typeof(VSGenero4GLLanguageInfo).GUID;
                if (langSid != Guid.Empty)
                {
                    Guid vsCoreSid = new Guid("{8239bec4-ee87-11d0-8c98-00c04fc2ab22}");
                    Guid currentSid;
                    ErrorHandler.ThrowOnFailure(textLines.GetLanguageServiceID(out currentSid));
                    // If the language service is set to the default SID, then
                    // set it to our language
                    if (currentSid == vsCoreSid)
                    {
                        ErrorHandler.ThrowOnFailure(textLines.SetLanguageServiceID(ref langSid));
                    }
                    else if (currentSid != langSid)
                    {
                        // Some other language service has it, so return VS_E_INCOMPATIBLEDOCDATA
                        throw new COMException("Incompatible doc data", VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                    }

                    Guid bufferDetectLang = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
                    ErrorHandler.ThrowOnFailure(userData.SetData(ref bufferDetectLang, false));
                }
            }
        }
    }

    [Guid(VSGeneroConstants.EditorFactoryPromptForEncodingGuid)]
    public class GeneroEditorFactoryPromptForEncoding : GeneroEditorFactory
    {
        public GeneroEditorFactoryPromptForEncoding(CommonProjectPackage package) : base(package, true) { }
    }
}
