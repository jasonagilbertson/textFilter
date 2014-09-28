using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace RegexViewer
{
    //public class RegexViewModel : MainViewModel, INotifyPropertyChanged, RegexViewer.IViewModel
    public class LogViewModel : BaseViewModel<ListBoxItem>
    {
        #region Private Fields

        // private FilterManager filterManager;
        //private LogManager logManager;

        #endregion Private Fields

        // private ObservableCollection<ITabViewModel> tabItems;

        #region Public Constructors

        public LogViewModel()
        {
            this.TabItems = new ObservableCollection<ITabViewModel>();
            //    this.filterManager = new FilterManager();
            this.FileManager = new LogFileManager();

            // load tabs from last session
            foreach (LogFileProperties logProperty in this.FileManager.OpenFiles(this.Settings.CurrentLogFiles.ToArray()))
            {
                AddTabItem(logProperty);
            }
        }

        #endregion Public Constructors

        //public bool CloseLog(TabItem tabItem)

        #region Public Methods

        public override void AddTabItem(IFileProperties<ListBoxItem> logProperties)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                LogTabViewModel tabItem = new LogTabViewModel();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                tabItem.Name = this.TabItems.Count.ToString();
                tabItem.ContentList = ((LogFileProperties)logProperties).ContentItems;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.FileName;
                TabItems.Add(tabItem);
            }
        }

        public override void OpenFile(object sender)
        {
            string logName = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            //dlg.Filter = "Text Files (*.txt)|*.txt|Csv Files (*.csv)|*.csv|All Files (*.*)|*.*"; // Filter files by extension
            dlg.Filter = "All Files (*.*)|*.*|Csv Files (*.csv)|*.csv|Text Files (*.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                logName = dlg.FileName;
            }

            LogFileProperties logProperties = new LogFileProperties();
            if (String.IsNullOrEmpty((logProperties = (LogFileProperties)this.FileManager.OpenFile(logName)).Tag))
            {
                return;
            }

            // make new tab
            AddTabItem(logProperties);
        }

        #endregion Public Methods
    }
}