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

        #region Fields

        public System.Timers.Timer _timer;

        private Command _copyCommand;

        

        private Command _helpCommand;

        
        private TextFilterSettings _settings;

        private Command _settingsCommand;

        private ObservableCollection<ListBoxItem> _status = new ObservableCollection<ListBoxItem>();

        private Command _statusChangedCommand;

        private Int32 _statusIndex;

        private Command _versionCheckCommand;

        private WorkerManager _workerManager = WorkerManager.Instance;

        #endregion Fields

        #region Constructors

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

                Base.NewStatus += HandleNewStatus;

                Base._Parser = new Parser();                        
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
                _timer.Enabled = true;

                SetStatus("loaded");
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception:" + e.ToString());
            }
        }

        #endregion Constructors

        #region Properties

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

        public ObservableCollection<ListBoxItem> Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        public Command StatusChangedCommand
        {
            get
            {
                if (_statusChangedCommand == null)
                {
                    _statusChangedCommand = new Command(StatusChangedExecuted);
                }
                _statusChangedCommand.CanExecute = true;

                return _statusChangedCommand;
            }
            set { _statusChangedCommand = value; }
        }

        public int StatusIndex
        {
            get
            {
                return _statusIndex;
            }
            set
            {
                if (_statusIndex != value)
                {
                    _statusIndex = value;
                    OnPropertyChanged("StatusIndex");
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

        #endregion Properties

        #region Methods

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
            string workingDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            CreateProcess("notepad.exe", workingDir + "\\TextFilter.exe.config");
            MessageBox.Show("TextFilter window settings are not in gui yet.\n\n"
                + "To change settings:\n"
                + " 1. Close TextFilter.exe as it can overwrite on close.\n"
                + " 2. Make changes in TextFilter.exe.config.\n"
                + " 3. Restart TextFilter.exe.", "TextFilter Window Settings", MessageBoxButton.OK);
            // OptionsDialog dialog = new OptionsDialog();
            //dialog.WaitForResult();
        }

        public void SetViewStatus(string statusData)
        {
            try
            {
                Application.Current.Dispatcher.InvokeAsync((Action)delegate ()
                {

                    while (this.Status.Count > 1000)
                    {
                        this.Status.RemoveAt(0);
                    }

                    ListBoxItem listBoxItem = new ListBoxItem();
                    listBoxItem.Content = string.Format("{0}: {1}", DateTime.Now.ToString("hh:mm:ss.fff"), statusData);
                    this.Status.Add(listBoxItem);
                    this.StatusIndex = Status.Count - 1;

                    Debug.Print(statusData);
                    OnPropertyChanged("Status");
                });
            }
            catch (Exception e)
            {
                Debug.Print(string.Format("SetViewStatus:exception: {0}: {1}", statusData, e));
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
            // force update of shared collection menu
            var oc = _FilterViewModel.SharedCollection;
            VersionCheck(silent);
        }

        private void HandleNewStatus(object sender, string status)
        {
            SetViewStatus(status);
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

        #endregion Methods

    }
}