/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 * 
 * Contents of this file are based on the MSDN walkthrough here:
 * http://msdn.microsoft.com/en-us/library/ee334194.aspx
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;

namespace VSGenero.EditorExtensions.Intellisense
{
    internal class GeneroFunctionSignature : ISignature
    {
        private ITextBuffer m_subjectBuffer;
        private IParameter m_currentParameter;
        private string m_content;
        private string m_documentation;
        private ITrackingSpan m_applicableToSpan;
        private ReadOnlyCollection<IParameter> m_parameters;
        private string m_printContent;

        internal GeneroFunctionSignature(ITextBuffer subjectBuffer, string content, string doc, ReadOnlyCollection<IParameter> parameters)
        {
            m_subjectBuffer = subjectBuffer;
            m_content = content;
            m_documentation = doc;
            m_parameters = parameters;
            m_subjectBuffer.Changed += new EventHandler<TextContentChangedEventArgs>(OnSubjectBufferChanged);
        }

        internal void OnSubjectBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            this.ComputeCurrentParameter();
        }

        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;

        public IParameter CurrentParameter
        {
            get { return m_currentParameter; }
            internal set
            {
                if (m_currentParameter != value)
                {
                    IParameter prevCurrentParameter = m_currentParameter;
                    m_currentParameter = value;
                    this.RaiseCurrentParameterChanged(prevCurrentParameter, m_currentParameter);
                }
            }
        }

        private void RaiseCurrentParameterChanged(IParameter prevCurrentParameter, IParameter newCurrentParameter)
        {
            EventHandler<CurrentParameterChangedEventArgs> tempHandler = this.CurrentParameterChanged;
            if (tempHandler != null)
            {
                tempHandler(this, new CurrentParameterChangedEventArgs(prevCurrentParameter, newCurrentParameter));
            }
        }

        internal void SetCurrentParameter(IParameter newValue)
        {
            if (newValue != m_currentParameter)
            {
                var args = new CurrentParameterChangedEventArgs(m_currentParameter, newValue);
                m_currentParameter = newValue;
                var changed = CurrentParameterChanged;
                if (changed != null)
                {
                    changed(this, args);
                }
            }
        }

        internal void ComputeCurrentParameter()
        {
            if (Parameters.Count == 0)
            {
                this.CurrentParameter = null;
                return;
            }

            //the number of commas in the string is the index of the current parameter 
            string sigText = ApplicableToSpan.GetText(m_subjectBuffer.CurrentSnapshot);

            int currentIndex = 0;
            int commaCount = 0;
            while (currentIndex < sigText.Length)
            {
                int commaIndex = sigText.IndexOf(',', currentIndex);
                if (commaIndex == -1)
                {
                    break;
                }
                commaCount++;
                currentIndex = commaIndex + 1;
            }

            if (commaCount < Parameters.Count)
            {
                this.CurrentParameter = Parameters[commaCount];
            }
            else
            {
                //too many commas, so use the last parameter as the current one. 
                this.CurrentParameter = Parameters[Parameters.Count - 1];
            }
        }

        public ITrackingSpan ApplicableToSpan
        {
            get { return (m_applicableToSpan); }
            internal set { m_applicableToSpan = value; }
        }

        public string Content
        {
            get { return (m_content); }
            internal set { m_content = value; }
        }

        public string Documentation
        {
            get { return (m_documentation); }
            internal set { m_documentation = value; }
        }

        public ReadOnlyCollection<IParameter> Parameters
        {
            get { return (m_parameters); }
            internal set { m_parameters = value; }
        }

        public string PrettyPrintedContent
        {
            get { return (m_printContent); }
            internal set { m_printContent = value; }
        }
    }

    internal class GeneroFunctionParameter : IParameter
    {
        public string Documentation { get; private set; }
        public Span Locus { get; private set; }
        public string Name { get; private set; }
        public ISignature Signature { get; private set; }
        public Span PrettyPrintedLocus { get; private set; }

        public GeneroFunctionParameter(string documentation, Span locus, string name, ISignature signature)
        {
            Documentation = documentation;
            Locus = locus;
            Name = name;
            Signature = signature;
            PrettyPrintedLocus = locus;
        }
    }

    internal sealed class SignatureHelpCommandHandler : IOleCommandTarget
    {
        IOleCommandTarget m_nextCommandHandler;
        ITextView m_textView;
        ISignatureHelpBroker m_broker;
        ISignatureHelpSession m_session;
        ITextStructureNavigator m_navigator;

        internal SignatureHelpCommandHandler(IVsTextView textViewAdapter, ITextView textView, ITextStructureNavigator nav, ISignatureHelpBroker broker)
        {
            this.m_textView = textView;
            this.m_broker = broker;
            this.m_navigator = nav;

            //add this to the filter chain
            textViewAdapter.AddCommandFilter(this, out m_nextCommandHandler);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            char typedChar = char.MinValue;

            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
                if (typedChar.Equals('('))
                {
                    //move the point back so it's in the preceding word
                    //SnapshotPoint point = m_textView.Caret.Position.BufferPosition - 1;
                    //TextExtent extent = m_navigator.GetExtentOfWord(point);
                    //string word = extent.Span.GetText();
                    m_session = m_broker.TriggerSignatureHelp(m_textView);

                }
                else if (typedChar.Equals(')') && m_session != null)
                {
                    m_session.Dismiss();
                    m_session = null;
                }
            }
            else if (nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || nCmdID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (m_session != null && !m_session.IsDismissed)
                {
                    m_session.Dismiss();
                    m_session = null;
                }
            }
            return m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return m_nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }

    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Genero 4GL Signature Help controller")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    internal class SignatureHelpCommandProvider : IVsTextViewCreationListener
    {
        //[Import]
        //internal IVsEditorAdaptersFactoryService AdapterService;

        internal readonly IVsEditorAdaptersFactoryService _adaptersFactory;

        [ImportingConstructor]
        public SignatureHelpCommandProvider(IVsEditorAdaptersFactoryService adaptersFactory)
        {
            _adaptersFactory = adaptersFactory;
        }

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ISignatureHelpBroker SignatureHelpBroker;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = _adaptersFactory.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            textView.Properties.GetOrCreateSingletonProperty(
                 () => new SignatureHelpCommandHandler(textViewAdapter,
                    textView,
                    NavigatorService.GetTextStructureNavigator(textView.TextBuffer),
                    SignatureHelpBroker));
        }
    }

    [Export(typeof(ISignatureHelpSourceProvider))]
    [Name("Signature Help source")]
    [Order(Before = "default")]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    internal class SignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import(AllowDefault = true)]
        internal IPublicFunctionProvider PublicFunctionProvider { get; set; }

        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return new SignatureHelpSource(textBuffer, this);
        }
    }

    internal class SignatureHelpSource : ISignatureHelpSource
    {
        private ITextBuffer m_textBuffer;
        private GeneroModuleContents _moduleContents;
        private SignatureHelpSourceProvider _provider;

        public SignatureHelpSource(ITextBuffer textBuffer, SignatureHelpSourceProvider provider)
        {
            m_textBuffer = textBuffer;
            _provider = provider;
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            ITextSnapshot snapshot = m_textBuffer.CurrentSnapshot;
            int position = session.GetTriggerPoint(m_textBuffer).GetPosition(snapshot);

            ITrackingSpan applicableToSpan = m_textBuffer.CurrentSnapshot.CreateTrackingSpan(
             new Span(position, 0), SpanTrackingMode.EdgeInclusive, 0);

            SnapshotSpan wordSpan;
            string word = session.GetCurrentMemberOrMemberAccess(out wordSpan);
            if (word == null)
            {
                SnapshotPoint point = session.TextView.Caret.Position.BufferPosition - 1;
                ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(m_textBuffer);
                TextExtent extent = navigator.GetExtentOfWord(point);
                word = extent.Span.GetText();
            }
            var signature = CreateSignature(session, m_textBuffer, word, applicableToSpan);
            if (signature != null)
                signatures.Add(signature);
        }

        private ISignature CreateSignature(ISignatureHelpSession session, ITextBuffer textBuffer, string functionName, ITrackingSpan span)
        {
            GeneroFileParserManager fpm = null;
            FunctionDefinition funcDef = null;
            if (m_textBuffer.Properties.TryGetProperty(typeof(GeneroFileParserManager), out fpm))
            {
                _moduleContents = fpm.ModuleContents;
                funcDef = IntellisenseExtensions.DetermineContainingFunction((session.TextView.Caret.Position.BufferPosition) - 1, fpm);
            }

            GeneroModuleContents programContents;
            VSGeneroPackage.Instance.ProgramContentsManager.Programs.TryGetValue(m_textBuffer.GetProgram(), out programContents);

            // see if we can split up the functionName by '.' chars. If so, we have a member access
            string[] splitTokens = functionName.Split(new[] { '.' });

            // no need to look for completions here
            if (splitTokens.Length == 1)
            {
                // get the function from the file parser in the subjectBuffer
                if (fpm.ModuleContents.FunctionDefinitions.TryGetValue(functionName.ToLower(), out funcDef) ||
                    (programContents != null && programContents.FunctionDefinitions.TryGetValue(functionName.ToLower(), out funcDef)))
                {
                    string methodSig = funcDef.GetIntellisenseText();
                    return GetSignature(methodSig, textBuffer, span);
                }
                else
                {
                    
                    if (_provider.PublicFunctionProvider != null)
                    {
                        var methodSig = _provider.PublicFunctionProvider.GetPublicFunctionSignature(functionName.ToLower());
                        if (methodSig != null)
                        {
                            return GetSignature(methodSig, textBuffer, span);
                        }
                    }

                    GeneroSystemClassFunction sysClassFunc;
                    if (GeneroSingletons.LanguageSettings.NativeMethods.TryGetValue(functionName.ToLower(), out sysClassFunc))
                    {
                        return GetSignature(sysClassFunc.GetIntellisenseText(), textBuffer, span);
                    }

                    GeneroOperator sysOperator;
                    if (GeneroSingletons.LanguageSettings.NativeOperators.TryGetValue(functionName.ToLower(), out sysOperator))
                    {
                        return GetSignature(sysOperator.GetIntellisenseText(), textBuffer, span);
                    }
                }
            }
            else
            {
                GeneroPackage tmpPackage = null;
                GeneroClass tmpClass = null;
                GeneroClassMethod tmpMethod = null;
                VariableDefinition varDef = null;
                GeneroSystemClass sysClass = null;
                GeneroSystemClassFunction sysClassFunc = null;
                ArrayElement arrayElement = null;
                ArrayElement prevArrayElement = null;
                for (int i = 0; i < splitTokens.Length; i++)
                {
                    string lowercase = splitTokens[i].ToLower();
                    if ((arrayElement = IntellisenseExtensions.GetArrayElement(splitTokens[i])) != null)
                    {
                        if (arrayElement.IsComplete)
                            splitTokens[i] = arrayElement.ArrayName;
                        else if (arrayElement.Indices.Count > 0 && !string.IsNullOrWhiteSpace(arrayElement.Indices[arrayElement.Indices.Count - 1]))
                            splitTokens[i] = arrayElement.Indices[arrayElement.Indices.Count - 1];
                    }

                    // look for a package
                    if (tmpPackage == null &&
                       GeneroSingletons.LanguageSettings.Packages.TryGetValue(lowercase, out tmpPackage))
                    {
                        continue;
                    }

                    // look for a class
                    if (tmpPackage != null && tmpClass == null &&
                       tmpPackage.Classes.TryGetValue(lowercase, out tmpClass))
                    {
                        continue;
                    }

                    // look for a class method
                    if (tmpClass != null && tmpMethod == null &&
                        tmpClass.Methods.TryGetValue(lowercase, out tmpMethod))
                    {
                        string methodSig = tmpMethod.GetIntellisenseText();
                        return GetSignature(methodSig, textBuffer, span);
                    }

                    if (i == 0)
                    {
                        if (funcDef != null)
                        {
                            if (!funcDef.Variables.TryGetValue(lowercase, out varDef))
                            {
                                // look in module variables
                                if (!_moduleContents.ModuleVariables.TryGetValue(lowercase, out varDef))
                                {
                                    if (!_moduleContents.GlobalVariables.TryGetValue(lowercase, out varDef) &&
                                        (programContents != null && !programContents.GlobalVariables.TryGetValue(lowercase, out varDef)))
                                    {
                                        GeneroSingletons.SystemVariables.TryGetValue(lowercase, out varDef);
                                    }
                                }
                            }
                        }

                        if (varDef != null)
                        {
                            if (varDef.ArrayType == ArrayType.None || (arrayElement != null && arrayElement.IsComplete))
                            {
                                TypeDefinition typeDef;
                                // check for a type definition
                                if (_moduleContents.GlobalTypes.TryGetValue(varDef.Type.ToLower(), out typeDef) ||
                                    (programContents != null && programContents.GlobalTypes.TryGetValue(varDef.Type.ToLower(), out typeDef)) ||
                                    _moduleContents.ModuleTypes.TryGetValue(varDef.Type.ToLower(), out typeDef) ||
                                    (programContents != null && programContents.ModuleTypes.TryGetValue(varDef.Type.ToLower(), out typeDef)) ||
                                    funcDef.Types.TryGetValue(varDef.Type.ToLower(), out typeDef))
                                {
                                    varDef = typeDef;
                                }
                            }
                        }
                    }
                    
                    if (varDef != null)
                    {
                        if ((arrayElement == null || !arrayElement.IsComplete) &&
                            (!string.IsNullOrWhiteSpace(varDef.Type) || varDef.ArrayType != ArrayType.None))
                        {
                            // get an extension or base class
                            GeneroClass generoClass = null;
                            if (IntellisenseExtensions.IsClassInstance(varDef, out generoClass))
                            {
                                GeneroClassMethod classMethod;
                                if (generoClass.Methods.TryGetValue(lowercase, out classMethod))
                                {
                                    string methodSig = classMethod.GetIntellisenseText();
                                    return GetSignature(methodSig, textBuffer, span);
                                }
                            }

                            if(varDef.ArrayType != ArrayType.None && 
                                (arrayElement == null || !arrayElement.IsComplete) &&
                                 GeneroSingletons.LanguageSettings.NativeClasses.TryGetValue("array", out sysClass))
                            {
                                if (sysClass.Functions.TryGetValue(lowercase, out sysClassFunc))
                                {
                                    string methodSig = sysClassFunc.GetIntellisenseText();
                                    return GetSignature(methodSig, textBuffer, span);
                                }
                            }


                            // get a system class
                            if (GeneroSingletons.LanguageSettings.NativeClasses.TryGetValue(varDef.Type, out sysClass))
                            {
                                if (sysClass.Functions.TryGetValue(lowercase, out sysClassFunc))
                                {
                                    string methodSig = sysClassFunc.GetIntellisenseText();
                                    return GetSignature(methodSig, textBuffer, span);
                                }
                            }
                        }

                        prevArrayElement = arrayElement;
                        // could be a record child, so set the varDef for the next pass
                        if (varDef.RecordElements != null && varDef.RecordElements.Count > 0)
                        {
                            VariableDefinition tempVarDef;
                            if (varDef.RecordElements.TryGetValue(lowercase, out tempVarDef))
                                varDef = tempVarDef;
                        }
                    }
                }
            }

            return null;
        }

        private ISignature GetSignature(string methodSignature, ITextBuffer textBuffer, ITrackingSpan span)
        {
            string[] pars = methodSignature.Split(new char[] { '(', ',', ')' });
            GeneroFunctionSignature sig = new GeneroFunctionSignature(textBuffer, methodSignature, "", null);
            textBuffer.Changed += new EventHandler<TextContentChangedEventArgs>(sig.OnSubjectBufferChanged);

            //find the parameters in the method signature (expect methodname(one, two) 
            List<IParameter> paramList = new List<IParameter>();
            int locusSearchStart = 0;
            for (int i = 1; i < pars.Length; i++)
            {
                string param = pars[i].Trim();

                if (string.IsNullOrEmpty(param))
                    continue;

                //find where this parameter is located in the method signature 
                int locusStart = methodSignature.IndexOf(param, locusSearchStart);
                if (locusStart >= 0)
                {
                    Span locus = new Span(locusStart, param.Length);
                    locusSearchStart = locusStart + param.Length;
                    paramList.Add(new GeneroFunctionParameter("", locus, param, sig));
                }
            }
            sig.Parameters = new ReadOnlyCollection<IParameter>(paramList);
            sig.ApplicableToSpan = span;
            sig.ComputeCurrentParameter();
            return sig;
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            if (session.Signatures.Count > 0)
            {
                ITrackingSpan applicableToSpan = session.Signatures[0].ApplicableToSpan;
                string text = applicableToSpan.GetText(applicableToSpan.TextBuffer.CurrentSnapshot);

                if (text.Trim().Equals("add"))  //get only "add"  
                    return session.Signatures[0];
            }
            return null;
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}
