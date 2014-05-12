/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using System.Data.SqlClient;
using System.Data;

namespace VSGenero.EditorExtensions.Intellisense
{
    public static class IntellisenseExtensions
    {
        internal static string GetIntellisenseText(this FunctionDefinition funcDef)
        {
            StringBuilder sb = new StringBuilder();
            if (funcDef.Private)
                sb.Append("private ");
            if (funcDef.Report)
                sb.Append("report ");
            else
                sb.Append("function ");
            sb.Append(funcDef.Name + "(");
            for (int i = 0; i < funcDef.Parameters.Count; i++)
            {
                // look for the parameter in the function (local) variables
                VariableDefinition varDef;
                if (funcDef.Variables.TryGetValue(funcDef.Parameters[i], out varDef))
                {
                    // put the type of the parameter
                    sb.Append(varDef.GetIntellisenseText());
                }
                else
                {
                    sb.Append(funcDef.Parameters[i]);
                }
                if (i + 1 < funcDef.Parameters.Count)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        internal static string GetIntellisenseText(this GeneroClassMethod classMethod)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(class method) ");
            sb.Append(classMethod.Name + "(");
            List<GeneroClassMethodParameter> sortedParams = classMethod.Parameters.Values.OrderBy(x => x.Position).ToList();
            for (int i = 0; i < sortedParams.Count; i++)
            {
                sb.Append(sortedParams[i].Type + " " + sortedParams[i].Name);
                if (i + 1 < sortedParams.Count)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        private static string GetVariableIntellisenseText(VariableDefinition varDef)
        {
            StringBuilder sb = new StringBuilder();
            if (varDef.ArrayType == ArrayType.Static)
            {
                sb.Append(string.Format("array[{0}] of ", varDef.StaticArraySize));
            }
            if (varDef.ArrayType == ArrayType.Dynamic)
            {
                sb.Append("dynamic array of ");
            }
            if (varDef.IsRecordType)
            {
                sb.Append("record ");
            }
            if (varDef.IsMimicType)
            {
                sb.Append("like ");
            }
            if (!string.IsNullOrWhiteSpace(varDef.Type))
            {
                sb.Append(varDef.Type);
                sb.Append(" ");
            }
            sb.Append(varDef.Name);
            return sb.ToString();
        }

        internal static string GetIntellisenseText(this VariableDefinition varDef, string context = null)
        {
            StringBuilder sb = new StringBuilder();
            if (context != null)
            {
                sb.Append(string.Format("({0} variable) ", context));
            }
            sb.Append(GetVariableIntellisenseText(varDef));
            return sb.ToString();
        }

        internal static string GetIntellisenseText(this ConstantDefinition constantDef, string context = null)
        {
            StringBuilder sb = new StringBuilder();
            if (context != null)
            {
                sb.Append(string.Format("({0} constant) ", context));
            }
            sb.Append(constantDef.Name + " = " + constantDef.Value);
            return sb.ToString();
        }

        internal static string GetIntellisenseText(this TypeDefinition typeDef, string context = null)
        {
            StringBuilder sb = new StringBuilder();
            if (context != null)
            {
                sb.Append(string.Format("({0} type) ", context));
            }
            sb.Append(GetVariableIntellisenseText(typeDef));
            return sb.ToString();
        }

        internal static string GetIntellisenseText(this CursorPreparation cursorPrep)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("cursor:");
            sb.AppendLine();
            foreach (var splitLine in IntelligentSplit(cursorPrep.CursorStatement, 40))
            {
                sb.AppendLine(splitLine);
            }
            return sb.ToString();
        }

        internal static IEnumerable<string> IntelligentSplit(string stringToSplit, int maxLineLength)
        {
            int charCount = 0;
            return stringToSplit.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .GroupBy(w => (charCount += w.Length + 1) / maxLineLength)
                                .Select(g => string.Join(" ", g));
        }

        internal static bool IsWithinComment(this IIntellisenseSession session)
        {
            return IsWithinTokenType(session, GeneroTokenType.Comment);
        }

        internal static bool IsWithinComment(this SnapshotPoint triggerPoint)
        {
            return IsWithinTokenType(triggerPoint, GeneroTokenType.Comment);
        }

        internal static bool IsWithinString(this IIntellisenseSession session)
        {
            return IsWithinTokenType(session, GeneroTokenType.String);
        }

        internal static bool IsWithinString(this SnapshotPoint triggerPoint)
        {
            return IsWithinTokenType(triggerPoint, GeneroTokenType.String);
        }

        internal static bool IsAttemptingMemberAccess(this SnapshotPoint triggerPoint, out string memberName)
        {
            memberName = null;
            int currPosition = triggerPoint.Position;

            string lineText = EditorExtensions.GetLineString(triggerPoint, currPosition);
            if (lineText.Length == 0)
                return false;
            if (char.IsWhiteSpace(lineText[lineText.Length - 1]))
                return false;

            var revParser = EditorExtensions.GetReverseParser(triggerPoint);

            bool isMemberAccess = false;
            int i = 0;
            GeneroTokenType lastTokenType = GeneroTokenType.Eof;
            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)
                    break;
                if (tagSpan.Span.End.Position <= currPosition)
                {
                    if (!isMemberAccess && tagSpan.Span.GetText() == ".")
                    {
                        lastTokenType = GeneroTokenType.Symbol;
                        isMemberAccess = true;
                        memberName = "";
                    }
                    else
                    {
                        if (isMemberAccess)
                        {
                            // get the token type
                            GeneroTokenType tokType = revParser.GetTokenType(tagSpan.Tag.ClassificationType);
                            if ((tokType == GeneroTokenType.Identifier ||
                                    tokType == GeneroTokenType.Keyword) &&
                                lastTokenType == GeneroTokenType.Symbol)
                            {
                                memberName = tagSpan.Span.GetText() + memberName;
                                lastTokenType = GeneroTokenType.Keyword;
                                continue;
                            }
                            else if ((tokType == GeneroTokenType.Symbol && tagSpan.Span.GetText() == ".") &&
                                    lastTokenType == GeneroTokenType.Keyword)
                            {
                                memberName = tagSpan.Span.GetText() + memberName;
                                lastTokenType = GeneroTokenType.Symbol;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            i++;
                        }
                        if (i > 1)
                        {
                            isMemberAccess = false;
                            break;
                        }
                    }

                }
            }
            return isMemberAccess;
        }

        internal static bool IsAttemptingMemberAccess(this IIntellisenseSession session, out string memberName)
        {
            memberName = null;
            if (IsEmptySession(session)) return false;
            return session.TextView.Caret.Position.BufferPosition.IsAttemptingMemberAccess(out memberName);
        }

        internal static bool IsWithinFunctionDefinition(this IIntellisenseSession session)
        {
            if (IsEmptySession(session)) return false;
            SnapshotPoint triggerPoint = session.TextView.Caret.Position.BufferPosition;
            int currPosition = triggerPoint.Position;

            string lineText = EditorExtensions.GetLineString(triggerPoint, currPosition);
            if (lineText.Length == 0)
                return false;
            if (char.IsWhiteSpace(lineText[lineText.Length - 1]))
                return false;

            var revParser = EditorExtensions.GetReverseParser(triggerPoint);

            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)
                    break;
                if (tagSpan.Span.End.Position <= currPosition)
                {
                    if (tagSpan.Span.GetText().ToLower() == "function")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool IsPotentialFunctionCall(this IIntellisenseSession session, out bool isDefiniteFunctionCall, out bool isDefiniteReportCall)
        {
            isDefiniteFunctionCall = false;
            isDefiniteReportCall = false;
            if (IsEmptySession(session)) return false;
            int currPosition = session.TextView.Caret.Position.BufferPosition.Position;
            var revParser = GetReverseParser(session);

            // There are a couple ways a function call can be made/used
            // 1) call [function call]
            // 2) let [variable] = [function call]
            //                   = ..... [keyword | symbol] [function call]
            // Maybe more..?
            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)
                    break;
                string lowerText = tagSpan.Span.GetText().ToLower();
                if (tagSpan.Span.End.Position < currPosition)
                {
                    isDefiniteFunctionCall = lowerText == "call";
                    isDefiniteReportCall = lowerText == "report";
                    if (isDefiniteFunctionCall || isDefiniteReportCall ||
                        revParser.GetTokenType(tagSpan.Tag.ClassificationType) == GeneroTokenType.Symbol)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //internal static bool IsRighthandOfAssignmentStatement(this IIntellisenseSession session)
        //{
        //    if (IsEmptySession(session)) return true;   // because in genero you can do a [x] := [y] assignment
        //    int currPosition = session.TextView.Caret.Position.BufferPosition.Position;
        //    var revParser = GetReverseParser(session);

        //    foreach (var tagSpan in revParser)
        //    {
        //        if (tagSpan == null)
        //            break;
        //        if (tagSpan.Span.End.Position < currPosition)
        //        {
        //            return tagSpan.Span.GetText().ToLower() == "let";
        //        }
        //    }

        //    return false;
        //}

        internal static bool IsLefthandOfAssignmentStatement(this IIntellisenseSession session)
        {
            if (IsEmptySession(session)) return true;   // because in genero you can do a [x] := [y] assignment
            int currPosition = session.TextView.Caret.Position.BufferPosition.Position;
            var revParser = GetReverseParser(session);

            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)
                    break;
                if (tagSpan.Span.End.Position < currPosition)
                {
                    return tagSpan.Span.GetText().ToLower() == "let";
                }
            }

            return false;
        }

        internal static string GetQuickInfoMemberOrMemberAccess(this SnapshotPoint triggerPoint, out SnapshotSpan applicableSpan, string testHoveringOver = null)
        {
            applicableSpan = new SnapshotSpan();
            string memberName = testHoveringOver;
            if (memberName == null)
                memberName = "";
            int currPosition = triggerPoint.Position;

            string lineText = EditorExtensions.GetLineString(triggerPoint, currPosition);
            if (lineText.Length == 0)
                return memberName;
            if (char.IsWhiteSpace(lineText[lineText.Length - 1]))
                return memberName;

            var revParser = EditorExtensions.GetReverseParser(triggerPoint);

            bool isMemberAccess = false;
            int i = 0;
            GeneroTokenType lastTokenType = GeneroTokenType.Eof;
            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)
                    break;
                string tokenText = tagSpan.Span.GetText();
                GeneroTokenType tokType = revParser.GetTokenType(tagSpan.Tag.ClassificationType);
                if (tagSpan.Span.End.Position <= currPosition)
                {
                    if (!isMemberAccess &&
                        (tokType == GeneroTokenType.Symbol && tokenText == "."))
                    {
                        lastTokenType = GeneroTokenType.Symbol;
                        isMemberAccess = true;
                        memberName = tokenText + memberName;
                        applicableSpan = tagSpan.Span;
                    }
                    else
                    {
                        if (isMemberAccess)
                        {
                            // get the token type
                            if ((tokType == GeneroTokenType.Identifier ||
                                    tokType == GeneroTokenType.Keyword) &&
                                lastTokenType == GeneroTokenType.Symbol)
                            {
                                memberName = tokenText + memberName;
                                lastTokenType = GeneroTokenType.Keyword;
                                continue;
                            }
                            else if ((tokType == GeneroTokenType.Symbol && tokenText == ".") &&
                                    lastTokenType == GeneroTokenType.Keyword)
                            {
                                memberName = tokenText + memberName;
                                lastTokenType = GeneroTokenType.Symbol;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            i++;
                        }
                        if (i > 1)
                        {
                            isMemberAccess = false;
                            break;
                        }
                    }

                }
            }

            return memberName;
        }

        internal static string GetCurrentMemberOrMemberAccess(this SnapshotPoint triggerPoint, out SnapshotSpan applicableSpan)
        {
            applicableSpan = new SnapshotSpan();
            string memberName = null;
            int currPosition = triggerPoint.Position;

            string lineText = EditorExtensions.GetLineString(triggerPoint, currPosition);
            if (lineText.Length == 0)
                return memberName;
            if (char.IsWhiteSpace(lineText[lineText.Length - 1]))
                return memberName;

            var revParser = EditorExtensions.GetReverseParser(triggerPoint);

            bool isMemberAccess = false;
            int i = 0;
            GeneroTokenType lastTokenType = GeneroTokenType.Eof;
            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)
                    break;
                GeneroTokenType tokType = revParser.GetTokenType(tagSpan.Tag.ClassificationType);
                if (tagSpan.Span.End.Position <= currPosition)
                {
                    if (!isMemberAccess &&
                        (tokType == GeneroTokenType.Identifier || tokType == GeneroTokenType.Keyword))
                    {
                        lastTokenType = GeneroTokenType.Keyword;
                        isMemberAccess = true;
                        memberName = tagSpan.Span.GetText();
                        applicableSpan = tagSpan.Span;
                    }
                    else
                    {
                        if (isMemberAccess)
                        {
                            // get the token type
                            if ((tokType == GeneroTokenType.Identifier ||
                                    tokType == GeneroTokenType.Keyword) &&
                                lastTokenType == GeneroTokenType.Symbol)
                            {
                                memberName = tagSpan.Span.GetText() + memberName;
                                lastTokenType = GeneroTokenType.Keyword;
                                continue;
                            }
                            else if ((tokType == GeneroTokenType.Symbol && tagSpan.Span.GetText() == ".") &&
                                    lastTokenType == GeneroTokenType.Keyword)
                            {
                                memberName = tagSpan.Span.GetText() + memberName;
                                lastTokenType = GeneroTokenType.Symbol;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            i++;
                        }
                        if (i > 1)
                        {
                            isMemberAccess = false;
                            break;
                        }
                    }

                }
            }

            return isMemberAccess ? memberName : null;
        }

        internal static string GetCurrentMemberOrMemberAccess(this IIntellisenseSession session, out SnapshotSpan applicableSpan)
        {
            applicableSpan = new SnapshotSpan();
            string memberName = null;
            if (IsEmptySession(session)) return memberName;
            return session.TextView.Caret.Position.BufferPosition.GetCurrentMemberOrMemberAccess(out applicableSpan);
        }

        // TODO: needs an overload for SnapshotPoint
        private static bool IsEmptySession(IIntellisenseSession session)
        {
            return session.TextView.Caret.Position.BufferPosition.Position == 0;
        }

        private static string GetLineString(IIntellisenseSession session, int startPosition)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            return EditorExtensions.GetLineString(currentPoint, startPosition);
        }

        

        private static GeneroReverseParser GetReverseParser(IIntellisenseSession session, int startPosition = -1)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            return EditorExtensions.GetReverseParser(currentPoint, startPosition);
        }

        private static bool IsWithinTokenType(IIntellisenseSession session, GeneroTokenType tokenType)
        {
            if (IsEmptySession(session)) return false;

            return IsWithinTokenType(session.TextView.Caret.Position.BufferPosition, tokenType);
        }

        private static bool IsWithinTokenType(SnapshotPoint triggerPoint, GeneroTokenType tokenType)
        {
            int currPosition = triggerPoint.Position;
            var revParser = EditorExtensions.GetReverseParser(triggerPoint);
            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)
                    break;
                if (tagSpan.Span.Start.Position < currPosition &&
                    tagSpan.Span.End.Position >= currPosition &&
                    revParser.GetTokenType(tagSpan.Tag.ClassificationType) == tokenType)
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] defineStatementKeywords = { "like", "record", "dynamic", "array", "of", "to" };

        public static bool IsWithinDefineStatement(this IIntellisenseSession session)
        {
            if (IsEmptySession(session)) return false;

            return IsWithinDefineStatement(session.TextView.Caret.Position.BufferPosition);
        }

        public static Tuple<string, GeneroTokenType> GetPreviousToken(this IIntellisenseSession session)
        {
            if (IsEmptySession(session)) return null;

            return GetPreviousToken(session.TextView.Caret.Position.BufferPosition);
        }

        private static Tuple<string, GeneroTokenType> GetPreviousToken(SnapshotPoint triggerPoint)
        {
            int currPosition = triggerPoint.Position;
            var revParser = EditorExtensions.GetReverseParser(triggerPoint, -1, true);

            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)    // shouldn't ever happen
                    break;
                if (tagSpan.Span.End.Position < currPosition)   // we're in tokens prior to where we are
                {
                    return new Tuple<string, GeneroTokenType>(tagSpan.Span.GetText().ToLower(),
                                                             revParser.GetTokenType(tagSpan.Tag.ClassificationType));
                }
            }
            return null;
        }

        private static bool IsWithinDefineStatement(SnapshotPoint triggerPoint)
        {
            int currPosition = triggerPoint.Position;
            var revParser = EditorExtensions.GetReverseParser(triggerPoint, -1, true);

            foreach (var tagSpan in revParser)
            {
                if (tagSpan == null)    // shouldn't ever happen
                    break;
                if (tagSpan.Span.End.Position < currPosition)   // we're in tokens prior to where we are
                {
                    GeneroTokenType tokenType = revParser.GetTokenType(tagSpan.Tag.ClassificationType);
                    string lowercaseText = tagSpan.Span.GetText().ToLower();
                    if (tokenType == GeneroTokenType.Keyword)
                    {
                        // check to see if it's a type or it's one of the keywords used in define statements
                        if (GeneroSingletons.LanguageSettings.DataTypeMap.ContainsKey(lowercaseText) ||
                           defineStatementKeywords.Contains(lowercaseText))
                        {
                            // continue reverse parsing if this is the case
                            continue;
                        }

                        return (lowercaseText == "define");
                    }
                }
            }

            return false;
        }

        public static FunctionDefinition DetermineContainingFunction(SnapshotPoint currentPoint, GeneroFileParserManager fpm)
        {
            return DetermineContainingFunction(currentPoint.Position, fpm);
        }

        public static FunctionDefinition DetermineContainingFunction(int position, GeneroFileParserManager fpm)
        {
            FunctionDefinition ret = null;  // assume no function yet
            if (fpm.ModuleContents != null)
            {
                var kvp = fpm.ModuleContents.FunctionDefinitions.Where(x => x.Value.ContainingFile == fpm.ModuleContents.ContentFilename)
                                                                .FirstOrDefault(x =>
                {
                    return position > x.Value.Position && position < x.Value.End;
                });

                if (kvp.Value != null)
                    ret = kvp.Value;
            }
            return ret;
        }

        public static bool IsClassInstance(string type, out GeneroClass classType)
        {
            bool isClass = false;
            classType = null;
            if (string.IsNullOrWhiteSpace(type))
                return isClass;
            string[] tokens = type.Split(new[] { '.' });
            // a class consists of a package name and a class name
            if (tokens.Length == 2)
            {
                GeneroPackage package;
                if (GeneroSingletons.LanguageSettings.Packages.TryGetValue(tokens[0], out package))
                {
                    if (package.Classes.TryGetValue(tokens[1], out classType))
                    {
                        isClass = true;
                    }
                }
            }
            return isClass;
        }
    }

    public class GeneroTableColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
