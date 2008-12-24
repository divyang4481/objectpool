using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemUtilities.Collections.Generic
{
    internal abstract class Enumerator<T> : DisposableObject, IEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public Enumerator(IEnumerator<T> enumerator)
        {
            ExceptionHelper.ThrowIfArgumentNull(enumerator, "enumerator");
            _enumerator = enumerator;
        }

        #region IEnumerator<T> Members

        public virtual T Current
        {
            get
            {
                ExceptionHelper.ThrowIfDisposed(this);
                return _enumerator.Current;
            }
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public virtual bool MoveNext()
        {
            ExceptionHelper.ThrowIfDisposed(this);
            return _enumerator.MoveNext();
        }

        public virtual void Reset()
        {
            ExceptionHelper.ThrowIfDisposed(this);
            _enumerator.Reset(); ;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed)
                {
                    _enumerator.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
