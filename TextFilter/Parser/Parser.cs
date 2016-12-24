﻿// *********************************************************************** Assembly : textFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-31-2015 ***********************************************************************
// <copyright file="Parser.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace TextFilter
{
    public class Parser : Base
    {
        private int _filteredLinesCount;

        private FilterFile _filterFilePrevious;

        private bool _filterMonitoringEnabled = false;

        private bool _logMonitoringEnabled = false;

        private List<FilterFile> _previousFilterFiles = new List<FilterFile>();

        private int _totalLinesCount;

        private WorkerManager _workerManager = WorkerManager.Instance;

        public Parser()
        {
            SetStatus("Parser:ctor");
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
                _workerManager.EnableMonitor(true);
                _FilterViewModel.PropertyChanged += filterViewManager_PropertyChanged;
                _LogViewModel.PropertyChanged += logViewManager_PropertyChanged;

                EnableFilterFileMonitoring(true);
                EnableLogFileMonitoring(true);
            }
            else
            {
                _workerManager.EnableMonitor(false);
                _FilterViewModel.PropertyChanged -= filterViewManager_PropertyChanged;
                _LogViewModel.PropertyChanged -= logViewManager_PropertyChanged;

                EnableFilterFileMonitoring(false);
                EnableLogFileMonitoring(false);
                _workerManager.CancelAllWorkers();
            }
        }

        public void EnableFilterFileMonitoring(bool enable)
        {
            if (enable & !_filterMonitoringEnabled)
            {
                foreach (IFile<FilterFileItem> item in CurrentFilterFiles())
                {
                    item.ContentItems.CollectionChanged += filterItems_CollectionChanged;
                }

                _FilterViewModel.TabItems.CollectionChanged += filterItems_CollectionChanged;
                _filterMonitoringEnabled = !_filterMonitoringEnabled;
            }
            else if (!enable & _filterMonitoringEnabled)
            {
                foreach (IFile<FilterFileItem> item in _previousFilterFiles)
                {
                    item.ContentItems.CollectionChanged -= filterItems_CollectionChanged;
                }

                _FilterViewModel.TabItems.CollectionChanged -= filterItems_CollectionChanged;
                _filterMonitoringEnabled = !_filterMonitoringEnabled;
            }
        }

        public void EnableLogFileMonitoring(bool enable)
        {
            if (enable & !_logMonitoringEnabled)
            {
                foreach (IFile<LogFileItem> item in CurrentLogFiles())
                {
                    item.ContentItems.CollectionChanged += logItems_CollectionChanged;
                }

                _LogViewModel.TabItems.CollectionChanged += logItems_CollectionChanged;
                _logMonitoringEnabled = !_logMonitoringEnabled;
            }
            else if (!enable & _logMonitoringEnabled)
            {
                foreach (IFile<LogFileItem> item in CurrentLogFiles())
                {
                    item.ContentItems.CollectionChanged -= logItems_CollectionChanged;
                }

                _LogViewModel.TabItems.CollectionChanged -= logItems_CollectionChanged;
                _logMonitoringEnabled = !_logMonitoringEnabled;
            }
        }

        public void filterItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("Parser:filterItemsCollectionChanged");
            ModifiedFilterFile(e);
        }

        public void filterViewManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetStatus("Parser:filterViewPropertyChanged:" + e.PropertyName);
            // todo: determine what changed and run parser new filter, modified filter, removed filter
            // see if tab was added or removed

            if (sender is FilterFileItem)
            {
            }

            ModifiedFilterFile(e);

            //check worker

            // todo : re parse current log with new selected filter bool ret =
            // ParseFile(_FilterViewModel.CurrentFile(), _LogViewModel.CurrentFile());
        }

        public void logItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetStatus("Parser:logItemsCollectionChanged");
            ModifiedLogFile(e);
        }

        public void logViewManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetStatus("Parser:logViewPropertyChanged:" + e.PropertyName);
            // todo: determine what changed and run parser new log or remove log

            ModifiedLogFile(e);
        }

        private FilterFile CurrentFilterFile()
        {
            if (_FilterViewModel != null)
            {
                SetStatus("Parser:CurrentFilterFile:exit: index: " + _FilterViewModel.SelectedIndex);
                return (FilterFile)_FilterViewModel.CurrentFile();
            }
            else
            {
                return null;
            }
        }

        private List<IFile<FilterFileItem>> CurrentFilterFiles()
        {
            if (_FilterViewModel.ViewManager != null)
            {
                SetStatus("Parser:CurrentFilterFiles:exit: count: " + _FilterViewModel.ViewManager.FileManager.Count);
                return (List<IFile<FilterFileItem>>)_FilterViewModel.ViewManager.FileManager;
            }
            else
            {
                SetStatus("Parser:CurrentFilterFiles:null ");
                return new List<IFile<FilterFileItem>>();
            }
        }

        //public void ReadFile()
        //{
        //    SetStatus("Parser:ReadFile");
        //    // todo: determine what changed and run parser new log or remove log
        private LogFile CurrentLogFile()
        {
            if (_LogViewModel != null)
            {
                SetStatus("Parser:CurrentLogFile:exit: index: " + _LogViewModel.SelectedIndex);
                return (LogFile)_LogViewModel.CurrentFile();
            }
            else
            {
                return null;
            }
        }

        private List<IFile<LogFileItem>> CurrentLogFiles()
        {
            if (_LogViewModel.ViewManager != null)
            {
                SetStatus("Parser:CurrentLogFiles:exit: count: " + _LogViewModel.ViewManager.FileManager.Count);
                return (List<IFile<LogFileItem>>)_LogViewModel.ViewManager.FileManager;
            }
            else
            {
                SetStatus("Parser:CurrentLogFiles:null ");
                return new List<IFile<LogFileItem>>();
            }
        }

        private void ModifiedFilterFile(object e)
        {
            SetStatus("Parser.ModifiedFilterFile:enter");
            WorkerItem workerItem = new WorkerItem();
            if (e == null)
            {
                throw new System.Exception("Parser.ModifiedFilterFile:enter:error argument null");
            }
            else if (e is NotifyCollectionChangedEventArgs)
            {
                NotifyCollectionChangedEventArgs col = (e as NotifyCollectionChangedEventArgs);
                switch (col.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            SetStatus("Parser.ModifiedFilterFile:filter file added");
                            workerItem = new WorkerItem()
                            {
                                FilterFile = (FilterFile)((FilterTabViewModel)col.NewItems[0]).File,
                                WorkerModification = WorkerItem.Modification.FilterAdded,
                                WorkerState = WorkerItem.State.NotStarted,
                                FilterNeed = FilterNeed.Filter
                            };
                            _workerManager.AddWorkersByWorkerItemFilterFile(workerItem);
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            SetStatus("Parser.ModifiedFilterFile:filter file removed");
                            _workerManager.RemoveWorkersByFilterFile((FilterFile)((FilterTabViewModel)col.OldItems[0]).File);
                        }
                        break;

                    default:
                        {
                            SetStatus("Parser.ModifiedFilterFile:error, unknown collection action: " + Enum.GetName(typeof(NotifyCollectionChangedAction), col.Action));
                            return;
                        }
                }
            }
            else if (e is PropertyChangedEventArgs)
            {
                SetStatus("Parser.ModifiedFilterFile:property changed event: " + (e as PropertyChangedEventArgs).PropertyName);

                if ((e as PropertyChangedEventArgs).PropertyName == BaseTabViewModelEvents.SelectedIndex)
                {
                    if ((_workerManager.GetWorkers(CurrentFilterFile(), CurrentLogFile()).Count > 0))
                    {
                        workerItem = _workerManager.GetWorkers(CurrentFilterFile(), CurrentLogFile()).First();
                        workerItem.WorkerModification = WorkerItem.Modification.FilterIndex;
                        _workerManager.ProcessWorker(workerItem);
                        return;
                    }
                }

                workerItem = new WorkerItem()
                {
                    FilterFile = CurrentFilterFile(),
                    WorkerModification = WorkerItem.Modification.Unknown,
                    FilterNeed = FilterNeed.Unknown,
                    LogFile = _LogViewModel == null ? null : CurrentLogFile()
                };

                if (CurrentFilterFile() != null && _previousFilterFiles.Exists(x => x.FileName != null && x.FileName == CurrentFilterFile().FileName))
                {
                    SetStatus("Parser:ModifiedFilterFile:have previous version");
                    FilterFile previousVersionFilterFile = _previousFilterFiles.First(x => x.FileName != null && x.FileName == CurrentFilterFile().FileName);
                    workerItem.FilterNeed = _FilterViewModel.CompareFilterList(previousVersionFilterFile.ContentItems.ToList());
                }
                else
                {
                    workerItem.FilterNeed = FilterNeed.Filter;
                }

                if (CurrentFilterFile() != _filterFilePrevious)
                {
                    SetStatus("Parser:ModifiedFilterFile:current filter file changed");
                    workerItem.WorkerModification = WorkerItem.Modification.FilterIndex;
                    workerItem.FilterNeed = FilterNeed.Current;
                    _filterFilePrevious = CurrentFilterFile();
                }

                switch (workerItem.FilterNeed)
                {
                    case FilterNeed.Unknown:
                    case FilterNeed.ShowAll:
                    case FilterNeed.Filter:
                    case FilterNeed.ApplyColor:
                        workerItem.WorkerModification = WorkerItem.Modification.FilterModified;
                        _workerManager.ProcessWorker(workerItem);
                        break;

                    case FilterNeed.Current:
                        SetStatus("Parser:ModifiedFilterFile:current");
                        _workerManager.ProcessWorker(workerItem);
                        break;

                    default:
                        break;
                }
            }
            return;
        }

        private void ModifiedLogFile(object e)
        {
            SetStatus("Parser.ModifiedLogFile:enter");
            WorkerItem workerItem = new WorkerItem();
            if (e == null)
            {
                throw new System.Exception("Parser.ModifiedLogFile:enter:error argument null");
            }
            else if (e is NotifyCollectionChangedEventArgs)
            {
                NotifyCollectionChangedEventArgs col = (e as NotifyCollectionChangedEventArgs);
                switch (col.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            SetStatus("Parser.ModifiedLogFile:new log file added");
                            workerItem = new WorkerItem()
                            {
                                LogFile = (LogFile)((LogTabViewModel)col.NewItems[0]).File,
                                WorkerModification = WorkerItem.Modification.LogAdded,
                                WorkerState = WorkerItem.State.NotStarted,
                                FilterNeed = FilterNeed.Filter
                            };

                            _workerManager.AddWorkersByWorkerItemLogFile(workerItem);
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            SetStatus("Parser.ModifiedLogFile:log file being removed");
                            _workerManager.RemoveWorkersByLogFile((LogFile)((LogTabViewModel)col.OldItems[0]).File);
                        }
                        break;

                    default:
                        {
                            SetStatus("Parser.ModifiedLogFile:error, unknown collection action: " + Enum.GetName(typeof(NotifyCollectionChangedAction), col.Action));
                            return;
                        }
                }
            }
            else if (e is PropertyChangedEventArgs)
            {
                SetStatus("Parser.ModifiedLogFile:returning, property changed event: " + (e as PropertyChangedEventArgs).PropertyName);

                if ((e as PropertyChangedEventArgs).PropertyName == BaseTabViewModelEvents.SelectedIndex)
                {
                    if ((_workerManager.GetWorkers(CurrentFilterFile(), CurrentLogFile()).Count > 0))
                    {
                        workerItem = _workerManager.GetWorkers(CurrentFilterFile(), CurrentLogFile()).First();

                        workerItem.WorkerModification = WorkerItem.Modification.LogIndex;
                        _workerManager.ProcessWorker(workerItem);
                        return;
                    }
                }
            }
        }

        private void SyncFilterFiles()
        {
            SetStatus("Parser:SyncFilterFiles:enter");
            // make sure all tabs have data
            foreach (FilterFile filterFile in CurrentFilterFiles())
            {
                // if(filterFile.ContentItems.Count == 0)
                {
                    SetStatus("Parser:SyncLogFiles:adding worker");
                    _workerManager.AddWorkersByWorkerItemFilterFile(new WorkerItem() { FilterFile = filterFile });
                }
            }
        }

        private void SyncLogFiles()
        {
            SetStatus("Parser:SyncLogFiles:enter");
            // make sure all tabs have data
            foreach (LogFile logFile in CurrentLogFiles())
            {
                //if(logFile.ContentItems.Count == 0)
                {
                    SetStatus("Parser:SyncLogFiles:adding worker");
                    _workerManager.AddWorkersByWorkerItemLogFile(new WorkerItem() { LogFile = logFile });
                }
            }
        }
    }
}