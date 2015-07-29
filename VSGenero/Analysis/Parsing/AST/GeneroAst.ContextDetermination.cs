using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public delegate IEnumerable<MemberResult> ContextSetProvider(int index);

    public partial class GeneroAst
    {
        private static object _contextMapLock = new object();
        private static Dictionary<object, IEnumerable<ContextPossibilities>> _contextMap;
        private static GeneroAst _instance;
        private static bool _includePublicFunctions;
        private static string _contextString;

        #region Context Map Init

        private static void InitializeContextMap()
        {
            lock (_contextMapLock)
            {
                if (_contextMap == null)
                {
                    _contextMap = new Dictionary<object, IEnumerable<ContextPossibilities>>();
                    var nothing = new ContextPossibilities[0];
                    var emptyTokenKindSet = new TokenKind[0];
                    var emptyContextSetProviderSet = new ContextSetProvider[0];
                    var emptyBackwardTokenSearchSet = new BackwardTokenSearchItem[0];
                    #region Context Rules
                    _contextMap.Add(TokenKind.ActionKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.AllKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.IntoKeyword, TokenKind.FromKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.SelectKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.AlterKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.SequenceKeyword },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.Ampersand, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] 
                            { 
                                TokenKind.IncludeKeyword, 
                                TokenKind.DefineKeyword, 
                                TokenKind.UndefKeyword, 
                                TokenKind.IfdefKeyword, 
                                TokenKind.EndifKeyword 
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.AndKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.WhereKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.AnyKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ErrorKeyword, TokenKind.SqlerrorKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.WheneverKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.ArrayKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.OfKeyword },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.OfKeyword, TokenKind.WithKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.DynamicKeyword) }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ScreenKeyword, TokenKind.ClearKeyword}))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.AsKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.SelectKeyword) }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetTypes },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.CastKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.AsciiKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.Assign, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    var attribPossibilities = new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.MessageKeyword),
                                new BackwardTokenSearchItem(TokenKind.ErrorKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DisplayKeyword})),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.NameKeyword, TokenKind.ByKeyword, TokenKind.DisplayKeyword })),
                            }
                        ),
                    };
                    _contextMap.Add(TokenKind.AttributeKeyword, attribPossibilities);
                    _contextMap.Add(TokenKind.AttributesKeyword, attribPossibilities);
                    _contextMap.Add(TokenKind.BeforeKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.MenuKeyword },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.BetweenKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ByKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] { 
                                new BackwardTokenSearchItem(TokenKind.GroupKeyword),
                                new BackwardTokenSearchItem(TokenKind.OrderKeyword),
                                new BackwardTokenSearchItem(TokenKind.IncrementKeyword) 
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.NameKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DisplayKeyword) 
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ScrollKeyword) 
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.CacheKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] { 
                                new BackwardTokenSearchItem(TokenKind.SequenceKeyword) 
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.CallKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetFunctions },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.CaseKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.WhenKeyword },
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.ExitKeyword, false) }
                        )
                    });
                    _contextMap.Add(TokenKind.ClearKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] 
                            {
                                TokenKind.FormKeyword,
                                TokenKind.ScreenKeyword,
                            },
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.Colon, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetLabels },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.GotoKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.ColumnKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.Comma, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[]
                            {
                                new BackwardTokenSearchItem(TokenKind.ReturningKeyword),
                                new BackwardTokenSearchItem(TokenKind.InitializeKeyword),
                                new BackwardTokenSearchItem(TokenKind.LocateKeyword),
                                new BackwardTokenSearchItem(TokenKind.ValidateKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.IntoKeyword, TokenKind.SelectKeyword}))
                            }),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[]
                            {
                                new BackwardTokenSearchItem(TokenKind.DefineKeyword),
                                new BackwardTokenSearchItem(TokenKind.TypeKeyword),
                                new BackwardTokenSearchItem(TokenKind.ConstantKeyword),
                                new BackwardTokenSearchItem(TokenKind.FunctionKeyword),
                                new BackwardTokenSearchItem(TokenKind.ReportKeyword),
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword),
                                new BackwardTokenSearchItem(TokenKind.OrderKeyword),
                                new BackwardTokenSearchItem(TokenKind.GroupKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.RecordKeyword, TokenKind.EndKeyword})),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.IntoKeyword, TokenKind.InsertKeyword})),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.LeftParenthesis, TokenKind.SetKeyword}))
                            }),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetOptionsStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.OptionsKeyword) }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[]
                            {
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.LeftParenthesis, TokenKind.InKeyword}))
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetDatabaseTables },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.FromKeyword) }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables, GetConstants },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.ValuesKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.CommandKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.KeyKeyword },
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.DoubleBar, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ContinueKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ForKeyword, 
                                TokenKind.ForeachKeyword, 
                                TokenKind.WhileKeyword, 
                                TokenKind.MenuKeyword, 
                                TokenKind.ConstructKeyword,
                                TokenKind.InputKeyword,
                                TokenKind.DialogKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ErrorKeyword, TokenKind.WheneverKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.SqlerrorKeyword, TokenKind.WheneverKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.WarningKeyword, TokenKind.WheneverKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.FoundKeyword, TokenKind.NotKeyword, TokenKind.WheneverKeyword }))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.CreateKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.SequenceKeyword, 
                                TokenKind.TempKeyword, 
                                TokenKind.TableKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.CurrentKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.OfKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.WhereKeyword) }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.YearKeyword, 
                                TokenKind.MonthKeyword, 
                                TokenKind.DayKeyword, 
                                TokenKind.HourKeyword, 
                                TokenKind.MinuteKeyword,
                                TokenKind.SecondKeyword,
                                TokenKind.FractionKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.DatabaseKeyword, nothing);
                    _contextMap.Add(TokenKind.DatetimeKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.YearKeyword, 
                                TokenKind.MonthKeyword, 
                                TokenKind.DayKeyword, 
                                TokenKind.HourKeyword, 
                                TokenKind.MinuteKeyword,
                                TokenKind.SecondKeyword,
                                TokenKind.FractionKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.DayKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ToKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DatetimeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.IntervalKeyword }))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.DefineKeyword, nothing);
                    _contextMap.Add(TokenKind.TypeKeyword, nothing);
                    _contextMap.Add(TokenKind.ConstantKeyword, nothing);
                    _contextMap.Add(TokenKind.DeleteKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.FromKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.DescribeKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.DatabaseKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.DimensionKeyword, nothing);
                    _contextMap.Add(TokenKind.DisplayKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] {
                                TokenKind.ByKeyword
                            },
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.DistinctKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.IntoKeyword, TokenKind.FromKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.SelectKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.Divide, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.DownKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ByKeyword
                            },
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.ScrollKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.DropKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.SequenceKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.DynamicKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ArrayKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ElseKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.EndKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.MainKeyword, 
                                TokenKind.GlobalsKeyword, 
                                TokenKind.FunctionKeyword, 
                                TokenKind.ReportKeyword, 
                                TokenKind.RecordKeyword,
                                TokenKind.CaseKeyword,
                                TokenKind.TryKeyword,
                                TokenKind.ForKeyword,
                                TokenKind.IfKeyword,
                                TokenKind.WhileKeyword,
                                TokenKind.SqlKeyword,       // TODO: this is only allowed in BDL 2.5 and up!
                                TokenKind.MenuKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.Equals, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.LetKeyword),
                                new BackwardTokenSearchItem(TokenKind.ForKeyword),
                                new BackwardTokenSearchItem(TokenKind.IfKeyword),
                                new BackwardTokenSearchItem(TokenKind.WhileKeyword),
                                new BackwardTokenSearchItem(TokenKind.WhereKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SetKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.DoubleEquals, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ErrorKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ContinueKeyword, 
                                TokenKind.StopKeyword, 
                                TokenKind.CallKeyword, 
                                TokenKind.RaiseKeyword, 
                                TokenKind.GotoKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WheneverKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ExistsKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] { 
                                new BackwardTokenSearchItem(TokenKind.SequenceKeyword),
                                new BackwardTokenSearchItem(TokenKind.TableKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.ExitKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ProgramKeyword, 
                                TokenKind.CaseKeyword, 
                                TokenKind.ForKeyword, 
                                TokenKind.ForeachKeyword, 
                                TokenKind.WhileKeyword,
                                TokenKind.MenuKeyword,
                                TokenKind.ConstructKeyword,
                                TokenKind.ReportKeyword,
                                TokenKind.DisplayKeyword,
                                TokenKind.InputKeyword,
                                TokenKind.DialogKeyword,
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.Power, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.FglKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetAvailableImportModules },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.ImportKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.FirstKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.SelectKeyword) }
                        )
                    });
                    _contextMap.Add(TokenKind.ForKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { new BackwardTokenSearchItem(TokenKind.EndKeyword) }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.FormKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ClearKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.FractionKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ToKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DatetimeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.IntervalKeyword }))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.FreeKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.FromKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetDatabaseTables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword),
                                new BackwardTokenSearchItem(TokenKind.DeleteKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.FoundKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ContinueKeyword, 
                                TokenKind.StopKeyword, 
                                TokenKind.CallKeyword, 
                                TokenKind.RaiseKeyword, 
                                TokenKind.GotoKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WheneverKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.FunctionKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.EndKeyword)
                            }),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet)
                    });

                    _contextMap.Add(TokenKind.GlobalsKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.DefineKeyword, 
                                TokenKind.TypeKeyword, 
                                TokenKind.ConstantKeyword, 
                                TokenKind.EndKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.EndKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.GotoKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetLabels },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.GreaterThan, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.GreaterThanOrEqual, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.GroupKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.ByKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.FromKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.HideKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.OptionKeyword },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.HourKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ToKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DatetimeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.IntervalKeyword }))
                            }
                        )
                    });
                    var identAndKeywordPossibilities = new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WhenKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenCategory.Identifier, TokenKind.DefineKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.LikeKeyword, TokenKind.DefineKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenCategory.Identifier, TokenKind.TypeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenCategory.Identifier, TokenKind.Comma, TokenKind.RecordKeyword, TokenKind.EndKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.LikeKeyword, TokenKind.RecordKeyword, TokenKind.ArrayKeyword, TokenKind.DynamicKeyword },
                            new ContextSetProvider[] { GetTypes },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.RecordKeyword),
                                new BackwardTokenSearchItem(TokenKind.DefineKeyword),
                                new BackwardTokenSearchItem(TokenKind.TypeKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Comma, TokenKind.DefineKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Comma, TokenKind.TypeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Comma, TokenKind.RecordKeyword, TokenKind.EndKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetSystemTypes },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ConstantKeyword),
                                new BackwardTokenSearchItem(TokenKind.TableKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.LetKeyword),
                                new BackwardTokenSearchItem(TokenKind.ForKeyword),
                                new BackwardTokenSearchItem(TokenKind.TableKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.LeftParenthesis, TokenKind.IntoKeyword, TokenKind.InsertKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.ToKeyword, TokenKind.ThruKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.InitializeKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.InKeyword, TokenKind.ThruKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.LocateKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.LikeKeyword, TokenKind.ThruKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ValidateKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.WhenKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CaseKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.StepKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.Equals, TokenKind.ForKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.ThenKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.IfKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.FromKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.IntoKeyword, TokenKind.SelectKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.AsKeyword, TokenKind.IntoKeyword, TokenKind.FromKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.ValuesKeyword, TokenKind.SelectKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.IntoKeyword, TokenKind.InsertKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.OuterKeyword, TokenKind.WhereKeyword, TokenKind.GroupKeyword, TokenKind.OrderKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.FromKeyword, TokenKind.SelectKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.WhereKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.FromKeyword, TokenKind.DeleteKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.NotKeyword, TokenKind.BetweenKeyword, TokenKind.LikeKeyword, TokenKind.InKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WhereKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenCategory.Keyword, TokenKind.WhereKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.AscKeyword, TokenKind.DescKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.OrderKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.HavingKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.GroupKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.SetKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.UpdateKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] 
                            { 
                                TokenKind.IncrementKeyword, 
                                TokenKind.StartKeyword, 
                                TokenKind.RestartKeyword,
                                TokenKind.NomaxvalueKeyword,
                                TokenKind.MaxvalueKeyword, 
                                TokenKind.NominvalueKeyword, 
                                TokenKind.MinvalueKeyword,
                                TokenKind.CycleKeyword,
                                TokenKind.NocycleKeyword, 
                                TokenKind.CacheKeyword, 
                                TokenKind.NocacheKeyword,
                                TokenKind.OrderKeyword,
                                TokenKind.NoorderKeyword
                            },
                            new ContextSetProvider[] { GetTypes },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SequenceKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Equals, TokenKind.LetKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Equals, TokenCategory.Identifier, TokenKind.WhereKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.ToKeyword },
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DisplayKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.AttributeKeyword, TokenKind.AttributesKeyword },
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.NameKeyword, TokenKind.ByKeyword, TokenKind.DisplayKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DisplayKeyword })),
                                new BackwardTokenSearchItem(TokenKind.MessageKeyword),
                                new BackwardTokenSearchItem(TokenKind.ErrorKeyword),
                                new BackwardTokenSearchItem(TokenKind.MenuKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.UpKeyword, TokenKind.DownKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ScrollKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ByKeyword, TokenKind.ScrollKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            emptyBackwardTokenSearchSet
                        ),
                    };
                    _contextMap.Add(TokenCategory.Identifier, identAndKeywordPossibilities);
                    _contextMap.Add(TokenCategory.Keyword, identAndKeywordPossibilities);
                    _contextMap.Add(TokenKind.IdleKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.IfKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.NotKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.SequenceKeyword, TokenKind.CreateKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.ExistsKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.SequenceKeyword, TokenKind.DropKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.EndKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ImportKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.FglKeyword, 
                                TokenKind.JavaKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.InKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.MemoryKeyword, TokenKind.FileKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.LocateKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.TableKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.IncrementKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.ByKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SequenceKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.InitializeKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.InsertKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.IntoKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.InstanceOfKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetTypes },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.IntervalKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.YearKeyword, 
                                TokenKind.MonthKeyword, 
                                TokenKind.DayKeyword, 
                                TokenKind.HourKeyword, 
                                TokenKind.MinuteKeyword,
                                TokenKind.SecondKeyword,
                                TokenKind.FractionKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.IntoKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.InsertKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.IsKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.NotKeyword,
                                TokenKind.NullKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.KeyKeyword, nothing);
                    _contextMap.Add(TokenKind.JavaKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,     // TODO: eventually it would be nice to return available java classes
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ImportKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.LabelKeyword, nothing);
                    _contextMap.Add(TokenKind.LeftBracket, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetConstants },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ArrayKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.LeftParenthesis, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CharKeyword),
                                new BackwardTokenSearchItem(TokenKind.CharacterKeyword),
                                new BackwardTokenSearchItem(TokenKind.VarcharKeyword),
                                new BackwardTokenSearchItem(TokenKind.DecKeyword),
                                new BackwardTokenSearchItem(TokenKind.DecimalKeyword),
                                new BackwardTokenSearchItem(TokenKind.NumericKeyword),
                                new BackwardTokenSearchItem(TokenKind.MoneyKeyword),
                                new BackwardTokenSearchItem(TokenKind.FractionKeyword),
                                new BackwardTokenSearchItem(TokenKind.YearKeyword),
                                new BackwardTokenSearchItem(TokenKind.MonthKeyword),
                                new BackwardTokenSearchItem(TokenKind.DayKeyword),
                                new BackwardTokenSearchItem(TokenKind.HourKeyword),
                                new BackwardTokenSearchItem(TokenKind.MinuteKeyword),
                                new BackwardTokenSearchItem(TokenKind.SecondKeyword),
                                new BackwardTokenSearchItem(TokenKind.FunctionKeyword),
                                new BackwardTokenSearchItem(TokenKind.InsertKeyword),
                                new BackwardTokenSearchItem(TokenKind.SetKeyword),
                                new BackwardTokenSearchItem(TokenKind.WhereKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Equals, TokenKind.WhereKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CallKeyword),
                                new BackwardTokenSearchItem(TokenKind.Equals),
                                new BackwardTokenSearchItem(TokenKind.DoubleEquals),
                                new BackwardTokenSearchItem(TokenKind.IfKeyword),
                                new BackwardTokenSearchItem(TokenKind.ForKeyword),
                                new BackwardTokenSearchItem(TokenKind.AsciiKeyword),
                                new BackwardTokenSearchItem(TokenKind.ColumnKeyword),
                                new BackwardTokenSearchItem(TokenKind.InKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetDatabaseTables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.FromKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CastKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables, GetConstants },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ValuesKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.PrimaryKeyword, TokenKind.UniqueKeyword, TokenKind.CheckKeyword, TokenKind.ForeignKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.TableKeyword, TokenKind.CreateKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        ),
                    });
                    _contextMap.Add(TokenKind.LessThan, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.LessThanOrEqual, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.LetKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.LikeKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetDatabaseTables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DefineKeyword),
                                new BackwardTokenSearchItem(TokenKind.TypeKeyword),
                                new BackwardTokenSearchItem(TokenKind.RecordKeyword),
                                new BackwardTokenSearchItem(TokenKind.InitializeKeyword),
                                new BackwardTokenSearchItem(TokenKind.ValidateKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.LimitKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.LocateKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.LogKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.InKeyword, TokenKind.ExtentKeyword, TokenKind.NextKeyword, TokenKind.LockKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WithKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.MainKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.MatchesKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.MaxvalueKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SequenceKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.MenuKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind [] { TokenKind.AttributeKeyword, TokenKind.AttributesKeyword },
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.MessageKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.MiddleKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.Subtract, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.MinuteKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ToKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DatetimeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.IntervalKeyword }))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.MinvalueKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SequenceKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.ModKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.MonthKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ToKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DatetimeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.IntervalKeyword }))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.NameKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ByKeyword, TokenKind.DisplayKeyword }))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.NextKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.OptionKeyword },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.NoKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.LogKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WithKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.NotKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.NullKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.IsKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.FoundKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WheneverKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.BetweenKeyword, TokenKind.LikeKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WhereKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ExistsKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SequenceKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.LikeKeyword, TokenKind.MatchesKeyword },
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.NotEquals, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.NotEqualsLTGT, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenCategory.NumericLiteral, new List<ContextPossibilities>
                    {
                         new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.LeftParenthesis),
                                new BackwardTokenSearchItem(TokenKind.LeftBracket),
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.OfKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DimensionKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.StepKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ToKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Equals, TokenKind.ForKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.StepKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.Equals, TokenKind.ForKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ToKeyword },
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DisplayKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.AttributeKeyword, TokenKind.AttributesKeyword },
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.MessageKeyword),
                                new BackwardTokenSearchItem(TokenKind.ErrorKeyword),
                                new BackwardTokenSearchItem(TokenKind.MenuKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DisplayKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.NameKeyword, TokenKind.ByKeyword, TokenKind.DisplayKeyword })),
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.LeftParenthesis, TokenKind.WhereKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.OfKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.RecordKeyword },
                            new ContextSetProvider[] { GetTypes },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ArrayKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetCursors },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CurrentKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.OnKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ActionKeyword, TokenKind.IdleKeyword },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.OptionsKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ShortKeyword, 
                                TokenKind.MenuKeyword, 
                                TokenKind.MessageKeyword, 
                                TokenKind.CommentKeyword, 
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
                                TokenKind.SqlKeyword,
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.OptionKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.AllKeyword },
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ShowKeyword),
                                new BackwardTokenSearchItem(TokenKind.HideKeyword),
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.NextKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.OrKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WhereKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.OrderKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ByKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.FromKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.Add, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.PrimaryKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.KeyKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.TableKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.PrivateKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.DefineKeyword, 
                                TokenKind.TypeKeyword, 
                                TokenKind.ConstantKeyword, 
                                TokenKind.FunctionKeyword, 
                                TokenKind.ReportKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.PublicKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.DefineKeyword, 
                                TokenKind.TypeKeyword, 
                                TokenKind.ConstantKeyword, 
                                TokenKind.FunctionKeyword, 
                                TokenKind.ReportKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.RecordKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.EndKeyword, TokenKind.LikeKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DefineKeyword),
                                new BackwardTokenSearchItem(TokenKind.TypeKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.EndKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.ReportKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.EndKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.RestartKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.WithKeyword },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ReturnKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ReturningKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.RightBracket, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.StepKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.Equals, TokenKind.ForKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ThenKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.IfKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.LetKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ToKeyword },
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DisplayKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.AttributeKeyword, TokenKind.AttributesKeyword },
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.MessageKeyword),
                                new BackwardTokenSearchItem(TokenKind.ErrorKeyword),
                                new BackwardTokenSearchItem(TokenKind.MenuKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DisplayKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.NameKeyword, TokenKind.ByKeyword, TokenKind.DisplayKeyword })),
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.RightParenthesis, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ToKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords, GetBinaryOperatorKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DisplayKeyword)
                            }
                        ),
                         new ContextPossibilities(
                            new TokenKind[] { TokenKind.AttributeKeyword, TokenKind.AttributesKeyword },
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.MessageKeyword),
                                new BackwardTokenSearchItem(TokenKind.ErrorKeyword),
                                new BackwardTokenSearchItem(TokenKind.MenuKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DisplayKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.NameKeyword, TokenKind.ByKeyword, TokenKind.DisplayKeyword })),
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ToKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.YearKeyword),
                                new BackwardTokenSearchItem(TokenKind.MonthKeyword),
                                new BackwardTokenSearchItem(TokenKind.DayKeyword),
                                new BackwardTokenSearchItem(TokenKind.HourKeyword),
                                new BackwardTokenSearchItem(TokenKind.MinuteKeyword),
                                new BackwardTokenSearchItem(TokenKind.SecondKeyword),
                                new BackwardTokenSearchItem(TokenKind.ForKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.EndKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.RecordKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DefineKeyword),
                                new BackwardTokenSearchItem(TokenKind.TypeKeyword),
                                new BackwardTokenSearchItem(TokenKind.ConstantKeyword),
                                new BackwardTokenSearchItem(TokenKind.FunctionKeyword),
                                new BackwardTokenSearchItem(TokenKind.ReportKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ReturningKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CallKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.YearKeyword, TokenKind.MonthKeyword, 
                                              TokenKind.DayKeyword, TokenKind.HourKeyword, 
                                              TokenKind.MinuteKeyword, TokenKind.SecondKeyword, TokenKind.FractionKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.StepKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.Equals, TokenKind.ForKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ThenKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.IfKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.OuterKeyword, TokenKind.WhereKeyword, 
                                              TokenKind.GroupKeyword, TokenKind.OrderKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.FromKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ValuesKeyword, TokenKind.SelectKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.IntoKeyword, TokenKind.InsertKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SetKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.WithKeyword, TokenKind.InKeyword, 
                                              TokenKind.ExtentKeyword, TokenKind.NextKeyword, TokenKind.LockKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.TableKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WhereKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetBinaryOperatorKeywords, GetStatementStartKeywords },
                            emptyBackwardTokenSearchSet
                        ),
                    });
                    _contextMap.Add(TokenKind.SchemaKeyword, nothing);
                    _contextMap.Add(TokenKind.ScreenKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ArrayKeyword,
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ClearKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.ScrollKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.SecondKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ToKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DatetimeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.IntervalKeyword }))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.SelectKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.SkipKeyword, 
                                TokenKind.FirstKeyword, 
                                TokenKind.MiddleKeyword, 
                                TokenKind.LimitKeyword, 
                                TokenKind.AllKeyword,
                                TokenKind.DistinctKeyword,
                                TokenKind.UniqueKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.SequenceKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.IfKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CreateKeyword),
                                new BackwardTokenSearchItem(TokenKind.DropKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.AlterKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.SetKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetDatabaseTables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.UpdateKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.ShortKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.CircuitKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.OptionsKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.ShowKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.OptionKeyword },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.SkipKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.SleepKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.SqlerrorKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ContinueKeyword, 
                                TokenKind.StopKeyword, 
                                TokenKind.CallKeyword, 
                                TokenKind.RaiseKeyword, 
                                TokenKind.GotoKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WheneverKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.Multiply, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.IntoKeyword, TokenKind.FromKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Dot, TokenKind.LikeKeyword, TokenKind.DefineKeyword }))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.UpKeyword, TokenKind.DownKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ScrollKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.StartKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.WithKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SequenceKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.StepKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenCategory.StringLiteral, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords, GetBinaryOperatorKeywords, GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WhileKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.HelpKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords, GetBinaryOperatorKeywords, GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CommandKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ToKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords, GetBinaryOperatorKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DisplayKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.AttributeKeyword, TokenKind.AttributesKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords, GetBinaryOperatorKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.MessageKeyword),
                                new BackwardTokenSearchItem(TokenKind.ErrorKeyword),
                                new BackwardTokenSearchItem(TokenKind.MenuKeyword),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DisplayKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Name, TokenKind.ByKeyword, TokenKind.DisplayKeyword })),
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords, GetBinaryOperatorKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Equals, TokenKind.LetKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.Equals, TokenCategory.Identifier, TokenKind.WhereKeyword })),
                            }
                        ),
                    });
                    _contextMap.Add(TokenKind.TableKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.IfKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CreateKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.TempKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.TableKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.CreateKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.ThenKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ThruKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.InitializeKeyword),
                                new BackwardTokenSearchItem(TokenKind.ValidateKeyword),
                                new BackwardTokenSearchItem(TokenKind.LocateKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.ToKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.YearKeyword, 
                                TokenKind.MonthKeyword, 
                                TokenKind.DayKeyword, 
                                TokenKind.HourKeyword, 
                                TokenKind.MinuteKeyword,
                                TokenKind.SecondKeyword,
                                TokenKind.FractionKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword),
                                new BackwardTokenSearchItem(TokenKind.CurrentKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.NullKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.InitializeKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ForKeyword),
                                new BackwardTokenSearchItem(TokenKind.DisplayKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.UniqueKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.IntoKeyword, TokenKind.FromKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.SelectKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.UnitsKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.YearKeyword, 
                                TokenKind.MonthKeyword, 
                                TokenKind.DayKeyword, 
                                TokenKind.HourKeyword, 
                                TokenKind.MinuteKeyword,
                                TokenKind.SecondKeyword,
                                TokenKind.FractionKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.UpKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.ByKeyword },
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.ScrollKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.UpdateKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetDatabaseTables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.UsingKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ValidateKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.ValuesKeyword, nothing);
                    _contextMap.Add(TokenKind.VarcharKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.EndKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.RecordKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DefineKeyword),
                                new BackwardTokenSearchItem(TokenKind.TypeKeyword),
                                new BackwardTokenSearchItem(TokenKind.ConstantKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.WarningKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ContinueKeyword, 
                                TokenKind.StopKeyword, 
                                TokenKind.CallKeyword, 
                                TokenKind.RaiseKeyword, 
                                TokenKind.GotoKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.WheneverKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.WhenKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetVariables },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.WheneverKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.AnyKeyword, 
                                TokenKind.ErrorKeyword, 
                                TokenKind.SqlerrorKeyword, 
                                TokenKind.NotKeyword, 
                                TokenKind.WarningKeyword
                            },
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.WhereKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.FromKeyword, TokenKind.SelectKeyword}))
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.CurrentKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.FromKeyword, TokenKind.DeleteKeyword}))
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.WhileKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.EndKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetExpressionComponents },
                            emptyBackwardTokenSearchSet
                        )
                    });
                    _contextMap.Add(TokenKind.WithKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.DimensionKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DynamicKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.StartKeyword),
                                new BackwardTokenSearchItem(TokenKind.RestartKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            new TokenKind[] { TokenKind.NoKeyword },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.TableKeyword)
                            }
                        )
                    });
                    _contextMap.Add(TokenKind.YearKeyword, new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            new TokenKind[] { 
                                TokenKind.ToKeyword
                            },
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(TokenKind.DatetimeKeyword),
                                new BackwardTokenSearchItem(TokenKind.IntervalKeyword)
                            }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            new ContextSetProvider[] { GetStatementStartKeywords },
                            new BackwardTokenSearchItem[] 
                            { 
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.DatetimeKeyword })),
                                new BackwardTokenSearchItem(new OrderedTokenSet(new object[] { TokenKind.ToKeyword, TokenKind.IntervalKeyword }))
                            }
                        )
                    });
                    #endregion Context Rules
                }
            }
        }

        #endregion

        #region Context Provider Functions

        private static IEnumerable<MemberResult> GetExpressionComponents(int index)
        {
            if (_instance != null)
            {
                return _instance.GetInstanceExpressionComponents(index);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetTypes(int index)
        {
            if (_instance != null)
            {
                return _instance.GetInstanceTypes(index);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetSystemTypes(int index)
        {
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetFunctions(int index)
        {
            if (_instance != null)
            {
                return _instance.GetInstanceFunctions(index);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetLabels(int index)
        {
            if (_instance != null)
            {
                return _instance.GetInstanceLabels(index);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetVariables(int index)
        {
            if (_instance != null)
            {
                return _instance.GetInstanceVariables(index);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetOptionsStartKeywords(int index)
        {
            return OptionsStartTokens
                .Select(x =>
                new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, _instance));
        }

        private static IEnumerable<MemberResult> GetConstants(int index)
        {
            if (_instance != null)
            {
                return _instance.GetInstanceVariables(index);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetDatabaseTables(int index)
        {
            if (_instance != null)
            {
                return _instance.GetDatabaseTables(index, null);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetAvailableImportModules(int index)
        {
            if (_instance != null)
            {
                return _instance.GetInstanceImportModules(index);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetStatementStartKeywords(int index)
        {
            TokenKind[] accessMods = new TokenKind[] { TokenKind.PublicKeyword, TokenKind.PrivateKeyword };
            return ValidStatementKeywords
                .Union(accessMods)
                .Select(x =>
                new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, _instance));
        }

        private static IEnumerable<MemberResult> GetCursors(int index)
        {
            if (_instance != null)
            {
                return _instance.GetInstanceCursors(index);
            }
            return new MemberResult[0];
        }

        private static IEnumerable<MemberResult> GetBinaryOperatorKeywords(int index)
        {
            return _binaryOperators.Where(x => x > TokenKind.LastOperator)
              .Select(x =>
                new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, _instance));
        }

        #endregion

        public IEnumerable<MemberResult> GetContextMembers(int index, IReverseTokenizer revTokenizer, IFunctionInformationProvider functionProvider,
                                                           IDatabaseInformationProvider databaseProvider, IProgramFileProvider programFileProvider,
                                                           bool includePublicFunctions, string contextStr, 
                                                           GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            _instance = this;
            _functionProvider = functionProvider;
            _databaseProvider = databaseProvider;
            _programFileProvider = programFileProvider;
            _includePublicFunctions = includePublicFunctions;
            _contextString = contextStr;

            List<MemberResult> members = new List<MemberResult>();
            // First see if we have a member completion
            if (TryMemberAccess(index, revTokenizer, out members))
            {
                _includePublicFunctions = false;
                return members;
            }
            
            if (!DetermineContext(index, revTokenizer, members) && members.Count == 0)
            {
                // TODO: do we want to put in the statement keywords?
                members.AddRange(GetStatementStartKeywords(index));
            }
            _includePublicFunctions = false;
            _contextString = null;
            return members;
        }

        private bool DetermineContext(int index, IReverseTokenizer revTokenizer, List<MemberResult> memberList)
        {
            var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    return false;
                }

                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) ||
                    tokInfo.Token.Kind == TokenKind.NewLine ||
                    tokInfo.Token.Kind == TokenKind.NLToken ||
                    tokInfo.Token.Kind == TokenKind.Comment)
                    continue;   // linebreak

                // Look for the token in the context map
                IEnumerable<ContextPossibilities> possibilities;
                if (_contextMap.TryGetValue(tokInfo.Token.Kind, out possibilities) ||
                    _contextMap.TryGetValue(tokInfo.Category, out possibilities))
                {
                    var matchContainer = new ContextPossibilityMatchContainer(possibilities);
                    IEnumerable<ContextPossibilities> matchingPossibilities;
                    if (matchContainer.TryMatchContextPossibility(tokInfo.SourceSpan.Start.Index, revTokenizer, out matchingPossibilities))
                    {
                        foreach(var matchedPossible in matchingPossibilities)
                            LoadPossibilitySet(index, matchedPossible, memberList);
                    }

                    return true;
                }
                else
                {
                    // we don't have the token in our context map, so return
                    return false;
                }
            }
        }

        private void LoadPossibilitySet(int index, ContextPossibilities matchedPossibility, List<MemberResult> members)
        {
            members.AddRange(matchedPossibility.SingleTokens.Select(x =>
                new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, _instance)));
            foreach (var provider in matchedPossibility.SetProviders)
                members.AddRange(provider(index));
        }

        private class ContextPossibilityMatchContainer
        {
            private Dictionary<object, List<BackwardTokenSearchItem>> _flatMatchingSet;
            private HashSet<object> _flatNonMatchingSet;
            private List<ContextPossibilities> _possibilitiesWithNoBackwardSearch;
            private int _matchingRound;

            public ContextPossibilityMatchContainer(IEnumerable<ContextPossibilities> possibilities)
            {
                _matchingRound = 0;
                _flatMatchingSet = new Dictionary<object, List<BackwardTokenSearchItem>>();
                _flatNonMatchingSet = new HashSet<object>();
                _possibilitiesWithNoBackwardSearch = new List<ContextPossibilities>();
                InitializeQueues(possibilities);
            }

            private void InitializeQueues(IEnumerable<ContextPossibilities> possibilities)
            {
                foreach (var possibility in possibilities)
                {
                    if (possibility.BackwardSearchItems.Length > 0)
                    {
                        foreach (var searchItem in possibility.BackwardSearchItems)
                        {
                            searchItem.ParentContext = possibility;
                            object key = TokenKind.EndOfFile;
                            if (searchItem.TokenSet == null)
                            {
                                key = searchItem.SingleToken;
                            }
                            else
                            {
                                key = searchItem.TokenSet.Set[0];
                            }

                            if (_flatMatchingSet.ContainsKey(key))
                            {
                                _flatMatchingSet[key].Add(searchItem);
                            }
                            else
                            {
                                _flatMatchingSet.Add(key, new List<BackwardTokenSearchItem> { searchItem });
                            }

                            if (!searchItem.Match)
                                _flatNonMatchingSet.Add(key);
                        }
                    }
                    else
                    {
                        _possibilitiesWithNoBackwardSearch.Add(possibility);
                    }
                }
            }

            public bool TryMatchContextPossibility(int index, IReverseTokenizer revTokenizer, out IEnumerable<ContextPossibilities> matchingPossibilities)
            {
                List<ContextPossibilities> retList = new List<ContextPossibilities>();
                bool isMatch = false;

                if (_flatMatchingSet.Count > 0)
                {
                    // start reverse parsing
                    var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            isMatch = false;
                            break;
                        }
                        var tokInfo = enumerator.Current;
                        if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                            continue;   // linebreak

                        // look for the token in the matching dictionary
                        List<BackwardTokenSearchItem> matchList;
                        if (_flatMatchingSet.TryGetValue(tokInfo.Token.Kind, out matchList) ||
                            _flatMatchingSet.TryGetValue(tokInfo.Category, out matchList))
                        {
                            // need to attempt matching the match list
                            // 1) grab the potential matches with an ordered set and attempt to completely match each set. If one of the sets completely matches, we have a winner
                            foreach (var potentialMatch in matchList.Where(x => x.TokenSet != null))
                            {
                                if (AttemptOrderedSetMatch(tokInfo.SourceSpan.Start.Index, revTokenizer, potentialMatch.TokenSet))
                                {
                                    retList.Add(potentialMatch.ParentContext);
                                    isMatch = true;
                                    break;
                                }
                            }
                            if (isMatch)
                                break;      // if we have a match from the for loop above, we're done.

                            // 2) If we have any single token matches, they win.
                            var singleMatches = matchList.Where(x =>
                                (x.SingleToken is TokenKind && (TokenKind)x.SingleToken != TokenKind.EndOfFile) ||
                                (x.SingleToken is TokenCategory && (TokenCategory)x.SingleToken != TokenCategory.None)).ToList();
                            if (singleMatches.Count > 0)
                            {
                                retList.AddRange(singleMatches.Select(x => x.ParentContext));
                                isMatch = true;// singleMatches.All(x => x.Match);  // TODO: the match flag isn't being used....need to figure that out.
                                break;
                            }

                            // At this point, nothing was matched correctly, so we continue
                        }
                        else if (_flatNonMatchingSet.Count > 0 &&
                                (!_flatNonMatchingSet.Contains(tokInfo.Token.Kind) ||
                                !_flatNonMatchingSet.Contains(tokInfo.Category)))
                        {
                            // need to attempt matching the match list
                            // 1) grab the potential matches with an ordered set and attempt to completely match each set. If one of the sets completely matches, we have a winner
                            foreach (var potentialMatch in _flatNonMatchingSet.SelectMany(x => _flatMatchingSet[x]).Where(x => x.TokenSet != null))
                            {
                                if (AttemptOrderedSetMatch(tokInfo.SourceSpan.Start.Index, revTokenizer, potentialMatch.TokenSet, false))
                                {
                                    retList.Add(potentialMatch.ParentContext);
                                    isMatch = true;
                                    break;
                                }
                            }
                            if (isMatch)
                                break;      // if we have a match from the for loop above, we're done.

                            // 2) If we have any single token matches, they win.
                            var singleMatches = _flatNonMatchingSet.SelectMany(x => _flatMatchingSet[x]).Where(x =>
                                (x.SingleToken is TokenKind && (TokenKind)x.SingleToken != TokenKind.EndOfFile) ||
                                (x.SingleToken is TokenCategory && (TokenCategory)x.SingleToken != TokenCategory.None)).ToList();
                            if (singleMatches.Count > 0)
                            {
                                retList.AddRange(singleMatches.Select(x => x.ParentContext));
                                isMatch = true;
                                break;
                            }

                            // At this point, nothing was matched correctly, so we continue
                        }
                        else
                        {
                            if (GeneroAst.ValidStatementKeywords.Contains(tokInfo.Token.Kind))
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                }

                if (!isMatch && _possibilitiesWithNoBackwardSearch.Count > 0)
                {
                    retList.AddRange(_possibilitiesWithNoBackwardSearch);
                    isMatch = true;
                }

                matchingPossibilities = retList;
                return isMatch;
            }

            private bool AttemptOrderedSetMatch(int index, IReverseTokenizer revTokenizer, OrderedTokenSet tokenSet, bool doMatch = true)
            {
                bool isMatch = false;
                int tokenIndex = 1;

                // start reverse parsing
                var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        isMatch = false;
                        break;
                    }
                    var tokInfo = enumerator.Current;
                    if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                        continue;   // linebreak

                    if (doMatch &&
                        (tokenSet.Set[tokenIndex] is TokenKind && (TokenKind)tokenSet.Set[tokenIndex] == tokInfo.Token.Kind) ||
                        (tokenSet.Set[tokenIndex] is TokenCategory && (TokenCategory)tokenSet.Set[tokenIndex] == tokInfo.Category))
                    {
                        tokenIndex++;
                        if (tokenSet.Set.Count == tokenIndex)
                        {
                            isMatch = true;
                            break;
                        }
                    }
                    else if (!doMatch &&
                        (tokenSet.Set[tokenIndex] is TokenKind && (TokenKind)tokenSet.Set[tokenIndex] != tokInfo.Token.Kind) ||
                        (tokenSet.Set[tokenIndex] is TokenCategory && (TokenCategory)tokenSet.Set[tokenIndex] != tokInfo.Category))
                    {
                        tokenIndex++;
                        if (tokenSet.Set.Count == tokenIndex)
                        {
                            isMatch = true;
                            break;
                        }
                    }
                    else
                    {
                        if (GeneroAst.ValidStatementKeywords.Contains(tokInfo.Token.Kind) ||
                            tokInfo.Token.Kind == TokenKind.EndOfFile)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                }

                return isMatch;
            }
        }

        private class ContextPossibilities
        {
            public TokenKind[] SingleTokens { get; private set; }
            public ContextSetProvider[] SetProviders { get; private set; }
            public BackwardTokenSearchItem[] BackwardSearchItems { get; private set; }

            public ContextPossibilities(TokenKind[] singleTokens,
                ContextSetProvider[] setProviders,
                BackwardTokenSearchItem[] backwardSearchItems)
            {
                SingleTokens = singleTokens;
                SetProviders = setProviders;
                BackwardSearchItems = backwardSearchItems;
            }
        }

        private class BackwardTokenSearchItem
        {
            public OrderedTokenSet TokenSet { get; private set; }
            public object SingleToken { get; private set; }
            public bool Match { get; private set; }
            public ContextPossibilities ParentContext { get; set; }

            public BackwardTokenSearchItem(OrderedTokenSet tokenSet, bool match = true)
            {
                TokenSet = tokenSet;
                SingleToken = TokenKind.EndOfFile;
                Match = match;
            }

            public BackwardTokenSearchItem(TokenKind singleToken, bool match = true)
            {
                SingleToken = singleToken;
                TokenSet = null;
                Match = match;
            }
        }

        private class OrderedTokenSet
        {
            public List<object> Set { get; private set; }

            public OrderedTokenSet(IEnumerable<object> tokenSet)
            {
                Set = new List<object>(tokenSet);
            }
        }
    }
}
