using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// [PUBLIC|PRIVATE] FUNCTION function-name ( [ argument [,...]] )
    ///     [ declaration [...] ]
    ///     [ statement [...] ]
    ///     [ return-clause ]
    /// END FUNCTION
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_Functions_syntax.html
    /// </summary>
    public class FunctionBlockNode : AstNode
    {
        public AccessModifier AccessModifier { get; protected set; }
        // TODO: instead of string, this should be the token
        public string AccessModifierToken { get; protected set; }

        public string Name { get; private set; }

        private List<string> _arguments;
        public List<string> Arguments
        {
            get
            {
                if (_arguments == null)
                    _arguments = new List<string>();
                return _arguments;
            }
        }

        public static bool TryParseNode(Parser parser, out FunctionBlockNode defNode)
        {
            defNode = null;
            // TODO: parse function block
            return false;
        }
    }
}
