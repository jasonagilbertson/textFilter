using System.Windows;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        private RegexViewModel ViewModel;

        #endregion Private Fields

        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            // Initialize the Regex View Model Object
            ViewModel = (RegexViewModel)this.FindResource("regexViewModel");
        }

        #endregion Public Constructors
    }
}