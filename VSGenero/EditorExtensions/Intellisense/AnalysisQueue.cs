/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSGenero.Analysis;

namespace VSGenero.EditorExtensions.Intellisense
{
    public enum AnalysisPriority
    {
        None,
        Low,
        Normal,
        High,
    }

    /// <summary>
    /// Provides a single threaded analysis queue.  Items can be enqueued into the
    /// analysis at various priorities.  
    /// </summary>
    sealed class AnalysisQueue : IDisposable
    {
        private readonly Thread _workThread;
        private readonly AutoResetEvent _workEvent;
        private readonly GeneroProjectAnalyzer _analyzer;
        private readonly object _queueLock = new object();
        private readonly List<IAnalyzable>[] _queue;
        private TaskScheduler _scheduler;
        private CancellationTokenSource _cancel;
        private bool _isAnalyzing;
        private int _analysisPending;

        private const int PriorityCount = (int)AnalysisPriority.High + 1;

        internal AnalysisQueue(GeneroProjectAnalyzer analyzer)
        {
            _workEvent = new AutoResetEvent(false);
            _cancel = new CancellationTokenSource();
            _analyzer = analyzer;

            _queue = new List<IAnalyzable>[PriorityCount];
            for (int i = 0; i < PriorityCount; i++)
            {
                _queue[i] = new List<IAnalyzable>();
            }

            _workThread = new Thread(Worker);
            _workThread.Name = "Genero Analysis Queue";
            _workThread.Priority = ThreadPriority.BelowNormal;
            _workThread.IsBackground = true;

            // start the thread, wait for our synchronization context to be created
            using (AutoResetEvent threadStarted = new AutoResetEvent(false))
            {
                _workThread.Start(threadStarted);
                threadStarted.WaitOne();
            }
        }

        public TaskScheduler Scheduler
        {
            get
            {
                return _scheduler;
            }
        }

        public void Enqueue(IAnalyzable item, AnalysisPriority priority)
        {
            int iPri = (int)priority;

            if (iPri < 0 || iPri > _queue.Length)
            {
                throw new ArgumentException("priority");
            }

            lock (_queueLock)
            {
                // see if we have the item in the queue anywhere...
                for (int i = 0; i < _queue.Length; i++)
                {
                    if (_queue[i].Remove(item))
                    {
                        Interlocked.Decrement(ref _analysisPending);

                        AnalysisPriority oldPri = (AnalysisPriority)i;

                        if (oldPri > priority)
                        {
                            // if it was at a higher priority then our current
                            // priority go ahead and raise the new entry to our
                            // old priority
                            priority = oldPri;
                        }

                        break;
                    }
                }

                // enqueue the work item
                Interlocked.Increment(ref _analysisPending);
                if (priority == AnalysisPriority.High)
                {
                    // always try and process high pri items immediately
                    _queue[iPri].Insert(0, item);
                }
                else
                {
                    _queue[iPri].Add(item);
                }
                _workEvent.Set();
            }
        }

        public void Stop()
        {
            _cancel.Cancel();
            if (_workThread.IsAlive)
            {
                _workEvent.Set();
                _workThread.Join();
            }
        }

        public bool IsAnalyzing
        {
            get
            {
                lock (_queueLock)
                {
                    return _isAnalyzing || _analysisPending > 0;
                }
            }
        }

        public int AnalysisPending
        {
            get
            {
                return _analysisPending;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
            if (_cancel != null)
                _cancel.Dispose();
            if (_workEvent != null)
                _workEvent.Dispose();
        }

        #endregion

        private IAnalyzable GetNextItem(out AnalysisPriority priority)
        {
            for (int i = PriorityCount - 1; i >= 0; i--)
            {
                if (_queue[i].Count > 0)
                {
                    var res = _queue[i][0];
                    _queue[i].RemoveAt(0);
                    Interlocked.Decrement(ref _analysisPending);
                    priority = (AnalysisPriority)i;
                    return res;
                }
            }
            priority = AnalysisPriority.None;
            return null;
        }

        private void Worker(object threadStarted)
        {
            try
            {
                SynchronizationContext.SetSynchronizationContext(new AnalysisSynchronizationContext(this));
                _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
            finally
            {
                ((AutoResetEvent)threadStarted).Set();
            }

            while (!_cancel.IsCancellationRequested)
            {
                IAnalyzable workItem;

                AnalysisPriority pri;
                lock (_queueLock)
                {
                    workItem = GetNextItem(out pri);
                    _isAnalyzing = true;
                }
                if (workItem != null)
                {
                    workItem.Analyze(_cancel.Token);
                }
                else
                {
                    _isAnalyzing = false;
                    WaitHandle.SignalAndWait(
                        _analyzer.QueueActivityEvent,
                        _workEvent
                    );
                }
            }
            _isAnalyzing = false;
        }
    }
}
