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

    public class FglStatementFactory
    {
        public bool TryParseNode(Parser parser, out FglStatement node, 
                                 Func<string, PrepareStatement> prepStatementResolver = null, 
                                 Action<PrepareStatement> prepStatementBinder = null,
                                 bool returnStatementsOnly = false)
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
                        if ((result = DeclareStatement.TryParseNode(parser, out declStmt, prepStatementResolver)))
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
                        if ((result = PrepareStatement.TryParseNode(parser, out prepStmt)))
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
                        if((result = IfStatement.TryParseNode(parser, out ifStmt, prepStatementResolver, prepStatementBinder)))
                        {
                            node = ifStmt;
                        }
                        break;
                    }
                case TokenKind.WhileKeyword:
                    {
                        WhileStatement whileStmt;
                        if((result = WhileStatement.TryParseNode(parser, out whileStmt, prepStatementResolver, prepStatementBinder)))
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
                        if((result = ForStatement.TryParserNode(parser, out forStmt, prepStatementResolver, prepStatementBinder)))
                        {
                            node = forStmt;
                        }
                        break;
                    }
                case TokenKind.CaseKeyword:
                    {
                        CaseStatement caseStmt;
                        if((result = CaseStatement.TryParseNode(parser, out caseStmt, prepStatementResolver, prepStatementBinder)))
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
                        if((result = TryCatchStatement.TryParseNode(parser, out tryStmt, prepStatementResolver, prepStatementBinder)))
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
                        if((result = ForeachStatement.TryParseNode(parser, out foreachStmt)))
                        {
                            node = foreachStmt;
                        }
                        break;
                    }
                case TokenKind.MenuKeyword:
                    {
                        MenuBlock menuStmt;
                        if ((result = MenuBlock.TryParseNode(parser, out menuStmt)))
                        {
                            node = menuStmt;
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

            return result;
        }
    }
}
