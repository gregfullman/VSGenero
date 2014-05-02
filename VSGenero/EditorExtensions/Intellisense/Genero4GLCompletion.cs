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
 * http://msdn.microsoft.com/en-us/library/ee372314.aspx
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.VSCommon;

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(ICompletionSourceProvider)), Order(Before = "default"), Name("VSGenero 4GL Completion Source"), ContentType(VSGeneroConstants.ContentType4GL)]
    internal class Genero4GLCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal IGlyphService _glyphService = null; // Assigned from MEF

        [Import(AllowDefault = true)]
        internal IDatabaseInformationProvider DbInformationProvider;

        [Import(AllowDefault = true)]
        internal IPublicFunctionProvider PublicFunctionProvider;

        public IGlyphService GlyphService
        {
            get { return _glyphService; }
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new Genero4GLCompletionSource(this, textBuffer);
        }
    }

    internal class Genero4GLCompletionSource : ICompletionSource
    {
        private Genero4GLCompletionSourceProvider m_sourceProvider;
        private ITextBuffer m_textBuffer;
        private List<MemberCompletion> m_compList;
        private GeneroModuleContents _moduleContents;
        private readonly int _removeFromIndex = 0;
        private readonly Genero4GLCompletionContextAnalyzer _completionContextAnalyzer;

        public Genero4GLCompletionSource(Genero4GLCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            m_sourceProvider = sourceProvider;
            _completionContextAnalyzer = new Genero4GLCompletionContextAnalyzer(m_sourceProvider.GlyphService,
                                                                                m_sourceProvider.DbInformationProvider,
                                                                                m_sourceProvider.PublicFunctionProvider);
            m_textBuffer = textBuffer;

            GeneroFileParserManager fpm = VSGeneroPackage.Instance.UpdateBufferFileParserManager(m_textBuffer);
            fpm.ParseComplete += CompletionSource_ParseComplete;
            _moduleContents = fpm.ModuleContents;

            m_compList = new List<MemberCompletion>();

            // Add system keywords
            foreach (var keyword in GeneroSingletons.LanguageSettings.KeywordMap)
            {
                var comp = new MemberCompletion(keyword.Key, keyword.Key, keyword.Key,
                                m_sourceProvider.GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic), null);
                m_compList.Add(comp);
            }

            // add Genero Package names
            foreach (var packageName in GeneroSingletons.LanguageSettings.Packages)
            {
                var comp = new MemberCompletion(packageName.Key, packageName.Key, packageName.Value.Description,
                                    m_sourceProvider.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic), null);
                m_compList.Add(comp);
            }
            _removeFromIndex = m_compList.Count;
        }

        void CompletionSource_ParseComplete(object sender, ParseCompleteEventArgs e)
        {
            _moduleContents = (sender as GeneroFileParserManager).ModuleContents;

            // update the completion list
            //m_compList.RemoveAll(x => !(bool)x.Properties["system"]);

            //// add global variables
            //foreach (var globalVar in _moduleContents.GlobalVariables)
            //{
            //    var comp = new MemberCompletion(globalVar.Key, globalVar.Key, globalVar.Key,
            //                    m_sourceProvider.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic), null);
            //    comp.Properties.AddProperty("system", false);
            //    m_compList.Add(comp);
            //}

            //// add the module variables
            //foreach (var moduleVar in _moduleContents.ModuleVariables)
            //{
            //    var comp = new MemberCompletion(moduleVar.Key, moduleVar.Key, moduleVar.Key,
            //                    m_sourceProvider.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemInternal), null);
            //    comp.Properties.AddProperty("system", false);
            //    m_compList.Add(comp);
            //}
        }



        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            ITrackingSpan applicableSpan = FindTokenSpanAtPosition(session.GetTriggerPoint(m_textBuffer), session);
            GeneroFileParserManager fpm = null;
            if (m_textBuffer.Properties.TryGetProperty(typeof(GeneroFileParserManager), out fpm))
            {
                _moduleContents = fpm.ModuleContents;
            }
            if (_moduleContents != null)
            {
                bool isMemberCompletion = false;
                bool isDefiniteFunctionOrReportCall = false;
                List<MemberCompletion> contextCompletions = _completionContextAnalyzer.AnalyzeContext(session, applicableSpan, fpm, _moduleContents, out isMemberCompletion, out isDefiniteFunctionOrReportCall);
                if (contextCompletions.Count > 0)
                {
                    List<MemberCompletion> completionListToUse = null;
                    if (isMemberCompletion)
                    {
                        applicableSpan = GetApplicableSpan(session, m_textBuffer);
                        completionListToUse = contextCompletions;
                    }
                    else
                    {
                        m_compList.RemoveRange(_removeFromIndex, m_compList.Count - _removeFromIndex);
                        if (isDefiniteFunctionOrReportCall)
                        {
                            completionListToUse = contextCompletions;
                        }
                        else
                        {
                            m_compList.AddRange(contextCompletions);
                            completionListToUse = m_compList;
                        }
                    }
                    MemberCompletionSet memberCompletionSet =
                            new MemberCompletionSet("Tokens",    //the non-localized title of the tab 
                                                    "Tokens",    //the display title of the tab
                                                    applicableSpan,
                                                    completionListToUse,
                                                    CompletionComparer.UnderscoresLast);
                    completionSets.Add(memberCompletionSet);
                }
            }
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = m_sourceProvider.NavigatorService.GetTextStructureNavigator(m_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        /// <summary>
        /// Returns the span to use for the provided intellisense session.
        /// </summary>
        /// <returns>A tracking span. The span may be of length zero if there
        /// is no suitable token at the trigger point.</returns>
        internal ITrackingSpan GetApplicableSpan(IIntellisenseSession session, ITextBuffer buffer)
        {
            var snapshot = buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(buffer);

            var span = snapshot.GetApplicableSpan(triggerPoint);
            if (span != null)
            {
                return span;
            }
            return snapshot.CreateTrackingSpan(triggerPoint.GetPosition(snapshot), 0, SpanTrackingMode.EdgeInclusive);
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
