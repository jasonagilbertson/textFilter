// ************************************************************************************
// Assembly: TextFilter
// File: RenameDialog.xaml.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Windows;
using System.Windows.Input;

namespace TextFilter
{
    public partial class RenameDialog : Window
    {
        public RenameDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            textBoxNewName.Focus();
        }

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

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            textBoxNewName.Text = string.Empty;
            Disable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
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
    }
}