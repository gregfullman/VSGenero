using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class SqlStatement : FglStatement
    {
        public static bool IsValidStatementStart(TokenKind tokenKind)
        {
            switch(tokenKind)
            {
                case TokenKind.SelectKeyword:
                case TokenKind.UpdateKeyword:
                case TokenKind.InsertKeyword:
                case TokenKind.ExecuteKeyword:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryParseNode(Parser parser, out SqlStatement defNode, out bool matchedBreakSequence, TokenKind limitTo = TokenKind.EndOfFile, List<List<TokenKind>> breakSequences = null)
        {
            matchedBreakSequence = false;
            defNode = null;
            bool result = false;

            if((limitTo != TokenKind.EndOfFile && parser.PeekToken(limitTo)) ||
               IsValidStatementStart(parser.PeekToken().Kind))
            {

            }

            return result;
        }
    }
}
