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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    public partial class Genero4glAst
    {
        private enum FunctionDefStatus
        {
            None,
            FuncKeyword,
            Comma,
            LParen,
            IdentOrKeyword
        }

        private bool TryFunctionDefContext(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results)
        {
            results = new List<MemberResult>();
            FunctionDefStatus status = FunctionDefStatus.None;
            bool quit = false;
            foreach (var tokInfo in revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index))
            {
                if (tokInfo.Equals(default(TokenInfo)))
                    continue;   // linebreak
                switch (status)
                {
                    case FunctionDefStatus.None:
                        {
                            if (tokInfo.Category == TokenCategory.Identifier ||
                               (tokInfo.Category == TokenCategory.Keyword && tokInfo.Token.Kind != TokenKind.FunctionKeyword && tokInfo.Token.Kind != TokenKind.ReportKeyword))
                            {
                                status = FunctionDefStatus.IdentOrKeyword;
                            }
                            else if (tokInfo.Token.Kind == TokenKind.FunctionKeyword || tokInfo.Token.Kind == TokenKind.ReportKeyword)
                            {
                                status = FunctionDefStatus.FuncKeyword;
                                quit = true;
                            }
                            else if (tokInfo.Token.Kind == TokenKind.Comma)
                            {
                                status = FunctionDefStatus.Comma;
                            }
                            else if (tokInfo.Token.Kind == TokenKind.LeftParenthesis)
                            {
                                status = FunctionDefStatus.LParen;
                            }
                            else
                            {
                                quit = true;
                            }
                            break;
                        }
                    case FunctionDefStatus.Comma:
                        {
                            if (tokInfo.Category == TokenCategory.Identifier ||
                               (tokInfo.Category == TokenCategory.Keyword && tokInfo.Token.Kind != TokenKind.FunctionKeyword && tokInfo.Token.Kind != TokenKind.ReportKeyword))
                            {
                                status = FunctionDefStatus.IdentOrKeyword;
                            }
                            else
                            {
                                quit = true;
                            }
                            break;
                        }
                    case FunctionDefStatus.IdentOrKeyword:
                        {
                            if (tokInfo.Token.Kind == TokenKind.FunctionKeyword || tokInfo.Token.Kind == TokenKind.ReportKeyword)
                            {
                                status = FunctionDefStatus.FuncKeyword;
                                quit = true;
                            }
                            else if (tokInfo.Token.Kind == TokenKind.Comma)
                            {
                                status = FunctionDefStatus.Comma;
                            }
                            else if (tokInfo.Token.Kind == TokenKind.LeftParenthesis)
                            {
                                status = FunctionDefStatus.LParen;
                            }
                            else
                            {
                                quit = true;
                            }
                            break;
                        }
                    case FunctionDefStatus.LParen:
                        {
                            if (tokInfo.Category == TokenCategory.Identifier ||
                                (tokInfo.Category == TokenCategory.Keyword && tokInfo.Token.Kind != TokenKind.FunctionKeyword && tokInfo.Token.Kind != TokenKind.ReportKeyword))
                            {
                                status = FunctionDefStatus.IdentOrKeyword;
                            }
                            else
                            {
                                quit = true;
                            }
                            break;
                        }
                    default:
                        quit = true;
                        break;
                }

                if (quit)
                {
                    break;
                }
            }

            return status == FunctionDefStatus.FuncKeyword;
        }

        #region Member Provider Helpers

        internal IFunctionInformationProvider _functionProvider;
        internal IDatabaseInformationProvider _databaseProvider;
        internal IProgramFileProvider _programFileProvider;

        private IEnumerable<MemberResult> GetAdditionalUserDefinedTypes(int index, string token)
        {
            return GetDefinedMembers(index, AstMemberType.Types);
        }

        private IEnumerable<MemberResult> GetDatabaseTables(int index, string token)
        {
            return GetDefinedMembers(index, AstMemberType.Tables);
        }

        private IEnumerable<MemberResult> GetDatabaseTableColumns(int index, string token)
        {
            if (_databaseProvider != null)
            {
                return _databaseProvider.GetColumns(token).Select(x => new MemberResult(x.Name, x, GeneroMemberType.DbColumn, this));
            }
            return new List<MemberResult>();
        }

        internal IAnalysisResult TryGetUserDefinedType(string typeName, int index)
        {
            // do a binary search to determine what node we're in
            IAnalysisResult type = null;
            if (_body.Children.Count > 0)
            {
                List<int> keys = _body.Children.Select(x => x.Key).ToList();
                int searchIndex = keys.BinarySearch(index);
                if (searchIndex < 0)
                {
                    searchIndex = ~searchIndex;
                    if (searchIndex > 0)
                        searchIndex--;
                }

                int key = keys[searchIndex];

                _body.SetNamespace(null);
                // TODO: need to handle multiple results of the same name
                AstNode4gl containingNode = _body.Children[key];
                if (containingNode != null)
                {
                    if (containingNode is IFunctionResult)
                    {
                        if ((containingNode as IFunctionResult).Types.TryGetValue(typeName, out type))
                        {
                            return type;
                        }
                    }

                    if (_body is IModuleResult)
                    {
                        if ((_body as IModuleResult).Types.TryGetValue(typeName, out type) ||
                            (_body as IModuleResult).GlobalTypes.TryGetValue(typeName, out type))
                        {
                            return type;
                        }
                    }
                }
            }

            // TODO: this could probably be done more efficiently by having each GeneroAst load globals and functions into
            // dictionaries stored on the IGeneroProject, instead of in each project entry.
            // However, this does required more upkeep when changes occur. Will look into it...
            if (_projEntry != null && _projEntry is IGeneroProjectEntry)
            {
                IGeneroProjectEntry genProj = _projEntry as IGeneroProjectEntry;
                if (genProj.ParentProject != null)
                {
                    foreach (var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                    {
                        if (projEntry.Value.Analysis != null &&
                           projEntry.Value.Analysis.Body != null)
                        {
                            projEntry.Value.Analysis.Body.SetNamespace(null);
                            IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                            if (modRes != null)
                            {
                                // check global types

                                if (modRes.GlobalTypes.TryGetValue(typeName, out type))
                                    return type;
                            }
                        }
                    }
                }
            }

            return type;
        }

        private enum AstMemberType
        {
            Variables = 1,
            Constants = 2,
            Types = 4,
            Functions = 8,
            Dialogs = 16,
            Reports = 32,
            Cursors = 64,
            Tables = 128,

            All = Variables | Constants | Types | Functions | Dialogs | Reports | Cursors | Tables
        }

        private IEnumerable<MemberResult> GetDefinedMembers(int index, AstMemberType memberType)
        {
            HashSet<MemberResult> members = new HashSet<MemberResult>();

            HashSet<GeneroPackage> includedPackages = new HashSet<GeneroPackage>();
            if (memberType.HasFlag(AstMemberType.Types))
            {
                // Built-in types
                members.AddRange(BuiltinTypes.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));

                foreach (var package in Packages.Values.Where(x => _importedPackages[x.Name] && x.ContainsInstanceMembers))
                {
                    members.Add(new MemberResult(package.Name, GeneroMemberType.Module, this));
                    includedPackages.Add(package);
                }
            }
            if (memberType.HasFlag(AstMemberType.Constants))
            {
                members.AddRange(SystemConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Keyword, this)));
                members.AddRange(SystemMacros.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
            }
            if (memberType.HasFlag(AstMemberType.Variables))
                members.AddRange(SystemVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Keyword, this)));
            if (memberType.HasFlag(AstMemberType.Functions))
            {
                members.AddRange(SystemFunctions.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Function, this)));
                foreach (var package in Packages.Values.Where(x => _importedPackages[x.Name] && x.ContainsStaticClasses))
                {
                    if (!includedPackages.Contains(package))
                        members.Add(new MemberResult(package.Name, GeneroMemberType.Module, this));
                }
            }

            // do a binary search to determine what node we're in
            if (_body.Children.Count > 0)
            {
                _body.SetNamespace(null);
                List<int> keys = _body.Children.Select(x => x.Key).ToList();
                int searchIndex = keys.BinarySearch(index);
                if (searchIndex < 0)
                {
                    searchIndex = ~searchIndex;
                    if (searchIndex > 0)
                        searchIndex--;
                }

                int key = keys[searchIndex];

                // TODO: need to handle multiple results of the same name
                AstNode4gl containingNode = _body.Children[key];
                if (containingNode != null)
                {
                    if (containingNode is IFunctionResult)
                    {
                        if (memberType.HasFlag(AstMemberType.Variables))
                        {
                            members.AddRange((containingNode as IFunctionResult).Variables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                            foreach (var varList in (containingNode as IFunctionResult).LimitedScopeVariables)
                            {
                                foreach (var item in varList.Value)
                                {
                                    if (item.Item2.IsInSpan(index))
                                    {
                                        members.Add(new MemberResult(item.Item1.Name, item.Item1, GeneroMemberType.Instance, this));
                                        break;
                                    }
                                }
                            }
                        }
                        if (memberType.HasFlag(AstMemberType.Types))
                            members.AddRange((containingNode as IFunctionResult).Types.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                        if (memberType.HasFlag(AstMemberType.Constants))
                            members.AddRange((containingNode as IFunctionResult).Constants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                        if (memberType.HasFlag(AstMemberType.Functions))
                        {
                            foreach (var res in (containingNode as IFunctionResult).Variables/*.Where(x => x.Value.HasChildFunctions(this))
                                                                                            */.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                if (!members.Contains(res))
                                    members.Add(res);

                            foreach (var varList in (containingNode as IFunctionResult).LimitedScopeVariables)
                            {
                                foreach (var item in varList.Value)
                                {
                                    if (item.Item2.IsInSpan(index))
                                    {
                                        members.Add(new MemberResult(item.Item1.Name, item.Item1, GeneroMemberType.Instance, this));
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (_body is IModuleResult)
                    {
                        // check for module vars, types, and constants (and globals defined in this module)
                        if (memberType.HasFlag(AstMemberType.Variables))
                        {
                            members.AddRange((_body as IModuleResult).Variables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                            members.AddRange((_body as IModuleResult).GlobalVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                        }
                        if (memberType.HasFlag(AstMemberType.Types))
                        {
                            members.AddRange((_body as IModuleResult).Types.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                            members.AddRange((_body as IModuleResult).GlobalTypes.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                        }
                        if (memberType.HasFlag(AstMemberType.Constants))
                        {
                            members.AddRange((_body as IModuleResult).Constants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                            members.AddRange((_body as IModuleResult).GlobalConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                        }
                        if (memberType.HasFlag(AstMemberType.Dialogs))
                        {
                            members.AddRange((_body as IModuleResult).Functions.Where(x => x.Value is DeclarativeDialogBlock)
                                                             .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Dialog, this)));
                            members.AddRange((_body as IModuleResult).FglImports.Select(x => new MemberResult(x, GeneroMemberType.Module, this)));
                        }
                        if (memberType.HasFlag(AstMemberType.Reports))
                        {
                            members.AddRange((_body as IModuleResult).Functions.Where(x => x.Value is ReportBlockNode)
                                                             .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Report, this)));
                        }
                        if (memberType.HasFlag(AstMemberType.Functions))
                        {
                            members.AddRange((_body as IModuleResult).Functions
                                                                     .Where(x => !(x.Value is ReportBlockNode) && !(x.Value is DeclarativeDialogBlock))
                                                                     .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Method, this)));
                            foreach (var res in (_body as IModuleResult).Variables/*.Where(x => x.Value.HasChildFunctions(this))
                                                                                            */.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                if (!members.Contains(res))
                                    members.Add(res);
                            foreach (var res in (_body as IModuleResult).GlobalVariables/*.Where(x => x.Value.HasChildFunctions(this))
                                                                                            */.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                if (!members.Contains(res))
                                    members.Add(res);
                        }

                        // Tables and cursors are module specific, and cannot be accessed via fgl import
                        if (memberType.HasFlag(AstMemberType.Cursors) ||
                            memberType.HasFlag(AstMemberType.Tables))
                        {
                            if (memberType.HasFlag(AstMemberType.Cursors))
                                members.AddRange((_body as IModuleResult).Cursors.Select(x => new MemberResult(x.Value.Name, x.Value, GeneroMemberType.Cursor, this)));
                            if (memberType.HasFlag(AstMemberType.Tables))
                                members.AddRange((_body as IModuleResult).Tables.Select(x => new MemberResult(x.Value.Name, x.Value, GeneroMemberType.DbTable, this)));
                        }
                        else
                        {
                            members.AddRange((_body as IModuleResult).FglImports.Select(x => new MemberResult(x, GeneroMemberType.Module, this)));
                        }
                    }
                }
            }

            // TODO: this could probably be done more efficiently by having each GeneroAst load globals and functions into
            // dictionaries stored on the IGeneroProject, instead of in each project entry.
            // However, this does required more upkeep when changes occur. Will look into it...
            if (_projEntry != null && _projEntry is IGeneroProjectEntry)
            {
                IGeneroProjectEntry genProj = _projEntry as IGeneroProjectEntry;
                if (genProj.ParentProject != null)
                {
                    foreach (var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                    {
                        if (projEntry.Value.Analysis != null &&
                           projEntry.Value.Analysis.Body != null)
                        {
                            projEntry.Value.Analysis.Body.SetNamespace(null);
                            IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                            if (modRes != null)
                            {
                                // check global types
                                // TODO: need to add an option to enable/disable legacy linking (to not reference other modules' non-public members
                                if (memberType.HasFlag(AstMemberType.Variables))
                                {
                                    members.AddRange(modRes.GlobalVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                                    members.AddRange(modRes.Variables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                                }
                                if (memberType.HasFlag(AstMemberType.Types))
                                {
                                    members.AddRange(modRes.GlobalTypes.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                                    members.AddRange(modRes.Types.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                                }
                                if (memberType.HasFlag(AstMemberType.Constants))
                                {
                                    members.AddRange(modRes.GlobalConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                                    members.AddRange(modRes.Constants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                                }
                                if (memberType.HasFlag(AstMemberType.Dialogs))
                                {
                                    members.AddRange(modRes.Functions.Where(x => x.Value is DeclarativeDialogBlock)
                                                                     .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Dialog, this)));
                                }
                                if (memberType.HasFlag(AstMemberType.Reports))
                                {
                                    members.AddRange(modRes.Functions.Where(x => x.Value is ReportBlockNode)
                                                                     .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Report, this)));
                                }
                                if (memberType.HasFlag(AstMemberType.Functions))
                                {
                                    members.AddRange(modRes.Functions.Where(x => !(x.Value is ReportBlockNode) && !(x.Value is DeclarativeDialogBlock))
                                                                     .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Method, this)));
                                    foreach (var res in modRes.GlobalVariables/*.Where(x => 
                                        {
                                            return x.Value.HasChildFunctions(this);
                                        })*/
                                        .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                        if (!members.Contains(res))
                                            members.Add(res);
                                    foreach (var res in modRes.Variables/*.Where(x => 
                                        {
                                            return x.Value.HasChildFunctions(this);
                                        })*/
                                        .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                        if (!members.Contains(res))
                                            members.Add(res);
                                }
                            }
                        }
                    }
                }
            }

            if (memberType.HasFlag(AstMemberType.Functions))
            {
                _includePublicFunctions = true; // allow for deferred adding of public functions
            }

            if(memberType.HasFlag(AstMemberType.Tables))
            {
                _includeDatabaseTables = true;  // allow for deferred adding of external database tables
            }

            members.AddRange(this.ProjectEntry.GetIncludedFiles().Where(x => x.Analysis != null).SelectMany(x => x.Analysis.GetDefinedMembers(1, memberType)));

            return members;
        }

        private IEnumerable<MemberResult> GetKeywordMembers(GetMemberOptions options/*, InterpreterScope scope*/)
        {
            IEnumerable<string> keywords = null;

            keywords = Tokens.Keywords.Keys;

            return keywords.Select(kw => new MemberResult(kw, GeneroMemberType.Keyword, this));
        }


        #endregion

        #region Context Determiners

        private bool TryMemberAccess(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results, MemberType memberType = MemberType.All)
        {
            results = new List<MemberResult>();
            bool skipGettingNext = false;
            var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
            while (true)
            {
                if (!skipGettingNext)
                {
                    if (!enumerator.MoveNext())
                    {
                        results.Clear();
                        return false;
                    }
                }
                else
                {
                    skipGettingNext = false;
                }

                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if (tokInfo.Token.Kind == TokenKind.Dot || tokInfo.Token.VerbatimImage.EndsWith("."))
                {
                    if (tokInfo.Category == TokenCategory.NumericLiteral)
                        return true;

                    // now we need to analyze the variable reference to get its members
                    int startIndex, endIndex;
                    bool isFunctionCallOrDefinition;
                    string var = revTokenizer.GetExpressionText(out startIndex, out endIndex, out isFunctionCallOrDefinition);
                    if (var != null)
                    {
                        int dotDiff = endIndex - tokInfo.SourceSpan.End.Index;
                        var = var.Substring(0, (var.Length - dotDiff));
                        if (var.EndsWith("."))
                        {
                            var = var.Substring(0, var.Length - 1);
                            IGeneroProject dummyProj;
                            IProjectEntry projEntry;
                            var analysisRes = GetValueByIndex(var, index, _functionProvider, _databaseProvider, _programFileProvider, false, out dummyProj, out projEntry);
                            if (analysisRes != null)
                            {
                                IEnumerable<MemberResult> memberList = analysisRes.GetMembers(this, memberType, !var[var.Length - 1].Equals(']'));
                                if (memberList != null)
                                {
                                    results.AddRange(memberList);
                                }
                            }
                            return true;
                        }
                    }
                }

                return false;
            }

            return false;
        }

        private static HashSet<TokenKind> _binaryOperators = new HashSet<TokenKind>
        {
            TokenKind.Add, TokenKind.Subtract, TokenKind.Multiply, TokenKind.Divide, TokenKind.Power,
            TokenKind.DoubleEquals, TokenKind.LessThan, TokenKind.GreaterThan, TokenKind.LessThanOrEqual,
            TokenKind.GreaterThanOrEqual, TokenKind.Equals, TokenKind.NotEquals, TokenKind.NotEqualsLTGT,
            TokenKind.DoubleBar, TokenKind.Assign, TokenKind.AndKeyword, TokenKind.OrKeyword, TokenKind.ModKeyword,
            TokenKind.UsingKeyword, TokenKind.AsKeyword, TokenKind.UnitsKeyword
        };

        private static HashSet<TokenKind> _preUnaryOperators = new HashSet<TokenKind>
        {
            TokenKind.NotKeyword, TokenKind.AsciiKeyword
        };

        private static HashSet<TokenKind> _postUnaryOperators = new HashSet<TokenKind>
        {
            TokenKind.ClippedKeyword
        };

        #endregion

        #region Enums

        public static HashSet<TokenKind> Acceptable_ReturnVariableName_StatementKeywords = new HashSet<TokenKind>
        {
            TokenKind.ConstantKeyword,
            TokenKind.TypeKeyword,
            TokenKind.DefineKeyword,
            TokenKind.MainKeyword,
            TokenKind.FunctionKeyword,
            TokenKind.ReportKeyword,
            TokenKind.ImportKeyword,
            TokenKind.SchemaKeyword,
            TokenKind.DescribeKeyword,
            TokenKind.DatabaseKeyword,
            TokenKind.GlobalsKeyword,
        };

        public static HashSet<TokenKind> ValidStatementKeywords = new HashSet<TokenKind>
        {
            // keywords that are valid after an access mod
            TokenKind.ConstantKeyword,
            TokenKind.TypeKeyword,
            TokenKind.DefineKeyword,
            TokenKind.MainKeyword,
            TokenKind.FunctionKeyword,
            TokenKind.ReportKeyword,

            // Valid keywords within the module
            TokenKind.OptionsKeyword,
            TokenKind.ImportKeyword,    // this is covered
            TokenKind.SchemaKeyword,
            TokenKind.DescribeKeyword,
            TokenKind.DatabaseKeyword,
            TokenKind.GlobalsKeyword,

            // Valid keywords that apply to the module keywords
            TokenKind.EndKeyword,       // end { GLOBALS | MAIN | FUNCTION | REPORT | CASE | FOR | IF | WHILE | FOREACH | MENU | DIALOG | CONSTRUCT | DISPLAY | INPUT | FOREACH }

            // Valid statement start keywords (TODO: definitely missing some here...)
            // Flow control
            TokenKind.CallKeyword,      // call func_name([param1 [,...]]) [returning ret1 [,...]] (this is covered)
            TokenKind.ReturnKeyword,    // return [ret1 [,...]]
            TokenKind.CaseKeyword,
            TokenKind.WhenKeyword,
            TokenKind.OtherwiseKeyword,
            TokenKind.ContinueKeyword,  // continue { FOR | FOREACH | WHILE | MENU | CONSTRUCT | INPUT | DIALOG }
            TokenKind.ExitKeyword,      // exit { FOR | FOREACH | WHILE | MENU | CONSTRUCT | REPORT | DISPLAY | INPUT | DIALOG }
            TokenKind.ForKeyword,
            TokenKind.GotoKeyword,
            TokenKind.IfKeyword,
            TokenKind.LabelKeyword,
            TokenKind.SleepKeyword,
            TokenKind.WhileKeyword,
            TokenKind.ElseKeyword,

            // assignment
            TokenKind.LetKeyword,   // this is covered

            // exceptions
            TokenKind.WheneverKeyword,
            TokenKind.TryKeyword,
            TokenKind.CatchKeyword,

            // variable statements
            TokenKind.InitializeKeyword,
            TokenKind.LocateKeyword,
            TokenKind.FreeKeyword,
            TokenKind.ValidateKeyword,

            // static sql
            TokenKind.SelectKeyword,
            TokenKind.UpdateKeyword,
            TokenKind.DeleteKeyword,
            TokenKind.InsertKeyword,
            TokenKind.SqlKeyword,
            TokenKind.CreateKeyword,
            TokenKind.AlterKeyword,
            TokenKind.DropKeyword,
            TokenKind.RenameKeyword,

            // dynamic sql
            TokenKind.PrepareKeyword,
            TokenKind.ExecuteKeyword,
            TokenKind.DeclareKeyword,
            TokenKind.OpenKeyword,
            TokenKind.FetchKeyword,
            TokenKind.CloseKeyword,
            TokenKind.ForeachKeyword,
            TokenKind.PutKeyword,
            TokenKind.FlushKeyword,
            TokenKind.LoadKeyword,
            TokenKind.UnloadKeyword,


            // config options
            TokenKind.DeferKeyword,
            
            // User Interface
            TokenKind.MenuKeyword,
            TokenKind.InputKeyword,
            TokenKind.DisplayKeyword,
            TokenKind.ConstructKeyword,
            TokenKind.DialogKeyword,
            TokenKind.PromptKeyword,
            TokenKind.MessageKeyword,
            TokenKind.ErrorKeyword,
            TokenKind.ClearKeyword,
            TokenKind.ScrollKeyword,

            TokenKind.CommandKeyword,
            TokenKind.OnKeyword,
            TokenKind.BeforeKeyword,
            TokenKind.AfterKeyword,
            TokenKind.NextKeyword,
            TokenKind.ShowKeyword,
            TokenKind.HideKeyword,
            

            // Report driver
            TokenKind.StartKeyword,
            TokenKind.FinishKeyword,
            TokenKind.OutputKeyword,
            TokenKind.TerminateKeyword,

            // Report routine
            TokenKind.FormatKeyword,
            TokenKind.PrintKeyword,
            TokenKind.PrintxKeyword,
            TokenKind.NeedKeyword,
            TokenKind.PauseKeyword,
            TokenKind.SkipKeyword,
        };

        private static TokenKind[] OptionsStartTokens = new TokenKind[]
        {
            TokenKind.ShortKeyword,
            TokenKind.MenuKeyword,
            TokenKind.MessageKeyword,
            TokenKind.Comment,
            TokenKind.PromptKeyword,
            TokenKind.ErrorKeyword,
            TokenKind.FormKeyword,
            TokenKind.InputKeyword,
            TokenKind.DisplayKeyword,
            TokenKind.FieldKeyword,
            TokenKind.OnKeyword,
            TokenKind.HelpKeyword,
            TokenKind.InsertKeyword,
            TokenKind.DeleteKeyword,
            TokenKind.NextKeyword,
            TokenKind.PreviousKeyword,
            TokenKind.AcceptKeyword,
            TokenKind.RunKeyword,
            TokenKind.SqlKeyword
        };

        public static HashSet<TokenKind> BuiltinTypes = new HashSet<TokenKind>
        {
            TokenKind.CharKeyword,
            TokenKind.CharacterKeyword,
            TokenKind.VarcharKeyword,
            TokenKind.StringKeyword,
            TokenKind.DatetimeKeyword,
            TokenKind.DateKeyword,
            TokenKind.IntervalKeyword,
            TokenKind.BigintKeyword,
            TokenKind.IntegerKeyword,
            TokenKind.IntKeyword,
            TokenKind.SmallintKeyword,
            TokenKind.TinyintKeyword,
            TokenKind.FloatKeyword,
            TokenKind.SmallfloatKeyword,
            TokenKind.DecimalKeyword,
            TokenKind.DecKeyword,
            TokenKind.NumericKeyword,
            TokenKind.MoneyKeyword,
            TokenKind.ByteKeyword,
            TokenKind.TextKeyword,
            TokenKind.BooleanKeyword
        };

        #endregion

        /// <summary>
        /// Gets the available names at the given location.  This includes built-in variables, global variables, and locals.
        /// </summary>
        /// <param name="index">The 0-based absolute index into the file where the available mebmers should be looked up.</param>
        public IEnumerable<MemberResult> GetAllAvailableMembersByIndex(int index, IReverseTokenizer revTokenizer, 
                                                                        out bool includePublicFunctions, out bool includeDatabaseTables, 
                                                                        GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            _includePublicFunctions = includePublicFunctions = false;    // this is a flag that the context determination logic sets if public functions should eventually be included in the set
            _includeDatabaseTables = includeDatabaseTables = false;

            List<MemberResult> members = new List<MemberResult>();
            // First see if we have a member completion
            if (TryMemberAccess(index, revTokenizer, out members))
            {
                _includePublicFunctions = false;
                _includeDatabaseTables = false;
            }
            else
            {
                // Calling determine context here verifies that the context isn't in a position where completions should be shown at all.
                if (!DetermineContext(index, revTokenizer, members, true))
                {
                    HashSet<MemberResult> res = new HashSet<MemberResult>();
                    res.AddRange(GetKeywordMembers(options));
                    res.AddRange(GetDefinedMembers(index, AstMemberType.All));
                    includePublicFunctions = _includePublicFunctions;
                    includeDatabaseTables = _includeDatabaseTables;
                    _includePublicFunctions = false;    // reset the flag
                    _includeDatabaseTables = false;
                    return res;
                }
            }
            includePublicFunctions = _includePublicFunctions;
            includeDatabaseTables = _includeDatabaseTables;
            _includePublicFunctions = false;    // reset the flag
            _includeDatabaseTables = false;
            return members;
        }

        private static AstNode4gl GetContainingNode(AstNode4gl currentNode, int index)
        {
            AstNode4gl containingNode = null;
            if (currentNode.Children.Count > 0)
            {
                List<int> keys = currentNode.Children.Select(x => x.Key).ToList();
                int searchIndex = keys.BinarySearch(index);
                if (searchIndex < 0)
                {
                    searchIndex = ~searchIndex;
                    if (searchIndex > 0)
                        searchIndex--;
                }

                int key = keys[searchIndex];

                // TODO: need to handle multiple results of the same name
                containingNode = currentNode.Children[key];
            }
            return containingNode;
        }

        private IEnumerable<MemberResult> GetInstanceExpressionComponents(int index)
        {
            // TODO: return
            // 1) Variables (system, local, module, global)
            // 2) Constants (system, local, module, global)
            // 3) Functions
            // 4) Function provider
            // 5) Imported modules
            // 6) Available packages
            // maybe more...
            return GetDefinedMembers(index, AstMemberType.Variables | AstMemberType.Constants | AstMemberType.Functions)
                .Union(_preUnaryOperators.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
        }

        private IEnumerable<MemberResult> GetInstanceTypes(int index)
        {
            return GetDefinedMembers(index, AstMemberType.Types);
        }

        private IEnumerable<MemberResult> GetInstanceFunctions(int index)
        {
            return GetDefinedMembers(index, AstMemberType.Functions);
        }

        private IEnumerable<MemberResult> GetInstanceLabels(int index)
        {
            return new MemberResult[0];
        }

        private IEnumerable<MemberResult> GetInstanceDeclaredDialogs(int index)
        {
            return GetDefinedMembers(index, AstMemberType.Dialogs);
        }

        private IEnumerable<MemberResult> GetInstanceReports(int index)
        {
            return GetDefinedMembers(index, AstMemberType.Reports);
        }

        private IEnumerable<MemberResult> GetInstanceVariables(int index)
        {
            return GetDefinedMembers(index, AstMemberType.Variables);
        }

        private IEnumerable<MemberResult> GetInstanceConstants(int index)
        {
            return GetDefinedMembers(index, AstMemberType.Constants);
        }

        private IEnumerable<MemberResult> GetInstanceImportModules(int index)
        {
            if (_programFileProvider != null)
                return _programFileProvider.GetAvailableImportModules().Select(x => new MemberResult(x, GeneroMemberType.Module, this));
            else
                return new MemberResult[0];
        }

        private IEnumerable<MemberResult> GetInstanceCursors(int index)
        {
            return GetDefinedMembers(index, AstMemberType.Cursors);
        }
    }
}
