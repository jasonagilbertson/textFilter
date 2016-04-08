using System.Windows;
using System.Windows.Input;

namespace RegexViewer
{
    public partial class RenameDialog : Window
    {
        #region Public Constructors

        public RenameDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            textBoxNewName.Focus();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Disable()
        {
            this.Hide();
            this.Close();
        }

        public string WaitForResult()
        {
            this.ShowDialog();

            return textBoxNewName.Text;
        }

        #endregion Public Methods

        #region Private Methods

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            textBoxNewName.Text = string.Empty;
            Disable();
        }

        private void textBoxNewName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonSave_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                textBoxNewName.Text = string.Empty;
                Disable();
            }
        }

        #endregion Private Methods
    }
}