using System;
using System.Threading;

namespace SystemUtilities.Threading
{
    internal sealed class ReadLock : BaseLock
    {
        private bool _lockAcquired = false;

        public ReadLock(ReaderWriterLockSlim rwl)
            : this(rwl, Timeout.Infinite)
        {
        }

        public ReadLock(ReaderWriterLockSlim rwl, int millisecondsTimeout)
            : base(rwl)
        {
            if (millisecondsTimeout == Timeout.Infinite)
            {
                while (!_lockAcquired)
                    _lockAcquired = rwl.TryEnterUpgradeableReadLock(1);
            }
            else
            {
                _lockAcquired = rwl.TryEnterUpgradeableReadLock(millisecondsTimeout);
            }
        }

        public ReadLock(ReaderWriterLockSlim rwl, TimeSpan timeout)
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
                if (!IsDisposed && _lockAcquired && Lock.IsUpgradeableReadLockHeld)
                {
                    Lock.ExitUpgradeableReadLock();
                }
            }
            base.Dispose(disposing);
        }
    }
}
