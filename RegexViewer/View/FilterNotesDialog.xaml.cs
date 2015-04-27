using System;
using System.Windows;
using System.Windows.Input;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for TimedSaveDialog.xaml
    /// </summary>
    public partial class FilterNotesDialog : Window
    {
        private string _initialNotes;

        #region Private Fields

        #endregion Private Fields

               
        #region Public Methods

        public FilterNotesDialog(string notes)
        {
            InitializeComponent();
            _initialNotes = notes;
            textBoxFilterNotes.Text = notes;
            textBoxFilterNotes.Focus();
        }


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
        public void Disable()
        {
            this.Hide();
            this.Close();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            textBoxFilterNotes.Text = _initialNotes;
            Disable();
        }
        #endregion Private Methods
    }
}