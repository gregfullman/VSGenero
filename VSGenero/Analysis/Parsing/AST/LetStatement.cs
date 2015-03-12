using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// LET target = expr [,...]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_LET.html
    /// </summary>
    public class LetStatement : FglStatement
    {
        public NameExpression Variable { get; private set; }

        public string GetLiteralValue()
        {
            if(Children.Count == 1)
            {
                StringExpressionNode strExpr = Children[Children.Keys[0]] as StringExpressionNode;
                if(strExpr != null)
                {
                    return strExpr.LiteralValue;
                }
                else
                {
                    return "(unable to evaluate expression)";
                }
            }
            else
            {
                return "";
            }
        }

        public static bool TryParseNode(Parser parser, out LetStatement defNode)
        {
            defNode = null;
            bool result = false;

            if (parser.PeekToken(TokenKind.LetKeyword))
            {
                result = true;
                defNode = new LetStatement();
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;

                NameExpression name;
                if (!NameExpression.TryParseNode(parser, out name, TokenKind.Equals))
                {
                    parser.ReportSyntaxError("Unexpected token found in let statement, expecting name expression.");
                }
                else
                {
                    defNode.Variable = name;
                }

                if (!parser.PeekToken(TokenKind.Equals))
                {
                    parser.ReportSyntaxError("Assignment statement is missing an assignment operator.");
                }
                else
                {
                    parser.NextToken();

                    // get the expression(s)
                    ExpressionNode mainExpression = null;
                    while (true)
                    {
                        ExpressionNode expr;
                        if (!ExpressionNode.TryGetExpressionNode(parser, out expr))
                        {
                            parser.ReportSyntaxError("Assignment statement must have one or more comma-separated expressions.");
                            break;
                        }
                        if (mainExpression == null)
                        {
                            mainExpression = expr;
                        }
                        else
                        {
                            mainExpression.AppendExpression(expr);
                        }

                        if (!parser.PeekToken(TokenKind.Comma))
                        {
                            break;
                        }
                        parser.NextToken();
                    }

                    if (mainExpression != null)
                    {
                        defNode.Children.Add(mainExpression.StartIndex, mainExpression);
                        defNode.EndIndex = mainExpression.EndIndex;
                    }
                }
            }

            return result;
        }
    }
}
