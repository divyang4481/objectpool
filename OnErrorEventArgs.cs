using System;
using System.Collections.Generic;

namespace SystemUtilities
{
    public sealed class OnErrorEventArgs : EventArgs
    {
        private Exception[] _arr;

        internal OnErrorEventArgs(Exception e)
        {
            _arr = new Exception[] { e };
        }

        internal OnErrorEventArgs(ICollection<Exception> collect)
        {
            _arr = new Exception[collect.Count];
            collect.CopyTo(_arr, 0);
        }

        public Exception[] Exceptions
        {
            get { return _arr; }
        }
    }
}
