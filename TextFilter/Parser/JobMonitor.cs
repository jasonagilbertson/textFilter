using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace TextFilter
{
    internal class JobMonitor : Base
    {
        private Thread _monitorThread;
        private WorkerManager _workerManager;

        public JobMonitor(WorkerManager workerManager)
        {
            _workerManager = workerManager;
            _monitorThread = new Thread(DoWork);
            _monitorThread.SetApartmentState(ApartmentState.STA);
            _monitorThread.Start(_workerManager);
        }

        public void DoWork(object data)
        {
            WorkerManager workerManager = (WorkerManager)data;
            int pccount = 0, ccount = 0;
            int pscount = 0, scount = 0;
            try
            {
                while (true)
                {
                    //if(!workerManager.ListLock.TryEnterReadLock(100))
                    //{
                    //    Debug.Print("jobmonitor:unable to get lock. skipping");
                    //    Thread.Yield();
                    //}
                    //else
                    //{ 
                    // see if any not completed
                    //scount = workerManager.BGWorkers.Count(x => x.WorkerState == WorkerItem.State.Started);
                    //ccount = workerManager.BGWorkers.Count(x => x.WorkerState != WorkerItem.State.Completed);

                    scount = workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Started);
                    ccount = workerManager.GetWorkers().Count(x => x.WorkerState != WorkerItem.State.Completed);
                    
                    if (scount != pscount | ccount != pccount)
                        {
                            SetStatus(string.Format("jobmonitor:status change:new not completed:{0} old not completed:{1} new started:{2} old started:{3}",ccount,pccount,scount,pscount));
                            pscount = scount;
                            pccount = ccount;
                        }
                    //}

                    //if(workerManager.ListLock.IsReadLockHeld)
                    //{
                    //    workerManager.ListLock.ExitReadLock();
                    //}

                    if (scount == 0 & ccount > 0)
                    {
                        
                        CheckWorkerStates(workerManager);
                       // workerManager.ListLock.EnterWriteLock();
                        ManageWorkerStates();
                       // workerManager.ListLock.ExitWriteLock();

                        Thread.Sleep(10);
                    }
                    else
                    {
#if DEBUG                        
                        Thread.Sleep(1000);
#else
                        Thread.Sleep(100);
#endif
                    }
                }
            }
            finally
            {
                //if (workerManager.ListLock.IsWriteLockHeld)
                //{
                //    workerManager.ListLock.ExitWriteLock();
                //}
            }
        }

        private void CheckWorkerStates(WorkerManager workerManager)
        {
            Debug.Print("jobmonitor:checkworkerstates:enter");

            if (_workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.NotStarted) > 0)
            {
                _workerManager.StartWorker(_workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.NotStarted));
            }
            else if (_workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Ready) > 0)
            {
                _workerManager.StartWorker(_workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.Ready));
            }
            else if (_workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Aborted) > 0)
            {
                _workerManager.StartWorker(_workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.Aborted));
            }
            else
            {
                SetStatus("WorkerManager.ProcessWorker:workerItem not ready or notstarted. exiting.");
            }
        }

        private void ManageWorkerStates()
        {
            Debug.Print("jobmonitor:ManageWorkerStates:enter:count:" + _workerManager.GetWorkers().Count);

#if DEBUG
            foreach (WorkerItem worker in _workerManager.GetWorkers())
            {
                SetStatus(string.Format("ManageWorkerStates:current states: {0} logfile:{1} filterfile:{2} state: {3} modification: {4}",
                    worker.GetHashCode(),
                    worker.LogFile == null ? string.Empty : worker.LogFile.Tag,
                    worker.FilterFile == null ? string.Empty : worker.FilterFile.Tag,
                    worker.WorkerState,
                    worker.WorkerModification));
            }
#endif
            if (_workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Started) == 0)
            {
                SetStatus("ManageWorkerStates:no workers in Started state.");
                if (_workerManager.GetWorkers().Exists(x => x.WorkerState == WorkerItem.State.NotStarted))
                {
                    SetStatus("ManageWorkerStates:starting worker in NotStarted state.");
                    _workerManager.ProcessWorker(_workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.NotStarted));
                    return;
                }
                if (_workerManager.GetWorkers().Exists(x => x.WorkerState == WorkerItem.State.Aborted))
                {
                    SetStatus("ManageWorkerStates:starting worker in Aborted state.");
                    _workerManager.ProcessWorker(_workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.Aborted));
                    return;
                }
                if (_workerManager.GetWorkers().Exists(x => x.WorkerState == WorkerItem.State.Ready))
                {
                    SetStatus("ManageWorkerStates:starting worker in Ready state.");
                    _workerManager.ProcessWorker(_workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.Ready));
                    return;
                }
            }

            // SetStatus("ManageWorkerStates:exiting");
        }

        internal void Abort()
        {
            _monitorThread.Abort();
        }
    }
}