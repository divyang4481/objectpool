using System;

namespace SystemUtilities.Threading
{
    internal interface IWorkRequest
    {
        bool Cancel();
    }
}
