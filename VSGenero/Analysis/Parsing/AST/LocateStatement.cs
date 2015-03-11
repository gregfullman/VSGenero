using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_variables_LOCATE.html
    /// </summary>
    public class LocateStatement : FglStatement
    {
        public static bool TryParseNode(Parser parser, out LocateStatement defNode)
        {
            defNode = null;
            // TODO: parse locate statement
            return false;
        }
    }
}
