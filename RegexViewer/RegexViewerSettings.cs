using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml;
using System.Runtime.InteropServices;

namespace RegexViewer
{
    public class RegexViewerSettings:Base
    {
        #region Private Fields
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

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
            

         
        }

        public bool ReadConfigFile()
        {
            

            this.ConfigFile = !string.IsNullOrEmpty(this.ConfigFile) ? this.ConfigFile : string.Format("{0}.config", Process.GetCurrentProcess().MainModule.FileName);
            _ConfigFileMap = new ExeConfigurationFileMap();
            if (!File.Exists(this.ConfigFile))
            {
                XmlTextWriter xmlw = new XmlTextWriter(this.ConfigFile, System.Text.Encoding.UTF8);
                xmlw.Formatting = Formatting.Indented;
                xmlw.WriteStartDocument();
                xmlw.WriteStartElement("configuration");
                    xmlw.WriteStartElement("startup");
                        xmlw.WriteStartElement("supportedRuntime");
                        xmlw.WriteAttributeString("version","v4.0");
                        xmlw.WriteAttributeString("sku", ".NETFramework,Version=v4.5");
                        xmlw.WriteEndElement();
                    xmlw.WriteEndElement();
                    xmlw.WriteStartElement("appSettings");
                    xmlw.WriteEndElement();
                xmlw.WriteEndElement();
                xmlw.WriteEndDocument();

                xmlw.Close();

            }

            _ConfigFileMap.ExeConfigFilename = this.ConfigFile;

            // Get the mapped configuration file.
            _Config = ConfigurationManager.OpenMappedExeConfiguration(_ConfigFileMap, ConfigurationUserLevel.None);
            _appSettings = _Config.AppSettings.Settings;
            VerifyAppSettings();

            if (!ProcessCommandLine())
            {
                return false;
            }

            return true;
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
            FontName,
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

        public string FontName
        {
            get
            {
                return _appSettings["FontName"].Value;
            }
            set
            {
                if (value.ToString() != _appSettings["FontName"].Value.ToString())
                {
                    _appSettings["FontName"].Value = value.ToString();
                    OnPropertyChanged("FontName");
                }
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
        private bool ProcessCommandLine()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            // this is the way fta passes arg
            if (arguments.Length == 2
                && !arguments[1].StartsWith("/")
                && File.Exists(arguments[1]))
            {
                Settings.RemoveAllLogs();
                Settings.AddLogFile(arguments[1]);
                return true;
            }
            else if (arguments.Length == 2 && arguments[1].Contains("/?"))
            {
                DisplayHelp();
                return false;
            }

            List<string> results = new List<string>();

            results = ProcessArg("/config:", arguments);
            if (results.Count == 1)
            {
                Settings.ConfigFile = results[0];
                // Settings.ReadConfigFile();
            }

            results = ProcessFiles(ProcessArg("/filter:", arguments));
            if (results.Count > 0)
            {
                Settings.RemoveAllFilters();
                foreach (string file in results)
                {
                    Settings.AddFilterFile(file);
                }
            }

            results = ProcessFiles(ProcessArg("/log:", arguments));
            if (results.Count > 0)
            {
                Settings.RemoveAllLogs();
                foreach (string file in results)
                {
                    Settings.AddLogFile(file);
                }
            }

            if (ProcessArg("/register", arguments).Count > 0)
            {
                FileTypeAssociation.Instance.ConfigureFTA(true);
                AttachConsole(ATTACH_PARENT_PROCESS);
                Console.WriteLine("registering file type association");
                return false;
            }

            if (ProcessArg("/unregister", arguments).Count > 0)
            {
                FileTypeAssociation.Instance.ConfigureFTA(false);
                AttachConsole(ATTACH_PARENT_PROCESS);
                Console.WriteLine("unregistering file type association");
                return false;
            }

            return true;
        }

        private List<string> ProcessFiles(List<string> results)
        {
         
            List<string> files = new List<string>();
            string wildcard = "*.*";

            foreach (string arg in results)
            {
                string cleanPath = arg.Trim('"');
                if (Regex.IsMatch(cleanPath, @"[^\\]*$") | cleanPath.Contains("*") | cleanPath.Contains("?"))
                {

                    string tempString = Regex.Match(cleanPath, @"[^\\]*$").Groups[0].Value;
                    if (tempString.Contains("*") | tempString.Contains("?"))
                    {
                        wildcard = tempString;
                        cleanPath = cleanPath.Replace(string.Format(@"\{0}", tempString), "");
                    }
                    if (string.IsNullOrEmpty(cleanPath) || string.IsNullOrEmpty(Path.GetDirectoryName(cleanPath)))
                    {
                        cleanPath = string.Format("{0}\\{1}", Environment.CurrentDirectory, cleanPath);
                    }
                    try
                    {
                        if (Directory.Exists(cleanPath))
                        {
                            files.AddRange(Directory.GetFiles(cleanPath, wildcard, SearchOption.AllDirectories));
                        }
                    }
                    catch (Exception e)
                    {
                        SetStatus("invalid dir: " + e.ToString());
                    }
                }
                else if (File.Exists(arg))
                {
                    files.Add(arg);
                }
            }

            return files;
        }

        private void RegisterFTA(bool p)
        {
            throw new NotImplementedException();
        }

        private List<string> ProcessArg(string setting, string[] arguments)
        {
            List<string> args = new List<string>();

            for (int i = 0; i < arguments.Length; i++)
            {

                if (arguments[i].ToLower().StartsWith(setting))
                {

                    string argument = arguments[i].ToLower().Replace(setting, "");
                    if (string.IsNullOrEmpty(argument) && arguments.Length > i + 1)
                    {
                        argument = arguments[i + 1].Trim();
                    }

                    foreach (string arg in argument.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        args.Add(arg.Trim());
                    }


                }
            }

            return args.ToList();
        }

        private void DisplayHelp()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
            Console.WriteLine(Properties.Resources.DisplayHelp);
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

        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}

        public void RemoveAllFilters()
        {
            foreach (string logfile in CurrentFilterFiles)
            {
                RemoveFilterFile(logfile);
            }
        }

        public void RemoveAllLogs()
        {
            foreach(string logfile in CurrentLogFiles)
            {
                RemoveLogFile(logfile);
            }
        }
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
                    case AppSettingNames.FontName:
                        {
                            _appSettings[name].Value = "Courier";
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


        public string ConfigFile { get; set; }
    }
}