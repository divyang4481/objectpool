using System;

namespace SystemUtilities.Threading
{
    internal delegate void WorkRequestDelegate(object state, DateTime requestEnqueueTime);
}
