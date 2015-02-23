using System;
using System.Windows;
using System.Windows.Input;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for GotoLineDialog.xaml
    /// </summary>
    public partial class GotoLineDialog : Window
    {
        #region Public Constructors

        public GotoLineDialog()
        {
            InitializeComponent();
            textBoxLineNumber.Focus();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Disable()
        {
            this.Hide();
            this.Close();
        }

        public int WaitForResult()
        {
            this.ShowDialog();
            int result = 0;
            if (Int32.TryParse(textBoxLineNumber.Text, out result))
            {
                return result;
            }
            return 0;
        }

        #endregion Public Methods

        #region Private Methods

        private void buttonGotoLine_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }

        private void textBoxLineNumber_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonGotoLine_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                Disable();
            }
        }

        #endregion Private Methods
    }
}