using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// 
    /// Syntax 1:
    /// GLOBALS
    ///     declaration-statement (variable, constant, or type definition)
    ///     [,...]
    /// END GLOBALS
    /// 
    /// or
    /// 
    /// Syntax 2:
    /// GLOBALS "filename"
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Globals_003.html
    /// </summary>
    public class GlobalsNode : AstNode, IGlobalsResult, IOutlinableResult
    {
        public string GlobalsFilename { get; private set; }

        public static bool TryParseNode(Parser parser, out GlobalsNode defNode)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.GlobalsKeyword))
            {
                result = true;
                parser.NextToken();
                defNode = new GlobalsNode();
                defNode.StartIndex = parser.Token.Span.Start;

                var tok = parser.PeekToken();
                if(Tokenizer.GetTokenInfo(tok).Category == TokenCategory.StringLiteral)
                {
                    parser.NextToken();
                    defNode.GlobalsFilename = parser.Token.Token.Value.ToString();
                }
                else
                {
                    List<List<TokenKind>> breakSequences = new List<List<TokenKind>>() 
                    { 
                        new List<TokenKind> { TokenKind.EndKeyword, TokenKind.GlobalsKeyword },
                        new List<TokenKind> { TokenKind.ConstantKeyword },
                        new List<TokenKind> { TokenKind.DefineKeyword },
                        new List<TokenKind> { TokenKind.TypeKeyword }
                    };
                    // try to parse one or more declaration statements
                    while(!parser.PeekToken(TokenKind.EndOfFile) &&
                          !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.GlobalsKeyword, 2)))
                    {
                        DefineNode defineNode;
                        TypeDefNode typeNode;
                        ConstantDefNode constNode;
                        bool matchedBreakSequence = false;
                        switch(parser.PeekToken().Kind)
                        {
                            case TokenKind.TypeKeyword:
                                {
                                    var bsList = new List<List<TokenKind>>(breakSequences);
                                    if (TypeDefNode.TryParseNode(parser, out typeNode, out matchedBreakSequence, breakSequences))
                                    {
                                        defNode.Children.Add(typeNode.StartIndex, typeNode);
                                        foreach (var def in typeNode.GetDefinitions())
                                        {
                                            def.Scope = "global type";
                                            defNode.GlobalTypes.Add(def.Name, def);
                                        }
                                    }
                                    break;
                                }
                            case TokenKind.ConstantKeyword:
                                {
                                    if (ConstantDefNode.TryParseNode(parser, out constNode, out matchedBreakSequence, breakSequences))
                                    {
                                        defNode.Children.Add(constNode.StartIndex, constNode);
                                        foreach (var def in constNode.GetDefinitions())
                                        {
                                            def.Scope = "global constant";
                                            defNode.GlobalConstants.Add(def.Name, def);
                                        }
                                    }
                                    break;
                                }
                            case TokenKind.DefineKeyword:
                                {
                                    if (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences))
                                    {
                                        defNode.Children.Add(defineNode.StartIndex, defineNode);
                                        foreach (var def in defineNode.GetDefinitions())
                                            foreach (var vardef in def.VariableDefinitions)
                                            {
                                                vardef.Scope = "global variable";
                                                defNode.GlobalVariables.Add(vardef.Name, vardef);
                                            }
                                    }
                                    break;
                                }
                        }
                        // if a break sequence was matched, we don't want to advance the token
                        if(!matchedBreakSequence)
                        {
                            // TODO: not sure whether to break or keep going...for right now, let's keep going until we hit the end keyword
                            parser.NextToken();
                        }
                    }

                    if (!parser.PeekToken(TokenKind.EndOfFile))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenKind.GlobalsKeyword))
                        {
                            parser.NextToken();
                            defNode.EndIndex = parser.Token.Span.End;
                        }
                        else
                        {
                            parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of globals definition.");
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Unexpected end of globals definition");
                    }
                }
            }

            return result;
        }

        private Dictionary<string, IAnalysisResult> _variables;
        public IDictionary<string, IAnalysisResult> GlobalVariables
        {
            get
            {
                if (_variables == null)
                    _variables = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _variables;
            }
        }

        private Dictionary<string, IAnalysisResult> _types;
        public IDictionary<string, IAnalysisResult> GlobalTypes
        {
            get
            {
                if (_types == null)
                    _types = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _types;
            }
        }

        private Dictionary<string, IAnalysisResult> _constants;
        public IDictionary<string, IAnalysisResult> GlobalConstants
        {
            get
            {
                if (_constants == null)
                    _constants = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _constants;
            }
        }

        public int DecoratorEnd
        {
            get
            {
                return StartIndex + 7;
            }
            set
            {
            }
        }

        public bool CanOutline
        {
            get { return string.IsNullOrWhiteSpace(GlobalsFilename); }
        }
    }
}
