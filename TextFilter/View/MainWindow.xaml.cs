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
            KeyUp += MainWindow_KeyUp;
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

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Base._FilterViewModel.FilterLogExecuted();
            }
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
    }
}