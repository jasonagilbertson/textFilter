using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace RegexViewer
{
    public class RegexViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private static TraceSource ts = new TraceSource("RegexViewer");
        private RegexViewerSettings _Settings;
        private Command closeCommand;
        private FilterManager filterManager;
        private ObservableCollection<ItemViewModel> logFileTabs;
        private LogManager logManager;
        private Command openCommand;

        #endregion Private Fields

        #region Public Constructors

        public RegexViewModel()
        {
            // LogCollection = _LogManager.GetLogs();
            // FilterCollection = _FilterManager.GetFilters();
            _Settings = new RegexViewerSettings();
            //   BackgroundColor = Color.Black;
            //_Settings.BackgroundColor = Color.Black;
            //_Settings.FontColor = Color.White;
            //_Settings.Save();
            this.logFileTabs = new ObservableCollection<ItemViewModel>();
            this.filterManager = new FilterManager();
            this.logManager = new LogManager();

            closeCommand = new Command(CloseLog);
            closeCommand.CanExecute = true;
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public Command CloseCommand
        {
            get
            {
                if (closeCommand == null)
                    closeCommand = new Command(CloseLog);
                return closeCommand;
            }
            set
            {
                closeCommand = value;
            }
        }

        public ObservableCollection<ItemViewModel> LogFileTabs
        {
            get
            {
                return this.logFileTabs;
            }
            set
            {
                logFileTabs = value;
                OnPropertyChanged("LogFileTabs");
            }
        }

        public Command OpenCommand
        {
            get
            {
                if (openCommand == null)
                    openCommand = new Command(OpenLog);
                return openCommand;
            }
            set
            {
                openCommand = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        //public bool CloseLog(TabItem tabItem)
        public void CloseLog(object sender)
        {
            ItemViewModel tabItem = new ItemViewModel();
            if (!logManager.CloseLog(tabItem.Name))
            {
                //return false;
            }

            // remove tab
            RemoveLogFileTab(tabItem);

            //return true;
        }

        //    set
        //    {
        //        _Settings.RecentFiles = string.Join(",", value);
        //    }
        //}
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void OpenLog(object sender)
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

            LogProperties logProperties = new LogProperties();
            if (String.IsNullOrEmpty((logProperties = logManager.OpenLog(logName)).Tag))
            {
                return;
            }

            // make new tab
            AddLogFileTab(logProperties);
        }

        public bool SaveFilter(string filterName)
        {
            return filterManager.Save(filterName);
        }

        #endregion Public Methods

        #region Private Methods

        private void AddLogFileTab(LogProperties logProperties)
        {
            if (!logFileTabs.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                ItemViewModel tabItem = new ItemViewModel();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                //ListBox listbox = new ListBox();
                //listbox.ItemsSource = logProperties.TextBlocks;
                //tabItem.Content = listbox;
                tabItem.Content = "testcontent";// logProperties.TextBlocks;
                //tabItem.Content = logProperties.TextBlocks;
                tabItem.ContentList = logProperties.TextBlocks;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.Tag;
                logFileTabs.Add(tabItem);
            }
        }

        private void RemoveLogFileTab(ItemViewModel tabItem)
        {
            if (logFileTabs.Any(x => String.Compare((string)x.Tag, tabItem.Name, true) == 0))
            {
                logFileTabs.Remove(tabItem);
            }
        }

        #endregion Private Methods

        //public Color BackgroundColor
        //{
        //    get
        //    {
        //        return Color.FromName(_Settings.BackgroundColor);
        //    }

        //    set
        //    {
        //        _Settings.BackgroundColor = value.ToString();
        //    }
        //}

        //public List<string> RecentFiles
        //{
        //    get
        //    {
        //        return _Settings.RecentFiles.Split(new string[] { "," } , StringSplitOptions.RemoveEmptyEntries).ToList();
        //    }
    }
}