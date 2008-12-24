using System;

namespace SystemUtilities
{
    public interface IDisposable : System.IDisposable
    {
        event OnDisposedEventHandler Disposed;
        bool IsDisposed { get; }
    }
}
