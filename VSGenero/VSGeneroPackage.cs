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
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Timers;
using System.ComponentModel;
using System.Threading;

using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

using VSGenero.Navigation;
using Microsoft.VisualStudio.VSCommon;
using Microsoft.VisualStudio.VSCommon.Utilities;

using EnvDTE;
using EnvDTE80;
using Extensibility;

using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Navigation;
using Microsoft.VisualStudioTools.Project;
using NativeMethods = Microsoft.VisualStudioTools.Project.NativeMethods;
using VSGenero.EditorExtensions;
using System.IO;
using VSGenero.Options;
using VSGenero.Snippets;
using VSGenero.VS2013_Specific;
using VSGenero.SqlSupport;

namespace VSGenero
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

    [ProvideEditorExtensionAttribute(typeof(EditorFactory), VSGeneroConstants.FileExtension4GL, 32)]
    [ProvideEditorExtensionAttribute(typeof(EditorFactory), VSGeneroConstants.FileExtensionPER, 32)]
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a701-06e5-11d1-8ebd-00a0c90f26ea}")]
    [PeekSupportedContentType(".4gl")]
    [RegisterSnippetsAttribute(VSGenero.Snippets.Constants.guidVSGeneroLanuageServiceString, false, 131, "Genero4GL", @"Snippets\CodeSnippets\SnippetsIndex.xml", @"Snippets\CodeSnippets\Snippets\", @"Snippets\CodeSnippets\Snippets\")]
    [ProvideLanguageService(typeof(VSGenero4GLLanguageInfo), VSGeneroConstants.LanguageName4GL, 106,
        //AutoOutlining = true,
        //EnableAsyncCompletion = true,
        //EnableCommenting = true,
                            RequestStockColors = true,
        //ShowSmartIndent = true,
                            ShowCompletion = true,
                            DefaultToInsertSpaces = true,
                            HideAdvancedMembersByDefault = true,
                            EnableAdvancedMembersOption = true,
                            ShowDropDownOptions = true)]
    [ProvideLanguageExtension(typeof(VSGenero4GLLanguageInfo), VSGeneroConstants.FileExtension4GL)]
    [ProvideLanguageService(typeof(VSGeneroPERLanguageInfo), VSGeneroConstants.LanguageNamePER, 107,
        //AutoOutlining = true,
        //EnableAsyncCompletion = true,
        //EnableCommenting = true,
                            RequestStockColors = true,
        //ShowSmartIndent = true,
                            ShowCompletion = true,
                            DefaultToInsertSpaces = true,
                            HideAdvancedMembersByDefault = true,
                            EnableAdvancedMembersOption = true,
                            ShowDropDownOptions = true)]
    [ProvideLanguageExtension(typeof(VSGeneroPERLanguageInfo), VSGeneroConstants.FileExtensionPER)]
    [ProvideLanguageEditorOptionPage(typeof(Genero4GLIntellisenseOptionsPage), VSGeneroConstants.LanguageName4GL, "", "Intellisense", "113")]
    [ProvideLanguageEditorOptionPage(typeof(Genero4GLAdvancedOptionsPage), VSGeneroConstants.LanguageName4GL, "", "Advanced", "114")]
    
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]

    // This causes the package to autoload when Visual Studio starts (guid for UICONTEXT_NoSolution)
    [ProvideAutoLoad("{adfc4e64-0397-11d1-9f4e-00a0c911004f}")]
    
    [Guid(GuidList.guidVSGeneroPkgString)]
    public sealed class VSGeneroPackage : VSCommonPackage
    {
        internal EditorFactory GeneroEditorFactory { get; private set; }

        private Genero4GLLanguagePreferences _langPrefs;
        internal Genero4GLLanguagePreferences LangPrefs
        {
            get
            {
                return _langPrefs;
            }
        }

        public new static VSGeneroPackage Instance;

        public Genero4GLIntellisenseOptionsPage IntellisenseOptions4GLPage
        {
            get
            {
                return (Genero4GLIntellisenseOptionsPage)GetDialogPage(typeof(Genero4GLIntellisenseOptionsPage));
            }
        }

        public Genero4GLAdvancedOptionsPage AdvancedOptions4GLPage
        {
            get
            {
                return (Genero4GLAdvancedOptionsPage)GetDialogPage(typeof(Genero4GLAdvancedOptionsPage));
            }
        }

        public VSGeneroPackage()
            : base()
        {
        }

        private IProgram4GLFileProvider _currentProgram4GLFileProvider;
        internal IProgram4GLFileProvider CurrentProgram4GLFileProvider
        {
            get { return _currentProgram4GLFileProvider; }
            set { _currentProgram4GLFileProvider = value; }
        }

        private GeneroProgramContentsManager _programContentsManager;
        internal GeneroProgramContentsManager ProgramContentsManager
        {
            get { return _programContentsManager; }
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            Instance = this;

            var langService4GL = new VSGenero4GLLanguageInfo(this);
            ((IServiceContainer)this).AddService(langService4GL.GetType(), langService4GL, true);
            var langServicePER = new VSGeneroPERLanguageInfo(this);
            ((IServiceContainer)this).AddService(langServicePER.GetType(), langServicePER, true);

            IVsTextManager textMgr = (IVsTextManager)Instance.GetService(typeof(SVsTextManager));
            var langPrefs = new LANGPREFERENCES[1];
            langPrefs[0].guidLang = typeof(VSGenero4GLLanguageInfo).GUID;
            int result = textMgr.GetUserPreferences(null, null, langPrefs, null);
            _langPrefs = new Genero4GLLanguagePreferences(langPrefs[0]);

            Guid guid = typeof(IVsTextManagerEvents2).GUID;
            IConnectionPoint connectionPoint;
            ((IConnectionPointContainer)textMgr).FindConnectionPoint(ref guid, out connectionPoint);
            uint cookie;
            connectionPoint.Advise(_langPrefs, out cookie);

            // TODO: not sure if this is needed...need to test
            DTE dte = (DTE)GetService(typeof(DTE));
            if (dte != null)
            {
                GeneroEditorFactory = new EditorFactory(this);
                this.RegisterEditorFactory(GeneroEditorFactory);
            }
            
            _programContentsManager = new GeneroProgramContentsManager();

            RegisterCommands(new CommonCommand[]
                {
                    new ExtractSqlStatementsCommand()
                }, GuidList.guidVSGeneroCmdSet);
        }

        #endregion

        private Dictionary<string, GeneroFileParserManager> _bufferFileParserManagers;
        public Dictionary<string, GeneroFileParserManager> BufferFileParserManagers
        {
            get
            {
                if (_bufferFileParserManagers == null)
                    _bufferFileParserManagers = new Dictionary<string, GeneroFileParserManager>();
                return _bufferFileParserManagers;
            }
        }

        private object bufferFileParserManagerLock = new object();

        public GeneroFileParserManager UpdateBufferFileParserManager(ITextBuffer buffer, string primarySibling = null)
        {
            GeneroFileParserManager fpm;
            lock (bufferFileParserManagerLock)
            {
                if (!buffer.Properties.TryGetProperty(typeof(GeneroFileParserManager), out fpm))
                {
                    string filename = buffer.GetFilePath();
                    // see if a file parser manager has been created for this file
                    if (VSGeneroPackage.Instance.BufferFileParserManagers.TryGetValue(filename, out fpm))
                    {
                        // use this file parser manager instead of a new one
                        fpm.UseNewBuffer(buffer);
                    }
                    else
                    {
                        fpm = new GeneroFileParserManager(buffer, primarySibling);
                        VSGeneroPackage.Instance.BufferFileParserManagers.Add(filename, fpm);
                    }
                    if (!buffer.Properties.ContainsProperty(typeof(GeneroFileParserManager)))
                    {
                        buffer.Properties.AddProperty(typeof(GeneroFileParserManager), fpm);
                    }
                }
            }
            return fpm;
        }

        public void RemoveBufferFileParserManager(ITextBuffer buffer)
        {
            GeneroFileParserManager fpm;
            if (buffer.Properties.TryGetProperty(typeof(GeneroFileParserManager), out fpm))
            {
                fpm.CancelParsing();
                string filename = buffer.GetFilePath();
                if (VSGeneroPackage.Instance.BufferFileParserManagers.ContainsKey(filename))
                    VSGeneroPackage.Instance.BufferFileParserManagers.Remove(filename);
                buffer.Properties.RemoveProperty(typeof(GeneroFileParserManager));
            }
        }

        public override Type GetLibraryManagerType()
        {
            return typeof(IGeneroLibraryManager);
        }

        public override bool IsRecognizedFile(string filename)
        {
            var ext = Path.GetExtension(filename);

            return String.Equals(ext, VSGeneroConstants.FileExtension4GL, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(ext, VSGeneroConstants.FileExtensionPER, StringComparison.OrdinalIgnoreCase);
        }

        public override LibraryManager CreateLibraryManager(CommonPackage package)
        {
            return new GeneroLibraryManager((VSGeneroPackage)package);
        }

        public object GetPackageService(Type t)
        {
            return Instance.GetService(t);
        }

        public Document ActiveDocument
        {
            get
            {
                EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
                return dte.ActiveDocument;
            }
        }
    }
}
