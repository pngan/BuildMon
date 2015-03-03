using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace BuildMon
{
    public interface IMainViewModel { }
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        public DelegateCommand FullScreenCommand { get; private set; }
        public DelegateCommand NormalScreenCommand { get; private set; }

        private bool _isFullScreen = false;
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set
            {
                _isFullScreen = value;
                OnPropertyChanged("IsFullScreen");
            }
        }


        private object _displayViewModel;

        public object DisplayViewModel
        {
            get { return _displayViewModel; }
            set { SetField(ref _displayViewModel, value); }
        }

        public MainViewModel( IEnumerable<IBuildDisplay> buildDisplays)
        {
            Debug.Assert(buildDisplays.Count() < 2, "Todo: Code currently does not handle multiple build displays");
            DisplayViewModel = buildDisplays.First();

            FullScreenCommand = new DelegateCommand(OnFullScreenCommand, () => true);
            NormalScreenCommand = new DelegateCommand(OnNormalScreenCommand, () => true);
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