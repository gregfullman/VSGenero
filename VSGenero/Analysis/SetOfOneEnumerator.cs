using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    class SetOfOneEnumerator<T> : IEnumerator<T>
    {
        private readonly T _value;
        private bool _enumerated;

        public SetOfOneEnumerator(T value)
        {
            _value = value;
        }

        #region IEnumerator<T> Members

        T IEnumerator<T>.Current
        {
            get { return _value; }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { return _value; }
        }

        bool System.Collections.IEnumerator.MoveNext()
        {
            if (_enumerated)
            {
                return false;
            }
            _enumerated = true;
            return true;
        }

        void System.Collections.IEnumerator.Reset()
        {
            _enumerated = false;
        }

        #endregion
    }
}
