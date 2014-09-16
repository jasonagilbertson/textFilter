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
        private static TraceSource ts = new TraceSource("RegexViewer");
        private FilterManager _FilterManager = new FilterManager();
        private LogManager _LogManager = new LogManager();

        public event PropertyChangedEventHandler PropertyChanged;

        private RegexViewerSettings _Settings;

        //   OnPropertyChanged("FirstName");
        private Command closeCommand;

        public Command CloseCommand
        {
            get
            {
                if (closeCommand == null)
                    closeCommand = new Command(() => { CloseLog(); });
                return closeCommand;
            }
            set
            {
                closeCommand = value;
            }
        }

        public RegexViewModel()
        {
            // LogCollection = _LogManager.GetLogs();
            // FilterCollection = _FilterManager.GetFilters();
            _Settings = new RegexViewerSettings();
            //   BackgroundColor = Color.Black;
            //_Settings.BackgroundColor = Color.Black;
            //_Settings.FontColor = Color.White;
            //_Settings.Save();
            this.LogFileTabs = new ObservableCollection<TabItem>();
            closeCommand = new Command(CloseLog);
            closeCommand.Enabled = true;
        }

        public ObservableCollection<TabItem> LogFileTabs
        {
            get;
            set;
        }

        public bool SaveFilter(string filterName)
        {
            return _FilterManager.Save(filterName);
        }

        public bool OpenLog(string LogName)
        {
            LogProperties logProperties = new LogProperties();
            if (String.IsNullOrEmpty((logProperties = _LogManager.OpenLog(LogName)).Name))
            {
                return false;
            }

            // make new tab
            AddLogFileTab(logProperties);

            return true;
        }

        //public bool CloseLog(TabItem tabItem)
        public void CloseLog()
        {
            TabItem tabItem = new TabItem();
            if (!_LogManager.CloseLog(tabItem.Name))
            {
                //return false;
            }

            // remove tab
            RemoveLogFileTab(tabItem);

            //return true;
        }

        private void AddLogFileTab(LogProperties logProperties)
        {
            if (!LogFileTabs.Any(x => String.Compare(x.Name, logProperties.FileName, true) == 0))
            {
                TabItem tabItem = new TabItem();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                tabItem.Content = logProperties.TextBlocks;
                tabItem.Name = logProperties.FileName;
                tabItem.Header = logProperties.Name;
                LogFileTabs.Add(tabItem);
            }
        }

        private void RemoveLogFileTab(TabItem tabItem)
        {
            if (LogFileTabs.Any(x => String.Compare(x.Name, tabItem.Name, true) == 0))
            {
                LogFileTabs.Remove(tabItem);
            }
        }

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
    }
}