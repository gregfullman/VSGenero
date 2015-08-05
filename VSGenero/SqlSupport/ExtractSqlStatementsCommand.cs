/* ****************************************************************************
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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.VSCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VSGenero.SqlSupport
{
    internal class ExtractSqlStatementsCommand : CommonCommand
    {
        private static string _tempfilename = null;

        public override void DoCommand(object sender, EventArgs args)
        {
            // TODO: get the selected text in the code window and copy any extracted sql statements to the clipboard
            if (VSGeneroPackage.Instance.ActiveDocument != null)
            {
                EnvDTE.TextSelection sel = (EnvDTE.TextSelection)VSGeneroPackage.Instance.ActiveDocument.Selection;
                if (sel != null && sel.Text.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var fragment in SqlStatementExtractor.ExtractStatements(sel.Text))
                    {
                        sb.AppendLine(fragment.GetText());
                        sb.AppendLine();
                    }

                    var tempText = sb.ToString().Trim();
                    if (tempText.Length > 0)
                    {
                        SqlExtensions.SetSqlExtractionFile(VSGeneroPackage.Instance.ActiveDocument.Path);
                        ITextBuffer buffer = null;
                        if (_tempfilename == null || (buffer = GetBufferAt(_tempfilename)) == null)
                        {
                            _tempfilename = Path.GetTempPath() + "temp_sql_file.sql";
                            File.WriteAllText(_tempfilename, tempText);
                            VSGeneroPackage.NavigateTo(_tempfilename, Guid.Empty, 0);
                        }
                        else
                        {
                            var edit = buffer.CreateEdit();
                            edit.Insert(buffer.CurrentSnapshot.Length, (buffer.CurrentSnapshot.LineCount > 0 ? "\n\n" : "") + tempText);
                            edit.Apply();
                        }
                    }
                }
            }
        }

        internal ITextBuffer GetBufferAt(string filePath)
        {
            var componentModel = (IComponentModel)VSGeneroPackage.GetGlobalService(typeof(SComponentModel));
            var editorAdapterFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            IOleServiceProvider provider = VSGeneroPackage.GetGlobalService(typeof(IOleServiceProvider)) as IOleServiceProvider;
            var serviceProvider = new Microsoft.VisualStudio.Shell.ServiceProvider(provider);

            IVsUIHierarchy uiHierarchy;
            uint itemID;
            IVsWindowFrame windowFrame;
            if (VsShellUtilities.IsDocumentOpen(
              serviceProvider,
              filePath,
              Guid.Empty,
              out uiHierarchy,
              out itemID,
              out windowFrame))
            {
                IVsTextView view = VsShellUtilities.GetTextView(windowFrame);
                IVsTextLines lines;
                if (view.GetBuffer(out lines) == 0)
                {
                    var buffer = lines as IVsTextBuffer;
                    if (buffer != null)
                        return editorAdapterFactoryService.GetDataBuffer(buffer);
                }
            }

            return null;
        }

        public override int CommandId
        {
            get { return (int)PkgCmdID.cmdidExtractSqlStatements; }
        }
    }
}
