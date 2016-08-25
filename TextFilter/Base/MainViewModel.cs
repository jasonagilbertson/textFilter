// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-31-2015 ***********************************************************************
// <copyright file="MainViewModel.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace TextFilter
{
    public class MainViewModel : Base, IMainViewModel
    {
      
        #region Private Fields

        public System.Timers.Timer _timer;

        private Command _copyCommand;

        private FilterViewModel _filterViewModel;

        private Command _helpCommand;

        private LogViewModel _logViewModel;

        private TextFilterSettings _settings;

        private Command _settingsCommand;

        private ObservableCollection<ListBoxItem> _status = new ObservableCollection<ListBoxItem>();

        private string _currentStatus;

        private Command _versionCheckCommand;

        private WorkerManager _workerManager = WorkerManager.Instance;

        #endregion Private Fields

        #region Public Constructors

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

                // clean up old log file if exists
                if(!string.IsNullOrEmpty(Settings.DebugFile) && File.Exists(Settings.DebugFile))
                {
                    File.Delete(Settings.DebugFile);
                }

                SetStatus("Starting textFilter: " + Process.GetCurrentProcess().Id.ToString());
                Base.NewCurrentStatus += HandleNewCurrentStatus;

                _filterViewModel = new FilterViewModel();
                _logViewModel = new LogViewModel(_filterViewModel);

                _filterViewModel._LogViewModel = _logViewModel;

               
                App.Current.MainWindow.Title = string.Format("{0} {1}", // {2}",
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
                _timer.Enabled = true;

                SetStatus("loaded");
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception:" + e.ToString());
            }
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => AfterLaunch(true)));
        }

        #endregion Public Constructors

        #region Public Properties

        public Command CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new Command(CopyExecuted);
                }
                _copyCommand.CanExecute = true;

                return _copyCommand;
            }
            set { _copyCommand = value; }
        }

        public FilterViewModel FilterViewModel
        {
            get { return _filterViewModel; }
            set { _filterViewModel = value; }
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

        public LogViewModel LogViewModel
        {
            get { return _logViewModel; }
            set { _logViewModel = value; }
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

        #endregion Public Properties

        #region Public Methods

        public void CopyExecuted(object contentList)
        {
            List<ListBoxItem> c_contentList = new List<ListBoxItem>();

            try
            {

                if (contentList is List<ListBoxItem>)
                {
                    c_contentList = (List<ListBoxItem>)contentList;
                }
                else if (contentList is ObservableCollection<ListBoxItem>)
                {
                    c_contentList = new List<ListBoxItem>((ObservableCollection<ListBoxItem>)contentList);
                }
                else
                {
                    return;
                }

                HtmlFragment htmlFragment = new HtmlFragment();
                foreach (ListBoxItem lbi in c_contentList)
                {
                    if (lbi != null && lbi.IsSelected)
                    {
                        htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
                    }
                }

                htmlFragment.CopyListToClipboard();
            }
            catch (Exception ex)
            {
                SetStatus("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

        public void HelpExecuted(object sender)
        {
            CreateProcess(Settings.HelpUrl);
        }

        public void SettingsExecuted(object sender)
        {
            TextFilterSettings configFileCache = Settings.ShallowCopy();
            
            OptionsDialog dialog = new OptionsDialog();

            switch(dialog.WaitForResult())
            {
                //case OptionsDialog.OptionsDialogResult.apply:
                //    CreateProcess(Process.GetCurrentProcess().MainModule.FileName, 
                //        string.Format("/filter: \"{0}\" /log: \"{1}\"", 
                //        string.Join("\";\"", Settings.CurrentFilterFiles),
                //        string.Join("\";\"",Settings.CurrentLogFiles)));
                //    Application.Current.Shutdown();
                //    break;
                case OptionsDialog.OptionsDialogResult.cancel:
                    Settings = configFileCache.ShallowCopy();
                    break;
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
        }

        public void StatusChangedExecuted(object sender)
        {
            try
            {
                if (sender is ListBox)
                {
                    ListBox listBox = (sender as ListBox);
                    listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
                }
            }
            catch { }
        }

        public void VersionCheckExecuted(object sender)
        {
            VersionCheck(false);
        }

        private void AfterLaunch(bool silent)
        {
            // force update of shared collection menu
            var oc = _filterViewModel.SharedCollection;
            VersionCheck(silent);
        }

        private void VersionCheck(bool silent)
        {
            try
            {
                if (string.IsNullOrEmpty(Settings.VersionCheckFile))
                {
                    if (silent)
                    {
                        SetStatus("VersionCheckExecuted:version check url not specified");
                    }
                    else
                    {
                        MessageBox.Show("version check url not specified");
                    }

                    return;
                }

                string version = string.Empty;
                string workingVersion = Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion;
                string downloadLocation = string.Empty;
                string destFile = string.Empty;
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
                    string message = "unable to check version: " + Settings.VersionCheckFile;
                    if (silent)
                    {
                        SetStatus("VersionCheck:" + message);
                    }
                    else
                    {
                        MessageBox.Show(message);
                    }
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
                            MessageBox.Show(string.Format("New version has been downloaded from: {0} \n to: {1}.\nExtract zip and restart.", downloadLocation, downloadZip));
                            CreateProcess("explorer.exe", workingDir);
                        }
                    }
                }
                else
                {
                    string message = "version is same: " + workingVersion;
                    if (silent)
                    {
                        SetStatus("VersionCheck:" + message);
                    }
                    else
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
                    MessageBox.Show("Unable to read version info from " + Settings.VersionCheckFile);
                }

                return;
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetStatus("Stopping textFilter: " + Process.GetCurrentProcess().Id.ToString());
            _logViewModel.Parser.Enable(false);
            _filterViewModel.SaveModifiedFiles(sender);
            _logViewModel.SaveModifiedFiles(sender);
            _settings.Save();
        }

        #endregion Internal Methods

        #region Private Methods

        private void HandleNewCurrentStatus(object sender, string status)
        {
            CurrentStatus = status;
        }
        #endregion Private Methods
    }
}