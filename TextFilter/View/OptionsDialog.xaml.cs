// ************************************************************************************
// Assembly: TextFilter
// File: OptionsDialog.xaml.cs
// Created: 9/6/2016
// Modified: 2/12/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace TextFilter
{
    public partial class OptionsDialog : Window
    {
        private StringBuilder _color = new StringBuilder();

        private List<string> _colorNames = new List<string>();

        private OptionsDialogResult _dialogResult;

        public OptionsDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        public enum OptionsDialogResult
        {
            unknown,
            apply,
            cancel,
            edit,
            register,
            reset,
            save,
            unregister
        }

        public void Disable()
        {
            this.Hide();
            this.Close();
        }

        public OptionsDialogResult WaitForResult()
        {
            this.ShowDialog();
            return _dialogResult;
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.apply;
            Disable();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.cancel;
            Disable();
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.edit;
            Disable();
        }

        private void buttonRecentFilterClear_Click(object sender, RoutedEventArgs e)
        {
            Base._FilterViewModel.ClearRecentExecuted();
        }

        private void buttonRecentLogClear_Click(object sender, RoutedEventArgs e)
        {
            Base._LogViewModel.ClearRecentExecuted();
        }

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.register;
            Disable();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.reset;
            Disable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.save;
            Disable();
        }

        private void buttonUnRegister_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.unregister;
            Disable();
        }

        private void colorCombo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //   _mainViewModel.ColorComboKeyDown(sender, e);
        }

        private void colorCombo_Selected(object sender, RoutedEventArgs e)
        {
            //   _mainViewModel.ColorComboSelected();
        }
    }
}