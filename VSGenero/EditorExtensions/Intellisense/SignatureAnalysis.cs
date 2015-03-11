using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.Intellisense
{
    public class SignatureAnalysis
    {
        private readonly string _text;
        private readonly int _paramIndex;
        private readonly ISignature[] _signatures;
        private readonly string _lastKeywordArgument;

        internal SignatureAnalysis(string text, int paramIndex, IList<ISignature> signatures, string lastKeywordArgument = null)
        {
            _text = text;
            _paramIndex = paramIndex;
            _signatures = new ISignature[signatures.Count];
            signatures.CopyTo(_signatures, 0);
            _lastKeywordArgument = lastKeywordArgument;
            Array.Sort(_signatures, (x, y) => x.Parameters.Count - y.Parameters.Count);
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }

        public int ParameterIndex
        {
            get
            {
                return _paramIndex;
            }
        }

        public string LastKeywordArgument
        {
            get
            {
                return _lastKeywordArgument;
            }
        }

        public IList<ISignature> Signatures
        {
            get
            {
                return _signatures;
            }
        }
    }
}
