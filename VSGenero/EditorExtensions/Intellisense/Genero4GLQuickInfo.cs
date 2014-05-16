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
 * http://msdn.microsoft.com/en-us/library/ee197646.aspx
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Editor;

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(IQuickInfoSourceProvider)), ContentType(VSGeneroConstants.ContentType4GL), Order, Name("Genero4GL Quick Info Source")]
    internal class Genero4GLQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        [Import(AllowDefault = true)]
        internal IDatabaseInformationProvider DbInformationProvider { get; set; }

        [Import(AllowDefault = true)]
        internal IPublicFunctionProvider PublicFunctionProvider { get; set; }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new Genero4GLQuickInfoSource(this, textBuffer);
        }
    }

    internal class Genero4GLQuickInfoSource : IQuickInfoSource
    {
        private Genero4GLQuickInfoSourceProvider m_provider;
        private ITextBuffer m_subjectBuffer;

        public Genero4GLQuickInfoSource(Genero4GLQuickInfoSourceProvider provider, ITextBuffer subjectBuffer)
        {
            m_provider = provider;
            m_subjectBuffer = subjectBuffer;
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> qiContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;
            // Map the trigger point down to our buffer.
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(m_subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                return;
            }
            if (subjectTriggerPoint.Value.IsWithinComment() ||
                subjectTriggerPoint.Value.IsWithinString())
            {
                return;
            }

            ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

            //look for occurrences of our QuickInfo words in the span
            SnapshotSpan textSpan;

            // get the word we're hovering over
            ITextStructureNavigator navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_subjectBuffer);
            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
            string searchText = extent.Span.GetText();
            textSpan = extent.Span;

            SnapshotSpan dummySpan;
            // try to get any member access associated with the text we're hovering over
            searchText = subjectTriggerPoint.Value.GetQuickInfoMemberOrMemberAccess(out dummySpan, searchText);
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string[] splitTokens = searchText.Split(new[] { '.' });
                // TODO: check for any record variables, tables, package names, or class variables in splitTokens[0]
                // TODO: if found, take a look at the members
                GeneroPackage tmpPackage = null;
                GeneroClass tmpClass = null;
                GeneroClassMethod tmpMethod = null;
                GeneroSystemClass tmpSysClass = null;
                GeneroSystemClassFunction tmpSysClassFunction = null;
                VariableDefinition tempDef = null;
                VariableDefinition parentDef = null;
                FunctionDefinition funcDef = null;
                CursorPreparation cursorPrep = null;
                TempTableDefinition tempTableDef = null;
                ConstantDefinition constantDef = null;
                TypeDefinition typeDef = null;
                GeneroOperator generoOperator = null;
                string publicFunctionQuickInfo = null;
                string context = null;
                string finalMatchType = null;
                bool continueMatching = true;
                GeneroTableColumn columnOrRecordField = null;
                for (int i = 0; i < splitTokens.Length; i++)
                {
                    ArrayElement arrayElement = null;
                    if ((arrayElement = IntellisenseExtensions.GetArrayElement(splitTokens[i])) != null)
                    {
                        // for now, just replace
                        if (arrayElement.IsComplete)
                            splitTokens[i] = arrayElement.ArrayName;
                        else if (arrayElement.Indices.Count > 0 && !string.IsNullOrWhiteSpace(arrayElement.Indices[arrayElement.Indices.Count - 1]))
                            splitTokens[i] = arrayElement.Indices[arrayElement.Indices.Count - 1];
                    }

                    // Take care of packages, classes, and methods
                    if (tmpClass != null && i == 2)
                    {
                        if (tmpClass.Methods.TryGetValue(splitTokens[i].ToLower(), out tmpMethod))
                        {
                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                        (
                                            textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                        );
                            if (i + 1 == splitTokens.Length)
                            {
                                continueMatching = false;
                                finalMatchType = "tmpMethod";
                            }
                        }
                    }
                    if (tmpPackage != null && i == 1)
                    {
                        if (tmpPackage.Classes.TryGetValue(splitTokens[i].ToLower(), out tmpClass))
                        {
                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                        (
                                            textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                        );
                            if (i + 1 == splitTokens.Length)
                            {
                                continueMatching = false;
                                finalMatchType = "tmpClass";
                            }
                        }
                    }
                    if (i == 0)
                    {
                        if (GeneroSingletons.LanguageSettings.Packages.TryGetValue(splitTokens[i].ToLower(), out tmpPackage))
                        {
                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                        (
                                            textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                        );
                            if (i + 1 == splitTokens.Length)
                            {
                                continueMatching = false;
                                finalMatchType = "tmpPackage";
                            }
                        }
                        else if (GeneroSingletons.LanguageSettings.NativeClasses.TryGetValue(splitTokens[i].ToLower(), out tmpSysClass))
                        {
                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                        (
                                            textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                        );
                            if (i + 1 == splitTokens.Length)
                            {
                                continueMatching = false;
                                finalMatchType = "tmpSysClass";
                            }
                        }
                    }

                    if (tmpPackage == null &&
                       tmpClass == null &&
                       tmpMethod == null)
                    {
                        // check globals and module variables first
                        var fpm = m_subjectBuffer.Properties.GetProperty(typeof(GeneroFileParserManager)) as GeneroFileParserManager;
                        if (fpm != null)
                        {
                            GeneroModuleContents programContents;
                            VSGeneroPackage.Instance.ProgramContentsManager.Programs.TryGetValue(m_subjectBuffer.GetProgram(), out programContents);
                            // look at the variables in the current function
                            funcDef = IntellisenseExtensions.DetermineContainingFunction(subjectTriggerPoint.Value, fpm);
                            if (i == 0)
                            {
                                if (funcDef != null)
                                {
                                    TryGetFunctionElement(splitTokens[i], funcDef, ref tempDef, ref constantDef, ref typeDef, ref context);
                                }
                                if (tempDef == null)
                                {
                                    if (!TryGetModuleOrGlobalElement(splitTokens[i], fpm, programContents, ref tempDef, ref constantDef, ref typeDef, ref context))
                                    {
                                        int recordElementPos = extent.Span.Start.Position;
                                        // Could be hovering over a record variable's child element in the definition
                                        var owningRecordVar = fpm.ModuleContents.ModuleVariables.FirstOrDefault(x =>
                                                x.Value.IsRecordType &&
                                                x.Value.RecordElements.Any(
                                                    y => y.Value.Position == recordElementPos));
                                        if (!owningRecordVar.Equals(default(KeyValuePair<string, VariableDefinition>)))
                                        {
                                            tempDef = owningRecordVar.Value.RecordElements[splitTokens[i]];
                                        }
                                    }
                                }
                                if (tempDef != null)
                                {
                                    applicableToSpan = currentSnapshot.CreateTrackingSpan
                                        (
                                            textSpan.Start.Position, tempDef.Name.Length, SpanTrackingMode.EdgeInclusive
                                        );
                                    if (i + 1 == splitTokens.Length)
                                    {
                                        continueMatching = false;
                                        finalMatchType = "tempDef";
                                    }
                                }
                                else if (constantDef != null)
                                {
                                    applicableToSpan = currentSnapshot.CreateTrackingSpan
                                        (
                                            textSpan.Start.Position, constantDef.Name.Length, SpanTrackingMode.EdgeInclusive
                                        );
                                    if (i + 1 == splitTokens.Length)
                                    {
                                        continueMatching = false;
                                        finalMatchType = "constantDef";
                                    }
                                }
                                else if (typeDef != null)
                                {
                                    applicableToSpan = currentSnapshot.CreateTrackingSpan
                                        (
                                            textSpan.Start.Position, typeDef.Name.Length, SpanTrackingMode.EdgeInclusive
                                        );
                                    if (i + 1 == splitTokens.Length)
                                    {
                                        continueMatching = false;
                                        finalMatchType = "typeDef";
                                    }
                                }
                                if (tempDef == null && constantDef == null && typeDef == null)
                                {
                                    // If we got down to here, it might be a function?
                                    if (fpm.ModuleContents.FunctionDefinitions.TryGetValue(splitTokens[i], out funcDef) ||
                                        (programContents != null && programContents.FunctionDefinitions.TryGetValue(splitTokens[i], out funcDef)))
                                    {
                                        string functionName = funcDef.Name;
                                        if (functionName == null && funcDef.Main)
                                            functionName = "main";
                                        applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, functionName.Length, SpanTrackingMode.EdgeInclusive
                                                );
                                        if (i + 1 == splitTokens.Length)
                                        {
                                            continueMatching = false;
                                            finalMatchType = "funcDef";
                                        }
                                    }
                                    else
                                    {
                                        if (m_provider.PublicFunctionProvider != null)
                                        {
                                            publicFunctionQuickInfo = m_provider.PublicFunctionProvider.GetPublicFunctionQuickInfo(splitTokens[i]);
                                            if (publicFunctionQuickInfo != null)
                                            {
                                                applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                        (
                                                            textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                        );
                                                if (i + 1 == splitTokens.Length)
                                                {
                                                    continueMatching = false;
                                                    finalMatchType = "publicFunctionQuickInfo";
                                                }
                                            }
                                        }

                                        // look for a system function
                                        if (GeneroSingletons.LanguageSettings.NativeMethods.TryGetValue(splitTokens[i], out tmpSysClassFunction))
                                        {
                                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                        (
                                                            textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                        );
                                            if (i + 1 == splitTokens.Length)
                                            {
                                                continueMatching = false;
                                                finalMatchType = "tmpSysClassFunction";
                                            }
                                        }

                                        if (GeneroSingletons.LanguageSettings.NativeOperators.TryGetValue(splitTokens[i], out generoOperator))
                                        {
                                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                        (
                                                            textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                        );
                                            if (i + 1 == splitTokens.Length)
                                            {
                                                continueMatching = false;
                                                finalMatchType = "generoOperator";
                                            }
                                        }

                                        // look at the cursor definitions
                                        CursorDeclaration cursorDecl;
                                        string searchName = splitTokens[i];
                                        if (fpm.ModuleContents.SqlCursors.TryGetValue(searchName, out cursorDecl))
                                        {
                                            // TODO: will have to rework this a bit when we support other types of cursor declarations
                                            searchName = cursorDecl.PreparationVariable;
                                        }
                                        if (fpm.ModuleContents.SqlPrepares.TryGetValue(searchName, out cursorPrep))
                                        {
                                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, searchName.Length, SpanTrackingMode.EdgeInclusive
                                                );
                                            if (i + 1 == splitTokens.Length)
                                            {
                                                continueMatching = false;
                                                finalMatchType = "cursorPrep";
                                            }
                                        }

                                        if (fpm.ModuleContents.TempTables.TryGetValue(searchName, out tempTableDef))
                                        {
                                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, searchName.Length, SpanTrackingMode.EdgeInclusive
                                                );
                                            if (i + 1 == splitTokens.Length)
                                            {
                                                continueMatching = false;
                                                finalMatchType = "tempTableDef";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // we're on a member access, so we're limited to mimic types, record types, and (when supported) system types (i.e. sqlca).
                                // We can also be hovering over a function belonging to a class instance
                                if (tempDef != null)
                                {
                                    GeneroClass generoClass = null;
                                    if (tempDef.IsMimicType && i == 1)  // can't go more than one level deep
                                    {
                                        if (m_provider.DbInformationProvider != null)
                                        {
                                            columnOrRecordField = m_provider.DbInformationProvider.GetTableColumn(tempDef.MimicTypeTable, splitTokens[i]);
                                            if (columnOrRecordField != null)
                                            {
                                                applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                );
                                                if (i + 1 == splitTokens.Length)
                                                {
                                                    continueMatching = false;
                                                    finalMatchType = "columnOrRecordField";
                                                }
                                            }
                                        }
                                    }
                                    if (continueMatching && tempDef.ArrayType != ArrayType.None)
                                    {
                                        // could be hovering over an array method
                                        if (GeneroSingletons.LanguageSettings.NativeClasses.TryGetValue("array", out tmpSysClass))
                                        {
                                            if (tmpSysClass.Functions.TryGetValue(splitTokens[i].ToLower(), out tmpSysClassFunction))
                                            {
                                                applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                );
                                                if (i + 1 == splitTokens.Length)
                                                {
                                                    continueMatching = false;
                                                    finalMatchType = "tmpSysClassFunction";
                                                }
                                            }
                                        }
                                    }
                                    if (continueMatching && tempDef.IsRecordType)  // possibly could go many levels deep...in theory the logic here should work.
                                    {
                                        // look for splitTokens[i] within the record's element variables
                                        VariableDefinition tempRecDef;
                                        if (tempDef.RecordElements.TryGetValue(splitTokens[i], out tempRecDef))
                                        {
                                            parentDef = tempDef;
                                            tempDef = tempRecDef;
                                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                );
                                            if (i + 1 == splitTokens.Length)
                                            {
                                                continueMatching = false;
                                                finalMatchType = "tempDef";
                                            }
                                        }
                                        else
                                        {
                                            // could be hovering over an indexer of an array
                                            if (funcDef != null)
                                            {
                                                TryGetFunctionElement(splitTokens[i], funcDef, ref tempDef, ref constantDef, ref typeDef, ref context);
                                            }
                                            if (tempDef == null)
                                            {
                                                TryGetModuleOrGlobalElement(splitTokens[i], fpm, programContents, ref tempDef, ref constantDef, ref typeDef, ref context);
                                            }
                                            if (tempDef != null)
                                            {
                                                applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                );
                                                if (i + 1 == splitTokens.Length)
                                                {
                                                    continueMatching = false;
                                                    finalMatchType = "tempDef";
                                                }
                                            }
                                        }
                                    }
                                    if (continueMatching && IntellisenseExtensions.IsClassInstance(tempDef.Type, out generoClass))
                                    {
                                        // find the function
                                        if (generoClass.Methods.TryGetValue(splitTokens[i].ToLower(), out tmpMethod))
                                        {
                                            applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                );
                                            if (i + 1 == splitTokens.Length)
                                            {
                                                continueMatching = false;
                                                finalMatchType = "tmpMethod";
                                            }
                                        }
                                    }
                                    if (continueMatching && tempDef != null)
                                    {
                                        GeneroClass tempClass;
                                        // check to see if the tempDef is a type that has record elements
                                        if ((funcDef != null && funcDef.Types.TryGetValue(tempDef.Type, out typeDef)) ||
                                           fpm.ModuleContents.GlobalTypes.TryGetValue(tempDef.Type, out typeDef) ||
                                           (programContents != null && programContents.GlobalTypes.TryGetValue(tempDef.Type, out typeDef)))
                                        {
                                            VariableDefinition tempRecDef;
                                            if (typeDef.RecordElements.TryGetValue(splitTokens[i], out tempRecDef))
                                            {
                                                parentDef = tempDef;
                                                tempDef = tempRecDef;
                                                applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                    (
                                                        textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                    );
                                                if (i + 1 == splitTokens.Length)
                                                {
                                                    continueMatching = false;
                                                    finalMatchType = "tempDef";
                                                }
                                            }
                                        }
                                        if (continueMatching && GeneroSingletons.LanguageSettings.NativeClasses.TryGetValue(tempDef.Type.ToLower(), out tmpSysClass))
                                        {
                                            if (tmpSysClass.Functions.TryGetValue(splitTokens[i].ToLower(), out tmpSysClassFunction))
                                            {
                                                applicableToSpan = currentSnapshot.CreateTrackingSpan
                                                (
                                                    textSpan.Start.Position, splitTokens[i].Length, SpanTrackingMode.EdgeInclusive
                                                );
                                                if (i + 1 == splitTokens.Length)
                                                {
                                                    continueMatching = false;
                                                    finalMatchType = "tmpSysClassFunction";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Now let's see what we have
                if (tmpSysClassFunction != null && finalMatchType == "tmpSysClassFunction")
                {
                    qiContent.Add(tmpSysClassFunction.GetIntellisenseText());   // TODO: this is still using the GeneroClassMethod extension method...need to change.
                }
                else if (generoOperator != null && finalMatchType == "generoOperator")
                {
                    qiContent.Add(generoOperator.GetIntellisenseText());
                }
                else if (tmpMethod != null && finalMatchType == "tmpMethod")
                {
                    qiContent.Add(tmpMethod.GetIntellisenseText());
                }
                else if (tmpSysClass != null && finalMatchType == "tmpSysClass")
                {
                    qiContent.Add(tmpSysClass.GetIntellisenseText());
                }
                else if (tmpClass != null && finalMatchType == "tmpClass")
                {
                    qiContent.Add(tmpClass.GetIntellisenseText());
                }
                else if (tmpPackage != null && finalMatchType == "tmpPackage")
                {
                    qiContent.Add(tmpPackage.GetIntellisenseText());
                }
                else if (columnOrRecordField != null && finalMatchType == "columnOrRecordField")
                {
                    qiContent.Add(columnOrRecordField.Type + " " + (tempDef.MimicTypeTable ?? "") + "." + columnOrRecordField.Name);
                }
                else if (tempDef != null && finalMatchType == "tempDef")
                {
                    qiContent.Add(tempDef.GetIntellisenseText(context, parentDef));
                }
                else if (constantDef != null && finalMatchType == "constantDef")
                {
                    qiContent.Add(constantDef.GetIntellisenseText(context));
                }
                else if (typeDef != null && finalMatchType == "typeDef")
                {
                    qiContent.Add(typeDef.GetIntellisenseText(context));
                }
                else if (cursorPrep != null && finalMatchType == "cursorPrep")
                {
                    qiContent.Add(cursorPrep.GetIntellisenseText());
                }
                else if (tempTableDef != null && finalMatchType == "tempTableDef")
                {
                    qiContent.Add("(temp table) " + tempTableDef.Name);
                }
                else if (publicFunctionQuickInfo != null && finalMatchType == "publicFunctionQuickInfo")
                {
                    qiContent.Add(publicFunctionQuickInfo);
                }
                else if (funcDef != null && finalMatchType == "funcDef")
                {
                    qiContent.Add(funcDef.GetIntellisenseText());
                }
            }
        }

        private bool TryGetFunctionElement(string token, FunctionDefinition funcDef, ref VariableDefinition varDef,
                                           ref ConstantDefinition constantDef, ref TypeDefinition typeDef,
                                            ref string context)
        {
            if (funcDef.Variables.TryGetValue(token, out varDef) ||
                funcDef.Constants.TryGetValue(token, out constantDef) ||
                funcDef.Types.TryGetValue(token, out typeDef))
            {
                if (varDef != null)
                {
                    // see if the variable is a parameter into the function
                    string varName = varDef.Name;
                    if (funcDef.Parameters.Any(x => x.Equals(varName, StringComparison.OrdinalIgnoreCase)))
                    {
                        context = "parameter";
                        return true;
                    }
                }
                context = "local";
                return true;
            }
            return false;
        }

        private bool TryGetModuleOrGlobalElement(string token, GeneroFileParserManager fpm,
                                                GeneroModuleContents programContents,
                                                ref VariableDefinition varDef,
                                                ref ConstantDefinition constantDef,
                                                ref TypeDefinition typeDef,
                                                ref string context)
        {
            if (!fpm.ModuleContents.ModuleVariables.TryGetValue(token, out varDef) &&
                                        !fpm.ModuleContents.ModuleConstants.TryGetValue(token, out constantDef) &&
                                        !fpm.ModuleContents.ModuleTypes.TryGetValue(token, out typeDef))
            {
                if ((fpm.ModuleContents.GlobalVariables.TryGetValue(token, out varDef) ||
                    (programContents != null && programContents.GlobalVariables.TryGetValue(token, out varDef))) ||
                    (fpm.ModuleContents.GlobalConstants.TryGetValue(token, out constantDef) ||
                    (programContents != null && programContents.GlobalConstants.TryGetValue(token, out constantDef))) ||
                    (fpm.ModuleContents.GlobalTypes.TryGetValue(token, out typeDef) ||
                    (programContents != null && programContents.GlobalTypes.TryGetValue(token, out typeDef))))
                {
                    context = "global";
                    return true;
                }
                else if(GeneroSingletons.SystemVariables.TryGetValue(token, out varDef))
                {
                    context = "system";
                    return true;
                }
            }
            else
            {
                context = "module";
                return true;
            }
            return false;
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
