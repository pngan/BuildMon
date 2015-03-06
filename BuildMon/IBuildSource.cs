using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildMon
{
    public interface IBuildSource
    {
        Task<IEnumerable<IBuildItem>> StartMonitoring(Action<IEnumerable<IBuildItem>> addedCallback,
            Action<IEnumerable<string>> removedCallback);
    }
}