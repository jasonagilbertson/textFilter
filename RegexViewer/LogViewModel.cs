using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace RegexViewer
{
    //public class RegexViewModel : MainViewModel, INotifyPropertyChanged, RegexViewer.IViewModel
    public class LogViewModel : INotifyPropertyChanged, RegexViewer.IViewModel
    {
        #region Private Fields

        // private RegexViewerSettings _Settings;
        private Command closeCommand;

        // private FilterManager filterManager;
        private LogManager logManager;

        private Command openCommand;
        private int selectedIndex;
        private ObservableCollection<ItemViewModel> tabItems;
        private TraceSource ts = new TraceSource("RegexViewer:RegexViewModel");
        private RegexViewerSettings settings = RegexViewerSettings.Settings;
        #endregion Private Fields

        #region Public Constructors

        public LogViewModel()
        {
            this.tabItems = new ObservableCollection<ItemViewModel>();
            //    this.filterManager = new FilterManager();
            this.logManager = new LogManager();

            // load tabs from last session
            foreach(LogProperties logProperty in logManager.OpenLogFiles(settings.CurrentLogFiles.ToArray()))
            {
                AddTabItem(logProperty);
            }
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public Command CloseCommand
        {
            get { return closeCommand ?? new Command(CloseFile); }
            set { openCommand = value; }
        }

        public Command OpenCommand
        {
            get { return openCommand ?? new Command(OpenFile); }
            set { openCommand = value; }
        }

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        public ObservableCollection<ItemViewModel> TabItems
        {
            get
            {
                return this.tabItems;
            }
            set
            {
                tabItems = value;
                OnPropertyChanged("TabItems");
            }
        }

        #endregion Public Properties

        #region Public Methods

        //public bool CloseLog(TabItem tabItem)
        public void CloseFile(object sender)
        {
            ItemViewModel tabItem = tabItems[selectedIndex];
            if (!logManager.CloseLog(tabItem.Tag))
            {
                return;
            }

            RemoveTabItem(tabItem);
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void OpenFile(object sender)
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
            AddTabItem(logProperties);
        }

        #endregion Public Methods

        #region Private Methods

        private void AddTabItem(LogProperties logProperties)
        {
            if (!tabItems.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                ItemViewModel tabItem = new ItemViewModel();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                tabItem.Name = this.tabItems.Count.ToString();
                tabItem.ContentList = logProperties.TextBlocks;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.FileName;
                // todo: fix this background binding for dynamic
               // tabItem.Background = settings.BackgroundColor.ToString();
                tabItems.Add(tabItem);
            }
        }

        private void RemoveTabItem(ItemViewModel tabItem)
        {
            if (tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Remove(tabItem);
            }
        }

        #endregion Private Methods
    }
}