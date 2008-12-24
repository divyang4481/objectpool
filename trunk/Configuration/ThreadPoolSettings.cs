using System;

namespace SystemUtilities.Configuration
{
    internal sealed class ThreadPoolSettings
    {
        private readonly int _initialThreads;
        private readonly int _maxThreads;
        private readonly string _name;

        public ThreadPoolSettings(ThreadPoolSection settings)
        {
            if (settings == null)
            {
                _initialThreads = 2;
                _maxThreads = 5;
                _name = "AsyncHelperThreadPool";
            }
            else
            {
                _initialThreads = settings.InitialThreads;
                _maxThreads = settings.MaxThreads;
                _name = settings.Name;
            }
        }

        public int InitialThreads
        {
            get { return _initialThreads; }
        }

        public int MaxThreads
        {
            get { return _maxThreads; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
