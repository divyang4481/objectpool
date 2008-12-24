using System;

namespace SystemUtilities
{
    internal static class ExceptionHelper
    {
        public static void ThrowIfArgumentNull(object arg, string name)
        {
            if (arg == null) throw new ArgumentNullException(name);
        }

        public static void ThrowIfArgumentNullOrEmptyString(string arg, string name)
        {
            if (String.IsNullOrEmpty(arg)) throw new ArgumentException(String.Format(Resources.ArgumentNullOrEmptyString, name));
        }

        public static void ThrowIfDisposed(IDisposable obj)
        {
            if (obj.IsDisposed) throw new ObjectDisposedException(obj.GetType().FullName);
        }
    }
}
