// *********************************************************************** Assembly : textFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-31-2015 ***********************************************************************
// <copyright file="Parser.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Linq;

namespace TextFilter
{
    public class Parser : Base
    {
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

        #region Private Fields

        private int _filteredLinesCount;

        private FilterFile _filterFilePrevious;

        private bool _filterMonitoringEnabled = false;

        private FilterViewModel _filterViewModel;

        private LogFile _logFilePrevious;

        // private bool _logMonitoringEnabled = false;

        private LogViewModel _logViewModel;

        private List<FilterFile> _previousFilterFiles = new List<FilterFile>();

        private List<LogFile> _previousLogFiles = new List<LogFile>();

        private int _totalLinesCount;

        //private List<BackgroundParser> _parsers = new List<BackgroundParser>();

        private WorkerManager _workerManager = WorkerManager.Instance;

        #endregion Private Fields

        #region Public Constructors

        public Parser(FilterViewModel filterViewModel, LogViewModel logViewModel)
        {
            SetStatus("Parser:ctor");
            // TODO: Complete member initialization
            _filterViewModel = filterViewModel;
            _logViewModel = logViewModel;
            Enable(true);

            // sync existing information filterviewmodel initialized before parser
            SyncFilterFiles();
            SyncLogFiles();
        }

        public void Enable(bool enable)
        {
            if (enable)
            {
                _filterViewModel.PropertyChanged += filterViewManager_PropertyChanged;
                _logViewModel.PropertyChanged += logViewManager_PropertyChanged;

                EnableFilterFileMonitoring(true);
                // EnableLogFileMonitoring(true);
            }
            else
            {
                _filterViewModel.PropertyChanged -= filterViewManager_PropertyChanged;
                _logViewModel.PropertyChanged -= logViewManager_PropertyChanged;

                EnableFilterFileMonitoring(false);
                // EnableLogFileMonitoring(false);
                _workerManager.CancelAllWorkers();
            }
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
                foreach (IFile<FilterFileItem> item in _previousFilterFiles)
                {
                    item.ContentItems.CollectionChanged += filterItems_CollectionChanged;
                }
                _filterMonitoringEnabled = !_filterMonitoringEnabled;
            }
            else if (!enable & _filterMonitoringEnabled)
            {
                foreach (IFile<FilterFileItem> item in _previousFilterFiles)
                {
                    item.ContentItems.CollectionChanged -= filterItems_CollectionChanged;
                }
                _filterMonitoringEnabled = !_filterMonitoringEnabled;
            }
        }

        public void ReadFile()
        {
            SetStatus("Parser:ReadFile");
            // todo: determine what changed and run parser new log or remove log

            WorkerItem worker = ModifiedLogFile();
            _workerManager.ProcessWorker(worker);
        }

        #endregion Public Methods

        #region Private Methods

        private FilterFile CurrentFilterFile()
        {
            SetStatus("Parser:CurrentFilterFile:exit: index: " + _filterViewModel.SelectedIndex);
            return (FilterFile)_filterViewModel.CurrentFile();
        }

        private List<IFile<FilterFileItem>> CurrentFilterFiles()
        {
            SetStatus("Parser:CurrentFilterFiles:exit: count: " + _filterViewModel.ViewManager.FileManager.Count);
            return (List<IFile<FilterFileItem>>)_filterViewModel.ViewManager.FileManager;

        }

        private LogFile CurrentLogFile()
        {
            SetStatus("Parser:CurrentLogFile:exit: index: " + _logViewModel.SelectedIndex);
            return (LogFile)_logViewModel.CurrentFile();
        }

        private List<IFile<LogFileItem>> CurrentLogFiles()
        {
            SetStatus("Parser:CurrentLogFiles:exit: count: " + _logViewModel.ViewManager.FileManager.Count);
            return (List<IFile<LogFileItem>>)_logViewModel.ViewManager.FileManager;
        }

        private void filterItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("Parser:filterItemsCollectionChanged");
            WorkerItem worker = ModifiedFilterFile();
            _workerManager.ProcessWorker(worker);
        }

        private void filterViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("Parser:filterViewPropertyChanged:" + e.PropertyName);
            // todo: determine what changed and run parser new filter, modified filter, removed filter
            // see if tab was added or removed

            WorkerItem worker = ModifiedFilterFile();
            _workerManager.ProcessWorker(worker);
            //check worker

            // todo : re parse current log with new selected filter bool ret =
            // ParseFile(_filterViewModel.CurrentFile(), _logViewModel.CurrentFile());
        }

        private void logViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("Parser:logViewPropertyChanged:" + e.PropertyName);
            // todo: determine what changed and run parser new log or remove log

            WorkerItem worker = ModifiedLogFile();
            _workerManager.ProcessWorker(worker);
        }

        private WorkerItem ModifiedFilterFile()
        {
            SetStatus("Parser:ModifiedFilterFile:enter");
                        
            //LogFile currentLog = (LogFile)_logViewModel.CurrentFile();

            if (CurrentFilterFiles() == null) // || currentFilterFile == null)
            {
                // SyncFilterFiles();
                return new WorkerItem();
            }


            WorkerItem workerItem = new WorkerItem()
            {
                FilterFile = CurrentFilterFile(),
                WorkerModification = WorkerItem.Modification.Unknown,
                FilterNeed = FilterNeed.Unknown
            };


            if (CurrentFilterFiles().Count > 0 && _previousFilterFiles.Count == CurrentFilterFiles().Count)
            {
                SetStatus("Parser:ModifiedFilterFile:same count");
                workerItem.LogFile = CurrentLogFile();

                // get current item based on filter file and log file
                if (_workerManager.GetWorkers(workerItem).Count == 1)
                {
                    workerItem = _workerManager.GetWorkers(workerItem).FirstOrDefault();
                }

                workerItem.WorkerModification = WorkerItem.Modification.Unknown;
                workerItem.FilterNeed = FilterNeed.Unknown;

                if (CurrentFilterFile() != null && _previousFilterFiles.Exists(x => x.FileName != null && x.FileName == CurrentFilterFile().FileName))
                {
                    SetStatus("Parser:ModifiedFilterFile:have previous version");
                    FilterFile previousVersionFilterFile = _previousFilterFiles.First(x => x.FileName != null && x.FileName == CurrentFilterFile().FileName);
                    workerItem.FilterNeed = _filterViewModel.CompareFilterList(previousVersionFilterFile.ContentItems.ToList());
                }
                else
                {
                    workerItem.FilterNeed = FilterNeed.Filter;
                }

                if (CurrentFilterFile() != _filterFilePrevious)
                {
                    SetStatus("Parser:ModifiedFilterFile:current filter file changed");
                    workerItem.WorkerModification = WorkerItem.Modification.FilterIndex;
                    workerItem.FilterNeed = FilterNeed.Filter;
                    _filterFilePrevious = CurrentFilterFile();
                }

                switch (workerItem.FilterNeed)
                {
                    case FilterNeed.Unknown:
                    case FilterNeed.ShowAll:
                    case FilterNeed.Filter:
                    case FilterNeed.ApplyColor:
                        workerItem.WorkerModification = WorkerItem.Modification.FilterModified;
                        return workerItem;

                    case FilterNeed.Current:
                        SetStatus("Parser:ModifiedFilterFile:current");
                        workerItem.FilterNeed = FilterNeed.Current;
                        workerItem.WorkerModification = WorkerItem.Modification.Unknown;
                        return workerItem;

                    default:
                        break;
                }
            }
            else if (_previousFilterFiles.Count < CurrentFilterFiles().Count)
            {
                SetStatus("Parser:ModifiedFilterFile:filter file added");
                SyncFilterFiles();
                EnableFilterFileMonitoring(true);

                workerItem.FilterNeed = FilterNeed.Filter;
                workerItem.WorkerModification = WorkerItem.Modification.FilterAdded;
            }
            else
            {
                SetStatus("Parser:ModifiedFilterFile:filter file removed");
                SyncFilterFiles();
                EnableFilterFileMonitoring(false);
                workerItem.FilterNeed = FilterNeed.Filter;
                workerItem.WorkerModification = WorkerItem.Modification.FilterRemoved;
            }

            return workerItem;
        }

        private WorkerItem ModifiedLogFile()
        {
            SetStatus("Parser.ModifiedLogFile:enter");
            
            if (CurrentLogFiles() == null) // || currentLogFile == null)
            {
                SetStatus("Parser.ModifiedLogFile:currentLog empty");
                // SyncLogFiles();
                return new WorkerItem();
            }

            
                WorkerItem workerItem = new WorkerItem()
                {
                    LogFile = CurrentLogFile(),
                    WorkerModification = WorkerItem.Modification.Unknown,
                    FilterNeed = FilterNeed.Current
                };
            

            if (_previousLogFiles.Count == CurrentLogFiles().Count)
            {
                workerItem.FilterFile = CurrentFilterFile();
                if(_workerManager.GetWorkers(workerItem).Count == 1)
                {
                    workerItem = _workerManager.GetWorkers(workerItem).FirstOrDefault();
                }

                if (CurrentLogFile() != _logFilePrevious)
                {
                    SetStatus("Parser.ModifiedLogFile:logfile index changed");
                    workerItem.WorkerModification = WorkerItem.Modification.LogIndex;
                    workerItem.FilterNeed = FilterNeed.Filter;
                    _logFilePrevious = CurrentLogFile();
                }

                SetStatus("Parser.ModifiedLogFile:logfile count same");
                return workerItem;
            }
            else if (_previousLogFiles.Count < CurrentLogFiles().Count)
            {
                SetStatus("Parser.ModifiedLogFiles:logfile file added");
                SyncLogFiles();

                workerItem.FilterNeed = FilterNeed.Filter;
                workerItem.WorkerModification = WorkerItem.Modification.LogAdded;
            }
            else
            {
                SetStatus("Parser.ModifiedLogFiles:logfile file removed");
                SyncLogFiles();
                workerItem.FilterNeed = FilterNeed.Filter;
                workerItem.WorkerModification = WorkerItem.Modification.LogRemoved;

            }

            return workerItem;
        }

        private void SyncFilterFiles()
        {
            foreach (FilterFile filterFile in CurrentFilterFiles())
            {
                if (!_previousFilterFiles.Exists(x => x == filterFile))
                {
                    SetStatus("Parser:SyncFilterFiles:Adding entry: " + filterFile.Tag);
                    _previousFilterFiles.Add(filterFile);
                    _workerManager.AddWorkersByWorkerItemFilterFile(new WorkerItem() { FilterFile = filterFile });
                }
            }

            foreach (FilterFile filterFile in CurrentFilterFiles())
            {
                if (_previousFilterFiles.Exists(x => x.Tag == filterFile.Tag))
                {
                    FilterFile previousVersionFilterFile = _previousFilterFiles.First(x => x.Tag == filterFile.Tag);
                    SetStatus("Parser:SyncFilterFiles:removing cache entry: " + previousVersionFilterFile.Tag);
                    // update list
                    _previousFilterFiles.Remove(previousVersionFilterFile);
                }

                SetStatus("Parser:SyncFilterFiles:saving entry for compare.");
                FilterFile file = new FilterFile()
                {
                    ContentItems = filterFile.ContentItems,
                    FileName = filterFile.FileName,
                    FilterNotes = filterFile.FilterNotes,
                    FilterVersion = filterFile.FilterVersion,
                    IsNew = filterFile.IsNew,
                    IsReadOnly = filterFile.IsReadOnly,
                    Modified = filterFile.Modified,
                    Tag = filterFile.Tag
                };

                SetStatus("Parser:SyncFilterFiles:adding cache entry: " + file.Tag);
                _previousFilterFiles.Add(file);
            }

            foreach (FilterFile filterFile in new List<FilterFile>(_previousFilterFiles))
            {
                if (!CurrentFilterFiles().Exists(x => x.Tag == filterFile.Tag))
                {
                    SetStatus("Parser:SyncFilterFiles:Removing entry: " + filterFile.Tag);
                    _previousFilterFiles.Remove(filterFile);
                    _workerManager.RemoveWorkersByFilterFile(filterFile);
                }
            }
        }

        private void SyncLogFiles()
        {
            foreach (LogFile logFile in CurrentLogFiles())
            {
                if (!_previousLogFiles.Exists(x => x == logFile))
                {
                    SetStatus("SyncLogFiles:Adding entry");
                    _previousLogFiles.Add(logFile);
                    _workerManager.AddWorkersByWorkerItemLogFile(new WorkerItem() { LogFile = logFile });
                }
            }

            foreach (LogFile logFile in new List<LogFile>(_previousLogFiles))
            {
                if (!CurrentLogFiles().Exists(x => x.Tag == logFile.Tag))
                {
                    SetStatus("SyncLogFiles:Removing entry");
                    _previousLogFiles.Remove(logFile);
                    _workerManager.RemoveWorkersByLogFile(logFile);
                }
            }
        }

        #endregion Private Methods
    }
}