using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextFilter
{
    public partial class DebugLogDialog : Window
    {
        #region Private Fields

        #endregion Private Fields

        #region Public Constructors

        public DebugLogDialog()
        {
            Owner = Application.Current.MainWindow;
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

        public void WaitForResult()
        {
            this.ShowDialog();
        }

        #endregion Public Methods

        #region Private Methods

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach(ListBoxItem item in listBoxDebugLog.Items)
            {
                sb.AppendLine(item.Content.ToString());
            }

            Clipboard.SetText(sb.ToString());
            Disable();
        }

        #endregion Private Methods
    }
}