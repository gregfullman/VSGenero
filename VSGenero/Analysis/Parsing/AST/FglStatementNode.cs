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
                                 Action<PrepareStatement> prepStatementBinder = null)
        {
            node = null;
            bool result = false;

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
                        SqlStatement sqlStmt;
                        bool dummy;
                        if ((result = SqlStatement.TryParseNode(parser, out sqlStmt, out dummy)))
                        {
                            node = sqlStmt;
                        }
                        break;
                    }
                case TokenKind.ReturnKeyword:
                    {
                        // parse out a return statement
                        ReturnStatement retStmt;
                        if((result = ReturnStatement.TryParseNode(parser, out retStmt)))
                        {
                            node = retStmt;
                        }
                        break;
                    }
                case TokenKind.CallKeyword:
                    {
                        // TODO: parse out a call statement
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
            }

            return result;
        }
    }
}
