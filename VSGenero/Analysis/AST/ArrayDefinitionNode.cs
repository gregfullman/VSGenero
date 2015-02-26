using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// Static array definition:
    /// variable ARRAY [ size [,size  [,size] ] ] OF datatype
    /// 
    /// Dynamic array definition:
    /// variable DYNAMIC ARRAY [ WITH DIMENSION rank ] OF datatype
    /// 
    /// Java array definition:
    /// variable ARRAY [ ] OF javatype
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Arrays_002.html
    /// </summary>
    public class ArrayDefinitionNode : VariableDefinitionNode
    {
        public static bool TryParseNode(Parser parser, out ArrayDefinitionNode defNode)
        {
            defNode = null;
            // TODO: parse array definition
            return false;
        }
    }
}
