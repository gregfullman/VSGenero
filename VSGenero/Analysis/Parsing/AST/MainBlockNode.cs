using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// MAIN
    ///     [ <see cref="DefineNode"/>
    ///     | <see cref="ConstantDefNode"/>
    ///     | <see cref="TypeDefNode"/>
    ///     ]
    ///     { [<see cref="DeferStatementNode"/>]
    ///     | fgl-statement
    ///     | sql-statement
    ///     }
    ///     [...]
    /// END MAIN
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_MAIN.html
    /// </summary>
    public class MainBlockNode : FunctionBlockNode, IFunctionResult
    {
        public new bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public new bool IsPublic { get { return false; } }

        public static bool TryParseNode(Parser parser, out MainBlockNode defNode, IModuleResult containingModule)
        {
            defNode = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.MainKeyword))
            {
                result = true;
                defNode = new MainBlockNode();
                parser.NextToken();
                defNode.Name = parser.Token.Token.Value.ToString();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode.DecoratorEnd = defNode.StartIndex + 4;
                defNode.AccessModifier = AccessModifier.Private;

                List<List<TokenKind>> breakSequences = 
                    new List<List<TokenKind>>(GeneroAst.ValidStatementKeywords
                        .Where(x => x != TokenKind.EndKeyword && x != TokenKind.MainKeyword)
                        .Select(x => new List<TokenKind> { x }))
                    { 
                        new List<TokenKind> { TokenKind.EndKeyword, TokenKind.MainKeyword }
                    };
                bool fglStatement = false;
                // try to parse one or more declaration statements
                while (!parser.PeekToken(TokenKind.EndOfFile) &&
                       !(parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.MainKeyword, 2)))
                {
                    fglStatement = false;
                    DefineNode defineNode;
                    TypeDefNode typeNode;
                    ConstantDefNode constNode;
                    bool matchedBreakSequence = false;
                    switch (parser.PeekToken().Kind)
                    {
                        case TokenKind.TypeKeyword:
                            {
                                if (TypeDefNode.TryParseNode(parser, out typeNode, out matchedBreakSequence, breakSequences) && typeNode != null)
                                {
                                    defNode.Children.Add(typeNode.StartIndex, typeNode);
                                    foreach (var def in typeNode.GetDefinitions())
                                    {
                                        def.Scope = "local type";
                                        if (!defNode.Types.ContainsKey(def.Name))
                                            defNode.Types.Add(def.Name, def);
                                        else
                                            parser.ReportSyntaxError(def.LocationIndex, def.LocationIndex + def.Name.Length, string.Format("Type {0} defined more than once.", def.Name), Severity.Warning);
                                    }
                                }
                                break;
                            }
                        case TokenKind.ConstantKeyword:
                            {
                                if (ConstantDefNode.TryParseNode(parser, out constNode, out matchedBreakSequence, breakSequences) && constNode != null)
                                {
                                    defNode.Children.Add(constNode.StartIndex, constNode);
                                    foreach (var def in constNode.GetDefinitions())
                                    {
                                        def.Scope = "local constant";
                                        if (!defNode.Constants.ContainsKey(def.Name))
                                            defNode.Constants.Add(def.Name, def);
                                        else
                                            parser.ReportSyntaxError(def.LocationIndex, def.LocationIndex + def.Name.Length, string.Format("Constant {0} defined more than once.", def.Name), Severity.Warning);
                                    }
                                }
                                break;
                            }
                        case TokenKind.DefineKeyword:
                            {
                                if (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences) && defineNode != null)
                                {
                                    defNode.Children.Add(defineNode.StartIndex, defineNode);
                                    foreach (var def in defineNode.GetDefinitions())
                                        foreach (var vardef in def.VariableDefinitions)
                                        {
                                            vardef.Scope = "local variable";
                                            if (!defNode.Variables.ContainsKey(vardef.Name))
                                                defNode.Variables.Add(vardef.Name, vardef);
                                            else
                                                parser.ReportSyntaxError(vardef.LocationIndex, vardef.LocationIndex + vardef.Name.Length, string.Format("Variable {0} defined more than once.", vardef.Name), Severity.Warning);
                                        }
                                }
                                break;
                            }
                        default:
                            {
                                List<TokenKind> validExits = new List<TokenKind> { TokenKind.ProgramKeyword };
                                FglStatement statement;
                                if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, defNode.BindPrepareCursorFromIdentifier, false, validExits) && statement != null)
                                {
                                    AstNode stmtNode = statement as AstNode;
                                    defNode.Children.Add(stmtNode.StartIndex, stmtNode);
                                    fglStatement = true;
                                }
                                break;
                            }
                    }

                    if (parser.PeekToken(TokenKind.EndOfFile) ||
                       (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.MainKeyword, 2)))
                    {
                        break;
                    }

                    // if a break sequence was matched, we don't want to advance the token
                    if (!matchedBreakSequence && !fglStatement)
                    {
                        // TODO: not sure whether to break or keep going...for right now, let's keep going until we hit the end keyword
                        parser.NextToken();
                    }
                }

                if (!parser.PeekToken(TokenKind.EndOfFile))
                {
                    parser.NextToken();
                    if (parser.PeekToken(TokenKind.MainKeyword))
                    {
                        parser.NextToken();
                        defNode.EndIndex = parser.Token.Span.End;
                        defNode.IsComplete = true;
                    }
                    else
                    {
                        parser.ReportSyntaxError(parser.Token.Span.Start, parser.Token.Span.End, "Invalid end of main definition.");
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Unexpected end of main definition");
                }
            }

            return result;
        }

        public override string Documentation
        {
            get
            {
                return Name;
            }
        }
    }
}
