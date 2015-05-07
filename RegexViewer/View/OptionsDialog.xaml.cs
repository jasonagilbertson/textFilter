using System.Windows;
using System.Windows.Media;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for TimedSaveDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
     
        #region Private Fields

        #endregion Private Fields

        #region Public Constructors

        public OptionsDialog()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Public Properties

        #endregion Public Properties

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
            _cancel = true;
            Disable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }

      
        #endregion Private Methods

        private bool _cancel;
       
    }
}