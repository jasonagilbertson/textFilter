using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace RegexViewer
{
    //public class RegexViewModel : MainViewModel, INotifyPropertyChanged, RegexViewer.IViewModel
    public class LogViewModel : BaseViewModel<LogFileItem>
    {
        #region Private Fields

        private FilterViewModel _filterViewModel;
        private LogFileManager _logFileManager;
        private int _previousIndex = -1;

        #endregion Private Fields

        #region Public Constructors

        public LogViewModel(FilterViewModel filterViewModel)
        {
            _filterViewModel = filterViewModel;
            this.TabItems = new ObservableCollection<ITabViewModel<LogFileItem>>();
            this.ViewManager = new LogFileManager();
            _logFileManager = (LogFileManager)this.ViewManager;

            // load tabs from last session
            foreach (LogFile logFile in this.ViewManager.OpenFiles(this.Settings.CurrentLogFiles.ToArray()))
            {
                AddTabItem(logFile);
                FilterTabItem(null,logFile);
            }

          //  FilterActiveTabItem();

            _filterViewModel.PropertyChanged += _filterViewModel_PropertyChanged;
            this.PropertyChanged += LogViewModel_PropertyChanged;
        }

        #endregion Public Constructors

        #region Public Methods

        public override void AddTabItem(IFile<LogFileItem> logFile)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logFile.Tag, true) == 0))
            {
                SetStatus("adding tab:" + logFile.Tag);
                LogTabViewModel tabItem = new LogTabViewModel();
                tabItem.Name = this.TabItems.Count.ToString();
                tabItem.Tag = logFile.Tag;
                tabItem.Header = logFile.FileName;
                TabItems.Add(tabItem);
            }
        }

        public override void NewFile(object sender)
        {
            SetStatus("new file not implemented");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens OpenFileDialog to test supply valid string file in argument sender
        /// </summary>
        /// <param name="sender"></param>
        public override void OpenFile(object sender)
        {
            SetStatus("opening file");
            bool test = false;
            if (sender is string && !String.IsNullOrEmpty(sender as string))
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
                LogFile logFileItems = new LogFile();
                if (String.IsNullOrEmpty((logFileItems = (LogFile)this.ViewManager.OpenFile(logName)).Tag))
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

        #region Private Methods

        private void _filterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // dont handle count updates

            SetStatus("_filterViewModel.PropertyChanged" + e.PropertyName);
            FilterTabItem();
        }

        private void CleanPreviousTabContent()
        {
            if (_previousIndex != SelectedIndex && SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count)
            {
           //     LogFile previousLogFile = (LogFile)_logFileManager.FileManager.First(x => x.Tag == this.TabItems[SelectedIndex].Tag);
            //    previousLogFile.ContentItems.Clear();
           //     GC.Collect();
            }

            _previousIndex = SelectedIndex;
        }

        public void FilterTabItem(FilterFileItem filter = null, LogFile logFile = null)
        {
            // Debug.Assert(TabItems != null & SelectedIndex != -1);
            List<FilterFileItem> filterFileItems = new List<FilterFileItem>();

            if (this.TabItems.Count > 0
                && this.TabItems.Count >= SelectedIndex
                && _logFileManager.FileManager.Exists(x => x.Tag == this.TabItems[SelectedIndex].Tag))
            {
                if (filter == null)
                {
                    FilterFile filterFile = (FilterFile)_filterViewModel.ViewManager.FileManager.First(
                        x => x.Tag == _filterViewModel.TabItems[_filterViewModel.SelectedIndex].Tag);

                    filterFileItems = _logFileManager.CleanFilterList(filterFile);
                }
                else
                {
                    
                    filterFileItems.Add(filter);
                }

                if (_previousIndex == SelectedIndex & _logFileManager.CompareFilterList(filterFileItems))
                //if (_logFileManager.CompareFilterList(filterFileItems))
                {
                    return;
                }
                else if (_previousIndex != SelectedIndex)
                {
                    CleanPreviousTabContent();
                }

                if (logFile == null)
                {
                    logFile = (LogFile)_logFileManager.FileManager.First(x => x.Tag == this.TabItems[SelectedIndex].Tag);
                }

                this.TabItems[SelectedIndex].ContentList = _logFileManager.ApplyFilter(logFile, filterFileItems);
            }
        }

        private void LogViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // dont handle count updates

            SetStatus("LogViewModel.PropertyChanged" + e.PropertyName);
            FilterTabItem();
        }

        private void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // dont handle count updates

            SetStatus("_filterViewModel.CollectionChanged" + sender.ToString());
            FilterTabItem();
        }

        #endregion Private Methods
    }
}