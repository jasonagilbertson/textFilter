using System.Windows;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        private RegexViewModel regexViewModel;
        private FilterViewModel filterViewModel;

        #endregion Private Fields

        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            // Initialize the View Model Objects
            this.regexViewModel = (RegexViewModel)this.FindResource("regexViewModel");
            this.filterViewModel = (FilterViewModel)this.FindResource("filterViewModel");
        }

        #endregion Public Constructors
    }
}