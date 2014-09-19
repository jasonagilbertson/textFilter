using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace RegexViewer
{
    public class RegexViewModel : INotifyPropertyChanged, RegexViewer.IViewModel
    {
        #region Private Fields

        private static TraceSource ts = new TraceSource("RegexViewer");
        private RegexViewerSettings _Settings;
        private Command closeCommand;
        private FilterManager filterManager;
        private ObservableCollection<ItemViewModel> tabItems;
        private LogManager logManager;
        private Command openCommand;
        private int selectedIndex;
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
            this.tabItems = new ObservableCollection<ItemViewModel>();
            this.filterManager = new FilterManager();
            this.logManager = new LogManager();

            closeCommand = new Command(CloseFile);
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
                    closeCommand = new Command(CloseFile);
                return closeCommand;
            }
            set
            {
                closeCommand = value;
            }
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
                    OnPropertyChanged("Name");
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

        public Command OpenCommand
        {
            get
            {
                if (openCommand == null)
                    openCommand = new Command(OpenFile);
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
                tabItem.Name = tabItems.Count.ToString();
                tabItem.ContentList = logProperties.TextBlocks;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.Tag;
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