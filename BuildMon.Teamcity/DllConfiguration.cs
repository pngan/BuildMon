using System;
using System.Configuration;

namespace BuildMon.Teamcity
{
    public class DllConfiguration : IDllConfiguration
    {
        private readonly Configuration _config;

        public DllConfiguration()
        {
            var exeConfigPath = GetType().Assembly.Location;
            try
            {
                _config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            }
            catch (Exception ex)
            {
            }
        }

        public string GetSetting(string key)
        {
            var element = _config.AppSettings.Settings[key];
            if (element != null)
            {
                var value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }
    }
}