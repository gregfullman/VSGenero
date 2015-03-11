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
        public bool TryParseNode(Parser parser, out FglStatement node)
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
                        if ((result = DeclareStatement.TryParseNode(parser, out declStmt)))
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
            }

            return result;
        }
    }
}
