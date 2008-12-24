using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using SystemUtilities.Serialization;

namespace SystemUtilities.Threading
{
    /*
     *
     * Based on Mike Woodring's Custom Thread Pool
     *
     * http://www.bearcanyon.com/dotnet/#threadpool
     *
     */
    internal sealed class ThreadPool : WaitHandle
    {
        private const int DEFAULT_DYNAMIC_THREAD_DECAY_TIME = 5 /* minutes */ * 60 /* sec/min */ * 1000 /* ms/sec */;
        private const int DEFAULT_NEW_THREAD_TRIGGER_TIME = 500; // milliseconds
        private const ThreadPriority DEFAULT_THREAD_PRIORITY = ThreadPriority.Normal;
        private const int DEFAULT_REQUEST_QUEUE_LIMIT = -1; // unbounded

        private bool _hasBeenStarted = false;
        private bool _stopInProgress = false;
        private readonly string _threadPoolName;
        private readonly int _initialThreadCount;     // Initial # of threads to create (called "static threads" in this class).
        private int _maxThreadCount;         // Cap for thread count.  Threads added above initialThreadCount are called "dynamic" threads.
        private int _currentThreadCount = 0; // Current # of threads in the pool (static + dynamic).
        private int _decayTime;              // If a dynamic thread is idle for this period of time w/o processing work requests, it will exit.
        private TimeSpan _newThreadTrigger;       // If a work request sits in the queue this long before being processed, a new thread will be added to queue up to the max.
        private ThreadPriority _threadPriority;
        private readonly ManualResetEvent _stopCompleteEvent = new ManualResetEvent(false); // Signaled after Stop called and last thread exits.
        private Queue<WorkRequest> _requestQueue;
        private int _requestQueueLimit;      // Throttle for maximum # of work requests that can be added.
        private bool _useBackgroundThreads = true;
        private bool _propogateThreadPrincipal = false;
        private bool _propogateCallContext = false;
        private readonly object _syncLock = new object();

        public event ThreadPoolDelegate Started;
        public event ThreadPoolDelegate Stopped;

        public ThreadPool(int initialThreadCount, int maxThreadCount, string poolName)
            : this(initialThreadCount, maxThreadCount, poolName,
                    DEFAULT_NEW_THREAD_TRIGGER_TIME,
                    DEFAULT_DYNAMIC_THREAD_DECAY_TIME,
                    DEFAULT_THREAD_PRIORITY,
                    DEFAULT_REQUEST_QUEUE_LIMIT)
        {
        }

        public ThreadPool(int initialThreadCount, int maxThreadCount, string poolName,
                           int newThreadTrigger, int dynamicThreadDecayTime,
                           ThreadPriority threadPriority, int requestQueueLimit)
        {
            SafeWaitHandle = _stopCompleteEvent.SafeWaitHandle;

            if (maxThreadCount < initialThreadCount)
            {
                throw new ArgumentException("Maximum thread count must be >= initial thread count.", "maxThreadCount");
            }
            if (dynamicThreadDecayTime <= 0)
            {
                throw new ArgumentException("Dynamic thread decay time cannot be <= 0.", "dynamicThreadDecayTime");
            }
            if (newThreadTrigger <= 0)
            {
                throw new ArgumentException("New thread trigger time cannot be <= 0.", "newThreadTrigger");
            }
            ExceptionHelper.ThrowIfArgumentNullOrEmptyString(poolName, "poolName");

            _initialThreadCount = initialThreadCount;
            _maxThreadCount = maxThreadCount;
            _requestQueueLimit = (requestQueueLimit < 0 ? DEFAULT_REQUEST_QUEUE_LIMIT : requestQueueLimit);
            _decayTime = dynamicThreadDecayTime;
            _newThreadTrigger = new TimeSpan(TimeSpan.TicksPerMillisecond * newThreadTrigger);
            _threadPriority = threadPriority;
            _requestQueue = new Queue<WorkRequest>();

            _threadPoolName = poolName;
        }

        public ThreadPriority Priority
        {
            get { return (_threadPriority); }
            set
            {
                if (_hasBeenStarted)
                {
                    throw new InvalidOperationException("Cannot adjust thread priority after pool has been started.");
                }
                _threadPriority = value;
            }
        }

        public int DynamicThreadDecay
        {
            get { return _decayTime; }
            set
            {
                if (_hasBeenStarted)
                {
                    throw new InvalidOperationException("Cannot adjust dynamic thread decay time after pool has been started.");
                }
                if (value <= 0)
                {
                    throw new ArgumentException("Dynamic thread decay time cannot be <= 0.", "value");
                }
                _decayTime = value;
            }
        }

        public int NewThreadTrigger
        {
            get { return (int)_newThreadTrigger.TotalMilliseconds; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("New thread trigger time cannot be <= 0.", "value");
                }

                Monitor.Enter(_syncLock);
                try
                {
                    _newThreadTrigger = new TimeSpan(TimeSpan.TicksPerMillisecond * value);
                }
                finally
                {
                    Monitor.Exit(_syncLock);
                }
            }
        }

        public int RequestQueueLimit
        {
            get { return _requestQueueLimit; }
            set { _requestQueueLimit = value < 0 ? DEFAULT_REQUEST_QUEUE_LIMIT : value; }
        }

        public int AvailableThreads
        {
            get { return _maxThreadCount - _currentThreadCount; }
        }

        public int MaxThreads
        {
            get { return _maxThreadCount; }
            set
            {
                if (value < _initialThreadCount)
                {
                    throw new ArgumentException("Maximum thread count must be >= initial thread count.", "MaxThreads");
                }
                _maxThreadCount = value;
            }
        }

        public bool IsStarted
        {
            get { return _hasBeenStarted; }
        }

        public bool PropogateThreadPrincipal
        {
            get { return _propogateThreadPrincipal; }
            set { _propogateThreadPrincipal = value; }
        }

        public bool PropogateCallContext
        {
            get { return _propogateCallContext; }
            set { _propogateCallContext = value; }
        }

        public bool IsBackground
        {
            get { return _useBackgroundThreads; }
            set
            {
                if (_hasBeenStarted)
                {
                    throw new InvalidOperationException("Cannot adjust background status after pool has been started.");
                }
                _useBackgroundThreads = value;
            }
        }

        public void Start()
        {
            ICollection<ThreadPoolDelegate> handlers = null;

            Monitor.Enter(_syncLock);
            try
            {
                if (_hasBeenStarted)
                {
                    throw new InvalidOperationException("Pool has already been started.");
                }

                _hasBeenStarted = true;

                /*
                 * Check to see if there were already items posted to the queue
                 * before Start was called.  If so, reset their timestamps to
                 * the current time.
                 */
                if (_requestQueue.Count > 0)
                {
                    ResetWorkRequestTimes();
                }

                for (int n = 0; n < _initialThreadCount; n++)
                {
                    ThreadWrapper thread = new ThreadWrapper(this, true, _threadPriority, string.Format("{0} (static)", _threadPoolName));
                    thread.Start();
                }

                if (Started != null)
                {
                    Delegate[] delegates = Started.GetInvocationList();
                    handlers = new List<ThreadPoolDelegate>(delegates.Length);
                    foreach (ThreadPoolDelegate handler in delegates)
                    {
                        if (handler != null)
                            handlers.Add(handler);
                    }
                }
            }
            finally
            {
                Monitor.Exit(_syncLock);
            }

            if (handlers != null)
            {
                foreach (ThreadPoolDelegate handler in handlers)
                {
                    handler();
                }
            }
        }

        public void Stop()
        {
            InternalStop(false, Timeout.Infinite);
        }

        public void StopAndWait()
        {
            InternalStop(true, Timeout.Infinite);
        }

        public bool StopAndWait(int timeout)
        {
            return InternalStop(true, timeout);
        }

        private bool InternalStop(bool wait, int timeout)
        {
            if (!_hasBeenStarted)
            {
                throw new InvalidOperationException("Cannot stop a thread pool that has not been started yet.");
            }

            Monitor.Enter(_syncLock);
            try
            {
                _stopInProgress = true;
                Monitor.PulseAll(_syncLock);
            }
            finally
            {
                Monitor.Exit(_syncLock);
            }

            if (wait)
            {
                bool stopComplete = WaitOne(timeout, true);

                if (stopComplete)
                {
                    /*
                     * If the stop was successful, we can support being
                     * to be restarted.  If the stop was requested, but not
                     * waited on, then we don't support restarting.
                     */
                    _hasBeenStarted = false;
                    _stopInProgress = false;
                    _requestQueue.Clear();
                    _stopCompleteEvent.Reset();
                }

                return (stopComplete);
            }

            return (true);
        }

        /*
         * Overloads for the early bound WorkRequestDelegate-based targets.
         */
        public bool PostRequest(WorkRequestDelegate cb)
        {
            return PostRequest(cb, (object)null);
        }

        public bool PostRequest(WorkRequestDelegate cb, object state)
        {
            IWorkRequest notUsed;
            return PostRequest(cb, state, out notUsed);
        }

        public bool PostRequest(WorkRequestDelegate cb, object state, out IWorkRequest reqStatus)
        {
            WorkRequest request = new WorkRequest(cb, state, _propogateThreadPrincipal, _propogateCallContext);
            reqStatus = request;
            return PostRequest(request);
        }

        /*
         * Overloads for the late bound Delegate.DynamicInvoke-based targets.
         */
        public bool PostRequest(Delegate cb, object[] args)
        {
            IWorkRequest notUsed;
            return PostRequest(cb, args, out notUsed);
        }

        public bool PostRequest(Delegate cb, object[] args, out IWorkRequest reqStatus)
        {
            WorkRequest request = new WorkRequest(cb, args, _propogateThreadPrincipal, _propogateCallContext);
            reqStatus = request;
            return PostRequest(request);
        }

        /*
         * The actual implementation of PostRequest.
         */
        private bool PostRequest(WorkRequest request)
        {
            Monitor.Enter(_syncLock);
            try
            {
                /*
                 * A requestQueueLimit of -1 means the queue is "unbounded"
                 * (subject to available resources).  IOW, no artificial limit
                 * has been placed on the maximum # of requests that can be
                 * placed into the queue.
                 */
                if (_requestQueueLimit == -1 || _requestQueue.Count < _requestQueueLimit)
                {
                    try
                    {
                        _requestQueue.Enqueue(request);
                        Monitor.Pulse(_syncLock);
                        return (true);
                    }
                    catch
                    {
                    }

                }
            }
            finally
            {
                Monitor.Exit(_syncLock);
            }

            return (false);
        }


        void ResetWorkRequestTimes()
        {
            Monitor.Enter(_syncLock);
            try
            {
                DateTime newTime = DateTime.Now; // DateTime.Now.Add(pool.newThreadTrigger);

                foreach (WorkRequest wr in _requestQueue)
                {
                    wr.WorkingTime = newTime;
                }
            }
            finally
            {
                Monitor.Exit(_syncLock);
            }
        }

        private sealed class ThreadInfo
        {
            private const BindingFlags bfNonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;

            private static readonly MethodInfo miGetLogicalCallContext = typeof(Thread).GetMethod("GetLogicalCallContext", bfNonPublicInstance);
            private static readonly MethodInfo miSetLogicalCallContext = typeof(Thread).GetMethod("SetLogicalCallContext", bfNonPublicInstance);

            private IPrincipal principal;
            private LogicalCallContext callContext;

            private ThreadInfo(bool propogateThreadPrincipal, bool propogateCallContext)
            {
                if (propogateThreadPrincipal)
                {
                    principal = Thread.CurrentPrincipal;
                }

                if (propogateCallContext && (miGetLogicalCallContext != null))
                {
                    callContext = (LogicalCallContext)miGetLogicalCallContext.Invoke(Thread.CurrentThread, null);
                    callContext = SerializationHelper.Clone<LogicalCallContext>(callContext);
                }
            }

            public static ThreadInfo Capture(bool propogateThreadPrincipal, bool propogateCallContext)
            {
                return new ThreadInfo(propogateThreadPrincipal, propogateCallContext);
            }

            public static ThreadInfo Impersonate(ThreadInfo ti)
            {
                ExceptionHelper.ThrowIfArgumentNull(ti, "ti");

                ThreadInfo prevInfo = Capture(true, true);
                Restore(ti);
                return (prevInfo);
            }

            public static void Restore(ThreadInfo ti)
            {
                ExceptionHelper.ThrowIfArgumentNull(ti, "ti");

                /*
                 * Restore call context.
                 */
                if (miSetLogicalCallContext != null)
                {
                    miSetLogicalCallContext.Invoke(Thread.CurrentThread, new object[] { ti.callContext });
                }

                /*
                 * Restore thread identity.  It's important that this be done after
                 * restoring call context above, since restoring call context also
                 * overwrites the current thread principal setting.  If propogateCallContext
                 * and propogateThreadPrincipal are both true, then the following is redundant.
                 * However, since propogating call context requires the use of reflection
                 * to capture/restore call context, I want that behavior to be independantly
                 * switchable so that it can be disabled; while still allowing thread principal
                 * to be propogated.  This also covers us in the event that call context
                 * propogation changes so that it no longer propogates thread principal.
                 */
                Thread.CurrentPrincipal = ti.principal;
            }
        }

        private sealed class WorkRequest : IWorkRequest
        {
            public const int PENDING = 0;
            public const int PROCESSED = 1;
            public const int CANCELLED = 2;

            public readonly Delegate TargetProc;  // Function to call.
            public readonly object ProcArg;       // State to pass to function.
            public readonly object[] ProcArgs;    // Used with Delegate.DynamicInvoke.
            public DateTime TimeStampStarted;     // Time work request was originally enqueued (held constant).
            public DateTime WorkingTime;          // Current timestamp used for triggering new threads (moving target).
            public ThreadInfo ThreadInfo;         // Everything we know about a thread.
            public int State = PENDING;           // The state of this particular request.

            public WorkRequest(WorkRequestDelegate cb, object arg,
                                bool propogateThreadPrincipal, bool propogateCallContext)
            {
                TargetProc = cb;
                ProcArg = arg;
                ProcArgs = null;

                Initialize(propogateThreadPrincipal, propogateCallContext);
            }

            public WorkRequest(Delegate cb, object[] args,
                                bool propogateThreadPrincipal, bool propogateCallContext)
            {
                TargetProc = cb;
                ProcArg = null;
                ProcArgs = args;

                Initialize(propogateThreadPrincipal, propogateCallContext);
            }

            private void Initialize(bool propogateThreadPrincipal, bool propogateCallContext)
            {
                WorkingTime = TimeStampStarted = DateTime.Now;
                ThreadInfo = ThreadInfo.Capture(propogateThreadPrincipal, propogateCallContext);
            }

            public bool Cancel()
            {
                /*
                 * If the work request was pending, mark it cancelled.  Otherwise,
                 * this method was called too late.  Note that this call can
                 * cancel an operation without any race conditions.  But if the
                 * result of this test-and-set indicates the request is in the
                 * "processed" state, it might actually be about to be processed.
                 */
                return (Interlocked.CompareExchange(ref State, CANCELLED, PENDING) == PENDING);
            }
        }

        private sealed class ThreadWrapper
        {
            private readonly ThreadPool _pool;
            private readonly bool _permanent;
            private readonly ThreadPriority _priority;
            private readonly string _name;

            public ThreadWrapper(ThreadPool pool, bool permanent, ThreadPriority priority, string name)
            {
                _pool = pool;
                _permanent = permanent;
                _priority = priority;
                _name = name;

                Monitor.Enter(pool._syncLock);
                try
                {
                    pool._currentThreadCount++;
                }
                finally
                {
                    Monitor.Exit(pool._syncLock);
                }
            }

            public void Start()
            {
                Thread t = new Thread(new ThreadStart(ThreadProc));
                t.SetApartmentState(ApartmentState.MTA);
                t.Name = _name;
                t.Priority = _priority;
                t.IsBackground = _pool._useBackgroundThreads;
                t.Start();
            }

            private void ThreadProc()
            {
                bool done = false;

                while (!done)
                {
                    WorkRequest wr = null;
                    ThreadWrapper newThread = null;

                    Monitor.Enter(_pool._syncLock);
                    try
                    {
                        /*
                         * As long as the request queue is empty and a shutdown hasn't
                         * been initiated, wait for a new work request to arrive.
                         */
                        bool timedOut = false;

                        while (!_pool._stopInProgress && !timedOut && _pool._requestQueue.Count == 0)
                        {
                            if (!Monitor.Wait(_pool._syncLock, _permanent ? Timeout.Infinite : _pool._decayTime))
                            {
                                /*
                                 * Timed out waiting for something to do.  Only dynamically created
                                 * threads will get here, so bail out.
                                 */
                                timedOut = true;
                            }
                        }
                        /*
                         * We exited the loop above because one of the following conditions
                         * was met:
                         *   - ThreadPool.Stop was called to initiate a shutdown.
                         *   - A dynamic thread timed out waiting for a work request to arrive.
                         *   - There are items in the work queue to process.
                         *
                         * If we exited the loop because there's work to be done,
                         * a shutdown hasn't been initiated, and we aren't a dynamic thread
                         * that timed out, pull the request off the queue and prepare to
                         * process it.
                         */
                        if (!_pool._stopInProgress && !timedOut && _pool._requestQueue.Count > 0)
                        {
                            wr = _pool._requestQueue.Dequeue();
                            /*
                             * Check to see if this work request languished in the queue
                             * very long.  If it was in the queue >= the new thread trigger
                             * time, and if we haven't reached the max thread count cap,
                             * add a new thread to the pool.
                             *
                             * If the decision is made, create the new thread object (updating
                             * the current # of threads in the pool), but defer starting the new
                             * thread until the lock is released.
                             */
                            TimeSpan requestTimeInQ = DateTime.Now.Subtract(wr.WorkingTime);

                            if (requestTimeInQ >= _pool._newThreadTrigger && _pool._currentThreadCount < _pool._maxThreadCount)
                            {
                                /*
                                 * Note - the constructor for ThreadWrapper will update
                                 * pool.currentThreadCount.
                                 */
                                newThread = new ThreadWrapper(_pool, false, _priority, string.Format("{0} (dynamic)", _pool._threadPoolName));

                                /*
                                 * Since the current request we just dequeued is stale,
                                 * everything else behind it in the queue is also stale.
                                 * So reset the timestamps of the remaining pending work
                                 * requests so that we don't start creating threads
                                 * for every subsequent request.
                                 */
                                _pool.ResetWorkRequestTimes();
                            }
                        }
                        else
                        {
                            /*
                             * Should only get here if this is a dynamic thread that
                             * timed out waiting for a work request, or if the pool
                             * is shutting down.
                             */
                            _pool._currentThreadCount--;

                            if (_pool._currentThreadCount == 0)
                            {
                                /*
                                 * Last one out turns off the lights.
                                 */
                                if (_pool.Stopped != null)
                                {
                                    _pool.Stopped();
                                }
                                _pool._stopCompleteEvent.Set();
                            }

                            done = true;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_pool._syncLock);
                    }

                    // No longer holding pool lock here...

                    if (!done && wr != null)
                    {
                        /*
                         * Check to see if this request has been cancelled while
                         * stuck in the work queue.
                         *
                         * If the work request was pending, mark it processed and proceed
                         * to handle.  Otherwise, the request must have been cancelled
                         * before we plucked it off the request queue.
                         */
                        if (Interlocked.CompareExchange(ref wr.State, WorkRequest.PROCESSED, WorkRequest.PENDING) != WorkRequest.PENDING)
                        {
                            /*
                             * Request was cancelled before we could get here.
                             * Bail out.
                             */
                            continue;
                        }
                        if (newThread != null)
                        {
                            newThread.Start();
                        }

                        /*
                         * Dispatch the work request.
                         */
                        ThreadInfo originalThreadInfo = null;
                        try
                        {
                            /*
                             * Impersonate (as much as possible) what we know about
                             * the thread that issued the work request.
                             */
                            originalThreadInfo = ThreadInfo.Impersonate(wr.ThreadInfo);

                            WorkRequestDelegate targetProc = wr.TargetProc as WorkRequestDelegate;

                            if (targetProc != null)
                            {
                                targetProc(wr.ProcArg, wr.TimeStampStarted);
                            }
                            else
                            {
                                wr.TargetProc.DynamicInvoke(wr.ProcArgs);
                            }
                        }
                        finally
                        {
                            /*
                             * Restore our worker thread's identity.
                             */
                            ThreadInfo.Restore(originalThreadInfo);
                        }
                    }
                }
            }
        }
    }
}
