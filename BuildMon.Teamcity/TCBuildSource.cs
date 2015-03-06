using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Serilog;

namespace BuildMon.Teamcity
{
    public class TCBuildSource : IBuildSource
    {
        private readonly ITCBuildConfig _buildConfig;
        //private CancellationTokenSource _cancellationTokenSource;
        private readonly TCBuildItem.Factory _buildItemFactory;
        private readonly List<IBuildSourceItem> _items = new List<IBuildSourceItem>();
        private readonly ILogger _logger;
        //public void StopMonitoring()
        //{
        //    if (_cancellationTokenSource != null)
        //        _cancellationTokenSource.Cancel(false);
        //}

        private Action<IEnumerable<IBuildSourceItem>> _addedCallback;
        private List<TeamcityBuildType> _buildTypes;
        private RestClient _client;
        private Action<IEnumerable<string>> _removedCallback;

        public TCBuildSource(ITCBuildConfig buildConfig, TCBuildItem.Factory buildItemFactory, ILogger logger)
        {
            _buildConfig = buildConfig;
            _buildItemFactory = buildItemFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<IBuildItem>> StartMonitoring(Action<IEnumerable<IBuildItem>> addedCallback,
            Action<IEnumerable<string>> removedCallback)
        {
            //_addedCallback = addedCallback;
            //_removedCallback = removedCallback;

            try
            {
                await Start();
                // _cancellationTokenSource = new CancellationTokenSource();
                PeriodicTask.RunAsync(MonitorBuildItems, TimeSpan.FromSeconds(10));
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
            var activeItemRequest = RequestGenerator(string.Format(requestTemplate, teamcityItem.id, "any"),
                _buildConfig.TeamcityIsGuest);
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

        public async Task Start()
        {
            _client = new RestClient();
            _client.BaseUrl = _buildConfig.TeamcityServer;
            if (!_buildConfig.TeamcityIsGuest)
            {
                _client.Authenticator = new HttpBasicAuthenticator(_buildConfig.TeamcityUser,
                    _buildConfig.TeamcityPassword);
            }

            var buildTypesRequest = RequestGenerator("buildTypes", _buildConfig.TeamcityIsGuest);
                // http://localhost:81/httpAuth/app/rest/buildTypes

            try
            {
                var buildTypesResponse = await _client.ExecuteTaskAsync<TeamcityBuildTypes>(buildTypesRequest);
                if (buildTypesResponse == null || buildTypesResponse.Data == null ||
                    buildTypesResponse.Data.buildType == null)
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
                _items.Add(_buildItemFactory(buildType.id, buildType.projectName, buildType.name));
            }
        }

        private IRestRequest RequestGenerator(string query, bool isGuestAuth)
        {
            return new RestRequest(string.Format("{0}/app/rest/{1}", isGuestAuth ? "guestAuth" : "httpAuth", query),
                Method.GET);
            //return new RestRequest(isGuestAuth ? "guestAuth" : "httpAuth" + "/app/rest/" + query, Method.GET);
        }
    }
}