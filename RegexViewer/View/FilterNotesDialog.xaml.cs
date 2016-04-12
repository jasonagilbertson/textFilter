using System.Windows;
using System.Windows.Media;

namespace TextFilter
{
    public partial class FilterNotesDialog : Window
    {
        #region Private Fields

        private string _initialNotes;

        #endregion Private Fields

        #region Public Constructors

        public FilterNotesDialog(string notes)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            _initialNotes = notes;
            textBoxFilterNotes.Text = notes;
            textBoxFilterNotes.Focus();
            textBoxFilterNotes.FontFamily = new FontFamily(TextFilterSettings.Settings.FontName);
            textBoxFilterNotes.FontSize = TextFilterSettings.Settings.FontSize;
            textBoxFilterNotes.Foreground = TextFilterSettings.Settings.ForegroundColor;
            textBoxFilterNotes.Background = TextFilterSettings.Settings.BackgroundColor;
        }

        #endregion Public Constructors

        #region Public Properties

        public bool DialogCanSave
        {
            get
            {
                return textBoxFilterNotes.IsEnabled;
            }

            set
            {
                textBoxFilterNotes.IsEnabled = value;
                buttonSave.IsEnabled = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Disable()
        {
            this.Hide();
            this.Close();
        }

        public string WaitForResult()
        {
            this.ShowDialog();
            if (DialogCanSave)
            {
                return textBoxFilterNotes.Text;
            }
            else
            {
                return _initialNotes;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            textBoxFilterNotes.Text = _initialNotes;
            Disable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }

        #endregion Private Methods
    }
}