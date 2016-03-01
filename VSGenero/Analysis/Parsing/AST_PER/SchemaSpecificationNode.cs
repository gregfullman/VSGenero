using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public class SchemaSpecificationNode : AstNodePer
    {
        public NameExpression SchemaName { get; private set; }
        public Token AlternateSchema { get; private set; }
        public bool WithoutNullInput { get; private set; }

        public static bool TryParseNode(IParser parser, out SchemaSpecificationNode node)
        {
            node = null;
            bool result = false;

            if(parser.PeekToken(TokenKind.SchemaKeyword))
            {
                node = new SchemaSpecificationNode();
                node.StartIndex = parser.Token.Span.Start;
                parser.NextToken();
                result = true;

                if(parser.PeekToken(TokenKind.FormonlyKeyword) ||
                   parser.PeekToken(TokenCategory.StringLiteral))
                {
                    node.AlternateSchema = parser.NextToken();
                }
                else
                {
                    NameExpression nameExp;
                    if(NameExpression.TryParseNode(parser, out nameExp))
                    {
                        node.SchemaName = nameExp;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid schema name found.");
                    }
                }

                node.EndIndex = parser.Token.Span.End;
            }
            else if(parser.PeekToken(TokenKind.DatabaseKeyword))
            {
                node = new SchemaSpecificationNode();
                node.StartIndex = parser.Token.Span.Start;
                parser.NextToken();
                result = true;

                if (parser.PeekToken(TokenKind.FormonlyKeyword) ||
                   parser.PeekToken(TokenCategory.StringLiteral))
                {
                    node.AlternateSchema = parser.NextToken();
                }
                else
                {
                    NameExpression nameExp;
                    if (NameExpression.TryParseNode(parser, out nameExp))
                    {
                        node.SchemaName = nameExp;
                    }
                    else
                    {
                        parser.ReportSyntaxError("Invalid schema name found.");
                    }
                }

                if(parser.PeekToken(TokenKind.WithoutKeyword))
                {
                    parser.NextToken();
                    if(parser.PeekToken(TokenKind.NullKeyword))
                    {
                        parser.NextToken();
                        if(parser.PeekToken(TokenKind.InputKeyword))
                        {
                            parser.NextToken();
                            node.WithoutNullInput = true;
                        }
                        else
                        {
                            parser.ReportSyntaxError("Expecting \"without null input\".");
                        }
                    }
                    else
                    {
                        parser.ReportSyntaxError("Expecting \"without null input\".");
                    }    
                }

                node.EndIndex = parser.Token.Span.End;
            }

            return result;
        }
    }
}
