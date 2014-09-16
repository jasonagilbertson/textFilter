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
        private ExeConfigurationFileMap _ConfigFileMap;
        private Configuration _Config;
        private KeyValueConfigurationCollection _appSettings;

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

        public void Save()
        {
            _Config.Save(ConfigurationSaveMode.Full);
        }

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