using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using SystemUtilities.Caching;
using SystemUtilities.Data;
using SystemUtilities.Threading;

namespace SystemUtilities.Collections.Generic
{
    public abstract class ObjectPool<TKey, TValue> : IObjectPool<TKey, TValue>
    {
        private readonly Cache<TKey, TValue> _cache;
        private readonly ICollection<OnErrorEventHandler> _errorHandlers = new List<OnErrorEventHandler>();
        private readonly ICollection<OnFilledEventHandler> _filledHandlers = new List<OnFilledEventHandler>();
        private readonly Queue<IResultSet<TKey, TValue>> _updateRequests = new Queue<IResultSet<TKey, TValue>>();
        private bool _updateInProgress = false;
        private readonly object _syncLock = new object();

        public ObjectPool()
            : this(-1)
        {
        }

        public ObjectPool(int capacity)
        {
            _cache = new Cache<TKey, TValue>(Timeout.Infinite, capacity);
        }

        #region IObjectPool<TKey,TValue> Members

        public virtual TValue this[TKey key]
        {
            get { return _cache[key]; }
        }

        #endregion

        #region IFillable<TKey,TValue> Members

        public event OnErrorEventHandler Error
        {
            add
            {
                Monitor.Enter(_errorHandlers);
                try
                {
                    _errorHandlers.Add(value);
                }
                finally
                {
                    Monitor.Exit(_errorHandlers);
                }
            }
            remove
            {
                Monitor.Enter(_errorHandlers);
                try
                {
                    _errorHandlers.Remove(value);
                }
                finally
                {
                    Monitor.Exit(_errorHandlers);
                }
            }
        }

        public event OnFilledEventHandler Filled
        {
            add
            {
                Monitor.Enter(_filledHandlers);
                try
                {
                    _filledHandlers.Add(value);
                }
                finally
                {
                    Monitor.Exit(_filledHandlers);
                }
            }
            remove
            {
                Monitor.Enter(_filledHandlers);
                try
                {
                    _filledHandlers.Remove(value);
                }
                finally
                {
                    Monitor.Exit(_filledHandlers);
                }
            }
        }

        public virtual void Fill(IResultSet<TKey, TValue> results)
        {
            ExceptionHelper.ThrowIfArgumentNull(results, "results");
            Monitor.Enter(_syncLock);
            try
            {
                _updateRequests.Enqueue(results);
                if (!_updateInProgress)
                {
                    AsyncHelper.FireAndForget(new ObjectPoolDelegate(ProcessUpdates), null);
                }
            }
            finally
            {
                Monitor.Exit(_syncLock);
            }
        }

        #endregion

        #region IEnumerable<TValue> Members

        public virtual IEnumerator<TValue> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private void ProcessUpdates()
        {
            IResultSet<TKey, TValue> results = null;
            IEnumerator<KeyValuePair<TKey, TValue>> enumerator;
            KeyValuePair<TKey, TValue> item;

            do
            {
                Monitor.Enter(_syncLock);
                try
                {
                    if (_updateRequests.Count == 0)
                    {
                        _updateInProgress = false;
                    }
                    else
                    {
                        _updateInProgress = true;
                        results = _updateRequests.Dequeue();
                    }
                }
                finally
                {
                    Monitor.Exit(_syncLock);
                }

                if (_updateInProgress && results != null)
                {
                    using (results)
                    {
                        using (enumerator = results.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                try
                                {
                                    item = enumerator.Current;
                                    _cache.Add(item);
                                }
                                catch (Exception e)
                                {
                                    OnError(new OnErrorEventArgs(e));
                                }
                            }
                        }
                    }

                    OnFilled(new OnFilledEventArgs());
                }
            } while (_updateInProgress);
        }

        protected virtual void OnError(OnErrorEventArgs e)
        {
            OnErrorEventHandler[] handlers = null;
            Monitor.Enter(_errorHandlers);
            try
            {
                if (_errorHandlers.Count > 0)
                {
                    handlers = new OnErrorEventHandler[_errorHandlers.Count];
                    _errorHandlers.CopyTo(handlers, 0);
                }
            }
            finally
            {
                Monitor.Exit(_errorHandlers);
            }

            if (handlers != null)
            {
                foreach (OnErrorEventHandler handler in handlers)
                {
                    if (handler != null)
                        AsyncHelper.FireAndForget(handler, this, e);
                }
            }
        }

        protected virtual void OnFilled(OnFilledEventArgs e)
        {
            OnFilledEventHandler[] handlers = null;
            Monitor.Enter(_filledHandlers);
            try
            {
                if (_filledHandlers.Count > 0)
                {
                    handlers = new OnFilledEventHandler[_filledHandlers.Count];
                    _filledHandlers.CopyTo(handlers, 0);
                    _filledHandlers.Clear();
                }
            }
            finally
            {
                Monitor.Exit(_filledHandlers);
            }

            if (handlers != null)
            {
                foreach (OnFilledEventHandler handler in handlers)
                {
                    if (handler != null)
                        AsyncHelper.FireAndForget(handler, this, e);
                }
            }
        }
    }
}
