using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexViewer
{
    public class WorkerManager:Base
    {
        public class BGWorkerInfo
        {
            public BackgroundWorker BackGroundWorker {get; set;}
            public Worker Worker { get; set; }
        }
        public List<BGWorkerInfo> Workers { get; set; }
        private static WorkerManager _workerManager;
        private WorkerManager() {}
        public static WorkerManager Instance
        {
            get
            {
                if(_workerManager == null)
                {
                    _workerManager = new WorkerManager();
                }
                return _workerManager;
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            // This event handler is where the actual work is done. 
            // This method runs on the background thread. 

            // Get the BackgroundWorker object that raised this event.
            System.ComponentModel.BackgroundWorker bgWorker;
            bgWorker = (System.ComponentModel.BackgroundWorker)sender;
            
            // Get the Words object and call the main method.
            Worker WC = (Worker)e.Argument;

           // WC.DoWork(worker, e);
        }
        public void CancelWorker(BGWorkerInfo bgWorker)
        {
            // Cancel the asynchronous operation. 
            bgWorker.BackGroundWorker.CancelAsync();
            if (this.Workers.Contains(bgWorker))
            {
                this.Workers.Remove(bgWorker);
            }
        }

        public BGWorkerInfo StartWorker(Worker worker)
        {
            
            // This method runs on the main thread. 
            BackgroundWorker bgWorker = new BackgroundWorker();
            // Initialize the object that the background worker calls.
            
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoWork);
            bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.RunWorkerCompleted);
            
            BGWorkerInfo bgInfo = new BGWorkerInfo
            {
                BackGroundWorker = bgWorker,
                Worker = worker
            };

            if(!this.Workers.Contains(bgInfo))
            {
                this.Workers.Add(bgInfo);
            }

            // Start the asynchronous operation.
            bgWorker.RunWorkerAsync(worker);
            return bgInfo;
        }

        public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes. 
            // This method runs on the main thread. 
            if (e.Error != null)
                SetStatus("Error: " + e.Error.Message);
            else if (e.Cancelled)
                SetStatus("Word counting canceled.");
            else
                SetStatus("Finished counting words.");
        }

        public void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.
            //Worker.CurrentState state =
            //    (Worker.CurrentState)e.UserState;
          //  this.LinesCounted.Text = state.LinesCounted.ToString();
          //  this.WordsCounted.Text = state.WordsMatched.ToString();
        }
    }
}
