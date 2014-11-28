using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
        FilterViewModel _filterViewModel;
        public LogViewModel(FilterViewModel filterViewModel)
        {
            _filterViewModel = filterViewModel;
            _filterViewModel.PropertyChanged += _filterViewModel_PropertyChanged;
            this.TabItems = new ObservableCollection<ITabViewModel<LogFileItem>>();
            this.FileManager = new LogFileManager();

            // load tabs from last session
            foreach (LogFileItems logProperty in this.FileManager.OpenFiles(this.Settings.CurrentLogFiles.ToArray()))
            {
                AddTabItem(logProperty);
            }
        }

           void _filterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // reparse log files
            SetStatus("_filterViewModel.PropertyChanged" + sender.ToString());
            //todo: prompt to save filter file(s) if listening for changes
            // todo: make handler for selected index?
            throw new NotImplementedException();
        }
     
        #endregion Public Constructors

        #region Public Methods
        public override void NewFile(object sender)
        {
            
            SetStatus("new file not implemented");
            throw new NotImplementedException();
        }
        public override void AddTabItem(IFileItems<LogFileItem> logProperties)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                SetStatus("adding tab:" + logProperties.Tag);
                LogTabViewModel tabItem = new LogTabViewModel();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                tabItem.Name = this.TabItems.Count.ToString();
                tabItem.ContentList = ((LogFileItems)logProperties).ContentItems;
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
            SetStatus("opening file");
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
                
                SetStatus(string.Format("opening file:{0}", logName));
                LogFileItems logFileItems = new LogFileItems();
                if (String.IsNullOrEmpty((logFileItems = (LogFileItems)this.FileManager.OpenFile(logName)).Tag))
                {
                    return;
                }

                // make new tab
                AddTabItem(logFileItems);
            }
            else
            {

            }
        }

        #endregion Public Methods

    }
}