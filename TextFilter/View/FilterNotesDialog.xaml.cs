// ************************************************************************************
// Assembly: TextFilter
// File: FilterNotesDialog.xaml.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Windows;
using System.Windows.Media;

namespace TextFilter
{
    public partial class FilterNotesDialog : Window
    {
        private string _initialNotes;

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

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            textBoxFilterNotes.Text = _initialNotes;
            Disable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }
    }
}