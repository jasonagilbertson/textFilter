using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace RegexViewer
{
    public class RegexViewerSettings
    {
        #region Private Fields

        private static RegexViewerSettings settings;

        private KeyValueConfigurationCollection _appSettings;

        private Configuration _Config;

        private ExeConfigurationFileMap _ConfigFileMap;

        #endregion Private Fields

        #region Public Constructors

        static RegexViewerSettings()
        {
            if (settings == null)
            {
                settings = new RegexViewerSettings();
            }
        }

        public RegexViewerSettings()
        {
            _ConfigFileMap = new ExeConfigurationFileMap();
            _ConfigFileMap.ExeConfigFilename = string.Format("{0}.config", Process.GetCurrentProcess().MainModule.FileName);

            // Get the mapped configuration file.
            _Config = ConfigurationManager.OpenMappedExeConfiguration(_ConfigFileMap, ConfigurationUserLevel.None);
            _appSettings = _Config.AppSettings.Settings;
            VerifyAppSettings();

            //List<string> test = new List<string>(RecentLogFiles);
            //try
            //{
            //    // check if exists in recentfiles if you dont want dupe
            //    test.Add("file1");
            //    test.Add("file2");
            //    test.Add("file3");
            //    test.Add("file4");
            //    test.Add("file5");
            //    test.Add("file6");
            //    test.Remove("file3");
            //}
            //catch { }
            //RecentLogFiles = test;
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Private Enums

        private enum AppSettingNames
        {
            AutoSaveFilters,
            BackgroundColor,
            FileHistoryCount,
            FilterDirectory,
            ForegroundColor,
            FontSize,
            CurrentFilterFiles,
            CurrentLogFiles,
            RecentFilterFiles,
            RecentLogFiles
        }

        #endregion Private Enums

        #region Public Properties

        public static RegexViewerSettings Settings
        {
            get { return RegexViewerSettings.settings; }
            set { RegexViewerSettings.settings = value; }
        }

        public bool AutoSaveFilters
        {
            get
            {
                return (Convert.ToBoolean(_appSettings["AutoSaveFilters"].Value));
            }
            set
            {
                _appSettings["AutoSaveFilters"].Value = value.ToString();
            }
        }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                return ((SolidColorBrush)new BrushConverter().ConvertFromString(_appSettings["BackgroundColor"].Value));
            }
            set
            {
                if (value.ToString() != _appSettings["BackgroundColor"].Value.ToString())
                {
                    _appSettings["BackgroundColor"].Value = value.ToString();
                    OnPropertyChanged("BackgroundColor");
                }
            }
        }

        public List<string> CurrentFilterFiles
        {
            get
            {
                return _appSettings["CurrentFilterFiles"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            set
            {
                _appSettings["CurrentFilterFiles"].Value = string.Join(",", value);
            }
        }

        public List<string> CurrentLogFiles
        {
            get
            {
                return _appSettings["CurrentLogFiles"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            set
            {
                _appSettings["CurrentLogFiles"].Value = string.Join(",", value);
            }
        }

        public int FileHistoryCount
        {
            get
            {
                return (Convert.ToInt32(_appSettings["FileHistoryCount"].Value));
            }
            set
            {
                _appSettings["FileHistoryCount"].Value = value.ToString();
            }
        }

        public int FontSize
        {
            get
            {
                return (Convert.ToInt32(_appSettings["FontSize"].Value));
            }
            set
            {
                _appSettings["FontSize"].Value = value.ToString();
            }
        }

        public SolidColorBrush ForegroundColor
        {
            get
            {
                return ((SolidColorBrush)new BrushConverter().ConvertFromString(_appSettings["ForegroundColor"].Value));
            }
            set
            {
                _appSettings["ForegroundColor"].Value = value.ToString();
            }
        }

        public string[] RecentFilterFiles
        {
            get
            {
                return _appSettings["RecentFilterFiles"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);//.ToList();
            }

            private set
            {
                _appSettings["RecentFilterFiles"].Value = string.Join(",", value);
            }
        }

        //public List<string> RecentLogFiles
        public string[] RecentLogFiles
        {
            get
            {
                return _appSettings["RecentLogFiles"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);//.ToList();
            }

            private set
            {
                _appSettings["RecentLogFiles"].Value = string.Join(",", value);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void AddFilterFile(string filterFile)
        {
            List<string> filterFiles = new List<string>(CurrentFilterFiles);
            if (!filterFiles.Contains(filterFile))
            {
                filterFiles.Add(filterFile);
                CurrentFilterFiles = filterFiles;
                RecentFilterFiles = ManageRecentFiles(filterFile, RecentFilterFiles);
            }
        }

        public void AddLogFile(string logFile)
        {
            List<string> logFiles = new List<string>(CurrentLogFiles);
            if (!logFiles.Contains(logFile))
            {
                logFiles.Add(logFile);
                CurrentLogFiles = logFiles;
                RecentLogFiles = ManageRecentFiles(logFile, RecentLogFiles);
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}

        public void RemoveLogFile(string logFile)
        {
            List<string> logFiles = new List<string>(CurrentLogFiles);
            if (logFiles.Contains(logFile))
            {
                Debug.Print("Removing LogFile:" + logFile);
                logFiles.Remove(logFile);
                CurrentLogFiles = logFiles;
            }
        }

        public void RemoveFilterFile(string filterFile)
        {
            List<string> logFiles = new List<string>(CurrentFilterFiles);
            if (logFiles.Contains(filterFile))
            {
                Debug.Print("Removing FilterFile:" + filterFile);
                logFiles.Remove(filterFile);
                CurrentFilterFiles = logFiles;
            }
        }
        public void Save()
        {
            _Config.Save(ConfigurationSaveMode.Full);
        }

        #endregion Public Methods

        #region Private Methods

        private string[] ManageRecentFiles(string logFile, string[] recentLogFiles)
        {
            //string[] recentLogFiles = RecentLogFiles;
            List<string> newList = new List<string>();

            // return if already in list
            if (recentLogFiles.ToList().Contains(logFile))
            {
                return newList.ToArray();
            }

            int i = Math.Min(recentLogFiles.Length, this.FileHistoryCount - 1) - 1;
            for (; i >= 0; i--)
            {
                Debug.Print("Removing RecentFile:" + recentLogFiles[i]);
                newList.Add(recentLogFiles[i]);
            }

            newList.Add(logFile);
            return newList.ToArray();
        }

        private void VerifyAppSettings()
        {
            foreach (string name in Enum.GetNames(typeof(AppSettingNames)))
            {
                if (!_appSettings.AllKeys.Contains(name))
                {
                    _appSettings.Add(name, string.Empty);
                }

                // set default settings
                if (!string.IsNullOrEmpty(_appSettings[name].Value))
                {
                    continue;
                }

                switch ((AppSettingNames)Enum.Parse(typeof(AppSettingNames), name))
                {
                    case AppSettingNames.BackgroundColor:
                        {
                            _appSettings[name].Value = "White";
                            break;
                        }
                    case AppSettingNames.FileHistoryCount:
                        {
                            _appSettings[name].Value = "20";
                            break;
                        }
                    case AppSettingNames.FontSize:
                        {
                            _appSettings[name].Value = "10";
                            break;
                        }
                    case AppSettingNames.ForegroundColor:
                        {
                            _appSettings[name].Value = "Black";
                            break;
                        }
                    case AppSettingNames.AutoSaveFilters:
                        {
                            _appSettings[name].Value = "False";
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        #endregion Private Methods

        //public string RecentFiles
        //{
        //    get
        //    {
        //        return (_appSettings["RecentFiles"].Value);
        //    }
        //    set
        //    {
        //        _appSettings["RecentFiles"].Value = value;
        //    }
        //}
    }
}