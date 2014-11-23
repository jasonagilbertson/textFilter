using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        //private FilterViewModel filterViewModel;
        //private RegexViewModel regexViewModel;
        private MainViewModel _mainViewModel;
        private StringBuilder _color = new StringBuilder();
        private List<string> _colorNames = new List<string>();
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
        private void backgroundColorCombo_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if(sender is ComboBox)
            {
                _mainViewModel.SetStatus(string.Format("colorCombo_LostFocus:{0}", ((FilterFileItem)(sender as ComboBox).Tag).BackgroundColor));
        //        _mainViewModel.SetStatus((sender as ComboBox).SelectedValue.ToString());
                ((FilterFileItem)(sender as ComboBox).Tag).BackgroundColor = (sender as ComboBox).SelectedValue.ToString();
            }
        }
        private void foregroundColorCombo_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if(sender is ComboBox)
            {
            _mainViewModel.SetStatus(string.Format("colorCombo_LostFocus:{0}", ((FilterFileItem)(sender as ComboBox).Tag).ForegroundColor));
         //   _mainViewModel.SetStatus((sender as ComboBox).SelectedValue.ToString());
            ((FilterFileItem)(sender as ComboBox).Tag).ForegroundColor = (sender as ComboBox).SelectedValue.ToString();
            }
        }
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
                if(!Regex.IsMatch(e.Key.ToString(),"[a-zA-Z]{1}",RegexOptions.IgnoreCase))
                {
                    return;
                }

                _color.Append(e.Key.ToString());
                //((SolidColorBrush)new BrushConverter().ConvertFromString(lbi.BackgroundColor)),
                ComboBox comboBox = (sender as ComboBox);
                
                string color = _colorNames.FirstOrDefault(c => Regex.IsMatch(c, "^" + _color.ToString(),RegexOptions.IgnoreCase));
                if (String.IsNullOrEmpty(color))
                {
                    color = _colorNames.FirstOrDefault(c => Regex.IsMatch(c, _color.ToString(), RegexOptions.IgnoreCase));
                }
                if(!String.IsNullOrEmpty(color))
                {
                    comboBox.SelectedValue = color;
                }
                else
                {
                    comboBox.SelectedIndex = 0;
                }
                
            }
        }

        public List<string> GetColorNames()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            List<string> list = new List<string>();
            foreach (var prop in typeof(Colors).GetProperties(flags))
            {
                if(prop.PropertyType.FullName == "System.Windows.Media.Color")
                {
                    Debug.Print(prop.PropertyType.FullName);
                    list.Add(prop.Name);
                }
            }
            return list;
        }

        private void colorCombo_Selected(object sender, RoutedEventArgs e)
        {
            _color.Clear();
            //(sender as ComboBox).UpdateLayout();
          //  (sender as mboBox).droppe
            //(sender as ComboBox).SelectedIndex = 1;
            
        }


    }
}