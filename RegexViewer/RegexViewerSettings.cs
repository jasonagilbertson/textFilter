using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace RegexViewer
{
    internal class RegexViewerSettings
    {
        #region Private Fields

        private KeyValueConfigurationCollection _appSettings;
        private Configuration _Config;
        private ExeConfigurationFileMap _ConfigFileMap;

        #endregion Private Fields

        #region Public Constructors

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

        #region Private Enums

        private enum AppSettingNames
        {
            BackgroundColor,
            FileHistoryCount,
            FilterDirectory,
            FontColor,
            FontSize,
            CurrentFilterFiles,
            CurrentLogFiles,
            RecentFilterFiles,
            RecentLogFiles
        }

        #endregion Private Enums

        #region Public Properties

        public Color BackgroundColor
        {
            get
            {
                return (Color.FromName(_appSettings["BackgroundColor"].Value));
            }
            set
            {
                _appSettings["BackgroundColor"].Value = value.Name;
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

        public Color FontColor
        {
            get
            {
                return (Color.FromName(_appSettings["FontColor"].Value));
            }
            set
            {
                _appSettings["FontColor"].Value = value.Name;
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

        public List<string> RecentFilterFiles
        {
            get
            {
                return _appSettings["RecentFilterFiles"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            set
            {
                _appSettings["RecentFilterFiles"].Value = string.Join(",", value);
            }
        }

        public List<string> RecentLogFiles
        {
            get
            {
                return _appSettings["RecentLogFiles"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            set
            {
                _appSettings["RecentLogFiles"].Value = string.Join(",", value);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Save()
        {
            _Config.Save(ConfigurationSaveMode.Full);
        }

        #endregion Public Methods

        #region Private Methods

        private void VerifyAppSettings()
        {
            foreach (string name in Enum.GetNames(typeof(AppSettingNames)))
            {
                if (!_appSettings.AllKeys.Contains(name))
                {
                    _appSettings.Add(name, string.Empty);
                }

                // set default  settings
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
                    case AppSettingNames.FontColor:
                        {
                            _appSettings[name].Value = "Black";
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