using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// Format:
    /// INITIALIZE target [,...]
    /// {
    ///    TO NULL
    ///     |
    ///    LIKE {table.*|table.column}
    /// }
    /// 
    /// For more info: see http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_INITIALIZE.html
    /// </summary>
    public class InitializeStatement : FglStatementNode
    {

        public static bool TryParseNode(Parser parser, out InitializeStatement defNode)
        {
            defNode = null;
            // TODO: parse initialization statement
            return false;
        }
    }
}
