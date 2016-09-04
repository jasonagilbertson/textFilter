using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TextFilter
{
    public partial class MainWindow : Window
    {
        #region Private Methods

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

        #endregion Private Methods

        #region Private Fields

        private MainViewModel _mainViewModel;

        #endregion Private Fields

        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            this._mainViewModel = (MainViewModel)this.FindResource("mainViewModel");

            // https: //msdn.microsoft.com/en-us/library/system.windows.frameworktemplate.findname(v=vs.110).aspx

            Closing += _mainViewModel.OnWindowClosing;
        }

        #endregion Public Constructors

        #region Public Methods

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

        #endregion Public Methods

        private void colorCombo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            _mainViewModel.ColorComboKeyDown(sender, e);
        }

        private void colorCombo_Selected(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ColorComboSelected();
        }

        private void DataGrid_CellGotFocus(object sender, RoutedEventArgs e)
        {
            // inconsistent. makes deletion difficult not worth the convenience

            //if (_endEditing)
            //{
            //    return;
            //}

            // Lookup for the source to be DataGridCell
            //if (e.OriginalSource.GetType() == typeof(DataGridCell))
            //{
            //    // Starts the Edit on the row;
            //    DataGrid grd = (DataGrid)sender;
            //    grd.BeginEdit(e);

            //    Control control = GetFirstChildByType<Control>(e.OriginalSource as DataGridCell);
            //    if (control != null)
            //    {
            //        if (control is CheckBox)
            //        {
            //            (control as CheckBox).IsChecked = !(control as CheckBox).IsChecked;
            //        }
            //        else
            //        {
            //            control.Focus();
            //        }
            //    }
            //}
        }

        private void DataGridCell_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    _endEditing = true;
            //}
            //else
            //{
            //    _endEditing = false;
            //}
        }

        private void DataGridCell_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    _endEditing = false;
            //}
            //else
            //{
            //    _endEditing = true;
            //}
        }

        private void FileData_Drop(object sender, DragEventArgs e)
        {
            this._mainViewModel.SetStatus("FileData_Drop");
            string[] fileNames = (string[])(((IDataObject)e.Data).GetData("FileDrop"));

            if (fileNames != null)
            {
                foreach (string filename in fileNames)
                {
                    if (Path.GetExtension(filename).ToLower() == ".xml"
                        | Path.GetExtension(filename).ToLower() == ".rvf"
                        | Path.GetExtension(filename).ToLower() == ".tat")
                    {
                        if (this._mainViewModel.FilterViewModel.VerifyAndOpenFile(filename))
                        {
                            continue;
                        }
                    }
                    // not a filter file
                    this._mainViewModel.LogViewModel.OpenFileExecuted(filename);
                }
            }
        }
    }
}