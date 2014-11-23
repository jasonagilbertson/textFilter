using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexViewer
{
    public class WorkerManager: Base
    {
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // This event handler is where the actual work is done. 
            // This method runs on the background thread. 

            // Get the BackgroundWorker object that raised this event.
            System.ComponentModel.BackgroundWorker worker;
            worker = (System.ComponentModel.BackgroundWorker)sender;

            // Get the Words object and call the main method.
            Worker WC = (Worker)e.Argument;
            WC.CountWords(worker, e);
        }
        private void CancelWorker(BackgroundWorker bgWorker)
        {
            // Cancel the asynchronous operation. 
            bgWorker.CancelAsync();
        }

        private BackgroundWorker StartWorker()
        {
            // This method runs on the main thread. 
       //     this.WordsCounted.Text = "0";
            BackgroundWorker bgWorker = new BackgroundWorker();
            // Initialize the object that the background worker calls.
            Worker WC = new Worker();
        //    WC.CompareString = this.CompareString.Text;
        //    WC.SourceFile = this.SourceFile.Text;

            // Start the asynchronous operation.
            bgWorker.RunWorkerAsync(WC);
            return bgWorker;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes. 
            // This method runs on the main thread. 
            if (e.Error != null)
                MainModel.SetStatus("Error: " + e.Error.Message);
            else if (e.Cancelled)
                MainModel.SetStatus("Word counting canceled.");
            else
                MainModel.SetStatus("Finished counting words.");
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.
            Worker.CurrentState state =
                (Worker.CurrentState)e.UserState;
          //  this.LinesCounted.Text = state.LinesCounted.ToString();
          //  this.WordsCounted.Text = state.WordsMatched.ToString();
        }
    }
}
