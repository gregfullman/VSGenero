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
        private const string _regexStr = "(\"\\s*)?((?<!\\()select|delete|insert|(?<!for\\s+)update)\\s+";
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

        public static string FormatSqlStatement(string sqlStatement)
        {
            _parseErrors = null;
            sqlStatement = sqlStatement.Replace("?", _dynamicPlaceholder);
            var fragment = _parser.Parse(new StringReader(sqlStatement), out _parseErrors);
            if (_parseErrors.Count == 0)
            {
                var result = fragment.GetText().Replace(_dynamicPlaceholder, "?");
                return fragment.GetText();
            }
            return null;
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
            var matches = Regex.Matches(text, _regexStr, RegexOptions.IgnoreCase);
            for (int i = 0; i < matches.Count; i++)
            {
                int startIndex = matches[i].Index;
                // does the match start with a quote?
                bool startsWithQuote = false;
                bool inSfmt = false;
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
                StringBuilder sb = new StringBuilder();
                for(int j = 0; j < substr.Length; j++)
                {
                    if (substr.Length > j && substr[j] == '\"' && startsWithQuote)
                    {
                        // look for comma
                        j++;
                        while (substr.Length > j && char.IsWhiteSpace(substr[j]))
                            j++;
                        if (substr.Length > j && substr[j] == ',')
                            j++;
                        while (substr.Length > j && char.IsWhiteSpace(substr[j]))
                            j++;
                        if (substr.Length > j && substr[j] != '\"')
                        {
                            bool alreadyInSfmt = inSfmt;
                            if(!alreadyInSfmt && substr.Substring(j, 4).Equals("sfmt", StringComparison.OrdinalIgnoreCase))
                            {
                                inSfmt = true;
                            }
                            // continue until the quote
                            do
                            {
                                if(alreadyInSfmt && substr[j] == ')')
                                {
                                    inSfmt = false;
                                }
                                j++;
                            }
                            while (j < substr.Length && substr[j] != '\"');
                            //sb.Append("\n");
                            //j--;
                        }
                        sb.Append(" ");
                    }
                    else if(substr.Length > (j + 4) &&
                            substr.Substring(j, 4).Equals("into", StringComparison.OrdinalIgnoreCase))
                    {
                        j = j + 4;
                        while (substr.Length > (j + 4) &&
                               !substr.Substring(j, 4).Equals("from", StringComparison.OrdinalIgnoreCase))
                            j++;
                        j--;
                    }
                    else if(inSfmt && substr[j] == '%' && j + 1 < substr.Length && char.IsNumber(substr[j + 1]))
                    {
                        // replace the sfmt placeholder with the _dynamicPlaceholder
                        j++;
                        while (j < substr.Length && char.IsNumber(substr[j]))
                            j++;
                        sb.Append(_dynamicPlaceholder);
                    }
                    else
                    {
                        sb.Append(substr[j]);
                    }
                }

                substr = sb.ToString();

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
