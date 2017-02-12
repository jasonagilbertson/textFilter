// ************************************************************************************
// Assembly: TextFilter
// File: TextFilterSettings.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Xml;

namespace TextFilter
{
    public class TextFilterSettings : Base
    {
        private const int ATTACH_PARENT_PROCESS = -1;

        private static TextFilterSettings settings;

        private KeyValueConfigurationCollection _appSettings;

        private Configuration _Config;

        private ExeConfigurationFileMap _ConfigFileMap;

        static TextFilterSettings()
        {
            if (settings == null)
            {
                settings = new TextFilterSettings();
            }
        }

        public TextFilterSettings()
        {
        }

        public enum ResourceType
        {
            Error,

            Local,

            Unc,

            Unknown,

            Url
        }

        private enum AppSettingNames
        {
            AutoSave,

            BackgroundColor,

            CountMaskedMatches,

            CurrentFilterFiles,

            CurrentLogFiles,

            DebugFile,

            FileExtensions,

            FilterDirectory,

            FilterHide,

            FileHistoryCount,

            FontName,

            FontSize,

            ForegroundColor,

            HelpUrl,

            MaxMultiFileCount,

            RecentFilterFiles,

            RecentLogFiles,

            SaveSessionInformation,

            SharedFilterDirectory,

            VersionCheckFile,

            WordWrap
        }

        public static TextFilterSettings Settings
        {
            get { return TextFilterSettings.settings; }
            set { TextFilterSettings.settings = value; }
        }

        public bool AutoSave
        {
            get
            {
                return (Convert.ToBoolean(_appSettings[(AppSettingNames.AutoSave).ToString()].Value));
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.AutoSave).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.AutoSave).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.AutoSave).ToString());
                }
            }
        }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                return ((SolidColorBrush)new BrushConverter().ConvertFromString(_appSettings[(AppSettingNames.BackgroundColor).ToString()].Value));
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.BackgroundColor).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.BackgroundColor).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.BackgroundColor).ToString());
                }
            }
        }

        public string BackgroundColorString
        {
            get
            {
                return _appSettings[(AppSettingNames.BackgroundColor).ToString()].Value.ToString();
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.BackgroundColor).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.BackgroundColor).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.BackgroundColor).ToString());
                }
            }
        }

        public string ConfigFile { get; set; }

        public string ContentColumnSize
        {
            // used to dynamically set logview content field tied to 'WordWrap' config setting is not
            // a config file setting
            get
            {
                if (this.WordWrap)
                {
                    return "300*";
                }
                else
                {
                    return "Auto";
                }
            }
        }

        public bool CountMaskedMatches
        {
            get
            {
                return (Convert.ToBoolean(_appSettings[(AppSettingNames.CountMaskedMatches).ToString()].Value));
            }
            set
            {
                _appSettings[(AppSettingNames.CountMaskedMatches).ToString()].Value = value.ToString();
            }
        }

        public List<string> CurrentFilterFiles
        {
            get
            {
                return _appSettings[(AppSettingNames.CurrentFilterFiles).ToString()].Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            set
            {
                _appSettings[(AppSettingNames.CurrentFilterFiles).ToString()].Value = string.Join(";", value);
            }
        }

        public List<string> CurrentLogFiles
        {
            get
            {
                return _appSettings[(AppSettingNames.CurrentLogFiles).ToString()].Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            set
            {
                _appSettings[(AppSettingNames.CurrentLogFiles).ToString()].Value = string.Join(";", value);
            }
        }

        public string DebugFile
        {
            get
            {
                return (_appSettings[(AppSettingNames.DebugFile).ToString()].Value);
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.DebugFile).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.DebugFile).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.DebugFile).ToString());
                }
            }
        }

        public string[] FileExtensions
        {
            get
            {
                return _appSettings[(AppSettingNames.FileExtensions).ToString()].Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            }

            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.FileExtensions).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.FileExtensions).ToString()].Value = string.Join(";", value);
                    OnPropertyChanged((AppSettingNames.FileExtensions).ToString());
                }
            }
        }

        public string FileExtensionsString
        {
            get
            {
                return _appSettings[(AppSettingNames.FileExtensions).ToString()].Value;
            }

            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.FileExtensions).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.FileExtensions).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.FileExtensions).ToString());
                }
            }
        }

        public int FileHistoryCount
        {
            get
            {
                return (Convert.ToInt32(_appSettings[(AppSettingNames.FileHistoryCount).ToString()].Value));
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.FileHistoryCount).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.FileHistoryCount).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.FileHistoryCount).ToString());
                }
            }
        }

        public string FilterDirectory
        {
            get
            {
                return _appSettings[(AppSettingNames.FilterDirectory).ToString()].Value;
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.FilterDirectory).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.FilterDirectory).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.FilterDirectory).ToString());
                }
            }
        }

        public bool FilterHide
        {
            get
            {
                return (Convert.ToBoolean(_appSettings[(AppSettingNames.FilterHide).ToString()].Value.ToString()));
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.FilterHide).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.FilterHide).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.FilterHide).ToString());
                }
            }
        }

        public string FontName
        {
            get
            {
                return _appSettings[(AppSettingNames.FontName).ToString()].Value;
            }
            set
            {
                if (value != _appSettings[(AppSettingNames.FontName).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.FontName).ToString()].Value = value;
                    OnPropertyChanged((AppSettingNames.FontName).ToString());
                }
            }
        }

        //public List<string> FontNameList
        //{
        //    get
        //    {
        //        return new InstalledFontCollection().Families.Select(x => x.Name).ToList();
        //    }
        //}
        public List<string> FontNameList
        {
            get
            {
                List<string> items = new List<string>();

                foreach (string name in (new InstalledFontCollection().Families.Select(x => x.Name).ToList()))
                {
                    items.Add(name);
                }

                return items;
            }
        }

        public int FontSize
        {
            get
            {
                return (Convert.ToInt32(_appSettings[(AppSettingNames.FontSize).ToString()].Value));
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.FontSize).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.FontSize).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.FontSize).ToString());
                }
            }
        }

        public SolidColorBrush ForegroundColor
        {
            get
            {
                return ((SolidColorBrush)new BrushConverter().ConvertFromString(_appSettings[(AppSettingNames.ForegroundColor).ToString()].Value));
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.ForegroundColor).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.ForegroundColor).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.ForegroundColor).ToString());
                }
            }
        }

        public string ForegroundColorString
        {
            get
            {
                return _appSettings[(AppSettingNames.ForegroundColor).ToString()].Value.ToString();
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.ForegroundColor).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.ForegroundColor).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.ForegroundColor).ToString());
                }
            }
        }

        public string HelpUrl
        {
            get
            {
                return _appSettings[(AppSettingNames.HelpUrl).ToString()].Value;
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.HelpUrl).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.HelpUrl).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.HelpUrl).ToString());
                }
            }
        }

        public int MaxMultiFileCount
        {
            get
            {
                return (Convert.ToInt32(_appSettings[(AppSettingNames.MaxMultiFileCount).ToString()].Value));
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.MaxMultiFileCount).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.MaxMultiFileCount).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.MaxMultiFileCount).ToString());
                }
            }
        }

        public string[] RecentFilterFiles
        {
            get
            {
                return _appSettings[(AppSettingNames.RecentFilterFiles).ToString()].Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            }

            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.RecentFilterFiles).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.RecentFilterFiles).ToString()].Value = string.Join(";", value);
                    OnPropertyChanged((AppSettingNames.RecentFilterFiles).ToString());
                }
            }
        }

        public string[] RecentLogFiles
        {
            get
            {
                return _appSettings[(AppSettingNames.RecentLogFiles).ToString()].Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            }

            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.RecentLogFiles).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.RecentLogFiles).ToString()].Value = string.Join(";", value);
                    OnPropertyChanged((AppSettingNames.RecentLogFiles).ToString());
                }
            }
        }

        public bool SaveSessionInformation
        {
            get
            {
                return (Convert.ToBoolean(_appSettings[(AppSettingNames.SaveSessionInformation).ToString()].Value));
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.SaveSessionInformation).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.SaveSessionInformation).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.SaveSessionInformation).ToString());
                }
            }
        }

        public string SharedFilterDirectory
        {
            get
            {
                return _appSettings[(AppSettingNames.SharedFilterDirectory).ToString()].Value;
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.SharedFilterDirectory).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.SharedFilterDirectory).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.SharedFilterDirectory).ToString());
                }
            }
        }

        public string VersionCheckFile
        {
            get
            {
                return _appSettings[(AppSettingNames.VersionCheckFile).ToString()].Value;
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.VersionCheckFile).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.VersionCheckFile).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.VersionCheckFile).ToString());
                }
            }
        }

        public bool WordWrap
        {
            get
            {
                // value was string now bool
                try
                {
                    return (Convert.ToBoolean(_appSettings[(AppSettingNames.WordWrap).ToString()].Value));
                }
                catch
                {
                    WordWrap = true;
                    return true;
                }
            }
            set
            {
                if (value.ToString() != _appSettings[(AppSettingNames.WordWrap).ToString()].Value.ToString())
                {
                    _appSettings[(AppSettingNames.WordWrap).ToString()].Value = value.ToString();
                    OnPropertyChanged((AppSettingNames.WordWrap).ToString());

                    // to have forms updated using string value
                    OnPropertyChanged("WordWrapString");
                }
            }
        }

        public string WordWrapString
        {
            // Wrap or NoWrap
            get
            {
                return WordWrap ? "Wrap" : "NoWrap";
            }
        }

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

        public bool ReadConfigFile()
        {
            // check config file first
            List<string> results = ProcessArg("/config:", Environment.GetCommandLineArgs());
            if (results.Count == 1)
            {
                Settings.ConfigFile = Environment.ExpandEnvironmentVariables(results[0]);
            }

            ConfigFile = !string.IsNullOrEmpty(ConfigFile) ? ConfigFile : string.Format("{0}.config", Process.GetCurrentProcess().MainModule.FileName);
            _ConfigFileMap = new ExeConfigurationFileMap();
            if (!File.Exists(ConfigFile))
            {
                XmlTextWriter xmlw = new XmlTextWriter(ConfigFile, System.Text.Encoding.UTF8);
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

            _ConfigFileMap.ExeConfigFilename = ConfigFile;

            // Get the mapped configuration file.
            _Config = ConfigurationManager.OpenMappedExeConfiguration(_ConfigFileMap, ConfigurationUserLevel.None);
            _appSettings = _Config.AppSettings.Settings;

            VerifyAppSettings(false);

            // verify files
            CurrentFilterFiles = ProcessFiles(CurrentFilterFiles);
            CurrentLogFiles = ProcessFiles(CurrentLogFiles);

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
            List<string> filterFiles = new List<string>(CurrentFilterFiles);
            if (filterFiles.Contains(filterFile))
            {
                Debug.Print("Removing FilterFile:" + filterFile);
                filterFiles.Remove(filterFile);
                CurrentFilterFiles = filterFiles;
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
            // TODO: FIX EXCEPTION when two instances open and one saves to config file first may
            // need to use reflection
            /*
                    KeyValueConfigurationElement[] kvcea = new KeyValueConfigurationElement[AppSettings.ToKeyValueConfigurationCollection().Count];
                    AppSettings.ToKeyValueConfigurationCollection().CopyTo(kvcea, 0);
                    List<KeyValueConfigurationElement> kList = kvcea.ToList();
                    kList.Sort((a, b) => { return (a.Key.CompareTo(b.Key)); });

                    foreach (KeyValueConfigurationElement kvce in kList)
                    {
                        _Config.AppSettings.Settings.Remove(kvce.Key);
                        _Config.AppSettings.Settings.Add(kvce);
                    } */
            try
            {
                if (!SaveSessionInformation)
                {
                    // set current properties back to what they were
                    List<string> currentFilterFiles = new List<string>(CurrentLogFiles);
                    List<string> currentLogFiles = new List<string>(CurrentLogFiles);
                    CurrentLogFiles = new List<string>();
                    CurrentFilterFiles = new List<string>();

                    _Config.Save(ConfigurationSaveMode.Minimal, true);

                    CurrentLogFiles = currentLogFiles;
                    CurrentFilterFiles = currentFilterFiles;
                }
                else
                {
                    _Config.Save(ConfigurationSaveMode.Minimal, true);
                }
            }
            catch { }
        }

        public TextFilterSettings ShallowCopy()
        {
            return (TextFilterSettings)MemberwiseClone();
        }

        public void VerifyAppSettings(bool overwrite)
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
                    if (!overwrite && !string.IsNullOrEmpty(_appSettings[name].Value))
                    {
                        continue;
                    }

                    switch ((AppSettingNames)Enum.Parse(typeof(AppSettingNames), name))
                    {
                        case AppSettingNames.AutoSave:
                            {
                                _appSettings[name].Value = "False";
                                break;
                            }
                        case AppSettingNames.BackgroundColor:
                            {
                                _appSettings[name].Value = "AliceBlue";
                                break;
                            }
                        case AppSettingNames.CountMaskedMatches:
                            {
                                _appSettings[name].Value = "False";
                                break;
                            }
                        case AppSettingNames.CurrentFilterFiles:
                            {
                                _appSettings[name].Value = "";
                                break;
                            }
                        case AppSettingNames.CurrentLogFiles:
                            {
                                _appSettings[name].Value = "";
                                break;
                            }

                        case AppSettingNames.DebugFile:
                            {
                                _appSettings[name].Value = "";
                                break;
                            }
                        case AppSettingNames.FileExtensions:
                            {
                                _appSettings[name].Value = ".log;.rvf";
                                break;
                            }
                        case AppSettingNames.FileHistoryCount:
                            {
                                _appSettings[name].Value = "20";
                                break;
                            }
                        case AppSettingNames.FilterHide:
                            {
                                _appSettings[name].Value = "False";
                                break;
                            }
                        case AppSettingNames.FontName:
                            {
                                _appSettings[name].Value = "Lucida Console";
                                break;
                            }
                        case AppSettingNames.FontSize:
                            {
                                _appSettings[name].Value = "12";
                                break;
                            }
                        case AppSettingNames.ForegroundColor:
                            {
                                _appSettings[name].Value = "Black";
                                break;
                            }
                        case AppSettingNames.HelpUrl:
                            {
                                _appSettings[name].Value = "https://github.com/jasonagilbertson/TextFilter";
                                break;
                            }

                        case AppSettingNames.MaxMultiFileCount:
                            {
                                _appSettings[name].Value = "10";
                                break;
                            }
                        case AppSettingNames.RecentFilterFiles:
                            {
                                _appSettings[name].Value = "";
                                break;
                            }
                        case AppSettingNames.RecentLogFiles:
                            {
                                _appSettings[name].Value = "";
                                break;
                            }

                        case AppSettingNames.SaveSessionInformation:
                            {
                                _appSettings[name].Value = "True";
                                break;
                            }
                        case AppSettingNames.VersionCheckFile:
                            {
                                _appSettings[name].Value = "https://raw.githubusercontent.com/jasonagilbertson/TextFilter/master/TextFilter/version.xml";
                                break;
                            }

                        case AppSettingNames.WordWrap:
                            {
                                _appSettings[name].Value = "True";
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
            List<string> newList = new List<string>(recentLogFiles);

            if (newList.Contains(logFile))
            {
                newList.Remove(logFile);
            }

            while (newList.Count >= settings.FileHistoryCount)
            {
                newList.RemoveAt(0);
            }
            if (GetPathType(logFile) != ResourceType.Url)
            {
                if (!File.Exists(logFile))
                {
                    return (newList.ToArray());
                }
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
                    // remove argument name
                    string argument = arguments[i].ToLower().Replace(setting, "");

                    // argument had space in it /filter: filter1, filter2
                    if (string.IsNullOrEmpty(argument)
                        && (arguments.Length > i + 1)
                        && !arguments[i + 1].StartsWith("/"))
                    {
                        argument = arguments[i + 1].Trim();
                    }

                    if (string.IsNullOrEmpty(argument))
                    {
                        // its an argument with no value
                        args.Add(arguments[i].Trim());
                    }

                    foreach (string arg in argument.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
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

                if (arguments[1] != null)
                {
                    string filename = arguments[1];
                    if ((Path.GetExtension(filename).ToLower() == ".xml"
                        | Path.GetExtension(filename).ToLower() == ".rvf"
                        | Path.GetExtension(filename).ToLower() == ".tat")
                        && new FilterFileManager().FilterFileVersion(filename) != FilterFileManager.FilterFileVersionResult.NotAFilterFile)
                    {
                        Settings.AddFilterFile(Environment.ExpandEnvironmentVariables(filename));
                    }
                    else
                    {
                        // not a filter file
                        Settings.AddLogFile(Environment.ExpandEnvironmentVariables(filename));
                    }
                }
            }
            else if (arguments.Length == 2 && arguments[1].Contains("/?")
                | (arguments.Length == 2 && arguments[1].Contains("-?")))
            {
                DisplayHelp();
                retval = false;
            }

            List<string> results = new List<string>();

            //results = ProcessArg("/config:", arguments);
            //if (results.Count == 1)
            //{
            //    Settings.ConfigFile = Environment.ExpandEnvironmentVariables(results[0]);
            //}

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
                Console.WriteLine("registering file type association");
                FileTypeAssociation.Instance.ConfigureFTA(true);

                retval = false;
            }

            if (ProcessArg("/unregister", arguments).Count > 0)
            {
                Console.WriteLine("unregistering file type association");
                FileTypeAssociation.Instance.ConfigureFTA(false);

                retval = false;
            }

            if (arguments.Length > 1)
            {
                Console.WriteLine("Press Enter to continue...");

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
    }
}