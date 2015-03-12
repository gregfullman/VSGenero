using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;

namespace VSGenero.EditorExtensions.Intellisense
{
    class Genero4glFunctionSignature : ISignature {
        private readonly ITrackingSpan _span;
        private readonly string _content, _ppContent;
        private readonly string _documentation;
        private readonly ReadOnlyCollection<IParameter> _parameters;
        private IParameter _currentParameter;
        private readonly IFunctionResult _overload;

        public Genero4glFunctionSignature(ITrackingSpan span, IFunctionResult overload, int paramIndex, string lastKeywordArg = null)
        {
            _span = span;
            _overload = overload;
            if (lastKeywordArg != null) {
                paramIndex = Int32.MaxValue;
            }

            var content = new StringBuilder(overload.Name);
            var ppContent = new StringBuilder(overload.Name);
            content.Append('(');
            ppContent.AppendLine("(");
            int start = content.Length, ppStart = ppContent.Length;
            var parameters = new IParameter[overload.Parameters.Length];
            for (int i = 0; i < overload.Parameters.Length; i++) {
                ppContent.Append("    ");
                ppStart = ppContent.Length;
                
                var param = overload.Parameters[i];
                if (i > 0) {
                    content.Append(", ");
                    start = content.Length;
                }

                content.Append(param.Name);
                ppContent.Append(param.Name);
                if (!string.IsNullOrEmpty(param.Type) && param.Type != "object") {
                    content.Append(": ");
                    content.Append(param.Type);
                    ppContent.Append(": ");
                    ppContent.Append(param.Type);
                }
                
                var paramSpan = new Span(start, content.Length - start);
                var ppParamSpan = new Span(ppStart, ppContent.Length - ppStart);

                ppContent.AppendLine(",");

                if (lastKeywordArg != null && param.Name == lastKeywordArg) {
                    paramIndex = i;
                }

                parameters[i] = new GeneroParameter(this, param, paramSpan, ppParamSpan);
            }
            content.Append(')');
            ppContent.Append(')');

            _content = content.ToString();
            _ppContent = ppContent.ToString();
            _documentation = overload.FunctionDocumentation.LimitLines(15, stopAtFirstBlankLine: true);

            _parameters = new ReadOnlyCollection<IParameter>(parameters);
            if (paramIndex < parameters.Length) {
                _currentParameter = parameters[paramIndex];
            } else {
                _currentParameter = null;
            }
        }

        internal void SetCurrentParameter(IParameter newValue) {
            if (newValue != _currentParameter) {
                var args = new CurrentParameterChangedEventArgs(_currentParameter, newValue);
                _currentParameter = newValue;
                var changed = CurrentParameterChanged;
                if (changed != null) {
                    changed(this, args);
                }
            }
        }

        public ITrackingSpan ApplicableToSpan {
            get { return _span; }
        }

        public string Content {
            get { return _content; }
        }

        public IParameter CurrentParameter {
            get { return _currentParameter; }
        }

        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;

        public string Documentation {
            get { return _documentation; }
        }

        public ReadOnlyCollection<IParameter> Parameters {
            get { return _parameters; }
        }

        #region ISignature Members


        public string PrettyPrintedContent {
            get { return _ppContent; }
        }

        #endregion
    }
}
