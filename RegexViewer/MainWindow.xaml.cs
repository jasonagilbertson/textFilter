using System.Windows;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RegexViewModel ViewModel;

        public MainWindow()
        {
            InitializeComponent();
            // Initialize the Regex View Model Object
            ViewModel = (RegexViewModel)this.FindResource("regexViewModel");
        }
    }
}