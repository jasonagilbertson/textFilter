﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TextFilter
{
    public partial class OptionsDialog : Window
    {
        #region Public Constructors
        public enum OptionsDialogResult
        {
            unknown,
            cancel,
            edit,
            register,
            reset,
            save,
            unregister
        }
        private MainViewModel _mainViewModel;
        private void colorCombo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            _mainViewModel.ColorComboKeyDown(sender, e);
        }

        private void colorCombo_Selected(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ColorComboSelected();
        }



        private StringBuilder _color = new StringBuilder();

        private List<string> _colorNames = new List<string>();

        private OptionsDialogResult _dialogResult;
        public OptionsDialog()
        {
            
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            this._mainViewModel = (MainViewModel)this.FindResource("mainViewModel");
            
            //((ListViewItem)listViewFontName.SelectedItem).BringIntoView();
        }

        
        #endregion Public Constructors

        #region Public Methods

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

        #endregion Public Methods

        #region Private Methods

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

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.save;
            Disable();
        }

        #endregion Private Methods

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.reset;
            Disable();
        }

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.register;
            Disable();
        }

        private void buttonUnRegister_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = OptionsDialogResult.unregister;
            Disable();
        }
    }
}