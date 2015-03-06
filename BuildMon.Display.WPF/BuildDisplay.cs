using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace BuildMon.Display.WPF
{
    public class BuildDisplay : ViewModelBase, IBuildDisplay
    {
        private readonly DisplayBuildItem.Factory _buildItemFactory;

        private readonly ObservableCollection<IBuildDisplayItem> _buildItems =
            new ObservableCollection<IBuildDisplayItem>();

        public BuildDisplay(DisplayBuildItem.Factory buildItemFactory)
        {
            _buildItemFactory = buildItemFactory;
        }

        public IEnumerable<IBuildDisplayItem> BuildItems
        {
            get { return _buildItems; }
        }

        public void SetBuildItems(IEnumerable<IBuildItem> items)
        {
            foreach (var item in items)
            {
                var buildItem = _buildItemFactory(item.Id, item.ProjectName, item.ItemName, item.IsFailure,
                    item.ProgressPercentage);
                Dispatcher.CurrentDispatcher.Invoke(() => _buildItems.Add(buildItem));
                item.BuildStateChanged += buildItem_BuildStateChanged;
                item.ProgressPercentageChanged += item_ProgressPercentageChanged;
            }
        }

        public void AddBuildItems(IEnumerable<IBuildItem> items)
        {
            throw new NotImplementedException();
        }

        public void RemoveBuildItems(IEnumerable<string> items)
        {
            throw new NotImplementedException();
        }

        private void item_ProgressPercentageChanged(object sender, ProgressPercentageChangedArgs e)
        {
            var progressPercentage = e.PercentageProgress;
            var item = (IBuildItem) sender;

            foreach (var buildItem in _buildItems)
            {
                if (buildItem.Id == item.Id)
                {
                    buildItem.ProgressPercentage = progressPercentage;
                    break;
                }
            }
        }

        private void buildItem_BuildStateChanged(object sender, BuildStateChangedArgs e)
        {
            var isFailure = e.IsFailure;
            var item = (IBuildItem) sender;

            foreach (var buildItem in _buildItems)
            {
                if (buildItem.Id == item.Id)
                {
                    buildItem.IsFailure = isFailure;
                    break;
                }
            }
        }
    }
}