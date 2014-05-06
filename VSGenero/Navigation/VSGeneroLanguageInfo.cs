/* ****************************************************************************
 * 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VSGenero.Navigation
{
    internal abstract class VSGeneroLanguageInfo : IVsLanguageInfo
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IComponentModel _componentModel;

        public VSGeneroLanguageInfo(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
        }

        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
        {
            var model = _serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            var service = model.GetService<IVsEditorAdaptersFactoryService>();

            IVsTextView textView;
            if (ErrorHandler.Succeeded(pCodeWin.GetPrimaryView(out textView)))
            {
                // need to implement the CodeWindowManager
                ppCodeWinMgr = new VSGeneroCodeWindowManager(pCodeWin, service.GetWpfTextView(textView));
                return VSConstants.S_OK;
            }

            ppCodeWinMgr = null;
            return VSConstants.E_FAIL;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
        {
            ppColorizer = null;
            return VSConstants.E_FAIL;
        }

        public abstract int GetFileExtensions(out string pbstrExtensions);
        public abstract int GetLanguageName(out string bstrName);
    }

    [Guid("c41c558d-4373-4ae1-8424-fb04873a0e9c")]
    internal sealed class VSGenero4GLLanguageInfo : VSGeneroLanguageInfo
    {
        public VSGenero4GLLanguageInfo(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override int GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = VSGeneroConstants.FileExtension4GL;
            return VSConstants.S_OK;
        }

        public override int GetLanguageName(out string bstrName)
        {
            bstrName = VSGeneroConstants.LanguageName4GL;
            return VSConstants.S_OK;
        }
    }

    [Guid("c41c558d-4373-4ae1-8424-fb04873a0e9d")]
    internal sealed class VSGeneroPERLanguageInfo : VSGeneroLanguageInfo
    {
        public VSGeneroPERLanguageInfo(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override int GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = VSGeneroConstants.FileExtensionPER;
            return VSConstants.S_OK;
        }

        public override int GetLanguageName(out string bstrName)
        {
            bstrName = VSGeneroConstants.LanguageNamePER;
            return VSConstants.S_OK;
        }
    }
}
