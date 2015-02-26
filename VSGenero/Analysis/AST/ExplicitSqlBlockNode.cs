using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// SQL
    ///   sql-statement
    ///  END SQL
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_static_sql_SQL_blocks.html
    /// </summary>
    public class ExplicitSqlBlockNode : AstNode
    {
        public static bool TryParseNode(Parser parser, out ExplicitSqlBlockNode defNode)
        {
            defNode = null;
            // TODO: parse compiler options
            return false;
        }
    }
}
