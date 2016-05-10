using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextFilter
{
    class QueueMonitor:Base
    {
        Thread _monitorThread;
        WorkerManager _workerManager;
        
        public QueueMonitor(WorkerManager workerManager)
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
                    _workerManager.ListLock.EnterReadLock();
                    // see if any not completed
                    scount = _workerManager.BGWorkers.Count(x => x.WorkerState == WorkerItem.State.Started);
                    ccount  = _workerManager.BGWorkers.Count(x=> x.WorkerState != WorkerItem.State.Completed);
                    _workerManager.ListLock.ExitReadLock();
                    if(scount == 0 & ccount > 0)
                    {
                        CheckWorkerStates(workerManager);
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
                if(workerManager.ListLock.IsWriteLockHeld)
                {
                    workerManager.ListLock.ExitWriteLock();
                }
            }
        }

        private void CheckWorkerStates(WorkerManager workerManager)
        {
            if (workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.NotStarted) > 0)
            {
                workerManager.StartWorker(workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.NotStarted));
            }
            else if (workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Ready) > 0)
            {
                workerManager.StartWorker(_workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.Ready));
            }
            else if (workerManager.GetWorkers().Count(x => x.WorkerState == WorkerItem.State.Aborted) > 0)
            {
                workerManager.StartWorker(_workerManager.GetWorkers().First(x => x.WorkerState == WorkerItem.State.Aborted));
            }
            else
            {
                //SetStatus("WorkerManager.ProcessWorker:workerItem not ready or notstarted. exiting.");
            }
        }

        internal void Abort()
        {
            _monitorThread.Abort();
        }
    }
}
