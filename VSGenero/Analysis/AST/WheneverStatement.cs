using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// WHENEVER exception-class
    ///     exception-action
    ///     
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Exceptions_006.html
    /// </summary>
    public class WheneverStatement : FglStatementNode
    {
        public static bool TryParseNode(Parser parser, out WheneverStatement defNode)
        {
            defNode = null;
            // TODO: parse whenever statement
            return false;
        }
    }
}
