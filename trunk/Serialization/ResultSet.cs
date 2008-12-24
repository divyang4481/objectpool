using System;
using System.Collections;
using System.Collections.Generic;
using SystemUtilities.Data;
using SystemUtilities.Collections.Generic;

namespace SystemUtilities.Serialization
{
    internal sealed class ResultSet<TKey, TValue, TResult> : DisposableObject, IResultSet<TKey, TResult>
        where TValue : IDeserializable
    {
        private int _count = 0;
        private readonly IDictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get
            {
                ExceptionHelper.ThrowIfDisposed(this);
                TValue value;
                _dict.TryGetValue(key, out value);
                return value;
            }
            set
            {
                ExceptionHelper.ThrowIfDisposed(this);
                ExceptionHelper.ThrowIfArgumentNull(value, "value");
                if (!_dict.Remove(key))
                {
                    _count++;
                }
                _dict.Add(key, value);
            }
        }

        #region IResultSet<TKey,TResult> Members

        public int Count
        {
            get
            {
                ExceptionHelper.ThrowIfDisposed(this);
                return _count;
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TResult>> Members

        public IEnumerator<KeyValuePair<TKey, TResult>> GetEnumerator()
        {
            ExceptionHelper.ThrowIfDisposed(this);
            return new Enumerator(_dict.GetEnumerator());
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private sealed class Enumerator : Enumerator<KeyValuePair<TKey, TValue>>, IEnumerator<KeyValuePair<TKey, TResult>>
        {
            public Enumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
                : base(enumerator)
            {
            }

            #region IEnumerator<KeyValuePair<TKey,TResult>> Members

            KeyValuePair<TKey, TResult> IEnumerator<KeyValuePair<TKey, TResult>>.Current
            {
                get
                {
                    KeyValuePair<TKey, TValue> item = base.Current;
                    return new KeyValuePair<TKey, TResult>(item.Key, SerializationHelper.Deserialize<TResult>(item.Value));
                }
            }

            #endregion
        }
    }
}
