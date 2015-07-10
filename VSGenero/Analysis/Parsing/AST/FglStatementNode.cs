using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public abstract class FglStatement : AstNode
    {
    }

    public delegate FglStatement ContextStatementFactory(Parser parser);

    public class FglStatementFactory
    {
        public bool TryParseNode(Parser parser, out FglStatement node,
                                 IModuleResult containingModule,
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 bool returnStatementsOnly = false,
                                 List<TokenKind> validExitKeywords = null,
                                 IEnumerable<ContextStatementFactory> contextStatementFactories = null)
        {
            node = null;
            bool result = false;

            if(returnStatementsOnly)
            {
                if(parser.PeekToken(TokenKind.ReturnKeyword))
                {
                    ReturnStatement retStmt;
                    if ((result = ReturnStatement.TryParseNode(parser, out retStmt)))
                    {
                        node = retStmt;
                    }
                }
                return result;
            }

            if(contextStatementFactories != null)
            {
                foreach (var context in contextStatementFactories)
                {
                    node = context(parser);
                    if (node != null)
                        break;
                }
                if (node != null)
                {
                    return true;
                }
            }

            switch (parser.PeekToken().Kind)
            {
                case TokenKind.LetKeyword:
                    {
                        LetStatement letStmt;
                        if ((result = LetStatement.TryParseNode(parser, out letStmt)))
                        {
                            node = letStmt;
                        }
                        break;
                    }
                case TokenKind.DeclareKeyword:
                    {
                        DeclareStatement declStmt;
                        if ((result = DeclareStatement.TryParseNode(parser, out declStmt, containingModule)))
                        {
                            node = declStmt;
                        }
                        break;
                    }
                case TokenKind.DeferKeyword:
                    {
                        DeferStatementNode deferStmt;
                        if ((result = DeferStatementNode.TryParseNode(parser, out deferStmt)))
                        {
                            node = deferStmt;
                        }
                        break;
                    }
                case TokenKind.PrepareKeyword:
                    {
                        PrepareStatement prepStmt;
                        if ((result = PrepareStatement.TryParseNode(parser, out prepStmt, containingModule)))
                        {
                            node = prepStmt;
                            if (prepStatementBinder != null)
                                prepStatementBinder(prepStmt);
                            else
                            {
                                int i = 0;
                            }
                        }
                        break;
                    }
                case TokenKind.SqlKeyword:
                    {
                        SqlBlockNode sqlStmt;
                        if ((result = SqlBlockNode.TryParseSqlNode(parser, out sqlStmt)))
                        {
                            node = sqlStmt;
                        }
                        break;
                    }
                case TokenKind.ReturnKeyword:
                    {
                        ReturnStatement retStmt;
                        if((result = ReturnStatement.TryParseNode(parser, out retStmt)))
                        {
                            node = retStmt;
                        }
                        break;
                    }
                case TokenKind.CallKeyword:
                    {
                        CallStatement callStmt;
                        if((result = CallStatement.TryParseNode(parser, out callStmt)))
                        {
                            node = callStmt;
                        }
                        break;
                    }
                case TokenKind.IfKeyword:
                    {
                        IfStatement ifStmt;
                        if ((result = IfStatement.TryParseNode(parser, out ifStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = ifStmt;
                        }
                        break;
                    }
                case TokenKind.WhileKeyword:
                    {
                        WhileStatement whileStmt;
                        if ((result = WhileStatement.TryParseNode(parser, out whileStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = whileStmt;
                        }
                        break;
                    }
                case TokenKind.ExitKeyword:
                    {
                        ExitStatement exitStatement;
                        if((result = ExitStatement.TryParseNode(parser, out exitStatement)))
                        {
                            node = exitStatement;
                        }
                        break;
                    }
                case TokenKind.ContinueKeyword:
                    {
                        ContinueStatement contStmt;
                        if((result = ContinueStatement.TryParseNode(parser, out contStmt)))
                        {
                            node = contStmt;
                        }
                        break;
                    }
                case TokenKind.WheneverKeyword:
                    {
                        WheneverStatement wheneverStmt;
                        if((result = WheneverStatement.TryParseNode(parser, out wheneverStmt)))
                        {
                            node = wheneverStmt;
                        }
                        break;
                    }
                case TokenKind.ForKeyword:
                    {
                        ForStatement forStmt;
                        if ((result = ForStatement.TryParserNode(parser, out forStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = forStmt;
                        }
                        break;
                    }
                case TokenKind.CaseKeyword:
                    {
                        CaseStatement caseStmt;
                        if ((result = CaseStatement.TryParseNode(parser, out caseStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = caseStmt;
                        }
                        break;
                    }
                case TokenKind.InitializeKeyword:
                    {
                        InitializeStatement initStmt;
                        if((result = InitializeStatement.TryParseNode(parser, out initStmt)))
                        {
                            node = initStmt;
                        }
                        break;
                    }
                case TokenKind.LocateKeyword:
                    {
                        LocateStatement locateStmt;
                        if((result = LocateStatement.TryParseNode(parser, out locateStmt)))
                        {
                            node = locateStmt;
                        }
                        break;
                    }
                case TokenKind.FreeKeyword:
                    {
                        FreeStatement freeStmt;
                        if((result = FreeStatement.TryParseNode(parser, out freeStmt)))
                        {
                            node = freeStmt;
                        }
                        break;
                    }
                case TokenKind.GotoKeyword:
                    {
                        GotoStatement gotoStmt;
                        if((result = GotoStatement.TryParseNode(parser, out gotoStmt)))
                        {
                            node = gotoStmt;
                        }
                        break;
                    }
                case TokenKind.LabelKeyword:
                    {
                        LabelStatement labelStmt;
                        if((result = LabelStatement.TryParseNode(parser, out labelStmt)))
                        {
                            node = labelStmt;
                        }
                        break;
                    }
                case TokenKind.SleepKeyword:
                    {
                        SleepStatement sleepStmt;
                        if((result = SleepStatement.TryParseNode(parser, out sleepStmt)))
                        {
                            node = sleepStmt;
                        }
                        break;
                    }
                case TokenKind.TryKeyword:
                    {
                        TryCatchStatement tryStmt;
                        if ((result = TryCatchStatement.TryParseNode(parser, out tryStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = tryStmt;
                        }
                        break;
                    }
                case TokenKind.ValidateKeyword:
                    {
                        ValidateStatement validateStmt;
                        if((result = ValidateStatement.TryParseNode(parser, out validateStmt)))
                        {
                            node = validateStmt;
                        }
                        break;
                    }
                case TokenKind.OptionsKeyword:
                    {
                        OptionsStatement optionsStmt;
                        if((result = OptionsStatement.TryParseNode(parser, out optionsStmt)))
                        {
                            node = optionsStmt;
                        }
                        break;
                    }
                case TokenKind.ExecuteKeyword:
                    {
                        ExecuteStatement exeStmt;
                        if((result = ExecuteStatement.TryParseNode(parser, out exeStmt)))
                        {
                            node = exeStmt;
                        }
                        break;
                    }
                case TokenKind.OpenKeyword:
                    {
                        OpenStatement openStmt;
                        if((result = OpenStatement.TryParseNode(parser, out openStmt)))
                        {
                            node = openStmt;
                        }
                        break;
                    }
                case TokenKind.FetchKeyword:
                    {
                        FetchStatement fetchStmt;
                        if((result = FetchStatement.TryParseNode(parser, out fetchStmt)))
                        {
                            node = fetchStmt;
                        }
                        break;
                    }
                case TokenKind.CloseKeyword:
                    {
                        CloseStatement closeStmt;
                        if((result = CloseStatement.TryParseNode(parser, out closeStmt)))
                        {
                            node = closeStmt;
                        }
                        break;
                    }
                case TokenKind.ForeachKeyword:
                    {
                        ForeachStatement foreachStmt;
                        if ((result = ForeachStatement.TryParseNode(parser, out foreachStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = foreachStmt;
                        }
                        break;
                    }
                case TokenKind.MessageKeyword:
                    {
                        MessageStatement msgStmt;
                        if((result = MessageStatement.TryParseNode(parser, out msgStmt)))
                        {
                            node = msgStmt;
                        }
                        break;
                    }
                case TokenKind.MenuKeyword:
                    {
                        MenuBlock menuStmt;
                        if ((result = MenuBlock.TryParseNode(parser, out menuStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = menuStmt;
                        }
                        break;
                    }
                case TokenKind.InputKeyword:
                    {
                        InputBlock inputStmt;
                        if ((result = InputBlock.TryParseNode(parser, out inputStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = inputStmt;
                        }
                        break;
                    }
                case TokenKind.ConstructKeyword:
                    {
                        ConstructBlock constructStmt;
                        if ((result = ConstructBlock.TryParseNode(parser, out constructStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = constructStmt;
                        }
                        break;
                    }
                case TokenKind.FlushKeyword:
                    {
                        FlushStatement flushStmt;
                        if((result = FlushStatement.TryParseNode(parser, out flushStmt)))
                        {
                            node = flushStmt;
                        }
                        break;
                    }
                case TokenKind.DisplayKeyword:
                    {
                        DisplayBlock dispStmt;
                        if ((result = DisplayBlock.TryParseNode(parser, out dispStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = dispStmt;
                        }
                        break;
                    }
                case TokenKind.PromptKeyword:
                    {
                        PromptStatement promptStmt;
                        if((result = PromptStatement.TryParseNode(parser, out promptStmt, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = promptStmt;
                        }
                        break;
                    }
                case TokenKind.DialogKeyword:
                    {
                        DialogBlock dialogBlock;
                        if ((result = DialogBlock.TryParseNode(parser, out dialogBlock, containingModule, prepStatementBinder, validExitKeywords, contextStatementFactories)))
                        {
                            node = dialogBlock;
                        }
                        break;
                    }
                case TokenKind.AcceptKeyword:
                    {
                        AcceptStatement acceptStmt;
                        if((result = AcceptStatement.TryParseNode(parser, out acceptStmt)))
                        {
                            node = acceptStmt;
                        }
                        break;
                    }
                case TokenKind.LoadKeyword:
                    {
                        LoadStatement loadStmt;
                        if((result = LoadStatement.TryParseNode(parser, out loadStmt)))
                        {
                            node = loadStmt;
                        }
                        break;
                    }
                case TokenKind.CreateKeyword:
                    {
                        CreateStatement createStmt;
                        if((result = CreateStatement.TryParseNode(parser, out createStmt)))
                        {
                            node = createStmt;
                        }
                        break;
                    }
                case TokenKind.BreakpointKeyword:
                    {
                        BreakpointStatement brkStmt;
                        if((result = BreakpointStatement.TryParseNode(parser, out brkStmt)))
                        {
                            node = brkStmt;
                        }
                        break;
                    }
                case TokenKind.OutputKeyword:
                    {
                        OutputToReportStatement outRpt;
                        if((result = OutputToReportStatement.TryParseNode(parser, out outRpt)))
                        {
                            node = outRpt;
                        }
                        break;
                    }
                case TokenKind.StartKeyword:
                    {
                        StartReportStatement startRpt;
                        if((result = StartReportStatement.TryParseNode(parser, out startRpt)))
                        {
                            node = startRpt;
                        }
                        break;
                    }
                case TokenKind.FinishKeyword:
                    {
                        FinishReportStatement finRpt;
                        if ((result = FinishReportStatement.TryParseNode(parser, out finRpt)))
                        {
                            node = finRpt;
                        }
                        break;
                    }
                case TokenKind.TerminateKeyword:
                    {
                        TerminateReportStatement termRpt;
                        if ((result = TerminateReportStatement.TryParseNode(parser, out termRpt)))
                        {
                            node = termRpt;
                        }
                        break;
                    }
                default:
                    {
                        if (SqlStatementFactory.IsValidStatementStart(parser.PeekToken().Kind))
                        {
                            bool dummy;
                            result = SqlStatementFactory.TryParseSqlStatement(parser, out node, out dummy);
                        }
                        break;
                    }
            }

            if(result)
            {
                // check for semicolon
                if (parser.PeekToken(TokenKind.Semicolon))
                    parser.NextToken();
            }

            return result;
        }
    }

    public class BreakpointStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out BreakpointStatement node)
        {
            node = null;
            if(parser.PeekToken(TokenKind.BreakpointKeyword))
            {
                node = new BreakpointStatement();
                parser.NextToken();
                node.StartIndex = parser.Token.Span.Start;
                node.EndIndex = parser.Token.Span.End;
                return true;
            }
            return false;
        }
    }
}
