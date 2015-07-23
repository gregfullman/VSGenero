using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public partial class GeneroAst
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
            return GetDefinedMembers(index, false, false, true, false);
        }

        private IEnumerable<MemberResult> GetDatabaseTables(int index, string token)
        {
            if (_databaseProvider != null)
            {
                return _databaseProvider.GetTables().Select(x => new MemberResult(x.Name, x, (x.TableType == DatabaseTableType.Table ? GeneroMemberType.DbTable : GeneroMemberType.DbView), this));
            }
            return new List<MemberResult>();
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

                // TODO: need to handle multiple results of the same name
                AstNode containingNode = _body.Children[key];
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

        private IEnumerable<MemberResult> GetDefinedMembers(int index, bool vars, bool consts, bool types, bool funcs)
        {
            HashSet<MemberResult> members = new HashSet<MemberResult>();

            HashSet<GeneroPackage> includedPackages = new HashSet<GeneroPackage>();
            if (types)
            {
                // Built-in types
                members.AddRange(BuiltinTypes.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));

                foreach (var package in Packages.Values.Where(x => _importedPackages[x.Name] && x.ContainsInstanceMembers))
                {
                    members.Add(new MemberResult(package.Name, GeneroMemberType.Module, this));
                    includedPackages.Add(package);
                }
            }
            if (consts)
            {
                members.AddRange(SystemConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Keyword, this)));
                members.AddRange(SystemMacros.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
            }
            if (vars)
                members.AddRange(SystemVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Keyword, this)));
            if (funcs)
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
                AstNode containingNode = _body.Children[key];
                if (containingNode != null)
                {
                    if (containingNode is IFunctionResult)
                    {
                        if (vars)
                            members.AddRange((containingNode as IFunctionResult).Variables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                        if (types)
                            members.AddRange((containingNode as IFunctionResult).Types.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                        if (consts)
                            members.AddRange((containingNode as IFunctionResult).Constants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                        if (funcs)
                        {
                            foreach (var res in (containingNode as IFunctionResult).Variables/*.Where(x => x.Value.HasChildFunctions(this))
                                                                                            */.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                if (!members.Contains(res))
                                    members.Add(res);
                        }
                    }

                    if (_body is IModuleResult)
                    {
                        // check for module vars, types, and constants (and globals defined in this module)
                        if (vars)
                        {
                            members.AddRange((_body as IModuleResult).Variables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                            members.AddRange((_body as IModuleResult).GlobalVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                        }
                        if (types)
                        {
                            members.AddRange((_body as IModuleResult).Types.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                            members.AddRange((_body as IModuleResult).GlobalTypes.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                        }
                        if (consts)
                        {
                            members.AddRange((_body as IModuleResult).Constants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                            members.AddRange((_body as IModuleResult).GlobalConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                        }
                        if (funcs)
                        {
                            members.AddRange((_body as IModuleResult).Functions.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Method, this)));
                            foreach (var res in (_body as IModuleResult).Variables/*.Where(x => x.Value.HasChildFunctions(this))
                                                                                            */.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                if (!members.Contains(res))
                                    members.Add(res);
                            foreach (var res in (_body as IModuleResult).GlobalVariables/*.Where(x => x.Value.HasChildFunctions(this))
                                                                                            */.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                if (!members.Contains(res))
                                    members.Add(res);
                        }

                        members.AddRange((_body as IModuleResult).FglImports.Select(x => new MemberResult(x, GeneroMemberType.Module, this)));
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
                            IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                            if (modRes != null)
                            {
                                // check global types
                                if (vars)
                                    members.AddRange(modRes.GlobalVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                                if (types)
                                    members.AddRange(modRes.GlobalTypes.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                                if (consts)
                                    members.AddRange(modRes.GlobalConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                                if (funcs)
                                {
                                    members.AddRange(modRes.Functions.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Method, this)));
                                    foreach (var res in modRes.GlobalVariables/*.Where(x => 
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

            if (funcs && _functionProvider != null)
            {
                members.Add(new MemberResult(_functionProvider.Name, GeneroMemberType.Namespace, this));

                if(_includePublicFunctions && !string.IsNullOrWhiteSpace(_contextString))
                {
                    members.AddRange(_functionProvider.GetFunctionsStartingWith(_contextString).Select(x => new MemberResult(x.Name, x, GeneroMemberType.Function, this)));
                }
            }

            members.AddRange(this.ProjectEntry.GetIncludedFiles().Where(x => x.Analysis != null).SelectMany(x => x.Analysis.GetDefinedMembers(1, vars, consts, types, funcs)));

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

                if (tokInfo.Token.Kind == TokenKind.Dot)
                {
                    // now we need to analyze the variable reference to get its members
                    int startIndex, endIndex;
                    string var = revTokenizer.GetExpressionText(out startIndex, out endIndex);
                    if (var != null)
                    {
                        int dotDiff = endIndex - tokInfo.SourceSpan.End.Index;
                        var = var.Substring(0, (var.Length - dotDiff));
                        if (var.EndsWith("."))
                        {
                            var = var.Substring(0, var.Length - 1);
                            IGeneroProject dummyProj;
                            IProjectEntry projEntry;
                            var analysisRes = GetValueByIndex(var, index, _functionProvider, _databaseProvider, _programFileProvider, out dummyProj, out projEntry);
                            if (analysisRes != null)
                            {
                                var members = analysisRes.GetMembers(this, memberType);
                                if (members != null)
                                {
                                    if (analysisRes is VariableDef && (analysisRes as VariableDef).Type.Children.Any(x => x.Value is ArrayTypeReference))
                                    {
                                        bool varsOnly = var[var.Length - 1].Equals(']');
                                        if (varsOnly)
                                            results.AddRange(members.Where(x => !(x.Var is IFunctionResult)));
                                        else
                                            results.AddRange(members.Where(x => x.Var is IFunctionResult));
                                    }
                                    else
                                    {
                                        results.AddRange(members);
                                    }
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

        private static TokenKind[] BuiltinTypes = new TokenKind[]
        {
            TokenKind.CharKeyword,
            TokenKind.CharacterKeyword,
            TokenKind.VarcharKeyword,
            TokenKind.StringKeyword,
            TokenKind.DatetimeKeyword,
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

        #region Public Member Providers

        /// <summary>
        /// Gets the available names at the given location.  This includes built-in variables, global variables, and locals.
        /// </summary>
        /// <param name="index">The 0-based absolute index into the file where the available mebmers should be looked up.</param>
        public IEnumerable<MemberResult> GetAllAvailableMembersByIndex(int index, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            IEnumerable<MemberResult> res = GetKeywordMembers(options);

            // do a binary search to determine what node we're in
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

                // TODO: need to handle multiple results of the same name
                AstNode containingNode = _body.Children[key];
                if (containingNode != null)
                {
                    if (containingNode is IFunctionResult)
                    {
                        IFunctionResult func = containingNode as IFunctionResult;
                        res = res.Union(func.Variables.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Instance, this)));
                        res = res.Union(func.Types.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Class, this)));
                        res = res.Union(func.Constants.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Constant, this)));
                    }

                    if (_body is IModuleResult)
                    {
                        // check for module vars, types, and constants (and globals defined in this module)
                        IModuleResult mod = _body as IModuleResult;
                        res = res.Union(mod.Variables.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Module, this)));
                        res = res.Union(mod.Types.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Class, this)));
                        res = res.Union(mod.Constants.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Constant, this)));
                        res = res.Union(mod.GlobalVariables.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Module, this)));
                        res = res.Union(mod.GlobalTypes.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Class, this)));
                        res = res.Union(mod.GlobalConstants.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Constant, this)));

                        // check for cursors in this module
                        res = res.Union(mod.Cursors.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Unknown, this)));

                        // check for module functio
                        res = res.Union(mod.Functions.Keys.Select(x => new MemberResult(x, x, GeneroMemberType.Method, this)));
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
                                    IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                                    if (modRes != null)
                                    {
                                        // check global vars, types, and constants
                                        res = res.Union(modRes.Variables.Keys.Select(x => new MemberResult(x, GeneroMemberType.Module, this)));
                                        res = res.Union(modRes.Types.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
                                        res = res.Union(modRes.Constants.Keys.Select(x => new MemberResult(x, GeneroMemberType.Constant, this)));
                                        res = res.Union(modRes.GlobalVariables.Keys.Select(x => new MemberResult(x, GeneroMemberType.Module, this)));
                                        res = res.Union(modRes.GlobalTypes.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
                                        res = res.Union(modRes.GlobalConstants.Keys.Select(x => new MemberResult(x, GeneroMemberType.Constant, this)));

                                        // check for cursors in this module
                                        res = res.Union(modRes.Cursors.Keys.Select(x => new MemberResult(x, GeneroMemberType.Unknown, this)));

                                        // check for module functions
                                        res = res.Union(modRes.Functions.Keys.Select(x => new MemberResult(x, GeneroMemberType.Method, this)));
                                    }
                                }
                            }
                        }
                    }

                    /* TODO:
                     * Need to check for:
                     * 1) Temp tables
                     * 2) DB Tables and columns
                     * 3) Record fields
                     * 7) Public functions
                     */
                }
            }

            return res;
        }

        /// <summary>
        /// Evaluates a given expression and returns a list of members which exist in the expression.
        /// 
        /// If the expression is an empty string returns all available members at that location.
        /// 
        /// index is a zero-based absolute index into the file.
        /// </summary>
        public IEnumerable<MemberResult> GetMembersByIndex(string exprText, int index, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            List<MemberResult> results = new List<MemberResult>();

            return results;
        }

        #endregion

        private AstNode GetContainingNode(AstNode currentNode, int index)
        {
            AstNode containingNode = null;
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
            return GetDefinedMembers(index, true, true, false, true)
                .Union(_preUnaryOperators.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
        }

        private IEnumerable<MemberResult> GetInstanceTypes(int index)
        {
            return GetDefinedMembers(index, false, false, true, false);
        }

        private IEnumerable<MemberResult> GetInstanceFunctions(int index)
        {
            return GetDefinedMembers(index, false, false, false, true);
        }

        private IEnumerable<MemberResult> GetInstanceLabels(int index)
        {
            return new MemberResult[0];
        }

        private IEnumerable<MemberResult> GetInstanceVariables(int index)
        {
            return GetDefinedMembers(index, true, false, false, false);
        }

        private IEnumerable<MemberResult> GetInstanceConstants(int index)
        {
            return GetDefinedMembers(index, false, true, false, false);
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
            return new MemberResult[0];
        }
    }
}
