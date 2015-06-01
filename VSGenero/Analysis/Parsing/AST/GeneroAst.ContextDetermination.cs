using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public delegate IEnumerable<IAnalysisResult> ContextSetProvider(int index);

    public partial class GeneroAst
    {
        private static Dictionary<object, IEnumerable<ContextPossibilities>> _contextMap;

        private static void InitializeContextMap()
        {
            _contextMap = new Dictionary<object, IEnumerable<ContextPossibilities>>();
            var nothing = new ContextPossibilities[0];
            var emptyTokenKindSet = new TokenKind[0];
            var emptyContextSetProviderSet = new ContextSetProvider[0];
            var emptyBackwardTokenSearchSet = new BackwardTokenSearchItem[0];
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
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetExpressionComponents },
                    new BackwardTokenSearchItem[] { }
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
                        new BackwardTokenSearchItem(new OrderedTokenSet(new TokenKind[] { TokenKind.IntoKeyword, TokenKind.InsertKeyword})),
                        new BackwardTokenSearchItem(new OrderedTokenSet(new TokenKind[] { TokenKind.LeftParenthesis, TokenKind.SetKeyword}))
                    }),
                new ContextPossibilities(
                    emptyTokenKindSet,
                    new ContextSetProvider[] { GetVariables },
                    new BackwardTokenSearchItem[]
                    {
                        new BackwardTokenSearchItem(TokenKind.ReturningKeyword),
                        new BackwardTokenSearchItem(TokenKind.InitializeKeyword),
                        new BackwardTokenSearchItem(TokenKind.LocateKeyword),
                        new BackwardTokenSearchItem(TokenKind.ValidateKeyword),
                        new BackwardTokenSearchItem(new OrderedTokenSet(new TokenKind[] { TokenKind.IntoKeyword, TokenKind.SelectKeyword}))
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
            _contextMap.Add(TokenKind.DefineKeyword, nothing);
            _contextMap.Add(TokenKind.TypeKeyword, nothing);
            _contextMap.Add(TokenKind.ConstantKeyword, nothing);
            _contextMap.Add(TokenKind.FunctionKeyword, nothing);
            _contextMap.Add(TokenKind.ReportKeyword, nothing);
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
                        TokenKind.SqlKeyword,
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
            _contextMap.Add(TokenKind.FirstKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    emptyTokenKindSet,
                    new ContextSetProvider[] { GetVariables },
                    emptyBackwardTokenSearchSet
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
            _contextMap.Add(TokenKind.IfKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind [] { TokenKind.NotKeyword },
                    emptyContextSetProviderSet,
                    new BackwardTokenSearchItem[] 
                    { 
                        new BackwardTokenSearchItem(new OrderedTokenSet(new TokenKind[] { TokenKind.SequenceKeyword, TokenKind.CreateKeyword }))
                    }
                ),
                new ContextPossibilities(
                    new TokenKind [] { TokenKind.ExistsKeyword },
                    emptyContextSetProviderSet,
                    new BackwardTokenSearchItem[] 
                    { 
                        new BackwardTokenSearchItem(new OrderedTokenSet(new TokenKind[] { TokenKind.SequenceKeyword, TokenKind.DropKeyword }))
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
                        new BackwardTokenSearchItem(TokenKind.SetKeyword)
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
                        new BackwardTokenSearchItem(TokenKind.ColumnKeyword)
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
                        new BackwardTokenSearchItem(new OrderedTokenSet(new TokenKind[] { TokenKind.TableKeyword, TokenKind.CreateKeyword }))
                    }
                )
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
        }

        private static IEnumerable<IAnalysisResult> GetExpressionComponents(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetTypes(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetFunctions(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetLabels(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetVariables(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetOptionsStartKeywords(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetConstants(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetDatabaseTables(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetAvailableImportModules(int index)
        {
            return new IAnalysisResult[0];
        }

        private static IEnumerable<IAnalysisResult> GetStatementStartKeywords(int index)
        {
            return new IAnalysisResult[0];
        }
    }

    public class ContextPossibilities
    {
        public IEnumerable<TokenKind> SingleTokens { get; private set; }
        public IEnumerable<ContextSetProvider> SetProviders { get; private set; }
        public IEnumerable<BackwardTokenSearchItem> BackwardSearchItems { get; private set; }

        public ContextPossibilities(IEnumerable<TokenKind> singleTokens,
            IEnumerable<ContextSetProvider> setProviders,
            IEnumerable<BackwardTokenSearchItem> backwardSearchItems)
        {
            SingleTokens = singleTokens;
            SetProviders = setProviders;
            BackwardSearchItems = backwardSearchItems;
        }
    }

    public class BackwardTokenSearchItem
    {
        public OrderedTokenSet TokenSet { get; private set; }
        public TokenKind SingleToken { get; private set; }
        public bool Match { get; private set; }

        public BackwardTokenSearchItem(OrderedTokenSet tokenSet, bool match = true)
        {
            TokenSet = tokenSet;
            Match = match;
        }

        public BackwardTokenSearchItem(TokenKind singleToken, bool match = true)
        {
            SingleToken = singleToken;
            Match = match;
        }
    }

    public class OrderedTokenSet
    {
        public List<TokenKind> Set { get; private set; }

        public OrderedTokenSet(IEnumerable<TokenKind> tokenSet)
        {
            Set = new List<TokenKind>(tokenSet);
        }
    }
}
