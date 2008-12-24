using System;
using System.Collections.Generic;
using SystemUtilities.Data;

namespace SystemUtilities.Collections.Generic
{
    public interface IObjectPool<TKey, TValue> : IEnumerable<TValue>, IFillable<TKey, TValue>
    {
        TValue this[TKey key] { get; }
    }
}
