// ************************************************************************************
// Assembly: TextFilter
// File: TraceMessageDialog.xaml.cs
// Created: 2/12/2017
// Modified: 2/12/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Windows;
using System.Windows.Media;

namespace TextFilter
{
    public partial class TraceMessageDialog : Window
    {
        private string _initialNotes;

        public TraceMessageDialog(string notes)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            _initialNotes = notes;
            textBoxTraceMessage.Text = notes;
            textBoxTraceMessage.Focus();
            textBoxTraceMessage.FontFamily = new FontFamily(TextFilterSettings.Settings.FontName);
            textBoxTraceMessage.FontSize = TextFilterSettings.Settings.FontSize;
            textBoxTraceMessage.Foreground = TextFilterSettings.Settings.ForegroundColor;
            textBoxTraceMessage.Background = TextFilterSettings.Settings.BackgroundColor;
        }

        public bool DialogCanSave
        {
            get
            {
                return textBoxTraceMessage.IsEnabled;
            }

            set
            {
                textBoxTraceMessage.IsEnabled = value;
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
                return textBoxTraceMessage.Text;
            }
            else
            {
                return _initialNotes;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            textBoxTraceMessage.Text = _initialNotes;
            Disable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }
    }
}