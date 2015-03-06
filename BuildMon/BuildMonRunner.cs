using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;

namespace BuildMon
{
    public class BuildMonRunner : IStartable
    {
        private readonly IEnumerable<IBuildDisplay> _buildDisplays;
        private readonly List<IBuildItem> _buildItems = new List<IBuildItem>();
        private readonly object _buildItemsLock = new object();
        private readonly IEnumerable<IBuildSource> _buildSources;
        private readonly Window _mainWindow;

        public BuildMonRunner(
            IMainWindow mainWindow,
            IBuildSource[] buildSources,
            IBuildDisplay[] buildDisplays)
        {
            _mainWindow = mainWindow as Window;
            if (_mainWindow != null)
                _mainWindow.Show();

            _buildSources = buildSources.ToArray();
            _buildDisplays = buildDisplays.ToArray();
        }

        public async void Start()
        {
            //Logger.Info("BuildMon starting");
            foreach (var source in _buildSources)
            {
                var buildItems = await source.StartMonitoring(addedCallback, removedCallback);
                lock (_buildItemsLock)
                {
                    _buildItems.AddRange(buildItems);
                }
            }

            foreach (var buildDisplay in _buildDisplays)
            {
                buildDisplay.SetBuildItems(_buildItems);
            }
        }

        private void removedCallback(IEnumerable<string> itemsToRemove)
        {
            lock (_buildItemsLock)
            {
                _buildItems.RemoveAll(i => itemsToRemove.Contains(i.Id));
            }

            foreach (var buildDisplay in _buildDisplays)
            {
                buildDisplay.RemoveBuildItems(itemsToRemove);
            }
        }

        private void addedCallback(IEnumerable<IBuildItem> buildItemsToAdd)
        {
            lock (_buildItemsLock)
            {
                _buildItems.AddRange(buildItemsToAdd);
            }

            foreach (var buildDisplay in _buildDisplays)
            {
                buildDisplay.AddBuildItems(buildItemsToAdd);
            }
        }
    }
}