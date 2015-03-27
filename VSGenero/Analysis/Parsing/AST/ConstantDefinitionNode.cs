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
        public string SpecifiedType { get; private set; }
        public string Literal { get; private set; }

        public static bool TryParseNode(IParser parser, out ConstantDefinitionNode defNode)
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
                defNode.Identifier = parser.Token.Token.Value.ToString();

                if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
                {
                    parser.NextToken();
                    defNode.SpecifiedType = parser.Token.Token.Value.ToString();

                    string typeStr;
                    if(TypeConstraints.VerifyValidConstraint(parser, out typeStr))
                    {
                        defNode.SpecifiedType = typeStr;
                    }
                }

                if(!parser.PeekToken(TokenKind.Equals) && !(parser.PeekToken(2) is ConstantValueToken))
                {
                    parser.ReportSyntaxError("A constant must be defined with a value.");
                }
                else
                {
                    parser.NextToken(); // advance to equals
                    
                    if(parser.PeekToken(TokenCategory.StringLiteral) ||
                       parser.PeekToken(TokenCategory.NumericLiteral) ||
                       parser.PeekToken(TokenCategory.CharacterLiteral))
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
                            }
                            else
                                parser.ReportSyntaxError("Interval constant has an invalid constraint.");
                        }
                        else
                            parser.ReportSyntaxError("Interval constant found with an invalid expression.");
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

        public override string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.Append(Name);
                if(!string.IsNullOrWhiteSpace(SpecifiedType))
                {
                    sb.AppendFormat(" ({0})", SpecifiedType);
                }
                sb.AppendFormat(" = {0}", Literal);
                return sb.ToString();
            }
        }


        public int LocationIndex
        {
            get { return StartIndex; }
        }


        public IAnalysisResult GetMember(string name, GeneroAst ast)
        {
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast)
        {
            return null;
        }

        public bool HasChildFunctions
        {
            get { return false; }
        }
    }
}
