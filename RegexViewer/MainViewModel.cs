﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RegexViewer
{
    public class MainViewModel:Base,IMainViewModel
    {
        #region Private Fields

        private Command _copyCommand;
        private FilterViewModel _filterViewModel;
        private LogViewModel _logViewModel;
        private RegexViewerSettings _settings = RegexViewerSettings.Settings;
        private ObservableCollection<string> _status = new ObservableCollection<string>();
        private WorkerManager _workerManager = WorkerManager.Instance;
        private Int32 _statusIndex;
        #endregion Private Fields

      
        #region Public Constructors

        public MainViewModel()
        {
            //Base.MainModel = this;
            Base.NewStatus += HandleNewStatus;
            _filterViewModel = new FilterViewModel();
            _logViewModel = new LogViewModel(_filterViewModel);
        }

     private void HandleNewStatus(object sender, string status)
     {
         SetViewStatus(status);
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
                SetStatus("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

     
        public void SetViewStatus(string statusData)
        {
        //    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
        //    {
                while (this._status.Count > 1000)
                {
                    this._status.RemoveAt(0);
                }

                this._status.Add(statusData);
                this.StatusIndex = _status.Count;
                Debug.Print(statusData);
                OnPropertyChanged("Status");
        //    }));
        }

        public int StatusIndex
        {
            get
            {
                return _statusIndex;
            }
            set
            {
                if(_statusIndex != value)
                {
                    _statusIndex = value;
                    OnPropertyChanged("StatusIndex");
                }
            }
        }
        #endregion Public Methods

        
        


        #region Internal Methods

        internal void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            _filterViewModel.SaveModifiedFiles(sender);
            _settings.Save();
        }

        #endregion Internal Methods
    }
}