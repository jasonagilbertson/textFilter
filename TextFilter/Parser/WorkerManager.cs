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
using System.Diagnostics;
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

        //private ReaderWriterLockSlim listLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
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
            WorkerItem baseItem = new WorkerItem()
            {
                FilterNeed = FilterNeed.Unknown,
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
                    if(logFile == null)
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
        }


        public void AddWorkersByWorkerItemLogFile(WorkerItem workerItem)
        {
            //if (workerItem == null || workerItem.LogFile == null | workerItem.FilterFile == null)
            //{
            //    SetStatus("AddWorkersByLogFile:returning due to incomplete workItem");
            //    return;
            //}

            // none should exist add base one with no filter used when no filter selected
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
            SetStatus("CompleteWorker:enter");

            if (GetWorkers().Exists(x => x.BackGroundWorker == worker))
            {
                foreach (WorkerItem workerItem in GetWorkers().Where(x => x.BackGroundWorker == worker))
                {
                    SetStatus("CompleteWorker:completing worker:" + worker.GetHashCode().ToString());
                    workerItem.WorkerState = WorkerItem.State.Completed;
                }
            }
            else if (GetWorkers().Exists(x => x.WorkerState == WorkerItem.State.Started))
            {
                foreach (WorkerItem workerItem in GetWorkers().Where(x => x.WorkerState == WorkerItem.State.Started))
                {
                    SetStatus("CompleteWorker:completing 'started' worker:" + worker.GetHashCode().ToString());
                    workerItem.WorkerState = WorkerItem.State.Completed;
                }
            }
            else
            {
                SetStatus("CompleteWorker:Error: worker does not exist:" + worker.GetHashCode().ToString()); 
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

        public WorkerItem GetBaseWorker(WorkerItem workerItem)
        {
            return GetWorkers(workerItem.FilterFile, workerItem.LogFile,true).FirstOrDefault();
        }
        public List<WorkerItem> GetWorkers(WorkerItem workerItem)
        {
            return GetWorkers(workerItem.FilterFile, workerItem.LogFile);
        }

        public List<WorkerItem> GetWorkers(FilterFile filterFile = null, LogFile logFile = null, bool baseFile = false)
        {
            SetStatus(string.Format("GetWorkers:enter:{0}, {1}",
                filterFile == null ? "null" : filterFile.Tag,
                logFile == null ? "null" : logFile.Tag));

            List<WorkerItem> workerItems = new List<WorkerItem>();

            //listLock.EnterWriteLock();
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
                    SetStatus(string.Format("Getworkers:searching by filter file and log object"));
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
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
                        return workerItems;
                    }
                    
                }

                if (filterFile != null && !string.IsNullOrEmpty(filterFile.Tag) && logFile != null && !string.IsNullOrEmpty(logFile.Tag))
                {
                    SetStatus(string.Format("Getworkers:searching by filter file tag: {0} and log file tag: {1}", filterFile.Tag, logFile.Tag));
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
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
                        return workerItems;
                    }

                }

                if (filterFile != null && !string.IsNullOrEmpty(filterFile.FileName) && logFile != null && !string.IsNullOrEmpty(logFile.FileName))
                {
                    SetStatus(string.Format("Getworkers:searching by filter file name: {0} and log file name: {1}", filterFile.FileName, logFile.FileName));
                    if (_workerManager.BGWorkers.Exists(x => x.FilterFile != null && x.LogFile != null && x.FilterFile.FileName == filterFile.FileName && x.LogFile.FileName == logFile.FileName))
                    {
                        workerItems = _workerManager.BGWorkers.Where(x => x.FilterFile != null && x.LogFile != null && x.FilterFile.FileName == filterFile.FileName && x.LogFile.FileName == logFile.FileName).ToList();
                    }

                    if (workerItems.Count > 1)
                    {
                        SetStatus("Error:Getworkers:duplicate workers");
                    }

                    if (workerItems.Count >= 1)
                    {
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
                        return workerItems;
                    }

                }


                if (filterFile != null && !string.IsNullOrEmpty(filterFile.FileName))
                {
                    SetStatus("Getworkers:searching by filter file file name: " + filterFile.FileName);
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
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
                        return workerItems;
                    }

                }

                if (filterFile != null && !string.IsNullOrEmpty(filterFile.Tag))
                {
                    SetStatus("Getworkers:searching by filter file tag: " + filterFile.Tag);
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
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
                        return workerItems;
                    }


                }

                if (logFile != null && !string.IsNullOrEmpty(logFile.Tag))
                {
                    SetStatus("Getworkers:searching by log file tag: " + logFile.Tag);
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
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
                        return workerItems;
                    }

                }



                if (logFile != null && !string.IsNullOrEmpty(logFile.FileName))
                {
                    SetStatus("Getworkers:searching by log file name: " + logFile.FileName);
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
                        SetStatus("Getworkers:return:worker count:" + workerItems.Count);
                        return workerItems;
                    }

                }

                if (logFile == null && filterFile == null)
                {
                    SetStatus("Getworkers:returning all workers");
                    workerItems = _workerManager.BGWorkers.ToList();
                }

                SetStatus("Getworkers:return:worker count:" + workerItems.Count);

                return workerItems;
            }
            catch(Exception e)
            {
                SetStatus("GetWorker:exception:" + e.ToString());
                return workerItems;
            }
            finally
            {
                //listLock.ExitWriteLock();
            }
        }

        

        public bool ProcessWorker(WorkerItem workerItem)
        {

            // todo: remove when switching to mapped file
           // return true;

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

        public void RemoveWorker(WorkerItem workerItem)
        {
            
            try
            {

                // Cancel the asynchronous operation.
                if (BGWorkers.Contains(workerItem))
                {
                    //listLock.EnterWriteLock();
                    SetStatus("RemoveWorker:removing worker:" + workerItem.GetHashCode().ToString());
                    workerItem.BackGroundWorker.CancelAsync();
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
                //if (listLock.IsWriteLockHeld)
                //{
                //    listLock.ExitWriteLock();
                //}
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
                // keep all view modifications on main thread
                Application.Current.Dispatcher.InvokeAsync((Action)delegate ()
                {
                    LogViewModel.UpdateLogFileCallBack((WorkerItem)e.Result);
                });
                
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

                // case WorkerItem.Modification.FilterIndex:
                // case WorkerItem.Modification.LogIndex:
                case WorkerItem.Modification.FilterAdded:
                case WorkerItem.Modification.FilterModified:
                    workerItem.BackGroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(DoFilterWork);
                    workerItem.VerifiedFilterItems = VerifyFilterPatterns(workerItem).VerifiedFilterItems;
                    break;

                default:
                    SetStatus("StartWorker:not configured WorkerModification:" + workerItem.WorkerModification.ToString());
                    workerItem.WorkerState = WorkerItem.State.Completed;
                    return;
            }

            Mouse.OverrideCursor = Cursors.AppStarting;
            CancelAllWorkers();

            workerItem.BackGroundWorker.WorkerSupportsCancellation = true;
            //workerItem.BackGroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(DoLogWork);
            workerItem.BackGroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(RunLogWorkerCompleted);

            // Start the asynchronous operation.
            workerItem.BackGroundWorker.RunWorkerAsync(workerItem);
            SetStatus("StartWorker:worker started:exit");
            return;
        }

        private void AddWorker(WorkerItem workerItem)
        {
         
            try
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

                    SetStatus("AddWorker:adding worker:" + workerItem.GetHashCode().ToString());
                    //listLock.EnterWriteLock();
                    BGWorkers.Add(workerItem);
                    RestartWorkers();
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
                //if (listLock.IsWriteLockHeld)
                //{
                //    listLock.ExitWriteLock();
                //}
            }
        }

        private void DoFilterWork(object sender, DoWorkEventArgs e)
        {
            Debug.Print("DoFilterWork:enter");

            BackgroundWorker bgWorker;
            bgWorker = (BackgroundWorker)sender;
            
            WorkerItem workerItem = (WorkerItem)e.Argument;
            workerItem.WorkerState = WorkerItem.State.Started;
            e.Result = MMFConcurrentFilter(workerItem);

            Debug.Print("WorkerManager:DoFilterWork:exit");
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
            Debug.Print("WorkerManager:DoLogWork:enter");
            BackgroundWorker bgWorker;
            bgWorker = (BackgroundWorker)sender;

            WorkerItem workerItem = (WorkerItem)e.Argument;
            workerItem.WorkerState = WorkerItem.State.Started;

            e.Result = MMFConcurrentRead(workerItem);

            Debug.Print("WorkerManager:DoLogWork:exit");
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