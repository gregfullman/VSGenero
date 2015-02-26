using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// MAIN
    ///     [ <see cref="DefineNode"/>
    ///     | <see cref="ConstantDefNode"/>
    ///     | <see cref="TypeDefNode"/>
    ///     ]
    ///     { [<see cref="DeferStatementNode"/>]
    ///     | fgl-statement
    ///     | sql-statement
    ///     }
    ///     [...]
    /// END MAIN
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_programs_MAIN.html
    /// </summary>
    public class MainBlockNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out MainBlockNode defNode)
        {
            defNode = null;
            // TODO: parse main block
            return false;
        }
    }
}
