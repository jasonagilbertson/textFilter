using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Xml;

namespace RegexViewer
{
    public class RegexViewerSettings : Base
    {
        #region Public Methods

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
                xmlw.WriteAttributeString("version", "v4.0");
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


            // verify files
            this.CurrentFilterFiles = ProcessFiles(CurrentFilterFiles);
            this.CurrentLogFiles = ProcessFiles(CurrentLogFiles);

            if (!ProcessCommandLine())
            {
                return false;
            }

            return true;
        }

        public void RemoveAllFilters()
        {
            foreach (string logfile in CurrentFilterFiles)
            {
                RemoveFilterFile(logfile);
            }
        }

        public void RemoveAllLogs()
        {
            foreach (string logfile in CurrentLogFiles)
            {
                RemoveLogFile(logfile);
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

        public void Save()
        {
            // TODO: FIX EXCEPTION when two instances open and one saves to config file first
            //if(File.Exists(this.ConfigFile))
            //{
            //    File.Delete(this.ConfigFile);
            //}
            try
            {

                _Config.Save(ConfigurationSaveMode.Full);
            }
            catch (Exception e) { }
        }

        #endregion Public Methods

        #region Private Methods

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        private void DisplayHelp()
        {
            Console.WriteLine(Properties.Resources.DisplayHelp);
        }

        private string[] ManageRecentFiles(string logFile, string[] recentLogFiles)
        {
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

        private bool ProcessCommandLine()
        {
            bool retval = true;
            string[] arguments = Environment.GetCommandLineArgs();

            if (arguments.Length > 1)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
            }
            // this is the way fta passes arg
            if (arguments.Length == 2
                && !arguments[1].StartsWith("/")
                && File.Exists(arguments[1]))
            {
                Settings.RemoveAllLogs();
                Settings.AddLogFile(Environment.ExpandEnvironmentVariables(arguments[1]));
            }
            else if (arguments.Length == 2 && arguments[1].Contains("/?"))
            {
                DisplayHelp();
                retval = false;
            }

            List<string> results = new List<string>();

            results = ProcessArg("/config:", arguments);
            if (results.Count == 1)
            {
                Settings.ConfigFile = Environment.ExpandEnvironmentVariables(results[0]);
                // Settings.ReadConfigFile();
            }

            results = ProcessFiles(ProcessArg("/filter:", arguments));
            if (results.Count > 0)
            {
                Settings.RemoveAllFilters();
                if (results.Count > Settings.MaxMultiFileCount)
                {
                    SetStatus("max filter count reached:" + settings.MaxMultiFileCount);
                }

                for (int i = 0; i < settings.MaxMultiFileCount & i < results.Count; i++)
                {
                    Settings.AddFilterFile(results[i]);
                }
            }

            results = ProcessFiles(ProcessArg("/log:", arguments));
            if (results.Count > 0)
            {
                Settings.RemoveAllLogs();
                if (results.Count > Settings.MaxMultiFileCount)
                {
                    SetStatus("max log count reached:" + settings.MaxMultiFileCount);
                }

                for (int i = 0; i < settings.MaxMultiFileCount & i < results.Count; i++)
                {
                    Settings.AddLogFile(results[i]);
                }
            }

            if (ProcessArg("/register", arguments).Count > 0)
            {
                FileTypeAssociation.Instance.ConfigureFTA(true);

                Console.WriteLine("registering file type association");
                retval = false;
            }

            if (ProcessArg("/unregister", arguments).Count > 0)
            {
                FileTypeAssociation.Instance.ConfigureFTA(false);

                Console.WriteLine("unregistering file type association");
                retval = false;
            }

            if (arguments.Length > 1)
            {
                Console.WriteLine("exiting. press enter for prompt.");

                FreeConsole();
            }

            return retval;
        }

        private List<string> ProcessFiles(List<string> results)
        {
            List<string> files = new List<string>();

            foreach (string arg in results)
            {
                string cleanPath = Environment.ExpandEnvironmentVariables(arg.Trim('"'));

                switch (GetPathType(cleanPath))
                {
                    case ResourceType.Url:
                        try
                        {
                            // only config files (xml) for now
                            XmlDocument myXmlDocument = new XmlDocument();
                            myXmlDocument.Load(cleanPath);
                            files.Add(cleanPath);
                        }
                        catch (Exception e)
                        {
                            SetStatus("invalid url path: " + cleanPath + e.ToString());
                        }
                        break;

                    case ResourceType.Local:
                    case ResourceType.Unc:

                        try
                        {
                            if (File.Exists(cleanPath))
                            {
                                files.Add(cleanPath);
                            }
                            else
                            {
                                files.AddRange(
                                    Directory.GetFiles(
                                        Path.GetDirectoryName(cleanPath),
                                        Path.GetFileName(cleanPath),
                                        SearchOption.AllDirectories).ToList());
                            }
                        }
                        catch (Exception e)
                        {
                            SetStatus("invalid path: " + e.ToString());
                        }
                        break;

                    case ResourceType.Error:
                    case ResourceType.Unknown:
                    default:
                        SetStatus("unknown file type:" + cleanPath);
                        break;
                }
            }

            return files;
        }

        private void RegisterFTA(bool p)
        {
            throw new NotImplementedException();
        }

        private void VerifyAppSettings()
        {
            try
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
                        case AppSettingNames.CountMaskedMatches:
                            {
                                _appSettings[name].Value = "False";
                                break;
                            }
                        case AppSettingNames.AutoSaveFilters:
                            {
                                _appSettings[name].Value = "False";
                                break;
                            }
                        case AppSettingNames.MaxMultiFileCount:
                            {
                                _appSettings[name].Value = "10";
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                SetStatus("Exception:verifyappsetings:" + e.ToString());
            }
        }

        #endregion Private Methods

        #region Private Fields

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

        #endregion Public Constructors

        #region Public Enums

        public enum ResourceType
        {
            Error,
            Local,
            Unc,
            Unknown,
            Url
        }

        #endregion Public Enums

        #region Private Enums

        private enum AppSettingNames
        {
            AutoSaveFilters,
            BackgroundColor,
            FileHistoryCount,
            FilterDirectory,
            CountMaskedMatches,
            ForegroundColor,
            FontName,
            FontSize,
            CurrentFilterFiles,
            CurrentLogFiles,
            RecentFilterFiles,
            RecentLogFiles,
            MaxMultiFileCount
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

        public string ConfigFile { get; set; }

        public bool CountMaskedMatches
        {
            get
            {
                return (Convert.ToBoolean(_appSettings["CountMaskedMatches"].Value));
            }
            set
            {
                _appSettings["CountMaskedMatches"].Value = value.ToString();
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

        public string FilterDirectory
        {
            get
            {
                return _appSettings["FilterDirectory"].Value;
            }
            set
            {
                if (value.ToString() != _appSettings["FilterDirectory"].Value.ToString())
                {
                    _appSettings["FilterDirectory"].Value = value.ToString();
                    OnPropertyChanged("FilterDirectory");
                }
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

        public int MaxMultiFileCount
        {
            get
            {
                return (Convert.ToInt32(_appSettings["MaxMultiFileCount"].Value));
            }
            set
            {
                _appSettings["MaxMultiFileCount"].Value = value.ToString();
            }
        }

        public string[] RecentFilterFiles
        {
            get
            {
                return _appSettings["RecentFilterFiles"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
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
                return _appSettings["RecentLogFiles"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }

            private set
            {
                _appSettings["RecentLogFiles"].Value = string.Join(",", value);
            }
        }

        #endregion Public Properties

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

        public ResourceType GetPathType(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return ResourceType.Unknown;
                }

                Uri uri;
                if (Uri.TryCreate(path, UriKind.Absolute, out uri))
                {
                    if (uri.IsUnc)
                    {
                        return ResourceType.Unc;
                    }
                    else if (uri.IsLoopback)
                    {
                        return ResourceType.Local;
                    }
                    else
                    {
                        return ResourceType.Url;
                    }
                }

                string fullpath = Path.GetFullPath(path);

                if (fullpath.StartsWith(@"\\\\"))
                {
                    return ResourceType.Unc;
                }

                //else if (fullpath.StartsWith("http")
                //         || fullpath.StartsWith("ftp"))
                //{
                //    return ResourceType.Url;
                //}
                //else if (path.Substring(1, 2).Contains(@":\")
                //         || (Directory.Exists(path)))
                //{
                //    return ResourceType.Local;
                //}
                //else
                //{
                //    return ResourceType.Unknown;
                //}

                if (File.Exists(fullpath)
                  || Directory.Exists(fullpath))
                {
                    return ResourceType.Local;
                }

                return ResourceType.Unknown;
            }
            catch (Exception e)
            {
                SetStatus("GetPathType:Exception" + e.ToString());
                return ResourceType.Error;
            }
        }
    }
}