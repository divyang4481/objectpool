using System;
using System.Collections.Generic;
using SystemUtilities.Threading;

namespace SystemUtilities.Collections.Generic
{
    internal sealed class SynchronizedEnumerator<T> : Enumerator<T>
    {
        private readonly ReadOnlyLock _lock;

        public SynchronizedEnumerator(IEnumerator<T> enumerator, ReadOnlyLock l)
            : base(enumerator)
        {
            _lock = l;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed)
                {
                    if (_lock != null)
                    {
                        _lock.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}
