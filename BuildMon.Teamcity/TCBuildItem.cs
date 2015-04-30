using System;

namespace BuildMon.Teamcity
{
    public class TcBuildItem : IBuildSourceItem
    {
        public delegate IBuildSourceItem Factory(string id, string projectName, string itemName);

        private bool _isFailure;
        private int? _progressPercentage;

        public TcBuildItem(string id, string projectName, string itemName)
        {
            Id = id;
            ProjectName = projectName;
            ItemName = itemName;
        }

        public string Id { get; private set; }
        public string ItemName { get; private set; }
        public event EventHandler<BuildStateChangedArgs> BuildStateChanged;
        public event EventHandler<ProgressPercentageChangedArgs> ProgressPercentageChanged;

        public bool IsFailure
        {
            get { return _isFailure; }
            set
            {
                if (_isFailure == value) return;
                _isFailure = value;
                BuildStateChanged.Raise(this, new BuildStateChangedArgs(value));
            }
        }

        public int? ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                if (_progressPercentage == value) return;
                _progressPercentage = value;
                ProgressPercentageChanged.Raise(this, new ProgressPercentageChangedArgs(value));
            }
        }

        public string ProjectName { get; private set; }
    }
}