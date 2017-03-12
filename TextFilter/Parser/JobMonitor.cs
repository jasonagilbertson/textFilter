using System;
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
            int ccount = 0;
            int scount = 0;
            try
            {
                while (true)
                {
                    workerManager.ListLock.EnterReadLock();
                    // see if any not completed
                    scount = workerManager.BGWorkers.Count(x => x.WorkerState == WorkerItem.State.Started);
                    ccount = workerManager.BGWorkers.Count(x => x.WorkerState != WorkerItem.State.Completed);
                    workerManager.ListLock.ExitReadLock();

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
                        Thread.Sleep(100);
                    }
                }
            }
            finally
            {
                if (workerManager.ListLock.IsWriteLockHeld)
                {
                    workerManager.ListLock.ExitWriteLock();
                }
            }
        }

        private void CheckWorkerStates(WorkerManager workerManager)
        {
            SetStatus("jobmonitor:checkworkerstates:enter");

            if (workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.NotStarted) > 0)
            {
                workerManager.StartWorker(workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.NotStarted));
            }
            else if (workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Ready) > 0)
            {
                workerManager.StartWorker(workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.Ready));
            }
            else if (workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Aborted) > 0)
            {
                workerManager.StartWorker(workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.Aborted));
            }
            else
            {
                //SetStatus("WorkerManager.ProcessWorker:workerItem not ready or notstarted. exiting.");
            }
        }

        private void ManageWorkerStates()
        {
            SetStatus("jobmonitor:ManageWorkerStates:enter:count:" + _workerManager.BGWorkers.Count);

//#if DEBUG
            foreach (WorkerItem worker in _workerManager.BGWorkers)
            {
                SetStatus(string.Format("ManageWorkerStates:current states: {0} logfile:{1} filterfile:{2} state: {3} modification: {4}",
                    worker.GetHashCode(),
                    worker.LogFile == null ? string.Empty : worker.LogFile.Tag,
                    worker.FilterFile == null ? string.Empty : worker.FilterFile.Tag,
                    worker.WorkerState,
                    worker.WorkerModification));
            }
//#endif
            if (_workerManager.BGWorkers.Count(x => x.WorkerState == WorkerItem.State.Started) == 0)
            {
                SetStatus("ManageWorkerStates:no workers in Started state.");
                if (_workerManager.BGWorkers.Exists(x => x.WorkerState == WorkerItem.State.NotStarted))
                {
                    SetStatus("ManageWorkerStates:starting worker in NotStarted state.");
                    _workerManager.ProcessWorker(_workerManager.BGWorkers.First(x => x.WorkerState == WorkerItem.State.NotStarted));
                    return;
                }
                if (_workerManager.BGWorkers.Exists(x => x.WorkerState == WorkerItem.State.Aborted))
                {
                    SetStatus("ManageWorkerStates:starting worker in Aborted state.");
                    _workerManager.ProcessWorker(_workerManager.BGWorkers.First(x => x.WorkerState == WorkerItem.State.Aborted));
                    return;
                }
                if (_workerManager.BGWorkers.Exists(x => x.WorkerState == WorkerItem.State.Ready))
                {
                    SetStatus("ManageWorkerStates:starting worker in Ready state.");
                    _workerManager.ProcessWorker(_workerManager.BGWorkers.First(x => x.WorkerState == WorkerItem.State.Ready));
                    return;
                }
            }

            SetStatus("ManageWorkerStates:exiting");
        }

        internal void Abort()
        {
            _monitorThread.Abort();
        }
    }
}