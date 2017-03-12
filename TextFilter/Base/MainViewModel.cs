// ************************************************************************************
// Assembly: TextFilter
// File: mainviewmodel.cs
// Created: 12/2/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace TextFilter
{
    public class MainViewModel : Base, IMainViewModel
    {
        public System.Timers.Timer _timer;
        private StringBuilder _color = new StringBuilder();
        private List<string> _colorNames = new List<string>();
        private Command _controlGotFocusCommand;
        private Command _controlLostFocusCommand;

        private string _currentStatus;

        private Command _duplicateWindowCommand;

        private Command _helpCommand;

        private Command _listViewSelectionChangedCommand;
        private TextFilterSettings _settings;

        private Command _settingsCommand;

        private ObservableCollection<ListBoxItem> _status = new ObservableCollection<ListBoxItem>();

        private Command _versionCheckCommand;

        private WorkerManager _workerManager = WorkerManager.Instance;

        public MainViewModel()
        {
            try
            {
                _settings = TextFilterSettings.Settings;
                if (!_settings.ReadConfigFile())
                {
                    Environment.Exit(1);
                    Application.Current.Shutdown(1);
                    return;
                }

                _colorNames = GetColorNames();
                // clean up old log file if exists
                if (!string.IsNullOrEmpty(Settings.DebugFile) && File.Exists(Settings.DebugFile))
                {
                    File.Delete(Settings.DebugFile);
                }

                SetStatus("Starting textFilter: " + Process.GetCurrentProcess().Id.ToString());
                Base._Parser = new Parser();
                Base.NewCurrentStatus += HandleNewCurrentStatus;
                Base._FilterViewModel = new FilterViewModel();
                Base._LogViewModel = new LogViewModel();
                
                _Parser.Enable(true);

                App.Current.MainWindow.Title = string.Format("{0} {1}",
                    Process.GetCurrentProcess().MainModule.ModuleName,
                    Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion);

                SetStatus(App.Current.MainWindow.Title);

                // to embed external libraries
                // http: //blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    String resourceName = "TextFilter." + new AssemblyName(args.Name).Name + ".dll";

                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        Byte[] assemblyData = new Byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);

                        return Assembly.Load(assemblyData);
                    }
                };

                _timer = new System.Timers.Timer(10000);
                _timer.AutoReset = false;
                _timer.Elapsed += _timer_Elapsed;

                SetStatus("loaded");
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception:" + e.ToString());
            }
        }

        public Command ControlGotFocusCommand
        {
            get
            {
                if (_controlGotFocusCommand == null)
                {
                    _controlGotFocusCommand = new Command(ControlGotFocusExecuted);
                }
                _controlGotFocusCommand.CanExecute = true;

                return _controlGotFocusCommand;
            }
            set { _controlGotFocusCommand = value; }
        }

        public Command ControlLostFocusCommand
        {
            get
            {
                if (_controlLostFocusCommand == null)
                {
                    _controlLostFocusCommand = new Command(ControlLostFocusExecuted);
                }
                _controlLostFocusCommand.CanExecute = true;

                return _controlLostFocusCommand;
            }
            set { _controlLostFocusCommand = value; }
        }

        public string CurrentStatus
        {
            get
            {
                return _currentStatus;
            }
            set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    OnPropertyChanged("CurrentStatus");
                }
            }
        }

        public Command DuplicateWindowCommand
        {
            get
            {
                if (_duplicateWindowCommand == null)
                {
                    _duplicateWindowCommand = new Command(DuplicateWindowExecuted);
                }
                _duplicateWindowCommand.CanExecute = true;

                return _duplicateWindowCommand;
            }
            set { _duplicateWindowCommand = value; }
        }

        public FilterViewModel FilterViewModel
        {
            get { return _FilterViewModel; }
            set { _FilterViewModel = value; }
        }

        public Command HelpCommand
        {
            get
            {
                if (_helpCommand == null)
                {
                    _helpCommand = new Command(HelpExecuted);
                }
                _helpCommand.CanExecute = true;

                return _helpCommand;
            }
            set { _helpCommand = value; }
        }

        public Command ListViewSelectionChangedCommand
        {
            get { return _listViewSelectionChangedCommand ?? new Command(ListViewSelectionChangedExecuted); }
            set { _listViewSelectionChangedCommand = value; }
        }

        public LogViewModel LogViewModel
        {
            get { return _LogViewModel; }
            set { _LogViewModel = value; }
        }

        public TextFilterSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        public Command SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                {
                    _settingsCommand = new Command(SettingsExecuted);
                }
                _settingsCommand.CanExecute = true;

                return _settingsCommand;
            }
            set { _settingsCommand = value; }
        }

        public Command VersionCheckCommand
        {
            get
            {
                if (_versionCheckCommand == null)
                {
                    _versionCheckCommand = new Command(VersionCheckExecuted);
                }
                _versionCheckCommand.CanExecute = true;

                return _versionCheckCommand;
            }
            set { _versionCheckCommand = value; }
        }

        public void ColorComboKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is ComboBox)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                    case Key.Tab:
                    case Key.Back:
                        {
                            _color.Clear();
                            return;
                        }
                    default:
                        {
                            break;
                        }
                }

                // dont add if not alpha character
                if (!Regex.IsMatch(e.Key.ToString(), "[a-zA-Z]{1}", RegexOptions.IgnoreCase))
                {
                    return;
                }

                _color.Append(e.Key.ToString());
                ComboBox comboBox = (sender as ComboBox);

                string color = _colorNames.FirstOrDefault(c => Regex.IsMatch(c, "^" + _color.ToString(), RegexOptions.IgnoreCase));
                if (String.IsNullOrEmpty(color))
                {
                    color = _colorNames.FirstOrDefault(c => Regex.IsMatch(c, _color.ToString(), RegexOptions.IgnoreCase));
                }
                if (!String.IsNullOrEmpty(color))
                {
                    comboBox.SelectedValue = color;
                }
                else
                {
                    comboBox.SelectedIndex = 0;
                }
            }
        }

        public void ColorComboSelected()
        {
            _color.Clear();
        }

        public List<string> GetColorNames()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            List<string> list = new List<string>();
            foreach (var prop in typeof(Colors).GetProperties(flags))
            {
                if (prop.PropertyType.FullName == "System.Windows.Media.Color")
                {
                    Debug.Print(prop.PropertyType.FullName);
                    list.Add(prop.Name);
                }
            }
            return list;
        }

        public void HelpExecuted(object sender)
        {
            CreateProcess(Settings.HelpUrl);
        }

        public void ListViewSelectionChangedExecuted(object sender)
        {
            if (sender is ListView)
            {
                if ((sender as ListView).SelectedItem != null)
                {
                    ((ListViewItem)(sender as ListView).SelectedItem).BringIntoView();
                }
            }
            else
            {
                Debug.Print("listviewselectionchanged but invalid call");
            }
        }

        public void SettingsExecuted(object sender)
        {
            TextFilterSettings configFileCache = Settings.ShallowCopy();

            OptionsDialog dialog = new OptionsDialog();

            switch (dialog.WaitForResult())
            {
                case OptionsDialog.OptionsDialogResult.apply:
                    {
                        DuplicateWindow();
                        Application.Current.Shutdown();
                        break;
                    }
                //case OptionsDialog.OptionsDialogResult.cancel:
                //    Settings = configFileCache.ShallowCopy();
                //    break;
                case OptionsDialog.OptionsDialogResult.edit:
                    string workingDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                    CreateProcess("notepad.exe", Settings.ConfigFile);
                    break;

                case OptionsDialog.OptionsDialogResult.register:
                    ExecuteAsAdmin(Process.GetCurrentProcess().MainModule.FileName, "/register");
                    SettingsExecuted(null);
                    //FileTypeAssociation.Instance.ConfigureFTA(true);
                    break;

                case OptionsDialog.OptionsDialogResult.reset:
                    Settings.VerifyAppSettings(true);
                    SettingsExecuted(null);
                    break;

                case OptionsDialog.OptionsDialogResult.save:
                    Settings.Save();
                    break;

                case OptionsDialog.OptionsDialogResult.unregister:
                    ExecuteAsAdmin(Process.GetCurrentProcess().MainModule.FileName, "/unregister");
                    SettingsExecuted(null);
                    //FileTypeAssociation.Instance.ConfigureFTA(false);
                    break;

                case OptionsDialog.OptionsDialogResult.unknown:
                default:
                    break;
            }

            _FilterViewModel.Refresh();
        }

        public void VersionCheckExecuted(object sender)
        {
            VersionCheck(false);
        }

        internal void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Parser.Enable(false);
            _FilterViewModel.SaveModifiedFiles(sender);
            _LogViewModel.SaveModifiedFiles(sender);
            _settings.Save();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => AfterLaunch(true)));
        }

        private void AfterLaunch(bool silent)
        {
            // clean recent lists
            _FilterViewModel.GroomFiles();
            _LogViewModel.GroomFiles();

            VersionCheck(silent);
        }

        private void ControlGotFocusExecuted(object sender)
        {
            //if (sender is Control)
            //{
            //    (sender as Control).BorderBrush = ((SolidColorBrush)new BrushConverter().ConvertFromString("Chartreuse"));
            //    (sender as Control).BorderThickness = new Thickness(2);
            //}
        }

        private void ControlLostFocusExecuted(object sender)
        {
            if (sender is ComboBox && string.IsNullOrEmpty((sender as ComboBox).Text))
            {
                _FilterViewModel.QuickFindChangedExecuted(sender);
                //    (sender as Control).BorderBrush = Settings.ForegroundColor;
                //    (sender as Control).BorderThickness = new Thickness(1);
            }
        }

        private void DuplicateWindow()
        {
            StringBuilder args = new StringBuilder();
            if (Settings.CurrentFilterFiles.Count > 0)
            {
                args.Append(string.Format("/filter: \"{0}\"", string.Join("\";\"", Settings.CurrentFilterFiles)));
            }

            if (Settings.CurrentLogFiles.Count > 0)
            {
                if (args.Length > 0)
                {
                    args.Append(" ");
                }

                args.Append(string.Format("/log: \"{0}\"", string.Join("\";\"", Settings.CurrentLogFiles)));
            }

            Settings.Save();
            CreateProcess(Process.GetCurrentProcess().MainModule.FileName, args.ToString());
            Debug.Print(args.ToString());
        }

        private void DuplicateWindowExecuted(object sender)
        {
            DuplicateWindow();
        }

        private void HandleNewCurrentStatus(object sender, string status)
        {
            CurrentStatus = status;
        }

        private void HandleNewStatus(object sender, string status)
        {
            CurrentStatus = status;
        }

        private void VersionCheck(bool silent)
        {
            string destFile = string.Empty;
            string downloadLocation = string.Empty;
            string message = string.Empty;
            string version = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(Settings.VersionCheckFile))
                {
                    message = "version check url not specified";

                    if (silent)
                    {
                        SetStatus(message);
                    }
                    else
                    {
                        MessageBox.Show(message);
                    }
                    return;
                }

                string workingVersion = Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion;
                string workingDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                XmlDocument doc = new XmlDocument();
                doc.Load(Settings.VersionCheckFile);

                XmlNode root = doc.DocumentElement;

                if (root.Name.ToLower() == "updateinfo")
                {
                    version = root.SelectSingleNode("version").InnerText;
                    downloadLocation = root.SelectSingleNode("download").InnerText;
                }
                if (string.IsNullOrEmpty(downloadLocation))
                {
                    message = "unable to check version: " + Settings.VersionCheckFile;
                }
                else if (string.Compare(version, workingVersion, true) > 0)
                {
                    if (silent)
                    {
                        SetStatus("VersionCheck: new version available");
                        App.Current.MainWindow.Title = string.Format(
                            "{0} {1} ** NEW VERSION AVAILABLE **. Select Help->Check for new version",
                            Process.GetCurrentProcess().MainModule.ModuleName,
                            Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion);
                    }
                    else
                    {
                        MessageBoxResult mbResult = MessageBox.Show(string.Format("New version available.\n Do you want to download from {0}?", downloadLocation), "New version", MessageBoxButton.YesNo);
                        if (mbResult == MessageBoxResult.Yes)
                        {
                            string downloadZip = string.Format("{0}\\{1}", workingDir.TrimEnd('\\'), Path.GetFileName(downloadLocation));
                            (new System.Net.WebClient()).DownloadFile(downloadLocation, downloadZip);

                            string downloadZipDir = string.Format("{0}-{1}", Path.GetFileNameWithoutExtension(downloadZip), version);

                            if (Directory.Exists(downloadZipDir))
                            {
                                Directory.Delete(downloadZipDir, true);
                            }

                            // extract zip
                            ZipFile.ExtractToDirectory(downloadZip, downloadZipDir);
                            File.Delete(downloadZip);

                            string currentExe = Process.GetCurrentProcess().MainModule.FileName;
                            //string currentConfig = currentExe + ".config";
                            string currentProcessName = Process.GetCurrentProcess().MainModule.ModuleName;
                            string newExe = string.Format("{0}\\{1}", downloadZipDir, currentProcessName);
                            //string newConfig = newExe + ".config";

                            // overwrite exe
                            if(File.Exists(currentExe + ".old"))
                            {
                                File.Delete(currentExe + ".old");
                            }

                            File.Move(currentExe, currentExe + ".old");
                            File.Copy(newExe, currentExe, true);

                            mbResult = MessageBox.Show("textFilter updated. do you want to restart textFilter now?", "New version updated", MessageBoxButton.YesNo);
                            if (mbResult == MessageBoxResult.Yes)
                            {
                                // todo: merge configs?
                                DuplicateWindow();
                                Application.Current.Shutdown();
                            }
                        }
                    }
                }
                else if (string.Compare(version, workingVersion, true) < 0)
                {
                    message = string.Format("running version is newer. running ver: {0} update ver: {1}", workingVersion, version);
                }
                else
                {
                    message = "version is same: " + workingVersion;
                }

                if (silent)
                {
                    SetStatus("VersionCheck:" + message);
                }
                else
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        MessageBox.Show(message);
                    }
                }

                return;
            }
            catch (Exception e)
            {
                if (silent)
                {
                    SetStatus("VersionCheckExecuted:exception:" + e.ToString());
                }
                else
                {
                    MessageBox.Show(string.Format("Unable to read version info from {0}.\n\nerror: {1}", Settings.VersionCheckFile,  e.ToString()),"update error");
                }

                return;
            }
        }
    }
}