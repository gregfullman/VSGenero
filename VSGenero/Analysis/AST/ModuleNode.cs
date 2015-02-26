using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// [ <see cref="CompilerOptionsNode"/>
    /// | <see cref="ImportModuleNode"/> [...]
    /// | <see cref="SchemaSpecificationNode"/>
    /// | <see cref="GlobalsNode"/>
    /// | <see cref="ConstantDefNode"/> [...]
    /// | <see cref="TypeDefNode"/> [...]
    /// | <see cref="DefineNode"/> [...]
    /// ]
    /// 
    /// [ <see cref="MainBlockNode"/> ]
    /// 
    /// [ declared-dialog-block
    /// | function-declaration
    /// | report-declaration
    ///     [...] ]
    /// ]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_019.html
    /// </summary>
    public class ModuleNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out ModuleNode defNode)
        {
            defNode = new ModuleNode();

            // TODO: need to think of a way of ensuring order of the pre-function nodes

            CompilerOptionsNode compOptionsNode;
            if(CompilerOptionsNode.TryParseNode(parser, out compOptionsNode))
            {
                defNode.Children.Add(compOptionsNode.StartIndex, compOptionsNode);
                parser.NextToken();
            }

            ImportModuleNode importNode;
            while(ImportModuleNode.TryParseNode(parser, out importNode))
            {
                defNode.Children.Add(importNode.StartIndex, importNode);
                parser.NextToken();
            }

            SchemaSpecificationNode schemaNode;
            if(SchemaSpecificationNode.TryParseDefine(parser, out schemaNode))
            {
                defNode.Children.Add(schemaNode.StartIndex, schemaNode);
                parser.NextToken();
            }

            GlobalsNode globalNode;
            if(GlobalsNode.TryParseNode(parser, out globalNode))
            {
                defNode.Children.Add(globalNode.StartIndex, globalNode);
                parser.NextToken();
            }

            ConstantDefNode constNode;
            while(ConstantDefNode.TryParseNode(parser, out constNode))
            {
                defNode.Children.Add(constNode.StartIndex, constNode);
                parser.NextToken();
            }

            TypeDefNode typeNode;
            while (TypeDefNode.TryParseNode(parser, out typeNode))
            {
                defNode.Children.Add(typeNode.StartIndex, typeNode);
                parser.NextToken();
            }

            DefineNode defineNode;
            while (DefineNode.TryParseDefine(parser, out defineNode))
            {
                defNode.Children.Add(typeNode.StartIndex, defineNode);
                parser.NextToken();
            }

            MainBlockNode mainBlock;
            if(MainBlockNode.TryParseNode(parser, out mainBlock))
            {
                defNode.Children.Add(typeNode.StartIndex, mainBlock);
                parser.NextToken();
            }

            while(!parser.PeekToken(TokenKind.EndOfFile))
            {
                // TODO: declared dialog block

                FunctionBlockNode funcNode;
                if(FunctionBlockNode.TryParseNode(parser, out funcNode))
                {
                    defNode.Children.Add(typeNode.StartIndex, funcNode);
                    parser.NextToken();
                }
                else
                {
                    ReportBlockNode repNode;
                    if(ReportBlockNode.TryParseNode(parser, out repNode))
                    {
                        defNode.Children.Add(typeNode.StartIndex, repNode);
                        parser.NextToken();
                    }
                }
            }

            return true;
        }
    }
}
