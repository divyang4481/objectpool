using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SystemUtilities.Threading;
using SystemUtilities.Collections.Generic;

namespace SystemUtilities.Caching
{
    internal sealed class Cache<TKey, TValue> : IEnumerable<TValue>
    {
        private int _count = 0;
        private readonly IDictionary<TKey, TValue> _dict;
        private readonly ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly int _timeout;

        public Cache()
            : this(Timeout.Infinite)
        {
        }

        public Cache(int millisecondsTimeout)
            : this(millisecondsTimeout, -1)
        {
        }

        public Cache(int millisecondsTimeout, int capacity)
        {
            _timeout = millisecondsTimeout;
            if (capacity < 0)
            {
                _dict = new Dictionary<TKey, TValue>();
            }
            else
            {
                _dict = new Dictionary<TKey, TValue>(capacity);
            }
        }

        public Cache(TimeSpan timeout)
            : this(timeout.Milliseconds)
        {
        }

        public Cache(TimeSpan timeout, int capacity)
            : this(timeout.Milliseconds, capacity)
        {
        }

        public int Count
        {
            get
            {
                using (ReadOnlyLock readLock = new ReadOnlyLock(_rwl, _timeout))
                {
                    if (!readLock.IsLockAcquired)
                    {
                        return 0;
                    }
                    return _count;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                using (ReadOnlyLock readLock = new ReadOnlyLock(_rwl, _timeout))
                {
                    if (!readLock.IsLockAcquired)
                    {
                        return default(TValue);
                    }
                    TValue value;
                    _dict.TryGetValue(key, out value);
                    return value;
                }
            }
            set
            {
                Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            using (WriteLock writeLock = new WriteLock(_rwl, _timeout))
            {
                if (writeLock.IsLockAcquired)
                {
                    if (_dict.Remove(item.Key))
                    {
                        _count--;
                    }
                    if (HasValue(item))
                    {
                        _dict.Add(item);
                        _count++;
                    }
                }
            }
        }

        private static bool HasValue(KeyValuePair<TKey, TValue> item)
        {
            TValue value = item.Value;
            if ((Object)value == null)
                return false;

            if (value is ValueType && value.Equals(default(TValue)))
                return false;

            return true;
        }

        #region IEnumerable<TValue> Members

        public IEnumerator<TValue> GetEnumerator()
        {
            ReadOnlyLock readLock = new ReadOnlyLock(_rwl, _timeout);
            if (!readLock.IsLockAcquired)
            {
                return (new List<TValue>()).GetEnumerator();
            }
            return new SynchronizedEnumerator<TValue>(_dict.Values.GetEnumerator(), readLock);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
