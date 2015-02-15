using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RegexViewer
{
    internal class Parser : Base
    {
        #region Private Fields

        private List<IFile<FilterFileItem>> _filterFiles = new List<IFile<FilterFileItem>>();
        private bool _filterMonitoringEnabled = false;
        private FilterViewModel _filterViewModel;
        private List<IFile<LogFileItem>> _logFiles = new List<IFile<LogFileItem>>();
        private bool _logMonitoringEnabled = false;
        private LogViewModel _logViewModel;
        //private List<BackgroundParser> _parsers = new List<BackgroundParser>();
        private WorkerManager _workerManager = WorkerManager.Instance;

        #endregion Private Fields

        #region Public Constructors

        public Parser(FilterViewModel filterViewModel, LogViewModel logViewModel)
        {
            SetStatus("Parser:ctor");
            // TODO: Complete member initialization
            this._filterViewModel = filterViewModel;
            this._logViewModel = logViewModel;
            _filterFiles = (List<IFile<FilterFileItem>>)_filterViewModel.ViewManager.FileManager;
            _filterViewModel.PropertyChanged += filterViewManager_PropertyChanged;

            _logFiles = (List<IFile<LogFileItem>>)_logViewModel.ViewManager.FileManager;
            _logViewModel.PropertyChanged += logViewManager_PropertyChanged;

            this.EnableFilterFileMonitoring(true);
          //  this.EnableLogFileMonitoring(true);
        }

        #endregion Public Constructors

        #region Public Methods

        public void EnableFilterFileMonitoring(bool enable)
        {
            if (enable & !_filterMonitoringEnabled)
            {
                foreach (IFile<FilterFileItem> item in _filterFiles)
                {
                    item.ContentItems.CollectionChanged += filterItems_CollectionChanged;
                }
                _filterMonitoringEnabled = !_filterMonitoringEnabled;
            }
            else if (!enable & _filterMonitoringEnabled)
            {
                foreach (IFile<FilterFileItem> item in _filterFiles)
                {
                    item.ContentItems.CollectionChanged -= filterItems_CollectionChanged;
                }
                _filterMonitoringEnabled = !_filterMonitoringEnabled;
            }
        }

        //public void EnableLogFileMonitoring(bool enable)
        //{
        //    if (enable & !_logMonitoringEnabled)
        //    {
        //        foreach (IFile<LogFileItem> item in _logFiles)
        //        {
        //            item.ContentItems.CollectionChanged += logItems_CollectionChanged;
        //        }
        //        _logMonitoringEnabled = !_logMonitoringEnabled;
        //    }
        //    else if (!enable & _logMonitoringEnabled)
        //    {
        //        foreach (IFile<LogFileItem> item in _logFiles)
        //        {
        //            item.ContentItems.CollectionChanged -= logItems_CollectionChanged;
        //        }
        //        _logMonitoringEnabled = !_logMonitoringEnabled;
        //    }
        //}

        #endregion Public Methods

        #region Private Methods

        private void filterItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("Parser:filterItemsCollectionChanged");
        }

        private void filterViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("Parser:filterViewPropertyChanged");
            // todo: determine what changed and run parser new filter, modified filter, removed filter
            // see if tab was added or removed

            List<IFile<FilterFileItem>> newFilterFiles = (List<IFile<FilterFileItem>>)_filterViewModel.ViewManager.FileManager;
            foreach (IFile<FilterFileItem> file in newFilterFiles)
            {
                if(!_filterFiles.Contains(file))
                {
                    // new filter file added
                    
                    _filterFiles.Add(file);
                    EnableFilterFileMonitoring(true);
                    // todo : re parse current log with new filter
                }
            }

            foreach (IFile<FilterFileItem> file in new List<IFile<FilterFileItem>>(_filterFiles))
            {
                if(!newFilterFiles.Contains(file))
                {
                    // existing filter file removed
                    EnableFilterFileMonitoring(false);
                    _filterFiles.Remove(file);
                    // todo : re parse current log with new selected filter
                    bool ret = ParseFile(_filterViewModel.CurrentFile(), _logViewModel.CurrentFile());
                }
            }
            
        }

        private bool ParseFile(IFile<FilterFileItem> filterFile, IFile<LogFileItem> logFile)
        {
            // http://stackoverflow.com/questions/1207832/wpf-dispatcher-begininvoke-and-ui-background-threads
            WorkerManager workerManager = WorkerManager.Instance;
            Worker worker = new Worker();

            workerManager.StartWorker(worker);
            
            return true;
        }

        //private void logItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    SetStatus("Parser:logItemsCollectionChanged");
        //    throw new NotSupportedException();
        //}

        private void logViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("Parser:logViewPropertyChanged");
            // todo: determine what changed and run parser new log or remove log
        }

        #endregion Private Methods
    }
}