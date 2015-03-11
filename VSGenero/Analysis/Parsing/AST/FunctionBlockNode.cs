using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// [PUBLIC|PRIVATE] FUNCTION function-name ( [ argument [,...]] )
    ///     [ declaration [...] ]
    ///     [ statement [...] ]
    ///     [ return-clause ]
    /// END FUNCTION
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Functions_syntax.html
    /// </summary>
    public class FunctionBlockNode : AstNode
    {
        public AccessModifier AccessModifier { get; protected set; }
        // TODO: instead of string, this should be the token
        public string AccessModifierToken { get; protected set; }

        public string Name { get; protected set; }

        public int DecoratorEnd { get; protected set; }

        public string DescriptiveName
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(Name);
                sb.Append('(');

                // if there are any parameters put them in
                int total = _arguments.Count;
                int i = 0;
                foreach (var varDef in _arguments.OrderBy(x => x.Key.Span.Start))
                {
                    sb.AppendFormat("{0} {1}", varDef.Value.Type.ToString(), varDef.Value.Name);
                    if(i + 1 < total)
                    {
                        sb.Append(", ");
                    }
                    i++;
                }

                sb.Append(')');
                return sb.ToString();
            }
        }

        private Dictionary<string, TokenWithSpan> _internalArguments = new Dictionary<string, TokenWithSpan>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<TokenWithSpan, VariableDef> _arguments = new Dictionary<TokenWithSpan, VariableDef>(TokenWithSpan.CaseInsensitiveNameComparer);

        protected bool AddArgument(TokenWithSpan token, out string errMsg)
        {
            errMsg = null;
            string key = token.Token.Value.ToString();
            if(!_internalArguments.ContainsKey(key))
            {
                _internalArguments.Add(key, token);
                _arguments.Add(token, null);
                return true;
            }
            errMsg = string.Format("Duplicate argument found: {0}", key);
            return false;
        }

        protected void BindArgument(VariableDef varDef)
        {
            if (_internalArguments != null)
            {
                TokenWithSpan t;
                if (_internalArguments.TryGetValue(varDef.Name, out t))
                {
                    if (_arguments.ContainsKey(t))
                    {
                        _arguments[t] = varDef;
                    }
                }
            }
        }

        //private List<string> _arguments;
        //public List<string> Arguments
        //{
        //    get
        //    {
        //        if (_arguments == null)
        //            _arguments = new List<string>();
        //        return _arguments;
        //    }
        //}

        public static bool TryParseNode(Parser parser, out FunctionBlockNode defNode)
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
            if (parser.PeekToken(TokenKind.FunctionKeyword, lookAheadBy))
            {
                result = true;
                defNode = new FunctionBlockNode();
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

                // get the name
                if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    defNode.Name = parser.Token.Token.Value.ToString();
                    defNode.DecoratorEnd = parser.Token.Span.End;
                }
                else
                {
                    parser.ReportSyntaxError("A function must have a name.");
                }

                if (!parser.PeekToken(TokenKind.LeftParenthesis))
                    parser.ReportSyntaxError("A function must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                // get the parameters
                while(parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    string errMsg;
                    if(!defNode.AddArgument(parser.Token, out errMsg))
                    {
                        parser.ReportSyntaxError(errMsg);
                    }
                    if (parser.PeekToken(TokenKind.Comma))
                        parser.NextToken();

                    // TODO: probably need to handle "end" "function" case...won't right now
                }

                if (!parser.PeekToken(TokenKind.RightParenthesis))
                    parser.ReportSyntaxError("A function must specify zero or more parameters in the form: ([param1][,...])");
                else
                    parser.NextToken();

                List<List<TokenKind>> breakSequences = new List<List<TokenKind>>() 
                    { 
                        new List<TokenKind> { TokenKind.EndKeyword, TokenKind.FunctionKeyword },
                        new List<TokenKind> { TokenKind.ConstantKeyword },
                        new List<TokenKind> { TokenKind.DefineKeyword },
                        new List<TokenKind> { TokenKind.TypeKeyword }
                    };
                // try to parse one or more declaration statements
                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                       !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.FunctionKeyword, 2)))
                {
                    DefineNode defineNode;
                    TypeDefNode typeNode;
                    ConstantDefNode constNode;
                    bool matchedBreakSequence = false;
                    switch (parser.PeekToken().Kind)
                    {
                        case TokenKind.TypeKeyword:
                            {
                                if (TypeDefNode.TryParseNode(parser, out typeNode, out matchedBreakSequence, breakSequences))
                                {
                                    defNode.Children.Add(typeNode.StartIndex, typeNode);
                                }
                                break;
                            }
                        case TokenKind.ConstantKeyword:
                            {
                                if (ConstantDefNode.TryParseNode(parser, out constNode, out matchedBreakSequence, breakSequences))
                                {
                                    defNode.Children.Add(constNode.StartIndex, constNode);
                                }
                                break;
                            }
                        case TokenKind.DefineKeyword:
                            {
                                if (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences, defNode.BindArgument))
                                {
                                    defNode.Children.Add(defineNode.StartIndex, defineNode);
                                }
                                break;
                            }
                        default:
                            {
                                FglStatement statement;
                                if(parser.StatementFactory.TryParseNode(parser, out statement))
                                {
                                    AstNode stmtNode = statement as AstNode;
                                    defNode.Children.Add(stmtNode.StartIndex, stmtNode);
                                }
                                break;
                            }
                    }
                    // if a break sequence was matched, we don't want to advance the token
                    if (!matchedBreakSequence)
                    {
                        // TODO: not sure whether to break or keep going...for right now, let's keep going until we hit the end keyword
                        parser.NextToken();
                    }
                }

                if (!parser.PeekToken(TokenKind.EndOfFile))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.FunctionKeyword))
                    {
                        parser.NextToken();
                        defNode.EndIndex = parser.Token.Span.End;
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of function definition.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Unexpected end of function definition");
                }
            }
            return result;
        }
    }
}
