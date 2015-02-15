using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;


namespace RegexViewer
{
    public class LogViewModel : BaseViewModel<LogFileItem>
    {
        #region Public Methods

        private List<FilterFileItem> _previousFilterFileItems = new List<FilterFileItem>();
        public void HideExecuted(object sender)
        {
            //if (!(sender is ListBox))
            //{
            //    return;
            //}

            ListBox listBox = (ListBox)this.CurrentTab().Viewer;
            LogFileItem logFileItem;

            // if count the same then assume it is not filtered

            if (!IsHiding())
            {
                logFileItem = _unFilteredSelectedItem = (LogFileItem)listBox.SelectedItem;

                // send empty function to reset to current filter in filterview
                if (!string.IsNullOrEmpty(QuickFindText))
                {
                    QuickFindChangedExecuted(null);
                }
                else
                {
                    this.FilterLogTabItems(FilterCommand.Hide);
                }
            }
            else
            {
                logFileItem = _filteredSelectedItem = (LogFileItem)listBox.SelectedItem;

                this.FilterLogTabItems(FilterCommand.ShowAll);
            }

            try
            {
                //ListBox listBox = (ListBox)this.TabItems[SelectedIndex].Viewer;
                if (listBox != null)
                {
                    if (listBox.Items.Contains(logFileItem))
                    {
                    }
                    else if (listBox.Items.Contains(_unFilteredSelectedItem))
                    {
                        logFileItem = _unFilteredSelectedItem;
                    }
                    else if (listBox.Items.Contains(_filteredSelectedItem))
                    {
                        logFileItem = _filteredSelectedItem;
                    }
                    else
                    {
                        return;
                    }

                    SetStatus("hiding:scrollingintoview:");
                    listBox.ScrollIntoView(logFileItem);
                    listBox.SelectedItem = logFileItem;
                }
            }
            catch (Exception e)
            {
                SetStatus("hiding:exception:" + e.ToString());
            }
        }

        public void KeyDownExecuted(object sender)
        {
            SetStatus("KeyDownExecuted");
            throw new NotImplementedException();
        }

        public void MouseWheelExecuted(object sender, KeyEventArgs e)
        {
            SetStatus("MouseWheelExecuted");
            throw new NotImplementedException();
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
            bool silent = (sender is string && !String.IsNullOrEmpty(sender as string)) ? true : false;

            string logName = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "";
            dlg.DefaultExt = ".csv";
            dlg.Filter = "All Files (*.*)|*.*|Csv Files (*.csv)|*.csv|Text Files (*.txt)|*.txt";

            Nullable<bool> result = false;
            // Show open file dialog box
            if (silent)
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
                LogFile logFile = new LogFile();
                if (String.IsNullOrEmpty((logFile = (LogFile)this.ViewManager.OpenFile(logName)).Tag))
                {
                    return;
                }

                // make new tab
                AddTabItem(logFile);
            }
            else
            {
            }
        }

        public void PageDownExecuted(object sender)
        {
            SetStatus("PageDownExecuted");
            throw new NotImplementedException();
        }

        public void PageUpExecuted(object sender)
        {
            SetStatus("PageUpExecuted");
            throw new NotImplementedException();
        }

        public void QuickFindChangedExecuted(object sender)
        {
            FilterFileItem fileItem = new FilterFileItem();
            SetStatus(string.Format("quickfindchangedexecuted:enter: {0}", (sender is string)));
            if (sender is string)
            {
                string filter = (sender as string);
                SetStatus(string.Format("quickfindchangedexecuted:string.length: {0}", (sender as string).Length));
                if (string.IsNullOrEmpty(filter))
                {
                    if (_filterViewModel.FilterList().Count > 0)
                    {
                        // send empty function to reset to current filter in filterview
                        this.FilterLogTabItems(FilterCommand.Reset);
                    }
                    else
                    {
                        // no filter show all
                        this.FilterLogTabItems(FilterCommand.ShowAll);
                    }

                    QuickFindText = string.Empty;
                    return;
                }

                fileItem.Filterpattern = QuickFindText = (sender as string);
            }
            else
            {
                fileItem.Filterpattern = QuickFindText;
            }

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
            this.FilterLogTabItems(FilterCommand.DynamicFilter, fileItem);
        }

        public override void RenameTabItem(string newName)
        {
            throw new NotImplementedException();
        }

        public override void SaveFile(object sender)
        {
            SetStatus("save file not implemented");
            throw new NotImplementedException();
        }

        public override void SaveFileAs(object sender)
        {
            ITabViewModel<LogFileItem> tabItem;

            if (sender is TabItem)
            {
                tabItem = (ITabViewModel<LogFileItem>)(sender as TabItem);
            }
            else
            {
                tabItem = (ITabViewModel<LogFileItem>)this.TabItems[this.SelectedIndex];
            }

            if (string.IsNullOrEmpty(tabItem.Tag))
            {
                this.TabItems.Remove(tabItem);
            }
            else
            {
                bool silent = (sender is string && !String.IsNullOrEmpty(sender as string)) ? true : false;

                string logName = string.Empty;
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                dlg.Filter = "All Files (*.*)|*.*|Csv Files (*.csv)|*.csv";
                dlg.FileName = string.Format("{0}.filtered{1}", Path.GetFileNameWithoutExtension(tabItem.Tag), Path.GetExtension(tabItem.Tag));
                Nullable<bool> result = false;
                // Show save file dialog box
                if (silent)
                {
                    result = true;
                    logName = (sender as string);
                }
                else
                {
                    result = dlg.ShowDialog();
                    logName = dlg.FileName;

                    if (string.IsNullOrEmpty(logName))
                    {
                        return;
                    }
                }

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    SetStatus(string.Format("saving file:{0}", logName));

                    this.ViewManager.SaveFile(logName, tabItem.ContentList);
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void _filterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // dont handle count updates

            SetStatus("_filterViewModel.PropertyChanged" + e.PropertyName);
            FilterLogTabItems();
        }

        private bool IsHiding()
        {
            ListBox listBox = (ListBox)this.CurrentTab().Viewer;

            // if count the same then assume it is not filtered
            SetStatus(string.Format("IsHiding:listBox.Items.Count:{0} CurrentFile().ContentItems.Count:{1}", listBox.Items.Count, CurrentFile().ContentItems.Count));
            if (listBox.Items.Count == CurrentFile().ContentItems.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void LogViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetStatus("LogViewModel.PropertyChanged" + e.PropertyName);
            FilterLogTabItems();
        }

        private void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("_filterViewModel.CollectionChanged" + sender.ToString());
            FilterLogTabItems();
        }

        #endregion Private Methods

        #region Private Fields

        //private int _selectedItemIndex;
        private LogFileItem _filteredSelectedItem;

        private FilterViewModel _filterViewModel;

        private Command _gotoLineCommand;

        private Command _hideCommand;

        private Command _keyDownCommand;

        private LogFileManager _logFileManager;

        private int _previousIndex;

        private Command _quickFindChangedCommand;

        private string _quickFindText = string.Empty;

        private LogFileItem _unFilteredSelectedItem;

        #endregion Private Fields

        #region Public Constructors

        public LogViewModel(FilterViewModel filterViewModel)
        {
            _filterViewModel = filterViewModel;
            this.TabItems = new ObservableCollection<ITabViewModel<LogFileItem>>();
            this.ViewManager = new LogFileManager();
            _logFileManager = (LogFileManager)this.ViewManager;

            _filterViewModel.PropertyChanged += _filterViewModel_PropertyChanged;
            this.PropertyChanged += LogViewModel_PropertyChanged;

            // load tabs from last session
            foreach (LogFile logFile in this.ViewManager.OpenFiles(this.Settings.CurrentLogFiles.ToArray()))
            {
                AddTabItem(logFile);
                // FilterLogTabItems(null, logFile);
            }

            FilterLogTabItems(FilterCommand.Reset);
            // FilterActiveTabItem();
        }

        #endregion Public Constructors

        #region Public Properties

        public Command GotoLineCommand
        {
            get
            {
                if (_gotoLineCommand == null)
                {
                    _gotoLineCommand = new Command(GotoLineExecuted);
                }
                _gotoLineCommand.CanExecute = true;

                return _gotoLineCommand;
            }
            set { _gotoLineCommand = value; }
        }

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

        public Command KeyDownCommand
        {
            get
            {
                if (_keyDownCommand == null)
                {
                    _keyDownCommand = new Command(KeyDownExecuted);
                }
                _keyDownCommand.CanExecute = true;

                return _keyDownCommand;
            }
            set { _keyDownCommand = value; }
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

        public string QuickFindText
        {
            get
            {
                return _quickFindText;
            }
            set
            {
                if (_quickFindText != value)
                {
                    _quickFindText = value;
                    // OnPropertyChanged("QuickFindText");
                }
            }
        }

        #endregion Public Properties

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
                _previousIndex = this.SelectedIndex;
                this.SelectedIndex = this.TabItems.Count - 1;
                //FilterLogTabItems(null, (LogFile)logFile, FilterCommand.Filter);
                FilterLogTabItems(FilterCommand.Filter);
            }
        }

        public void CtrlEndExecuted(object sender)
        {
            SetStatus("CtrlEndExecuted");
            throw new NotImplementedException();
        }

        public void CtrlHomeExecuted(object sender)
        {
            SetStatus("CtrlHomeExecuted");
            throw new NotImplementedException();
        }

        public void FilterLogTabItems(FilterCommand filterIntent = FilterCommand.Filter, FilterFileItem filter = null)
        {

            List<FilterFileItem> filterFileItems = new List<FilterFileItem>();
            SetStatus(string.Format("filterLogTabItems:enter filterIntent: {0}", filterIntent));
            LogFile logFile;

            // get current log file
            if (_logFileManager.FileManager.Count > 0)
            {
                logFile = (LogFile)CurrentFile();
            }
            else
            {
                return;
            }
          

            // get current filter list
            filterFileItems = _filterViewModel.FilterList();

            // dont check filter need if intent is to reset list to current filter or to show all
            if (filterIntent != FilterCommand.Reset 
                & filterIntent != FilterCommand.ShowAll
                & filterIntent != FilterCommand.Hide)
            {
                FilterNeed filterNeed = _filterViewModel.CompareFilterList(GetPreviousFilter());
                SetStatus(string.Format("filterLogTabItems: filterNeed: {0}", filterNeed));
            

                switch (filterNeed)
                {
                    case FilterNeed.ApplyColor:
                        {
                            this.TabItems[this.SelectedIndex].ContentList = _logFileManager.ApplyColor(logFile.ContentItems,filterFileItems);
                            SaveCurrentFilter(filterFileItems);
                            return;
                        }
                    case FilterNeed.Current:
                        {
                            if (_previousIndex == SelectedIndex & filter == null)
                            {
                                SetStatus("filterLogTabItems:no change");
                                return;
                            }

                            break;
                        }
                    case FilterNeed.Filter:
                    case FilterNeed.Unknown:
                    default:
                        break;
                }
            }

            

            switch(filterIntent)
            {
             
                case FilterCommand.DynamicFilter:
                    {
                        SetStatus(string.Format("switch:DynamicFilter: filterIntent:{0}", filterIntent));
                        // quick find
                        filterFileItems = new List<FilterFileItem>();
                        filterFileItems.Add(filter);
                        goto case FilterCommand.Filter;
                    }
                case FilterCommand.Reset:
                case FilterCommand.Filter:
                    {
                        SetStatus(string.Format("switch:Filter: filterIntent:{0}", filterIntent));
                        this.TabItems[this.SelectedIndex].ContentList = _logFileManager.ApplyFilter(logFile, filterFileItems, filterIntent);
                        break;
                    }
                case FilterCommand.Hide:
                    {
                        SetStatus(string.Format("switch:Hide: filterIntent:{0}", filterIntent));
                        // this.TabItems[this.SelectedIndex].ContentList = _logFileManager.ApplyColor(logFile.ContentItems, filterFileItems);
                        this.TabItems[this.SelectedIndex].ContentList = new ObservableCollection<LogFileItem>(logFile.ContentItems.Where(x => x.FilterIndex != -1));
                        break;
                    }
                case FilterCommand.ShowAll:
                    {
                        SetStatus(string.Format("switch:ShowAll: filterIntent:{0}", filterIntent));
                        // this.TabItems[this.SelectedIndex].ContentList = _logFileManager.ApplyColor(logFile.ContentItems, filterFileItems, true);
                        this.TabItems[this.SelectedIndex].ContentList = logFile.ContentItems;
                        break;
                    }
               
                case FilterCommand.Unknown:
                default:
                    {
                        break;
                    }
            }

            SaveCurrentFilter(filterFileItems);
        }

        private List<FilterFileItem> GetPreviousFilter()
        {
            return _previousFilterFileItems;
        }

        private void SaveCurrentFilter(List<FilterFileItem> filterFileItems)
        {
            // save filter for next applyfilter compare
            _previousFilterFileItems.Clear();
            foreach (FilterFileItem item in filterFileItems)
            {
                _previousFilterFileItems.Add((FilterFileItem)item.ShallowCopy());
            }
            _previousIndex = SelectedIndex;
        }

        public void GotoLineExecuted(object sender)
        {
            SetStatus("gotoLine");
            GotoLineDialog gotoDialog = new GotoLineDialog();
            int result = gotoDialog.WaitForResult();
            SetStatus("gotoLine:" + result.ToString());

            ListBox listBox = (ListBox)CurrentTab().Viewer;
            //if ((listBox.Items.Count >= result) && (result >= 0))
            if (result >= 0)
            {
                // todo: currently only works when unfiltered
                if (IsHiding())
                {
                    HideExecuted(null);
                }

                LogFileItem logFileItem = listBox.Items.Cast<LogFileItem>().FirstOrDefault(x => x.Index == result);
                listBox.ScrollIntoView(logFileItem);
                listBox.SelectedItem = logFileItem;
            }
        }
    }
}