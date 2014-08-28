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
    internal sealed class VSGenero4GLLanguageInfo : VSGeneroLanguageInfo, IVsLanguageDebugInfo
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

        public int GetLanguageID(IVsTextBuffer pBuffer, int iLine, int iCol, out Guid pguidLanguageID)
        {
            pguidLanguageID = Guid.Empty;
            return VSConstants.S_OK;
        }

        public int GetLocationOfName(string pszName, out string pbstrMkDoc, TextSpan[] pspanLocation)
        {
            pbstrMkDoc = null;
            return VSConstants.E_FAIL;
        }

        public int GetNameOfLocation(IVsTextBuffer pBuffer, int iLine, int iCol, out string pbstrName, out int piLineOffset)
        {
            var model = VSGeneroPackage.Instance.GetPackageService(typeof(SComponentModel)) as IComponentModel;
            var service = model.GetService<IVsEditorAdaptersFactoryService>();
            var buffer = service.GetDataBuffer(pBuffer);

            pbstrName = "";
            piLineOffset = iCol;
            return VSConstants.E_FAIL;
        }

        public int GetProximityExpressions(IVsTextBuffer pBuffer, int iLine, int iCol, int cLines, out IVsEnumBSTR ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_FAIL;
        }

        public int IsMappedLocation(IVsTextBuffer pBuffer, int iLine, int iCol)
        {
            return VSConstants.E_FAIL;
        }

        public int ResolveName(string pszName, uint dwFlags, out IVsEnumDebugName ppNames)
        {
            /*if((((RESOLVENAMEFLAGS)dwFlags) & RESOLVENAMEFLAGS.RNF_BREAKPOINT) != 0) {
                    // TODO: This should go through the project/analysis and see if we can
                    // resolve the names...
                }*/
            ppNames = null;
            return VSConstants.E_FAIL;
        }

        public int ValidateBreakpointLocation(IVsTextBuffer pBuffer, int iLine, int iCol, TextSpan[] pCodeSpan)
        {
            // per the docs, even if we don't indend to validate, we need to set the span info:
            // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.textmanager.interop.ivslanguagedebuginfo.validatebreakpointlocation.aspx
            // 
            // Caution
            // Even if you do not intend to support the ValidateBreakpointLocation method but your 
            // language does support breakpoints, you must implement this method and return a span 
            // that contains the specified line and column; otherwise, breakpoints cannot be set 
            // anywhere except line 1. You can return E_NOTIMPL to indicate that you do not otherwise 
            // support this method but the span must always be set. The example shows how this can be done.

            // http://pytools.codeplex.com/workitem/787
            // We were previously returning S_OK here indicating to VS that we have in fact validated
            // the breakpoint.  Validating breakpoints actually interacts and effectively disables
            // the "Highlight entire source line for breakpoints and current statement" option as instead
            // VS highlights the validated region.  So we return E_NOTIMPL here to indicate that we have 
            // not validated the breakpoint, and then VS will happily respect the option when we're in 
            // design mode.
            pCodeSpan[0].iStartLine = iLine;
            pCodeSpan[0].iEndLine = iLine;
            return VSConstants.E_NOTIMPL;
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
