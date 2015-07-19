using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.EditorExtensions
{
    internal static partial class AutoIndent
    {
        private static object _contextMapLock = new object();
        private static Dictionary<object, IEnumerable<ContextPossibilities>> _contextMap;

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
                    _contextMap = new Dictionary<object, IEnumerable<ContextPossibilities>>();
                    var nothing = new ContextPossibilities[0];
                    var emptyTokenKindSet = new TokenKind[0];
                    var emptyContextSetProviderSet = new ContextSetProvider[0];
                    var emptyBackwardTokenSearchSet = new BackwardTokenSearchItem[0];
                    #region Context Rules
					var notEndContext = new List<ContextPossibilities>
                    {
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            new BackwardTokenSearchItem[] { new BackwardTokenSearchItem(TokenKind.EndKeyword) }
                        ),
                        new ContextPossibilities(
                            emptyTokenKindSet,
                            emptyContextSetProviderSet,
                            emptyBackwardTokenSearchSet
                        )
                    };
                    _contextMap.Add(TokenKind.GlobalsKeyword, notEndContext);
                    _contextMap.Add(TokenKind.RecordKeyword, notEndContext);
                    _contextMap.Add(TokenKind.MainKeyword, notEndContext);
                    _contextMap.Add(TokenKind.TryKeyword, notEndContext);
                    _contextMap.Add(TokenKind.SqlKeyword, notEndContext);
                    _contextMap.Add(TokenKind.FunctionKeyword, notEndContext);
                    _contextMap.Add(TokenKind.IfKeyword, notEndContext);
                    _contextMap.Add(TokenKind.ElseKeyword, notEndContext);
                    _contextMap.Add(TokenKind.WhileKeyword, notEndContext);
                    _contextMap.Add(TokenKind.ForKeyword, notEndContext);
                    _contextMap.Add(TokenKind.ForeachKeyword, notEndContext);
                    // TODO: more...

                    #endregion
                }
            }
        }
    }
}
