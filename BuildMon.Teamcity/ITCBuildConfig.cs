using System.Collections.Generic;

namespace BuildMon.Teamcity
{
    public interface ITCBuildConfig
    {
        string TeamcityServer { get; }
        string TeamcityUser { get; }
        string TeamcityPassword { get; }
        bool TeamcityIsGuest { get; }
        IEnumerable<string> BuildIds();
    }
}