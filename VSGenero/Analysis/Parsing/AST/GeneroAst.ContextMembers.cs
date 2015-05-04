using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public partial class GeneroAst
    {
        #region Static Member Framework

        private static object _contextMapsLock = new object();
        private static CompletionContextMap _defineStatementMap;
        private static CompletionContextMap _typeConstraintsMap;

        private static IEnumerable<MemberResult> ProvideAdditionalTypes(int index, string token)
        {
            if (_additionalTypesProvider != null)
            {
                return _additionalTypesProvider(index, token);
            }
            return new List<MemberResult>();
        }

        private static IEnumerable<MemberResult> ProvideTables(int index, string token)
        {
            if (_tablesProvider != null)
            {
                return _tablesProvider(index, token);
            }
            return new List<MemberResult>();
        }

        private static IEnumerable<MemberResult> ProvideTableColumns(int index, string token)
        {
            if(_tableColumnsProvider != null)
            {
                return _tableColumnsProvider(index, token);
            }
            return new List<MemberResult>();
        }

        private static Func<int, string, IEnumerable<MemberResult>> _additionalTypesProvider;
        private static Func<int, string, IEnumerable<MemberResult>> _tablesProvider;
        private static Func<int, string, IEnumerable<MemberResult>> _tableColumnsProvider;

        private static void InitializeCompletionContextMaps()
        {
            lock (_contextMapsLock)
            {
                if (_defineStatementMap == null)
                {
                    // 1) Define statement map
                    _defineStatementMap = new CompletionContextMap(new HashSet<TokenKind>(new[] { TokenKind.DefineKeyword, TokenKind.TypeKeyword }));
                    _defineStatementMap.Map.Add(TokenKind.Comma, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.Multiply, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.RecordKeyword, new List<TokenKindWithConstraint>()),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.NumericLiteral }, new List<TokenKindWithConstraint>()),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier }, new List<TokenKindWithConstraint>())
                });
                    _defineStatementMap.Map.Add(TokenCategory.NumericLiteral, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.Comma, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.LeftBracket, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.DimensionKeyword, new List<TokenKindWithConstraint>())
                });
                    _defineStatementMap.Map.Add(TokenKind.LeftBracket, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.ArrayKeyword, new List<TokenKindWithConstraint>())
                });
                    _defineStatementMap.Map.Add(TokenKind.Multiply, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.Dot, new List<TokenKindWithConstraint>()) { IsBreakingStateOnFirstPrevious = true }
                });
                    _defineStatementMap.Map.Add(TokenKind.ArrayKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.DynamicKeyword, new List<TokenKindWithConstraint>()),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier },
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.WithKeyword, TokenKind.DynamicKeyword, 1),
                            new TokenKindWithConstraint(TokenKind.OfKeyword, TokenKind.DynamicKeyword, 1)
                        })
                });
                    _defineStatementMap.Map.Add(TokenKind.DynamicKeyword, new List<CompletionPossibility>
                {
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier },
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.ArrayKeyword),
                        })
                });
                    _defineStatementMap.Map.Add(TokenKind.RightBracket, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.LeftBracket, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.OfKeyword)
                        }),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.NumericLiteral },
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.OfKeyword)
                        })
                });
                    _defineStatementMap.Map.Add(TokenKind.WithKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.ArrayKeyword, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.DimensionKeyword, TokenKind.DynamicKeyword, 2),
                        })
                });
                    _defineStatementMap.Map.Add(TokenKind.DimensionKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.WithKeyword, new List<TokenKindWithConstraint>())
                });
                    _defineStatementMap.Map.Add(TokenKind.OfKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.RightBracket, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, ProvideAdditionalTypes),
                    new TokenKindCompletionPossiblity(TokenKind.ArrayKeyword, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, ProvideAdditionalTypes),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.NumericLiteral },
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, ProvideAdditionalTypes)
                });
                    _defineStatementMap.Map.Add(TokenKind.LikeKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.RecordKeyword, new List<TokenKindWithConstraint>(), ProvideTables),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier },
                        new List<TokenKindWithConstraint>(), ProvideTables)
                });
                    _defineStatementMap.Map.Add(TokenKind.RecordKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.EndKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.OfKeyword, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.LikeKeyword),
                            new TokenKindWithConstraint(TokenKind.AttributeKeyword)
                        }),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier },
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.LikeKeyword),
                            new TokenKindWithConstraint(TokenKind.AttributeKeyword)
                        })
                });
                    _defineStatementMap.Map.Add(TokenKind.EndKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.RecordKeyword, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                        }),
                    new TokenKindCompletionPossiblity(TokenKind.Multiply, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                        }),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier },
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                        })
                });
                    List<CompletionPossibility> keywordAndIdentPossibilities = new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.Multiply, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.Dot, new List<TokenKindWithConstraint>()) { IsBreakingStateOnFirstPrevious = true },
                    new TokenKindCompletionPossiblity(new HashSet<TokenKind> { TokenKind.DefineKeyword, TokenKind.TypeKeyword }, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, ProvideAdditionalTypes),
                    new TokenKindCompletionPossiblity(TokenKind.RecordKeyword, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, ProvideAdditionalTypes),
                        new TokenKindCompletionPossiblity(TokenKind.Comma, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, ProvideAdditionalTypes),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier }, new List<TokenKindWithConstraint>())
                };
                    _defineStatementMap.Map.Add(TokenCategory.Keyword, keywordAndIdentPossibilities);
                    _defineStatementMap.Map.Add(TokenCategory.Identifier, keywordAndIdentPossibilities);
                    _defineStatementMap.Map.Add(TokenKind.Dot, new List<CompletionPossibility>
                {
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier }, new List<TokenKindWithConstraint>(), ProvideTableColumns)
                });
                }

                // 2) Type constraint map
                if (_typeConstraintsMap == null)
                {
                    _typeConstraintsMap = new CompletionContextMap(new HashSet<TokenKind>
                {
                    TokenKind.DecimalKeyword,
                    TokenKind.DecKeyword,
                    TokenKind.NumericKeyword,
                    TokenKind.MoneyKeyword,
                    TokenKind.FloatKeyword,
                    TokenKind.VarcharKeyword,
                    TokenKind.CharKeyword,
                    TokenKind.CharacterKeyword,
                    TokenKind.DatetimeKeyword,
                    TokenKind.IntervalKeyword
                });
                    List<TokenKind> dtAndIntervalCompletions = new List<TokenKind> 
            { 
                TokenKind.MonthKeyword, TokenKind.YearKeyword, TokenKind.FractionKeyword, TokenKind.SecondKeyword, TokenKind.MinuteKeyword, TokenKind.HourKeyword, TokenKind.DayKeyword
            };
                    _typeConstraintsMap.CompletionsForStartToken.Add(TokenKind.IntervalKeyword, dtAndIntervalCompletions);
                    _typeConstraintsMap.CompletionsForStartToken.Add(TokenKind.DatetimeKeyword, dtAndIntervalCompletions);
                    _typeConstraintsMap.Map.Add(TokenKind.RightParenthesis, new List<CompletionPossibility>
                {
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.NumericLiteral }, new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.ToKeyword, TokenKind.IntervalKeyword, 5)
                        })
                });
                    _typeConstraintsMap.Map.Add(TokenCategory.NumericLiteral, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.Comma, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.LeftParenthesis, new List<TokenKindWithConstraint>())
                });
                    _typeConstraintsMap.Map.Add(TokenKind.Comma, new List<CompletionPossibility>
                {
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.NumericLiteral }, new List<TokenKindWithConstraint>())
                });
                    _typeConstraintsMap.Map.Add(TokenKind.LeftParenthesis, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.DecimalKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.DecKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.NumericKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.MoneyKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.FloatKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.FractionKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.VarcharKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.CharKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.CharacterKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.YearKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.MonthKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.DayKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.HourKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.MinuteKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.SecondKeyword, new List<TokenKindWithConstraint>()),
                });
                    var toDtList = new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.MonthKeyword),
                            new TokenKindWithConstraint(TokenKind.YearKeyword),
                            new TokenKindWithConstraint(TokenKind.FractionKeyword),
                            new TokenKindWithConstraint(TokenKind.SecondKeyword),
                            new TokenKindWithConstraint(TokenKind.MinuteKeyword),
                            new TokenKindWithConstraint(TokenKind.HourKeyword),
                            new TokenKindWithConstraint(TokenKind.DayKeyword)
                        };
                    _typeConstraintsMap.Map.Add(TokenKind.ToKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.RightParenthesis, toDtList),
                    new TokenKindCompletionPossiblity(TokenKind.YearKeyword, toDtList),
                    new TokenKindCompletionPossiblity(TokenKind.MonthKeyword, toDtList),
                    new TokenKindCompletionPossiblity(TokenKind.DayKeyword, toDtList),
                    new TokenKindCompletionPossiblity(TokenKind.HourKeyword, toDtList),
                    new TokenKindCompletionPossiblity(TokenKind.MinuteKeyword, toDtList),
                    new TokenKindCompletionPossiblity(TokenKind.SecondKeyword, toDtList),
                    new TokenKindCompletionPossiblity(TokenKind.FractionKeyword, toDtList)
                });
                    var toDtList2 = new List<CompletionPossibility> 
                { 
                    new TokenKindCompletionPossiblity(TokenKind.IntervalKeyword, new List<TokenKindWithConstraint> { new TokenKindWithConstraint(TokenKind.ToKeyword) }),
                    new TokenKindCompletionPossiblity(TokenKind.DatetimeKeyword, new List<TokenKindWithConstraint> { new TokenKindWithConstraint(TokenKind.ToKeyword) }),
                    new TokenKindCompletionPossiblity(TokenKind.ToKeyword, new List<TokenKindWithConstraint>())
                };
                    _typeConstraintsMap.Map.Add(TokenKind.FractionKeyword, toDtList2);
                    _typeConstraintsMap.Map.Add(TokenKind.YearKeyword, toDtList2);
                    _typeConstraintsMap.Map.Add(TokenKind.MonthKeyword, toDtList2);
                    _typeConstraintsMap.Map.Add(TokenKind.DayKeyword, toDtList2);
                    _typeConstraintsMap.Map.Add(TokenKind.HourKeyword, toDtList2);
                    _typeConstraintsMap.Map.Add(TokenKind.MinuteKeyword, toDtList2);
                    _typeConstraintsMap.Map.Add(TokenKind.SecondKeyword, toDtList2);
                    // end Type constraint map
                }
            }
        }

        private static void SetMemberProviders(Func<int, string, IEnumerable<MemberResult>> addtlTypesProvider,
                                                Func<int, string, IEnumerable<MemberResult>> tablesProvider,
                                                Func<int, string, IEnumerable<MemberResult>> tableColumnsProvider)
        {
            _additionalTypesProvider = addtlTypesProvider;
            _tablesProvider = tablesProvider;
            _tableColumnsProvider = tableColumnsProvider;
        }

        #endregion

        #region Member Provider Helpers

        internal IFunctionInformationProvider _functionProvider;
        internal IDatabaseInformationProvider _databaseProvider;

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
            if(_databaseProvider != null)
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
            List<MemberResult> members = new List<MemberResult>();

            HashSet<GeneroPackage> includedPackages = new HashSet<GeneroPackage>();
            if (types)
            {
                // Built-in types
                members.AddRange(BuiltinTypes.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));

                foreach(var package in Packages.Values.Where(x => _importedPackages[x.Name] && x.ContainsInstanceMembers))
                {
                    members.Add(new MemberResult(package.Name, GeneroMemberType.Module, this));
                    includedPackages.Add(package);
                }
            }
            if (consts)
                members.AddRange(SystemConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Keyword, this)));
            if (vars)
                members.AddRange(SystemVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Keyword, this)));
            if(funcs)
            {
                members.AddRange(SystemFunctions.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Function, this)));
                foreach (var package in Packages.Values.Where(x => _importedPackages[x.Name] && x.ContainsStaticClasses))
                {
                    if(!includedPackages.Contains(package))
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
                            foreach (var res in (containingNode as IFunctionResult).Variables.Where(x => x.Value.HasChildFunctions(this))
                                                                                            .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
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
                            foreach (var res in (_body as IModuleResult).Variables.Where(x => x.Value.HasChildFunctions(this))
                                                                                            .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                if (!members.Contains(res))
                                    members.Add(res);
                            foreach (var res in (_body as IModuleResult).GlobalVariables.Where(x => x.Value.HasChildFunctions(this))
                                                                                            .Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)))
                                if (!members.Contains(res))
                                    members.Add(res);
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
                                if (vars)
                                    members.AddRange(modRes.GlobalVariables.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Variable, this)));
                                if (types)
                                    members.AddRange(modRes.GlobalTypes.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Class, this)));
                                if (consts)
                                    members.AddRange(modRes.GlobalConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Constant, this)));
                                if (funcs)
                                {
                                    members.AddRange(modRes.Functions.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Method, this)));
                                    foreach (var res in modRes.GlobalVariables.Where(x => x.Value.HasChildFunctions(this))
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
            }

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

        private bool TryCall(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results)
        {
            TokenKind startingKeyword = TokenKind.CallKeyword;
            results = new List<MemberResult>();
            CallStatus currStatus = CallStatus.None;
            CallStatus firstStatus = CallStatus.None;
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

                if(tokInfo.Category == TokenCategory.Keyword)
                {
                    // check to see if it's a keyword for another statement
                    if(tokInfo.Token.Kind != startingKeyword && ValidStatementKeywords.Contains(tokInfo.Token.Kind))
                    {
                        results.Clear();
                        return false;
                    }
                }

                if (tokInfo.Token.Kind == TokenKind.CallKeyword)
                {
                    if (firstStatus == CallStatus.None)
                    {
                        // provide functions
                        results.AddRange(GetDefinedMembers(index, false, false, false, true));
                    }
                    if (firstStatus == CallStatus.Expression)
                    {
                        // don't clear the results, since it includes "returning"
                        return false;
                    }

                    return true;
                }
                else if (tokInfo.Token.Kind == TokenKind.ReturningKeyword)
                {
                    if (firstStatus == CallStatus.None)
                    {
                        firstStatus = CallStatus.ReturningKeyword;
                        results.AddRange(GetDefinedMembers(index, true, false, false, false));
                    }

                    if (currStatus == CallStatus.None ||
                       currStatus == CallStatus.VariableRef ||
                       currStatus == CallStatus.Expression)
                    {
                        currStatus = CallStatus.ReturningKeyword;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.Comma)
                {
                    // ideally, we'd look back to see if we're after a "returning" keyword...but that may be too much effort.
                    if (firstStatus == CallStatus.None)
                    {
                        firstStatus = CallStatus.Comma;
                        results.AddRange(GetDefinedMembers(index, true, true, false, true));
                    }
                    if (currStatus == CallStatus.None ||
                        currStatus == CallStatus.VariableRef)
                    {
                        currStatus = CallStatus.Comma;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else
                {
                    // first try an expression. The expression will fail (and not have a changed starting index) if it's "returning x, y, ..." because of the returning keyword
                    List<MemberResult> dummyList;
                    int currIndex = tokInfo.SourceSpan.Start.Index + 1;
                    int newIndex;
                    if (!TryExpression(currIndex, revTokenizer, index, out dummyList, out newIndex, new TokenKind[] { TokenKind.ReturningKeyword, TokenKind.CallKeyword }))
                    {
                        if (newIndex < currIndex)
                        {
                            // we're not within a variable reference, but we reversed through one
                            while (tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.NLToken ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.SourceSpan.Start.Index > newIndex)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    results.Clear();
                                    return false;
                                }
                                tokInfo = enumerator.Current;
                            }

                            // we reversed partially through the expression, so we need to use the members in its list
                            if (firstStatus == CallStatus.None)
                            {
                                firstStatus = CallStatus.Expression;
                                results.Add(new MemberResult(Tokens.TokenKinds[TokenKind.ReturningKeyword], GeneroMemberType.Keyword, this));
                            }
                            if (currStatus == CallStatus.None ||
                               currStatus == CallStatus.ReturningKeyword ||
                               currStatus == CallStatus.Comma)
                            {
                                currStatus = CallStatus.Expression;
                            }
                        }
                        else
                        {
                            if (!TryVariableReference(currIndex, revTokenizer, out dummyList, out newIndex) || newIndex < currIndex)
                            {
                                if (newIndex < currIndex)
                                {
                                    // we're not within a variable reference, but we reversed through one
                                    while (tokInfo.Equals(default(TokenInfo)) ||
                                            tokInfo.Token.Kind == TokenKind.NewLine ||
                                            tokInfo.Token.Kind == TokenKind.NLToken ||
                                            tokInfo.Token.Kind == TokenKind.Comment ||
                                            tokInfo.SourceSpan.Start.Index > newIndex)
                                    {
                                        if (!enumerator.MoveNext())
                                        {
                                            results.Clear();
                                            return false;
                                        }
                                        tokInfo = enumerator.Current;
                                    }

                                    // we reversed entirely through an expression
                                    if (firstStatus == CallStatus.None)
                                    {
                                        firstStatus = CallStatus.VariableRef;
                                        //results.AddRange(dummyList);
                                    }
                                    if (currStatus == CallStatus.None ||
                                       currStatus == CallStatus.Comma)
                                    {
                                        currStatus = CallStatus.VariableRef;
                                    }
                                }
                                else
                                {
                                    results.Clear();
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        // we reversed entirely through an expression
                        if (firstStatus == CallStatus.None)
                        {
                            firstStatus = CallStatus.Expression;
                            results.AddRange(dummyList);
                        }
                        if (currStatus == CallStatus.None ||
                           currStatus == CallStatus.ReturningKeyword)
                        {
                            currStatus = CallStatus.Expression;
                        }
                    }


                    // then try a variable reference
                }
            }
            return false;
        }

        private bool TryMemberAccess(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results)
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
                    // we're trying to access a member...get the member
                    List<MemberResult> dummyList;
                    int currIndex = tokInfo.SourceSpan.Start.Index;
                    int newIndex;
                    if (!TryVariableReference(currIndex, revTokenizer, out dummyList, out newIndex))
                    {
                        if (newIndex < currIndex)
                        {
                            StringBuilder sb = new StringBuilder();
                            // we're not within a variable reference, but we reversed through one
                            while (tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.NLToken ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.SourceSpan.Start.Index > newIndex)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    results.Clear();
                                    return false;
                                }
                                tokInfo = enumerator.Current;
                                sb.Insert(0, tokInfo.Token.Value.ToString());
                            }

                            // now we need to analyze the variable reference to get its members
                            string var = sb.ToString();

                            var analysisRes = GetValueByIndex(var, index, _functionProvider, _databaseProvider);
                            var members = analysisRes.GetMembers(this);
                            if(members != null)
                                results.AddRange(members);

                            return true;
                        }
                    }
                }

                return false;
            }

            return false;
        }

        private enum ExpressionState
        {
            None,
            LeftParen,
            RightParen,
            Operator,
            Operand     // an operand can be a literal, a variable (or constant) reference, or a function call
        }

        private bool TryExpression(int index, IReverseTokenizer revTokenizer, int cursorIndex, out List<MemberResult> results, out int startIndex, TokenKind[] bannedOperators = null)
        {
            startIndex = index;
            results = new List<MemberResult>();
            ExpressionState firstState = ExpressionState.None;
            ExpressionState currState = ExpressionState.None;
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

                if (tokInfo.Token.Kind == TokenKind.ModKeyword ||
                    tokInfo.Token.Kind == TokenKind.UsingKeyword ||
                    tokInfo.Token.Kind == TokenKind.Comma ||            // TODO: if we add a function context detection this should be removed
                  ((int)tokInfo.Token.Kind >= (int)TokenKind.FirstOperator &&
                  (int)tokInfo.Token.Kind <= (int)TokenKind.LastOperator))
                {
                    if (firstState == ExpressionState.None)
                    {
                        firstState = ExpressionState.Operator;
                        // provide valid variables, constants, and functions
                        results.AddRange(GetDefinedMembers(index, true, true, false, true));
                    }

                    if (currState == ExpressionState.None ||
                       currState == ExpressionState.Operand ||
                       currState == ExpressionState.LeftParen)
                    {
                        if (bannedOperators != null && bannedOperators.Contains(tokInfo.Token.Kind))
                        {
                            startIndex = tokInfo.SourceSpan.End.Index + 1;
                            if (firstState != ExpressionState.LeftParen &&
                                firstState != ExpressionState.Operator)
                            {
                                results.Clear();
                                return false;
                            }
                            return true;
                        }

                        currState = ExpressionState.Operator;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.RightParenthesis)
                {
                    // TODO: check to see if it's a function call
                    // if it is, we advance (reverse) past it and call it an operand

                    // if it's not, there should be a grouping within the expression (if it's valid)
                    if (firstState == ExpressionState.None)
                    {
                        firstState = ExpressionState.RightParen;
                    }
                    if (currState == ExpressionState.None ||
                       currState == ExpressionState.Operator ||
                       currState == ExpressionState.RightParen)
                    {
                        currState = ExpressionState.RightParen;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.LeftParenthesis)
                {
                    if (firstState == ExpressionState.None)
                    {
                        firstState = ExpressionState.LeftParen;
                        // provide variables, constants, and functions
                        results.AddRange(GetDefinedMembers(index, true, true, false, true));
                    }
                    if (currState == ExpressionState.None ||
                       currState == ExpressionState.Operand ||
                       currState == ExpressionState.LeftParen ||
                       currState == ExpressionState.RightParen)     // TODO: eventually we'll probably need to handle detecting a function call. For now, we'll allow this state transition
                    {
                        currState = ExpressionState.LeftParen;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else
                {
                    // function call is already covered in the RightParen case for that operand
                    // So all we have left is variable and constant reference, and literals, the first two of which can be handled in the same way
                    List<MemberResult> dummyList;
                    int currIndex = tokInfo.SourceSpan.Start.Index + 1;
                    int newIndex;
                    if (!TryVariableReference(currIndex, revTokenizer, out dummyList, out newIndex))
                    {
                        if (newIndex < currIndex)
                        {
                            // we're not within a variable reference, but we reversed through one
                            while (tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.NLToken ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.SourceSpan.Start.Index > newIndex)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    results.Clear();
                                    return false;
                                }
                                tokInfo = enumerator.Current;
                            }

                            if (firstState == ExpressionState.None)
                            {
                                firstState = ExpressionState.Operand;
                            }
                            currState = ExpressionState.Operand;
                        }
                        else
                        {
                            // check for literals
                            if (tokInfo.Category == TokenCategory.NumericLiteral ||
                               tokInfo.Category == TokenCategory.CharacterLiteral)
                            {
                                if (firstState == ExpressionState.None)
                                {
                                    firstState = ExpressionState.Operand;
                                }
                                currState = ExpressionState.Operand;
                                if (tokInfo.SourceSpan.End.Index == cursorIndex)
                                {
                                    results.Clear();
                                    return true;
                                }
                            }
                            else if (tokInfo.Category == TokenCategory.StringLiteral ||
                                    tokInfo.Category == TokenCategory.IncompleteMultiLineStringLiteral)
                            {
                                if (firstState == ExpressionState.None)
                                {
                                    firstState = ExpressionState.Operand;
                                }

                                if (!enumerator.MoveNext())
                                {
                                    results.Clear();
                                    return false;
                                }
                                skipGettingNext = true;
                                TokenInfo backupToken = tokInfo;    // store the current (actual previous) token
                                tokInfo = enumerator.Current;       // get the next (actually current)

                                // try to collect the whole multi-line string
                                while (tokInfo.Equals(default(TokenInfo)) ||
                                        tokInfo.Token.Kind == TokenKind.NewLine ||
                                        tokInfo.Token.Kind == TokenKind.NLToken ||
                                        tokInfo.Token.Kind == TokenKind.Comment ||
                                        tokInfo.Category == TokenCategory.IncompleteMultiLineStringLiteral)
                                {
                                    if (!enumerator.MoveNext())
                                    {
                                        results.Clear();
                                        return false;
                                    }
                                    backupToken = tokInfo;
                                    tokInfo = enumerator.Current;
                                }

                                tokInfo = backupToken;  // restore the previous token

                                currState = ExpressionState.Operand;
                            }
                        }
                    }
                    else
                    {
                        // we're actually within the variable reference, and should not be returning anything related to the let statement
                        results.Clear();
                        return false;
                    }
                }

                // down here, we check for the operand token
                if (currState == ExpressionState.Operand)
                {
                    // we need to peek backward and see if there's another operand behind us (or a left paren). If not, then the expression is complete
                    TokenInfo dummyToken;
                    if (!skipGettingNext)
                    {
                        if (!enumerator.MoveNext())
                        {
                            results.Clear();
                            return false;
                        }
                        dummyToken = enumerator.Current;
                        while (dummyToken.Equals(default(TokenInfo)) ||
                                dummyToken.Token.Kind == TokenKind.NewLine ||
                                dummyToken.Token.Kind == TokenKind.Comment ||
                                dummyToken.Token.Kind == TokenKind.NLToken)
                        {
                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }
                            dummyToken = enumerator.Current;
                        }
                        dummyToken = enumerator.Current;
                    }
                    else
                    {
                        skipGettingNext = false;
                    }
                    dummyToken = enumerator.Current;

                    if ((!(bannedOperators != null && bannedOperators.Contains(dummyToken.Token.Kind)) &&   // check to see if the operator is banned
                        ((int)dummyToken.Token.Kind >= (int)TokenKind.FirstOperator && (int)dummyToken.Token.Kind <= (int)TokenKind.LastOperator)) ||
                        dummyToken.Token.Kind == TokenKind.LeftParenthesis ||
                        dummyToken.Token.Kind == TokenKind.Comma ||
                        dummyToken.Token.Kind == TokenKind.ModKeyword ||
                        dummyToken.Token.Kind == TokenKind.UsingKeyword)    // Looks like we'll just have to maintain the keywords that are used as operators here
                    {
                        tokInfo = dummyToken;
                        skipGettingNext = true;
                    }
                    else
                    {
                        // we're at the end of the expression. Now let's see if we reversed through the entire expression
                        startIndex = tokInfo.SourceSpan.Start.Index;
                        if (firstState == ExpressionState.Operand ||
                           firstState == ExpressionState.RightParen)
                        {
                            results.Clear();
                            return false;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private enum VariableReferenceState
        {
            None,
            KeywordIdent,
            Dot,
            Star,
            RightBracket,
            LeftBracket,
            FunctionCall,
            Expression
        }

        private bool TryVariableReference(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results, out int startIndex)
        {
            startIndex = index;
            results = new List<MemberResult>();
            VariableReferenceState firstState = VariableReferenceState.None;
            VariableReferenceState currState = VariableReferenceState.None;
            bool skipGetNext = false;
            var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
            while (true)
            {
                if (!skipGetNext)
                {
                    if (!enumerator.MoveNext())
                    {
                        results.Clear();
                        return false;
                    }
                }
                else
                {
                    skipGetNext = false;
                }

                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if (tokInfo.Token.Kind == TokenKind.Dot)
                {
                    if (firstState == VariableReferenceState.None)
                    {
                        firstState = VariableReferenceState.Dot;
                        // TODO: provide members of the resulting variable
                    }
                    if (currState == VariableReferenceState.None ||
                       currState == VariableReferenceState.KeywordIdent ||
                       currState == VariableReferenceState.Star)
                    {
                        currState = VariableReferenceState.Dot;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.Multiply)
                {
                    if (firstState == VariableReferenceState.None)
                    {
                        firstState = VariableReferenceState.Star;
                    }
                    if (currState == VariableReferenceState.None)
                    {
                        currState = VariableReferenceState.Star;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.LeftBracket)
                {
                    if (firstState == VariableReferenceState.None)
                    {
                        firstState = VariableReferenceState.LeftBracket;
                        // provide variables, constants, and functions
                        results.AddRange(GetDefinedMembers(index, true, true, false, true));
                    }
                    if (currState == VariableReferenceState.None ||
                        currState == VariableReferenceState.Expression)
                    {
                        currState = VariableReferenceState.LeftBracket;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.RightBracket)
                {
                    if (firstState == VariableReferenceState.None)
                    {
                        firstState = VariableReferenceState.RightBracket;
                    }
                    if (currState != VariableReferenceState.None &&
                       currState != VariableReferenceState.Dot)
                    {
                        results.Clear();
                        return false;
                    }

                    // now try to reverse parse an expression that defines the array index. If it fails, then we fail
                    List<MemberResult> dummyList;
                    int currIndex = tokInfo.SourceSpan.Start.Index;
                    int exprStartIndex;
                    bool exprSuccess = TryExpression(currIndex, revTokenizer, index, out dummyList, out exprStartIndex);
                    if (!exprSuccess)
                    {
                        if (exprStartIndex < currIndex)
                        {
                            // this means we're not positioned within the expression, but we reversed all the way through one.
                            while (tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.NLToken ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.SourceSpan.Start.Index > exprStartIndex)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    results.Clear();
                                    return false;
                                }
                                tokInfo = enumerator.Current;
                            }
                            // advance to the next token, which should be a left bracket
                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }
                            skipGetNext = true;
                            tokInfo = enumerator.Current;
                            if (tokInfo.Token.Kind != TokenKind.LeftBracket)
                            {
                                results.Clear();
                                return false;
                            }

                            currState = VariableReferenceState.Expression;
                        }
                        else
                        {
                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }

                            tokInfo = enumerator.Current;
                            if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                                continue;   // linebreak

                            if (tokInfo.Category != TokenCategory.NumericLiteral)
                            {
                                results.Clear();
                                return false;
                            }

                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }
                            skipGetNext = true;
                            tokInfo = enumerator.Current;
                            if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                                continue;   // linebreak

                            if (tokInfo.Token.Kind != TokenKind.LeftBracket)
                            {
                                results.Clear();
                                return false;
                            }

                            currState = VariableReferenceState.Expression;
                        }
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Category == TokenCategory.Keyword || tokInfo.Category == TokenCategory.Identifier)
                {
                    if (firstState == VariableReferenceState.None)
                    {
                        firstState = VariableReferenceState.KeywordIdent;
                    }

                    // check to see if the previous token is a dot
                    skipGetNext = true;
                    TokenInfo dummyInfo = tokInfo;
                    do
                    {
                        if (!enumerator.MoveNext())
                        {
                            results.Clear();
                            return false;
                        }
                        tokInfo = enumerator.Current;
                    }
                    while (tokInfo.Equals(default(TokenInfo)) ||
                            tokInfo.Token.Kind == TokenKind.NewLine ||
                            tokInfo.Token.Kind == TokenKind.NLToken ||
                            tokInfo.Token.Kind == TokenKind.Comment);

                    if (tokInfo.Token.Kind != TokenKind.Dot)
                    {
                        tokInfo = dummyInfo;
                        if (currState == VariableReferenceState.None ||
                           currState == VariableReferenceState.LeftBracket ||
                           currState == VariableReferenceState.Dot)
                        {
                            startIndex = tokInfo.SourceSpan.Start.Index;
                            // we're in the correct state. Now check to see if the first state indicates that we actually reverse parsed a complete variable reference
                            if (firstState == VariableReferenceState.Star ||
                               firstState == VariableReferenceState.RightBracket ||
                               firstState == VariableReferenceState.KeywordIdent)
                            {
                                // we reverse parsed right through a variable reference
                                results.Clear();
                                return false;
                            }
                            else
                            {
                                return true;    // we're within an incomplete variable reference!
                            }
                        }
                        else
                        {
                            results.Clear();
                            return false;
                        }
                    }
                    else
                    {
                        currState = VariableReferenceState.KeywordIdent;
                    }
                }
                else
                {
                    results.Clear();
                    return false;
                }
            }

            return false;
        }

        private bool TryLiteralMacro(int index, IReverseTokenizer revTokenizer, out bool isWithinIncompleteMacro, out int startIndex)
        {
            startIndex = index;
            isWithinIncompleteMacro = false;
            LiteralMacroStatus firstStatus = LiteralMacroStatus.None;
            LiteralMacroStatus currStatus = LiteralMacroStatus.None;
            foreach (var tokInfo in revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index))
            {
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if (tokInfo.Token.Kind == TokenKind.MdyKeyword ||
                   tokInfo.Token.Kind == TokenKind.DatetimeKeyword ||
                   tokInfo.Token.Kind == TokenKind.IntervalKeyword)
                {
                    if (currStatus != LiteralMacroStatus.LeftParen)
                    {
                        return false;
                    }
                    if (firstStatus != LiteralMacroStatus.RightParen && firstStatus != LiteralMacroStatus.None)
                    {
                        isWithinIncompleteMacro = true;
                    }
                    startIndex = tokInfo.SourceSpan.Start.Index;
                    return true;
                }
                else if (tokInfo.Token.Kind == TokenKind.LeftParenthesis)
                {
                    if (firstStatus == LiteralMacroStatus.None)
                        firstStatus = LiteralMacroStatus.LeftParen;
                    if (currStatus == LiteralMacroStatus.None ||
                       currStatus == LiteralMacroStatus.RightParen)
                    {
                        currStatus = LiteralMacroStatus.LeftParen;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.RightParenthesis)
                {
                    if (firstStatus == LiteralMacroStatus.None)
                        firstStatus = LiteralMacroStatus.RightParen;
                    if (currStatus == LiteralMacroStatus.None)
                    {
                        currStatus = LiteralMacroStatus.RightParen;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (tokInfo.Category == TokenCategory.Identifier || tokInfo.Category == TokenCategory.Keyword)
                {
                    return false;
                }
            }

            return false;
        }

        private bool TryConstantContext(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results)
        {
            results = new List<MemberResult>();
            ConstantDefStatus currStatus = ConstantDefStatus.None;
            ConstantDefStatus firstStatus = ConstantDefStatus.None;
            List<TokenWithSpan> tokensCollected = new List<TokenWithSpan>();
            var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    results.Clear();
                    return false;
                }

                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if (tokInfo.Token.Kind == TokenKind.ConstantKeyword)
                {
                    if (currStatus != ConstantDefStatus.IdentOrKeyword &&
                       currStatus != ConstantDefStatus.None)
                    {
                        results.Clear();
                        return false;
                    }

                    // parse out the constant definition to see if we're actually within it
                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    tokensCollected.Insert(0, default(TokenWithSpan));
                    TokenListParser parser = new TokenListParser(tokensCollected);
                    ConstantDefNode defNode;
                    bool dummy;
                    if (ConstantDefNode.TryParseNode(parser, out defNode, out dummy) && (parser.ErrorSink as StubErrorSink).ErrorCount == 0)
                    {
                        if (firstStatus != ConstantDefStatus.None &&
                           firstStatus != ConstantDefStatus.IdentOrKeyword &&
                           firstStatus != ConstantDefStatus.Equals &&
                           firstStatus != ConstantDefStatus.Comma)
                        {
                            return false;
                        }
                    }

                    return true;
                }
                else if (tokInfo.Token.Kind == TokenKind.Comma)
                {
                    if (firstStatus == ConstantDefStatus.None)
                        firstStatus = ConstantDefStatus.Comma;
                    if (currStatus == ConstantDefStatus.None ||
                       currStatus == ConstantDefStatus.IdentOrKeyword)
                    {
                        currStatus = ConstantDefStatus.Comma;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.Equals)
                {
                    if (firstStatus == ConstantDefStatus.None)
                        firstStatus = ConstantDefStatus.Equals;
                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    if (currStatus == ConstantDefStatus.None)
                    {
                        TokenKind[] kinds = new TokenKind[] { TokenKind.MdyKeyword, TokenKind.DatetimeKeyword, TokenKind.IntervalKeyword };
                        results.AddRange(kinds.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
                        results.AddRange(SystemConstants.Select(x => new MemberResult(x.Key, x.Value, GeneroMemberType.Keyword, this)));
                        currStatus = ConstantDefStatus.Equals;
                    }
                    else if (currStatus == ConstantDefStatus.LiteralMacro ||
                             currStatus == ConstantDefStatus.Literal ||
                             currStatus == ConstantDefStatus.TypeConstraint)
                    {
                        currStatus = ConstantDefStatus.Equals;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Category == TokenCategory.CharacterLiteral ||
                        tokInfo.Category == TokenCategory.NumericLiteral ||
                        tokInfo.Category == TokenCategory.StringLiteral ||
                        SystemConstants.ContainsKey(tokInfo.Token.Value.ToString()))
                {
                    if (firstStatus == ConstantDefStatus.None)
                        firstStatus = ConstantDefStatus.Literal;
                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    if (currStatus == ConstantDefStatus.None ||
                       currStatus == ConstantDefStatus.Comma)
                    {
                        currStatus = ConstantDefStatus.Literal;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Category == TokenCategory.IncompleteMultiLineStringLiteral)
                {
                    if (firstStatus == ConstantDefStatus.None)
                        firstStatus = ConstantDefStatus.Literal;
                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    if (currStatus == ConstantDefStatus.None ||
                       currStatus == ConstantDefStatus.Comma ||
                        currStatus == ConstantDefStatus.Literal)
                    {
                        currStatus = ConstantDefStatus.Literal;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else if (tokInfo.Token.Kind == TokenKind.RightParenthesis)
                {
                    bool dummyIsWithin;
                    int startIndex;
                    if (TryLiteralMacro((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyIsWithin, out startIndex))
                    {
                        if (firstStatus == ConstantDefStatus.None)
                            firstStatus = ConstantDefStatus.LiteralMacro;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                        while (tokInfo.Equals(default(TokenInfo)) ||
                                tokInfo.Token.Kind == TokenKind.NewLine ||
                                tokInfo.Token.Kind == TokenKind.NLToken ||
                                tokInfo.Token.Kind == TokenKind.Comment ||
                                tokInfo.SourceSpan.Start.Index > startIndex)
                        {
                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }
                            tokInfo = enumerator.Current;
                            if (!(tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.NLToken ||
                                    tokInfo.Token.Kind == TokenKind.Comment))
                            {
                                tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                            }
                        }

                        if (currStatus == ConstantDefStatus.None)
                        {
                            TokenKind[] kinds = new TokenKind[] { TokenKind.DatetimeKeyword, TokenKind.IntervalKeyword };
                            results.AddRange(kinds.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
                            currStatus = ConstantDefStatus.LiteralMacro;
                        }
                        else if (currStatus == ConstantDefStatus.TypeConstraint ||
                                currStatus == ConstantDefStatus.Comma)
                        {
                            currStatus = ConstantDefStatus.LiteralMacro;
                        }
                        else
                        {
                            results.Clear();
                            return false;
                        }
                    }
                    else
                    {
                        List<MemberResult> dummyResults;
                        if (TryTypeConstraintContext((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyResults, out startIndex))
                        {
                            if (firstStatus == ConstantDefStatus.None)
                                firstStatus = ConstantDefStatus.TypeConstraint;
                            tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                            // move us past the type constraint
                            while (tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.NLToken ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.SourceSpan.Start.Index > startIndex)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    results.Clear();
                                    return false;
                                }
                                tokInfo = enumerator.Current;
                                if (!(tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.Token.Kind == TokenKind.NLToken))
                                {
                                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                                }
                            }

                            if (currStatus == ConstantDefStatus.None)
                            {
                                results.AddRange(dummyResults);
                                currStatus = ConstantDefStatus.TypeConstraint;
                            }
                            else if (currStatus == ConstantDefStatus.Equals ||
                                    currStatus == ConstantDefStatus.Comma)
                            {
                                currStatus = ConstantDefStatus.TypeConstraint;
                            }
                            else
                            {
                                results.Clear();
                                return false;
                            }
                        }
                        else
                        {
                            results.Clear();
                            return false;
                        }
                    }
                }
                else
                {
                    List<MemberResult> dummyResults;
                    int startIndex;
                    if (TryTypeConstraintContext((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyResults, out startIndex))
                    {
                        if (firstStatus == ConstantDefStatus.None)
                            firstStatus = ConstantDefStatus.TypeConstraint;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                        // move us past the type constraint
                        while (tokInfo.Equals(default(TokenInfo)) ||
                                tokInfo.Token.Kind == TokenKind.NewLine ||
                                tokInfo.Token.Kind == TokenKind.NLToken ||
                                tokInfo.Token.Kind == TokenKind.Comment ||
                                tokInfo.SourceSpan.Start.Index > startIndex)
                        {
                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }
                            tokInfo = enumerator.Current;
                            if (!(tokInfo.Equals(default(TokenInfo)) ||
                                tokInfo.Token.Kind == TokenKind.NewLine ||
                                tokInfo.Token.Kind == TokenKind.Comment ||
                                tokInfo.Token.Kind == TokenKind.NLToken))
                            {
                                tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                            }
                        }

                        if (currStatus == ConstantDefStatus.None)
                        {
                            results.AddRange(dummyResults);
                            currStatus = ConstantDefStatus.TypeConstraint;
                        }
                        else if (currStatus == ConstantDefStatus.Equals ||
                                currStatus == ConstantDefStatus.Comma)
                        {
                            currStatus = ConstantDefStatus.TypeConstraint;
                        }
                        else
                        {
                            results.Clear();
                            return false;
                        }
                    }
                    else if (BuiltinTypes.Where(x => x != TokenKind.TextKeyword && x != TokenKind.ByteKeyword)
                                        .Contains(tokInfo.Token.Kind))
                    {
                        if (firstStatus == ConstantDefStatus.None)
                            firstStatus = ConstantDefStatus.BuiltinType;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                        if (currStatus == ConstantDefStatus.None ||
                           currStatus == ConstantDefStatus.Equals)
                        {
                            currStatus = ConstantDefStatus.BuiltinType;
                        }
                        else
                        {
                            results.Clear();
                            return false;
                        }
                    }
                    else if (tokInfo.Category == TokenCategory.Identifier || tokInfo.Category == TokenCategory.Keyword)
                    {
                        if (firstStatus == ConstantDefStatus.None)
                            firstStatus = ConstantDefStatus.IdentOrKeyword;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());

                        if (currStatus == ConstantDefStatus.None)
                        {
                            currStatus = ConstantDefStatus.IdentOrKeyword;
                            // insert built-in types to the completions (excluding text and byte)
                            results.AddRange(BuiltinTypes.Where(x => x != TokenKind.ByteKeyword && x != TokenKind.TextKeyword)
                                                         .Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
                        }
                        else if (currStatus == ConstantDefStatus.BuiltinType ||
                               currStatus == ConstantDefStatus.Equals ||
                               currStatus == ConstantDefStatus.TypeConstraint)
                        {
                            currStatus = ConstantDefStatus.IdentOrKeyword;
                        }
                        else
                        {
                            results.Clear();
                            return false;
                        }
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
            }

            return false;
        }

        private enum ImportContextState
        {
            None,
            FglKeyword,
            JavaKeyword,
            KeywordOrIdent
        }

        private bool TryImportContext(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results)
        {
            results = new List<MemberResult>();
            ImportContextState firstState = ImportContextState.None;
            var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
            bool skipGettingNext = false;
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
                    skipGettingNext = false;    //reset
                }
                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if(tokInfo.Token.Kind == TokenKind.ImportKeyword)
                {
                    if (firstState == ImportContextState.None)
                    {
                        // provide FGL, JAVA, and the available package keywords
                        TokenKind[] kinds = new TokenKind[] { TokenKind.FglKeyword, TokenKind.JavaKeyword };
                        results.AddRange(kinds.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
                        results.AddRange(Packages.Values.Where(x => x.ExtensionPackage).Select(x => new MemberResult(x.Name, x, GeneroMemberType.Module, this)));
                    }

                    return firstState != ImportContextState.KeywordOrIdent;
                }
                else if(tokInfo.Token.Kind == TokenKind.FglKeyword)
                {
                    if(firstState == ImportContextState.None)
                    {
                        // TODO: provide available fgl imports
                        // Would this just be all importable programs? Yikes...

                        firstState = ImportContextState.FglKeyword;
                    }
                }
                else if(tokInfo.Token.Kind == TokenKind.JavaKeyword)
                {
                    if(firstState == ImportContextState.None)
                    {
                        // TODO: provide java classes?

                        firstState = ImportContextState.JavaKeyword;
                    }
                }
                else if(tokInfo.Category == TokenCategory.Keyword ||
                        tokInfo.Category == TokenCategory.Identifier)
                {
                    if(firstState == ImportContextState.None)
                    {
                        firstState = ImportContextState.KeywordOrIdent;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private bool TryTypeConstraintContext(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results, out int startingIndex)
        {
            bool completionsSupplied = false;
            results = new List<MemberResult>();
            IList<CompletionPossibility> possibilities = null;
            List<TokenKindWithConstraint> tokensAwaitingValidation = new List<TokenKindWithConstraint>();
            bool firstPreviousToken = true;
            bool isFirstToken = true;
            startingIndex = index;

            var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
            bool skipGettingNext = false;
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
                    skipGettingNext = false;    //reset
                }
                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if (tokInfo.Token.Kind == TokenKind.RightParenthesis)
                {
                    bool dummyIsWithin;
                    int startIndex;
                    if (TryLiteralMacro((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyIsWithin, out startIndex))
                    {
                        while (tokInfo.Equals(default(TokenInfo)) ||
                                tokInfo.Token.Kind == TokenKind.NewLine ||
                                tokInfo.Token.Kind == TokenKind.NLToken ||
                                tokInfo.Token.Kind == TokenKind.Comment ||
                                tokInfo.SourceSpan.Start.Index > startIndex)
                        {
                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }
                            tokInfo = enumerator.Current;
                        }
                        skipGettingNext = true;
                        possibilities = null;
                        continue;
                    }
                }

                if (possibilities != null)
                {
                    // we have a set of possiblities from the token before. Let's try to match something out of it
                    var entry = possibilities.FirstOrDefault(x =>
                    {
                        if (x is TokenKindCompletionPossiblity)
                            return (x as TokenKindCompletionPossiblity).MultipleKinds.Contains(tokInfo.Token.Kind);
                        else
                            return (x as CategoryCompletionPossiblity).Categories.Contains(tokInfo.Category);
                    });
                    if (entry == null)
                    {
                        results.Clear();
                        return false;
                    }

                    // we only want to supply completions from the position we're in, based on the previous token.
                    if (!completionsSupplied)
                    {
                        // ok, we have an entry with zero or more completion members
                        foreach (var poss in entry.KeywordCompletions)
                        {
                            if (poss.TokenKindToCheck != TokenKind.EndOfFile && poss.TokensPreviousToCheck > 0)
                                tokensAwaitingValidation.Add(poss);
                            else
                                results.Add(new MemberResult(Tokens.TokenKinds[poss.Kind], GeneroMemberType.Keyword, this));
                        }

                        // check to see if additional completions can be added via provider delegate
                        if (entry.AdditionalCompletionsProvider != null)
                        {
                            results.AddRange(entry.AdditionalCompletionsProvider(index, tokInfo.Token.Value.ToString()));
                        }
                        completionsSupplied = true;
                    }

                    // go through the tokens that are awaiting validation before being added to the result set
                    for (int i = tokensAwaitingValidation.Count - 1; i >= 0; i--)
                    {
                        tokensAwaitingValidation[i].DecrementPreviousToCheck();
                        if (tokensAwaitingValidation[i].TokensPreviousToCheck == 0)
                        {
                            // check for the token kind we're supposed to check for
                            if (tokInfo.Token.Kind == tokensAwaitingValidation[i].TokenKindToCheck)
                            {
                                results.Add(new MemberResult(Tokens.TokenKinds[tokensAwaitingValidation[i].Kind], GeneroMemberType.Keyword, this));
                            }
                            // and remove the token
                            tokensAwaitingValidation.RemoveAt(i);
                        }
                    }

                    if (firstPreviousToken)
                    {
                        firstPreviousToken = false;
                        if (entry.IsBreakingStateOnFirstPrevious)
                        {
                            results.Clear();
                            return false;
                        }
                    }
                }

                if (_typeConstraintsMap.StatementStartTokens.Contains(tokInfo.Token.Kind))
                {
                    if (isFirstToken)
                    {
                        IList<TokenKind> stmtStartCompletions;
                        if (_typeConstraintsMap.CompletionsForStartToken.TryGetValue(tokInfo.Token.Kind, out stmtStartCompletions))
                        {
                            foreach (var tok in stmtStartCompletions)
                                results.Add(new MemberResult(Tokens.TokenKinds[tok], GeneroMemberType.Keyword, this));
                        }
                    }
                    startingIndex = tokInfo.SourceSpan.Start.Index;
                    return true;
                }

                if (_typeConstraintsMap.Map.TryGetValue(tokInfo.Token.Kind, out possibilities))
                {
                }
                else if (_typeConstraintsMap.Map.TryGetValue(tokInfo.Category, out possibilities))
                {
                }
                else
                {
                    // no matches found, not in a valid define statement
                    results.Clear();
                    return false;
                }
                isFirstToken = false;
            }

            results.Clear();
            return false;
        }

        // The logic in here isn't exactly pretty, but it seems to work for most cases. It would probably be good to define a formal set of unit
        // tests to exercise this better. Still need to account for types that have constraints (e.g. datetime ___ TO ____).
        private bool TryDefineDefContext(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results, List<TokenKind> failingKeywords = null)
        {
            bool completionsSupplied = false;
            results = new List<MemberResult>();
            List<TokenWithSpan> tokensCollected = new List<TokenWithSpan>();
            IList<CompletionPossibility> possibilities = null;
            List<TokenKindWithConstraint> tokensAwaitingValidation = new List<TokenKindWithConstraint>();
            int maxKeywordsOrIdentsBackToBack = 2;
            int currKeywordsOrIdentsBackToBack = 0;
            bool firstPreviousToken = true;
            bool isFirstToken = true;
            bool skipMovingNext = false;
            int nestedRecordDefCount = 0;
            var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
            bool? isFirstTokenHitRecord = null;
            while (true)
            {
                if (!skipMovingNext)
                {
                    if (!enumerator.MoveNext())
                    {
                        results.Clear();
                        return false;
                    }
                }
                else
                {
                    skipMovingNext = false; // reset
                }
                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if (failingKeywords != null && failingKeywords.Contains(tokInfo.Token.Kind))
                {
                    results.Clear();
                    return false;
                }

                if (tokInfo.Token.Kind == TokenKind.LeftParenthesis ||
                    tokInfo.Token.Kind == TokenKind.RightParenthesis)
                {
                    // TODO: try a define attribute context
                    List<MemberResult> dummyList2;
                    int startIndex;
                    bool isAttribute = TryDefineAttributeContext((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyList2, out startIndex);
                    if (tokInfo.SourceSpan.Start.Index > startIndex)
                    {
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                        // move us past the type constraint
                        while (tokInfo.Equals(default(TokenInfo)) ||
                                tokInfo.Token.Kind == TokenKind.NewLine ||
                                tokInfo.Token.Kind == TokenKind.NLToken ||
                                tokInfo.Token.Kind == TokenKind.Comment ||
                                tokInfo.SourceSpan.Start.Index > startIndex)
                        {
                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }
                            tokInfo = enumerator.Current;
                            if (!(tokInfo.Equals(default(TokenInfo)) ||
                                tokInfo.Token.Kind == TokenKind.NewLine ||
                                tokInfo.Token.Kind == TokenKind.Comment ||
                                tokInfo.Token.Kind == TokenKind.NLToken))
                            {
                                tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                            }
                        }

                        if (isAttribute)
                        {
                            results.Clear();
                            results.AddRange(dummyList2);
                            return true;
                        }
                        continue;
                    }
                }

                if (possibilities != null)
                {
                    // we have a set of possiblities from the token before. Let's try to match something out of it
                    var entry = possibilities.FirstOrDefault(x =>
                    {
                        if (x is TokenKindCompletionPossiblity)
                            return (x as TokenKindCompletionPossiblity).MultipleKinds.Contains(tokInfo.Token.Kind);
                        else
                            return (x as CategoryCompletionPossiblity).Categories.Contains(tokInfo.Category);
                    });
                    if (entry == null)
                    {
                        List<MemberResult> dummyList2;
                        List<TokenWithSpan> tokenList;
                        int typeConstraintStartingIndex2;
                        if (TryTypeConstraintContext((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyList2, out typeConstraintStartingIndex2))
                        {
                            tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                            // move us past the type constraint
                            while (tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.NLToken ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.SourceSpan.Start.Index > typeConstraintStartingIndex2)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    results.Clear();
                                    return false;
                                }
                                tokInfo = enumerator.Current;
                                if (!(tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.Token.Kind == TokenKind.NLToken))
                                {
                                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                                }
                            }
                            continue;
                        }

                        results.Clear();
                        return false;
                    }

                    // we only want to supply completions from the position we're in, based on the previous token.
                    if (!completionsSupplied)
                    {
                        // ok, we have an entry with zero or more completion members
                        foreach (var poss in entry.KeywordCompletions)
                        {
                            if (poss.TokenKindToCheck != TokenKind.EndOfFile && poss.TokensPreviousToCheck > 0)
                                tokensAwaitingValidation.Add(poss);
                            else
                                results.Add(new MemberResult(Tokens.TokenKinds[poss.Kind], GeneroMemberType.Keyword, this));
                        }

                        // check to see if additional completions can be added via provider delegate
                        if (entry.AdditionalCompletionsProvider != null)
                        {
                            results.AddRange(entry.AdditionalCompletionsProvider(index, tokInfo.Token.Value.ToString()));
                        }
                        completionsSupplied = true;
                    }

                    // go through the tokens that are awaiting validation before being added to the result set
                    for (int i = tokensAwaitingValidation.Count - 1; i >= 0; i--)
                    {
                        tokensAwaitingValidation[i].DecrementPreviousToCheck();
                        if (tokensAwaitingValidation[i].TokensPreviousToCheck == 0)
                        {
                            // check for the token kind we're supposed to check for
                            if (tokInfo.Token.Kind == tokensAwaitingValidation[i].TokenKindToCheck)
                            {
                                results.Add(new MemberResult(Tokens.TokenKinds[tokensAwaitingValidation[i].Kind], GeneroMemberType.Keyword, this));
                            }
                            // and remove the token
                            tokensAwaitingValidation.RemoveAt(i);
                        }
                    }

                    if (firstPreviousToken)
                    {
                        firstPreviousToken = false;
                        if (entry.IsBreakingStateOnFirstPrevious)
                        {
                            results.Clear();
                            return false;
                        }
                    }
                }

                // handle the Define token
                if (_defineStatementMap.StatementStartTokens.Contains(tokInfo.Token.Kind))
                {
                    if (tokensCollected.Count > 0)
                    {
                        if (tokensCollected.Last().Token.Kind == TokenKind.Comma)
                        {
                            results.Clear();
                            return true;
                        }
                        else if (tokensCollected.Last().Token.Kind == TokenKind.RecordKeyword &&
                                tokensCollected.ElementAt(tokensCollected.Count - 2).Token.Kind != TokenKind.EndKeyword)
                        {
                            return true;
                        }
                    }
                    if (isFirstToken)
                    {
                        results.Clear();
                        return true;
                    }
                    //if (/*(currKeywordsOrIdentsBackToBack >= maxKeywordsOrIdentsBackToBack) ||*/
                    //    (isFirstTokenHitRecord.HasValue && isFirstTokenHitRecord.Value))
                    //{
                    //    results.Clear();
                    //    return false;
                    //}
                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    tokensCollected.Insert(0, default(TokenWithSpan));
                    // Now Forward parse the collected tokens and see if the define statement is complete.
                    // If so, we're not actually IN the statement
                    TokenListParser parser = new TokenListParser(tokensCollected);
                    DefineNode defNode;
                    bool dummy;
                    if (DefineNode.TryParseDefine(parser, out defNode, out dummy) && (parser.ErrorSink as StubErrorSink).ErrorCount == 0)
                        return false;
                    TypeDefNode typeDefNode;
                    parser.Reset();
                    if (TypeDefNode.TryParseNode(parser, out typeDefNode, out dummy) && (parser.ErrorSink as StubErrorSink).ErrorCount == 0)
                        return false;

                    return true;
                }

                // try reverse parsing a type constraint
                List<MemberResult> dummyList;
                int typeConstraintStartingIndex;
                if (TryTypeConstraintContext((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyList, out typeConstraintStartingIndex))
                {
                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    // move us past the type constraint
                    while (tokInfo.Equals(default(TokenInfo)) ||
                            tokInfo.Token.Kind == TokenKind.NewLine ||
                            tokInfo.Token.Kind == TokenKind.NLToken ||
                            tokInfo.Token.Kind == TokenKind.Comment ||
                            tokInfo.SourceSpan.Start.Index > typeConstraintStartingIndex)
                    {
                        if (!enumerator.MoveNext())
                        {
                            results.Clear();
                            return false;
                        }
                        tokInfo = enumerator.Current;
                        if (!(tokInfo.Equals(default(TokenInfo)) ||
                            tokInfo.Token.Kind == TokenKind.NewLine ||
                            tokInfo.Token.Kind == TokenKind.Comment ||
                            tokInfo.Token.Kind == TokenKind.NLToken))
                        {
                            tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                        }
                    }
                    possibilities = new List<CompletionPossibility>() 
                    { 
                        new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Identifier, TokenCategory.Keyword }, new List<TokenKindWithConstraint>()) 
                    };
                    continue;
                }
                else if (_defineStatementMap.Map.TryGetValue(tokInfo.Token.Kind, out possibilities))
                {
                    currKeywordsOrIdentsBackToBack = 0;
                    if (tokInfo.Token.Kind == TokenKind.RecordKeyword)
                    {
                        // advance the enumerator to see if the next (previous) token is 'end'
                        enumerator.MoveNext();
                        skipMovingNext = true;
                        var tempToken = enumerator.Current;
                        if (tempToken.Token.Kind != TokenKind.EndKeyword)
                        {
                            nestedRecordDefCount--;
                        }
                        else
                        {
                            if (!isFirstTokenHitRecord.HasValue)
                            {
                                isFirstTokenHitRecord = true;
                            }
                        }
                    }
                    else if (tokInfo.Token.Kind == TokenKind.EndKeyword)
                    {
                        nestedRecordDefCount++;
                    }
                }
                else if (_defineStatementMap.Map.TryGetValue(tokInfo.Category, out possibilities))
                {
                    if (tokInfo.Category == TokenCategory.Keyword || tokInfo.Category == TokenCategory.Identifier)
                    {
                        currKeywordsOrIdentsBackToBack++;
                        if (currKeywordsOrIdentsBackToBack > maxKeywordsOrIdentsBackToBack && nestedRecordDefCount == 0)
                        {
                            results.Clear();
                            return false;
                        }
                    }
                }
                else
                {
                    // no matches found, not in a valid define statement
                    results.Clear();
                    return false;
                }

                if (!isFirstTokenHitRecord.HasValue)
                {
                    isFirstTokenHitRecord = false;
                }
                isFirstToken = false;
                tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
            }

            results.Clear();
            return false;
        }

        private enum DefineAttributeContextState
        {
            None,
            RightParen,
            LeftParen
            // TODO: more to come...
        }

        private bool TryDefineAttributeContext(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results, out int startingIndex)
        {
            results = new List<MemberResult>();
            startingIndex = index;
            DefineAttributeContextState firstState = DefineAttributeContextState.None;
            bool skipMovingNext = false;
            var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
            bool leftParenHit = false;
            while (true)
            {
                if (!skipMovingNext)
                {
                    if (!enumerator.MoveNext())
                    {
                        results.Clear();
                        return false;
                    }
                }
                else
                {
                    skipMovingNext = false; // reset
                }
                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if(tokInfo.Token.Kind == TokenKind.AttributeKeyword)
                {
                    startingIndex = tokInfo.SourceSpan.Start.Index;
                    if (firstState == DefineAttributeContextState.RightParen)
                        return false;
                    if (!leftParenHit)
                        return false;
                    return true;
                }
                else if(tokInfo.Token.Kind == TokenKind.RightParenthesis)
                {
                    if (firstState == DefineAttributeContextState.None)
                        firstState = DefineAttributeContextState.RightParen;
                }
                else if(tokInfo.Token.Kind == TokenKind.LeftParenthesis)
                {
                    if (firstState == DefineAttributeContextState.None)
                        firstState = DefineAttributeContextState.LeftParen;
                    leftParenHit = true;
                }
                else
                {
                    if (leftParenHit)
                        break;
                }
            }

            return false;
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

        private bool TryPreprocessorContext(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results)
        {
            bool isAmpersand = false;
            foreach (var tokInfo in revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index))
            {
                if (tokInfo.Equals(default(TokenInfo)))
                    break;
                if (tokInfo.Token.Kind == TokenKind.Ampersand)
                {
                    isAmpersand = true;
                    break;
                }
                break;
            }

            if (isAmpersand)
            {
                results = new List<MemberResult>()
                {
                    new MemberResult(Tokens.TokenKinds[TokenKind.IncludeKeyword], GeneroMemberType.Keyword, this),
                    new MemberResult(Tokens.TokenKinds[TokenKind.DefineKeyword], GeneroMemberType.Keyword, this),
                    new MemberResult(Tokens.TokenKinds[TokenKind.UndefKeyword], GeneroMemberType.Keyword, this),
                    new MemberResult(Tokens.TokenKinds[TokenKind.IfdefKeyword], GeneroMemberType.Keyword, this),
                    new MemberResult(Tokens.TokenKinds[TokenKind.ElseKeyword], GeneroMemberType.Keyword, this),
                    new MemberResult(Tokens.TokenKinds[TokenKind.EndifKeyword], GeneroMemberType.Keyword, this)
                };
            }
            else
            {
                results = new List<MemberResult>();
            }
            return isAmpersand;
        }

        /// <summary>
        /// Determines whether we're just after an access modifier (private or public)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="revTokenizer"></param>
        /// <returns></returns>
        private bool TryAllowAccessModifiers(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results)
        {
            results = new List<MemberResult>();
            bool isPrevTokenPublicOrPrivate = false;
            foreach (var tokInfo in revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index))
            {
                if (tokInfo.Equals(default(TokenInfo)))
                    continue;
                if (tokInfo.Category == TokenCategory.WhiteSpace)
                    continue;
                if (tokInfo.Token.Kind == TokenKind.PrivateKeyword || tokInfo.Token.Kind == TokenKind.PublicKeyword)
                {
                    isPrevTokenPublicOrPrivate = true;
                    break;
                }
                else
                {
                    break;
                }
            }

            if (!isPrevTokenPublicOrPrivate)
            {
                results.AddRange(new MemberResult[]
                {
                    new MemberResult(Tokens.TokenKinds[TokenKind.PrivateKeyword], GeneroMemberType.Keyword, this),
                    new MemberResult(Tokens.TokenKinds[TokenKind.PublicKeyword], GeneroMemberType.Keyword, this)
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        private enum LetStatementState
        {
            None,
            Let,
            VariableReference,
            Equals,
            Expression
        }

        private bool TryLetStatement(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results, out bool isMemberAccess)
        {
            isMemberAccess = false;
            results = new List<MemberResult>();
            LetStatementState firstState = LetStatementState.None;
            LetStatementState secondState = LetStatementState.None;
            LetStatementState currState = LetStatementState.None;
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
                    skipGettingNext = true;
                }

                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                if (tokInfo.Token.Kind == TokenKind.LetKeyword)
                {
                    if (currState == LetStatementState.None)
                    {
                        results.AddRange(GetDefinedMembers(index, true, false, false, false));
                        return true;
                    }
                    //if(firstState == LetStatementState.Expression ||
                    //   firstState == LetStatementState.VariableReference)
                    else if (firstState != LetStatementState.Expression ||
                             (firstState == LetStatementState.Expression && secondState != LetStatementState.Equals))
                    {
                        // we're in a valid state
                        return true;
                    }
                    else if (results.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
                else if (tokInfo.Token.Kind == TokenKind.Equals)
                {
                    if (firstState == LetStatementState.None)
                    {
                        firstState = LetStatementState.Equals;
                        results.AddRange(GetDefinedMembers(index, true, true, false, true));
                    }
                    else if (secondState == LetStatementState.None)
                    {
                        secondState = LetStatementState.Equals;
                    }

                    if (currState == LetStatementState.None ||
                       currState == LetStatementState.Expression ||
                        currState == LetStatementState.VariableReference)
                    {
                        currState = LetStatementState.Equals;
                    }
                    else
                    {
                        results.Clear();
                        return false;
                    }
                }
                else
                {
                    // 1) see if we're within an expression
                    List<MemberResult> dummyList;
                    int currIndex = tokInfo.SourceSpan.Start.Index + 1;
                    int exprStartIndex;
                    bool exprSuccess = TryExpression(currIndex, revTokenizer, index, out dummyList, out exprStartIndex, new TokenKind[] { TokenKind.Equals });
                    if (!exprSuccess)
                    {
                        if (exprStartIndex < currIndex)
                        {
                            // this means we're not positioned within the expression, but we reversed all the way through one.
                            while (tokInfo.Equals(default(TokenInfo)) ||
                                    tokInfo.Token.Kind == TokenKind.NewLine ||
                                    tokInfo.Token.Kind == TokenKind.NLToken ||
                                    tokInfo.Token.Kind == TokenKind.Comment ||
                                    tokInfo.SourceSpan.Start.Index > exprStartIndex)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    results.Clear();
                                    return false;
                                }
                                tokInfo = enumerator.Current;
                            }

                            if (firstState == LetStatementState.None)
                            {
                                firstState = LetStatementState.Expression;
                            }
                            else if (secondState == LetStatementState.None)
                            {
                                secondState = LetStatementState.Expression;
                            }

                            if (currState == LetStatementState.None ||
                                currState == LetStatementState.Equals)
                            {
                                currState = LetStatementState.Expression;
                            }
                            else
                            {
                                results.Clear();
                                return false;
                            }
                        }
                        else
                        {
                            // 1) See if we're within a variable reference
                            int newIndex;
                            if (!TryVariableReference(currIndex, revTokenizer, out dummyList, out newIndex))
                            {
                                if (newIndex < currIndex)
                                {
                                    // we're not within a variable reference, but we reversed through one
                                    while (tokInfo.Equals(default(TokenInfo)) ||
                                            tokInfo.Token.Kind == TokenKind.NewLine ||
                                            tokInfo.Token.Kind == TokenKind.NLToken ||
                                            tokInfo.Token.Kind == TokenKind.Comment ||
                                            tokInfo.SourceSpan.Start.Index > newIndex)
                                    {
                                        if (!enumerator.MoveNext())
                                        {
                                            results.Clear();
                                            return false;
                                        }
                                        tokInfo = enumerator.Current;
                                    }

                                    if (firstState == LetStatementState.None)
                                    {
                                        firstState = LetStatementState.VariableReference;
                                    }
                                    else if (secondState == LetStatementState.None)
                                    {
                                        secondState = LetStatementState.VariableReference;
                                    }

                                    if (currState == LetStatementState.None ||
                                       currState == LetStatementState.Equals)
                                    {
                                        currState = LetStatementState.VariableReference;
                                    }
                                    else
                                    {
                                        results.Clear();
                                        return false;
                                    }
                                }
                                else
                                {
                                    results.Clear();
                                    return false;
                                }
                            }
                            else
                            {
                                // check for a member access
                                List<MemberResult> memberAccessResults = new List<MemberResult>();
                                if (TryMemberAccess(index, revTokenizer, out memberAccessResults))
                                {
                                    while (tokInfo.Equals(default(TokenInfo)) ||
                                           tokInfo.Token.Kind == TokenKind.NewLine ||
                                           tokInfo.Token.Kind == TokenKind.NLToken ||
                                           tokInfo.Token.Kind == TokenKind.Comment ||
                                           tokInfo.SourceSpan.Start.Index > newIndex)
                                    {
                                        if (!enumerator.MoveNext())
                                        {
                                            results.Clear();
                                            return false;
                                        }
                                        tokInfo = enumerator.Current;
                                    }

                                    if (firstState == LetStatementState.None)
                                    {
                                        firstState = LetStatementState.VariableReference;
                                        results.AddRange(memberAccessResults);
                                        isMemberAccess = true;
                                    }
                                    else if (secondState == LetStatementState.None)
                                    {
                                        secondState = LetStatementState.VariableReference;
                                    }
                                    if (currState == LetStatementState.None ||
                                        currState == LetStatementState.Equals)
                                    {
                                        currState = LetStatementState.VariableReference;
                                    }
                                }
                                else
                                {
                                    // we're actually within the variable reference, and should not be returning anything related to the let statement
                                    // TODO: reinvestigate the comment above...I think it would only apply if we were doing the variable reference check by itself
                                    // (not nested in the let statement detection).
                                    if (dummyList.Count > 0)
                                    {
                                        results.AddRange(dummyList);
                                        return true;
                                    }
                                    else
                                    {
                                        // This may still be valid.
                                        results.Clear();
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        while (tokInfo.Equals(default(TokenInfo)) ||
                                           tokInfo.Token.Kind == TokenKind.NewLine ||
                                           tokInfo.Token.Kind == TokenKind.NLToken ||
                                           tokInfo.Token.Kind == TokenKind.Comment ||
                                           tokInfo.SourceSpan.Start.Index > exprStartIndex)
                        {
                            if (!enumerator.MoveNext())
                            {
                                results.Clear();
                                return false;
                            }
                            tokInfo = enumerator.Current;
                        }

                        results.AddRange(dummyList);
                        if (firstState == LetStatementState.None)
                        {
                            firstState = LetStatementState.Expression;
                        }
                        else if (secondState == LetStatementState.None)
                        {
                            secondState = LetStatementState.Expression;
                        }

                        if (currState == LetStatementState.None ||
                            currState == LetStatementState.Equals)
                        {
                            currState = LetStatementState.Expression;
                        }
                        else
                        {
                            results.Clear();
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Enums

        private enum CallStatus
        {
            None,
            CallKeyword,
            Expression,
            ReturningKeyword,
            VariableRef,
            Comma
        }

        private enum ConstantDefStatus
        {
            None,
            ConstantKeyword,
            Equals,
            BuiltinType,
            TypeConstraint,
            LiteralMacro,
            Literal,
            IdentOrKeyword,
            Comma
        }

        private enum LiteralMacroStatus
        {
            None,
            LeftParen,
            RightParen
        }

        private enum FunctionDefStatus
        {
            None,
            FuncKeyword,
            Comma,
            LParen,
            IdentOrKeyword
        }

        public static TokenKind[] ValidStatementKeywords = new TokenKind[]
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
            TokenKind.SkipKeyword
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

        public IEnumerable<MemberResult> GetContextMembersByIndex(int index, IReverseTokenizer revTokenizer, IFunctionInformationProvider functionProvider,
                                                                  IDatabaseInformationProvider databaseProvider, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            _functionProvider = functionProvider;
            _databaseProvider = databaseProvider;

            // set up the static providers
            SetMemberProviders(GetAdditionalUserDefinedTypes, GetDatabaseTables, GetDatabaseTableColumns);

            /**********************************************************************************************************************************
             * Using the specified index, we can attempt to determine what our scope is. Then, using the reverse tokenizer, we can attempt to
             * determine where within the scope we are, and attempt to provide a set of context-sensitive members based on that.
             **********************************************************************************************************************************/
            int dummyIndex;
            List<MemberResult> members = new List<MemberResult>();

            //if (TryMemberAccess(index, revTokenizer, out members))
            //{
            //    return members;
            //}

            // If any type constraint completions come back, we know that a constraint has not been completed, so we bypass searching for other completions
            if (TryTypeConstraintContext(index, revTokenizer, out members, out dummyIndex) && members.Count > 0)
            {
                return members;
            }

            bool isWithinIncompleteLiteralMacro;
            if (TryLiteralMacro(index, revTokenizer, out isWithinIncompleteLiteralMacro, out dummyIndex) && isWithinIncompleteLiteralMacro)
            {
                return members;
            }

            if (TryPreprocessorContext(index, revTokenizer, out members) ||
                TryFunctionDefContext(index, revTokenizer, out members) ||
                TryConstantContext(index, revTokenizer, out members) ||
                TryImportContext(index, revTokenizer, out members))
            {
                return members;
            }

            if (TryDefineDefContext(index, revTokenizer, out members, new List<TokenKind> { TokenKind.PublicKeyword, TokenKind.PrivateKeyword, TokenKind.GlobalsKeyword, TokenKind.FunctionKeyword }))
            {
                List<MemberResult> memberAccessResults = new List<MemberResult>();
                if (TryMemberAccess(index, revTokenizer, out memberAccessResults))
                {
                    // if there are any GeneroPackageClasses, limit them to only ones that are instance classes
                    return memberAccessResults.Where(x =>
                        {
                            IAnalysisResult tempRes = x.Var;
                            if (tempRes is GeneroPackageClass)
                            {
                                return !(tempRes as GeneroPackageClass).IsStatic;
                            }
                            return true;
                        });
                }
                return members;
            }

            bool isMemberAccess;
            if (TryLetStatement(index, revTokenizer, out members, out isMemberAccess))
            {
                //List<MemberResult> memberAccessResults = new List<MemberResult>();
                //if (TryMemberAccess(index, revTokenizer, out memberAccessResults))
                if (isMemberAccess)
                {
                    // if there are any GeneroPackageClasses, limit them to only ones that are instance classes
                    return members.Where(x =>
                    {
                        IAnalysisResult tempRes = x.Var;
                        if (tempRes is GeneroPackageClass)
                        {
                            return !(tempRes as GeneroPackageClass).IsStatic;
                        }
                        return true;
                    });
                }
                return members;
            }

            if (TryCall(index, revTokenizer, out members))
            {
                List<MemberResult> memberAccessResults = new List<MemberResult>();
                if (TryMemberAccess(index, revTokenizer, out memberAccessResults))
                {
                    // if there are any GeneroPackageClasses, limit them to only ones that are static classes
                    return memberAccessResults.Where(x =>
                    {
                        IAnalysisResult tempRes = x.Var;
                        if (tempRes is GeneroPackageClass)
                        {
                            return (tempRes as GeneroPackageClass).IsStatic;
                        }
                        return true;
                    });
                }
                return members;
            }

            List<MemberResult> accessMods;
            bool allowAccessModifiers = TryAllowAccessModifiers(index, revTokenizer, out accessMods);
            members.AddRange(accessMods);

            // TODO: need some way of knowing whether to include global members outside the scope of _body
            members.AddRange(ValidStatementKeywords.Take(allowAccessModifiers ? ValidStatementKeywords.Length : 6)
                                                   .Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
            // ..And for right now, we'll just include everything else we can include. This will test the performance a bit.
            //members.AddRange(GetDefinedMembers(index, true, true, true, true));
            return members;
        }

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
    }

    #region Context Completion Helper Classes

    public class CompletionContextMap
    {
        private readonly HashSet<TokenKind> _statementStartTokens;
        public HashSet<TokenKind> StatementStartTokens { get { return _statementStartTokens; } }

        private Dictionary<object, IList<CompletionPossibility>> _map;
        public IDictionary<object, IList<CompletionPossibility>> Map
        {
            get { return _map; }
        }

        public CompletionContextMap(HashSet<TokenKind> startTokens, IDictionary<object, IList<CompletionPossibility>> mapContents = null)
        {
            _statementStartTokens = startTokens;
            _map = new Dictionary<object, IList<CompletionPossibility>>(mapContents ?? new Dictionary<object, IList<CompletionPossibility>>());
        }

        private Dictionary<TokenKind, IList<TokenKind>> _completionsForStartToken;
        public Dictionary<TokenKind, IList<TokenKind>> CompletionsForStartToken
        {
            get
            {
                if (_completionsForStartToken == null)
                    _completionsForStartToken = new Dictionary<TokenKind, IList<TokenKind>>();
                return _completionsForStartToken;
            }
        }
    }

    public abstract class CompletionPossibility
    {
        public IEnumerable<TokenKindWithConstraint> KeywordCompletions { get; private set; }
        public Func<int, string, IEnumerable<MemberResult>> AdditionalCompletionsProvider { get; private set; }
        public bool IsBreakingStateOnFirstPrevious { get; set; }

        public CompletionPossibility(IEnumerable<TokenKindWithConstraint> keywordCompletions, Func<int, string, IEnumerable<MemberResult>> additionalCompletionsProvider = null)
        {
            KeywordCompletions = keywordCompletions;
            AdditionalCompletionsProvider = additionalCompletionsProvider;
        }
    }

    public class CategoryCompletionPossiblity : CompletionPossibility
    {
        public HashSet<TokenCategory> Categories { get; private set; }

        public CategoryCompletionPossiblity(HashSet<TokenCategory> categories, IEnumerable<TokenKindWithConstraint> keywordCompletions, Func<int, string, IEnumerable<MemberResult>> additionalCompletionsProvider = null)
            : base(keywordCompletions, additionalCompletionsProvider)
        {
            Categories = categories;
        }

        public bool MatchesCategory(TokenCategory category)
        {
            foreach (var cat in Categories)
            {
                if (cat == category)
                    return true;
            }
            return false;
        }
    }

    public class TokenKindCompletionPossiblity : CompletionPossibility
    {
        public TokenKind Kind { get; private set; }

        private HashSet<TokenKind> _multiKinds;
        public HashSet<TokenKind> MultipleKinds
        {
            get
            {
                if (_multiKinds == null)
                    _multiKinds = new HashSet<TokenKind>() { Kind };
                return _multiKinds;
            }
        }

        public TokenKindCompletionPossiblity(TokenKind kind, IEnumerable<TokenKindWithConstraint> keywordCompletions, Func<int, string, IEnumerable<MemberResult>> additionalCompletionsProvider = null)
            : base(keywordCompletions, additionalCompletionsProvider)
        {
            Kind = kind;
        }

        public TokenKindCompletionPossiblity(HashSet<TokenKind> kinds, IEnumerable<TokenKindWithConstraint> keywordCompletions, Func<int, string, IEnumerable<MemberResult>> additionalCompletionsProvider = null)
            : base(keywordCompletions, additionalCompletionsProvider)
        {
            _multiKinds = kinds;
        }
    }

    public class TokenKindWithConstraint
    {
        public TokenKind Kind { get; private set; }
        public TokenKind TokenKindToCheck { get; private set; }
        public int TokensPreviousToCheck { get; private set; }

        public TokenKindWithConstraint(TokenKind kind, TokenKind kindToCheck = Parsing.TokenKind.EndOfFile, int previousToCheck = 0)
        {
            Kind = kind;
            TokenKindToCheck = kindToCheck;
            TokensPreviousToCheck = previousToCheck;
        }

        public void DecrementPreviousToCheck()
        {
            if (TokensPreviousToCheck > 0)
                TokensPreviousToCheck--;
        }
    }

    #endregion
}
