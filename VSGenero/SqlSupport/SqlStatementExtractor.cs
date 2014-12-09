using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VSGenero.SqlSupport
{
    public static class SqlStatementExtractor
    {
        private const string _regexStr = "(\"\\s*)?(select|delete|insert|(?<!for\\s+)update)\\s+";
        private static TSql100Parser _parser = new TSql100Parser(false);
        private static Sql100ScriptGenerator _scriptGenerator = new Sql100ScriptGenerator();
        private static IList<ParseError> _parseErrors;

        private const string _dynamicPlaceholder = "qm";

        public static string GetText(this IScriptFragment fragment)
        {
            string script;
            _scriptGenerator.GenerateScript(fragment, out script);
            script = script.Replace(_dynamicPlaceholder, "?");
            script = script.Replace(";", string.Empty);
            return script.Trim();
        }

        /// <summary>
        /// Given some text, extract any sql statements contained within.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IEnumerable<IScriptFragment> ExtractStatements(string text)
        {
            // first replace all question marks with 'qm'
            text = text.Replace("?", _dynamicPlaceholder);
            text = text.Replace("\r\n", "\n");

            List<IScriptFragment> fragments = new List<IScriptFragment>();
            var matches = Regex.Matches(text, _regexStr);
            for (int i = 0; i < matches.Count; i++)
            {
                int startIndex = matches[i].Index;
                // does the match start with a quote?
                bool startsWithQuote = false;
                if (matches[i].Value.StartsWith("\""))
                {
                    startIndex++;
                    startsWithQuote = true;
                }

                int endIndex = text.Length - 1;
                // get text up to the next match
                if (i + 1 < matches.Count)
                {
                    endIndex = matches[i + 1].Index - 1;
                }

                string substr = text.Substring(startIndex, (endIndex - startIndex) + 1);
                var stringJoinMatches = Regex.Matches(substr, "\"\\s*,");
                if (stringJoinMatches.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    int currInd = 0;
                    for (int j = 0; j < stringJoinMatches.Count; j++)
                    {
                        int matchEndIndex = stringJoinMatches[j].Index + stringJoinMatches[j].Length;
                        sb.Append(substr.Substring(currInd, matchEndIndex - currInd));
                        matchEndIndex++;
                        if (substr.Length > matchEndIndex)
                        {
                            // advance currInd to the next good bit of info
                            while (substr.Length > matchEndIndex)
                            {
                                if (!char.IsWhiteSpace(substr[matchEndIndex]))
                                    break;
                                matchEndIndex++;
                            }
                            if (substr.Length > matchEndIndex)
                            {
                                if(substr[matchEndIndex] == '\"')
                                {
                                    // advance past the quote so it will join cleanly
                                    currInd = matchEndIndex + 1;
                                    // remove the comma and quote
                                    sb.Remove(sb.Length - 2, 2);
                                }
                                else 
                                {
                                    // we're attempting to join two strings where the one on the right is not a string literal.
                                    // Can't do much more.

                                    // remove the comma
                                    sb.Remove(sb.Length - 1, 1);
                                    sb.Append("\n");
                                    currInd = matchEndIndex;
                                    break;
                                }
                            }
                            else
                            {
                                currInd = matchEndIndex;
                            }
                        }
                        else
                        {
                            if (startsWithQuote)
                            {
                                // only remove the comma
                                sb.Remove(sb.Length - 1, 1);
                            }
                            else
                            {
                                // remove the command and quote
                                sb.Remove(sb.Length - 2, 2);
                            }
                        }
                    }
                    sb.Append(substr.Substring(currInd));
                    substr = sb.ToString().TrimEnd().Replace("\r\n", "\n");
                }


                if (startsWithQuote)
                {
                    int prevNewline = substr.Length - 1;
                    // we need to finish with a quote
                    while (!substr.EndsWith("\""))
                    {
                        prevNewline = substr.LastIndexOf('\n', substr.Length - 1);
                        if (prevNewline >= 0)
                        {
                            substr = substr.Substring(0, prevNewline);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (prevNewline < 0)
                    {
                        continue;
                    }
                    else
                    {
                        substr = substr.TrimEnd(new[] { '\"' });
                    }
                }
                _parseErrors = null;
                var fragment = _parser.Parse(new StringReader(substr), out _parseErrors);
                bool notAStatement = false;
                while (_parseErrors.Count > 0 && substr.Length > 0)
                {
                    // whittle down the substring by newline
                    int prevNewline = substr.Length - 1;
                    prevNewline = substr.LastIndexOf('\n', prevNewline);
                    if (prevNewline >= 0)
                    {
                        substr = substr.Substring(0, prevNewline);
                        if (startsWithQuote)
                        {
                            // we need to finish with a quote
                            while (!substr.EndsWith("\""))
                            {
                                prevNewline = substr.LastIndexOf('\n', substr.Length - 1);
                                if (prevNewline >= 0)
                                {
                                    substr = substr.Substring(0, prevNewline);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (prevNewline < 0)
                            {
                                continue;
                            }
                            else
                            {
                                substr = substr.TrimEnd(new[] { '\"' });
                            }
                        }
                        fragment = _parser.Parse(new StringReader(substr), out _parseErrors);
                    }
                    else
                    {
                        notAStatement = true;
                        break;
                    }
                }

                if (notAStatement)
                    continue;

                yield return fragment;
            }
        }
    }
}
