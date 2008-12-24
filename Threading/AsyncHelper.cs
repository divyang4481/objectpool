using System;
using System.Threading;
using SystemUtilities.Configuration;

namespace SystemUtilities.Threading
{
    internal static class AsyncHelper
    {
        private static readonly object _syncLock = new object();
        private static readonly ThreadPool _threadPool = new ThreadPool(
                                ConfigurationManager.ThreadPool.InitialThreads,
                                ConfigurationManager.ThreadPool.MaxThreads,
                                ConfigurationManager.ThreadPool.Name);

        public static void FireAndForget(Delegate d, params object[] args)
        {
            StartThreadPool();

            _threadPool.PostRequest(d, args);
        }

        private static void StartThreadPool()
        {
            if (!_threadPool.IsStarted)
            {
                Monitor.Enter(_syncLock);
                try
                {
                    if (!_threadPool.IsStarted)
                    {
                        _threadPool.Start();
                    }
                }
                finally
                {
                    Monitor.Exit(_syncLock);
                }
            }
        }
    }
}
