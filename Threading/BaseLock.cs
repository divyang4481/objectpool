using System;
using System.Threading;

namespace SystemUtilities.Threading
{
    internal abstract class BaseLock : DisposableObject
    {
        private readonly ReaderWriterLockSlim _rwl;

        protected BaseLock(ReaderWriterLockSlim rwl)
        {
            ExceptionHelper.ThrowIfArgumentNull(rwl, "rwl");
            _rwl = rwl;
        }

        public abstract bool IsLockAcquired { get; }

        protected ReaderWriterLockSlim Lock
        {
            get { return _rwl; }
        }
    }
}
