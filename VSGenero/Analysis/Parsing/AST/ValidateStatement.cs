using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// VALIDATE target [,...] LIKE
    /// {
    ///    table.*
    /// |
    ///    table.column
    /// }
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_VALIDATE.html
    /// </summary>
    public class ValidateStatement : FglStatement
    {

        public static bool TryParseNode(Parser parser, out ValidateStatement defNode)
        {
            defNode = null;
            // TODO: parse initialization statement
            return false;
        }
    }
}
