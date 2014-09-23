using System.Windows;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        //private FilterViewModel filterViewModel;
        //private RegexViewModel regexViewModel;
        private MainViewModel mainViewModel;

        #endregion Private Fields

        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            // Initialize the View Model Objects
            this.mainViewModel = (MainViewModel)this.FindResource("mainViewModel");
            //this.regexViewModel = (RegexViewModel)this.FindResource("regexViewModel");
            //this.filterViewModel = (FilterViewModel)this.FindResource("filterViewModel");

            Closing += mainViewModel.OnWindowClosing;
        }

        #endregion Public Constructors
    }
}