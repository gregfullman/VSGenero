using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// TRY
    ///     instruction
    ///     [...]
    /// CATCH
    ///     instruction
    ///     [...]
    /// END TRY
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Exceptions_007.html
    /// </summary>
    public class TryBlockNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out TryBlockNode defNode)
        {
            defNode = null;
            // TODO: parse try block
            return false;
        }
    }
}
