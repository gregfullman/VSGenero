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
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Timers;
using System.ComponentModel;
using System.ComponentModel.Composition;
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
#if DEV12_OR_LATER
using VSGenero.Peek;
#endif
using VSGenero.SqlSupport;
using System.Reflection;
using VSGenero.EditorExtensions.Intellisense;
using Microsoft.VisualStudio.Text.Adornments;
using VSGenero.Analysis;
using VSGenero.EditorExtensions.BraceCompletion;
using System.ComponentModel.Composition.Hosting;

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
    [ProvideEditorExtensionAttribute(typeof(EditorFactory), VSGeneroConstants.FileExtensionINC, 32)]
    [ProvideEditorExtensionAttribute(typeof(EditorFactory), VSGeneroConstants.FileExtensionPER, 32)]
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a701-06e5-11d1-8ebd-00a0c90f26ea}")]
#if DEV12_OR_LATER
    [PeekSupportedContentType(".4gl")]
    [PeekSupportedContentType(".inc")]
#endif  
    [RegisterSnippetsAttribute(VSGeneroConstants.guidGenero4glLanguageService, false, 131, "Genero4GL", @"Snippets\CodeSnippets\SnippetsIndex.xml", @"Snippets\CodeSnippets\Snippets\", @"Snippets\CodeSnippets\Snippets\")]
    [ProvideLanguageService(typeof(VSGenero4GLLanguageInfo), VSGeneroConstants.LanguageName4GL, 106,
                            RequestStockColors = true,
                            ShowSmartIndent = true,       // enable this when we want to support smart indenting
                            ShowCompletion = true,
                            DefaultToInsertSpaces = true,
                            HideAdvancedMembersByDefault = true,
                            EnableAdvancedMembersOption = true,
                            ShowDropDownOptions = true)]
    [LanguageBraceCompletion(VSGeneroConstants.LanguageName4GL, EnableCompletion = true)]
    [ProvideLanguageExtension(typeof(VSGenero4GLLanguageInfo), VSGeneroConstants.FileExtension4GL)]
    [ProvideLanguageService(typeof(VSGeneroPERLanguageInfo), VSGeneroConstants.LanguageNamePER, 107,
                            RequestStockColors = true,
                            //ShowSmartIndent = true,       // enable this when we want to support smart indenting
                            ShowCompletion = true,
                            DefaultToInsertSpaces = true,
                            HideAdvancedMembersByDefault = true,
                            EnableAdvancedMembersOption = true,
                            ShowDropDownOptions = true)]
    [LanguageBraceCompletion(VSGeneroConstants.LanguageNamePER, EnableCompletion = true)]
    [ProvideLanguageExtension(typeof(VSGeneroPERLanguageInfo), VSGeneroConstants.FileExtensionPER)]
    [ProvideLanguageService(typeof(VSGeneroINCLanguageInfo), VSGeneroConstants.LanguageNameINC, 108,
                            RequestStockColors = true,
                            ShowSmartIndent = true,       // enable this when we want to support smart indenting
                            ShowCompletion = true,
                            DefaultToInsertSpaces = true,
                            HideAdvancedMembersByDefault = true,
                            EnableAdvancedMembersOption = true,
                            ShowDropDownOptions = true)]
    [LanguageBraceCompletion(VSGeneroConstants.LanguageNameINC, EnableCompletion = true)]
    [ProvideLanguageExtension(typeof(VSGeneroINCLanguageInfo), VSGeneroConstants.FileExtensionINC)]
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
        private GeneroProjectAnalyzer _analyzer;

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
            Instance = this;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private List<string> _manuallyLoadedDlls = new List<string>();
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                return null;
            }
            else
            {
                int index = args.Name.IndexOf(',');
                if (index < 0)
                    index = args.Name.Length;
                var assemblyName = args.Name.Substring(0, index) + ".dll";
                if (!_manuallyLoadedDlls.Contains(assemblyName))
                {
                    string asmLocation = Assembly.GetExecutingAssembly().Location;
                    asmLocation = Path.GetDirectoryName(asmLocation);
                    string filename = Path.Combine(asmLocation, assemblyName);

                    if (File.Exists(filename))
                    {
                        _manuallyLoadedDlls.Add(assemblyName);
                        return Assembly.LoadFrom(filename);
                    }
                }
            }
            return null;
        }

        internal IBuildTaskProvider _BuildTaskProvider;

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
            var services = (IServiceContainer)this;

            var langService4GL = new VSGenero4GLLanguageInfo(this);
            services.AddService(langService4GL.GetType(), langService4GL, true);
            var langServicePER = new VSGeneroPERLanguageInfo(this);
            services.AddService(langServicePER.GetType(), langServicePER, true);

            services.AddService(
                typeof(ErrorTaskProvider),
                (container, serviceType) =>
                {
                    var errorList = GetService(typeof(SVsErrorList)) as IVsTaskList;
                    var model = ComponentModel;
                    var errorProvider = model != null ? model.GetService<IErrorProviderFactory>() : null;
                    return new ErrorTaskProvider(this, errorList, errorProvider);
                },
                promote: true);

            services.AddService(
                typeof(CommentTaskProvider),
                (container, serviceType) =>
                {
                    var taskList = GetService(typeof(SVsTaskList)) as IVsTaskList;
                    var model = ComponentModel;
                    var errorProvider = model != null ? model.GetService<IErrorProviderFactory>() : null;
                    return new CommentTaskProvider(this, taskList, errorProvider);
                },
                promote: true);

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

            RegisterCommands(new CommonCommand[]
                {
                    new ExtractSqlStatementsCommand()
                }, GuidList.guidVSGeneroCmdSet);

            TestCompletionAnalysis.InitializeResults();

            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var sp = new ServiceProvider(dte2 as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            var mefContainer = sp.GetService(typeof(Microsoft.VisualStudio.ComponentModelHost.SComponentModel))
                                as Microsoft.VisualStudio.ComponentModelHost.IComponentModel;
            var exportSpec = mefContainer.DefaultExportProvider.GetExport<IBuildTaskProvider>();
            if(exportSpec != null)
            {
                _BuildTaskProvider = exportSpec.Value;
            }
            if (DefaultAnalyzer != null)
            { }
        }

        #endregion

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

        private GeneroLibraryManager _generoLibManager;
        public override LibraryManager CreateLibraryManager(CommonPackage package)
        {
            if (_generoLibManager == null)
                _generoLibManager = new GeneroLibraryManager((VSGeneroPackage)package);
            return _generoLibManager;
        }

        public void LoadLibraryManager()
        {
            this.CreateService(null, typeof(IGeneroLibraryManager));
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

        /// <summary>
        /// The analyzer which is used for loose files.
        /// </summary>
        internal GeneroProjectAnalyzer DefaultAnalyzer
        {
            get
            {
                if (_analyzer == null)
                {
                    _analyzer = CreateAnalyzer();
                }
                return _analyzer;
            }
        }

        internal void RecreateAnalyzer()
        {
            if (_analyzer != null)
            {
                _analyzer.Dispose();
            }
            _analyzer = CreateAnalyzer();
        }

        private GeneroProjectAnalyzer CreateAnalyzer()
        {
            return new GeneroProjectAnalyzer(this, _BuildTaskProvider);
        }

        private IFunctionInformationProvider _functionProvider;
        public IFunctionInformationProvider GlobalFunctionProvider
        {
            get { return _functionProvider; }
            set { _functionProvider = value; }
        }

        private IDatabaseInformationProvider _dbProvider;
        public IDatabaseInformationProvider GlobalDatabaseProvider
        {
            get { return _dbProvider; }
            set { _dbProvider = value; }
        }

        private IEnumerable<ICommentValidator> _commentValidators;
        public IEnumerable<ICommentValidator> CommentValidators
        {
            get { return _commentValidators; }
            set { _commentValidators = value; }
        }

        #region Program File Provider

        private object _programFileProviderLock = new object();
        private IProgramFileProvider _programFileProvider;
        public IProgramFileProvider ProgramFileProvider
        {
            get { return _programFileProvider; }
            set 
            {
                lock (_programFileProviderLock)
                {
                    if (_programFileProvider == null)
                    {
                        _programFileProvider = value;
                        if (_programFileProvider != null)
                        {
                            _programFileProvider.ImportModuleLocationChanged += _programFileProvider_ImportModuleLocationChanged;
                            _programFileProvider.IncludeFileLocationChanged += _programFileProvider_IncludeFileLocationChanged;
                        }
                    }
                }
            }
        }

        void _programFileProvider_IncludeFileLocationChanged(object sender, IncludeFileLocationChangedEventArgs e)
        {
            DefaultAnalyzer.UpdateIncludedFile(e.NewLocation);
        }

        void _programFileProvider_ImportModuleLocationChanged(object sender, ImportModuleLocationChangedEventArgs e)
        {
            DefaultAnalyzer.UpdateImportedProject(e.ImportModule, e.NewLocation);
        }

        #endregion

        private List<IContentType> _programCodeContentTypes;
        public IList<IContentType> ProgramCodeContentTypes
        {
            get
            {
                if (_programCodeContentTypes == null)
                {
                    var regSvc = ComponentModel.GetService<IContentTypeRegistryService>();
                    _programCodeContentTypes = new List<IContentType>(); 
                    _programCodeContentTypes.Add(regSvc.GetContentType(VSGeneroConstants.ContentType4GL));
                    _programCodeContentTypes.Add(regSvc.GetContentType(VSGeneroConstants.ContentTypeINC));
                    _programCodeContentTypes.Add(regSvc.GetContentType(VSGeneroConstants.ContentTypePER));
                }
                return _programCodeContentTypes;
            }
        }

        internal static ITextBuffer GetBufferForDocument(System.IServiceProvider serviceProvider, string filename)
        {
            IVsTextView viewAdapter;
            IVsWindowFrame frame;
            VsUtilities.OpenDocument(serviceProvider, filename, out viewAdapter, out frame);

            IVsTextLines lines;
            ErrorHandler.ThrowOnFailure(viewAdapter.GetBuffer(out lines));

            var adapter = serviceProvider.GetComponentModel().GetService<IVsEditorAdaptersFactoryService>();

            return adapter.GetDocumentBuffer(lines);
        }
    }
}
