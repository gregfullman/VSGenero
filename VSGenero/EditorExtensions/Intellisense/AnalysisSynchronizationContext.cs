using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSGenero.Analysis;

namespace VSGenero.EditorExtensions.Intellisense
{
    /// <summary>
    /// Provides the synchronization context for our analysis.  This enables working with
    /// System.Threading.Tasks to post work back onto the analysis queue thread in a simple
    /// manner.
    /// </summary>
    class AnalysisSynchronizationContext : SynchronizationContext
    {
        private readonly AnalysisQueue _queue;
        [ThreadStatic]
        internal static AutoResetEvent _waitEvent;

        public AnalysisSynchronizationContext(AnalysisQueue queue)
        {
            _queue = queue;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _queue.Enqueue(new AnalysisItem(d, state), AnalysisPriority.High);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (_waitEvent == null)
            {
                _waitEvent = new AutoResetEvent(false);
            }
            var waitable = new WaitableAnalysisItem(d, state);
            _queue.Enqueue(waitable, AnalysisPriority.High);
            _waitEvent.WaitOne();
        }

        class AnalysisItem : IAnalyzable
        {
            private SendOrPostCallback _delegate;
            private object _state;

            public AnalysisItem(SendOrPostCallback callback, object state)
            {
                _delegate = callback;
                _state = state;
            }

            #region IAnalyzable Members

            public virtual void Analyze(CancellationToken cancel)
            {
                _delegate(_state);
            }

            #endregion
        }

        class WaitableAnalysisItem : AnalysisItem
        {
            public WaitableAnalysisItem(SendOrPostCallback callback, object state)
                : base(callback, state)
            {
            }

            public override void Analyze(CancellationToken cancel)
            {
                base.Analyze(cancel);
                AnalysisSynchronizationContext._waitEvent.Set();
            }
        }
    }
}
