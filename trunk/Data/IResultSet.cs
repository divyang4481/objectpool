using System;
using System.Collections.Generic;

namespace SystemUtilities.Data
{
    public interface IResultSet<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
    {
        int Count { get; }
    }
}
