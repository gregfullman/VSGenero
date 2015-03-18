using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public sealed class GeneroAst : ILocationResolver
    {
        private readonly GeneroLanguageVersion _langVersion;
        private readonly AstNode _body;
        internal readonly int[] _lineLocations;
        private readonly IProjectEntry _projEntry;
        private readonly string _filename;

        private readonly Dictionary<AstNode, Dictionary<object, object>> _attributes = new Dictionary<AstNode, Dictionary<object, object>>();

        private CompletionContextMap _defineStatementMap;
        private CompletionContextMap _typeConstraintsMap;

        public GeneroAst(AstNode body, int[] lineLocations, GeneroLanguageVersion langVersion = GeneroLanguageVersion.None, IProjectEntry projEntry = null, string filename = null)
        {
            if (body == null) {
                throw new ArgumentNullException("body");
            }
            _langVersion = langVersion;
            _body = body;
            _lineLocations = lineLocations;
            _projEntry = projEntry;

            InitializeCompletionContextMaps();
        }

        private void InitializeCompletionContextMaps()
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
                        }, GetTypesForIndex),
                    new TokenKindCompletionPossiblity(TokenKind.ArrayKeyword, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, GetTypesForIndex),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.NumericLiteral },
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, GetTypesForIndex)
                });
            _defineStatementMap.Map.Add(TokenKind.LikeKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.RecordKeyword, new List<TokenKindWithConstraint>(), (i) => new List<MemberResult>()   /* TODO: table completions  */),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier },
                        new List<TokenKindWithConstraint>(), (i) => new List<MemberResult>()   /* TODO: table completions  */)
                });
            _defineStatementMap.Map.Add(TokenKind.RecordKeyword, new List<CompletionPossibility>
                {
                    new TokenKindCompletionPossiblity(TokenKind.EndKeyword, new List<TokenKindWithConstraint>()),
                    new TokenKindCompletionPossiblity(TokenKind.OfKeyword, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.LikeKeyword),
                        }),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier },
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.LikeKeyword),
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
                        }, GetTypesForIndex),
                    new TokenKindCompletionPossiblity(TokenKind.RecordKeyword, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, GetTypesForIndex),
                        new TokenKindCompletionPossiblity(TokenKind.Comma, 
                        new List<TokenKindWithConstraint>
                        {
                            new TokenKindWithConstraint(TokenKind.RecordKeyword),
                            new TokenKindWithConstraint(TokenKind.LikeKeyword)
                        }, GetTypesForIndex),
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier }, new List<TokenKindWithConstraint>())
                };
            _defineStatementMap.Map.Add(TokenCategory.Keyword, keywordAndIdentPossibilities);
            _defineStatementMap.Map.Add(TokenCategory.Identifier, keywordAndIdentPossibilities);
            _defineStatementMap.Map.Add(TokenKind.Dot, new List<CompletionPossibility>
                {
                    new CategoryCompletionPossiblity(new HashSet<TokenCategory> { TokenCategory.Keyword, TokenCategory.Identifier }, new List<TokenKindWithConstraint>())
                });
            // end Define statement map

            // 2) Type constraint map
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

        public AstNode Body
        {
            get { return _body; }
        }

        public GeneroLanguageVersion LanguageVersion
        {
            get { return _langVersion; }
        }

        public IGeneroProjectEntry ProjectEntry
        {
            get
            {
                return _projEntry as IGeneroProjectEntry;
            }
        }

        internal bool TryGetAttribute(AstNode node, object key, out object value)
        {
            Dictionary<object, object> nodeAttrs;
            if (_attributes.TryGetValue(node, out nodeAttrs))
            {
                return nodeAttrs.TryGetValue(key, out value);
            }
            else
            {
                value = null;
            }
            return false;
        }

        internal void SetAttribute(AstNode node, object key, object value)
        {
            Dictionary<object, object> nodeAttrs;
            if (!_attributes.TryGetValue(node, out nodeAttrs))
            {
                nodeAttrs = _attributes[node] = new Dictionary<object, object>();
            }
            nodeAttrs[key] = value;
        }

        /// <summary>
        /// Copies attributes that apply to one node and makes them available for the other node.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void CopyAttributes(AstNode from, AstNode to)
        {
            Dictionary<object, object> nodeAttrs;
            if (_attributes.TryGetValue(from, out nodeAttrs))
            {
                _attributes[to] = new Dictionary<object, object>(nodeAttrs);
            }
        }

        internal SourceLocation IndexToLocation(int index)
        {
            if (index == -1)
            {
                return SourceLocation.Invalid;
            }

            var locs = _lineLocations;
            int match = Array.BinarySearch(locs, index);
            if (match < 0)
            {
                // If our index = -1, it means we're on the first line.
                if (match == -1)
                {
                    return new SourceLocation(index, 1, index + 1);
                }

                // If we couldn't find an exact match for this line number, get the nearest
                // matching line number less than this one
                match = ~match - 1;
            }
            return new SourceLocation(index, match + 2, index - locs[match] + 1);
        }

        LocationInfo ILocationResolver.ResolveLocation(IProjectEntry project, object location)
        {
            IAnalysisResult result = location as IAnalysisResult;
            if (result != null)
            {
                var locIndex = result.LocationIndex;
                var loc = IndexToLocation(locIndex);
                return new LocationInfo(project, loc.Line, loc.Column);
            }
            return null;
        }

        public LocationInfo ResolveLocation(object location)
        {
            IAnalysisResult result = location as IAnalysisResult;
            if(result != null)
            {
                var locIndex = result.LocationIndex;
                var loc = IndexToLocation(locIndex);
                return _projEntry == null ? new LocationInfo(_filename, loc.Line, loc.Column) : new LocationInfo(_projEntry, loc.Line, loc.Column);
            }
            return null;
        }

        public MemberResult[] GetModules(bool topLevelOnly = false)
        {
            List<MemberResult> res = new List<MemberResult>();

            //var children = GlobalScope.GetChildrenPackages(InterpreterContext);

            //foreach (var child in children)
            //{
            //    res.Add(new MemberResult(child.Key, PythonMemberType.Module));
            //}

            return res.ToArray();
        }

        public MemberResult[] GetModuleMembers(string[] names, bool includeMembers = false)
        {
            var res = new List<MemberResult>();
            //var children = GlobalScope.GetChildrenPackages(InterpreterContext);

            //foreach (var child in children)
            //{
            //    var mod = (ModuleInfo)child.Value;
            //    var childName = mod.Name.Substring(this.GlobalScope.Name.Length + 1);

            //    if (childName.StartsWith(names[0]))
            //    {
            //        res.AddRange(PythonAnalyzer.GetModuleMembers(InterpreterContext, names, includeMembers, mod as IModule));
            //    }
            //}

            return res.ToArray();
        }

        #region Completion Members

        public IEnumerable<MemberResult> GetContextMembersByIndex(int index, IReverseTokenizer revTokenizer, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            /**********************************************************************************************************************************
             * Using the specified index, we can attempt to determine what our scope is. Then, using the reverse tokenizer, we can attempt to
             * determine where within the scope we are, and attempt to provide a set of context-sensitive members based on that.
             **********************************************************************************************************************************/
            List<MemberResult> members = new List<MemberResult>();
            int dummyIndex;
            // If any type constraint completions come back, we know that a constraint has not been completed, so we bypass searching for other completions
            if (TryTypeConstraintContext(index, revTokenizer, out members, out dummyIndex) && members.Count > 0)
            {
                return members;
            }
            bool isWithinIncompleteLiteralMacro;
            if(TryLiteralMacro(index, revTokenizer, out isWithinIncompleteLiteralMacro, out dummyIndex) && isWithinIncompleteLiteralMacro)
            {
                return members;
            }
            if(TryPreprocessorContext(index, revTokenizer, out members) ||
               TryFunctionDefContext(index, revTokenizer, out members) ||
               TryConstantContext(index, revTokenizer, out members) ||
               TryDefineDefContext(index, revTokenizer, out members, new List<TokenKind> { TokenKind.PublicKeyword, TokenKind.PrivateKeyword, TokenKind.GlobalsKeyword, TokenKind.FunctionKeyword }))
            {
                return members;
            }

            List<MemberResult> accessMods;
            bool allowAccessModifiers = TryAllowAccessModifiers(index, revTokenizer, out accessMods);
            members.AddRange(accessMods);

            // TODO: need some way of knowing whether to include global members outside the scope of _body
            members.AddRange(ValidStatementKeywords.Take(allowAccessModifiers ? ValidStatementKeywords.Length : 6)
                                                   .Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
            return members;
        }

        private IEnumerable<MemberResult> GetTypesForIndex(int index)
        {
            List<MemberResult> members = new List<MemberResult>();
            // Built-in types
            members.AddRange(BuiltinTypes.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));

            // do a binary search to determine what node we're in
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
                    members.AddRange((containingNode as IFunctionResult).Types.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
                }

                if (_body is IModuleResult)
                {
                    // check for module vars, types, and constants (and globals defined in this module)
                    members.AddRange((_body as IModuleResult).Types.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
                    members.AddRange((_body as IModuleResult).GlobalTypes.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
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
                                    members.AddRange(modRes.Types.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
                                }
                            }
                        }
                    }
                }
            }

            return members;
        }

        #region Constant Member Context

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

        private bool TryLiteralMacro(int index, IReverseTokenizer revTokenizer, out bool isWithinIncompleteMacro, out int startIndex)
        {
            startIndex = index;
            isWithinIncompleteMacro = false;
            LiteralMacroStatus firstStatus = LiteralMacroStatus.None;
            LiteralMacroStatus currStatus = LiteralMacroStatus.None;
            foreach (var tokInfo in revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index))
            {
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken)
                    continue;   // linebreak

                if(tokInfo.Token.Kind == TokenKind.MdyKeyword ||
                   tokInfo.Token.Kind == TokenKind.DatetimeKeyword ||
                   tokInfo.Token.Kind == TokenKind.IntervalKeyword)
                {
                    if (currStatus != LiteralMacroStatus.LeftParen)
                    {
                        return false;
                    }
                    if(firstStatus != LiteralMacroStatus.RightParen && firstStatus != LiteralMacroStatus.None)
                    {
                        isWithinIncompleteMacro = true;
                    }
                    startIndex = tokInfo.SourceSpan.Start.Index;
                    return true;
                }
                else if(tokInfo.Token.Kind == TokenKind.LeftParenthesis)
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
                else if(tokInfo.Token.Kind == TokenKind.RightParenthesis)
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
                else if(tokInfo.Category == TokenCategory.Identifier || tokInfo.Category == TokenCategory.Keyword)
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
            while(true)
            {
                if (!enumerator.MoveNext())
                {
                    results.Clear();
                    return false;
                }

                var tokInfo = enumerator.Current;
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken)
                    continue;   // linebreak

                if (tokInfo.Token.Kind == TokenKind.ConstantKeyword)
                {
                    if(currStatus != ConstantDefStatus.IdentOrKeyword &&
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
                        if(firstStatus != ConstantDefStatus.None &&
                           firstStatus != ConstantDefStatus.IdentOrKeyword &&
                           firstStatus != ConstantDefStatus.Equals &&
                           firstStatus != ConstantDefStatus.Comma)
                        {
                            return false;
                        }
                    }

                    return true;
                }
                else if(tokInfo.Token.Kind == TokenKind.Comma)
                {
                    if (firstStatus == ConstantDefStatus.None)
                        firstStatus = ConstantDefStatus.Comma;
                    if(currStatus == ConstantDefStatus.None ||
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
                else if(tokInfo.Token.Kind == TokenKind.Equals)
                {
                    if (firstStatus == ConstantDefStatus.None)
                        firstStatus = ConstantDefStatus.Equals;
                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    if (currStatus == ConstantDefStatus.None)
                    {
                        TokenKind[] kinds = new TokenKind[] { TokenKind.MdyKeyword, TokenKind.DatetimeKeyword, TokenKind.IntervalKeyword };
                        results.AddRange(kinds.Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
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
                else if(tokInfo.Category == TokenCategory.CharacterLiteral ||
                        tokInfo.Category == TokenCategory.NumericLiteral ||
                        tokInfo.Category == TokenCategory.StringLiteral)
                {
                    if (firstStatus == ConstantDefStatus.None)
                        firstStatus = ConstantDefStatus.Literal;
                    tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                    if(currStatus == ConstantDefStatus.None ||
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
                else if(tokInfo.Category == TokenCategory.IncompleteMultiLineStringLiteral)
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
                else if(tokInfo.Token.Kind == TokenKind.RightParenthesis)
                {
                    bool dummyIsWithin;
                    int startIndex;
                    if(TryLiteralMacro((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyIsWithin, out startIndex))
                    {
                        if (firstStatus == ConstantDefStatus.None)
                            firstStatus = ConstantDefStatus.LiteralMacro;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                        while (tokInfo.Equals(default(TokenInfo)) ||
                                tokInfo.Token.Kind == TokenKind.NewLine ||
                                tokInfo.Token.Kind == TokenKind.NLToken ||
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
                                    tokInfo.Token.Kind == TokenKind.NLToken))
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
                        else if(currStatus == ConstantDefStatus.TypeConstraint ||
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
                    if(TryTypeConstraintContext((tokInfo.SourceSpan.Start.Index + 1), revTokenizer, out dummyResults, out startIndex))
                    {
                        if (firstStatus == ConstantDefStatus.None)
                            firstStatus = ConstantDefStatus.TypeConstraint;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                        // move us past the type constraint
                        while (tokInfo.Equals(default(TokenInfo)) ||
                                tokInfo.Token.Kind == TokenKind.NewLine ||
                                tokInfo.Token.Kind == TokenKind.NLToken ||
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
                        else if(currStatus == ConstantDefStatus.Equals ||
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
                    else if(BuiltinTypes.Where(x => x != TokenKind.TextKeyword && x != TokenKind.ByteKeyword)
                                        .Contains(tokInfo.Token.Kind))
                    {
                        if (firstStatus == ConstantDefStatus.None)
                            firstStatus = ConstantDefStatus.BuiltinType;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
                        if(currStatus == ConstantDefStatus.None ||
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
                    else if(tokInfo.Category == TokenCategory.Identifier || tokInfo.Category == TokenCategory.Keyword)
                    {
                        if (firstStatus == ConstantDefStatus.None)
                            firstStatus = ConstantDefStatus.IdentOrKeyword;
                        tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());

                        if(currStatus == ConstantDefStatus.None)
                        {
                            currStatus = ConstantDefStatus.IdentOrKeyword;
                            // insert built-in types to the completions (excluding text and byte)
                            results.AddRange(BuiltinTypes.Where(x => x != TokenKind.ByteKeyword && x != TokenKind.TextKeyword)
                                                         .Select(x => new MemberResult(Tokens.TokenKinds[x], GeneroMemberType.Keyword, this)));
                        }
                        else if(currStatus == ConstantDefStatus.BuiltinType ||
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

        #endregion

        #region Type Constraint Member Context

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
            while(true)
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
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken)
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
                            results.AddRange(entry.AdditionalCompletionsProvider(index));
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
                        if(_typeConstraintsMap.CompletionsForStartToken.TryGetValue(tokInfo.Token.Kind, out stmtStartCompletions))
                        {
                            foreach(var tok in stmtStartCompletions)
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

        #endregion

        #region Define Member Context

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
                if (tokInfo.Equals(default(TokenInfo)) || tokInfo.Token.Kind == TokenKind.NewLine || tokInfo.Token.Kind == TokenKind.NLToken)
                    continue;   // linebreak

                if(failingKeywords != null && failingKeywords.Contains(tokInfo.Token.Kind))
                {
                    results.Clear();
                    return false;
                }

                if(possibilities != null)
                {
                    // we have a set of possiblities from the token before. Let's try to match something out of it
                    var entry = possibilities.FirstOrDefault(x =>
                        {
                            if (x is TokenKindCompletionPossiblity)
                                return (x as TokenKindCompletionPossiblity).MultipleKinds.Contains(tokInfo.Token.Kind);
                            else
                                return (x as CategoryCompletionPossiblity).Categories.Contains(tokInfo.Category);
                        });
                    if(entry == null)
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
                            results.AddRange(entry.AdditionalCompletionsProvider(index));
                        }
                        completionsSupplied = true;
                    }

                    // go through the tokens that are awaiting validation before being added to the result set
                    for (int i = tokensAwaitingValidation.Count - 1; i >= 0; i--)
                    {
                        tokensAwaitingValidation[i].DecrementPreviousToCheck();
                        if(tokensAwaitingValidation[i].TokensPreviousToCheck == 0)
                        {
                            // check for the token kind we're supposed to check for
                            if(tokInfo.Token.Kind == tokensAwaitingValidation[i].TokenKindToCheck)
                            {
                                results.Add(new MemberResult(Tokens.TokenKinds[tokensAwaitingValidation[i].Kind], GeneroMemberType.Keyword, this));
                            }
                            // and remove the token
                            tokensAwaitingValidation.RemoveAt(i);
                        }
                    }

                    if(firstPreviousToken)
                    {
                        firstPreviousToken = false;
                        if(entry.IsBreakingStateOnFirstPrevious)
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
                        else if(tokensCollected.Last().Token.Kind == TokenKind.RecordKeyword &&
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
                else if(_defineStatementMap.Map.TryGetValue(tokInfo.Token.Kind, out possibilities))
                {
                    currKeywordsOrIdentsBackToBack = 0;
                    if(tokInfo.Token.Kind == TokenKind.RecordKeyword)
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
                    else if(tokInfo.Token.Kind == TokenKind.EndKeyword)
                    {
                        nestedRecordDefCount++;
                    }
                }
                else if(_defineStatementMap.Map.TryGetValue(tokInfo.Category, out possibilities))
                {
                    if(tokInfo.Category == TokenCategory.Keyword || tokInfo.Category == TokenCategory.Identifier)
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

                if(!isFirstTokenHitRecord.HasValue)
                {
                    isFirstTokenHitRecord = false;
                }
                isFirstToken = false;
                tokensCollected.Insert(0, tokInfo.ToTokenWithSpan());
            }

            results.Clear();
            return false;
        }

        #endregion

        #region Function Member Context

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
                switch(status)
                {
                    case FunctionDefStatus.None:
                        {
                            if(tokInfo.Category == TokenCategory.Identifier || 
                               (tokInfo.Category == TokenCategory.Keyword && tokInfo.Token.Kind != TokenKind.FunctionKeyword && tokInfo.Token.Kind != TokenKind.ReportKeyword))
                            {
                                status = FunctionDefStatus.IdentOrKeyword;
                            }
                            else if(tokInfo.Token.Kind == TokenKind.FunctionKeyword || tokInfo.Token.Kind == TokenKind.ReportKeyword)
                            {
                                status = FunctionDefStatus.FuncKeyword;
                                quit = true;
                            }
                            else if(tokInfo.Token.Kind == TokenKind.Comma)
                            {
                                status = FunctionDefStatus.Comma;
                            }
                            else if(tokInfo.Token.Kind == TokenKind.LeftParenthesis)
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
                                else if(tokInfo.Token.Kind == TokenKind.Comma)
                                {
                                    status = FunctionDefStatus.Comma;
                                }
                                else if(tokInfo.Token.Kind == TokenKind.LeftParenthesis)
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

                if(quit)
                {
                    break;
                }
            }

            return status == FunctionDefStatus.FuncKeyword;
        }

        #endregion

        private bool TryPreprocessorContext(int index, IReverseTokenizer revTokenizer, out List<MemberResult> results)
        {
            bool isAmpersand = false;
            foreach (var tokInfo in revTokenizer.GetReversedTokens().Where(x => x.SourceSpan.Start.Index < index))
            {
                if (tokInfo.Equals(default(TokenInfo)))
                    break;
                if(tokInfo.Token.Kind == TokenKind.Ampersand)
                {
                    isAmpersand = true;
                    break;
                }
                break;
            }

            if(isAmpersand)
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
            TokenKind.ImportKeyword,
            TokenKind.SchemaKeyword,
            TokenKind.DescribeKeyword,
            TokenKind.DatabaseKeyword,
            TokenKind.GlobalsKeyword,

            // Valid keywords that apply to the module keywords
            TokenKind.EndKeyword,

            // Valid statement start keywords (TODO: definitely missing some here...)
            TokenKind.CallKeyword,
            TokenKind.LetKeyword,
            TokenKind.IfKeyword,
            TokenKind.ElseKeyword,
            TokenKind.ForKeyword,
            TokenKind.ForeachKeyword,
            TokenKind.WhileKeyword,
            TokenKind.DoKeyword,
            TokenKind.DeferKeyword
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

            if(!isPrevTokenPublicOrPrivate)
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
        
        #endregion

        /// <summary>
        /// Gets the available names at the given location.  This includes built-in variables, global variables, and locals.
        /// </summary>
        /// <param name="index">The 0-based absolute index into the file where the available mebmers should be looked up.</param>
        public IEnumerable<MemberResult> GetAllAvailableMembersByIndex(int index, GetMemberOptions options = GetMemberOptions.IntersectMultipleResults)
        {
            /*
             * Need to return (depending on context):
             * 1) Local variables, types, or constants
             * 2) Module variables, types, or constants
             * 3) Module cursors
             * 4) Functions within the current module
             * 5) Global variables, types, or constants
             * 6) Functions within the current project
             * 7) Public function modules
             */

            //List<MemberResult> results = new List<MemberResult>();
            //return results;
            //var result = new Dictionary<string, List<AnalysisValue>>();

            //// collect builtins
            //foreach (var variable in ProjectState.BuiltinModule.GetAllMembers(ProjectState._defaultContext))
            //{
            //    result[variable.Key] = new List<AnalysisValue>(variable.Value);
            //}

            //// collect variables from user defined scopes
            //var scope = FindScope(index);
            //foreach (var s in scope.EnumerateTowardsGlobal)
            //{
            //    foreach (var kvp in s.GetAllMergedVariables())
            //    {
            //        result[kvp.Key] = new List<AnalysisValue>(kvp.Value.TypesNoCopy);
            //    }
            //}

            //var res = MemberDictToResultList(GetPrivatePrefix(scope), options, result);
            //if (options.Keywords())
            //{
            //    res = GetKeywordMember    `s(options, scope).Union(res);
            //}

            //return res;h
            IEnumerable<MemberResult> res = GetKeywordMembers(options);

            // do a binary search to determine what node we're in
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
                    res = res.Union(func.Variables.Keys.Select(x => new MemberResult(x, GeneroMemberType.Instance, this)));
                    res = res.Union(func.Types.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
                    res = res.Union(func.Constants.Keys.Select(x => new MemberResult(x, GeneroMemberType.Constant, this)));
                }

                if (_body is IModuleResult)
                {
                    // check for module vars, types, and constants (and globals defined in this module)
                    IModuleResult mod = _body as IModuleResult;
                    res = res.Union(mod.Variables.Keys.Select(x => new MemberResult(x, GeneroMemberType.Module, this)));
                    res = res.Union(mod.Types.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
                    res = res.Union(mod.Constants.Keys.Select(x => new MemberResult(x, GeneroMemberType.Constant, this)));
                    res = res.Union(mod.GlobalVariables.Keys.Select(x => new MemberResult(x, GeneroMemberType.Module, this)));
                    res = res.Union(mod.GlobalTypes.Keys.Select(x => new MemberResult(x, GeneroMemberType.Class, this)));
                    res = res.Union(mod.GlobalConstants.Keys.Select(x => new MemberResult(x, GeneroMemberType.Constant, this)));

                    // check for cursors in this module
                    res = res.Union(mod.Cursors.Keys.Select(x => new MemberResult(x, GeneroMemberType.Unknown, this)));

                    // check for module functio
                    res = res.Union(mod.Functions.Keys.Select(x => new MemberResult(x, GeneroMemberType.Method, this)));
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

            return res;
        }

        /// <summary>
        /// Gets information about the available signatures for the given expression.
        /// </summary>
        /// <param name="exprText">The expression to get signatures for.</param>
        /// <param name="index">The 0-based absolute index into the file.</param>
        public IEnumerable<IFunctionResult> GetSignaturesByIndex(string exprText, int index, IReverseTokenizer revTokenizer)
        {
            // First see if we're in the process of defining a function
            List<MemberResult> dummyList;
            if (TryFunctionDefContext(index, revTokenizer, out dummyList))
            {
                return null;
            }

            /*
             * Need to check for:
             * 1) Functions within the current module
             * 2) Functions within the current project
             * 3) Public functions
             */
            if (_body is IModuleResult)
            {
                // check for module vars, types, and constants (and globals defined in this module)
                IModuleResult mod = _body as IModuleResult;

                // check for module functions
                IFunctionResult funcRes;
                if (mod.Functions.TryGetValue(exprText, out funcRes))
                {
                    return new IFunctionResult[1] { funcRes };
                }
            }

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
                                // check project functions
                                IFunctionResult funcRes;
                                if (modRes.Functions.TryGetValue(exprText, out funcRes))
                                {
                                    if (funcRes.AccessModifier == AccessModifier.Public)
                                        return new IFunctionResult[1] { funcRes };
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Evaluates the given expression in at the provided line number and returns the values
        /// that the expression can evaluate to.
        /// </summary>
        /// <param name="exprText">The expression to determine the result of.</param>
        /// <param name="index">The 0-based absolute index into the file where the expression should be evaluated within the module.</param>
        public IAnalysisResult GetValueByIndex(string exprText, int index)
        {
            // do a binary search to determine what node we're in
            List<int> keys = _body.Children.Select(x => x.Key).ToList();
            int searchIndex = keys.BinarySearch(index);
            if(searchIndex < 0)
            {
                searchIndex = ~searchIndex;
                if (searchIndex > 0)
                    searchIndex--;
            }

            int key = keys[searchIndex];

            // TODO: need to handle multiple results of the same name
            AstNode containingNode = _body.Children[key];
            if(containingNode != null)
            {
                if(containingNode is IFunctionResult)
                {
                    // Check for local vars, types, and constants
                    IFunctionResult func = containingNode as IFunctionResult;
                    IAnalysisResult res;
                    if(func.Variables.TryGetValue(exprText, out res) ||
                       func.Types.TryGetValue(exprText, out res) ||
                       func.Constants.TryGetValue(exprText, out res))
                    {
                        return res;
                    }
                }

                if(_body is IModuleResult)
                {
                    // check for module vars, types, and constants (and globals defined in this module)
                    IModuleResult mod = _body as IModuleResult;
                    IAnalysisResult res;
                    if(mod.Variables.TryGetValue(exprText, out res) ||
                       mod.Types.TryGetValue(exprText, out res) ||
                       mod.Constants.TryGetValue(exprText, out res) ||
                       mod.GlobalVariables.TryGetValue(exprText, out res) ||
                       mod.GlobalTypes.TryGetValue(exprText, out res) ||
                       mod.GlobalConstants.TryGetValue(exprText, out res))
                    {
                        return res;
                    }

                    // check for cursors in this module
                    if(mod.Cursors.TryGetValue(exprText, out res))
                    {
                        return res;
                    }

                    // check for module functions
                    IFunctionResult funcRes;
                    if(mod.Functions.TryGetValue(exprText, out funcRes))
                    {
                        return funcRes;
                    }
                }

                // TODO: this could probably be done more efficiently by having each GeneroAst load globals and functions into
                // dictionaries stored on the IGeneroProject, instead of in each project entry.
                // However, this does required more upkeep when changes occur. Will look into it...
                if(_projEntry != null && _projEntry is IGeneroProjectEntry)
                {
                    IGeneroProjectEntry genProj = _projEntry as IGeneroProjectEntry;
                    if(genProj.ParentProject != null)
                    {
                        foreach(var projEntry in genProj.ParentProject.ProjectEntries.Where(x => x.Value != genProj))
                        {
                            if(projEntry.Value.Analysis != null &&
                               projEntry.Value.Analysis.Body != null)
                            {
                                IModuleResult modRes = projEntry.Value.Analysis.Body as IModuleResult;
                                if(modRes != null)
                                {
                                    // check global vars, types, and constants
                                    IAnalysisResult res;
                                    if(modRes.GlobalVariables.TryGetValue(exprText, out res) ||
                                       modRes.GlobalTypes.TryGetValue(exprText, out res) ||
                                       modRes.GlobalConstants.TryGetValue(exprText, out res))
                                    {
                                        return res;
                                    }

                                    // check project functions
                                    IFunctionResult funcRes;
                                    if (modRes.Functions.TryGetValue(exprText, out funcRes))
                                    {
                                        if(funcRes.AccessModifier == AccessModifier.Public)
                                            return funcRes;
                                    }

                                    // check for cursors in this module
                                    if (modRes.Cursors.TryGetValue(exprText, out res))
                                    {
                                        return res;
                                    }
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


            return null;
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
            //if (exprText.Length == 0)
            //{
            //    return GetAllAvailableMembersByIndex(index, options);
            //}

            //var scope = FindScope(index);
            //var privatePrefix = GetPrivatePrefixClassName(scope);

            //var expr = Statement.GetExpression(GetAstFromText(exprText, privatePrefix).Body);
            //if (expr is ConstantExpression && ((ConstantExpression)expr).Value is int)
            //{
            //    // no completions on integer ., the user is typing a float
            //    return new MemberResult[0];
            //}

            //var unit = GetNearestEnclosingAnalysisUnit(scope);
            //var lookup = new ExpressionEvaluator(unit.CopyForEval(), scope, mergeScopes: true).Evaluate(expr);
            //return GetMemberResults(lookup, scope, options);
        }

        private IEnumerable<MemberResult> GetKeywordMembers(GetMemberOptions options/*, InterpreterScope scope*/)
        {
            IEnumerable<string> keywords = null;

            keywords = Tokens.Keywords.Keys;

            //if (options.ExpressionKeywords())
            //{
            //    // keywords available in any context
            //    keywords = PythonKeywords.Expression(ProjectState.LanguageVersion);
            //}
            //else
            //{
            //    keywords = Enumerable.Empty<string>();
            //}

            //if (options.StatementKeywords())
            //{
            //    keywords = keywords.Union(PythonKeywords.Statement(ProjectState.LanguageVersion));
            //}

            //if (!(scope is FunctionScope))
            //{
            //    keywords = keywords.Except(PythonKeywords.InvalidOutsideFunction(ProjectState.LanguageVersion));
            //}

            return keywords.Select(kw => new MemberResult(kw, GeneroMemberType.Keyword, this));
        }

        /// <summary>
        /// Gets the variables the given expression evaluates to.  Variables include parameters, locals, and fields assigned on classes, modules and instances.
        /// 
        /// Variables are classified as either definitions or references.  Only parameters have unique definition points - all other types of variables
        /// have only one or more references.
        /// 
        /// index is a 0-based absolute index into the file.
        /// </summary>
        public IEnumerable<IAnalysisVariable> GetVariablesByIndex(string exprText, int index)
        {
            List<IAnalysisVariable> vars = new List<IAnalysisVariable>();

            // do a binary search to determine what node we're in
            List<int> keys = _body.Children.Select(x => x.Key).ToList();
            int searchIndex = keys.BinarySearch(index);
            if (searchIndex < 0)
            {
                searchIndex = ~searchIndex;
                if (searchIndex > 0)
                    searchIndex--;
            }

            int key = keys[searchIndex];

            AstNode containingNode = _body.Children[key];
            if (containingNode != null)
            {
                if (containingNode is IFunctionResult)
                {
                    // Check for local vars, types, and constants
                    IFunctionResult func = containingNode as IFunctionResult;
                    IAnalysisResult res;

                    if (func.Variables.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }
                     
                    if(func.Types.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }
                    
                    if(func.Constants.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }
                }

                if (_body is IModuleResult)
                {
                    // check for module vars, types, and constants (and globals defined in this module)
                    IModuleResult mod = _body as IModuleResult;
                    IAnalysisResult res;

                    if (mod.Variables.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.Types.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.Constants.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.GlobalVariables.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.GlobalTypes.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    if (mod.GlobalConstants.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    // check for cursors in this module
                    if (mod.Cursors.TryGetValue(exprText, out res))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(res), VariableType.Definition));
                    }

                    // check for module functions
                    IFunctionResult funcRes;
                    if (mod.Functions.TryGetValue(exprText, out funcRes))
                    {
                        vars.Add(new AnalysisVariable(this.ResolveLocation(funcRes), VariableType.Definition));
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
                                    // check global vars, types, and constants
                                    IAnalysisResult res;
                                    if (modRes.GlobalVariables.TryGetValue(exprText, out res))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                    }

                                    if (modRes.GlobalTypes.TryGetValue(exprText, out res))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                    }

                                    if (modRes.GlobalConstants.TryGetValue(exprText, out res))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                    }

                                    // check for cursors in this module
                                    if (modRes.Cursors.TryGetValue(exprText, out res))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(res), VariableType.Definition));
                                    }

                                    // check for module functions
                                    IFunctionResult funcRes;
                                    if (modRes.Functions.TryGetValue(exprText, out funcRes))
                                    {
                                        vars.Add(new AnalysisVariable(projEntry.Value.Analysis.ResolveLocation(funcRes), VariableType.Definition));
                                    }
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


            return vars;
        }
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
        public Func<int, IEnumerable<MemberResult>> AdditionalCompletionsProvider { get; private set; }
        public bool IsBreakingStateOnFirstPrevious { get; set; }

        public CompletionPossibility(IEnumerable<TokenKindWithConstraint> keywordCompletions, Func<int, IEnumerable<MemberResult>> additionalCompletionsProvider = null)
        {
            KeywordCompletions = keywordCompletions;
            AdditionalCompletionsProvider = additionalCompletionsProvider;
        }
    }

    public class CategoryCompletionPossiblity : CompletionPossibility
    {
        public HashSet<TokenCategory> Categories { get; private set; }

        public CategoryCompletionPossiblity(HashSet<TokenCategory> categories, IEnumerable<TokenKindWithConstraint> keywordCompletions, Func<int, IEnumerable<MemberResult>> additionalCompletionsProvider = null)
            : base(keywordCompletions, additionalCompletionsProvider)
        {
            Categories = categories;
        }

        public bool MatchesCategory(TokenCategory category)
        {
            foreach(var cat in Categories)
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

        public TokenKindCompletionPossiblity(TokenKind kind, IEnumerable<TokenKindWithConstraint> keywordCompletions, Func<int, IEnumerable<MemberResult>> additionalCompletionsProvider = null)
            : base(keywordCompletions, additionalCompletionsProvider)
        {
            Kind = kind;
        }

        public TokenKindCompletionPossiblity(HashSet<TokenKind> kinds, IEnumerable<TokenKindWithConstraint> keywordCompletions, Func<int, IEnumerable<MemberResult>> additionalCompletionsProvider = null)
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
