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
        #region Private Methods

    
        #endregion Private Methods
        //public DataGrid dataGridFilter = new DataGrid();
        //public ListBox logFileData = new ListBox();
        #region Private Fields
        
        private StringBuilder _color = new StringBuilder();

        private List<string> _colorNames = new List<string>();

        //private FilterViewModel filterViewModel;
        //private RegexViewModel regexViewModel;
        private MainViewModel _mainViewModel;
        private bool _endEditing;
        //private Command _gotoLineCommand;

        #endregion Private Fields

        #region Public Constructors

        public MainWindow()
        {
          //  this.GotoLineCommand = new Command(GotoLine, true);
            InitializeComponent();
            
            // Initialize the View Model Objects
            this._mainViewModel = (MainViewModel)this.FindResource("mainViewModel");
            
             
            
            //https://msdn.microsoft.com/en-us/library/system.windows.frameworktemplate.findname(v=vs.110).aspx
            
           
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
        private void DataGridCell_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _endEditing = true;
            }
            else
            {
                _endEditing = false;
            }

        }
        private void DataGridCell_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _endEditing = false;
            }
            else
            {
                _endEditing = true;
            }

        }
        private void DataGrid_CellGotFocus(object sender, RoutedEventArgs e)
        {
            if(_endEditing)
            {
               // _endEditing = false;
                return;
            }
            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                // Starts the Edit on the row;
                DataGrid grd = (DataGrid)sender;
                grd.BeginEdit(e);

                Control control = GetFirstChildByType<Control>(e.OriginalSource as DataGridCell);
                if (control != null)
                {
                    if (control is CheckBox)
                    {
                        (control as CheckBox).IsChecked = !(control as CheckBox).IsChecked;
                    }
                    else
                    {
                        control.Focus();
                    }
                }
            }
        }

        private T GetFirstChildByType<T>(DependencyObject prop) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(prop); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild((prop), i) as DependencyObject;
                if (child == null)
                    continue;

                T castedProp = child as T;
                if (castedProp != null)
                    return castedProp;

                castedProp = GetFirstChildByType<T>(child);

                if (castedProp != null)
                    return castedProp;
            }
            return null;
        }
        #endregion Public Methods
        
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

        
        private void FileData_Drop(object sender, DragEventArgs e)
        {
            this._mainViewModel.SetStatus("FileData_Drop");
            string[] fileNames = (string[])(((IDataObject)e.Data).GetData("FileDrop"));

            if (fileNames != null)
            {
                foreach (string filename in fileNames)
                {
                    if (Path.GetExtension(filename).ToLower() == ".xml")
                    {
                        if (this._mainViewModel.FilterViewModel.VerifyAndOpenFile(filename))
                        {
                            continue;
                        }
                    }
                    // not a filter file
                    this._mainViewModel.LogViewModel.OpenFile(filename);
                }
            }
        }
        public T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        //public Command GotoLineCommand
        //{
        //    get
        //    {
        //        if (_gotoLineCommand == null)
        //        {
        //            _gotoLineCommand = new Command(GotoLine);
        //        }
        //        _gotoLineCommand.CanExecute = true;

        //        return _gotoLineCommand;
        //    }
        //    set { _gotoLineCommand = value; }
        //}

        //public void GotoLine(object line)
        //{
        //    Debug.Print("here");
        //    listboxStatus.Items.Add(new ListBoxItem() { Content = "test" });
        //    //_mainViewModel.LogViewModel.
        //}
    }
}