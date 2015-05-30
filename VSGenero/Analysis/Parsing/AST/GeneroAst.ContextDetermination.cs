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
            _contextMap.Add(TokenKind.AllKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { TokenKind.IntoKeyword, TokenKind.FromKeyword },
                    new ContextSetProvider[] { },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.SelectKeyword) }
                )
            });
            _contextMap.Add(TokenKind.AlterKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { TokenKind.SequenceKeyword },
                    new ContextSetProvider[] { },
                    new BackwardTokenSearchItem[] { }
                )
            });
            _contextMap.Add(TokenKind.Ampersand, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { TokenKind.IncludeKeyword, TokenKind.DefineKeyword, TokenKind.UndefKeyword, TokenKind.IfdefKeyword, TokenKind.EndifKeyword },
                    new ContextSetProvider[] { },
                    new BackwardTokenSearchItem[] { }
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
                    new ContextSetProvider[] { },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.WheneverKeyword) }
                )
            });
            _contextMap.Add(TokenKind.ArrayKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { TokenKind.OfKeyword },
                    new ContextSetProvider[] { },
                    new BackwardTokenSearchItem[] { }
                ),
                new ContextPossibilities(
                    new TokenKind[] { TokenKind.OfKeyword, TokenKind.WithKeyword },
                    new ContextSetProvider[] { },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.DynamicKeyword) }
                )
            });
            _contextMap.Add(TokenKind.AsKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] {  },
                    new ContextSetProvider[] { },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.SelectKeyword) }
                ),
                new ContextPossibilities(
                    new TokenKind[] {  },
                    new ContextSetProvider[] { GetTypes },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.CastKeyword) }
                )
            });
            _contextMap.Add(TokenKind.AsciiKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetExpressionComponents },
                    new BackwardTokenSearchItem[] { }
                )
            });
            _contextMap.Add(TokenKind.Assign, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetExpressionComponents },
                    new BackwardTokenSearchItem[] { }
                )
            });
            _contextMap.Add(TokenKind.ByKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { },
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
                    new TokenKind[] { },
                    new ContextSetProvider[] { },
                    new BackwardTokenSearchItem[] { 
                        new BackwardTokenSearchItem(TokenKind.SequenceKeyword) 
                    }
                )
            });
            _contextMap.Add(TokenKind.CallKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetFunctions },
                    new BackwardTokenSearchItem[] { }
                )
            });
            _contextMap.Add(TokenKind.CaseKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { TokenKind.WhenKeyword },
                    new ContextSetProvider[] { GetExpressionComponents },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.ExitKeyword, false) }
                )
            });
            _contextMap.Add(TokenKind.Colon, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetLabels },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.GotoKeyword) }
                )
            });
            _contextMap.Add(TokenKind.ColumnKeyword, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetExpressionComponents },
                    new BackwardTokenSearchItem[] { }
                )
            });
            _contextMap.Add(TokenKind.Comma, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] {},
                    new ContextSetProvider[] {},
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
                        new BackwardTokenSearchItem(new OrderedTokenSet(new List<TokenKind> { TokenKind.IntoKeyword, TokenKind.InsertKeyword})),
                        new BackwardTokenSearchItem(new OrderedTokenSet(new List<TokenKind> { TokenKind.LeftParenthesis, TokenKind.SetKeyword}))
                    }),
                new ContextPossibilities(
                    new TokenKind[] {},
                    new ContextSetProvider[] { GetVariables },
                    new BackwardTokenSearchItem[]
                    {
                        new BackwardTokenSearchItem(TokenKind.ReturningKeyword),
                        new BackwardTokenSearchItem(TokenKind.InitializeKeyword),
                        new BackwardTokenSearchItem(TokenKind.LocateKeyword),
                        new BackwardTokenSearchItem(TokenKind.ValidateKeyword),
                        new BackwardTokenSearchItem(new OrderedTokenSet(new List<TokenKind> { TokenKind.IntoKeyword, TokenKind.SelectKeyword}))
                    }),
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetOptionsStartKeywords },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.OptionsKeyword) }
                ),
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetExpressionComponents },
                    new BackwardTokenSearchItem[] { }
                ),
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetDatabaseTables },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.FromKeyword) }
                ),
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetVariables, GetConstants },
                    new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.ValuesKeyword) }
                )
            });
            _contextMap.Add(TokenKind.DoubleBar, new List<ContextPossibilities>
            {
                new ContextPossibilities(
                    new TokenKind[] { },
                    new ContextSetProvider[] { GetExpressionComponents },
                    new BackwardTokenSearchItem[] { }
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
