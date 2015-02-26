using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// LET target = expr [,...]
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_LET.html
    /// </summary>
    public class LetStatement : FglStatementNode
    {
        public static bool TryParseNode(Parser parser, out LetStatement defNode)
        {
            defNode = null;
            // TODO: parse let statement
            return false;
        }
    }
}
