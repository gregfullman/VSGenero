using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public class FreeStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out FreeStatement defNode)
        {
            defNode = null;
            // TODO: parse free statement
            return false;
        }
    }
}
