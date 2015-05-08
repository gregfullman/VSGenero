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
            }

            return result;
        }
    }
}
