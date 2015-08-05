/* ****************************************************************************
* 
* Copyright (c) 2015 Greg Fullman 
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
using System.Text;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using System.Security.Principal;
using Microsoft.VisualStudio.VSCommon;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Reflection;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace VSGenero.SqlSupport
{
    [ContentType("T-SQL90")]
    [ContentType("SQL Server Tools")]
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class SqlTextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import(typeof(IVsEditorAdaptersFactoryService))]
        internal IVsEditorAdaptersFactoryService editorFactory;

        [Import(AllowDefault = true)]
        internal ISqlContextDeterminator contextDeterminator;

        private static OleMenuCommandService _commandService;
        private static CommandID _sqlEditorConnectCmdId;

        public SqlTextViewCreationListener()
        {
            if (_commandService == null)
            {
                _commandService = VSGeneroPackage.Instance.GetPackageService(typeof(IMenuCommandService)) as OleMenuCommandService;
                _sqlEditorConnectCmdId = new CommandID(new Guid("{b371c497-6d81-4b13-9db8-8e3e6abad0c3}"), 0x300);
            }
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (contextDeterminator != null && editorFactory != null)
            {
                var vsTextView = editorFactory.GetViewAdapter(textView);
                if (vsTextView != null)
                {
                    IVsTextLines ppBuffer = null;
                    vsTextView.GetBuffer(out ppBuffer);
                    if (ppBuffer != null)
                    {
                        SqlExtensions.SetSqlEditorConnection(ppBuffer, contextDeterminator, _commandService, _sqlEditorConnectCmdId);
                    }
                }
            }
        }
    }
}