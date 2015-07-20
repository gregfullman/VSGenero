using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.EditorExtensions
{
    internal static partial class AutoIndent
    {
        private static object _contextMapLock = new object();
        private static Dictionary<object, IEnumerable<ContextPossibility>> _contextMap;
        private static TokenKind[] BlockKeywords = new TokenKind[]
        {
             TokenKind.GlobalsKeyword,
             TokenKind.RecordKeyword,
             TokenKind.MainKeyword,
             TokenKind.TryKeyword,
             TokenKind.SqlKeyword,
             TokenKind.FunctionKeyword,
             TokenKind.IfKeyword,
             TokenKind.ElseKeyword,
             TokenKind.WhileKeyword, 
             TokenKind.ForKeyword,
             TokenKind.ForeachKeyword
        };

        internal static void Initialize()
        {
            InitializeContextMap();
        }

        private static void InitializeContextMap()
        {
            lock (_contextMapLock)
            {
                if (_contextMap == null)
                {
                    _contextMap = new Dictionary<object, IEnumerable<ContextPossibility>>();
                    #region Context Rules

                    // This covers the standard cases, where we want to indent after a block keyword, but don't indent after "end" block keyword.
                    foreach(var kind in BlockKeywords)
                    {
                        var notEndContext = new List<ContextPossibility>
                        {
                            new ContextPossibility(new OrderedTokenSet(new object[] { kind }), true),
                            new ContextPossibility(new OrderedTokenSet(new object[] { kind, TokenKind.EndKeyword }), false),
                        };
                        _contextMap.Add(kind, notEndContext);
                    }
                    // TODO: more...

                    #endregion
                }
            }
        }

        private class ContextPossibilityMatchContainer
        {
            private Dictionary<object, List<OrderedTokenSet>> _flatMatchingSet;
            private int _matchingRound;

            public ContextPossibilityMatchContainer(IEnumerable<ContextPossibility> possibilities)
            {
                _matchingRound = 0;
                _flatMatchingSet = new Dictionary<object, List<OrderedTokenSet>>();
                InitializeQueues(possibilities);
            }

            private void InitializeQueues(IEnumerable<ContextPossibility> possibilities)
            {
                foreach (var possibility in possibilities)
                {
                    object key = possibility.TokenSet.Set[0];
                    possibility.TokenSet.ParentContext = possibility;
                    if(_flatMatchingSet.ContainsKey(key))
                    {
                        _flatMatchingSet[key].Add(possibility.TokenSet);
                    }
                    else
                    {
                        _flatMatchingSet.Add(key, new List<OrderedTokenSet> { possibility.TokenSet });
                    }
                }
            }

            public bool ShouldIndent(int index, IReverseTokenizer revTokenizer)
            {
                if (_flatMatchingSet.Count > 0)
                {
                    var enumerator = revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index).GetEnumerator();
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            return false;
                        }
                        var tokInfo = enumerator.Current;
                        if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken || tokInfo.Token.Kind == TokenKind.Comment)
                            continue;   // linebreak

                        List<OrderedTokenSet> matchList;
                        if (_flatMatchingSet.TryGetValue(tokInfo.Token.Kind, out matchList) ||
                            _flatMatchingSet.TryGetValue(tokInfo.Category, out matchList))
                        {
                            bool shouldIndent = false;
                            bool temp;
                            foreach (var potentialMatch in matchList)
                            {
                                if (AttemptOrderedSetMatch(tokInfo.SourceSpan.Start.Index + 1, revTokenizer, potentialMatch))
                                {
                                    temp = potentialMatch.ParentContext.ShouldIndent;
                                    if (temp)
                                    {
                                        if (!shouldIndent)
                                            shouldIndent = true;
                                    }
                                    else
                                    {
                                        if (shouldIndent)
                                            return false;
                                    }
                                }
                            }
                            return shouldIndent;
                        }
                        else
                        {
                            if (GeneroAst.ValidStatementKeywords.Contains(tokInfo.Token.Kind))
                            {
                                return false;
                            }
                        }
                    }
                }
                return false;
            }

            private bool AttemptOrderedSetMatch(int index, IReverseTokenizer revTokenizer, OrderedTokenSet tokenSet)
            {
                bool isMatch = false;
                int tokenIndex = 0;

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

                    if ((tokenSet.Set[tokenIndex] is TokenKind && (TokenKind)tokenSet.Set[tokenIndex] == tokInfo.Token.Kind) ||
                        (tokenSet.Set[tokenIndex] is TokenCategory && (TokenCategory)tokenSet.Set[tokenIndex] == tokInfo.Category))
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

        private class ContextPossibility
        {
            public OrderedTokenSet TokenSet { get; private set; }
            public bool ShouldIndent { get; private set; }

            public ContextPossibility(OrderedTokenSet tokenSet, bool shouldIndent)
            {
                TokenSet = tokenSet;
                ShouldIndent = shouldIndent;
            }
        }

        private class OrderedTokenSet
        {
            public List<object> Set { get; private set; }
            public ContextPossibility ParentContext { get; set; }

            public OrderedTokenSet(IEnumerable<object> tokenSet)
            {
                Set = new List<object>(tokenSet);
            }
        }
    }
}
