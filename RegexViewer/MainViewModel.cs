using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RegexViewer
{
    public class MainViewModel : Base, IMainViewModel
    {
        #region Private Fields

        private Command _copyCommand;
        private FilterViewModel _filterViewModel;
        private LogViewModel _logViewModel;
        private RegexViewerSettings _settings = RegexViewerSettings.Settings;
        private ObservableCollection<string> _status = new ObservableCollection<string>();
        private WorkerManager _workerManager = WorkerManager.Instance;
        #endregion Private Fields

      
        #region Public Constructors

        public MainViewModel()
        {
            //SetStatusHandler = SetStatus;
            Base.MainModel = this;
           
            _logViewModel = new LogViewModel();
            _filterViewModel = new FilterViewModel();
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

        //   public event PropertyChangedEventHandler PropertyChanged;
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

        public ObservableCollection<string> Status
        {
            get
            {
                return _status;
            }
            //private set
            //{
            //        status = value;
            //        OnPropertyChanged("Status");
            //}
        }

        #endregion Public Properties

        #region Public Methods

        public void CopyExecuted(object contentList)
        {
            try
            {
                List<ListBoxItem> ContentList = (List<ListBoxItem>)contentList;

                HtmlFragment htmlFragment = new HtmlFragment();
                foreach (ListBoxItem lbi in ContentList)
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
                MainModel.SetStatus("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

        public void SetStatus(string statusData)
        {
        //    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
        //    {
                while (this._status.Count > 1000)
                {
                    this._status.RemoveAt(0);
                }

                this._status.Add(statusData);
                Debug.Print(statusData);
                OnPropertyChanged("Status");
        //    }));
        }

        #endregion Public Methods

        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}

        #region Internal Methods

        internal void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _settings.Save();
        }

        #endregion Internal Methods
    }
}