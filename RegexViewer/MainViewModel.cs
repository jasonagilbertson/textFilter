using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace RegexViewer
{
    public class MainViewModel : Base, IMainViewModel
    {
        #region Private Fields
        private Command _copyCommand;

        private FilterViewModel _filterViewModel;

        private LogViewModel _logViewModel;

        
        private RegexViewerSettings _settings;

        private ObservableCollection<ListBoxItem> _status = new ObservableCollection<ListBoxItem>();

        private Command _statusChangedCommand;

        private Int32 _statusIndex;

        private WorkerManager _workerManager = WorkerManager.Instance;

        #endregion Private Fields

        #region Public Constructors

        public MainViewModel()
        {
            
            _settings = RegexViewerSettings.Settings;
            if(!_settings.ReadConfigFile())
            {
                Environment.Exit(1);
            }

            
            
            //Base.MainModel = this;
            Base.NewStatus += HandleNewStatus;
            _filterViewModel = new FilterViewModel();
            _logViewModel = new LogViewModel(_filterViewModel);

            // to embed external libraries
            // http: //blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                String resourceName = "RegexViewer." + new AssemblyName(args.Name).Name + ".dll";

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];

                    stream.Read(assemblyData, 0, assemblyData.Length);

                    return Assembly.Load(assemblyData);
                }
            };

            SetStatus("loaded");
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

        // public event PropertyChangedEventHandler PropertyChanged;
        public LogViewModel LogViewModel
        {
            get { return _logViewModel; }
            set { _logViewModel = value; }
        }

        public RegexViewerSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
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
                //else if (contentList is ObservableCollection<string>)
                //{
                //    c_contentList = new List<ListBoxItem>((ObservableCollection<string>)contentList);
                //}
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
                    //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
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

        public void SetViewStatus(string statusData)
        {
            // Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => {
            while (this.Status.Count > 1000)
            {
                this.Status.RemoveAt(0);
            }

            //this.Status.Add(string.Format("{0}: {1}",DateTime.Now,statusData));
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = string.Format("{0}: {1}", DateTime.Now, statusData);
            this.Status.Add(listBoxItem);
            this.StatusIndex = Status.Count - 1;

            Debug.Print(statusData);
            OnPropertyChanged("Status");
            // }));
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

        #endregion Public Methods

        #region Internal Methods

        internal void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _filterViewModel.SaveModifiedFiles(sender);
            _settings.Save();
        }

        #endregion Internal Methods

        #region Private Methods

        private void HandleNewStatus(object sender, string status)
        {
            SetViewStatus(status);
        }

        #endregion Private Methods

        //public void QuickFindChangedExecuted(object sender)
        //{
        //    // todo: move to filter source?
        //    if (sender is string)
        //    {
        //        string filter = (sender as string);
        //        if(string.IsNullOrEmpty(filter))
        //        {
        //            // send empty function to reset to current filter in filterview
        //            _logViewModel.FilterTabItem(null, null);
        //            return;
        //        }

        // FilterFileItem fileItem = new FilterFileItem() { Filterpattern = (sender as string) };
        // try { Regex test = new Regex(fileItem.Filterpattern); fileItem.Regex = true;

        // } catch { SetStatus("quick find not a regex:" + fileItem.Filterpattern); fileItem.Regex =
        // false; }

        //        fileItem.Enabled = true;
        //        _logViewModel.FilterTabItem(fileItem);
        //    }
        //}
    }
}