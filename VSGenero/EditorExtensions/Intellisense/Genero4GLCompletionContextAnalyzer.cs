using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace VSGenero.EditorExtensions.Intellisense
{
    public class Genero4GLCompletionContextAnalyzer
    {
        private readonly IGlyphService _glyphService;
        private readonly IDatabaseInformationProvider _dbInfoProvider;
        private readonly IPublicFunctionProvider _publicFunctionProvider;

        public Genero4GLCompletionContextAnalyzer(IGlyphService glyphService, IDatabaseInformationProvider dbInfoProvider, IPublicFunctionProvider publicFunctionProvider)
        {
            _glyphService = glyphService;
            _dbInfoProvider = dbInfoProvider;
            _publicFunctionProvider = publicFunctionProvider;
        }

        public List<MemberCompletion> AnalyzeContext(ICompletionSession session, ITrackingSpan applicableSpan, GeneroFileParserManager fileParserManager, GeneroModuleContents moduleContents, out bool isMemberCompletion, out bool isDefiniteFunctionOrReportCall)
        {
            isMemberCompletion = false;
            isDefiniteFunctionOrReportCall = false;
            List<MemberCompletion> applicableCompletions = new List<MemberCompletion>();

            if (!IsWithinNonCompletionArea(session) &&
                !IsNonCompletionText(applicableSpan, session))
            {
                FunctionDefinition currentFunction = IntellisenseExtensions.DetermineContainingFunction((session.TextView.Caret.Position.BufferPosition) - 1, fileParserManager);
                string memberAccessName = null;
                if (session.IsAttemptingMemberAccess(out memberAccessName))
                {
                    isMemberCompletion = true;
                    return GetMemberAccessCompletions(memberAccessName, currentFunction, moduleContents);
                }

                bool isDefiniteFunctionCall = false;
                bool isDefiniteReportCall = false;
                if (session.IsPotentialFunctionCall(out isDefiniteFunctionCall, out isDefiniteReportCall))
                {
                    var funcRptCompletions = GetFunctionCallCompletions(isDefiniteFunctionCall, isDefiniteReportCall, moduleContents, currentFunction);
                    isDefiniteFunctionOrReportCall = isDefiniteFunctionCall || isDefiniteReportCall;
                    if (isDefiniteFunctionOrReportCall)
                        return funcRptCompletions;  // we only want to use the retrieved completions
                    else
                        applicableCompletions.AddRange(funcRptCompletions); // we want the other stuff too
                }

                if (session.IsWithinDefineStatement())
                {
                    // we want to have completions show up if we're typing the defined variable's type
                    var previousToken = session.GetPreviousToken();
                    if (previousToken != null)
                    {
                        if (previousToken.Item1 == "define" || previousToken.Item1 == ",")
                        {
                            return new List<MemberCompletion>();
                        }
                        else
                        {
                            // TODO: would like to be smarter about completion here...
                            // like only showing types if in the midst of defining a variable
                        }
                    }
                }

                applicableCompletions.AddRange(GetNormalCompletions(moduleContents, currentFunction));
            }

            return applicableCompletions;
        }

        private bool IsWithinNonCompletionArea(ICompletionSession session)
        {
            return session.IsWithinComment() ||
                   session.IsWithinString() ||
                   session.IsWithinFunctionDefinition();
        }

        private bool IsNonCompletionText(ITrackingSpan span, ICompletionSession session)
        {
            string text = span.GetText(session.TextView.TextSnapshot);
            // first check to see if the text is something that should not have a completion list
            // i.e. a number (anything else?)
            double dummyDouble = 0;
            if (double.TryParse(text, out dummyDouble))
            {
                return true;
            }
            return false;
        }

        private List<MemberCompletion> GetFunctionCallCompletions(bool isDefiniteFunctionCall, bool isDefiniteReportCall, GeneroModuleContents moduleContents, FunctionDefinition currentFunction)
        {
            List<MemberCompletion> functionsOnly = new List<MemberCompletion>();
            List<MemberCompletion> reportsOnly = new List<MemberCompletion>();
            List<MemberCompletion> others = new List<MemberCompletion>();

            foreach (var function in moduleContents.FunctionDefinitions)
            {
                var comp = new MemberCompletion(function.Key, function.Key, function.Value.GetIntellisenseText(),
                                _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod,
                                    (function.Value.Private ? StandardGlyphItem.GlyphItemPrivate : StandardGlyphItem.GlyphItemPublic)),
                                null);
                comp.Properties.AddProperty("system", false);
                if (isDefiniteFunctionCall && !function.Value.Report)
                {
                    functionsOnly.Add(comp);
                }
                else if (isDefiniteReportCall && function.Value.Report)
                {
                    reportsOnly.Add(comp);
                }
                else
                {
                    others.Add(comp);
                }
            }

            if (isDefiniteFunctionCall)
            {
                if (_publicFunctionProvider != null)
                {
                    foreach (var completion in _publicFunctionProvider.GetPublicFunctionCompletions())
                    {
                        completion.Properties.AddProperty("public", false);
                        completion.IconSource = _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                        functionsOnly.Add(completion);
                    }
                }

                // add Genero packages, since their classes have functions too
                foreach (var packageName in GeneroSingletons.LanguageSettings.Packages)
                {
                    var comp = new MemberCompletion(packageName.Key, packageName.Key, packageName.Value.Description,
                                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic), null);
                    functionsOnly.Add(comp);
                }

                GeneroClass dummy;
                // look for variables that are instances of classes with instance methods
                foreach (var sysVar in GeneroSingletons.SystemVariables.Where(x => IntellisenseExtensions.IsClassInstance(x.Value.Type, out dummy) && !dummy.IsStatic))
                {
                    var comp = new MemberCompletion(sysVar.Key, sysVar.Key, sysVar.Value.GetIntellisenseText("system"),
                                    _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic), null);
                    comp.Properties.AddProperty("system", false);
                    functionsOnly.Add(comp);
                }

                foreach (var globalVar in moduleContents.GlobalVariables.Where(x => IntellisenseExtensions.IsClassInstance(x.Value.Type, out dummy) && !dummy.IsStatic))
                {
                    var comp = new MemberCompletion(globalVar.Key, globalVar.Key, globalVar.Value.GetIntellisenseText("global"),
                                    _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic), null);
                    comp.Properties.AddProperty("system", false);
                    functionsOnly.Add(comp);
                }

                // add the module variables
                foreach (var moduleVar in moduleContents.ModuleVariables.Where(x => IntellisenseExtensions.IsClassInstance(x.Value.Type, out dummy) && !dummy.IsStatic))
                {
                    var comp = new MemberCompletion(moduleVar.Key, moduleVar.Key, moduleVar.Value.GetIntellisenseText("module"),
                                    _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemInternal), null);
                    comp.Properties.AddProperty("system", false);
                    functionsOnly.Add(comp);
                }

                if (currentFunction != null)
                {
                    foreach (var functionVar in currentFunction.Variables.Where(x => IntellisenseExtensions.IsClassInstance(x.Value.Type, out dummy) && !dummy.IsStatic))
                    {
                        var comp = new MemberCompletion(functionVar.Key, functionVar.Key, functionVar.Value.GetIntellisenseText("local"),
                                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPrivate), null);
                        comp.Properties.AddProperty("system", false);
                        functionsOnly.Add(comp);
                    }
                }
                return functionsOnly;
            }
            else if (isDefiniteReportCall)
            {
                return reportsOnly;
            }
            else
            {
                // this should never happen...but just in case
                return others;
            }
        }

        private List<MemberCompletion> GetMemberAccessCompletions(string memberAccessName, FunctionDefinition currentFunction, GeneroModuleContents moduleContents)
        {
            List<MemberCompletion> memberCompletionList = new List<MemberCompletion>();
            string[] memberCompletionTokens = memberAccessName.Split(new[] { '.' });
            if (memberCompletionTokens.Length == 1)
            {
                VariableDefinition varDef = null;
                // look for the member name in 1) locals, 2) module, 3) globals
                if (currentFunction != null)
                {
                    if (!currentFunction.Variables.TryGetValue(memberAccessName, out varDef))
                    {
                        // look in module variables
                        if (!moduleContents.ModuleVariables.TryGetValue(memberAccessName, out varDef))
                        {
                            if (!moduleContents.GlobalVariables.TryGetValue(memberAccessName, out varDef))
                            {
                                GeneroSingletons.SystemVariables.TryGetValue(memberAccessName, out varDef);
                            }
                        }
                    }
                }
                if (varDef != null)
                {
                    // see if we have any child members to access
                    if (varDef.RecordElements.Count > 0)
                    {
                        foreach (var ele in varDef.RecordElements)
                        {
                            var comp = new MemberCompletion(ele.Value.Name, ele.Value.Name, "(record variable) " + ele.Value.Type + " " + ele.Value.Name,
                                _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemInternal), null);
                            memberCompletionList.Add(comp);
                        }
                    }
                    else if (varDef.IsMimicType)
                    {
                        // attempt to get the table's columns
                        var comp = new MemberCompletion("*", "*", "All columns",
                                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemInternal), null);
                        memberCompletionList.Add(comp);
                        if (_dbInfoProvider != null)
                        {
                            var colNames = _dbInfoProvider.GetTableColumns(varDef.MimicTypeTable);
                            if (colNames != null && colNames.Count > 0)
                            {
                                foreach (var tableColumn in colNames)
                                {
                                    comp = new MemberCompletion(tableColumn.Name, tableColumn.Name, tableColumn.Type + " " + tableColumn.Name,
                                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemInternal), null);
                                    memberCompletionList.Add(comp);
                                }
                            }
                        }
                    }
                    else
                    {
                        GeneroClass potentialClass;
                        if (IntellisenseExtensions.IsClassInstance(varDef.Type, out potentialClass))
                        {
                            foreach (var classMethod in potentialClass.Methods)
                            {
                                if (classMethod.Value.Scope == GeneroClassMethod.GeneroClassScope.Instance)
                                {
                                    var comp = new MemberCompletion(classMethod.Key, classMethod.Key, classMethod.Value.Description,
                                            _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic), null);
                                    memberCompletionList.Add(comp);
                                }
                            }
                        }
                    }
                }
            }

            // handle member access with a depth greater than one
            GeneroPackage tmpPackage = null;
            GeneroClass tmpClass = null;
            GeneroClassMethod tmpMethod = null;
            for (int i = 0; i < memberCompletionTokens.Length; i++)
            {
                if (tmpPackage == null && i == 0)   // TODO: not sure if the index is needed
                {
                    if (GeneroSingletons.LanguageSettings.Packages.TryGetValue(memberCompletionTokens[i], out tmpPackage))
                    {
                        if (i + 1 == memberCompletionTokens.Length && tmpPackage.Classes.Count > 0)
                        {
                            foreach (var packageClass in tmpPackage.Classes)
                            {
                                var comp = new MemberCompletion(packageClass.Key, packageClass.Key, packageClass.Value.Description,
                                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic), null);
                                memberCompletionList.Add(comp);
                            }
                        }
                    }
                }
                else if (tmpPackage != null && tmpClass == null && i == 1)
                {
                    if (tmpPackage.Classes.TryGetValue(memberCompletionTokens[i], out tmpClass))
                    {
                        if (i + 1 == memberCompletionTokens.Length && tmpClass.Methods.Count > 0)
                        {
                            foreach (var classMethod in tmpClass.Methods)
                            {
                                if (classMethod.Value.Scope == GeneroClassMethod.GeneroClassScope.Static)
                                {
                                    var comp = new MemberCompletion(classMethod.Key, classMethod.Key, classMethod.Value.Description,
                                            _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic), null);
                                    memberCompletionList.Add(comp);
                                }
                            }
                        }
                    }
                }
                //else if (tmpPackage != null && tmpClass != null && tmpClass == null && i == 2)
                //{

                //}
                else
                {
                    break;
                }
            }

            return memberCompletionList;
        }

        private List<MemberCompletion> GetNormalCompletions(GeneroModuleContents moduleContents, FunctionDefinition currentFunction)
        {
            List<MemberCompletion> completions = new List<MemberCompletion>();

            // add system variables
            foreach (var sysVar in GeneroSingletons.SystemVariables)
            {
                var comp = new MemberCompletion(sysVar.Key, sysVar.Key, sysVar.Value.GetIntellisenseText("system"),
                                _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic), null);
                comp.Properties.AddProperty("system", false);
                completions.Add(comp);
            }

            // add global variables
            foreach (var globalVar in moduleContents.GlobalVariables)
            {
                var comp = new MemberCompletion(globalVar.Key, globalVar.Key, globalVar.Value.GetIntellisenseText("global"),
                                _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic), null);
                comp.Properties.AddProperty("system", false);
                completions.Add(comp);
            }

            // add the module variables
            foreach (var moduleVar in moduleContents.ModuleVariables)
            {
                var comp = new MemberCompletion(moduleVar.Key, moduleVar.Key, moduleVar.Value.GetIntellisenseText("module"),
                                _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemInternal), null);
                comp.Properties.AddProperty("system", false);
                completions.Add(comp);
            }

            //figure out what function we're in
            if (currentFunction != null)
            {
                foreach (var functionVar in currentFunction.Variables)
                {
                    var comp = new MemberCompletion(functionVar.Key, functionVar.Key, functionVar.Value.GetIntellisenseText("local"),
                                    _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPrivate), null);
                    comp.Properties.AddProperty("system", false);
                    completions.Add(comp);
                }
            }

            return completions;
        }
    }
}
