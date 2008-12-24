using System;

namespace SystemUtilities.Data
{
    public interface IFillable<TKey, TValue>
    {
        event OnErrorEventHandler Error;
        event OnFilledEventHandler Filled;
        void Fill(IResultSet<TKey, TValue> results);
    }
}
