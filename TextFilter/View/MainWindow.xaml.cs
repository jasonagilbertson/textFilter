// ************************************************************************************
// Assembly: TextFilter
// File: MainWindow.xaml.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TextFilter
{
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            this._mainViewModel = (MainViewModel)this.FindResource("mainViewModel");
            Closing += _mainViewModel.OnWindowClosing;
            KeyUp += MainWindow_KeyUp;
        }

        private void colorCombo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            _mainViewModel.ColorComboKeyDown(sender, e);
        }

        private void colorCombo_Selected(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ColorComboSelected();
        }

        private void FileData_Drop(object sender, DragEventArgs e)
        {
            this._mainViewModel.SetStatus("FileData_Drop");
            string[] fileNames = (string[])(((IDataObject)e.Data).GetData("FileDrop"));

            if (fileNames != null)
            {
                foreach (string filename in fileNames)
                {
                    if (Path.GetExtension(filename).ToLower() == ".rvf"
                        | Path.GetExtension(filename).ToLower() == ".tat")
                    {
                        if (Base._FilterViewModel.VerifyAndOpenFile(filename))
                        {
                            continue;
                        }
                    }

                    // not a filter file
                    Base._LogViewModel.OpenFileExecuted(filename);
                }
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Base._FilterViewModel.OnPropertyChanged(BaseTabViewModelEvents.Enter);
                //Base._FilterViewModel.FilterLogExecuted();
            }
        }
    }
}