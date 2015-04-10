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
        #region Private Methods

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

        private List<FilterFileItem> _previousFilterFileItems = new List<FilterFileItem>();

        private Command _quickFindChangedCommand;

        //private int _previousIndex;
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

                this.SelectedIndex = this.TabItems.Count - 1;
                _previousFilterFileItems  = new List<FilterFileItem>();
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
            try
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

                filterFileItems = _filterViewModel.FilterList();

                // dont check filter need if intent is to reset list to current filter or to show all
                if (filterIntent != FilterCommand.DynamicFilter
                    & filterIntent != FilterCommand.Reset
                    & filterIntent != FilterCommand.ShowAll
                    & filterIntent != FilterCommand.Hide)
                {
                    FilterNeed filterNeed = _filterViewModel.CompareFilterList(GetPreviousFilter());
                    SetStatus(string.Format("filterLogTabItems: filterNeed: {0}", filterNeed));

                    switch (filterNeed)
                    {
                        case FilterNeed.ApplyColor:
                            {
                                this.TabItems[this.SelectedIndex].ContentList = _logFileManager.ApplyColor(logFile.ContentItems, filterFileItems);
                                SaveCurrentFilter(filterFileItems);
                                return;
                            }
                        case FilterNeed.Current:
                            {
                                if (this.PreviousIndex == this.SelectedIndex & filter == null)
                                {
                                    SetStatus("filterLogTabItems:no change");
                                    return;
                                }

                                break;
                            }

                        case FilterNeed.ShowAll:
                            {
                                filterIntent = FilterCommand.ShowAll;
                                break;
                            }
                        case FilterNeed.Filter:
                            break;

                        case FilterNeed.Unknown:

                        default:
                            SaveCurrentFilter(filterFileItems);
                            return;
                    }
                }

                switch (filterIntent)
                {
                    case FilterCommand.DynamicFilter:
                        {
                            SetStatus(string.Format("switch:DynamicFilter: filterIntent:{0}", filterIntent));
                            filter.Include = true;
                            filter.Regex = true;
                            // quick find
                            filterFileItems.Add(filter);
                            goto case FilterCommand.Filter;
                        }
                    case FilterCommand.Reset:
                    case FilterCommand.Filter:
                        {
                            SetStatus(string.Format("switch:Filter: filterIntent:{0}", filterIntent));
                            this.TabItems[this.SelectedIndex].ContentList = _logFileManager.ApplyColor(_logFileManager.ApplyFilter(logFile, filterFileItems, filterIntent), filterFileItems);

                            break;
                        }
                    case FilterCommand.Hide:
                        {
                            SetStatus(string.Format("switch:Hide: filterIntent:{0}", filterIntent));
                            // causes exception if no filter? FilterLogTabItems:exceptionSystem.ArgumentOutOfRangeException:
                            this.TabItems[this.SelectedIndex].ContentList = new ObservableCollection<LogFileItem>(logFile.ContentItems.Where(x => x.FilterIndex > -1));
                            break;
                        }
                    case FilterCommand.ShowAll:
                        {
                            SetStatus(string.Format("switch:ShowAll: filterIntent:{0}", filterIntent));
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
            catch (Exception e)
            {
                SetStatus("FilterLogTabItems:exception" + e.ToString());
            }
        }

        public void GotoLineExecuted(object sender)
        {
            SetStatus("gotoLine");
            GotoLineDialog gotoDialog = new GotoLineDialog();
            int result = gotoDialog.WaitForResult();
            SetStatus("gotoLine:" + result.ToString());

            ListBox listBox = (ListBox)CurrentTab().Viewer;

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
                listBox.SelectedIndex = listBox.Items.IndexOf(logFileItem);
             
            }
        }

        public void HideExecuted(object sender)
        {
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
                    listBox.SelectedIndex = listBox.Items.IndexOf(logFileItem);
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

        public override void OpenFile(object sender)
        {
            SetStatus("opening file");
            bool silent = (sender is string && !String.IsNullOrEmpty(sender as string)) ? true : false;

            string[] logNames;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "";
            dlg.DefaultExt = ".csv";
            dlg.Filter = "All Files (*.*)|*.*|Csv Files (*.csv)|*.csv|Text Files (*.txt)|*.txt";
            dlg.Multiselect = true;

            Nullable<bool> result = false;
            // Show open file dialog box
            if (silent)
            {
                result = true;
                logNames = new string[1] { (sender as string) };
            }
            else
            {
                result = dlg.ShowDialog();
                logNames = dlg.FileNames;
            }

            if(result != true)
            {
                return;
            }

            foreach (string logName in logNames)
            {
                // Process open file dialog box results
                if (File.Exists(logName))
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
                        // send empty function to reset to current filter in filterview this.FilterLogTabItems(FilterCommand.Reset);
                        this.FilterLogTabItems(FilterCommand.Filter);
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

        public override void RenameTabItem(string logName)
        {
            // rename tab
            ITabViewModel<LogFileItem> tabItem = this.TabItems[SelectedIndex];
            Settings.RemoveLogFile(tabItem.Tag);
            tabItem.Tag = CurrentFile().Tag = logName;
            CurrentFile().FileName = tabItem.Header = tabItem.Name = Path.GetFileName(logName);
            Settings.AddFilterFile(logName);
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
                dlg.InitialDirectory = Path.GetDirectoryName(tabItem.Tag) ?? "";

                string fileName = Path.GetFileName(tabItem.Tag);
                if(!fileName.ToLower().Contains(".filtered"))
                {
                    fileName = string.Format("{0}.filtered{1}", Path.GetFileNameWithoutExtension(tabItem.Tag), Path.GetExtension(tabItem.Tag));
                }

                dlg.FileName = fileName;
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
                    this.ViewManager.SaveFile(logName, tabItem.ContentList);

                    // open filtered view into new tab if not a '-new x-' tab
                    if (string.Compare(tabItem.Tag, logName, true) != 0
                        &&  !Regex.IsMatch(tabItem.Tag, _tempTabNameFormatPattern))
                    {
                        AddTabItem(_logFileManager.NewFile(logName, tabItem.ContentList));
                        
                        
                    }
                    else
                    {
                        RenameTabItem(logName);
                    }
                }
            }
        }

        #endregion Public Methods

        private void _filterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // dont handle count updates
            SetStatus("_filterViewModel.PropertyChanged" + e.PropertyName);
            FilterLogTabItems();
        }

        private List<FilterFileItem> GetPreviousFilter()
        {
            return _previousFilterFileItems;
        }

        private bool IsHiding()
        {
           // if count the same then assume it is not filtered
           try
            {
                 ListBox listBox = (ListBox)this.CurrentTab().Viewer;
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
            catch (Exception e)
            {
                SetStatus("IsHiding:exception " + e.ToString());
                return false;
            }
        }

        private void LogViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetStatus("LogViewModel.PropertyChanged" + e.PropertyName);
            FilterLogTabItems();
        }

        private void SaveCurrentFilter(List<FilterFileItem> filterFileItems)
        {
            // save filter for next applyfilter compare
            _previousFilterFileItems.Clear();
            foreach (FilterFileItem item in filterFileItems)
            {
                _previousFilterFileItems.Add((FilterFileItem)item.ShallowCopy());
            }

            PreviousIndex = SelectedIndex;
        }
    }
}