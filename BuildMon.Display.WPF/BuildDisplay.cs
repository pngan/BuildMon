using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Threading;

namespace BuildMon.Display.WPF
{
    public class DisplayBuildItem : ViewModelBase, IBuildDisplayItem
    {
        public delegate IBuildDisplayItem Factory(string id, string projectName, string itemName, bool isFailure, int? progressPercentage);

        public DisplayBuildItem(string id, string projectName, string itemName, bool isFailure, int? progressPercentage)
        {
            Id = id;
            ProjectName = projectName;
            ItemName = itemName;
            IsFailure = isFailure;
            ProgressPercentage = progressPercentage; 
        }

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set { SetField(ref _itemName, value); }
        }

        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set { SetField(ref _projectName, value); }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetField(ref _id, value); }
        }

        private bool _isFailure;
        public bool IsFailure
        {
            get { return _isFailure; }
            set { SetField(ref _isFailure, value); }
        }

        private int? _progressPercentage;
        public int? ProgressPercentage
        {
            get { return _progressPercentage; }
            set { SetField(ref _progressPercentage, value); }
        }

        public event EventHandler<BuildStateChangedArgs> BuildStateChanged;
        public event EventHandler<ProgressPercentageChangedArgs> ProgressPercentageChanged;
    }

    public interface IBuildDisplayItem : IBuildItem { }

    public class BuildDisplay : ViewModelBase, IBuildDisplay
    {
        private ObservableCollection<IBuildDisplayItem> _buildItems = new ObservableCollection<IBuildDisplayItem>();

        public IEnumerable<IBuildDisplayItem> BuildItems
        {
            get { return _buildItems; }
        }

        private DisplayBuildItem.Factory _buildItemFactory;

        public BuildDisplay(DisplayBuildItem.Factory buildItemFactory)
        {
            _buildItemFactory = buildItemFactory;
        }

        public void SetBuildItems(IEnumerable<IBuildItem> items)
        {
            foreach (var item in items)
            {
                IBuildDisplayItem buildItem = _buildItemFactory(item.Id, item.ProjectName, item.ItemName, item.IsFailure, item.ProgressPercentage);
                Dispatcher.CurrentDispatcher.Invoke(() => _buildItems.Add(buildItem));
                item.BuildStateChanged += buildItem_BuildStateChanged;
                item.ProgressPercentageChanged += item_ProgressPercentageChanged;
            }
        }

        void item_ProgressPercentageChanged(object sender, ProgressPercentageChangedArgs e)
        {
            var progressPercentage = e.PercentageProgress;
            var item = (IBuildItem)sender;

            foreach (var buildItem in _buildItems)
            {
                if (buildItem.Id == item.Id)
                {
                    buildItem.ProgressPercentage = progressPercentage;
                    break;
                }
            }
        }

        void buildItem_BuildStateChanged(object sender, BuildStateChangedArgs e)
        {
            var isFailure = e.IsFailure;
            var item = (IBuildItem)sender;

            foreach( var buildItem in _buildItems )
            {
                if ( buildItem.Id == item.Id)
                {
                    buildItem.IsFailure = isFailure;
                    break;
                }
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
    }
}