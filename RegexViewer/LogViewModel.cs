using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RegexViewer
{
    //public class RegexViewModel : MainViewModel, INotifyPropertyChanged, RegexViewer.IViewModel
    public class LogViewModel : BaseViewModel<LogFileItem>
    {
        #region Public Constructors

        public LogViewModel()
        {
            this.TabItems = new ObservableCollection<ITabViewModel<LogFileItem>>();
            this.FileManager = new LogFileManager();

            // load tabs from last session
            foreach (LogFileProperties logProperty in this.FileManager.OpenFiles(this.Settings.CurrentLogFiles.ToArray()))
            {
                AddTabItem(logProperty);
            }
        }

     
        #endregion Public Constructors

        #region Public Methods
        public override void NewFile(object sender)
        {
            //SetStatusHandler("new file not implemented");
            MainModel.SetStatus("new file not implemented");
            throw new NotImplementedException();
        }
        public override void AddTabItem(IFileProperties<LogFileItem> logProperties)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                MainModel.SetStatus("adding tab:" + logProperties.Tag);
                LogTabViewModel tabItem = new LogTabViewModel();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                tabItem.Name = this.TabItems.Count.ToString();
                tabItem.ContentList = ((LogFileProperties)logProperties).ContentItems;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.FileName;
                TabItems.Add(tabItem);
            }
        }

        /// <summary>
        /// Opens OpenFileDialog
        /// to test supply valid string file in argument sender
        /// </summary>
        /// <param name="sender"></param>
        public override void OpenFile(object sender)
        {
         //   this.OpenDialogVisible = true;
            //SetStatusHandler("opening file");
            MainModel.SetStatus("opening file");
            bool test = false;
            if(sender is string && !String.IsNullOrEmpty(sender as string))
            {
                test = true;
            }

            string logName = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            //dlg.Filter = "Text Files (*.txt)|*.txt|Csv Files (*.csv)|*.csv|All Files (*.*)|*.*"; // Filter files by extension
            dlg.Filter = "All Files (*.*)|*.*|Csv Files (*.csv)|*.csv|Text Files (*.txt)|*.txt"; // Filter files by extension

            Nullable<bool> result = false;
            // Show open file dialog box
            if (test)
            {
                result = true;
                logName = (sender as string);
            }
            else
            {
                result = dlg.ShowDialog();
                logName = dlg.FileName;
            }

            // Process open file dialog box results
            if (result == true && File.Exists(logName))
            {
                // Open document
                
                // SetStatusHandler(string.Format("opening file:{0}", logName));
                MainModel.SetStatus(string.Format("opening file:{0}", logName));
                LogFileProperties logProperties = new LogFileProperties();
                if (String.IsNullOrEmpty((logProperties = (LogFileProperties)this.FileManager.OpenFile(logName)).Tag))
                {
                    return;
                }

                // make new tab
                AddTabItem(logProperties);
            }
            else
            {

            }
        }

        #endregion Public Methods
    }
}