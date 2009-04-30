using System;
using System.Configuration;
using System.Threading;

namespace SystemUtilities.Configuration
{
    internal static class ConfigurationManager
    {
        private const string CONVERTER_SECTION = "serialization/converter";
        private const string THREAD_POOL_SECTION = "threading/customThreadPool";

        private static ConverterSettings _converterSettings = null;
        private static ThreadPoolSettings _threadPoolSettings = null;
        private static readonly object _syncLock = new object();

        public static ConverterSettings Converter
        {
            get
            {
                if (_converterSettings == null)
                {
                    Monitor.Enter(_syncLock);
                    try
                    {
                        if (_converterSettings == null)
                        {
                            _converterSettings = new ConverterSettings((ConverterSection)System.Configuration.ConfigurationManager.GetSection(CONVERTER_SECTION));
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_syncLock);
                    }
                }
                return _converterSettings;
            }
        }

        public static ThreadPoolSettings ThreadPool
        {
            get
            {
                if (_threadPoolSettings == null)
                {
                    Monitor.Enter(_syncLock);
                    try
                    {
                        if (_converterSettings == null)
                        {
                            _threadPoolSettings = new ThreadPoolSettings((ThreadPoolSection)System.Configuration.ConfigurationManager.GetSection(THREAD_POOL_SECTION));
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_syncLock);
                    }
                }
                return _threadPoolSettings;
            }
        }
    }
}
