using System.Collections.Generic;
using System.Linq;

namespace RegexViewer
{
    internal class Parser : Base
    {
        #region Private Fields

        private int _filteredLinesCount;
        private List<IFile<FilterFileItem>> _filterFiles = new List<IFile<FilterFileItem>>();
        private bool _filterMonitoringEnabled = false;
        private FilterViewModel _filterViewModel;
        private List<IFile<LogFileItem>> _logFiles = new List<IFile<LogFileItem>>();

        // private bool _logMonitoringEnabled = false;
        private LogViewModel _logViewModel;

        private int _totalLinesCount;

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
            // _filterFiles = (List<IFile<FilterFileItem>>)_filterViewModel.ViewManager.FileManager;
            _filterViewModel.PropertyChanged += filterViewManager_PropertyChanged;

            // _logFiles = (List<IFile<LogFileItem>>)_logViewModel.ViewManager.FileManager;
            _logViewModel.PropertyChanged += logViewManager_PropertyChanged;

            this.EnableFilterFileMonitoring(true);
            // this.EnableLogFileMonitoring(true);
        }

        #endregion Public Constructors

        #region Public Properties

        public int FilteredLinesCount
        {
            get
            {
                return _filteredLinesCount;
            }
            set
            {
                if (_filteredLinesCount != value)
                {
                    _filteredLinesCount = value;
                    OnPropertyChanged("FilteredLinesCount");
                }
            }
        }

        public int TotalLinesCount
        {
            get
            {
                return _totalLinesCount;
            }
            set
            {
                if (_totalLinesCount != value)
                {
                    _totalLinesCount = value;
                    OnPropertyChanged("TotalLinesCount");
                }
            }
        }

        #endregion Public Properties

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

        #endregion Public Methods

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

        #region Private Methods

        private void filterItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("Parser:filterItemsCollectionChanged");
            Worker worker = ModifiedFilterFile();
            _workerManager.ProcessWorker(worker);
        }

        private void filterViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("Parser:filterViewPropertyChanged");
            // todo: determine what changed and run parser new filter, modified filter, removed filter
            // see if tab was added or removed

            Worker worker = ModifiedFilterFile();
            _workerManager.ProcessWorker(worker);
            //check worker

            // todo : re parse current log with new selected filter bool ret =
            // ParseFile(_filterViewModel.CurrentFile(), _logViewModel.CurrentFile());
        }

        private void logViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("Parser:logViewPropertyChanged");
            // todo: determine what changed and run parser new log or remove log

            Worker worker = ModifiedLogFile();
            _workerManager.ProcessWorker(worker);
        }

        private Worker ModifiedFilterFile()
        {
            List<IFile<FilterFileItem>> currentFilterFiles = (List<IFile<FilterFileItem>>)_filterViewModel.ViewManager.FileManager;
            FilterFile currentFilter = (FilterFile)_filterViewModel.CurrentFile();

            if (currentFilterFiles == null || currentFilter == null)
            {
                return new Worker();
            }

            if (currentFilterFiles.Count > 0 && _filterFiles.Count == currentFilterFiles.Count)
            {
                Worker filterFileWorkerItem = new Worker()
                {
                    WorkerModification = Worker.Modification.FilterIndex,
                    FilterFile = (FilterFile)_filterViewModel.CurrentFile()
                };

                // foreach(FilterFile file in _filterFiles) {
                // todo: arg not right...
                FilterNeed filterNeed = _filterViewModel.CompareFilterList(filterFileWorkerItem.FilterFile.ContentItems.ToList());
                switch (filterNeed)
                {
                    case FilterNeed.Unknown:
                    case FilterNeed.ShowAll:
                    case FilterNeed.Filter:
                    case FilterNeed.ApplyColor:
                        filterFileWorkerItem.FilterNeed = filterNeed;
                        filterFileWorkerItem.WorkerModification = Worker.Modification.FilterModified;
                        return filterFileWorkerItem;

                    case FilterNeed.Current:
                        break;

                    default:
                        break;
                }
                // }
            }
            else if (_filterFiles.Count < currentFilterFiles.Count)
            {
                foreach (FilterFile file in currentFilterFiles)
                {
                    if (!_filterFiles.Contains(file))
                    {
                        // new filter file added

                        _filterFiles.Add(file);
                        EnableFilterFileMonitoring(true);
                        // todo : re parse current log with new filter
                        return new Worker()
                        {
                            FilterFile = file,
                            FilterNeed = FilterNeed.Filter,
                            WorkerModification = Worker.Modification.FilterAdded
                        };
                    }
                }
            }
            else
            {
                foreach (FilterFile file in new List<IFile<FilterFileItem>>(_filterFiles))
                {
                    if (!currentFilterFiles.Contains(file))
                    {
                        // existing filter file removed
                        EnableFilterFileMonitoring(false);
                        _filterFiles.Remove(file);
                        return new Worker()
                        {
                            FilterFile = file,
                            FilterNeed = FilterNeed.Filter,
                            WorkerModification = Worker.Modification.FilterRemoved
                        };
                    }
                }
            }

            return new Worker()
                    {
                        FilterNeed = FilterNeed.Unknown,
                        WorkerModification = Worker.Modification.Unknown
                    };
        }

        private Worker ModifiedLogFile()
        {
            List<IFile<LogFileItem>> currentLogFiles = (List<IFile<LogFileItem>>)_logViewModel.ViewManager.FileManager;
            LogFile currentLog = (LogFile)_logViewModel.CurrentFile();

            if (currentLogFiles == null || currentLog == null)
            {
                return new Worker();
            }

            if (_logFiles.Count == currentLogFiles.Count)
            {
                return new Worker()
                {
                    LogFile = currentLog,
                    WorkerModification = Worker.Modification.LogIndex,
                    FilterNeed = FilterNeed.Current
                };
            }
            else if (_logFiles.Count < currentLogFiles.Count)
            {
                foreach (LogFile file in currentLogFiles)
                {
                    if (!_logFiles.Contains(file))
                    {
                        // new filter file added

                        _logFiles.Add(file);
                        // EnableFilterFileMonitoring(true); todo : re parse current log with new filter
                        return new Worker()
                        {
                            LogFile = file,
                            FilterNeed = FilterNeed.Filter,
                            WorkerModification = Worker.Modification.LogAdded
                        };
                    }
                }
            }
            else
            {
                foreach (LogFile file in new List<IFile<FilterFileItem>>(_filterFiles))
                {
                    if (!currentLogFiles.Contains(file))
                    {
                        // existing filter file removed EnableFilterFileMonitoring(false);
                        _logFiles.Remove(file);
                        return new Worker()
                        {
                            LogFile = file,
                            FilterNeed = FilterNeed.Filter,
                            WorkerModification = Worker.Modification.LogRemoved
                        };
                    }
                }
            }

            return new Worker()
            {
                FilterNeed = FilterNeed.Unknown,
                WorkerModification = Worker.Modification.Unknown
            };
        }

        private bool ParseFile(IFile<FilterFileItem> filterFile, IFile<LogFileItem> logFile)
        {
            // http: //stackoverflow.com/questions/1207832/wpf-dispatcher-begininvoke-and-ui-background-threads

            Worker worker = new Worker();

            _workerManager.StartWorker(worker);

            return true;
        }

        #endregion Private Methods

        //private void logItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    SetStatus("Parser:logItemsCollectionChanged");
        //    throw new NotSupportedException();
        //}
    }
}