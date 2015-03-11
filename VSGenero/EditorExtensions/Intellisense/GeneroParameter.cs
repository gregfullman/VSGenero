using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;

namespace VSGenero.EditorExtensions.Intellisense
{
    class GeneroParameter : IParameter {
        private readonly ISignature _signature;
        private readonly ParameterResult _param;
        private readonly string _documentation;
        private readonly Span _locus, _ppLocus;

        public GeneroParameter(ISignature signature, ParameterResult param, Span locus, Span ppLocus)
        {
            _signature = signature;
            _param = param;
            _locus = locus;
            _ppLocus = ppLocus;
            _documentation = _param.Documentation.LimitLines(15, stopAtFirstBlankLine: true);
        }

        public string Documentation {
            get { return _documentation; }
        }

        public Span Locus {
            get { return _locus; }
        }

        public string Name {
            get { return _param.Name; }
        }

        public ISignature Signature {
            get { return _signature; }
        }

        public Span PrettyPrintedLocus {
            get { return _ppLocus; }
        }
    }
}
