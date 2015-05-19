using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public enum TokenKind
    {
        EndOfFile = -1,
        Error    = 0,
        NewLine  = 1,
        Indent   = 2,
        Dedent   = 3,
        Comment  = 4,
        Name     = 8,
        Constant = 9,
        Dot      = 10,

        // numeric expression operators
        FirstOperator = Add,
        LastOperator = DoubleBar,
        Add      = 32,
        Subtract = 33,
        Multiply = 34,
        Divide   = 35,
        Assign   = 36,
        Power    = 37,

        // boolean expression operators
        DoubleEquals       = 38,
        LessThan           = 39,
        GreaterThan        = 40,
        LessThanOrEqual    = 41,
        GreaterThanOrEqual = 42,
        Equals             = 43,
        NotEquals          = 44,
        NotEqualsLTGT      = 45,

        // string expression operators
        DoubleBar        = 46,

        // other non-keyword tokens
        Ampersand        = 47,
        LeftParenthesis  = 48,
        RightParenthesis = 49,
        LeftBracket      = 50,
        RightBracket     = 51,
        LeftBrace        = 52,
        RightBrace       = 53,
        Comma            = 54,
        Colon            = 55,
        BackQuote        = 56,
        Semicolon        = 57,
        

        FirstLanguageKeyword = AbsoluteKeyword,
        LastLanguageKeyword  = YellowKeyword,
        AbsoluteKeyword      = 75,
        AcceptKeyword,
        ActionKeyword,
        AfterKeyword,
        AggregateKeyword,
        AllKeyword,
        AllocateKeyword,
        AllRowsKeyword,
        AlterKeyword,
        AndKeyword,
        AnyKeyword,
        AppendKeyword,
        ArrayKeyword,
        AsciiKeyword,
        AscKeyword,
        AsKeyword,
        AtKeyword,
        AttributeKeyword,
        AttributesKeyword,
        AutoKeyword,
        AverageKeyword,
        AvgKeyword,
        BaseKeyword,
        BeforeKeyword,
        BeginKeyword,
        BetweenKeyword,
        BigintKeyword,
        BlackKeyword,
        BlinkKeyword,
        BlueKeyword,
        BoldKeyword,
        BooleanKeyword,
        BorderKeyword,
        BottomKeyword,
        BufferedKeyword,
        ButtonKeyword,
        ByKeyword,
        ByteKeyword,
        CacheKeyword,
        CallKeyword,
        CancelKeyword,
        CaseKeyword,
        CastKeyword,
        CatchKeyword,
        ChangeKeyword,
        CharacterKeyword,
        CharKeyword,
        CharLengthKeyword,
        CheckboxKeyword,
        CheckKeyword,
        CircuitKeyword,
        ClearKeyword,
        ClippedKeyword,
        CloseKeyword,
        ClusterKeyword,
        CollapseKeyword,
        ColumnKeyword,
        ColumnsKeyword,
        CommandKeyword,
        CommentsKeyword,
        CommitKeyword,
        CommittedKeyword,
        ConstantKeyword,
        ConstrainedKeyword,
        ConstraintKeyword,
        ConstructKeyword,
        ContinueKeyword,
        CopyKeyword,
        CountKeyword,
        CrcolsKeyword,
        CreateKeyword,
        CurrentKeyword,
        CursorKeyword,
        CyanKeyword,
        DatabaseKeyword,
        DateKeyword,
        DatetimeKeyword,
        DayKeyword,
        DeallocateKeyword,
        DecimalKeyword,
        DecKeyword,
        DeclareKeyword,
        DecodeKeyword,
        DefaultKeyword,
        DefaultsKeyword,
        DeferKeyword,
        DefineKeyword,
        DeleteKeyword,
        DelimiterKeyword,
        DescKeyword,
        DescribeKeyword,
        DialogKeyword,
        DimensionKeyword,
        DimensionsKeyword,
        DimKeyword,
        DirtyKeyword,
        DisplayKeyword,
        DistinctKeyword,
        DoKeyword,
        DoubleKeyword,
        DownKeyword,
        Drag_EnterKeyword,
        Drag_FinishKeyword,
        Drag_OverKeyword,
        Drag_StartKeyword,
        DropKeyword,
        DynamicKeyword,
        EditKeyword,
        ElseKeyword,
        EndifKeyword,
        EndKeyword,
        ErrorKeyword,
        EscapeKeyword,
        EveryKeyword,
        ExclusiveKeyword,
        ExecKeyword,
        ExecuteKeyword,
        ExistsKeyword,
        ExitKeyword,
        ExpandKeyword,
        ExplainKeyword,
        ExtendKeyword,
        ExtentKeyword,
        ExternalKeyword,
        FalseKeyword,
        FetchKeyword,
        FglKeyword,
        Field_TouchedKeyword,
        FieldKeyword,
        FileKeyword,
        FinishKeyword,
        First_RowsKeyword,
        FirstKeyword,
        FloatKeyword,
        FlushKeyword,
        ForeachKeyword,
        ForKeyword,
        FormatKeyword,
        FormKeyword,
        FormonlyKeyword,
        FoundKeyword,
        FractionKeyword,
        FreeKeyword,
        FromKeyword,
        FunctionKeyword,
        Get_FldbufKeyword,
        GlobalsKeyword,
        GoKeyword,
        GotoKeyword,
        GreenKeyword,
        GridKeyword,
        GroupKeyword,
        HavingKeyword,
        HboxKeyword,
        HeaderKeyword,
        HelpKeyword,
        HideKeyword,
        HoldKeyword,
        HourKeyword,
        IdleKeyword,
        IfdefKeyword,
        IfKeyword,
        IifKeyword,
        ImmediateKeyword,
        ImportKeyword,
        IncludeKeyword,
        IndexKeyword,
        IndicatorKeyword,
        InfieldKeyword,
        InitializeKeyword,
        InKeyword,
        InnerKeyword,
        InOutKeyword,
        InputKeyword,
        InsertKeyword,
        InstanceOfKeyword,
        InstructionsKeyword,
        Int_FlagKeyword,
        IntegerKeyword,
        InterruptKeyword,
        IntervalKeyword,
        IntKeyword,
        IntoKeyword,
        InvisibleKeyword,
        IsKeyword,
        IsolationKeyword,
        JavaKeyword,
        JoinKeyword,
        KeepKeyword,
        KeyKeyword,
        LabelKeyword,
        LastKeyword,
        LayoutKeyword,
        LeftKeyword,
        LengthKeyword,
        LetKeyword,
        LikeKeyword,
        LimitKeyword,
        LineKeyword,
        LinenoKeyword,
        LinesKeyword,
        LoadKeyword,
        LocateKeyword,
        LockKeyword,
        LogKeyword,
        LongKeyword,
        LstrKeyword,
        MagentaKeyword,
        MainKeyword,
        MarginKeyword,
        MatchesKeyword,
        MaxKeyword,
        MaxCountKeyword,
        MdyKeyword,
        MemoryKeyword,
        MenuKeyword,
        MessageKeyword,
        MiddleKeyword,
        MinKeyword,
        MinuteKeyword,
        ModeKeyword,
        ModKeyword,
        ModuleKeyword,
        MoneyKeyword,
        MonthKeyword,
        NameKeyword,
        NcharKeyword,
        NeedKeyword,
        NewKeyword,
        NextKeyword,
        NoKeyword,
        NormalKeyword,
        NotfoundKeyword,
        NotKeyword,
        NowKeyword,
        NullKeyword,
        NumericKeyword,
        NvarcharKeyword,
        NvlKeyword,
        OffKeyword,
        OfKeyword,
        OnKeyword,
        OpenKeyword,
        OptionKeyword,
        OptionsKeyword,
        OrderKeyword,
        OrdKeyword,
        OrKeyword,
        OtherwiseKeyword,
        OuterKeyword,
        OutKeyword,
        OutputKeyword,
        PageKeyword,
        PagenoKeyword,
        PauseKeyword,
        PercentKeyword,
        PipeKeyword,
        PrecisionKeyword,
        PrepareKeyword,
        PreviousKeyword,
        PrinterKeyword,
        PrintKeyword,
        PrintxKeyword,
        PriorKeyword,
        PrivateKeyword,
        ProgramKeyword,
        PromptKeyword,
        PublicKeyword,
        PutKeyword,
        Quit_FlagKeyword,
        QuitKeyword,
        RaiseKeyword,
        ReadKeyword,
        RealKeyword,
        RecordKeyword,
        RedKeyword,
        RelativeKeyword,
        RemoveKeyword,
        RenameKeyword,
        ReoptimizationKeyword,
        RepeatableKeyword,
        RepeatKeyword,
        ReportKeyword,
        RequiredKeyword,
        ResizeKeyword,
        ReturningKeyword,
        ReturnKeyword,
        ReverseKeyword,
        RightKeyword,
        RollbackKeyword,
        RowKeyword,
        RowsKeyword,
        RunKeyword,
        SchemaKeyword,
        ScreenKeyword,
        ScrollKeyword,
        SecondKeyword,
        SelectKeyword,
        SetKeyword,
        SfmtKeyword,
        ShareKeyword,
        ShowKeyword,
        ShortKeyword,
        SizeKeyword,
        SizepolicyKeyword,
        SkipKeyword,
        SleepKeyword,
        SmallfloatKeyword,
        SmallintKeyword,
        SpaceKeyword,
        SpacesKeyword,
        SqlErrMessageKeyword,
        SqlerrorKeyword,
        SqlKeyword,
        SqlStateKeyword,
        SqlwarningKeyword,
        StabilityKeyword,
        StartKeyword,
        StatisticsKeyword,
        StatusKeyword,
        StepKeyword,
        StopKeyword,
        StringKeyword,
        SumKeyword,
        TabindexKeyword,
        TableKeyword,
        TablesKeyword,
        TagKeyword,
        TempKeyword,
        TerminateKeyword,
        TextKeyword,
        ThenKeyword,
        ThroughKeyword,
        ThruKeyword,
        TimeKeyword,
        TinyintKeyword,
        TodayKeyword,
        ToKeyword,
        TopKeyword,
        TrailerKeyword,
        TrueKeyword,
        TryKeyword,
        TypeKeyword,
        UnbufferedKeyword,
        UnconstrainedKeyword,
        UndefKeyword,
        UnderlineKeyword,
        UnionKeyword,
        UniqueKeyword,
        UnitsKeyword,
        UnloadKeyword,
        UpdateKeyword,
        UpKeyword,
        UserKeyword,
        UsingKeyword,
        ValidateKeyword,
        ValuecheckedKeyword,
        ValueKeyword,
        ValuesKeyword,
        valueuncheckedKeyword,
        VarcharKeyword,
        VboxKeyword,
        ViewKeyword,
        WaitingKeyword,
        WaitKeyword,
        WarningKeyword,
        WeekdayKeyword,
        WheneverKeyword,
        WhenKeyword,
        WhereKeyword,
        WhileKeyword,
        WhiteKeyword,
        WindowKeyword,
        WithKeyword,
        WithoutKeyword,
        WordwrapKeyword,
        WorkKeyword,
        WrapKeyword,
        YearKeyword,
        YellowKeyword,

        NLToken,
        ExplicitLineJoin
    }

    internal static class Tokens
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token EndOfFileToken = new VerbatimToken(TokenKind.EndOfFile, "", "<eof>");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token ImpliedNewLineToken = new VerbatimToken(TokenKind.NewLine, "", "<newline>");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token NewLineToken = new VerbatimToken(TokenKind.NewLine, "\n", "<newline>");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token NewLineTokenCRLF = new VerbatimToken(TokenKind.NewLine, "\r\n", "<newline>");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token NewLineTokenCR = new VerbatimToken(TokenKind.NewLine, "\r", "<newline>");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token NLToken = new VerbatimToken(TokenKind.NLToken, "\n", "<NL>");  // virtual token used for error reporting
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token NLTokenCRLF = new VerbatimToken(TokenKind.NLToken, "\r\n", "<NL>");  // virtual token used for error reporting
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token NLTokenCR = new VerbatimToken(TokenKind.NLToken, "\r", "<NL>");  // virtual token used for error reporting

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token IndentToken = new DentToken(TokenKind.Indent, "<indent>");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dedent")]
        public static readonly Token DedentToken = new DentToken(TokenKind.Dedent, "<dedent>");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token DotToken = new SymbolToken(TokenKind.Dot, ".");

        // Generated
        private static readonly Token symAddToken = new OperatorToken(TokenKind.Add, "+", 4);
        private static readonly Token symSubtractToken = new OperatorToken(TokenKind.Subtract, "-", 4);
        private static readonly Token symMultiplyToken = new OperatorToken(TokenKind.Multiply, "*", 5);
        private static readonly Token symDivideToken = new OperatorToken(TokenKind.Divide, "/", 5);
        private static readonly Token symPowerToken = new OperatorToken(TokenKind.Power, "**", 6);
        private static readonly Token symConcatToken = new OperatorToken(TokenKind.DoubleBar, "||", 6);
        private static readonly Token symNotEqualGTLT = new OperatorToken(TokenKind.NotEqualsLTGT, "<>", 6);
        private static readonly Token symNotEqual = new OperatorToken(TokenKind.NotEquals, "!=", 6);
        private static readonly Token symLessThan = new OperatorToken(TokenKind.LessThan, "<", 6);
        private static readonly Token symLessThanEquals = new OperatorToken(TokenKind.LessThanOrEqual, "<=", 6);
        private static readonly Token symGreaterThan = new OperatorToken(TokenKind.GreaterThan, ">", 6);
        private static readonly Token symGreaterThanEquals = new OperatorToken(TokenKind.GreaterThanOrEqual, ">=", 6);
        private static readonly Token symLeftParenthesisToken = new SymbolToken(TokenKind.LeftParenthesis, "(");
        private static readonly Token symRightParenthesisToken = new SymbolToken(TokenKind.RightParenthesis, ")");
        private static readonly Token symLeftBracketToken = new SymbolToken(TokenKind.LeftBracket, "[");
        private static readonly Token symRightBracketToken = new SymbolToken(TokenKind.RightBracket, "]");
        private static readonly Token symLeftBraceToken = new SymbolToken(TokenKind.LeftBrace, "{");
        private static readonly Token symRightBraceToken = new SymbolToken(TokenKind.RightBrace, "}");
        private static readonly Token symCommaToken = new SymbolToken(TokenKind.Comma, ",");
        private static readonly Token symColonToken = new SymbolToken(TokenKind.Colon, ":");
        private static readonly Token symSemicolonToken = new SymbolToken(TokenKind.Semicolon, ";");
        private static readonly Token symDoubleEqualsToken = new OperatorToken(TokenKind.DoubleEquals, "==", -1);
        private static readonly Token symEqualsToken = new OperatorToken(TokenKind.Equals, "=", -1);
        private static readonly Token symAssign = new OperatorToken(TokenKind.Assign, ":=", -1);
        private static readonly Token symAmpersand = new OperatorToken(TokenKind.Ampersand, "&", -1);

        private static Dictionary<string, TokenKind> _keywords;
        public static Dictionary<string, TokenKind> Keywords
        {
            get
            {
                if(_keywords == null)
                {
                    InitializeTokens();
                }
                return _keywords;
            }
        }

        private static Dictionary<TokenKind, string> _tokenKinds;
        public static Dictionary<TokenKind, string> TokenKinds
        {
            get
            {
                if (_tokenKinds == null)
                {
                    InitializeTokens();
                }
                return _tokenKinds;
            }
        }

        private static object _tokLock = new object();

        private static void InitializeTokens()
        {
            lock (_tokLock)
            {
                if (_keywords == null)
                {
                    var keywords = Enum.GetNames(typeof(TokenKind));
                    var values = Enum.GetValues(typeof(TokenKind)).OfType<TokenKind>().ToArray();
                    _keywords = new Dictionary<string, TokenKind>(keywords.Length, StringComparer.OrdinalIgnoreCase);
                    _tokenKinds = new Dictionary<TokenKind, string>(keywords.Length);
                    for (int i = 0; i < keywords.Length; i++)
                    {
                        if (keywords[i].EndsWith("Keyword") && !keywords[i].Contains("Language"))
                        {
                            string key = keywords[i].Replace("Keyword", string.Empty);
                            TokenKind val = (TokenKind)values[i];
                            if (!_keywords.ContainsKey(key))
                            {
                                _keywords.Add(key.ToLower(), val);
                                _tokenKinds.Add(val, key.ToLower());
                            }
                            else
                            {
                                // This should never happen, since we're now handling the race condition
                                int j = 0;
                            }
                        }
                    }
                }
            }
        }

        public static Token GetToken(string possibleKeyword)
        {
            Token tok = null;
            InitializeTokens();
            TokenKind tryKind;
            if (_keywords.TryGetValue(possibleKeyword, out tryKind))
            {
                tok = new SymbolToken(tryKind, possibleKeyword);
            }
            return tok;
        }

        public static Token AmpersandToken
        {
            get { return symAmpersand; }
        }

        public static Token PowerToken
        {
            get { return symPowerToken; }
        }

        public static Token ConcatToken
        {
            get { return symConcatToken; }
        }

        public static Token NotEqualsToken
        {
            get { return symNotEqual; }
        }

        public static Token NotEqualsLTGTToken
        {
            get { return symNotEqualGTLT; }
        }

        public static Token LessThanToken
        {
            get { return symLessThan; }
        }

        public static Token LessThanEqualToken
        {
            get { return symLessThanEquals; }
        }

        public static Token GreaterThanToken
        {
            get { return symGreaterThan; }
        }

        public static Token GreaterThanEqualToken
        {
            get { return symGreaterThanEquals; }
        }

        public static Token LeftParenthesisToken
        {
            get { return symLeftParenthesisToken; }
        }

        public static Token RightParenthesisToken
        {
            get { return symRightParenthesisToken; }
        }

        public static Token LeftBracketToken
        {
            get { return symLeftBracketToken; }
        }

        public static Token RightBracketToken
        {
            get { return symRightBracketToken; }
        }

        public static Token LeftBraceToken
        {
            get { return symLeftBraceToken; }
        }

        public static Token RightBraceToken
        {
            get { return symRightBraceToken; }
        }

        public static Token CommaToken
        {
            get { return symCommaToken; }
        }

        public static Token ColonToken
        {
            get { return symColonToken; }
        }

        public static Token SemicolonToken
        {
            get { return symSemicolonToken; }
        }

        public static Token EqualsToken
        {
            get { return symEqualsToken; }
        }

        public static Token DoubleEqualsToken
        {
            get { return symDoubleEqualsToken; }
        }

        public static Token AssignToken
        {
            get { return symAssign; }
        }

        public static Token AddToken
        {
            get { return symAddToken; }
        }

        public static Token SubtractToken
        {
            get { return symSubtractToken; }
        }

        public static Token MultiplyToken
        {
            get { return symMultiplyToken; }
        }

        public static Token DivideToken
        {
            get { return symDivideToken; }
        }
    }
}
