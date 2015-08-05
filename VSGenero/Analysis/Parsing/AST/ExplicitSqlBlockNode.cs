/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/ 

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
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
