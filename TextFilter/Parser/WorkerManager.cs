// *********************************************************************** Assembly : textFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-29-2015 ***********************************************************************
// <copyright file="WorkerManager.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace TextFilter
{
    public class WorkerManager : WorkerFunctions
    {
        #region Private Fields

        private static WorkerManager _workerManager;

        private WorkerFunctions _workerFunctions = new WorkerFunctions();

        #endregion Private Fields

        private Thread _monitorThread;

        #region Private Constructors

        private WorkerManager()
        {
            BGWorkers = new List<WorkerItem>();
        }

        #endregion Private Constructors

        #region Public Properties

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

        #endregion Public Properties

        #region Public Methods

        public void AddWorkersByWorkerItemFilterFile(WorkerItem workerItem)
        {
            //if (workerItem == null || workerItem.LogFile == null | workerItem.FilterFile == null)
            //{
            //    SetStatus("AddWorkersByFilterFile:returning due to incomplete workItem");
            //    return;
            //}

            // add base one with no log used when no log selected
            workerItem.FilterNeed = workerItem.FilterFile == null ? FilterNeed.ShowAll : FilterNeed.Filter;
            workerItem.WorkerModification = WorkerItem.Modification.FilterAdded;
            workerItem.WorkerState = WorkerItem.State.NotStarted;
            AddWorker(workerItem);

            List<LogFile> logFiles = GetLogFiles();

            if (logFiles.Count > 0)
            {
                foreach (LogFile logFile in logFiles)
                {
                    if (GetWorkers(workerItem.FilterFile, logFile).Count == 0)
                    {
                        WorkerItem bgItem = new WorkerItem()
                        {
                            FilterFile = workerItem.FilterFile,
                            LogFile = logFile,
                            FilterNeed = workerItem.FilterNeed,
                            WorkerModification = workerItem.WorkerModification,
                            // if current logfile, set to notstarted so it gets processed first
                            WorkerState = logFile == workerItem.LogFile ? WorkerItem.State.NotStarted : WorkerItem.State.Ready
                        };

                        AddWorker(bgItem);
                        SetStatus(string.Format("AddWorkersByWorkerItemFilterFile:Added worker:{0} {1} total workers:{2}",
                           workerItem.LogFile != null ? workerItem.LogFile.Tag : string.Empty,
                           workerItem.FilterFile.Tag,
                           _workerManager.BGWorkers.Count()));
                    }
                }
            }
            else
            {
                WorkerItem bgItem = new WorkerItem()
                {
                    FilterFile = workerItem.FilterFile,
                    FilterNeed = workerItem.FilterNeed,
                    WorkerModification = workerItem.WorkerModification,
                    // if current logfile, set to notstarted so it gets processed first
                    WorkerState = WorkerItem.State.NotStarted
                };

                AddWorker(bgItem);
                SetStatus(string.Format("AddWorkersByWorkerItemFilterFile:Added null worker:{0} {1} total workers:{2}",
                   workerItem.LogFile != null ? workerItem.LogFile.Tag : string.Empty,
                   workerItem.FilterFile.Tag,
                   _workerManager.BGWorkers.Count()));
            }
        }

        public void AddWorkersByWorkerItemLogFile(WorkerItem workerItem)
        {
            //if (workerItem == null || workerItem.LogFile == null | workerItem.FilterFile == null)
            //{
            //    SetStatus("AddWorkersByLogFile:returning due to incomplete workItem");
            //    return;
            //}

            // none should exist add base one with no filter used when no filter selected
            workerItem.FilterNeed = workerItem.FilterFile == null ? FilterNeed.ShowAll : FilterNeed.Filter;
            workerItem.WorkerModification = WorkerItem.Modification.LogAdded;
            workerItem.WorkerState = WorkerItem.State.NotStarted;
            AddWorker(workerItem);

            List<FilterFile> filterFiles = GetFilterFiles();
            if (filterFiles.Count > 0)
            {
                foreach (FilterFile filterFile in GetWorkers().Select(x => x.FilterFile).Distinct())
                {
                    if (GetWorkers(filterFile, workerItem.LogFile).Count == 0)
                    {
                        WorkerItem bgItem = new WorkerItem()
                        {
                            FilterFile = filterFile,
                            LogFile = workerItem.LogFile,
                            FilterNeed = workerItem.FilterNeed,
                            WorkerModification = workerItem.WorkerModification,
                            // if current logfile, set to notstarted so it gets processed first
                            WorkerState = filterFile == workerItem.FilterFile ? WorkerItem.State.NotStarted : WorkerItem.State.Ready
                        };

                        AddWorker(bgItem);
                        SetStatus(string.Format("AddWorkersByWorkerItemLogFile:Added worker:{0} {1} total workers:{2}",
                           workerItem.LogFile.Tag,
                           workerItem.FilterFile != null ? workerItem.FilterFile.Tag : string.Empty,
                           _workerManager.BGWorkers.Count()));
                    }
                }
            }
            else
            {
                WorkerItem bgItem = new WorkerItem()
                {
                    LogFile = workerItem.LogFile,
                    FilterNeed = workerItem.FilterNeed,
                    WorkerModification = workerItem.WorkerModification,
                    // if current logfile, set to notstarted so it gets processed first
                    WorkerState = WorkerItem.State.NotStarted
                };

                AddWorker(bgItem);
                SetStatus(string.Format("AddWorkersByWorkerItemLogFile:Added null worker:{0} {1} total workers:{2}",
                    workerItem.LogFile.Tag,
                    workerItem.FilterFile != null ? workerItem.FilterFile.Tag : string.Empty,
                    _workerManager.BGWorkers.Count()));
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

        public void CompleteWorker(BackgroundWorker worker)
        {
            if (BGWorkers.Exists(x => x.BackGroundWorker == worker))
            {
                foreach (WorkerItem workerItem in GetWorkers().Where(x => x.BackGroundWorker == worker))
                {
                    SetStatus("CompleteWorker:completing worker");
                    workerItem.WorkerState = WorkerItem.State.Completed;
                }
            }
            else
            {
                SetStatus("CompleteWorker:Error: worker does not exist");
            }

            RestartWorkers();
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

        public List<WorkerItem> GetWorkers(FilterFile filterFile = null, LogFile logFile = null)
        {
            SetStatus(string.Format("GetWorkers:enter:{0}, {1}",
                filterFile == null ? "null" : filterFile.Tag,
                logFile == null ? "null" : logFile.Tag));

            List<WorkerItem> workerItems = new List<WorkerItem>();

            if (filterFile != null && logFile != null)
            {
                workerItems = _workerManager.BGWorkers.Where(x => x.FilterFile == filterFile && x.LogFile == logFile).ToList();
                if (workerItems.Count > 1)
                {
                    SetStatus("Error:Getworkers:duplicate workers");
                }
            }
            else if (filterFile != null)
            {
                workerItems = _workerManager.BGWorkers.Where(x => x.FilterFile == filterFile).ToList();
            }
            else if (logFile != null)
            {
                workerItems = _workerManager.BGWorkers.Where(x => x.LogFile == logFile).ToList();
            }
            else
            {
                workerItems = _workerManager.BGWorkers.ToList();
            }

            SetStatus("Getworkers:return:worker count:" + workerItems.Count);

            return workerItems;
        }

        public bool ProcessWorker(WorkerItem workerItem)
        {

            // todo: remove when switching to mapped file
            return true;

            if (workerItem == null)
            {
                SetStatus("WorkerManager.ProcessWorker enter worker null. returning");
                return false;
            }

            SetStatus("WorkerManager.ProcessWorker enter worker.Modification:" + workerItem.WorkerModification.ToString());

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
                    CheckCurrentWorkers(workerItem);
                    break;

                case WorkerItem.Modification.LogIndex:
                    CheckCurrentWorkers(workerItem);
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

            if (workerItem.WorkerState == WorkerItem.State.Ready | workerItem.WorkerState == WorkerItem.State.NotStarted)
            {
                StartWorker(workerItem);
            }
            else if (GetWorkers().Count(x => x.WorkerState == WorkerItem.State.NotStarted) > 0)
            {
                StartWorker(GetWorkers().First(x => x.WorkerState == WorkerItem.State.NotStarted));
            }
            else if (GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Ready) > 0)
            {
                StartWorker(GetWorkers().First(x => x.WorkerState == WorkerItem.State.Ready));
            }
            else if (GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Aborted) > 0)
            {
                StartWorker(GetWorkers().First(x => x.WorkerState == WorkerItem.State.Aborted));
            }
            else
            {
                SetStatus("WorkerManager.ProcessWorker:workerItem not ready or notstarted. exiting.");
            }

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

        public void RemoveWorker(WorkerItem bgWorker)
        {
            // Cancel the asynchronous operation.
            if (BGWorkers.Contains(bgWorker))
            {
                SetStatus("RemoveWorker:removing worker");
                bgWorker.BackGroundWorker.CancelAsync();
                BGWorkers.Remove(bgWorker);
            }
        }

        public void RemoveWorkersByFilterFile(FilterFile filterFile)
        {
            foreach (WorkerItem item in GetWorkers(filterFile))
            {
                RemoveWorker(item);
                SetStatus(string.Format("RemoveWorkersByFilterFile:Removed worker:{0}", item.FilterFile == null ? "null" : item.FilterFile.Tag));
            }
        }

        public void RemoveWorkersByLogFile(LogFile logFile)
        {
            foreach (WorkerItem item in GetWorkers(null, logFile))
            {
                RemoveWorker(item);
                SetStatus(string.Format("RemoveWorkersByLogFile:Removed worker:{0}", item.LogFile == null ? "null" : item.LogFile.Tag));
            }
        }

        public void RunLogWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetStatus("RunWorkerCompleted:callback:enter");
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                SetStatus("RunWorkerCompleted: Error:" + e.ToString());
                SetStatus(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                SetStatus("RunWorkerCompleted:Cancelled");
            }
            else if (e.Result != null)
            {
                LogViewModel.UpdateLogFile((LogFile)e.Result);
            }
            else
            {
                SetStatus("RunWorkerCompleted:error result null");
            }

            Mouse.OverrideCursor = null;

            // keep all worker modifications on main thread
            Application.Current.Dispatcher.InvokeAsync((Action)delegate ()
            {
                CompleteWorker((BackgroundWorker)sender);
            });
            //CompleteWorker((BackgroundWorker)sender);
            // RemoveWorker((BackgroundWorker)sender);
        }

        public void StartWorker(WorkerItem workerItem)
        {
            // This method runs on the main thread.
            SetStatus(string.Format("StartWorker:enter:WorkerModification:{0} {1}", workerItem.WorkerModification, workerItem.WorkerState));

            switch (workerItem.WorkerModification)
            {
                case WorkerItem.Modification.LogAdded:
                    workerItem.BackGroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(DoLogWork);
                    break;

                case WorkerItem.Modification.FilterAdded:
                case WorkerItem.Modification.FilterModified:
                    workerItem.BackGroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(DoFilterWork);
                    break;

                default:
                    SetStatus("StartWorker:not configured WorkerModification:" + workerItem.WorkerModification.ToString());
                    return;
            }

            Mouse.OverrideCursor = Cursors.AppStarting;
            CancelAllWorkers();

            workerItem.BackGroundWorker.WorkerSupportsCancellation = true;
            workerItem.BackGroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(DoLogWork);
            workerItem.BackGroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(RunLogWorkerCompleted);

            // Start the asynchronous operation.
            workerItem.BackGroundWorker.RunWorkerAsync(workerItem);
            SetStatus("StartWorker:worker started:exit");
            return;
        }

        private void AddWorker(WorkerItem workerItem)
        {
            if (GetWorkers(workerItem).Count == 0)
            {
                if (workerItem.WorkerState == WorkerItem.State.NotStarted)
                {
                    foreach (WorkerItem item in GetWorkers().Where(x => x.WorkerState == WorkerItem.State.NotStarted))
                    {
                        item.WorkerState = WorkerItem.State.Ready;
                    }
                }

                SetStatus("AddWorker:adding worker");

                BGWorkers.Add(workerItem);
            }
            else
            {
                SetStatus("AddWorker:exiting: worker already exists");
            }
        }

        private void DoFilterWork(object sender, DoWorkEventArgs e)
        {
            SetStatus("DoFilterWork:enter:notimplementedexception");

            BackgroundWorker bgWorker;
            bgWorker = (BackgroundWorker)sender;

            WorkerItem workerItem = (WorkerItem)e.Argument;
            workerItem.WorkerState = WorkerItem.State.Started;

            //e.Result = MMFConcurrentRead(workerItem.LogFile, bgWorker);

            SetStatus("WorkerManager:DoFilterWork:exit");
        }

        #endregion Public Methods

        #region Private Methods

        private void CancelWorker(WorkerItem bgWorker)
        {
            if (bgWorker.BackGroundWorker != null && bgWorker.BackGroundWorker.IsBusy)
            {
                SetStatus("CancelAllWorkers:cancelling worker");
                bgWorker.BackGroundWorker.CancelAsync();
                bgWorker.WorkerState = WorkerItem.State.Aborted;
            }
        }

        private bool CheckCurrentWorkers(WorkerItem workerItem)
        {
            SetStatus("CheckCurrentWorkers:enter:");

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
                }
                else if (item.WorkerState != WorkerItem.State.Completed)
                {
                    item.WorkerModification = workerItem.WorkerModification;
                    item.WorkerState = newState;
                    SetStatus("CheckCurrentWorkers:resetting state:" + item.WorkerState.ToString());
                }
            }

            return true;
        }

        private void DoLogWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker;
            bgWorker = (BackgroundWorker)sender;

            WorkerItem workerItem = (WorkerItem)e.Argument;
            workerItem.WorkerState = WorkerItem.State.Started;

            e.Result = MMFConcurrentRead(workerItem.LogFile, bgWorker);

            SetStatus("WorkerManager:DoWork:exit");
        }

        private bool ResetCurrentWorkersByFilter(WorkerItem workerItem)
        {
            SetStatus("ResetCurrentWorkersbyFilter:enter");
            if (GetWorkers(workerItem.FilterFile).Count == 0)
            {
                return false;
            }

            foreach (WorkerItem item in GetWorkers(workerItem.FilterFile))
            {
                if (item.WorkerState == WorkerItem.State.Started)
                {
                    CancelWorker(item);
                }

                item.WorkerState = workerItem.LogFile == item.LogFile ? WorkerItem.State.NotStarted : WorkerItem.State.Ready;
                SetStatus("ResetCurrentworkersByfilter:resetting state:" + item.WorkerState.ToString());

                item.WorkerModification = WorkerItem.Modification.FilterModified;
            }

            return true;
        }

        private void RestartWorkers()
        {
            SetStatus("RestartWorkers:enter:count:" + BGWorkers.Count);

            foreach (WorkerItem worker in BGWorkers)
            {
                SetStatus(string.Format("RestartWorkers:logfile:{0} filterfile:{1} state: {2} modification: {3}",
                    worker.LogFile == null ? string.Empty : worker.LogFile.Tag,
                    worker.FilterFile == null ? string.Empty : worker.FilterFile.Tag,
                    worker.WorkerState,
                    worker.WorkerModification));
            }

            if (BGWorkers.Count(x => x.WorkerState == WorkerItem.State.Started) == 0)
            {
                SetStatus("RestartWorkers:no workers in Started state.");
                if (BGWorkers.Exists(x => x.WorkerState == WorkerItem.State.NotStarted))
                {
                    SetStatus("RestartWorkers:starting worker in NotStarted state.");
                    ProcessWorker(BGWorkers.First(x => x.WorkerState == WorkerItem.State.NotStarted));
                    return;
                }
                if (BGWorkers.Exists(x => x.WorkerState == WorkerItem.State.Aborted))
                {
                    SetStatus("RestartWorkers:starting worker in Aborted state.");
                    ProcessWorker(BGWorkers.First(x => x.WorkerState == WorkerItem.State.Aborted));
                    return;
                }
                if (BGWorkers.Exists(x => x.WorkerState == WorkerItem.State.Ready))
                {
                    SetStatus("RestartWorkers:starting worker in Ready state.");
                    ProcessWorker(BGWorkers.First(x => x.WorkerState == WorkerItem.State.Ready));
                    return;
                }
            }

            SetStatus("RestartWorkers:worker in started state.exiting");
        }

        #endregion Private Methods
    }
}