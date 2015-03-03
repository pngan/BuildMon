using Autofac;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using log4net;
using Serilog;

namespace BuildMon.Teamcity
{
    public class TeamcityBuildItems
    {
        public List<TeamcityBuildItem> buildType { get; set; }
    }

    public class TeamcityBuildItem : TeamcityDescriptor{}

    public class TeamcityDescriptor
    {
        public string id { get; set; }

        public string name { get; set; }

        public string href { get; set; }
    }


    public class TeamcityBuildTypes
    {
        public List<TeamcityBuildType> buildType { get; set; }
    }
    
    public class TeamcityBuildType : TeamcityDescriptor
    {
        public string projectName { get; set; }

        public string projectId { get; set; }

        public string webUrl { get; set; }
    }

    public class TeamcityBuilds
    {
        public List<TeamcityBuild> build { get; set; }
    }

    public class TeamcityBuild : TeamcityDescriptor
    {
        public int percentageComplete { get; set; }

        public string status { get; set; }

        public string buildTypeId { get; set; }

        public bool running { get; set; }
    }

    public class TCBuildSource : IBuildSource
    {
        public ILog Logger { get; set; }

        public async Task<IEnumerable<IBuildItem>> StartMonitoring(Action<IEnumerable<IBuildItem>> addedCallback, Action<IEnumerable<string>> removedCallback)
        {
            //_addedCallback = addedCallback;
            //_removedCallback = removedCallback;

            try
            {
                await Start();
               // _cancellationTokenSource = new CancellationTokenSource();
                BuildMon.PeriodicTask.RunAsync(MonitorBuildItems, TimeSpan.FromSeconds(10));
//                BuildMon.PeriodicTask.RunAsync(MonitorBuildItems, TimeSpan.FromSeconds(1), _cancellationTokenSource.Token);

                return _items;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<IBuildItem>();
            }
            return Enumerable.Empty<IBuildItem>();
        }


        // http://localhost:81/httpAuth/app/rest/buildTypes/id:bt5/builds?locator=count:1,running:true
        //    <builds count="1" nextHref="/httpAuth/app/rest/buildTypes/id:bt5/builds?locator=count:1,running:true&count=1&start=1">
        //        <build id="48" number="9" running="true" percentageComplete="100" status="SUCCESS" buildTypeId="bt5" startDate="20141201T202134+1300" href="/httpAuth/app/rest/builds/id:48" webUrl="http://localhost:81/viewLog.html?buildId=48&buildTypeId=bt5"/>
        //    </builds>
        private async Task UpdateSingleTeamcityItem(TeamcityDescriptor teamcityItem)
        //private async Task UpdateSingleTeamcityItem(TeamcityDescriptor teamcityItem, CancellationToken cancellationToken)
        {
            // First check to see if the build item is a running configuration
            var requestTemplate = "buildTypes/id:{0}/builds?locator=count:1,running:{1}";
            var activeItemRequest = RequestGenerator(string.Format(requestTemplate, teamcityItem.id, "any"), _buildConfig.TeamcityIsGuest);
            var activeItemResponse = await _client.ExecuteTaskAsync<TeamcityBuilds>(activeItemRequest);
            //var activeItemResponse = await _client.ExecuteTaskAsync<TeamcityBuilds>(activeItemRequest, cancellationToken);
            if (activeItemResponse != null
                && activeItemResponse.Data != null
                && activeItemResponse.Data.build != null
                && activeItemResponse.Data.build.Any())
            {
                var item = activeItemResponse.Data.build.First();

                var buildItem = _items.FirstOrDefault(i => i.Id == item.buildTypeId);
                if (buildItem == null)
                    return;
                buildItem.IsFailure = string.Equals(item.status, "FAILURE");

                buildItem.ProgressPercentage = item.running ? item.percentageComplete : (int?) null;
            }
        }

        private async Task MonitorBuildItems()
        {
            _logger.Information("Monitoring Teamcity Build Item at {Time}", DateTime.Now);
            //Logger.Info("Monitoring Teamcityy Build Item");
            foreach (var buildType in _buildTypes)
            {
                await UpdateSingleTeamcityItem(buildType);
                //await UpdateSingleTeamcityItem(buildType, _cancellationTokenSource.Token);
            }
        }

        //public void StopMonitoring()
        //{
        //    if (_cancellationTokenSource != null)
        //        _cancellationTokenSource.Cancel(false);
        //}

        private Action<IEnumerable<IBuildSourceItem>> _addedCallback;
        private Action<IEnumerable<string>> _removedCallback;
        private ITCBuildConfig _buildConfig;
        private RestClient _client;
        //private CancellationTokenSource _cancellationTokenSource;
        private TCBuildItem.Factory _buildItemFactory;

        private List<IBuildSourceItem> _items = new List<IBuildSourceItem>();
        private List<TeamcityBuildType> _buildTypes;
        private ILogger _logger;

        public TCBuildSource(ITCBuildConfig buildConfig, TCBuildItem.Factory buildItemFactory, ILogger logger)
        {
            _buildConfig = buildConfig;
            _buildItemFactory = buildItemFactory;
            _logger = logger;
        }

        public async Task Start()
        {
            _client = new RestClient();
            _client.BaseUrl = _buildConfig.TeamcityServer;
            if (!_buildConfig.TeamcityIsGuest)
            {
                _client.Authenticator = new HttpBasicAuthenticator(_buildConfig.TeamcityUser, _buildConfig.TeamcityPassword);
            }

            var buildTypesRequest = RequestGenerator("buildTypes", _buildConfig.TeamcityIsGuest); // http://localhost:81/httpAuth/app/rest/buildTypes

            try
            {
                var buildTypesResponse = await _client.ExecuteTaskAsync<TeamcityBuildTypes>(buildTypesRequest);
                if (buildTypesResponse == null || buildTypesResponse.Data == null || buildTypesResponse.Data.buildType == null)
                    return;
                _buildTypes = buildTypesResponse.Data.buildType;
            }
            catch (Exception ex)
            {

                return;
            }

            // Create list of BuildItems from Configuration file
            foreach (var buildId in _buildConfig.BuildIds())
            {
                var buildType = _buildTypes.FirstOrDefault(bt => bt.id == buildId);
                if (buildType == null)
                    continue;
                _items.Add( _buildItemFactory( buildType.id, buildType.projectName, buildType.name ));
            }
        }

        private IRestRequest RequestGenerator(string query, bool isGuestAuth )
        {
            return new RestRequest(string.Format("{0}/app/rest/{1}", isGuestAuth ? "guestAuth" : "httpAuth", query), Method.GET);
            //return new RestRequest(isGuestAuth ? "guestAuth" : "httpAuth" + "/app/rest/" + query, Method.GET);
        }
    }

    public interface ITCBuildConfig
    {
        string TeamcityServer { get; }

        string TeamcityUser { get; }

        string TeamcityPassword { get; }

        bool TeamcityIsGuest { get; }

        IEnumerable<string> BuildIds();
    }

    public class TCBuildConfig : ITCBuildConfig, IBuildConfig
    {
        public TCBuildConfig(IDllConfiguration config )
        {
            _buildIds = new List<string>(config.GetSetting( "BuildIds").Split(new char[] { ';' }));
            _teamcityServer = config.GetSetting("TeamcityServer");
            _teamcityUser = config.GetSetting("Username");
            _teamcityPassword = config.GetSetting("Password");
            bool isGuest;
            if (bool.TryParse(config.GetSetting("IsGuest"), out isGuest))
                _isGuest = isGuest;
        }

        public string TeamcityServer { get { return _teamcityServer; } }

        public string TeamcityUser
        {
            get { return _teamcityUser; }
        }

        public string TeamcityPassword
        {
            get { return _teamcityPassword; }
        }

        public bool TeamcityIsGuest
        {
            get { return _isGuest; }
        }

        public IEnumerable<string> BuildIds()
        {
            return _buildIds;
        }

        private List<string> _buildIds;
        private string _teamcityServer;
        private string _teamcityUser;
        private string _teamcityPassword;
        private bool _isGuest;
    }

    public interface IDllConfiguration
    {
        string GetSetting(string key);
    }

    public class DllConfiguration : IDllConfiguration
    {
        private Configuration _config = null;

        public DllConfiguration()
        {
            string exeConfigPath = this.GetType().Assembly.Location;
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
            KeyValueConfigurationElement element = _config.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }
    }

    public class TCBuildItem : IBuildSourceItem
    {
        public delegate IBuildSourceItem Factory(string id, string projectName, string itemName);

        public string Id { get; private set; }

        public string ItemName { get; private set; }

        public event EventHandler<BuildStateChangedArgs> BuildStateChanged;
        public event EventHandler<ProgressPercentageChangedArgs> ProgressPercentageChanged;

        public TCBuildItem(string id, string projectName, string itemName)
        {
            Id = id;
            ProjectName = projectName;
            ItemName = itemName;
        }

        private bool _isFailure;
        private int? _percentageProgress;

        public bool IsFailure
        {
            get { return _isFailure; }
            set
            {
                if (_isFailure == value) return;
                _isFailure = value;
                BuildStateChanged.Raise<BuildStateChangedArgs>(this, new BuildStateChangedArgs(value));
            }
        }

        private int? _progressPercentage;
        public int? ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                if (_progressPercentage == value) return;
                _progressPercentage = value;
                ProgressPercentageChanged.Raise<ProgressPercentageChangedArgs>(this, new ProgressPercentageChangedArgs(value));
            }
        }

        public string ProjectName
        {
            get;
            private set;
        }
    }
}