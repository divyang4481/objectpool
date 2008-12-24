﻿using System;
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
        private readonly Cache<TKey, TValue> _cache = new Cache<TKey, TValue>();
        private readonly ICollection<OnErrorEventHandler> _errorHandlers = new List<OnErrorEventHandler>();
        private readonly ICollection<OnFilledEventHandler> _filledHandlers = new List<OnFilledEventHandler>();
        private readonly Queue<IResultSet<TKey, TValue>> _updateRequests = new Queue<IResultSet<TKey, TValue>>();
        private bool _updateInProgress = false;
        private readonly object _syncLock = new object();

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
                    _updateInProgress = true;
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

        public IEnumerator<TValue> GetEnumerator()
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
            ICollection<Exception> exceptions = null;
            while (_updateInProgress)
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
                                    if (exceptions == null)
                                    {
                                        exceptions = new List<Exception>();
                                    }
                                    exceptions.Add(e);
                                }
                            }
                        }
                    }

                    if (exceptions != null)
                    {
                        OnError(new OnErrorEventArgs(exceptions));
                        exceptions.Clear();
                    }

                    OnFilled(new OnFilledEventArgs());
                }
            }
        }

        protected virtual void OnError(OnErrorEventArgs e)
        {
            ICollection<OnErrorEventHandler> handlers = null;
            Monitor.Enter(_errorHandlers);
            try
            {
                handlers = new List<OnErrorEventHandler>(_errorHandlers.Count);
                foreach (OnErrorEventHandler handler in _errorHandlers)
                {
                    if (handler != null)
                        handlers.Add(handler);
                }
                _errorHandlers.Clear();
            }
            finally
            {
                Monitor.Exit(_errorHandlers);
            }

            if (handlers != null)
            {
                foreach (OnErrorEventHandler handler in handlers)
                {
                    AsyncHelper.FireAndForget(handler, this, e);
                }
            }
        }

        protected virtual void OnFilled(OnFilledEventArgs e)
        {
            ICollection<OnFilledEventHandler> handlers = null;
            Monitor.Enter(_filledHandlers);
            try
            {
                handlers = new List<OnFilledEventHandler>(_filledHandlers.Count);
                foreach (OnFilledEventHandler handler in _filledHandlers)
                {
                    if (handler != null)
                        handlers.Add(handler);
                }
                _filledHandlers.Clear();
            }
            finally
            {
                Monitor.Exit(_filledHandlers);
            }

            if (handlers != null)
            {
                foreach (OnFilledEventHandler handler in handlers)
                {
                    AsyncHelper.FireAndForget(handler, this, e);
                }
            }
        }
    }
}