using System;

namespace SystemUtilities
{
    public sealed class OnErrorEventArgs : EventArgs
    {
        private Exception _e;

        internal OnErrorEventArgs(Exception e)
        {
            _e = e;
        }

        public Exception Exception
        {
            get { return _e; }
        }
    }
}
