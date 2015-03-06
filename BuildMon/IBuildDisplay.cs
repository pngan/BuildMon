using System.Collections.Generic;

namespace BuildMon
{
    public interface IBuildDisplay
    {
        void SetBuildItems(IEnumerable<IBuildItem> items);
        void AddBuildItems(IEnumerable<IBuildItem> items);
        void RemoveBuildItems(IEnumerable<string> items);
    }
}