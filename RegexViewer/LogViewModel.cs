using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RegexViewer
{
    //public class RegexViewModel : MainViewModel, INotifyPropertyChanged, RegexViewer.IViewModel
    public class LogViewModel : BaseViewModel<LogFileItem>
    {
        #region Private Fields

        private FilterViewModel _filterViewModel;

        // private string _quickFindText = string.Empty;
        private Command _hideCommand;

        private bool _hiding = false;

        private LogFileManager _logFileManager;

        private int _previousIndex = -1;

        private Command _quickFindChangedCommand;

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
                FilterTabItem(null, logFile);
            }

            // FilterActiveTabItem();

            _filterViewModel.PropertyChanged += _filterViewModel_PropertyChanged;
            this.PropertyChanged += LogViewModel_PropertyChanged;
        }

        #endregion Public Constructors

        #region Public Enums

        public enum FilterCommand
        {
            filter,
            highlight,
            reset
        }

        #endregion Public Enums

        #region Public Properties

        public Command HideCommand
        {
            get
            {
                if (_hideCommand == null)
                {
                    _hideCommand = new Command(HideExecuted);
                }
                _hideCommand.CanExecute = true;

                return _hideCommand;
            }
            set { _hideCommand = value; }
        }

        public Command QuickFindChangedCommand
        {
            get
            {
                if (_quickFindChangedCommand == null)
                {
                    _quickFindChangedCommand = new Command(QuickFindChangedExecuted);
                }
                _quickFindChangedCommand.CanExecute = true;

                return _quickFindChangedCommand;
            }
            set { _quickFindChangedCommand = value; }
        }

        #endregion Public Properties

        #region Public Methods

        public override void AddTabItem(IFile<LogFileItem> logFile)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logFile.Tag, true) == 0))
            {
                SetStatus("adding tab:" + logFile.Tag);
                LogTabViewModel tabItem = new LogTabViewModel()
                {
                    Name = logFile.FileName,
                    Tag = logFile.Tag,
                    Header = logFile.FileName
                };

                TabItems.Add(tabItem);
            }
        }

        public void FilterTabItem(FilterFileItem filter = null, LogFile logFile = null, FilterCommand filterCommand = FilterCommand.filter)
        {
            try
            {
                // Debug.Assert(TabItems != null & SelectedIndex != -1);
                List<FilterFileItem> filterFileItems = new List<FilterFileItem>();

                if (this.TabItems.Count > 0
                    && this.TabItems.Count >= SelectedIndex
                    && _logFileManager.FileManager.Exists(x => x.Tag == this.TabItems[this.SelectedIndex].Tag))
                {
                    // find filter if it was not supplied
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

                    if (logFile == null)
                    {
                        // find logFile if it was not supplied
                        logFile = (LogFile)_logFileManager.FileManager.First(x => x.Tag == this.TabItems[SelectedIndex].Tag);
                    }

                    if (filterCommand == FilterCommand.reset)
                    {
                        // reset previous filter list
                        _logFileManager.CompareFilterList(filterFileItems);
                    }

                    // return full list if no filters
                    if (filterFileItems.Count == 0 | filterFileItems.Count(x => x.Enabled) == 0 & filterCommand == FilterCommand.filter)
                    {
                        // reset colors

                        this.TabItems[SelectedIndex].ContentList = _logFileManager.ResetColors(logFile.ContentItems);

                        // reset previous filter list
                        _logFileManager.CompareFilterList(filterFileItems);

                        return;
                    }

                    // return if nothing changed
                    if (_previousIndex == this.SelectedIndex & _logFileManager.CompareFilterList(filterFileItems) & filterCommand == FilterCommand.filter)
                    {
                        return;
                    }
                    else if (_previousIndex != this.SelectedIndex)
                    {
                        CleanPreviousTabContent();
                    }

                    // apply filter
                    this.TabItems[this.SelectedIndex].ContentList = _logFileManager.ApplyFilter(logFile, filterFileItems, filterCommand == FilterCommand.highlight);
                }
                else
                {
                    // return unfiltered
                    this.TabItems[this.SelectedIndex].ContentList = logFile.ContentItems;
                }
            }
            catch (Exception e)
            {
                SetStatus("Exception:FilterTabItem:" + e.ToString());
            }
        }

        public void HideExecuted(object sender)
        {
            if (!_hiding)
            {
                this.FilterTabItem(null, null, FilterCommand.highlight);
            }
            else
            {
                // send empty function to reset to current filter in filterview
                this.FilterTabItem(null, null, FilterCommand.reset);
            }

            _hiding = !_hiding;
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

        public void QuickFindChangedExecuted(object sender)
        {
            // todo: move to filter source?
            if (sender is string)
            {
                string filter = (sender as string);
                if (string.IsNullOrEmpty(filter))
                {
                    // send empty function to reset to current filter in filterview
                    this.FilterTabItem(null, null);
                    return;
                }

                FilterFileItem fileItem = new FilterFileItem() { Filterpattern = (sender as string) };
                try
                {
                    Regex test = new Regex(fileItem.Filterpattern);
                    fileItem.Regex = true;
                }
                catch
                {
                    SetStatus("quick find not a regex:" + fileItem.Filterpattern);
                    fileItem.Regex = false;
                }

                fileItem.Enabled = true;
                this.FilterTabItem(fileItem);
            }
        }

        public override void SaveFile(object sender)
        {
            SetStatus("save file not implemented");
            throw new NotImplementedException();
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
            _previousIndex = SelectedIndex;
        }

        private void LogViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetStatus("LogViewModel.PropertyChanged" + e.PropertyName);
            FilterTabItem();
        }

        private void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("_filterViewModel.CollectionChanged" + sender.ToString());
            FilterTabItem();
        }

        #endregion Private Methods

    }
}