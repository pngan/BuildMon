using System.Windows;

namespace BuildMon
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        private readonly IMainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(IMainViewModel viewModel) : this()
        {
            _viewModel = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement) sender).DataContext = _viewModel;
        }
    }
}