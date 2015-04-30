using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Serilog;

namespace BuildMon.Teamcity
{
    public class TcBuildSource : IBuildSource
    {
        private readonly ITcBuildConfig _buildConfig;
        private readonly TcBuildItem.Factory _buildItemFactory;
        private readonly List<IBuildSourceItem> _items = new List<IBuildSourceItem>();
        private readonly ILogger _logger;

        private List<TeamcityBuildType> _buildTypes;
        private RestClient _client;

        public TcBuildSource(ITcBuildConfig buildConfig, TcBuildItem.Factory buildItemFactory, ILogger logger)
        {
            _buildConfig = buildConfig;
            _buildItemFactory = buildItemFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<IBuildItem>> StartMonitoring(Action<IEnumerable<IBuildItem>> addedCallback,
            Action<IEnumerable<string>> removedCallback)
        {
            try
            {
                await InitializeTeamcityBuildSource();
                PeriodicTask.RunAsync(MonitorBuildItems, TimeSpan.FromSeconds(10));

                return _items;
            }
            catch (Exception)
            {
                return Enumerable.Empty<IBuildItem>();
            }
        }


        private async Task InitializeTeamcityBuildSource()
        {
            _client = new RestClient {BaseUrl = _buildConfig.TeamcityServer};
            if (!_buildConfig.TeamcityIsGuest)
            {
                _client.Authenticator = new HttpBasicAuthenticator(_buildConfig.TeamcityUser,
                    _buildConfig.TeamcityPassword);
            }

            var buildTypesRequest = RequestGenerator("buildTypes", _buildConfig.TeamcityIsGuest);
            try
            {
                var buildTypesResponse = await _client.ExecuteTaskAsync<TeamcityBuildTypes>(buildTypesRequest);
                if (buildTypesResponse == null || buildTypesResponse.Data == null ||
                    buildTypesResponse.Data.buildType == null)
                    return;
                var buildIds = _buildConfig.BuildIds().ToList();
                _buildTypes = buildTypesResponse.Data.buildType.Where(bt => buildIds.Contains(bt.id)).ToList();
            }
            catch (Exception)
            {
                _logger.Error("An error occurred while requesting the url {Request}", _client.BaseUrl + "/" + buildTypesRequest.Resource);
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

        private async Task MonitorBuildItems()
        {
            _logger.Information("Monitoring Teamcity Build Item at {Time}", DateTime.Now);
            foreach (var buildType in _buildTypes)
            {
                await UpdateSingleTeamcityItem(buildType);
            }
        }

        // http://localhost:81/httpAuth/app/rest/buildTypes/id:bt5/builds?locator=count:1,running:true
        //    <builds count="1" nextHref="/httpAuth/app/rest/buildTypes/id:bt5/builds?locator=count:1,running:true&count=1&start=1">
        //        <build id="48" number="9" running="true" percentageComplete="100" status="SUCCESS" buildTypeId="bt5" startDate="20141201T202134+1300" href="/httpAuth/app/rest/builds/id:48" webUrl="http://localhost:81/viewLog.html?buildId=48&buildTypeId=bt5"/>
        //    </builds>
        private async Task UpdateSingleTeamcityItem(TeamcityDescriptor teamcityItem)
        {
            // First check to see if the build item is a running configuration
            const string requestTemplate = "buildTypes/id:{0}/builds?locator=count:1,running:{1}";
            var activeItemRequest = RequestGenerator(string.Format(requestTemplate, teamcityItem.id, "any"),
                _buildConfig.TeamcityIsGuest);
            try
            {
                var activeItemResponse = await _client.ExecuteTaskAsync<TeamcityBuilds>(activeItemRequest);
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

                    buildItem.ProgressPercentage = item.running ? item.percentageComplete : (int?)null;
                }
            }
            catch (Exception)
            {
                _logger.Error("An error occurred while requesting the url {Request}", _client.BaseUrl + "/" + activeItemRequest.Resource);
            }
        }


        private IRestRequest RequestGenerator(string query, bool isGuestAuth)
        {
            return new RestRequest(string.Format("{0}/app/rest/{1}", isGuestAuth ? "guestAuth" : "httpAuth", query),
                Method.GET);
        }
    }
}