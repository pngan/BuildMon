using System.Collections.Generic;

namespace BuildMon.Teamcity
{
    public class TCBuildConfig : ITCBuildConfig, IBuildConfig
    {
        private readonly List<string> _buildIds;

        public TCBuildConfig(IDllConfiguration config)
        {
            _buildIds = new List<string>(config.GetSetting("BuildIds").Split(';'));
            TeamcityServer = config.GetSetting("TeamcityServer");
            TeamcityUser = config.GetSetting("Username");
            TeamcityPassword = config.GetSetting("Password");
            bool isGuest;
            if (bool.TryParse(config.GetSetting("IsGuest"), out isGuest))
                TeamcityIsGuest = isGuest;
        }

        public string TeamcityServer { get; private set; }
        public string TeamcityUser { get; private set; }
        public string TeamcityPassword { get; private set; }
        public bool TeamcityIsGuest { get; private set; }

        public IEnumerable<string> BuildIds()
        {
            return _buildIds;
        }
    }
}