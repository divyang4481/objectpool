using System;
using System.Threading;
using System.Collections.Generic;
using SystemUtilities.Threading;

namespace SystemUtilities
{
    [Serializable]
    public abstract class DisposableObject : IDisposable
    {
        [NonSerialized]
        private bool _disposed;

        [NonSerialized]
        private ICollection<OnDisposedEventHandler> _handlers;

        #region IDisposable Members

        public event OnDisposedEventHandler Disposed
        {
            add
            {
                ExceptionHelper.ThrowIfDisposed(this);
                if (_handlers == null)
                {
                    Monitor.Enter(this);
                    {
                        try
                        {
                            if (_handlers == null)
                            {
                                _handlers = new List<OnDisposedEventHandler>();
                            }
                        }
                        finally
                        {
                            Monitor.Exit(this);
                        }
                    }
                }
                _handlers.Add(value);
            }
            remove
            {
                if (_handlers != null)
                {
                    Monitor.Enter(this);
                    try
                    {
                        _handlers.Remove(value);
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
            }
        }

        public virtual bool IsDisposed
        {
            get { return _disposed; }
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed)
                {
                    _disposed = true;
                    OnDisposed(new OnDisposedEventArgs());
                }
            }
        }

        #region System.IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~DisposableObject()
        {
            Dispose(false);
        }

        protected virtual void OnDisposed(OnDisposedEventArgs e)
        {
            if (_handlers != null)
            {
                OnDisposedEventHandler[] handlers = null;
                Monitor.Enter(this);
                try
                {
                    handlers = new OnDisposedEventHandler[_handlers.Count];
                    _handlers.CopyTo(handlers, 0);
                    _handlers.Clear();
                }
                finally
                {
                    Monitor.Exit(this);
                }

                if (handlers != null)
                {
                    foreach (OnDisposedEventHandler handler in handlers)
                    {
                        AsyncHelper.FireAndForget(handler, this, e);
                    }
                }
            }
        }
    }
}
