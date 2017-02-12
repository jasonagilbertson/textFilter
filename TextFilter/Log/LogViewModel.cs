// ************************************************************************************
// Assembly: TextFilter
// File: LogViewModel.cs
// Created: 9/6/2016
// Modified: 2/12/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TextFilter
{
    public class LogViewModel : BaseViewModel<LogFileItem>
    {
        public static LogFileContentItems UpdateLogFile;

        private Command _exportCommand;

        private LogFileItem _filteredSelectedItem;

        private Command _keyDownCommand;

        private string _lineTotals;

        private LogFileManager _logFileManager;

        private Parser _parser;

        private List<FilterFileItem> _previousFilterFileItems = new List<FilterFileItem>();

        private ObservableCollection<WPFMenuItem> _recentCollection;

        private LogFileItem _unFilteredSelectedItem;

        public LogViewModel()
        {
            SetStatus("LogViewModel.ctor");

            TabItems = new ObservableCollection<ITabViewModel<LogFileItem>>();
            ViewManager = new LogFileManager();
            _logFileManager = (LogFileManager)ViewManager;

            // load tabs from last session
            AddTabItems(ViewManager.OpenFiles(Settings.CurrentLogFiles.ToArray()));
            _FilterViewModel.PropertyChanged += _FilterViewModel_PropertyChanged;
            PropertyChanged += LogViewModel_PropertyChanged;
            LogViewModel_PropertyChanged(this, new PropertyChangedEventArgs("Tab"));
        }

        public delegate void LogFileContentItems(LogFile logFile);

        public Command ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new Command(ExportExecuted);
                }
                _exportCommand.CanExecute = true;

                return _exportCommand;
            }
            set { _exportCommand = value; }
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

        public string LineTotals
        {
            get
            {
                return _lineTotals;
            }
            set
            {
                if (_lineTotals != value)
                {
                    _lineTotals = value;
                    OnPropertyChanged(LogViewModelEvents.LineTotals);
                }
            }
        }

        public Parser Parser
        {
            get
            {
                return _parser;
            }

            set
            {
                _parser = value;
            }
        }

        public ObservableCollection<WPFMenuItem> RecentCollection
        {
            get
            {
                return _recentCollection ?? RecentCollectionBuilder(Settings.RecentLogFiles);
            }

            set
            {
                _recentCollection = value ?? RecentCollectionBuilder(Settings.RecentLogFiles);
                OnPropertyChanged("RecentCollection");
            }
        }

        public LogTabViewModel SelectedTab
        {
            get
            {
                return (LogTabViewModel)TabItems[SelectedIndex];
            }
        }

        public void _FilterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // dont handle count updates
            SetStatus("LogViewModel.FilterViewModel.PropertyChanged: " + e.PropertyName);
            FilterLogTabItems();
        }

        public override void AddTabItem(IFile<LogFileItem> logFile)
        {
            if (!TabItems.Any(x => String.Compare((string)x.Tag, logFile.Tag, true) == 0))
            {
                SetStatus("adding tab:" + logFile.Tag);
                LogTabViewModel tabItem = new LogTabViewModel()
                {
                    Name = logFile.FileName,
                    Tag = logFile.Tag,
                    Header = logFile.FileName,
                    IsNew = logFile.IsNew
                };

                TabItems.Add(tabItem);

                SelectedIndex = TabItems.Count - 1;
            }
        }

        public override void ClearRecentExecuted()
        {
            Settings.RecentLogFiles = new string[0];
            UpdateRecentCollection();
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

        public void ExportExecuted(object sender)
        {
            try
            {
                SetStatus("export");

                // determine which fields and separator to use /save
                LogFile logFile = (LogFile)CurrentFile();
                ExportDialog exportDialog = new ExportDialog(logFile.ExportConfiguration);

                logFile.ExportConfiguration = exportDialog.WaitForResult();
                if (logFile.ExportConfiguration.Cancel)
                {
                    return;
                }
                if (logFile.ExportConfiguration.Copy)
                {
                    CurrentTab().CopyExecuted(logFile.ExportConfiguration);
                }
                else
                {
                    SaveFileAsExecuted(logFile);
                }
            }
            catch (Exception e)
            {
                SetStatus("ExportExecuted:exception" + e.ToString());
            }
        }

        public void FilterLogTabItems(FilterCommand filterIntent = FilterCommand.Filter)
        {
            try
            {
                bool processFilterIntent = true;
                List<FilterFileItem> filterFileItems = new List<FilterFileItem>(_FilterViewModel.FilterList());
                SetStatus(string.Format("filterLogTabItems:enter filterIntent: {0}", filterIntent));
                LogFile logFile;

                // get current log file
                if (_logFileManager.FileManager.Count > 0)
                {
                    logFile = (LogFile)CurrentFile();
                }
                else
                {
                    PreviousIndex = SelectedIndex;
                    return;
                }

                LogTabViewModel logTab = (LogTabViewModel)TabItems[SelectedIndex];

                // dont check filter need if intent is to reset list to current filter or to show all
                if (filterIntent == FilterCommand.Filter)
                {
                    FilterNeed filterNeed = _FilterViewModel.CompareFilterList(GetPreviousFilter());
                    SetStatus(string.Format("filterLogTabItems: filterNeed: {0}", filterNeed));

                    switch (filterNeed)
                    {
                        case FilterNeed.ApplyColor:
                            {
                                logTab.ContentList = _logFileManager.ApplyColor(logFile.ContentItems, filterFileItems);
                                processFilterIntent = false;
                                break;
                            }
                        case FilterNeed.Current:
                            {
                                if (PreviousIndex == SelectedIndex)
                                {
                                    SetStatus("filterLogTabItems:no change");
                                    processFilterIntent = false;
                                    break;
                                }

                                break;
                            }
                        case FilterNeed.ShowAll:
                            {
                                filterIntent = FilterCommand.ShowAll;
                                break;
                            }
                        case FilterNeed.Filter:
                            {
                                break;
                            }
                        case FilterNeed.Unknown:
                        default:
                            {
                                processFilterIntent = false;
                                SaveCurrentFilter(filterFileItems);
                                return;
                            }
                    }
                }

                if (processFilterIntent)
                {
                    SetCurrentStatus(CurrentStatusSetting.filtering);

                    switch (filterIntent)
                    {
                        case FilterCommand.Filter:
                            {
                                SetStatus(string.Format("switch:Filter: filterIntent:{0}", filterIntent));
                                logTab.ContentList = _logFileManager.ApplyColor(
                                    _logFileManager.ApplyFilter(
                                        logTab,
                                        logFile,
                                        filterFileItems,
                                        filterIntent),
                                    filterFileItems);
                                SetCurrentStatus(CurrentStatusSetting.filtered);
                                break;
                            }
                        case FilterCommand.Hide:
                            {
                                SetStatus(string.Format("switch:Hide: filterIntent:{0}", filterIntent));
                                logTab.ContentList = new ObservableCollection<LogFileItem>(logFile.ContentItems.Where(x => x.FilterIndex > -2));
                                SetCurrentStatus(CurrentStatusSetting.filtered);
                                break;
                            }
                        case FilterCommand.ShowAll:
                            {
                                SetStatus(string.Format("switch:ShowAll: filterIntent:{0}", filterIntent));
                                logTab.ContentList = logFile.ContentItems;
                                break;
                            }

                        case FilterCommand.Unknown:
                        default:
                            {
                                break;
                            }
                    }
                }

                // update line total counts
                LineTotals = string.Format("{0}/{1}", logTab.ContentList.Count, logFile.ContentItems.Count);

                // set current status message
                if (logTab.ContentList.Count == logFile.ContentItems.Count)
                {
                    SetCurrentStatus(CurrentStatusSetting.showing_all);
                }
                else if (_FilterViewModel.QuickFindItem.Enabled)
                {
                    SetCurrentStatus(CurrentStatusSetting.quick_filtered);
                }
                else
                {
                    SetCurrentStatus(CurrentStatusSetting.filtered);
                }

                SaveCurrentFilter(filterFileItems);
            }
            catch (Exception e)
            {
                SetStatus("FilterLogTabItems:exception" + e.ToString());
            }
        }

        public override void FindNextExecuted(object sender)
        {
            try
            {
                int filterIndex = -1;
                int index = 0;

                if (((Selector)CurrentTab().Viewer).SelectedItem != null)
                {
                    filterIndex = (int?)((LogFileItem)((Selector)CurrentTab().Viewer).SelectedItem).FilterIndex ?? 0;
                    index = (int?)((LogFileItem)((Selector)CurrentTab().Viewer).SelectedItem).Index ?? 0;
                    SetStatus(string.Format("LogViewModel.FindNextExecuted: setting log file index. filterindex: {0} index: {1} ", filterIndex, index));
                }

                if ((sender is int))
                {
                    filterIndex = Convert.ToInt32(sender);
                    SetStatus(string.Format("LogViewModel.FindNextExecuted: sender is filter. filterindex: {0} index: {1} ", filterIndex, index));
                }

                LogFileItem nextLogFileItem = CurrentFile().ContentItems.FirstOrDefault(
                        x => x.FilterIndex == filterIndex && x.Index > index);
                if (nextLogFileItem != null && nextLogFileItem.Index >= 0)
                {
                    SetStatus(string.Format("LogViewModel.FindNextExecuted: find next. filterindex: {0} index: {1} ", filterIndex, nextLogFileItem.Index));
                    GotoLineExecuted(nextLogFileItem.Index);
                }
                else
                {
                    // try from beginning
                    nextLogFileItem = CurrentFile().ContentItems.FirstOrDefault(
                        x => x.FilterIndex == filterIndex && x.Index >= 0);
                    if (nextLogFileItem != null && nextLogFileItem.Index >= 0)
                    {
                        SetStatus(string.Format("LogViewModel.FindNextExecuted: find first. filterindex: {0} index: {1} ", filterIndex, nextLogFileItem.Index));
                        GotoLineExecuted(nextLogFileItem.Index);
                    }
                    else
                    {
                        SetStatus(string.Format("LogViewModel.FindNextExecuted: not found! filterindex: {0} index: {1} ", filterIndex, index));
                        SetStatus(string.Format("QuickFindItem: filter pattern: {0} include: {1} exclude: {2}",
                            _FilterViewModel.QuickFindItem.Filterpattern,
                            _FilterViewModel.QuickFindItem.Include,
                            _FilterViewModel.QuickFindItem.Exclude));
                        foreach (FilterFileItem item in _FilterViewModel.FilterList())
                        {
                            SetStatus(string.Format("file item:{0}:{1}:{2}", item.Index, item.Filterpattern, item.Exclude, item.Include));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SetStatus("LogViewModel.FindNextExecuted:exception" + e.ToString());
            }
        }

        public override void GotoLineExecuted(object sender)
        {
            try
            {
                SetStatus("gotoLine");
                int index = 0;

                if ((sender is int))
                {
                    index = Convert.ToInt32(sender);
                }

                if (index == 0)
                {
                    index = SelectedTab.SelectedIndex;
                }

                if (sender == null)
                {
                    // display dialog to get index
                    GotoLineDialog gotoDialog = new GotoLineDialog(index);
                    index = gotoDialog.WaitForResult();
                }

                SetStatus("gotoLine:" + index.ToString());
                DataGrid dataGrid = (DataGrid)CurrentTab().Viewer;

                if (dataGrid != null && index >= 0)
                {
                    // try without unhiding
                    LogFileItem logFileItem = CurrentFile().ContentItems.FirstOrDefault(x => x.Index == index);

                    if (logFileItem == null | logFileItem != null && logFileItem.FilterIndex < -1 && IsHiding())
                    {
                        HideExecuted(null);
                        logFileItem = CurrentFile().ContentItems.FirstOrDefault(x => x.Index == index);
                    }

                    dataGrid.ScrollIntoView(logFileItem);

                    dataGrid.SelectedItem = logFileItem;
                    dataGrid.SelectedIndex = dataGrid.Items.IndexOf(logFileItem);
                }
            }
            catch (Exception e)
            {
                SetStatus("GotoLineExecuted:exception" + e.ToString());
            }
        }

        public void GroomFiles()
        {
            // check if recent files are still valid
            foreach (string file in (new List<string>(Settings.RecentLogFiles).ToList()))
            {
                if (!File.Exists(file))
                {
                    List<string> recent = Settings.RecentLogFiles.ToList();
                    recent.Remove(file);
                    Settings.RecentLogFiles = recent.ToArray();
                }
            }

            UpdateRecentCollection();
        }

        public override void HideExecuted(object sender)
        {
            try
            {
                SetStatus("HideExecuted:enter");
                DataGrid dataGrid = (DataGrid)CurrentTab().Viewer;
                LogFileItem logFileItem;

                // if count the same then assume it is not filtered
                if (!IsHiding())
                {
                    logFileItem = _unFilteredSelectedItem = (LogFileItem)dataGrid.SelectedItem;
                    FilterLogTabItems(FilterCommand.Hide);
                }
                else
                {
                    logFileItem = _filteredSelectedItem = (LogFileItem)dataGrid.SelectedItem;
                    FilterLogTabItems(FilterCommand.ShowAll);
                }

                if (dataGrid != null)
                {
                    if (dataGrid.Items.Contains(logFileItem))
                    {
                    }
                    else if (dataGrid.Items.Contains(_unFilteredSelectedItem))
                    {
                        logFileItem = _unFilteredSelectedItem;
                    }
                    else if (dataGrid.Items.Contains(_filteredSelectedItem))
                    {
                        logFileItem = _filteredSelectedItem;
                    }
                    else
                    {
                        return;
                    }

                    SetStatus("hiding:scrollingintoview:");
                    dataGrid.ScrollIntoView(logFileItem);
                    dataGrid.SelectedItem = logFileItem;
                    dataGrid.SelectedIndex = dataGrid.Items.IndexOf(logFileItem);
                    DataGridRow selectedRow = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
                    dataGrid.Focus();
                    FocusManager.SetIsFocusScope(selectedRow, true);
                    FocusManager.SetFocusedElement(selectedRow, selectedRow);
                }
            }
            catch (Exception e)
            {
                SetStatus("hiding:exception:" + e.ToString());
            }
        }

        public bool IsHiding()
        {
            // if count the same then assume it is not filtered
            try
            {
                DataGrid dataGrid = (DataGrid)CurrentTab().Viewer;
                SetStatus(string.Format("IsHiding:listBox.Items.Count:{0} CurrentFile().ContentItems.Count:{1}", dataGrid.Items.Count, CurrentFile().ContentItems.Count));
                if (dataGrid.Items.Count == CurrentFile().ContentItems.Count)
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

        public override void NewFileExecuted(object sender)
        {
            LogFile file = new LogFile();

            string tempTag = GenerateTempTagName();

            if (IsValidTabIndex())
            {
                file = (LogFile)ViewManager.NewFile(tempTag, TabItems[SelectedIndex].ContentList);
            }
            else
            {
                file = (LogFile)ViewManager.NewFile(tempTag);
            }

            AddTabItem(file);
            UpdateRecentCollection();
        }

        public override void OpenFileExecuted(object sender)
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

            if (result != true)
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
                    if (String.IsNullOrEmpty((logFile = (LogFile)ViewManager.OpenFile(logName)).Tag))
                    {
                        return;
                    }

                    // make new tab
                    AddTabItem(logFile);
                }
            }

            UpdateRecentCollection();
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

        public override void PasteText(object sender)
        {
            SetStatus("paste text");
            string rawText = System.Windows.Clipboard.GetText();
            if (string.IsNullOrEmpty(rawText))
            {
                return;
            }

            LogFile tempLogFile = new LogFile();

            ObservableCollection<LogFileItem> logFileItems = new ObservableCollection<LogFileItem>();
            int index = 0;
            foreach (string line in rawText.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                LogFileItem logFileItem = new LogFileItem();
                logFileItem.Index = ++index;
                logFileItem.Content = line;
                logFileItem.Background = Settings.BackgroundColor;
                logFileItem.Foreground = Settings.ForegroundColor;
                logFileItems.Add(logFileItem);
            }

            // save pasted text to a file first then add empty tab event will populate tab from temp
            // file tmp2606.tmp in user %temp%
            string tempFilePath = Path.GetTempFileName();
            tempLogFile.Tag = tempFilePath;
            tempLogFile.ContentItems = logFileItems;
            _logFileManager.SaveFile(tempFilePath, tempLogFile);

            // temp file name from paste text is being sent and has already been saved but needs new temp name
            IFile<LogFileItem> file = _logFileManager.OpenFile(tempFilePath);

            // set to new as save file sets to false and this needs to be set to verify temp file
            // on modified file close
            //file.IsNew = true;
            file.Modified = true;
            // change temp file name to generic -new ##- name only and not tag
            file.FileName = GenerateTempTagName();
            AddTabItem(file);
        }

        public override void RenameTabItem(string logName)
        {
            // rename tab
            ITabViewModel<LogFileItem> tabItem = TabItems[SelectedIndex];
            Settings.RemoveLogFile(tabItem.Tag);
            if (CurrentFile() != null)
            {
                tabItem.Tag = CurrentFile().Tag = logName;
                CurrentFile().FileName = tabItem.Header = tabItem.Name = Path.GetFileName(logName);
            }
            else
            {
                SetStatus("RenameTabItem:error: current file is null: " + logName);
            }

            Settings.AddLogFile(logName);
        }

        public override void SaveFileAsExecuted(object sender)
        {
            bool exportConfg = false;
            ITabViewModel<LogFileItem> tabItem;

            LogFile logFile = new LogFile();

            if (sender is LogFile)
            {
                // export configuration uses this
                exportConfg = true;
                logFile = sender as LogFile;
            }

            if (sender is TabItem)
            {
                tabItem = (ITabViewModel<LogFileItem>)(sender as TabItem);
            }
            else
            {
                if (IsValidTabIndex())
                {
                    tabItem = (ITabViewModel<LogFileItem>)TabItems[SelectedIndex];
                }
                else
                {
                    return;
                }
            }

            if (string.IsNullOrEmpty(tabItem.Tag))
            {
                TabItems.Remove(tabItem);
            }
            else
            {
                bool silent = (sender is string && !String.IsNullOrEmpty(sender as string)) ? true : false;

                string logName = string.Empty;
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                dlg.Filter = "All Files (*.*)|*.*|Csv Files (*.csv)|*.csv";
                dlg.InitialDirectory = Path.GetDirectoryName(tabItem.Tag) ?? "";

                string extension = string.IsNullOrEmpty(Path.GetExtension(tabItem.Tag)) ? ".csv" : Path.GetExtension(tabItem.Tag);
                string fileName = Path.GetFileNameWithoutExtension(tabItem.Tag) + extension;

                if (!fileName.ToLower().Contains(".filtered"))
                {
                    fileName = string.Format("{0}.filtered{1}", Path.GetFileNameWithoutExtension(tabItem.Tag), extension);
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
                    if (exportConfg)
                    {
                        // export configuration uses this
                        logFile = sender as LogFile;
                    }
                    else
                    {
                        logFile.ContentItems = tabItem.ContentList;
                    }

                    tabItem.IsNew = false;
                    ViewManager.SaveFile(logName, logFile);

                    // open filtered view into new tab if not a '-new ##-' tab
                    if (string.Compare(tabItem.Tag, logName, true) != 0
                        && !Regex.IsMatch(tabItem.Tag, _tempTabNameFormatPattern, RegexOptions.IgnoreCase))
                    {
                        if (!exportConfg)
                        {
                            AddTabItem(_logFileManager.NewFile(logName, tabItem.ContentList));
                        }
                    }
                    else
                    {
                        RenameTabItem(logName);
                    }
                }
            }
        }

        public override void SaveFileExecuted(object sender)
        {
            ITabViewModel<LogFileItem> tabItem;

            if (sender is TabItem)
            {
                tabItem = (ITabViewModel<LogFileItem>)(sender as TabItem);
            }
            else
            {
                if (IsValidTabIndex())
                {
                    tabItem = (ITabViewModel<LogFileItem>)TabItems[SelectedIndex];
                }
                else
                {
                    // can get here by having no filters and hitting save file.
                    // todo: disable save file if no tab items
                    return;
                }
            }

            SetStatus(string.Format("LogViewModel.SaveFileExecuted:header: {0} tag: {1}", tabItem.Header, tabItem.Tag, tabItem.Name));

            //if (string.IsNullOrEmpty(tabItem.Tag) || Regex.IsMatch(tabItem.Tag, _tempTabNameFormatPattern, RegexOptions.IgnoreCase))
            if (tabItem.IsNew)
            {
                // if saving new file
                SaveFileAsExecuted(tabItem);
            }
            else
            {
                LogFile file = (LogFile)CurrentFile();
                if (file != null)
                {
                    tabItem.IsNew = false;
                    file.ContentItems = tabItem.ContentList;
                    ViewManager.SaveFile(tabItem.Tag, file);
                }
            }
        }

        private List<FilterFileItem> GetPreviousFilter()
        {
            SetStatus("GetPreviousFilter item count: " + _previousFilterFileItems.Count);
            return _previousFilterFileItems;
        }

        private void LogViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetStatus("LogViewModel.PropertyChanged: " + e.PropertyName);

            // dont filter on form updates
            if (e.PropertyName == LogViewModelEvents.LineTotals
                | e.PropertyName == LogTabViewModel.LogTabViewModelEvents.Group1Visibility
                | e.PropertyName == LogTabViewModel.LogTabViewModelEvents.Group2Visibility
                | e.PropertyName == LogTabViewModel.LogTabViewModelEvents.Group3Visibility
                | e.PropertyName == LogTabViewModel.LogTabViewModelEvents.Group4Visibility)
            {
                return;
            }

            SetStatus("LogViewModel.PropertyChanged Passing to filter: " + e.PropertyName);
            FilterLogTabItems();
        }

        private void SaveCurrentFilter(List<FilterFileItem> filterFileItems)
        {
            // save filter for next applyfilter compare shallow copy keeps color

            _previousFilterFileItems.Clear();
            foreach (FilterFileItem item in filterFileItems)
            {
                _previousFilterFileItems.Add((FilterFileItem)item.ShallowCopy());
            }
            SetStatus("SaveCurrentFilter item count: " + _previousFilterFileItems.Count);
            PreviousIndex = SelectedIndex;
        }

        private void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("_FilterViewModel.CollectionChanged: " + sender.ToString());
            FilterLogTabItems();
        }

        private void UpdateRecentCollection()
        {
            // setting to null forces refresh
            RecentCollection = null;
        }

        public struct LogViewModelEvents
        {
            public static string LineTotals = "LineTotals";
        }
    }
}