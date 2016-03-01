using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public abstract class ExpressionNode : AstNode
    {
        public void AppendExpression(ExpressionNode node)
        {
            AppendedExpressions.Add(node);
            if (!Children.ContainsKey(node.StartIndex))
                Children.Add(node.StartIndex, node);
            EndIndex = AppendedExpressions[AppendedExpressions.Count - 1].EndIndex;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(GetStringForm());

            int index = 0;
            while (index < AppendedExpressions.Count)
            {
                sb.Append(' ');
                sb.Append(AppendedExpressions[index]);
                index++;
            }
            return sb.ToString();
        }

        protected abstract string GetStringForm();
        public abstract string GetExpressionType(GeneroAst ast);

        private List<ExpressionNode> _appendedExpressions;
        public List<ExpressionNode> AppendedExpressions
        {
            get
            {
                if (_appendedExpressions == null)
                    _appendedExpressions = new List<ExpressionNode>();
                return _appendedExpressions;
            }
        }
    }
}
