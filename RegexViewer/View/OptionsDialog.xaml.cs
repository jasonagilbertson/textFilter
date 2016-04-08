using System.Windows;

namespace RegexViewer
{
    public partial class OptionsDialog : Window
    {
        #region Public Constructors

        public OptionsDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Disable()
        {
            this.Hide();
            this.Close();
        }

        public bool WaitForResult()
        {
            this.ShowDialog();
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            //_cancel = true;
            Disable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }

        #endregion Private Methods
    }
}