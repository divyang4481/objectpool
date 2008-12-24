using System;
using System.Threading;

namespace SystemUtilities.Threading
{
    internal sealed class WriteLock : BaseLock
    {
        private bool _lockAcquired = false;

        public WriteLock(ReaderWriterLockSlim rwl)
            : this(rwl, Timeout.Infinite)
        {
        }

        public WriteLock(ReaderWriterLockSlim rwl, int millisecondsTimeout)
            : base(rwl)
        {
            if (millisecondsTimeout == Timeout.Infinite)
            {
                while (!_lockAcquired)
                    _lockAcquired = rwl.TryEnterWriteLock(1);
            }
            else
            {
                _lockAcquired = rwl.TryEnterWriteLock(millisecondsTimeout);
            }
        }

        public WriteLock(ReaderWriterLockSlim rwl, TimeSpan timeout)
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
                if (!IsDisposed && _lockAcquired && Lock.IsWriteLockHeld)
                {
                    Lock.ExitWriteLock();
                }
            }
            base.Dispose(disposing);
        }
    }
}
