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

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    /// <summary>
    /// [PUBLIC|PRIVATE] REPORT report-name (argument-list)
    ///  [ define-section ]
    ///  [ output-section ]
    ///  [  sort-section ]
    ///  [ format-section ] 
    /// END REPORT
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_reports_Report_Definition.html
    /// </summary>
    public class ReportBlockNode : FunctionBlockNode
    {
        public ExpressionNode OutputFilename { get; private set; }
        public ExpressionNode OutputProgram { get; private set; }
        public ExpressionNode MarginLeft { get; private set; }
        public ExpressionNode MarginRight { get; private set; }
        public ExpressionNode MarginTop { get; private set; }
        public ExpressionNode MarginBottom { get; private set; }
        public ExpressionNode PageLength { get; private set; }
        public ExpressionNode TopOfPage { get; private set; }
        public List<FglNameExpression> OrderVarNames { get; private set; }

        public override GeneroMemberType FunctionType
        {
            get
            {
                return GeneroMemberType.Report;
            }
        }

        public static bool TryParseNode(Genero4glParser parser, out ReportBlockNode defNode, IModuleResult containingModule)
        {
            defNode = null;
            bool result = false;
            AccessModifier? accMod = null;
            string accModToken = null;

            if (parser.PeekToken(TokenKind.PublicKeyword))
            {
                accMod = AccessModifier.Public;
                accModToken = parser.PeekToken().Value.ToString();
            }
            else if (parser.PeekToken(TokenKind.PrivateKeyword))
            {
                accMod = AccessModifier.Private;
                accModToken = parser.PeekToken().Value.ToString();
            }

            uint lookAheadBy = (uint)(accMod.HasValue ? 2 : 1);
            if (parser.PeekToken(TokenKind.ReportKeyword, lookAheadBy))
            {
                result = true;
                defNode = new ReportBlockNode();
                if (accMod.HasValue)
                {
                    parser.NextToken();
                    defNode.AccessModifier = accMod.Value;
                }
                else
                {
                    defNode.AccessModifier = AccessModifier.Public;
                }

                parser.NextToken(); // move past the Function keyword
                defNode.StartIndex = parser.Token.Span.Start;
                defNode.OrderVarNames = new List<FglNameExpression>();

                // get the name
                if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    defNode.Name = parser.Token.Token.Value.ToString();
                    defNode.LocationIndex = parser.Token.Span.Start;
                    defNode.DecoratorEnd = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError("A report must have a name.");
                }

                if (!parser.PeekToken(TokenKind.LeftParenthesis))
                    parser.ReportSyntaxError("A report must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                // get the parameters
                while (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    string errMsg;
                    if (!defNode.AddArgument(parser.Token, out errMsg))
                    {
                        parser.ReportSyntaxError(errMsg);
                    }
                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();

                    // TODO: probably need to handle "end" "function" case...won't right now
                }

                if (!parser.PeekToken(TokenKind.RightParenthesis))
                    parser.ReportSyntaxError("A report must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                List<List<TokenKind>> breakSequences =
                    new List<List<TokenKind>>(Genero4glAst.ValidStatementKeywords
                        .Where(x => x != TokenKind.EndKeyword && x != TokenKind.ReportKeyword)
                        .Select(x => new List<TokenKind> { x }))
                    { 
                        new List<TokenKind> { TokenKind.EndKeyword, TokenKind.ReportKeyword },
                        new List<TokenKind> { TokenKind.OutputKeyword },
                        new List<TokenKind> { TokenKind.OrderKeyword },
                         new List<TokenKind> { TokenKind.FormatKeyword }
                    };

                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                       !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ReportKeyword, 2)))
                {
                    DefineNode defineNode;
                    TypeDefNode typeNode;
                    ConstantDefNode constNode;
                    bool matchedBreakSequence = false;
                    bool inDefSection = true;
                    switch (parser.PeekToken().Kind)
                    {
                        case TokenKind.TypeKeyword:
                            {
                                if (TypeDefNode.TryParseNode(parser, out typeNode, out matchedBreakSequence, breakSequences) && typeNode != null)
                                {
                                    defNode.Children.Add(typeNode.StartIndex, typeNode);
                                    foreach (var def in typeNode.GetDefinitions())
                                    {
                                        def.Scope = "local type";
                                        if (!defNode.Types.ContainsKey(def.Name))
                                            defNode.Types.Add(def.Name, def);
                                        else
                                            parser.ReportSyntaxError(def.LocationIndex, def.LocationIndex + def.Name.Length, string.Format("Type {0} defined more than once.", def.Name), Severity.Error);
                                    }
                                }
                                break;
                            }
                        case TokenKind.ConstantKeyword:
                            {
                                if (ConstantDefNode.TryParseNode(parser, out constNode, out matchedBreakSequence, breakSequences) && constNode != null)
                                {
                                    defNode.Children.Add(constNode.StartIndex, constNode);
                                    foreach (var def in constNode.GetDefinitions())
                                    {
                                        def.Scope = "local constant";
                                        if (!defNode.Constants.ContainsKey(def.Name))
                                            defNode.Constants.Add(def.Name, def);
                                        else
                                            parser.ReportSyntaxError(def.LocationIndex, def.LocationIndex + def.Name.Length, string.Format("Constant {0} defined more than once.", def.Name), Severity.Error);
                                    }
                                }
                                break;
                            }
                        case TokenKind.DefineKeyword:
                            {
                                if (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences, defNode.BindArgument) && defineNode != null)
                                {
                                    defNode.Children.Add(defineNode.StartIndex, defineNode);
                                    foreach (var def in defineNode.GetDefinitions())
                                        foreach (var vardef in def.VariableDefinitions)
                                        {
                                            vardef.Scope = "local variable";
                                            if (!defNode.Variables.ContainsKey(vardef.Name))
                                                defNode.Variables.Add(vardef.Name, vardef);
                                            else
                                                parser.ReportSyntaxError(vardef.LocationIndex, vardef.LocationIndex + vardef.Name.Length, string.Format("Variable {0} defined more than once.", vardef.Name), Severity.Error);
                                        }
                                }
                                break;
                            }
                        default:
                            inDefSection = false;
                            break;
                    }

                    if (!inDefSection)
                        break;
                }

                if (parser.PeekToken(TokenKind.OutputKeyword))
                {
                    parser.NextToken();
                    bool isValid = true;
                    ExpressionNode expr;
                    while (isValid)
                    {
                        if (parser.PeekToken(TokenKind.ReportKeyword))
                        {
                            parser.NextToken();
                            if (parser.PeekToken(TokenKind.ToKeyword))
                            {
                                parser.NextToken();
                                switch (parser.PeekToken().Kind)
                                {
                                    case TokenKind.ScreenKeyword:
                                    case TokenKind.PrinterKeyword:
                                        parser.NextToken();
                                        break;
                                    case TokenKind.FileKeyword:
                                        parser.NextToken();
                                        if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                            defNode.OutputFilename = expr;
                                        else
                                            parser.ReportSyntaxError("Invalid filename found in report.");
                                        break;
                                    case TokenKind.PipeKeyword:
                                        parser.NextToken();
                                        if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                            defNode.OutputProgram = expr;
                                        else
                                            parser.ReportSyntaxError("Invalid program name found in report.");
                                        if (parser.PeekToken(TokenKind.InKeyword))
                                        {
                                            parser.NextToken();
                                            if ((parser.PeekToken(TokenKind.FormKeyword) || parser.PeekToken(TokenKind.LineKeyword)) &&
                                               parser.PeekToken(TokenKind.ModeKeyword, 2))
                                            {
                                                parser.NextToken();
                                                parser.NextToken();
                                            }
                                            else
                                                parser.ReportSyntaxError("Expected \"form mode\" or \"line mode\" in report.");
                                        }
                                        break;
                                    default:
                                        if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                            defNode.OutputFilename = expr;
                                        else
                                            parser.ReportSyntaxError("Invalid filename found in report.");
                                        break;
                                }
                            }
                            else
                                parser.ReportSyntaxError("Expected \"to\" keyword in report.");
                        }

                        switch (parser.PeekToken().Kind)
                        {
                            case TokenKind.WithKeyword:
                                parser.NextToken();
                                break;
                            case TokenKind.LeftKeyword:
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.MarginKeyword))
                                {
                                    parser.NextToken();
                                    if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                        defNode.MarginLeft = expr;
                                    else
                                        parser.ReportSyntaxError("Invalid margin value found in report.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected \"margin\" keyword in report.");
                                break;
                            case TokenKind.RightKeyword:
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.MarginKeyword))
                                {
                                    parser.NextToken();
                                    if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                        defNode.MarginRight = expr;
                                    else
                                        parser.ReportSyntaxError("Invalid margin value found in report.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected \"margin\" keyword in report.");
                                break;
                            case TokenKind.BottomKeyword:
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.MarginKeyword))
                                {
                                    parser.NextToken();
                                    if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                        defNode.MarginBottom = expr;
                                    else
                                        parser.ReportSyntaxError("Invalid margin value found in report.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected \"margin\" keyword in report.");
                                break;
                            case TokenKind.PageKeyword:
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.LengthKeyword))
                                {
                                    parser.NextToken();
                                    if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                        defNode.PageLength = expr;
                                    else
                                        parser.ReportSyntaxError("Invalid page length value found in report.");
                                }
                                else
                                    parser.ReportSyntaxError("Expected \"length\" keyword in report.");
                                break;
                            case TokenKind.TopKeyword:
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.OfKeyword) && parser.PeekToken(TokenKind.PageKeyword, 2))
                                {
                                    parser.NextToken();
                                    parser.NextToken();
                                    if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                        defNode.TopOfPage = expr;
                                    else
                                        parser.ReportSyntaxError("Invalid top of page value found in report.");
                                }
                                else if (parser.PeekToken(TokenKind.MarginKeyword))
                                {
                                    parser.NextToken();
                                    if (FglExpressionNode.TryGetExpressionNode(parser, out expr))
                                        defNode.MarginTop = expr;
                                    else
                                        parser.ReportSyntaxError("Invalid margin value found in report.");
                                }
                                else
                                    parser.ReportSyntaxError("Invalid token found in report.");
                                break;
                            default:
                                isValid = false;
                                parser.ReportSyntaxError("Invalid token found in output section of report.");
                                break;
                        }

                        if (parser.PeekToken(TokenKind.OrderKeyword) ||
                           parser.PeekToken(TokenKind.FormatKeyword) ||
                           parser.PeekToken(TokenKind.EndOfFile) ||
                           (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ReportKeyword, 2)))
                        {
                            break;
                        }
                    }
                }

                if (parser.PeekToken(TokenKind.OrderKeyword))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.ExternalKeyword))
                        parser.NextToken();
                    if (parser.PeekToken(TokenKind.ByKeyword))
                        parser.NextToken();
                    else
                        parser.ReportSyntaxError("Expected \"by\" keyword in order section of report.");

                    // collect report variables
                    FglNameExpression varName;
                    while (FglNameExpression.TryParseNode(parser, out varName))
                    {
                        defNode.OrderVarNames.Add(varName);
                        if (parser.PeekToken(TokenKind.AscKeyword) || parser.PeekToken(TokenKind.DescKeyword))
                            parser.NextToken();
                        if (parser.PeekToken(TokenKind.Comma))
                            parser.NextToken();
                        else
                            break;
                    }
                }

                List<TokenKind> validExits = new List<TokenKind> { TokenKind.ProgramKeyword, TokenKind.ReportKeyword };

                ReportFormatSection rfs;
                while (ReportFormatSection.TryParseNode(parser, out rfs, containingModule, defNode, null, defNode.StoreReturnStatement, 
                                                        defNode.AddLimitedScopeVariable, validExits) && rfs != null)
                {
                    defNode.Children.Add(rfs.StartIndex, rfs);
                    if (parser.PeekToken(TokenKind.EndOfFile) ||
                        (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.ReportKeyword, 2)))
                    {
                        break;
                    }
                }

                if (!parser.PeekToken(TokenKind.EndOfFile))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.ReportKeyword))
                    {
                        parser.NextToken();
                        defNode.EndIndex = parser.Token.Span.End;
                        defNode.IsComplete = true;
                        defNode.AddLimitedScopeVariable(Genero4glAst.PagenoVariable, defNode.StartIndex, defNode.EndIndex);
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of report definition.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Unexpected end of report definition");
                }
            }
            return result;
        }
    }

    public class ReportFormatSection : AstNode4gl
    {
        private bool _canOutline;
        public FglNameExpression ReportVariable { get; private set; }

        public static bool TryParseNode(Genero4glParser parser, out ReportFormatSection node,
                                 IModuleResult containingModule,
                                 ReportBlockNode reportNode,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
        {
            node = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.FormatKeyword))
            {
                parser.NextToken();
                result = true;
                node = new ReportFormatSection();
                node.StartIndex = parser.Token.Span.Start;

                bool isValid = true;
                while (isValid)
                {
                    if (parser.PeekToken(TokenKind.EveryKeyword) && parser.PeekToken(TokenKind.RowKeyword, 2))
                    {
                        parser.NextToken();
                        parser.NextToken();
                        node._canOutline = false;
                    }
                    else
                    {
                        node._canOutline = true;
                        switch (parser.PeekToken().Kind)
                        {
                            case TokenKind.FirstKeyword:
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.PageKeyword) && parser.PeekToken(TokenKind.HeaderKeyword, 2))
                                {
                                    parser.NextToken();
                                    parser.NextToken();
                                    if (node.DecoratorEnd == 0)
                                        node.DecoratorEnd = parser.Token.Span.End;
                                }
                                else
                                    parser.ReportSyntaxError("Expected \"page header\" in format section of report.");
                                break;
                            case TokenKind.PageKeyword:
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.HeaderKeyword))
                                {
                                    parser.NextToken();
                                    if (node.DecoratorEnd == 0)
                                        node.DecoratorEnd = parser.Token.Span.End;
                                }
                                else if (parser.PeekToken(TokenKind.TrailerKeyword))
                                {
                                    parser.NextToken();
                                    if (node.DecoratorEnd == 0)
                                        node.DecoratorEnd = parser.Token.Span.End;
                                }
                                else
                                    parser.ReportSyntaxError("Expected \"header\" or \"trailer\" in format section of report.");
                                break;
                            case TokenKind.OnKeyword:
                                parser.NextToken();
                                if ((parser.PeekToken(TokenKind.EveryKeyword) || parser.PeekToken(TokenKind.LastKeyword)) &&
                                   parser.PeekToken(TokenKind.RowKeyword, 2))
                                {
                                    parser.NextToken();
                                    parser.NextToken();
                                    if (node.DecoratorEnd == 0)
                                        node.DecoratorEnd = parser.Token.Span.End;
                                }
                                else
                                    parser.ReportSyntaxError("Expected \"every row\" or \"last row\" in format section of report.");
                                break;
                            case TokenKind.BeforeKeyword:
                            case TokenKind.AfterKeyword:
                                parser.NextToken();
                                if (parser.PeekToken(TokenKind.GroupKeyword) &&
                                   parser.PeekToken(TokenKind.OfKeyword, 2))
                                {
                                    parser.NextToken();
                                    parser.NextToken();
                                    // TODO: get report variable
                                    FglNameExpression name;
                                    if (FglNameExpression.TryParseNode(parser, out name))
                                        node.ReportVariable = name;
                                    else
                                        parser.ReportSyntaxError("Invalid name expression found in format section of report.");
                                    if (node.DecoratorEnd == 0)
                                        node.DecoratorEnd = parser.Token.Span.End;
                                }
                                else
                                    parser.ReportSyntaxError("Expected \"group of\" in format section of report.");
                                break;
                            default:
                                isValid = false;
                                break;
                        }

                        if (isValid)
                        {
                            // collect statements
                            FglStatement rptStmt;
                            while (ReportStatementFactory.TryGetStatement(parser, out rptStmt, containingModule, reportNode, new List<Func<PrepareStatement, bool>>(), returnStatementBinder, 
                                                                          limitedScopeVariableAdder, validExitKeywords) && rptStmt != null)
                            {
                                if (rptStmt.StartIndex < 0)
                                    continue;
                                node.Children.Add(rptStmt.StartIndex, rptStmt);
                            }
                        }
                    }
                    node.EndIndex = parser.Token.Span.End;
                }
            }

            return result;
        }

        public override bool CanOutline
        {
            get { return _canOutline; }
        }

        public override int DecoratorEnd { get; set; }
    }

    public class ReportStatementFactory
    {
        public static HashSet<TokenKind> StatementStartKeywords = new HashSet<TokenKind>
        {
            TokenKind.PrintKeyword, TokenKind.PrintxKeyword, TokenKind.NeedKeyword, TokenKind.PauseKeyword, TokenKind.SkipKeyword
        };

        private static bool TryGetReportStatement(IParser parser, out FglStatement node, ReportBlockNode reportNode, bool returnFalseInsteadOfErrors = false)
        {
            bool result = false;
            node = null;

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.PrintKeyword:
                    {
                        PrintStatement prtStmt;
                        if ((result = PrintStatement.TryParseNode(parser, out prtStmt, reportNode)))
                            node = prtStmt;
                        break;
                    }
                case TokenKind.PrintxKeyword:
                    {
                        PrintxStatement prtxStmt;
                        if ((result = PrintxStatement.TryParseNode(parser, out prtxStmt)))
                            node = prtxStmt;
                        break;
                    }
                case TokenKind.NeedKeyword:
                    {
                        NeedStatement needStmt;
                        if ((result = NeedStatement.TryParseNode(parser, out needStmt)))
                            node = needStmt;
                        break;
                    }
                case TokenKind.PauseKeyword:
                    {
                        PauseStatement pauseStmt;
                        if ((result = PauseStatement.TryParseNode(parser, out pauseStmt)))
                            node = pauseStmt;
                        break;
                    }
                case TokenKind.SkipKeyword:
                    {
                        SkipStatement skipStmt;
                        if ((result = SkipStatement.TryParseNode(parser, out skipStmt)))
                            node = skipStmt;
                        break;
                    }
                default:
                    result = false;
                    break;
            }

            return result;
        }

        public static bool TryGetStatement(Genero4glParser parser, out FglStatement node,
                                 IModuleResult containingModule,
                                 ReportBlockNode reportNode,
                                 List<Func<PrepareStatement, bool>> prepStatementBinders,
                                 Func<ReturnStatement, ParserResult> returnStatementBinder = null,
                                 Action<IAnalysisResult, int, int> limitedScopeVariableAdder = null,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
        {
            bool result = false;
            node = null;

            if (!(result = TryGetReportStatement(parser, out node, reportNode)))
            {
                List<ContextStatementFactory> csfs = new List<ContextStatementFactory>();
                if (contextStatementFactories != null)
                    csfs.AddRange(contextStatementFactories);
                csfs.Add((x) =>
                {
                    FglStatement testNode;
                    TryGetReportStatement(x, out testNode, reportNode, true);
                    return testNode;
                });
                result = parser.StatementFactory.TryParseNode(parser, out node, containingModule, prepStatementBinders, returnStatementBinder, 
                                                              limitedScopeVariableAdder, false, validExitKeywords, csfs,
                    new ExpressionParsingOptions
                    {
                        AllowStarParam = true,
                        AdditionalExpressionParsers = new ExpressionParser[] { (x) => ParseAggregateReportFunction(x, reportNode) }
                    });
            }

            return result;
        }

        internal static ExpressionNode ParseAggregateReportFunction(IParser parser, ReportBlockNode reportNode)
        {
            ExpressionNode node = null;

            if(reportNode != null)
            {
                var tokStr = parser.PeekToken().Value.ToString();
                if(reportNode.Variables.ContainsKey(tokStr))
                {
                    switch (tokStr.ToLower())
                    {
                        case "group":
                            {
                                var nextTokKind = parser.PeekToken(2).Kind;
                                if(nextTokKind != TokenKind.CountKeyword &&
                                   nextTokKind != TokenKind.PercentKeyword &&
                                   nextTokKind != TokenKind.SumKeyword &&
                                   nextTokKind != TokenKind.AvgKeyword &&
                                   nextTokKind != TokenKind.MinKeyword &&
                                   nextTokKind != TokenKind.MaxKeyword)
                                {
                                    return null;
                                }
                                break;
                            }
                        case "count":
                        case "percent":
                        case "sum":
                        case "avg":
                        case "min":
                        case "max":
                            {
                                if (!parser.PeekToken(TokenKind.LeftParenthesis, 2))
                                    return null;
                                break;
                            }
                    }
                }
            }

            if (parser.PeekToken(TokenKind.GroupKeyword))
            {
                parser.NextToken();
                node = new TokenExpressionNode(parser.Token);
            }

            switch(parser.PeekToken().Kind)
            {
                case TokenKind.CountKeyword:
                case TokenKind.PercentKeyword:
                    {
                        parser.NextToken();
                        if (node == null)
                            node = new TokenExpressionNode(parser.Token);
                        else
                            node.AppendExpression(new TokenExpressionNode(parser.Token));
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            node.AppendExpression(new TokenExpressionNode(parser.Token));
                            if(parser.PeekToken(TokenKind.Multiply))
                            {
                                parser.NextToken();
                                node.AppendExpression(new TokenExpressionNode(parser.Token));
                                if(parser.PeekToken(TokenKind.RightParenthesis))
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                else
                                    parser.ReportSyntaxError("Expected right-paren in report aggregate function.");
                            }
                            else
                                parser.ReportSyntaxError("Expected '*' in report aggregate function.");
                        }
                        else
                            parser.ReportSyntaxError("Expected left-paren in report aggregate function.");

                        // get the optional where clause
                        if (parser.PeekToken(TokenKind.WhereKeyword))
                        {
                            parser.NextToken();
                            node.AppendExpression(new TokenExpressionNode(parser.Token));
                            ExpressionNode whereExpr;
                            if (FglExpressionNode.TryGetExpressionNode(parser, out whereExpr))
                                node.AppendExpression(whereExpr);
                            else
                                parser.ReportSyntaxError("Invalid expression in report aggregate function.");
                        }
                        break;
                    }
                case TokenKind.SumKeyword:
                case TokenKind.AvgKeyword:
                case TokenKind.MinKeyword:
                case TokenKind.MaxKeyword:
                    {
                        parser.NextToken();
                        if (node == null)
                            node = new TokenExpressionNode(parser.Token);
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            node.AppendExpression(new TokenExpressionNode(parser.Token));
                            ExpressionNode expr;
                            if(FglExpressionNode.TryGetExpressionNode(parser, out expr))
                            {
                                node.AppendExpression(expr);
                                if (parser.PeekToken(TokenKind.RightParenthesis))
                                {
                                    parser.NextToken();
                                    node.AppendExpression(new TokenExpressionNode(parser.Token));
                                }
                                else
                                    parser.ReportSyntaxError("Expected right-paren in report aggregate function.");
                            }
                            else
                                parser.ReportSyntaxError("Invalid expression in report aggregate function.");
                        }
                        else
                            parser.ReportSyntaxError("Expected left-paren in report aggregate function.");

                        // get the optional where clause
                        if (parser.PeekToken(TokenKind.WhereKeyword))
                        {
                            parser.NextToken();
                            node.AppendExpression(new TokenExpressionNode(parser.Token));
                            ExpressionNode whereExpr;
                            if (FglExpressionNode.TryGetExpressionNode(parser, out whereExpr))
                                node.AppendExpression(whereExpr);
                            else
                                parser.ReportSyntaxError("Invalid expression in report aggregate function.");
                        }
                        break;
                    }
            }

            return node;
        }
    }
}
