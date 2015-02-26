using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.AST
{
    /// <summary>
    /// [PUBLIC|PRIVATE] REPORT report-name (argument-list)
    ///  [ define-section ]
    ///  [ output-section ]
    ///  [  sort-section ]
    ///  [ format-section ] 
    /// END REPORT
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_reports_Report_Definition.html
    /// </summary>
    public class ReportBlockNode : FunctionBlockNode
    {
        public static bool TryParseNode(Parser parser, out ReportBlockNode defNode)
        {
            defNode = null;
            // TODO: parse function block
            return false;
        }
    }
}
