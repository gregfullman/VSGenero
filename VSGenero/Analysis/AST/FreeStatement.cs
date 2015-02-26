using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    public class FreeStatement : FglStatementNode
    {
        public static bool TryParseNode(Parser parser, out FreeStatement defNode)
        {
            defNode = null;
            // TODO: parse free statement
            return false;
        }
    }
}
