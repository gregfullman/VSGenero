using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// identifier 
    ///     <see cref="TypeReference"/> [<see cref="AttributeSpecifier"/>]
    ///     |
    ///     <see cref="RecordDefinitionNode"/>
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_user_types_003.html
    /// </summary>
    public class TypeDefinitionNode : AstNode
    {
        public string Identifier { get; private set; }

        public static bool TryParseDefine(Parser parser, out TypeDefinitionNode defNode)
        {
            defNode = null;
            // TODO: attempt to parse a variable definition
            return false;
        }
    }
}
