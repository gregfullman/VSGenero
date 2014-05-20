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
                if (isDefiniteReportCall && function.Value.Report)
                {
                    reportsOnly.Add(comp);
                }
                else
                //if (isDefiniteFunctionCall && !function.Value.Report)
                {
                    functionsOnly.Add(comp);
                }
            }

            //if (isDefiniteFunctionCall)
            //{
            if (isDefiniteReportCall)
            {
                return reportsOnly;
            }
            else
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

                foreach (var genOper in GeneroSingletons.LanguageSettings.NativeOperators)
                {
                    var comp = new MemberCompletion(genOper.Value.Name, genOper.Value.Name, genOper.Value.GetIntellisenseText(),
                                       _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupOperator, StandardGlyphItem.GlyphItemPublic), null);
                    functionsOnly.Add(comp);
                }

                foreach (var sysFunc in GeneroSingletons.LanguageSettings.NativeMethods)
                {
                    var comp = new MemberCompletion(sysFunc.Value.Name, sysFunc.Value.Name, sysFunc.Value.GetIntellisenseText(),
                                       _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic), null);
                    functionsOnly.Add(comp);
                }

                // add Genero packages, since their classes have functions too
                foreach (var packageName in GeneroSingletons.LanguageSettings.Packages)
                {
                    var comp = new MemberCompletion(packageName.Value.Name, packageName.Value.Name, packageName.Value.GetIntellisenseText(),
                                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic), null);
                    functionsOnly.Add(comp);
                }

                GeneroClass dummy;
                // look for variables that are instances of classes with instance methods
                foreach (var sysVar in GeneroSingletons.SystemVariables.Where(x => IntellisenseExtensions.IsClassInstance(x.Value.Type, out dummy) && !dummy.IsStatic))
                {
                    var comp = new MemberCompletion(sysVar.Value.Name, sysVar.Value.Name, sysVar.Value.GetIntellisenseText("system"),
                                    _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic), null);
                    comp.Properties.AddProperty("system", false);
                    functionsOnly.Add(comp);
                }

                foreach (var globalVar in moduleContents.GlobalVariables.Where(x => IntellisenseExtensions.IsClassInstance(x.Value.Type, out dummy) && !dummy.IsStatic))
                {
                    var comp = new MemberCompletion(globalVar.Value.Name, globalVar.Value.Name, globalVar.Value.GetIntellisenseText("global"),
                                    _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic), null);
                    comp.Properties.AddProperty("system", false);
                    functionsOnly.Add(comp);
                }

                // add the module variables
                foreach (var moduleVar in moduleContents.ModuleVariables.Where(x => IntellisenseExtensions.IsClassInstance(x.Value.Type, out dummy) && !dummy.IsStatic))
                {
                    var comp = new MemberCompletion(moduleVar.Value.Name, moduleVar.Value.Name, moduleVar.Value.GetIntellisenseText("module"),
                                    _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemInternal), null);
                    comp.Properties.AddProperty("system", false);
                    functionsOnly.Add(comp);
                }

                if (currentFunction != null)
                {
                    foreach (var functionVar in currentFunction.Variables.Where(x => IntellisenseExtensions.IsClassInstance(x.Value.Type, out dummy) && !dummy.IsStatic))
                    {
                        var comp = new MemberCompletion(functionVar.Value.Name, functionVar.Value.Name, functionVar.Value.GetIntellisenseText("local"),
                                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPrivate), null);
                        comp.Properties.AddProperty("system", false);
                        functionsOnly.Add(comp);
                    }
                }
                return functionsOnly;
            }
        }

        private List<MemberCompletion> GetMemberAccessCompletions(string memberAccessName, FunctionDefinition currentFunction, GeneroModuleContents moduleContents)
        {
            List<MemberCompletion> memberCompletionList = new List<MemberCompletion>();
            string[] memberCompletionTokens = memberAccessName.Split(new[] { '.' });

            // handle member access with a depth greater than one
            GeneroPackage tmpPackage = null;
            GeneroClass tmpClass = null;
            GeneroClassMethod tmpMethod = null;
            VariableDefinition varDef = null;
            TypeDefinition typeDef = null;
            for (int i = 0; i < memberCompletionTokens.Length; i++)
            {
                ArrayElement arrayElement = null;
                if ((arrayElement = IntellisenseExtensions.GetArrayElement(memberCompletionTokens[i])) != null)
                {
                    if (arrayElement.IsComplete)
                        memberCompletionTokens[i] = arrayElement.ArrayName;
                    else if (arrayElement.Indices.Count > 0 && !string.IsNullOrWhiteSpace(arrayElement.Indices[arrayElement.Indices.Count - 1]))
                        memberCompletionTokens[i] = arrayElement.Indices[arrayElement.Indices.Count - 1];
                }

                if (tmpPackage == null && i == 0)   // TODO: not sure if the index is needed
                {
                    if (GeneroSingletons.LanguageSettings.Packages.TryGetValue(memberCompletionTokens[i].ToLower(), out tmpPackage))
                    {
                        if (i + 1 == memberCompletionTokens.Length && tmpPackage.Classes.Count > 0)
                        {
                            foreach (var packageClass in tmpPackage.Classes)
                            {
                                var comp = new MemberCompletion(packageClass.Value.Name, packageClass.Value.Name, packageClass.Value.GetIntellisenseText(),
                                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic), null);
                                memberCompletionList.Add(comp);
                            }
                            break;
                        }
                    }
                }
                else if (tmpPackage != null && tmpClass == null && i == 1)
                {
                    if (tmpPackage.Classes.TryGetValue(memberCompletionTokens[i].ToLower(), out tmpClass))
                    {
                        if (i + 1 == memberCompletionTokens.Length && tmpClass.Methods.Count > 0)
                        {
                            foreach (var classMethod in tmpClass.Methods)
                            {
                                if (classMethod.Value.Scope == GeneroClassMethod.GeneroClassScope.Static)
                                {
                                    var comp = new MemberCompletion(classMethod.Value.Name, classMethod.Value.Name, classMethod.Value.GetIntellisenseText(),
                                            _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic), null);
                                    memberCompletionList.Add(comp);
                                }
                            }
                            break;
                        }
                    }
                }

                if (i == 0)
                {
                    // look for the member name in 1) locals, 2) module, 3) globals
                    if (currentFunction != null)
                    {
                        if (!currentFunction.Variables.TryGetValue(memberCompletionTokens[i].ToLower(), out varDef))
                        {
                            // look in module variables
                            if (!moduleContents.ModuleVariables.TryGetValue(memberCompletionTokens[i].ToLower(), out varDef))
                            {
                                if (!moduleContents.GlobalVariables.TryGetValue(memberCompletionTokens[i].ToLower(), out varDef))
                                {
                                    GeneroSingletons.SystemVariables.TryGetValue(memberCompletionTokens[i].ToLower(), out varDef);
                                }
                            }
                        }
                    }

                    if (varDef != null)
                    {
                        if (varDef.ArrayType == ArrayType.None || (arrayElement != null && arrayElement.IsComplete))
                        {
                            // check for a type definition
                            if (moduleContents.GlobalTypes.TryGetValue(varDef.Type.ToLower(), out typeDef) ||
                                        moduleContents.ModuleTypes.TryGetValue(varDef.Type.ToLower(), out typeDef) ||
                                        currentFunction.Types.TryGetValue(varDef.Type.ToLower(), out typeDef))
                            {
                                varDef = typeDef;
                            }
                        }
                    }
                }
                else
                {
                    // look for the current token within the current varDef's record children
                    if (varDef != null)
                    {
                        if (varDef.RecordElements.Count > 0)
                        {
                            VariableDefinition tempRecDef;
                            if (varDef.RecordElements.TryGetValue(memberCompletionTokens[i].ToLower(), out tempRecDef))
                            {
                                varDef = tempRecDef;
                            }
                        }
                    }
                }

                if (varDef != null)
                {
                    // see if we have any child members to access
                    if (i + 1 == memberCompletionTokens.Length &&
                        varDef.RecordElements.Count > 0 &&
                        (varDef.ArrayType == ArrayType.None || (arrayElement != null && arrayElement.IsComplete)))
                    {
                        foreach (var ele in varDef.RecordElements)
                        {
                            var comp = new MemberCompletion(ele.Value.Name, ele.Value.Name, ele.Value.GetIntellisenseText("record"),
                                _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemInternal), null);
                            memberCompletionList.Add(comp);
                        }
                        break;
                    }
                    else if (i + 1 == memberCompletionTokens.Length && varDef.ArrayType != ArrayType.None && (arrayElement == null || !arrayElement.IsComplete))
                    {
                        // we want to display the array functions
                        GeneroSystemClass arrayClass;
                        if (GeneroSingletons.LanguageSettings.NativeClasses.TryGetValue("array", out arrayClass))
                        {
                            foreach (var classFunction in arrayClass.Functions)
                            {
                                if (classFunction.Value.Scope == GeneroClassMethod.GeneroClassScope.Instance)
                                {
                                    var comp = new MemberCompletion(classFunction.Value.Name, classFunction.Value.Name, classFunction.Value.GetIntellisenseText(),
                                            _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic), null);
                                    memberCompletionList.Add(comp);
                                }
                            }
                            break;
                        }
                    }
                    else if (i + 1 == memberCompletionTokens.Length && varDef.IsMimicType)
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
                                break;
                            }
                        }
                    }
                    else if (i + 1 == memberCompletionTokens.Length)
                    {
                        GeneroClass potentialClass;
                        GeneroSystemClass sysClass;
                        if (IntellisenseExtensions.IsClassInstance(varDef.Type, out potentialClass))
                        {
                            foreach (var classMethod in potentialClass.Methods)
                            {
                                if (classMethod.Value.Scope == GeneroClassMethod.GeneroClassScope.Instance)
                                {
                                    var comp = new MemberCompletion(classMethod.Value.Name, classMethod.Value.Name, classMethod.Value.GetIntellisenseText(),
                                            _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic), null);
                                    memberCompletionList.Add(comp);
                                }
                            }
                            break;
                        }
                        else if (GeneroSingletons.LanguageSettings.NativeClasses.TryGetValue(varDef.Type.ToLower(), out sysClass))
                        {
                            foreach (var classFunction in sysClass.Functions)
                            {
                                if (classFunction.Value.Scope == GeneroClassMethod.GeneroClassScope.Instance)
                                {
                                    var comp = new MemberCompletion(classFunction.Value.Name, classFunction.Value.Name, classFunction.Value.GetIntellisenseText(),
                                            _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic), null);
                                    memberCompletionList.Add(comp);
                                }
                            }
                            break;
                        }
                    }
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
                    var comp = new MemberCompletion(functionVar.Value.Name, functionVar.Value.Name, functionVar.Value.GetIntellisenseText("local"),
                                    _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPrivate), null);
                    comp.Properties.AddProperty("system", false);
                    completions.Add(comp);
                }
            }

            return completions;
        }
    }
}
