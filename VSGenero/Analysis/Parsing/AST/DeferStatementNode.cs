using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// DEFER { INTERRUPT | QUIT }
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_DEFER.html
    /// </summary>
    public class DeferStatementNode : FglStatement
    {
        public static bool TryParseNode(Parser parser, out DeferStatementNode defNode)
        {
            defNode = null;
            // TODO: parse constant node
            return false;
        }
    }
}
