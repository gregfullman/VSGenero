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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// 
    /// identifier [ datatype] = literal
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Constants_003.html
    /// </summary>
    public class ConstantDefinitionNode : AstNode, IAnalysisResult
    {
        public string Identifier { get; private set; }
        public string Literal { get; private set; }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        private bool _isPublic;
        public bool IsPublic { get { return _isPublic; } }

        public static bool TryParseNode(IParser parser, out ConstantDefinitionNode defNode, bool isPublic = false)
        {
            defNode = null;
            bool result = false;
            // parse constant definition
            if(parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
            {
                defNode = new ConstantDefinitionNode();
                result = true;
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._location = parser.TokenLocation;
                defNode._isPublic = isPublic;
                defNode.Identifier = parser.Token.Token.Value.ToString();

                if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                {
                    parser.NextToken();
                    defNode.Typename = parser.Token.Token.Value.ToString();

                    string typeStr;
                    // This gets things like char(50), etc.
                    if(TypeConstraints.VerifyValidConstraint(parser, out typeStr))
                    {
                        defNode.Typename = typeStr;
                    }
                }

                if(!parser.PeekToken(TokenKind.Equals) && !(parser.PeekToken(2) is ConstantValueToken))
                {
                    parser.ReportSyntaxError("A constant must be defined with a value.");
                }
                else
                {
                    parser.NextToken(); // advance to equals
                    
                    if(parser.PeekToken(TokenCategory.StringLiteral))
                    {
                        parser.NextToken();
                        defNode.Literal = string.Format("\"{0}\"", parser.Token.Token.Value.ToString());
                        defNode.Typename = "string";
                    }
                    else if(parser.PeekToken(TokenCategory.CharacterLiteral))
                    {
                        parser.NextToken();
                        defNode.Literal = string.Format("\'{0}\'", parser.Token.Token.Value.ToString());
                        defNode.Typename = "string";
                    }
                    else if(parser.PeekToken(TokenKind.Subtract))
                    {
                        parser.NextToken();
                        if (parser.PeekToken(TokenCategory.NumericLiteral))
                        {
                            parser.NextToken();
                            defNode.Literal = string.Format("-{0}", parser.Token.Token.Value.ToString());
                        }
                    }
                    else if(parser.PeekToken(TokenCategory.NumericLiteral))
                    {
                        parser.NextToken();
                        defNode.Literal = parser.Token.Token.Value.ToString();
                    }
                    else if(parser.PeekToken(TokenCategory.IncompleteMultiLineStringLiteral))
                    {
                        StringBuilder sb = new StringBuilder(parser.NextToken().Value.ToString());
                        while(parser.PeekToken(TokenCategory.IncompleteMultiLineStringLiteral))
                        {
                            parser.NextToken();
                            sb.Append(parser.Token.Token.Value.ToString());
                        }
                        defNode.Literal = sb.ToString();
                        defNode.IsComplete = true;
                        defNode.Typename = "string";
                    }
                    else if(parser.PeekToken(TokenKind.MdyKeyword))
                    {
                        StringBuilder sb = new StringBuilder(parser.NextToken().Value.ToString());
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            sb.Append("(");
                            while(!parser.PeekToken(TokenKind.Comma))
                            {
                                if(parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                                {
                                    parser.ReportSyntaxError("Invalid token found in MDY specification.");
                                    return result;
                                }
                                sb.Append(parser.NextToken().Value.ToString());
                            }
                            sb.Append(", ");
                            parser.NextToken();
                            while (!parser.PeekToken(TokenKind.Comma))
                            {
                                if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                                {
                                    parser.ReportSyntaxError("Invalid token found in MDY specification.");
                                    return result;
                                }
                                sb.Append(parser.NextToken().Value.ToString());
                            }
                            sb.Append(", ");
                            parser.NextToken();
                            while (!parser.PeekToken(TokenKind.RightParenthesis))
                            {
                                if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                                {
                                    parser.ReportSyntaxError("Invalid token found in MDY specification.");
                                    return result;
                                }
                                sb.Append(parser.NextToken().Value.ToString());
                            }
                            sb.Append(")");
                            parser.NextToken();
                            defNode.Literal = sb.ToString();
                            defNode.IsComplete = true;
                            defNode.Typename = "date";
                        }
                        else
                            parser.ReportSyntaxError("Date constant found with an invalid MDY expression.");
                    }
                    else if(parser.PeekToken(TokenKind.DatetimeKeyword))
                    {
                        StringBuilder sb = new StringBuilder(parser.NextToken().Value.ToString());
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            sb.Append("(");
                            while (!parser.PeekToken(TokenKind.RightParenthesis))
                            {
                                if (parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                                {
                                    parser.ReportSyntaxError("Invalid token found in MDY specification.");
                                    return result;
                                }
                                sb.Append(parser.NextToken().Value.ToString());
                            }
                            sb.Append(") ");
                            parser.NextToken();

                            string constraintStr;
                            if(TypeConstraints.VerifyValidConstraint(parser, out constraintStr, TokenKind.DatetimeKeyword))
                            {
                                sb.Append(constraintStr);
                                defNode.Literal = sb.ToString();
                                defNode.IsComplete = true;
                                defNode.Typename = "datetime";
                            }
                            else
                                parser.ReportSyntaxError("Datetime constant has an invalid constraint.");
                        }
                        else
                            parser.ReportSyntaxError("Datetime constant found with an invalid expression.");
                    }
                    else if(parser.PeekToken(TokenKind.IntervalKeyword))
                    {
                        StringBuilder sb = new StringBuilder(parser.NextToken().Value.ToString());
                        if (parser.PeekToken(TokenKind.LeftParenthesis))
                        {
                            parser.NextToken();
                            sb.Append("(");
                            while (!parser.PeekToken(TokenKind.RightParenthesis))
                            {
                                if(parser.PeekToken(TokenCategory.Keyword) || parser.PeekToken(TokenCategory.Identifier))
                                {
                                    parser.ReportSyntaxError("Invalid token found in MDY specification.");
                                    return result;
                                }
                                sb.Append(parser.NextToken().Value.ToString());
                            }
                            sb.Append(") ");
                            parser.NextToken();

                            string constraintStr;
                            if (TypeConstraints.VerifyValidConstraint(parser, out constraintStr, TokenKind.IntervalKeyword))
                            {
                                sb.Append(constraintStr);
                                defNode.Literal = sb.ToString();
                                defNode.IsComplete = true;
                                defNode.Typename = "interval";
                            }
                            else
                                parser.ReportSyntaxError("Interval constant has an invalid constraint.");
                        }
                        else
                            parser.ReportSyntaxError("Interval constant found with an invalid expression.");
                    }
                    else
                    {
                        // look for the constant in the system constants
                        var tok = parser.PeekToken();
                        IAnalysisResult sysConst;
                        if(GeneroAst.SystemConstants.TryGetValue(tok.Value.ToString(), out sysConst))
                        {
                            parser.NextToken();
                            defNode.Literal = sysConst.Name;
                            defNode.Typename = sysConst.Typename;
                        }
                    }
                }
            }
            return result;
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

        public string Name
        {
            get { return Identifier; }
        }

        private string _oneTimeNamespace;

        public override string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                if(!string.IsNullOrWhiteSpace(_oneTimeNamespace))
                {
                    sb.AppendFormat("{0}.", _oneTimeNamespace);
                    _oneTimeNamespace = null;
                }
                sb.Append(Name);
                if(!string.IsNullOrWhiteSpace(Typename))
                {
                    sb.AppendFormat(" ({0})", Typename);
                }
                sb.AppendFormat(" = {0}", Literal);
                return sb.ToString();
            }
        }


        public int LocationIndex
        {
            get { return StartIndex; }
        }

        private LocationInfo _location;
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


        public void SetOneTimeNamespace(string nameSpace)
        {
            _oneTimeNamespace = nameSpace;
        }

        private string _typename;
        public string Typename
        {
            get 
            {
                if (_typename == null)
                {
                    // it's a number, so see if we can parse it as an int
                    int parseInt;
                    if (!int.TryParse(Literal, out parseInt))
                    {
                        _typename = "numeric";
                    }
                    else
                        _typename = "integer";
                }
                return _typename;
            }
            set
            {
                _typename = value;
            }
        }
    }
}
