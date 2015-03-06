using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BuildMon
{
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private object _displayViewModel;
        private bool _isFullScreen;

        public MainViewModel(IEnumerable<IBuildDisplay> buildDisplays)
        {
            Debug.Assert(buildDisplays.Count() < 2, "Todo: Code currently does not handle multiple build displays");
            DisplayViewModel = buildDisplays.First();

            FullScreenCommand = new DelegateCommand(OnFullScreenCommand, () => true);
            NormalScreenCommand = new DelegateCommand(OnNormalScreenCommand, () => true);
        }

        public DelegateCommand FullScreenCommand { get; private set; }
        public DelegateCommand NormalScreenCommand { get; private set; }

        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set
            {
                _isFullScreen = value;
                OnPropertyChanged("IsFullScreen");
            }
        }

        public object DisplayViewModel
        {
            get { return _displayViewModel; }
            set { SetField(ref _displayViewModel, value); }
        }

        private void OnNormalScreenCommand()
        {
            IsFullScreen = false;
        }

        private void OnFullScreenCommand()
        {
            IsFullScreen = true;
        }
    }
}