using System;
using System.Threading;

namespace SystemUtilities.Threading
{
    internal sealed class ReadOnlyLock : BaseLock
    {
        private bool _lockAcquired = false;

        public ReadOnlyLock(ReaderWriterLockSlim rwl)
            : this(rwl, Timeout.Infinite)
        {
        }

        public ReadOnlyLock(ReaderWriterLockSlim rwl, int millisecondsTimeout)
            : base(rwl)
        {
            if (millisecondsTimeout == Timeout.Infinite)
            {
                while (!_lockAcquired)
                    _lockAcquired = rwl.TryEnterReadLock(1);
            }
            else
            {
                _lockAcquired = rwl.TryEnterReadLock(millisecondsTimeout);
            }
        }

        public ReadOnlyLock(ReaderWriterLockSlim rwl, TimeSpan timeout)
            : this(rwl, timeout.Milliseconds)
        {
        }

        public override bool IsLockAcquired
        {
            get
            {
                ExceptionHelper.ThrowIfDisposed(this);
                return _lockAcquired;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed && _lockAcquired && Lock.IsReadLockHeld)
                {
                    Lock.ExitReadLock();
                }
            }
            base.Dispose(disposing);
        }
    }
}
