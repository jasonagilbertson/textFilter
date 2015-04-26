using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RegexViewer
{
    public class WorkerManager : Base
    {
        #region Private Fields

        private static WorkerManager _workerManager;

        #endregion Private Fields

        #region Private Constructors

        private WorkerManager()
        {
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

        public List<BGWorkerInfo> Workers { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void CancelWorker(BGWorkerInfo bgWorker)
        {
            // Cancel the asynchronous operation.
            bgWorker.BackGroundWorker.CancelAsync();
            if (this.Workers.Contains(bgWorker))
            {
                this.Workers.Remove(bgWorker);
            }
        }

        public bool ProcessWorker(WorkerItem workerItem)
        {
            if (workerItem == null)
            {
                SetStatus("WorkerManager.ProcessWorker enter worker null. returning");
                return false;
            }

            SetStatus("WorkerManager.ProcessWorker enter worker.Modification:" + workerItem.WorkerModification.ToString());

            switch (workerItem.WorkerModification)
            {
                case WorkerItem.Modification.FilterAdded:
                case WorkerItem.Modification.FilterIndex:
                case WorkerItem.Modification.FilterModified:
                case WorkerItem.Modification.FilterRemoved:
                case WorkerItem.Modification.LogAdded:
                case WorkerItem.Modification.LogIndex:
                case WorkerItem.Modification.LogRemoved:
                case WorkerItem.Modification.Unknown:
                default:
                    {
                        SetStatus("WorkerManager.ProcessWorker unknown state exit.");
                        return false;
                    }
            }

            return true;
        }

        public void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.
            //Worker.CurrentState state =
            //    (Worker.CurrentState)e.UserState;
            //  this.LinesCounted.Text = state.LinesCounted.ToString();
            //  this.WordsCounted.Text = state.WordsMatched.ToString();
        }

        public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes. This method runs on
            // the main thread.
            if (e.Error != null)
                SetStatus("WorkerItem Error: " + e.Error.Message);
            else if (e.Cancelled)
                SetStatus("WorkerItem canceled.");
            else
                SetStatus("Finished workerItem.");
        }

        public BGWorkerInfo StartWorker(WorkerItem workerItem)
        {
            // This method runs on the main thread.
            BackgroundWorker bgWorker = new BackgroundWorker();
            // Initialize the object that the background worker calls.

            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoWork);
            bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.RunWorkerCompleted);

            BGWorkerInfo bgWorkerInfo = new BGWorkerInfo
            {
                BackGroundWorker = bgWorker,
                Worker = workerItem
            };

            if (!this.Workers.Contains(bgWorkerInfo))
            {
                this.Workers.Add(bgWorkerInfo);
            }

            // Start the asynchronous operation.
            bgWorker.RunWorkerAsync(workerItem);
            return bgWorkerInfo;
        }

        #endregion Public Methods

        #region Private Methods

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            // This event handler is where the actual work is done. This method runs on the
            // background thread.

            // Get the BackgroundWorker object that raised this event.
            System.ComponentModel.BackgroundWorker bgWorker;
            bgWorker = (System.ComponentModel.BackgroundWorker)sender;

            // Get the Words object and call the main method.
            WorkerItem workerItem = (WorkerItem)e.Argument;

            SetStatus("WorkerManager:DoWork:notImplemented");
        }

        #endregion Private Methods

        #region Public Classes

        public class BGWorkerInfo
        {
            #region Public Properties

            public BackgroundWorker BackGroundWorker { get; set; }

            public WorkerItem Worker { get; set; }

            #endregion Public Properties
        }

        #endregion Public Classes

        public List<WorkerItem> GetWorkers(WorkerItem workerItem)
        {
            if(workerItem.FilterFile != null && workerItem.LogFile != null)
            {
                return (List<WorkerItem>)_workerManager.Workers.Select(x => x.Worker)
                    .Where(x => x.FilterFile == workerItem.FilterFile && x.LogFile == workerItem.LogFile)
                    .Cast<WorkerItem>();
            }
            else if (workerItem.FilterFile != null)
            {
                return (List<WorkerItem>)_workerManager.Workers.Select(x => x.Worker)
                    .Where(x => x.FilterFile == workerItem.FilterFile)
                    .Cast<WorkerItem>();
            }
            else if (workerItem.LogFile != null)
            {
                return (List<WorkerItem>)_workerManager.Workers.Select(x => x.Worker)
                    .Where(x => x.LogFile == workerItem.LogFile)
                    .Cast<WorkerItem>();
            }
            else
            {
                return new List<WorkerItem>();
            }

        }
    }
}