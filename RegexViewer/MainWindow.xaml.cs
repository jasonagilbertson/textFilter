using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        private StringBuilder _color = new StringBuilder();

        private List<string> _colorNames = new List<string>();

        //private FilterViewModel filterViewModel;
        //private RegexViewModel regexViewModel;
        private MainViewModel _mainViewModel;

        #endregion Private Fields

        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            // Initialize the View Model Objects
            this._mainViewModel = (MainViewModel)this.FindResource("mainViewModel");
            //this.regexViewModel = (RegexViewModel)this.FindResource("regexViewModel");
            //this.filterViewModel = (FilterViewModel)this.FindResource("filterViewModel");
            _colorNames = GetColorNames();
            Closing += _mainViewModel.OnWindowClosing;
        }

        #endregion Public Constructors

        #region Public Methods

        public List<string> GetColorNames()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            List<string> list = new List<string>();
            foreach (var prop in typeof(Colors).GetProperties(flags))
            {
                if (prop.PropertyType.FullName == "System.Windows.Media.Color")
                {
                    Debug.Print(prop.PropertyType.FullName);
                    list.Add(prop.Name);
                }
            }
            return list;
        }

        #endregion Public Methods

        #region Private Methods

        private void colorCombo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is ComboBox)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                    case Key.Tab:
                    case Key.Back:
                        {
                            // _color.Remove(_color.Length - 1, 1);
                            _color.Clear();
                            return;
                        }
                    default:
                        {
                            break;
                        }
                }

                // dont add if not alpha character
                if (!Regex.IsMatch(e.Key.ToString(), "[a-zA-Z]{1}", RegexOptions.IgnoreCase))
                {
                    return;
                }

                _color.Append(e.Key.ToString());
                //((SolidColorBrush)new BrushConverter().ConvertFromString(lbi.BackgroundColor)),
                ComboBox comboBox = (sender as ComboBox);

                string color = _colorNames.FirstOrDefault(c => Regex.IsMatch(c, "^" + _color.ToString(), RegexOptions.IgnoreCase));
                if (String.IsNullOrEmpty(color))
                {
                    color = _colorNames.FirstOrDefault(c => Regex.IsMatch(c, _color.ToString(), RegexOptions.IgnoreCase));
                }
                if (!String.IsNullOrEmpty(color))
                {
                    comboBox.SelectedValue = color;
                }
                else
                {
                    comboBox.SelectedIndex = 0;
                }
            }
        }

        private void colorCombo_Selected(object sender, RoutedEventArgs e)
        {
            _color.Clear();
        }

        private void FilterCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void FilterCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.Print("here");
        }

        private void LogCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void LogCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.Print("here");
        }

        #endregion Private Methods

        private void logFileData_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void dataGridFilter_Drop(object sender, DragEventArgs e)
        {
            this._mainViewModel.SetStatus("dataGridFilter_Drop");
            string[] fileNames = (string[])(((IDataObject)e.Data).GetData("FileName"));
            if (fileNames != null)
            {
                foreach(string filename in fileNames)
                {
                    string ext = Path.GetExtension(filename).ToLower();
                    if (ext == ".xml")
                    {
                        this._mainViewModel.FilterViewModel.OpenFile(filename);
                    }
                }
            }
        }

        private void logFileData_Drop(object sender, DragEventArgs e)
        {
            this._mainViewModel.SetStatus("logFileData_Drop");
            string[] fileNames = (string[])(((IDataObject)e.Data).GetData("FileName"));
            if (fileNames != null)
            {
                foreach (string filename in fileNames)
                {
                    this._mainViewModel.LogViewModel.OpenFile(filename);
                }
            }
        }
    }
}