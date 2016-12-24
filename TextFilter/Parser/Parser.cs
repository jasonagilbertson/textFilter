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

        private int _filteredLinesCount;

        private FilterFile _filterFilePrevious;

        private bool _filterMonitoringEnabled = false;

        private LogFile _logFilePrevious;

        // private bool _logMonitoringEnabled = false;

        private LogViewModel _logViewModel;

        private List<FilterFile> _previousFilterFiles = new List<FilterFile>();

        private List<LogFile> _previousLogFiles = new List<LogFile>();

        private int _totalLinesCount;

        //private List<BackgroundParser> _parsers = new List<BackgroundParser>();

        private WorkerManager _workerManager = WorkerManager.Instance;

        public Parser(FilterViewModel filterViewModel, LogViewModel logViewModel)
        {
            SetStatus("Parser:ctor");
            // TODO: Complete member initialization
            _FilterViewModel = filterViewModel;
            _logViewModel = logViewModel;
            Enable(true);

            // sync existing information filterviewmodel initialized before parser
            SyncFilterFiles();
            SyncLogFiles();
        }

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

        public void Enable(bool enable)
        {
            if (enable)
            {
                _FilterViewModel.PropertyChanged += filterViewManager_PropertyChanged;
                _logViewModel.PropertyChanged += logViewManager_PropertyChanged;

                EnableFilterFileMonitoring(true);
                // EnableLogFileMonitoring(true);
            }
            else
            {
                _FilterViewModel.PropertyChanged -= filterViewManager_PropertyChanged;
                _logViewModel.PropertyChanged -= logViewManager_PropertyChanged;

                EnableFilterFileMonitoring(false);
                // EnableLogFileMonitoring(false);
                _workerManager.CancelAllWorkers();
            }
        }

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

        private FilterFile CurrentFilterFile()
        {
            return (FilterFile)_FilterViewModel.CurrentFile();
        }

        private List<IFile<FilterFileItem>> CurrentFilterFiles()
        {
            return (List<IFile<FilterFileItem>>)_FilterViewModel.ViewManager.FileManager;
        }

        private LogFile CurrentLogFile()
        {
            return (LogFile)_logViewModel.CurrentFile();
        }

        private List<IFile<LogFileItem>> CurrentLogFiles()
        {
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
            List<IFile<FilterFileItem>> currentFilterFiles = CurrentFilterFiles();
            FilterFile currentFilterFile = CurrentFilterFile();
            //LogFile currentLog = (LogFile)_logViewModel.CurrentFile();

            if (currentFilterFiles == null) // || currentFilterFile == null)
            {
                // SyncFilterFiles();
                return new WorkerItem();
            }

            if (currentFilterFiles.Count > 0 && _previousFilterFiles.Count == currentFilterFiles.Count)
            {
                SetStatus("Parser:ModifiedFilterFile:same count");
                WorkerItem filterFileWorkerItem = new WorkerItem()
                {
                    WorkerModification = WorkerItem.Modification.Unknown,
                    FilterFile = currentFilterFile,
                    LogFile = (LogFile)_logViewModel.CurrentFile()
                };

                FilterNeed filterNeed = FilterNeed.Unknown;
                //if (_previousFilterFiles.Contains(currentFilterFile))
                if (_previousFilterFiles.Exists(x => x.Tag == currentFilterFile.Tag))
                {
                    SetStatus("Parser:ModifiedFilterFile:have previous version");
                    FilterFile previousVersionFilterFile = _previousFilterFiles.First(x => x.Tag == currentFilterFile.Tag);
                    filterNeed = _FilterViewModel.CompareFilterList(previousVersionFilterFile.ContentItems.ToList());
                }
                else
                {
                    filterNeed = FilterNeed.Filter;
                }

                if (currentFilterFile != _filterFilePrevious)
                {
                    SetStatus("Parser:ModifiedFilterFile:current filter file changed");
                    filterFileWorkerItem.WorkerModification = WorkerItem.Modification.FilterIndex;
                    filterNeed = FilterNeed.Filter;
                    _filterFilePrevious = currentFilterFile;
                }

                switch (filterNeed)
                {
                    case FilterNeed.Unknown:
                    case FilterNeed.ShowAll:
                    case FilterNeed.Filter:
                    case FilterNeed.ApplyColor:
                        filterFileWorkerItem.FilterNeed = filterNeed;
                        filterFileWorkerItem.WorkerModification = WorkerItem.Modification.FilterModified;
                        return filterFileWorkerItem;

                    case FilterNeed.Current:
                        SetStatus("Parser:ModifiedFilterFile:current");
                        filterFileWorkerItem.FilterNeed = FilterNeed.Current;
                        filterFileWorkerItem.WorkerModification = WorkerItem.Modification.Unknown;
                        return filterFileWorkerItem;

                    default:
                        break;
                }
            }
            else if (_previousFilterFiles.Count < currentFilterFiles.Count)
            {
                SetStatus("Parser:ModifiedFilterFile:filter file added");
                SyncFilterFiles();
                EnableFilterFileMonitoring(true);
                // todo : re parse current log with new filter
                return new WorkerItem()
                {
                    FilterFile = CurrentFilterFile(),
                    FilterNeed = FilterNeed.Filter,
                    WorkerModification = WorkerItem.Modification.FilterAdded
                };
            }
            else
            {
                SetStatus("Parser:ModifiedFilterFile:filter file removed");
                SyncFilterFiles();
                EnableFilterFileMonitoring(false);
                // todo : re parse current log with new filter
                return new WorkerItem()
                {
                    FilterFile = CurrentFilterFile(),
                    FilterNeed = FilterNeed.Filter,
                    WorkerModification = WorkerItem.Modification.FilterRemoved
                };
            }

            return new WorkerItem()
            {
                FilterNeed = FilterNeed.Unknown,
                WorkerModification = WorkerItem.Modification.Unknown
            };
        }

        private WorkerItem ModifiedLogFile()
        {
            SetStatus("Parser.ModifiedLogFile:enter");
            List<IFile<LogFileItem>> currentLogFiles = CurrentLogFiles();
            LogFile currentLogFile = CurrentLogFile();

            if (currentLogFiles == null) // || currentLogFile == null)
            {
                SetStatus("Parser.ModifiedLogFile:currentLog empty");
                // SyncLogFiles();
                return new WorkerItem();
            }

            if (_previousLogFiles.Count == currentLogFiles.Count)
            {
                WorkerItem workerItem = new WorkerItem()
                {
                    FilterFile = (FilterFile)_FilterViewModel.CurrentFile(),
                    LogFile = currentLogFile,
                    WorkerModification = WorkerItem.Modification.Unknown,
                    FilterNeed = FilterNeed.Current
                };

                if (currentLogFile != _logFilePrevious)
                {
                    workerItem.WorkerModification = WorkerItem.Modification.LogIndex;
                    workerItem.FilterNeed = FilterNeed.Filter;
                    _logFilePrevious = currentLogFile;
                }

                SetStatus("Parser.ModifiedLogFile:logfile count same");
                return workerItem;
            }
            else if (_previousLogFiles.Count < currentLogFiles.Count)
            {
                SetStatus("Parser.ModifiedLogFiles:logfile file added");
                SyncLogFiles();
                return new WorkerItem()
                {
                    LogFile = CurrentLogFile(),
                    FilterNeed = FilterNeed.Filter,
                    WorkerModification = WorkerItem.Modification.LogAdded
                };
            }
            else
            {
                SetStatus("Parser.ModifiedLogFiles:logfile file removed");
                SyncLogFiles();
                return new WorkerItem()
                {
                    LogFile = CurrentLogFile(),
                    FilterNeed = FilterNeed.Filter,
                    WorkerModification = WorkerItem.Modification.LogRemoved
                };
            }

            //return new WorkerItem()
            //{
            //    FilterNeed = FilterNeed.Unknown,
            //    WorkerModification = WorkerItem.Modification.Unknown
            //};
        }

        private void SyncFilterFiles()
        {
            foreach (FilterFile filterFile in CurrentFilterFiles())
            {
                if (!_previousFilterFiles.Exists(x => x == filterFile))
                {
                    SetStatus("SyncFilterFiles:Adding entry");
                    _previousFilterFiles.Add(filterFile);
                    _workerManager.AddWorkersByWorkerItemFilterFile(new WorkerItem() { FilterFile = filterFile });
                }
            }

            foreach (FilterFile filterFile in CurrentFilterFiles())
            {
                if (_previousFilterFiles.Exists(x => x.Tag == filterFile.Tag))
                {
                    FilterFile previousVersionFilterFile = _previousFilterFiles.First(x => x.Tag == filterFile.Tag);

                    // update list
                    _previousFilterFiles.Remove(previousVersionFilterFile);
                }

                SetStatus("SyncFilterFiles:saving entry for compare.");
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

                _previousFilterFiles.Add(file);
            }

            foreach (FilterFile filterFile in new List<FilterFile>(_previousFilterFiles))
            {
                if (!CurrentFilterFiles().Exists(x => x == filterFile))
                {
                    SetStatus("SyncFilterFiles:Removing entry");
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
                if (!CurrentLogFiles().Exists(x => x == logFile))
                {
                    SetStatus("SyncLogFiles:Removing entry");
                    _previousLogFiles.Remove(logFile);
                    _workerManager.RemoveWorkersByLogFile(logFile);
                }
            }
        }
    }
}