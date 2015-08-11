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

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
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
    public class FunctionBlockNode : AstNode, IFunctionResult
    {
        public AccessModifier AccessModifier { get; protected set; }
        // TODO: instead of string, this should be the token
        public string AccessModifierToken { get; protected set; }

        public string Name { get; protected set; }

        public string Namespace { get; protected set; }
        private string _oneTimeNamespace;
        public int DecoratorEnd { get; set; }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public bool IsPublic { get { return AccessModifier == Analysis.AccessModifier.Public; } }

        public string DescriptiveName
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(_oneTimeNamespace))
                {
                    sb.AppendFormat("{0}.", _oneTimeNamespace);
                    _oneTimeNamespace = null;
                }
                sb.Append(Name);
                sb.Append('(');

                // if there are any parameters put them in
                int total = _arguments.Count;
                int i = 0;
                foreach (var varDef in _arguments.OrderBy(x => x.Key.Span.Start))
                {
                    if (varDef.Value != null)
                        sb.AppendFormat("{0} {1}", varDef.Value.Type.ToString(), varDef.Value.Name);
                    else
                        sb.AppendFormat("{0}", varDef.Key.Token.Value.ToString());
                    if (i + 1 < total)
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

        private List<ReturnStatement> _internalReturns = new List<ReturnStatement>();

        protected bool AddArgument(TokenWithSpan token, out string errMsg)
        {
            errMsg = null;
            string key = token.Token.Value.ToString();
            if (!_internalArguments.ContainsKey(key))
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

        protected ParserResult StoreReturnStatement(ReturnStatement retStmt)
        {
            ParserResult result = new ParserResult { Success = true };
            // TODO: resolve any variables in the return statement, and verify that if there are multiple return statements, they all have the same number of return values.
            if (_internalReturns == null)
                _internalReturns = new List<ReturnStatement>();
            bool valid = true;
            foreach (var ret in _internalReturns)
            {
                if (ret.Returns.Count != retStmt.Returns.Count)
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
                _internalReturns.Add(retStmt);
            else
                result.ErrorMessage = "Return statement does not return the same number of values as other return statements in this function.";

            return result;
        }

        protected void BindPrepareCursorFromIdentifier(PrepareStatement prepStmt)
        {
            // If the prepare statement uses a variable from prepare from, that variable should have been encountered
            // prior to the prepare statement. So we'll do a binary search in the children of this function to look for
            // a LetStatement above the prepare statement where the prepare statement's from identifier was assigned. 
            // If it can't be found, then we have to assume that the identifier was assigned outside of this function,
            // and we have no real way to determining the cursor SQL text.

            if (prepStmt.Children.Count == 1)
            {
                StringExpressionNode strExpr = prepStmt.Children[prepStmt.Children.Keys[0]] as StringExpressionNode;
                if (strExpr != null)
                {
                    prepStmt.SetSqlStatement(strExpr.LiteralValue);
                }
                else
                {
                    NameExpression exprNode = prepStmt.Children[prepStmt.Children.Keys[0]] as NameExpression;
                    if (exprNode != null)
                    {
                        string ident = exprNode.Name;

                        List<int> keys = Children.Select(x => x.Key).ToList();
                        int searchIndex = keys.BinarySearch(prepStmt.StartIndex);
                        if (searchIndex < 0)
                        {
                            searchIndex = ~searchIndex;
                            if (searchIndex > 0)
                                searchIndex--;
                        }

                        LetStatement letStmt = null;
                        while (searchIndex >= 0 && searchIndex < keys.Count)
                        {
                            int key = keys[searchIndex];
                            letStmt = Children[key] as LetStatement;
                            if (letStmt != null)
                            {
                                // check for the LetStatement's identifier
                                if (ident.Equals(letStmt.Variable.Name, StringComparison.OrdinalIgnoreCase))
                                    break;
                                else
                                    letStmt = null;
                            }
                            searchIndex--;
                        }

                        if (letStmt != null)
                        {
                            // we have a match, bind the let statement's value
                            prepStmt.SetSqlStatement(letStmt.GetLiteralValue());
                        }
                        else
                        {
                            // no match was found, so we'll put something in its place
                            int i = 0;
                        }
                    }
                }
            }
        }

        public static bool TryParseNode(Parser parser, out FunctionBlockNode defNode, out int bufferPosition,
                                        IModuleResult containingModule, bool abbreviatedParse = false, bool advanceOnEnd = true)
        {
            bufferPosition = 0;
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
                    defNode.LocationIndex = parser.Token.Span.Start;
                    if (containingModule != null)
                        defNode.Namespace = containingModule.ProgramName;
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
                while (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                {
                    parser.NextToken();
                    string errMsg;
                    if (!defNode.AddArgument(parser.Token, out errMsg))
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

                List<List<TokenKind>> breakSequences =
                    new List<List<TokenKind>>(GeneroAst.ValidStatementKeywords
                        .Where(x => x != TokenKind.EndKeyword && x != TokenKind.FunctionKeyword)
                        .Select(x => new List<TokenKind> { x }))
                    { 
                        new List<TokenKind> { TokenKind.EndKeyword, TokenKind.FunctionKeyword }
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
                                if (DefineNode.TryParseDefine(parser, out defineNode, out matchedBreakSequence, breakSequences, defNode.BindArgument) && defineNode != null)
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
                                if (parser.StatementFactory.TryParseNode(parser, out statement, containingModule, 
                                                                         defNode.BindPrepareCursorFromIdentifier, defNode.StoreReturnStatement, defNode.AddLimitedScopeVariable,
                                                                         abbreviatedParse, validExits))
                                {
                                    AstNode stmtNode = statement as AstNode;
                                    if (stmtNode != null && !defNode.Children.ContainsKey(stmtNode.StartIndex))
                                    {
                                        defNode.Children.Add(stmtNode.StartIndex, stmtNode);
                                    }

                                    continue;
                                }
                                break;
                            }
                    }

                    if (parser.PeekToken(TokenKind.EndOfFile) ||
                       (parser.PeekToken(TokenKind.EndKeyword) && parser.PeekToken(TokenKind.FunctionKeyword, 2)))
                    {
                        break;
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
                        var tokSpan = parser.PeekTokenWithSpan();
                        bufferPosition = tokSpan.BufferPosition + tokSpan.Span.Length;
                        if (advanceOnEnd)
                            parser.NextToken();
                        defNode.EndIndex = parser.Token.Span.End;
                        defNode.IsComplete = true;
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

        public ParameterResult[] Parameters
        {
            get
            {
                return _arguments.OrderBy(x => x.Key.Span.Start)
                                 .Where(x => x.Value != null)
                                 .Select(x => new ParameterResult(x.Value.Name, x.Value.Documentation, x.Value.Type.ToString()))
                                 .ToArray();
            }
        }

        private string[] _returns;
        public string[] Returns
        {
            get
            {
                if (_returns == null)
                {
                    if (_internalReturns.Count > 0)
                    {
                        _returns = new string[_internalReturns.Max(ir => ir.Returns.Count)];
                        // Need to go through the internal returns and determine return names and types
                        foreach (var retStmt in _internalReturns)
                        {
                            for (int i = 0; i < retStmt.Returns.Count; i++)
                            {
                                var ret = retStmt.Returns[i];
                                string type = ret.GetExpressionType(SyntaxTree);
                                _returns[i] = type;
                            }
                        }
                    }
                    else
                    {
                        _returns = new string[0];
                    }
                }
                return _returns;
            }
        }

        protected void AddLimitedScopeVariable(IAnalysisResult res, int start, int end)
        {
            if(LimitedScopeVariables.ContainsKey(res.Name))
            {
                LimitedScopeVariables[res.Name].Add(new Tuple<IAnalysisResult, IndexSpan>(res, new IndexSpan(start, end - start)));
            }
            else
            {
                LimitedScopeVariables.Add(res.Name, new List<Tuple<IAnalysisResult, IndexSpan>> { new Tuple<IAnalysisResult, IndexSpan>(res, new IndexSpan(start, end - start)) });
            }
        }

        private Dictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> _limitedScopeVariables;
        public IDictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>> LimitedScopeVariables
        {
            get
            {
                if (_limitedScopeVariables == null)
                    _limitedScopeVariables = new Dictionary<string, List<Tuple<IAnalysisResult, IndexSpan>>>(StringComparer.OrdinalIgnoreCase);
                return _limitedScopeVariables;
            }
        }

        private Dictionary<string, IAnalysisResult> _variables;
        public IDictionary<string, IAnalysisResult> Variables
        {
            get
            {
                if (_variables == null)
                    _variables = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _variables;
            }
        }

        private Dictionary<string, IAnalysisResult> _types;
        public IDictionary<string, IAnalysisResult> Types
        {
            get
            {
                if (_types == null)
                    _types = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _types;
            }
        }

        private Dictionary<string, IAnalysisResult> _constants;
        public IDictionary<string, IAnalysisResult> Constants
        {
            get
            {
                if (_constants == null)
                    _constants = new Dictionary<string, IAnalysisResult>(StringComparer.OrdinalIgnoreCase);
                return _constants;
            }
        }

        private string _scope;
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
                _scope = value;
            }
        }

        public override string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                if (Returns.Length == 1)
                {
                    sb.AppendFormat("{0} ", Returns[0]);
                }
                else if (Returns.Length == 0)
                {
                    sb.Append("void ");
                }
                sb.Append(DescriptiveName);

                if (Returns.Length > 1)
                {
                    sb.Append("\nreturning ");
                    for (int i = 0; i < Returns.Length; i++)
                    {
                        sb.Append(Returns[i]);
                        if (i + 1 < Returns.Length)
                            sb.Append(", ");
                    }
                }

                return sb.ToString();
            }
        }

        public bool CanOutline
        {
            get { return true; }
        }


        public string FunctionDocumentation
        {
            get { return ""; }  // TODO: Provide source doc documentation
        }

        private int _locationIndex;
        public int LocationIndex
        {
            get { return _locationIndex; }
            protected set { _locationIndex = value; }
        }

        private LocationInfo _location = null;
        public LocationInfo Location { get { return _location; } }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry)
        {
            definingProject = null;
            projEntry = null;
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType)
        {
            return null;
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            return false;
        }

        public string CompletionParentName
        {
            get { return null; }
        }


        public int DecoratorStart
        {
            get
            {
                return StartIndex;
            }
            set
            {
            }
        }


        public void SetOneTimeNamespace(string nameSpace)
        {
            _oneTimeNamespace = nameSpace;
        }

        private string _typename;
        public string Typename
        {
            get
            {
                //if(_typename == null)
                //{
                    if (Returns.Length > 0 && Returns.Length < 2)
                    {
                        //StringBuilder sb = new StringBuilder();
                        //if (!string.IsNullOrWhiteSpace(_oneTimeNamespace))
                        //{
                        //    sb.AppendFormat("{0}.", _oneTimeNamespace);
                        //    _oneTimeNamespace = null;
                        //}
                        //sb.Append(Returns[0]);
                        //_typename = sb.ToString();
                        return Returns[0];
                    }
                //}
                
                return _typename;
            }
        }

        public override void PropagateSyntaxTree(GeneroAst ast)
        {
            // set location
            _location = ast.ResolveLocation(this);

            base.PropagateSyntaxTree(ast);
        }

        public override void CheckForErrors(GeneroAst ast, Action<string, int, int> errorFunc,
                                            Dictionary<string, List<int>> deferredFunctionSearches,
                                            GeneroAst.FunctionProviderSearchMode searchInFunctionProvider = GeneroAst.FunctionProviderSearchMode.NoSearch, bool isFunctionCallOrDefinition = false)
        {
            if (_arguments != null)
            {
                // 1) check to make sure arguments have corresponding defined variables
                foreach (var undefinedArg in _arguments.Where(x => x.Value == null))
                {
                    errorFunc(string.Format("No definition found for parameter {0}", undefinedArg.Key.Token.Value.ToString()),
                                undefinedArg.Key.Span.Start, undefinedArg.Key.Span.End);
                }
            }

            // TODO: 2) Check to make sure the types in return statements match up across multiple return statements.
            if (_internalReturns != null && _internalReturns.Count > 1)
            {

            }

            base.CheckForErrors(ast, errorFunc, deferredFunctionSearches);
        }
    }
}
