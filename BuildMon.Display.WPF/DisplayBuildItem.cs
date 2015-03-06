using System;

namespace BuildMon.Display.WPF
{
    public class DisplayBuildItem : ViewModelBase, IBuildDisplayItem
    {
        public delegate IBuildDisplayItem Factory(
            string id, string projectName, string itemName, bool isFailure, int? progressPercentage);

        private string _id;
        private bool _isFailure;
        private string _itemName;
        private int? _progressPercentage;
        private string _projectName;

        public DisplayBuildItem(string id, string projectName, string itemName, bool isFailure, int? progressPercentage)
        {
            Id = id;
            ProjectName = projectName;
            ItemName = itemName;
            IsFailure = isFailure;
            ProgressPercentage = progressPercentage;
        }

        public string ItemName
        {
            get { return _itemName; }
            set { SetField(ref _itemName, value); }
        }

        public string ProjectName
        {
            get { return _projectName; }
            set { SetField(ref _projectName, value); }
        }

        public string Id
        {
            get { return _id; }
            set { SetField(ref _id, value); }
        }

        public bool IsFailure
        {
            get { return _isFailure; }
            set { SetField(ref _isFailure, value); }
        }

        public int? ProgressPercentage
        {
            get { return _progressPercentage; }
            set { SetField(ref _progressPercentage, value); }
        }

        public event EventHandler<BuildStateChangedArgs> BuildStateChanged;
        public event EventHandler<ProgressPercentageChangedArgs> ProgressPercentageChanged;
    }
}