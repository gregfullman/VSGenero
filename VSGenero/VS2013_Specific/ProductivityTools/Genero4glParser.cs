using Microsoft.PowerToolsEx.BlockTagger.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.EditorExtensions;
using Microsoft.VisualStudio.VSCommon;
using Microsoft.PowerToolsEx.BlockTagger;
using Microsoft.VisualStudio.Text;
using System.Threading;

namespace VSGenero.VS2013Plus.ProductivityTools
{
    internal class Genero4glParser : IParser
    {
        public CodeBlock Parse(ITextSnapshot snapshot, AbortCheck abort)
        {
            CodeBlock block = new CodeBlock(null, BlockType.Root, null, new SnapshotSpan(snapshot, 0, snapshot.Length), 0, 0);
            CodeBlock parent = block;
            Stack<CodeBlock> stack = new Stack<CodeBlock>();
            StringBuilder rawStatement = new StringBuilder();
            StringBuilder filteredStatement = new StringBuilder();
            StringBuilder tempStatement = new StringBuilder();
            QuoteFilter filter = new QuoteFilter(snapshot);
            while (filter.Next())
            {
                int position = filter.Position;
                char character = filter.Character;
                if (!filter.InQuote)
                {
                    BlockType blockType;
                    int endPosition;
                    string endStatement;
                    // see if we're at the start of a code block
                    if (TryParseCodeBlockStart(filter, tempStatement, out blockType))
                    {
                        // don't allow methods to be nested within other blocks (can't happen)
                        if (!(blockType == BlockType.Method && stack.Count > 0))
                        {
                            CodeBlock item = new CodeBlock(parent, blockType, tempStatement.ToString(), new SnapshotSpan(snapshot, position, 0), position, stack.Count + 1);
                            stack.Push(item);
                            parent = item;
                        }
                    }
                    else if (TryParseCodeBlockEnd(filter, out blockType, out endPosition, out endStatement) && stack.Count > 0)
                    {
                        // check to see if the end block type matches the stack item about to be popped. If not, no go
                        while(stack.Count > 0 && stack.Peek().Type != blockType)
                            stack.Pop();
                        if (stack.Count > 0)
                        {
                            CodeBlock block4 = stack.Pop();
                            block4.SetSpan(new SnapshotSpan(snapshot, Span.FromBounds((int)block4.Span.Start, endPosition + 1)));
                            filter.Position = endPosition;
                            parent = block4.Parent;
                        }
                    }
                }
                if (abort())
                {
                    return null;
                }
            }
            while (stack.Count > 0)
            {
                CodeBlock block5 = stack.Pop();
                block5.SetSpan(new SnapshotSpan(snapshot, Span.FromBounds((int)block5.Span.Start, snapshot.Length)));
            }
            return block;

        }

        private int AdvancePastWhitespace(StringBuilder statement, QuoteFilter filter, int offset = 1)
        {
            int advanced = 0;
            char nextchar;
            while (char.IsWhiteSpace((nextchar = filter.PeekNextChar(offset + advanced))))
            {
                statement.Append(nextchar);
                advanced++;
            }
            return advanced;
        }

        private int AdvanceToString(string s, StringBuilder statement, QuoteFilter filter, out bool failed, int offset = 1)
        {
            failed = false;
            int advanced = 0;
            char nextchar;
            char c = s[0];
            while (!filter.PeekEof(offset + advanced))
            {
                while ((nextchar = filter.PeekNextChar(offset + advanced)) != char.MaxValue &&
                       char.ToLower(nextchar) != c)
                {
                    statement.Append(nextchar);
                    advanced++;
                }
                if (char.ToLower(nextchar) != c)
                {
                    failed = true;
                    break;
                }
                else
                {
                    statement.Append(nextchar);
                    advanced++;
                    bool falsePos = false;
                    // now continue trying to match the string
                    for (int i = 1; i < s.Length; i++)
                    {
                        nextchar = filter.PeekNextChar(offset + advanced);
                        if (char.ToLower(nextchar) != s[i])
                        {
                            falsePos = true;
                            break;
                        }
                        statement.Append(nextchar);
                        advanced++;
                    }

                    // we matched the string
                    if (!falsePos)
                        break;
                }
            }
            return advanced;
        }

        private int AdvanceToCharacter(char c, StringBuilder statement, QuoteFilter filter, out bool failed, int offset = 1)
        {
            failed = false;
            int advanced = 0;
            char nextchar;
            while ((nextchar = filter.PeekNextChar(offset + advanced)) != char.MaxValue &&
                   char.ToLower(nextchar) != c)
            {
                statement.Append(nextchar);
                advanced++;
            }
            if (char.ToLower(nextchar) != c)
            {
                failed = true;
            }
            else
            {
                statement.Append(nextchar);
                advanced++;
            }
            return advanced;
        }

        private bool TryParseCodeBlockEnd(QuoteFilter filter, out BlockType blockType, out int position, out string statement)
        {
            position = -1;
            blockType = BlockType.Unknown;
            StringBuilder tempStatement = new StringBuilder();
            if (TryMatchRemainingKeywordFragment("end", filter, tempStatement))
            {
                int continueInd = tempStatement.Length + 1;
                continueInd += AdvancePastWhitespace(tempStatement, filter, continueInd);
                switch (char.ToLower(filter.PeekNextChar(continueInd)))
                {
                    case 'm':
                        // main
                        if (TryMatchRemainingKeywordFragment("main", filter, tempStatement, continueInd - 1))
                        {
                            blockType = BlockType.Method;
                        }
                        break;
                    case 'f':
                        // function
                        if (TryMatchRemainingKeywordFragment("function", filter, tempStatement, continueInd - 1))
                        {
                            blockType = BlockType.Method;
                        }
                        // foreach
                        else if (TryMatchRemainingKeywordFragment("foreach", filter, tempStatement, continueInd - 1))
                        {
                            blockType = BlockType.Loop;
                        }
                        // for
                        else if (TryMatchRemainingKeywordFragment("for", filter, tempStatement, continueInd - 1))
                        {
                            blockType = BlockType.Loop;
                        }
                        break;
                    case 'r':
                        // report
                        break;
                    case 'i':
                        // if
                        if (TryMatchRemainingKeywordFragment("if", filter, tempStatement, continueInd - 1))
                        {
                            blockType = BlockType.Conditional;
                        }
                        break;
                    case 'w':
                        // while
                        if (TryMatchRemainingKeywordFragment("while", filter, tempStatement, continueInd - 1))
                        {
                            blockType = BlockType.Loop;
                        }
                        break;
                }
            }
            statement = null;
            if (blockType != BlockType.Unknown)
            {
                statement = tempStatement.ToString();
                position = filter.Position + tempStatement.Length;
                return true;
            }
            return false;
        }

        private bool TryParseCodeBlockStart(QuoteFilter filter, StringBuilder statementStart, out BlockType blockType)
        {
            blockType = BlockType.Unknown;
            statementStart.Append(filter.Character);
            switch (filter.LowerCharacter)
            {
                case 'm':
                    // main
                    return TryParseMainBlockStart(filter, statementStart, out blockType);
                case 'f':
                    {
                        // function
                        if (TryParseFunctionBlockStart(TryFunctionEnum.Function, filter, statementStart, out blockType))
                            return true;
                        else if (TryParseLoopBlockStart(TryLoopEnum.Foreach, filter, statementStart, out blockType))    // foreach
                            return true;
                        else
                        {
                            // restore the filter character, since it was probably stripped off in the foreach match-attempt
                            statementStart.Append(filter.Character);
                            // for
                            return TryParseLoopBlockStart(TryLoopEnum.For, filter, statementStart, out blockType);
                        }
                    }
                case 'r':
                    // report
                    return TryParseFunctionBlockStart(TryFunctionEnum.Function, filter, statementStart, out blockType);
                case 'p':
                    // public
                    // private
                    return TryParseFunctionAccessBlockStart(filter, statementStart, out blockType);
                case 'i':
                    // if
                    return TryParseIfBlockStart(filter, statementStart, out blockType);
                case 'w':
                    // while
                    return TryParseLoopBlockStart(TryLoopEnum.While, filter, statementStart, out blockType);
            }

            // if we got down to here, clear the statementStart
            statementStart.Clear();
            return false;
        }

        private enum TryLoopEnum
        {
            For,
            Foreach,
            While
        }

        private bool TryParseLoopBlockStart(TryLoopEnum tryLoop, QuoteFilter filter, StringBuilder statementStart, out BlockType blockType)
        {
            blockType = BlockType.Unknown;
            string remaining = null;
            switch(tryLoop)
            {
                case TryLoopEnum.For: remaining = "or"; break;
                case TryLoopEnum.Foreach: remaining = "oreach"; break;
                case TryLoopEnum.While: remaining = "hile"; break;
            }
            if (TryMatchRemainingKeywordFragment(remaining, filter, statementStart))
            {
                // make sure there's nothing else before the for on this line
                int prev = 1;
                char prevChar;
                while((prevChar = filter.PeekPrevChar(prev)) != '\n')
                {
                    if(!char.IsWhiteSpace(prevChar))
                        return false;
                    prev++;
                }

                var nextChar = filter.PeekNextChar(statementStart.Length);
                if (nextChar == ' ' || nextChar == '(')
                {
                    bool failed = false;
                    statementStart.Append(nextChar);
                    int continueInd = statementStart.Length;
                    continueInd += AdvanceToCharacter('\n', statementStart, filter, out failed, continueInd);
                    if (failed || filter.PeekEof(continueInd))
                        return false;
                    blockType = BlockType.Loop;
                    filter.IncrementPosition(statementStart.Length);
                    return true;
                }
            }
            return false;
        }

        private bool TryParseIfBlockStart(QuoteFilter filter, StringBuilder statementStart, out BlockType blockType)
        {
            blockType = BlockType.Unknown;
            if (TryMatchRemainingKeywordFragment("f", filter, statementStart))
            {
                var nextChar = filter.PeekNextChar(statementStart.Length);
                if (nextChar == ' ' || nextChar == '(')
                {
                    bool failed = false;
                    statementStart.Append(nextChar);
                    int continueInd = statementStart.Length;
                    continueInd += AdvanceToString("then", statementStart, filter, out failed, continueInd);
                    if (!failed)
                    {
                        blockType = BlockType.Conditional;
                        filter.IncrementPosition(statementStart.Length);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryParseMainBlockStart(QuoteFilter filter, StringBuilder statementStart, out BlockType blockType)
        {
            blockType = BlockType.Unknown;
            if (TryMatchRemainingKeywordFragment("ain", filter, statementStart))
            {
                // ensure the character after is a whitespace
                if (char.IsWhiteSpace(filter.PeekNextChar(statementStart.Length)))
                {
                    blockType = BlockType.Method;
                    filter.IncrementPosition(statementStart.Length);
                    return true;
                }
            }
            return false;
        }

        private bool TryParseFunctionAccessBlockStart(QuoteFilter filter, StringBuilder statementStart, out BlockType blockType)
        {
            TryFunctionEnum tryFunction = TryFunctionEnum.Function | TryFunctionEnum.Report;
            blockType = BlockType.Unknown;
            string backup = statementStart.ToString();
            if (TryMatchRemainingKeywordFragment("ublic", filter, statementStart))
            {
                int continueInd = statementStart.Length;
                continueInd += AdvancePastWhitespace(statementStart, filter, continueInd);
                statementStart.Append('f');
                return TryParseFunctionBlockStart(tryFunction, filter, statementStart, out blockType, continueInd);
            }
            else
            {
                // need to reset the statement to check private
                statementStart.Append(backup);
            }
            if (TryMatchRemainingKeywordFragment("rivate", filter, statementStart))
            {
                int continueInd = statementStart.Length;
                continueInd += AdvancePastWhitespace(statementStart, filter, continueInd);
                statementStart.Append('f');
                return TryParseFunctionBlockStart(tryFunction, filter, statementStart, out blockType, continueInd);
            }
            return false;
        }

        private enum TryFunctionEnum
        {
            Function,
            Report
        }

        private bool TryParseFunctionBlockStart(TryFunctionEnum tryFunction, QuoteFilter filter, StringBuilder statementStart, out BlockType blockType, int offset = 0)
        {
            blockType = BlockType.Unknown;
            string backup = statementStart.ToString();
            if (tryFunction.HasFlag(TryFunctionEnum.Function))
            {
                if (TryMatchRemainingKeywordFragment("unction", filter, statementStart, offset))
                {
                    bool failed = false;
                    int continueInd = statementStart.Length;
                    continueInd += AdvanceToCharacter('(', statementStart, filter, out failed, continueInd);
                    if (failed || filter.PeekEof(continueInd))
                        return false;
                    continueInd += AdvanceToCharacter(')', statementStart, filter, out failed, continueInd);
                    if (failed || filter.PeekEof(continueInd))
                        return false;
                    blockType = BlockType.Method;
                    filter.IncrementPosition(statementStart.Length);
                    return true;
                }
                else
                {
                    // need to reset the statement to check report
                    statementStart.Append(backup);
                    if (tryFunction.HasFlag(TryFunctionEnum.Report))
                    {
                        statementStart.Remove(statementStart.Length - 1, 1);    // remove 'f'
                        statementStart.Append('r');
                    }
                }
            }
            if (tryFunction.HasFlag(TryFunctionEnum.Report))
            {
                if (TryMatchRemainingKeywordFragment("eport", filter, statementStart, offset))
                {
                    bool failed = false;
                    int continueInd = statementStart.Length;
                    continueInd += AdvanceToCharacter('(', statementStart, filter, out failed, continueInd);
                    if (failed || filter.PeekEof(continueInd))
                        return false;
                    continueInd += AdvanceToCharacter(')', statementStart, filter, out failed, continueInd);
                    if (failed || filter.PeekEof(continueInd))
                        return false;
                    blockType = BlockType.Method;
                    filter.IncrementPosition(statementStart.Length);
                    return true;
                }
            }
            return false;
        }

        private bool TryMatchRemainingKeywordFragment(string keywordFragment, QuoteFilter filter, StringBuilder statementStart, int offset = 0)
        {
            for (int i = 0; i < keywordFragment.Length; i++)
            {
                var nextChar = filter.PeekNextChar(offset + i + 1);
                if (char.ToLower(nextChar) != keywordFragment[i])
                {
                    statementStart.Clear();
                    return false;
                }
                else
                {
                    AppendCharacter(statementStart, nextChar);
                }
            }
            return true;
        }

        private static void AppendCharacter(StringBuilder statement, char c)
        {
            if (char.IsWhiteSpace(c))
            {
                if ((statement.Length > 0) && (statement[statement.Length - 1] != ' '))
                {
                    statement.Append(' ');
                }
            }
            else
            {
                statement.Append(c);
            }
        }

        public static bool ContainsEquals(string statement)
        {
            int num = 0;
            for (int i = 0; i < statement.Length; i++)
            {
                char ch = statement[i];
                if ((ch == '=') && (num == 0))
                {
                    return true;
                }
                if (ch == '(')
                {
                    num++;
                }
                else if (ch == ')')
                {
                    num--;
                }
            }
            return false;
        }
    }
}
