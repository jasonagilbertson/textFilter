// ************************************************************************************
// Assembly: TextFilter
// File: WorkerManager.cs
// Created: 9/6/2016
// Modified: 3/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace TextFilter
{
    public class WorkerManager : WorkerFunctions
    {
        public ReaderWriterLockSlim ListLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private List<FilterFileItem> _previousFilterItems = new List<FilterFileItem>();
        private static WorkerManager _workerManager;

        private JobMonitor _monitor;
        private WorkerFunctions _workerFunctions = new WorkerFunctions();

        private WorkerManager()
        {
            BGWorkers = new List<WorkerItem>();
        }

        public static WorkerManager Instance
        {
            get
            {
                if (_workerManager == null)
                {
                    _workerManager = new WorkerManager();
                }
                return _workerManager;
            }
        }

        public List<WorkerItem> BGWorkers { get; set; }

        public void AddWorkersByWorkerItemFilterFile(WorkerItem workerItem)
        {
            CancelAllWorkers();
            WorkerItem baseItem = new WorkerItem()
            {
                FilterNeed = FilterNeed.Filter,
                WorkerModification = WorkerItem.Modification.FilterAdded,
                WorkerState = WorkerItem.State.NotStarted,
                LogFile = null,
                FilterFile = workerItem.FilterFile
            };

            if (GetBaseWorker(baseItem) == null)
            {
                SetStatus("AddWorkersByWorkerItemFilterFile:adding base worker");
                AddWorker(baseItem);
            }

            List<LogFile> logFiles = GetLogFiles();

            if (logFiles.Count > 0)
            {
                foreach (LogFile logFile in logFiles)
                {
                    if (logFile == null)
                    {
                        continue;
                    }
                    if (GetWorkers(workerItem.FilterFile, logFile).Count == 0)
                    {
                        WorkerItem bgItem = new WorkerItem()
                        {
                            FilterFile = workerItem.FilterFile,
                            LogFile = logFile,
                            FilterNeed = workerItem.FilterNeed,
                            WorkerModification = WorkerItem.Modification.FilterModified,
                            WorkerState = WorkerItem.State.Ready
                        };

                        AddWorker(bgItem);
                        SetStatus(string.Format("AddWorkersByWorkerItemFilterFile:Added worker:{0} {1} total workers:{2}",
                           workerItem.LogFile != null ? workerItem.LogFile.Tag : string.Empty,
                           workerItem.FilterFile.Tag,
                           _workerManager.BGWorkers.Count()));
                    }
                }
            }
        }

        public void AddWorkersByWorkerItemLogFile(WorkerItem workerItem)
        {
            CancelAllWorkers();
            WorkerItem baseItem = new WorkerItem()
            {
                FilterNeed = FilterNeed.ShowAll,
                WorkerModification = WorkerItem.Modification.LogAdded,
                WorkerState = WorkerItem.State.NotStarted,
                FilterFile = null,
                LogFile = workerItem.LogFile
            };

            if (GetBaseWorker(baseItem) == null)
            {
                SetStatus("AddWorkersByWorkerItemLogFile:adding base worker");
                AddWorker(baseItem);
            }

            List<FilterFile> filterFiles = GetFilterFiles();
            if (filterFiles.Count > 0)
            {
                foreach (FilterFile filterFile in GetWorkers().Select(x => x.FilterFile).Distinct())
                {
                    if (filterFile == null)
                    {
                        continue;
                    }
                    if (GetWorkers(filterFile, workerItem.LogFile).Count == 0)
                    {
                        WorkerItem bgItem = new WorkerItem()
                        {
                            FilterFile = filterFile,
                            LogFile = workerItem.LogFile,
                            FilterNeed = workerItem.FilterNeed,
                            WorkerModification = WorkerItem.Modification.FilterModified,
                            WorkerState = WorkerItem.State.Ready
                        };

                        AddWorker(bgItem);
                        SetStatus(string.Format("AddWorkersByWorkerItemLogFile:Added worker:{0} {1} total workers:{2}",
                           workerItem.LogFile.Tag,
                           workerItem.FilterFile != null ? workerItem.FilterFile.Tag : string.Empty,
                           _workerManager.BGWorkers.Count()));
                    }
                }
            }
        }

        public void CancelAllWorkers()
        {
            // Cancel the asynchronous operation.
            foreach (WorkerItem bgWorker in GetWorkers())
            {
                CancelWorker(bgWorker);
            }
        }

        public void CancelWorker(WorkerItem workerItem)
        {
            try
            {
                ListLock.EnterWriteLock();
                if (workerItem.BackGroundWorker != null && workerItem.BackGroundWorker.IsBusy && !workerItem.BackGroundWorker.CancellationPending)
                {
                    SetStatus(string.Format("CancelWorker:cancelling worker: {0} {1} {2}", workerItem.GetHashCode(),
                        workerItem.LogFile == null ? "" : workerItem.LogFile.FileName,
                        workerItem.FilterFile == null ? "" : workerItem.FilterFile.FileName));
                    Application.Current.Dispatcher.InvokeAsync((Action)delegate ()
                    {
                        workerItem.BackGroundWorker.CancelAsync();
                    });
                    //while (workerItem.BackGroundWorker.IsBusy)
                    //{
                    //    Thread.Sleep(10);
                    //}

                    workerItem.WorkerState = WorkerItem.State.Aborted;
                }
            }
            catch (Exception e)
            {
                SetStatus("CancelWorker:exception: " + e.ToString());
            }
            finally
            {
                ListLock.ExitWriteLock();
            }
        }

        public void EnableMonitor(bool enable)
        {
            if (enable)
            {
                _monitor = new JobMonitor(this);
            }
            else
            {
                _monitor.Abort();
            }
        }

        public WorkerItem GetBaseWorker(WorkerItem workerItem)
        {
            return GetWorkers(workerItem.FilterFile, workerItem.LogFile, true).FirstOrDefault();
        }

        public List<FilterFile> GetFilterFiles()
        {
            return (List<FilterFile>)_workerManager.BGWorkers
                .Select(y => y.FilterFile)
                .GroupBy(x => x)
                .Select(g => g.First()).ToList();
        }

        public List<LogFile> GetLogFiles()
        {
            return (List<LogFile>)_workerManager.BGWorkers
                .Select(x => x.LogFile)
                .GroupBy(y => y)
                .Select(g => g.First()).ToList();
        }

        public List<WorkerItem> GetWorkers(WorkerItem workerItem)
        {
            return GetWorkers(workerItem.FilterFile, workerItem.LogFile);
        }

        public List<WorkerItem> GetWorkers(FilterFile filterFile = null, LogFile logFile = null, bool baseFile = false)
        {
            List<WorkerItem> workerItems = GetWorkersInternal(filterFile, logFile, baseFile);
#if DEBUG
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("getworkers returned:");

            foreach (WorkerItem workerItem in workerItems)
            {
                sb.AppendLine(string.Format("\tworker: {0} modification: {1} state: {2} logfile: {3} filterfile: {4}", 
                    workerItem.GetHashCode(), 
                    workerItem.WorkerModification, 
                    workerItem.WorkerState, 
                    workerItem.LogFile, 
                    workerItem.FilterFile));
            }

            SetStatus(sb.ToString());
#endif
            return workerItems;
        }
        private List<WorkerItem> GetWorkersInternal(FilterFile filterFile = null, LogFile logFile = null, bool baseFile = false)
        {
#if DEBUG
            SetStatus(string.Format("GetWorkers:enter:{0}, {1}",
                filterFile == null ? "null" : filterFile.Tag,
                logFile == null ? "null" : logFile.Tag));
#endif
            List<WorkerItem> workerItems = new List<WorkerItem>();
            while(!ListLock.TryEnterReadLock(10))
            {
                //Thread.Sleep(100);
                Thread.Yield();
            }
                        
            try
            {
                //foreach(WorkerItem item in _workerManager.BGWorkers)
                //{
                //    SetStatus(string.Format("GetWorkers:BGWorker:state: {0}", Enum.GetName(typeof(WorkerItem.State),item.WorkerState)));
                //    if (item.FilterFile != null)
                //    {
                //        SetStatus(string.Format("\tGetWorkers:BGWorker:filter file : {0}", item.FilterFile.FileName));
                //    }
                //    if (item.LogFile != null)
                //    {
                //        SetStatus(string.Format("\tGetWorkers:BGWorker:log file : {0}", item.LogFile.FileName));
                //    }
                //}

                if (filterFile != null && logFile != null)
                {
#if DEBUG
                    SetStatus(string.Format("Getworkers:searching by filter file and log object"));
#endif
                    if (_workerManager.BGWorkers.Exists(x => x.FilterFile != null && x.LogFile != null && x.FilterFile == filterFile && x.LogFile == logFile))
                    {
                        workerItems = _workerManager.BGWorkers.Where(x => x.FilterFile != null && x.LogFile != null && x.FilterFile == filterFile && x.LogFile == logFile).ToList();
                    }

                    if (workerItems.Count > 1)
                    {
                        SetStatus("Error:Getworkers:duplicate workers");
                    }

                    if (workerItems.Count >= 1)
                    {
#if DEBUG
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
#endif
                        return workerItems;
                    }
                }

                if (filterFile != null && !string.IsNullOrEmpty(filterFile.Tag) && logFile != null && !string.IsNullOrEmpty(logFile.Tag))
                {
#if DEBUG
                    SetStatus(string.Format("Getworkers:searching by filter file tag: {0} and log file tag: {1}", filterFile.Tag, logFile.Tag));
#endif
                    if (_workerManager.BGWorkers.Exists(x => x.FilterFile != null && x.LogFile != null && x.FilterFile.Tag == filterFile.Tag && x.LogFile.Tag == logFile.Tag))
                    {
                        workerItems = _workerManager.BGWorkers.Where(x => x.FilterFile != null && x.LogFile != null && x.FilterFile.Tag == filterFile.Tag && x.LogFile.Tag == logFile.Tag).ToList();
                    }

                    if (workerItems.Count > 1)
                    {
                        SetStatus("Error:Getworkers:duplicate workers");
                    }

                    if (workerItems.Count >= 1)
                    {
#if DEBUG
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
#endif
                        return workerItems;
                    }
                }

                if (filterFile != null && !string.IsNullOrEmpty(filterFile.FileName) && logFile != null && !string.IsNullOrEmpty(logFile.FileName))
                {
#if DEBUG
                    SetStatus(string.Format("Getworkers:searching by filter file name: {0} and log file name: {1}", filterFile.FileName, logFile.FileName));
#endif
                    if (_workerManager.BGWorkers.Exists(x => x.FilterFile != null && x.LogFile != null && x.FilterFile.FileName == filterFile.FileName && x.LogFile.FileName == logFile.FileName))
                    {
                        workerItems = _workerManager.BGWorkers.Where(x => x.FilterFile != null && x.LogFile != null && x.FilterFile.FileName == filterFile.FileName && x.LogFile.FileName == logFile.FileName).ToList();
                    }

                    if (workerItems.Count > 1)
                    {
                        SetStatus("Error:Getworkers:duplicate workers");
                    }

#if DEBUG
                    SetStatus("Getworkers:return:worker count:" + workerItems.Count);
#endif
                    return workerItems;
                }

                if (filterFile != null && !string.IsNullOrEmpty(filterFile.FileName))
                {
#if DEBUG
                    SetStatus("Getworkers:searching by filter file file name: " + filterFile.FileName);
#endif
                    if (_workerManager.BGWorkers.Exists(x => x.FilterFile != null && x.FilterFile.FileName == filterFile.FileName))
                    {
                        workerItems = _workerManager.BGWorkers.Where(x => x.FilterFile != null && x.FilterFile.FileName == filterFile.FileName).ToList();
                    }

                    if (baseFile)
                    {
                        workerItems = workerItems.Where(x => x.LogFile == null).ToList();
                    }
                    else
                    {
                        workerItems = workerItems.Where(x => x.LogFile != null).ToList();
                    }

                    if (workerItems.Count >= 1)
                    {
#if DEBUG
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
#endif
                        return workerItems;
                    }
                }

                if (filterFile != null && !string.IsNullOrEmpty(filterFile.Tag))
                {
#if DEBUG
                    SetStatus("Getworkers:searching by filter file tag: " + filterFile.Tag);
#endif
                    if (_workerManager.BGWorkers.Exists(x => x.FilterFile != null && x.FilterFile.Tag == filterFile.Tag))
                    {
                        workerItems = _workerManager.BGWorkers.Where(x => x.FilterFile != null && x.FilterFile.Tag == filterFile.Tag).ToList();
                    }

                    if (baseFile)
                    {
                        workerItems = workerItems.Where(x => x.LogFile == null).ToList();
                    }
                    else
                    {
                        workerItems = workerItems.Where(x => x.LogFile != null).ToList();
                    }

                    if (workerItems.Count >= 1)
                    {
#if DEBUG
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
#endif
                        return workerItems;
                    }
                }

                if (logFile != null && !string.IsNullOrEmpty(logFile.Tag))
                {
#if DEBUG
                    SetStatus("Getworkers:searching by log file tag: " + logFile.Tag);
#endif
                    if (_workerManager.BGWorkers.Exists(x => x.LogFile != null && x.LogFile.Tag == logFile.Tag))
                    {
                        workerItems = _workerManager.BGWorkers.Where(x => x.LogFile != null && x.LogFile.Tag == logFile.Tag).ToList();
                    }

                    if (baseFile)
                    {
                        workerItems = workerItems.Where(x => x.FilterFile == null).ToList();
                    }
                    else
                    {
                        workerItems = workerItems.Where(x => x.FilterFile != null).ToList();
                    }

                    if (workerItems.Count >= 1)
                    {
#if DEBUG
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
#endif
                        return workerItems;
                    }
                }

                if (logFile != null && !string.IsNullOrEmpty(logFile.FileName))
                {
#if DEBUG
                    SetStatus("Getworkers:searching by log file name: " + logFile.FileName);
#endif
                    if (_workerManager.BGWorkers.Exists(x => x.LogFile != null && x.LogFile.FileName == logFile.FileName))
                    {
                        workerItems = _workerManager.BGWorkers.Where(x => x.LogFile != null && x.LogFile.FileName == logFile.FileName).ToList();
                    }

                    if (baseFile)
                    {
                        workerItems = workerItems.Where(x => x.FilterFile == null).ToList();
                    }
                    else
                    {
                        workerItems = workerItems.Where(x => x.FilterFile != null).ToList();
                    }

                    if (workerItems.Count >= 1)
                    {
#if DEBUG
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
#endif
                        return workerItems;
                    }
                }

                if (logFile == null && filterFile == null)
                {
#if DEBUG
                    SetStatus("Getworkers:returning all workers");
#endif
                    workerItems = _workerManager.BGWorkers.ToList();
                }
#if DEBUG
                SetStatus("Getworkers:return:worker count:" + workerItems.Count);
#endif

                return workerItems;
            }
            catch (Exception e)
            {
                SetStatus("GetWorker:exception:" + e.ToString());
                return workerItems;
            }
            finally
            {
                if (ListLock.IsReadLockHeld)
                {
                    ListLock.ExitReadLock();
                }
            }
        }

        public bool ProcessWorker(WorkerItem workerItem)
        {
            if (workerItem == null)
            {
                SetStatus("WorkerManager.ProcessWorker enter worker null. returning");
                return false;
            }

            SetStatus(string.Format("WorkerManager.ProcessWorker enter: worker: {0} current worker.Modification: {1}", workerItem.GetHashCode(), workerItem.WorkerModification.ToString()));

            switch (workerItem.WorkerModification)
            {
                case WorkerItem.Modification.LogRemoved:
                    RemoveWorkersByLogFile(workerItem.LogFile);
                    return true;

                case WorkerItem.Modification.FilterRemoved:
                    RemoveWorkersByFilterFile(workerItem.FilterFile);
                    return true;

                case WorkerItem.Modification.FilterModified:
                    ResetCurrentWorkersByFilter(workerItem);
                    break;
                case WorkerItem.Modification.FilterIndex:
                case WorkerItem.Modification.LogIndex:
                    {
                        if (workerItem.WorkerState == WorkerItem.State.Completed)
                        {
                            LogViewModel.UpdateViewCallback(workerItem);
                            return true;
                        }
                        else
                        {
                            ResetWorkerStates(workerItem);
                        }
                    }
                    break;

                case WorkerItem.Modification.FilterAdded:
                    AddWorkersByWorkerItemFilterFile(workerItem);
                    break;

                case WorkerItem.Modification.LogAdded:
                    AddWorkersByWorkerItemLogFile(workerItem);
                    break;

                case WorkerItem.Modification.Unknown:
                default:
                    {
                        SetStatus("Error:WorkerManager.ProcessWorker unknown state exit.");
                        return false;
                    }
            }

            SetStatus("WorkerManager.ProcessWorker:workerItem.State:" + workerItem.WorkerState.ToString());
            return true;
        }

        public void ProcessWorkers(List<WorkerItem> workerItems)
        {
            foreach (WorkerItem workerItem in workerItems)
            {
                ProcessWorker(workerItem);
            }
        }

        public void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.
        }

        public void RemoveWorker(WorkerItem workerItem)
        {
            if(workerItem == null)
            {
                SetStatus("RemoveWorker: workerItem null, returning.");
                return;
            }

            try
            {
                // Cancel the asynchronous operation.
                if (BGWorkers.Contains(workerItem))
                {
                    ListLock.EnterWriteLock();

                    SetStatus("RemoveWorker:removing worker:" + workerItem.GetHashCode().ToString());
                    CancelWorker(workerItem);
                    BGWorkers.Remove(workerItem);
                }
                else
                {
                    SetStatus("RemoveWorker:worker does not exist:" + workerItem.GetHashCode().ToString());
                }
            }
            catch (Exception e)
            {
                SetStatus("RemoveWorker:exception:" + e.ToString());
            }
            finally
            {
                if (ListLock.IsWriteLockHeld)
                {
                    ListLock.ExitWriteLock();
                }
            }
        }

        public void RemoveWorkersByFilterFile(FilterFile filterFile)
        {
            foreach (WorkerItem item in GetWorkers(filterFile))
            {
                RemoveWorker(item);
                SetStatus(string.Format("RemoveWorkersByFilterFile:Removed worker:{0}", item.FilterFile == null ? "null" : item.FilterFile.Tag));
            }

            RemoveWorker(GetWorkers(filterFile, null, true).FirstOrDefault());
        }

        public void RemoveWorkersByLogFile(LogFile logFile)
        {
            foreach (WorkerItem item in GetWorkers(null, logFile))
            {
                RemoveWorker(item);
                SetStatus(string.Format("RemoveWorkersByLogFile:Removed worker:{0}", item.LogFile == null ? "null" : item.LogFile.Tag));
            }

            RemoveWorker(GetWorkers(null, logFile, true).FirstOrDefault());
        }

        public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // keep all view modifications on main thread
            Application.Current.Dispatcher.InvokeAsync((Action)delegate ()
            {
                ListLock.EnterWriteLock();
                WorkerItem workerItem = ((WorkerItem)e.Result);
                workerItem.BackGroundWorker.RunWorkerCompleted -= RunWorkerCompleted;
                SetStatus(string.Format("RunWorkerCompleted:enter: worker: {0} WorkerModification:{1} state:{2}", workerItem.GetHashCode(), workerItem.WorkerModification, workerItem.WorkerState));
                
                // First, handle the case where an exception was thrown.
                if (e.Error != null)
                {
                    SetStatus("RunWorkerCompleted: Error:" + e.ToString());
                    SetStatus(e.Error.Message);
                    workerItem.WorkerState = WorkerItem.State.Unknown;
                }
                else if (e.Cancelled)
                {
                    SetStatus("RunWorkerCompleted:Cancelled");
                    workerItem.WorkerState = WorkerItem.State.Aborted;
                }
                else if (workerItem.WorkerState != WorkerItem.State.Aborted)
                {
                    //WorkerItem workerItem = ((WorkerItem)e.Result);
                    workerItem.WorkerState = WorkerItem.State.Completed;
                    SetStatus("RunWorkerCompleted:callback:enter: \n" + workerItem.Status.ToString());
                    workerItem.Status.Clear();

                    SetStatus(string.Format("RunWorkerCompleted:workeritem modification: {0}", workerItem.WorkerModification));

                    switch (workerItem.WorkerModification)
                    {
                        case WorkerItem.Modification.FilterAdded:
                        case WorkerItem.Modification.FilterRemoved:
                            FilterViewModel.UpdateViewCallback(workerItem);
                            break;

                        case WorkerItem.Modification.FilterIndex:
                        case WorkerItem.Modification.FilterModified:
                        case WorkerItem.Modification.LogAdded:
                        case WorkerItem.Modification.LogIndex:
                        case WorkerItem.Modification.LogRemoved:
                            LogViewModel.UpdateViewCallback(workerItem);
                            break;

                        default:
                            SetStatus("RunWorkerCompleted:Cancelled");
                            break;
                    }
                }

                ListLock.ExitWriteLock();
                Mouse.OverrideCursor = null;
            });
        }

        public void StartWorker(WorkerItem workerItem)
        {
            // This method runs on background thread.
            try
            {
                SetStatus(string.Format("StartWorker:enter: worker: {0} WorkerModification:{1} state:{2}", workerItem.GetHashCode(), workerItem.WorkerModification, workerItem.WorkerState));

                ListLock.EnterWriteLock();
                switch (workerItem.WorkerModification)
                {
                    case WorkerItem.Modification.LogAdded:
                        workerItem.BackGroundWorker.DoWork -= DoLogWork;
                        workerItem.BackGroundWorker.DoWork += new DoWorkEventHandler(DoLogWork);
                        break;

                    case WorkerItem.Modification.FilterIndex:
                    case WorkerItem.Modification.LogIndex:

                    case WorkerItem.Modification.FilterAdded:
                    case WorkerItem.Modification.FilterModified:
                        workerItem.BackGroundWorker.DoWork -= DoFilterWork;
                        workerItem.BackGroundWorker.DoWork += new DoWorkEventHandler(DoFilterWork);
                        ListLock.ExitWriteLock();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            workerItem.VerifiedFilterItems = VerifyFilterPatterns(workerItem).VerifiedFilterItems;
                        });
                        ListLock.EnterWriteLock();

                        break;

                    default:
                        SetStatus("StartWorker:not configured WorkerModification:" + workerItem.WorkerModification.ToString());
                        workerItem.WorkerState = WorkerItem.State.Completed;
                        return;
                }

                // this shouldnt happen
                if (workerItem.BackGroundWorker.IsBusy)
                {
                    SetStatus("StartWorker:error worker already running! returning. configured WorkerModification:" + workerItem.WorkerModification.ToString());
                    return;
                }

                Mouse.OverrideCursor = Cursors.AppStarting;
                CancelAllWorkers();
                workerItem.BackGroundWorker.WorkerSupportsCancellation = true;
                //workerItem.BackGroundWorker.RunWorkerCompleted -= RunWorkerCompleted;
                workerItem.BackGroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);

                // Start the asynchronous operation.
                workerItem.WorkerState = WorkerItem.State.Started;
                workerItem.BackGroundWorker.RunWorkerAsync(workerItem);
                SetStatus("StartWorker:worker started:exit");
                return;
            }
            finally
            {
                ListLock.ExitWriteLock();
            }
        }

        private void AddWorker(WorkerItem workerItem)
        {
            try
            {
                ListLock.EnterWriteLock();

                if (GetWorkers(workerItem).Count == 0)
                {
                    if (workerItem.WorkerState == WorkerItem.State.NotStarted)
                    {
                        foreach (WorkerItem item in GetWorkers().Where(x => x.WorkerState == WorkerItem.State.NotStarted))
                        {
                            item.WorkerState = WorkerItem.State.Ready;
                        }
                    }

                    SetStatus(string.Format("AddWorker:adding worker: {0} {1} {2}", workerItem.GetHashCode(),
                        workerItem.LogFile == null ? "" : workerItem.LogFile.FileName,
                        workerItem.FilterFile == null ? "" : workerItem.FilterFile.FileName));


                    BGWorkers.Add(workerItem);
                    // RestartWorkers();
                }
                else
                {
                    SetStatus("AddWorker:exiting: worker already exists:" + workerItem.GetHashCode().ToString());
                }
            }
            catch (Exception e)
            {
                SetStatus("AddWorker:exception:" + e.ToString());
            }
            finally
            {
                if (ListLock.IsWriteLockHeld)
                {
                    ListLock.ExitWriteLock();
                }
            }
        }

        private void DoFilterWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker;
            bgWorker = (BackgroundWorker)sender;

            WorkerItem workerItem = (WorkerItem)e.Argument;

            workerItem.Status.AppendLine("DoFilterWork:enter");

            if (workerItem.FilterFile.ContentItems.Count == 0)
            {
                //workerItem.FilterFile.ContentItems = (ObservableCollection<IFileItem>)MMFConcurrentRead(workerItem).LogFile.ContentItems;
            }
            e.Result = MMFConcurrentFilter(workerItem);

            if (bgWorker.CancellationPending)
            {
                workerItem.WorkerState = WorkerItem.State.Aborted;
                e.Cancel = true;
            }

            workerItem.Status.AppendLine("WorkerManager:DoFilterWork:exit");
        }

        private void DoLogWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker;
            bgWorker = (BackgroundWorker)sender;

            WorkerItem workerItem = (WorkerItem)e.Argument;

            workerItem.Status.AppendLine("WorkerManager:DoLogWork:enter");

            e.Result = MMFConcurrentRead(workerItem);
            if (bgWorker.CancellationPending)
            {
                workerItem.WorkerState = WorkerItem.State.Aborted;
                e.Cancel = true;
            }

            workerItem.Status.AppendLine("WorkerManager:DoLogWork:exit");
        }

        private bool ResetCurrentWorkersByFilter(WorkerItem workerItem)
        {
            SetStatus(string.Format("ResetCurrentWorkersbyFilter:enter:{0}", workerItem.GetHashCode()));
            if (GetWorkers(workerItem.FilterFile).Count == 0)
            {
                SetStatus("ResetCurrentWorkersbyFilter:no workers. exiting");
                return false;
            }

            ListLock.EnterWriteLock();
            // to keep from resetting for same filter changes

            if (workerItem.FilterFile != null)
            {
                //if(_FilterViewModel.CompareFilterList(_previousFilterItems) != FilterNeed.Current)
                if (!CompareFilterLists(_previousFilterItems.ToList(), workerItem.FilterFile.ContentItems.ToList()))
                {
                    SetStatus("ResetCurrentWorkersbyFilter:different filter hash. RESETTING");
                    _previousFilterItems.Clear();
                    foreach (FilterFileItem fileItem in workerItem.FilterFile.ContentItems)
                    {
                        _previousFilterItems.Add((FilterFileItem)fileItem.ShallowCopy());
                    }

                    foreach (WorkerItem item in GetWorkers(workerItem.FilterFile))
                    {
                        if (item.WorkerState == WorkerItem.State.Started)
                        {
                            CancelWorker(item);
                        }

                        item.WorkerState = workerItem.LogFile == item.LogFile ? WorkerItem.State.NotStarted : WorkerItem.State.Ready;
                        SetStatus(string.Format("ResetCurrentWorkersbyFilter:resetting state:{0} new state:{1}", item.GetHashCode(), item.WorkerState));
                        item.WorkerModification = WorkerItem.Modification.FilterModified;
                    }
                }
            }
            else
            {
                SetStatus("ResetCurrentWorkersbyFilter:null FilterFile");
            }

            ListLock.ExitWriteLock();

            return true;
        }

        private bool CompareFilterLists(List<FilterFileItem> previousFilterItems, List<FilterFileItem> currentFilterItems)
        {
            if(previousFilterItems == null & currentFilterItems == null)
            {
                SetStatus("WorkerManager:CompareFilterLists:both null. returning true.");
                return true;
            }

            if(previousFilterItems == null | currentFilterItems == null)
            {
                SetStatus("WorkerManager:CompareFilterLists:one null. returning false.");
                return false;
            }

            foreach (FilterFileItem item in currentFilterItems.OrderBy(x => x.Index))
            {
                FilterFileItem previousItem = previousFilterItems.FirstOrDefault(x => x.Index == item.Index);
                if (previousItem == null)
                {
                    SetStatus("WorkerManager:CompareFilterLists:previous null, returning false.");
                    return false;
                }
                SetStatus(string.Format("CompareFilterLists:current:index: {0} enabled: {1} exclude: {2} regex: {3} filterpattern: {4} backgroundcolor: {5} foregroundcolor {6} ", item.Index, item.Enabled, item.Exclude, item.Regex, item.Filterpattern, item.BackgroundColor, item.ForegroundColor));
                SetStatus(string.Format("CompareFilterLists:previous:index: {0} enabled: {1} exclude: {2} regex: {3} filterpattern: {4} backgroundcolor: {5} foregroundcolor {6} ", previousItem.Index, previousItem.Enabled, previousItem.Exclude, previousItem.Regex, previousItem.Filterpattern, previousItem.BackgroundColor, previousItem.ForegroundColor));

                if (previousItem.Filterpattern != item.Filterpattern
                    | previousItem.Enabled != item.Enabled
                    | previousItem.Exclude != item.Exclude
                    | previousItem.Regex != item.Regex)
            //        | previousItem.ForegroundColor != item.ForegroundColor
            //        | previousItem.BackgroundColor != item.BackgroundColor)
                {
                    SetStatus("WorkerManager:CompareFilterLists:previous different, returning false.");
                    return false;
                }
            }

            SetStatus("WorkerManager:CompareFilterLists:same items. returning true.");
            return true;
        }

        private bool ResetWorkerStates(WorkerItem workerItem)
        {
            SetStatus("ResetWorkerStates:enter:");
            ListLock.EnterWriteLock();
            List<WorkerItem> workerItems = GetWorkers(workerItem);
            WorkerItem.State newState = WorkerItem.State.Ready;

            if (workerItems.Count == 0)
            {
                return false;
            }
            else if (workerItems.Count == 1)
            {
                newState = WorkerItem.State.NotStarted;
            }

            foreach (WorkerItem item in workerItems)
            {
                if (item.WorkerState == WorkerItem.State.Started)
                {
                    CancelWorker(item);
                }
                else if (item.WorkerState != WorkerItem.State.Completed)
                {
                    item.WorkerModification = workerItem.WorkerModification;
                    item.WorkerState = newState;
                    SetStatus("ResetWorkerStates:resetting state:" + item.WorkerState.ToString());
                }
            }

            ListLock.ExitWriteLock();
            return true;
        }
    }
}