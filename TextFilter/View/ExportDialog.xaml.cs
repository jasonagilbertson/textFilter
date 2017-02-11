// ************************************************************************************
// Assembly: TextFilter
// File: ExportDialog.xaml.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Windows;

namespace TextFilter
{
    public partial class ExportDialog : Window
    {
        private bool _cancel;

        private bool _copy;

        public ExportDialog(LogFile.ExportConfigurationInfo config)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            this.checkIndex.IsChecked = config.Index;
            this.checkContent.IsChecked = config.Content;
            this.checkGroup1.IsChecked = config.Group1;
            this.checkGroup2.IsChecked = config.Group2;
            this.checkGroup3.IsChecked = config.Group3;
            this.checkGroup4.IsChecked = config.Group4;
            this.checkRemoveEmptyRows.IsChecked = config.RemoveEmpty;
            this.textSeparator.Text = config.Separator;
        }

        public void Disable()
        {
            this.Hide();
            this.Close();
        }

        public LogFile.ExportConfigurationInfo WaitForResult()
        {
            this.ShowDialog();
            return new LogFile.ExportConfigurationInfo
            {
                Index = (bool)this.checkIndex.IsChecked,
                Content = (bool)this.checkContent.IsChecked,
                Group1 = (bool)this.checkGroup1.IsChecked,
                Group2 = (bool)this.checkGroup2.IsChecked,
                Group3 = (bool)this.checkGroup3.IsChecked,
                Group4 = (bool)this.checkGroup4.IsChecked,
                Separator = (string)this.textSeparator.Text,
                Cancel = _cancel,
                Copy = _copy,
                RemoveEmpty = (bool)this.checkRemoveEmptyRows.IsChecked
            };
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            _cancel = true;
            Disable();
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            _copy = true;
            Disable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Disable();
        }
    }
}