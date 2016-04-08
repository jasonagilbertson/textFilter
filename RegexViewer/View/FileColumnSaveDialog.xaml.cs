using System.Windows;
using System.Windows.Media;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for TimedSaveDialog.xaml
    /// </summary>
    public partial class FileColumnSaveDialog : Window
    {
        public class Results
        {
            public bool Index;
            public bool Content;
            public bool Group1;
            public bool Group2;
            public bool Group3;
            public bool Group4;
            public string Separator;
            public bool Cancel;
        }
        #region Private Fields

        #endregion Private Fields

        #region Public Constructors

        public FileColumnSaveDialog()
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

        public Results WaitForResult()
        {
            this.ShowDialog();
            return new Results
            {
                Index = (bool)this.checkIndex.IsChecked,
                Content = (bool)this.checkContent.IsChecked,
                Group1 = (bool)this.checkGroup1.IsChecked,
                Group2 = (bool)this.checkGroup2.IsChecked,
                Group3 = (bool)this.checkGroup3.IsChecked,
                Group4 = (bool)this.checkGroup4.IsChecked,
                Separator = (string)this.textSeparator.Text,
                Cancel = _cancel
            };
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