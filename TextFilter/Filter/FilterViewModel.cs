// ************************************************************************************
// Assembly: TextFilter
// File: FilterViewModel.cs
// Created: 3/19/2017
// Modified: 3/28/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TextFilter
{
    public class FilterViewModel : BaseViewModel<FilterFileItem>
    {
        private string _filterHide;
        private Command _filterNotesCommand;
        private Command _insertFilterItemCommand;
        private Command _insertFilterItemFromTextCommand;
        private Command _newFilterFromTextCommand;
        private bool _quickFindAnd;
        private Command _quickFindChangedCommand;
        private int _quickFindIndex;
        private FilterFileItem _quickFindItem = new FilterFileItem() { Index = -1 };
        private Command _quickFindKeyPressCommand;
        private ObservableCollection<ListBoxItem> _quickFindList = new ObservableCollection<ListBoxItem>();
        private bool _quickFindNot;
        private bool _quickFindOr;
        private bool _quickFindRegex;
        private string _quickFindText = string.Empty;
        private Command _quickFindTextCommand;
        private ObservableCollection<WPFMenuItem> _recentCollection;
        private Command _removeFilterItemCommand;
        private ObservableCollection<MenuItem> _sharedCollection;
        private SpinLock _spinLock = new SpinLock();

        public FilterViewModel()
        {
            TabItems = new ObservableCollection<ITabViewModel<FilterFileItem>>();

            ViewManager = new FilterFileManager();

            // load tabs from last session
            AddTabItems(ViewManager.OpenFiles(Settings.CurrentFilterFiles.ToArray()));

            TabItems.CollectionChanged += TabItems_CollectionChanged;
            ViewManager.PropertyChanged += ViewManager_PropertyChanged;
            ViewManager_PropertyChanged(this, new PropertyChangedEventArgs("Tab"));
        }

        public string FilterHide
        {
            get
            {
                if (string.IsNullOrEmpty(_filterHide))
                {
                    if (Settings.FilterHide)
                    {
                        _filterHide = "0";
                    }
                    else
                    {
                        _filterHide = "25*";
                    }
                }

                return _filterHide;
            }

            set
            {
                if (_filterHide != value)
                {
                    _filterHide = value;
                    OnPropertyChanged("FilterHide");
                }
            }
        }

        public Command FilterNotesCommand
        {
            get
            {
                if (_filterNotesCommand == null)
                {
                    _filterNotesCommand = new Command(FilterNotesExecuted);
                }
                _filterNotesCommand.CanExecute = true;

                return _filterNotesCommand;
            }
            set { _filterNotesCommand = value; }
        }

        public Command InsertFilterItemCommand
        {
            get
            {
                if (_insertFilterItemCommand == null)
                {
                    _insertFilterItemCommand = new Command(InsertFilterItemExecuted);
                }
                _insertFilterItemCommand.CanExecute = true;

                return _insertFilterItemCommand;
            }
            set
            {
                _insertFilterItemCommand = value;
            }
        }

        public Command InsertFilterItemFromTextCommand
        {
            get
            {
                if (_insertFilterItemFromTextCommand == null)
                {
                    _insertFilterItemFromTextCommand = new Command(InsertFilterItemFromTextExecuted);
                }
                _insertFilterItemFromTextCommand.CanExecute = true;

                return _insertFilterItemFromTextCommand;
            }
            set
            {
                _insertFilterItemFromTextCommand = value;
            }
        }

        public Command NewFromTextCommand
        {
            get
            {
                if (_newFilterFromTextCommand == null)
                {
                    _newFilterFromTextCommand = new Command(NewFilterFromTextExecuted);
                }
                _newFilterFromTextCommand.CanExecute = true;

                return _newFilterFromTextCommand;
            }
            set
            {
                _newFilterFromTextCommand = value;
            }
        }

        public bool QuickFindAnd
        {
            get
            {
                return _quickFindAnd;
            }
            set
            {
                if (_quickFindAnd != value)
                {
                    _quickFindAnd = value;
                    if (_quickFindAnd)
                    {
                        QuickFindOr = false;
                        QuickFindNot = false;
                        QuickFindItem.Include = true;
                        QuickFindItem.Exclude = true;
                    }
                    else
                    {
                        QuickFindItem.Include = false;
                        QuickFindItem.Exclude = false;
                    }
                    OnPropertyChanged("QuickFindAnd");
                }
            }
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

        public ComboBox QuickFindCombo { get; private set; }

        public int QuickFindIndex
        {
            get
            {
                return _quickFindIndex;
            }
            set
            {
                if (_quickFindIndex != value)
                {
                    _quickFindIndex = value;
                    OnPropertyChanged("QuickFindIndex");
                }
            }
        }

        public FilterFileItem QuickFindItem
        {
            get
            {
                return _quickFindItem;
            }
            set
            {
                if (_quickFindItem != value)
                {
                    _quickFindItem = value;
                    // OnPropertyChanged("QuickFindItem");
                }
            }
        }

        public Command QuickFindKeyPressCommand
        {
            get
            {
                if (_quickFindKeyPressCommand == null)
                {
                    _quickFindKeyPressCommand = new Command(QuickFindKeyPressExecuted);
                }
                _quickFindKeyPressCommand.CanExecute = true;

                return _quickFindKeyPressCommand;
            }
            set { _quickFindKeyPressCommand = value; }
        }

        public ObservableCollection<ListBoxItem> QuickFindList
        {
            get
            {
                return _quickFindList;
            }
            set
            {
                if (_quickFindList != value)
                {
                    _quickFindList = value;
                    OnPropertyChanged("QuickFindList");
                }
            }
        }

        public bool QuickFindNot
        {
            get
            {
                return _quickFindNot;
            }
            set
            {
                if (_quickFindNot != value)
                {
                    _quickFindNot = value;
                    if (_quickFindNot)
                    {
                        QuickFindAnd = false;
                        QuickFindOr = false;
                        QuickFindItem.Exclude = true;
                        QuickFindItem.Include = false;
                    }
                    else
                    {
                        QuickFindItem.Exclude = false;
                    }
                    OnPropertyChanged("QuickFindNot");
                }
            }
        }

        public bool QuickFindOr
        {
            get
            {
                return _quickFindOr;
            }
            set
            {
                if (_quickFindOr != value)
                {
                    _quickFindOr = value;

                    if (_quickFindOr)
                    {
                        QuickFindAnd = false;
                        QuickFindNot = false;
                        QuickFindItem.Include = true;
                        QuickFindItem.Exclude = false;
                    }
                    else
                    {
                        QuickFindItem.Include = false;
                    }
                    OnPropertyChanged("QuickFindOr");
                }
            }
        }

        public bool QuickFindRegex
        {
            get
            {
                return _quickFindRegex;
            }
            set
            {
                if (_quickFindRegex != value)
                {
                    _quickFindRegex = value;
                    OnPropertyChanged("QuickFindRegex");
                }
            }
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
                    OnPropertyChanged("QuickFindText");
                }
            }
        }

        public Command QuickFindTextCommand
        {
            get
            {
                if (_quickFindTextCommand == null)
                {
                    _quickFindTextCommand = new Command(QuickFindTextExecuted);
                }
                _quickFindTextCommand.CanExecute = true;

                return _quickFindTextCommand;
            }
            set { _quickFindTextCommand = value; }
        }

        public ObservableCollection<WPFMenuItem> RecentCollection
        {
            get
            {
                return _recentCollection ?? RecentCollectionBuilder(Settings.RecentFilterFiles);
            }

            set
            {
                _recentCollection = value ?? RecentCollectionBuilder(Settings.RecentFilterFiles);
                OnPropertyChanged("RecentCollection");
            }
        }

        public Command RemoveFilterItemCommand
        {
            get
            {
                if (_removeFilterItemCommand == null)
                {
                    _removeFilterItemCommand = new Command(RemoveFilterItemExecuted);
                }
                _removeFilterItemCommand.CanExecute = true;

                return _removeFilterItemCommand;
            }
            set
            {
                _removeFilterItemCommand = value;
            }
        }

        public ObservableCollection<MenuItem> SharedCollection
        {
            get
            {
                _sharedCollection = Menubuilder(Settings.SharedFilterDirectory);
                return _sharedCollection;
            }
            set
            {
                _sharedCollection = value;
                OnPropertyChanged("SharedCollection");
            }
        }

        public TabControl TabControl { get; set; }

        public override void AddTabItem(IFile<FilterFileItem> filterFile)
        {
            if (!TabItems.Any(x => String.Compare((string)x.Tag, filterFile.Tag, true) == 0))
            {
                SetStatus("adding tab:" + filterFile.Tag);
                FilterTabViewModel tabItem = new FilterTabViewModel()
                {
                    Name = filterFile.FileName,
                    ContentList = ((FilterFile)filterFile).ContentItems,
                    Tag = filterFile.Tag,
                    Header = filterFile.FileName,
                    Modified = false,
                    IsNew = filterFile.IsNew
                };

                // tabItem.ContentList.CollectionChanged += ContentList_CollectionChanged;
                tabItem.PropertyChanged += tabItem_PropertyChanged;
                TabItems.Add(tabItem);

                SelectedIndex = TabItems.Count - 1;
            }
        }

        public List<FilterFileItem> CleanFilterList(List<FilterFileItem> filterFileItems)
        {
            List<FilterFileItem> fileItems = new List<FilterFileItem>();

            // clean up list
            foreach (FilterFileItem fileItem in filterFileItems.OrderBy(x => x.Index))
            {
                if (!fileItem.Enabled | string.IsNullOrEmpty(fileItem.Filterpattern))
                {
                    continue;
                }

                fileItems.Add(fileItem);
            }

            return fileItems;
        }

        public override void ClearRecentExecuted()
        {
            Settings.RecentFilterFiles = new string[0];
            UpdateRecentCollection();
        }

        public FilterNeed CompareFilterList(List<FilterFileItem> previousFilterFileItems)
        {
            SetStatus("CompareFilterList: enter");

            FilterNeed retval = FilterNeed.Unknown;
            List<FilterFileItem> currentItems = FilterList();
            List<FilterFileItem> previousItems = previousFilterFileItems;

            if (previousItems == null && currentItems != null)
            {
                SetStatus("CompareFilterList: previous null, current not null");
                retval = FilterNeed.Filter;
            }
            else if (currentItems == null)
            {
                SetStatus("CompareFilterList: current null");
                retval = FilterNeed.ShowAll;
            }
            else if (currentItems.Count == 0)
            {
                SetStatus("CompareFilterList: current count = 0");
                retval = FilterNeed.ShowAll;
            }
            else if (previousItems.Count == 0)
            {
                SetStatus("CompareFilterList: previous count = 0");
                retval = FilterNeed.Filter;
            }
            else if (currentItems.Count > 0
                && previousItems.Count > 0
                && currentItems.Count == previousItems.Count)
            {
                SetStatus("CompareFilterList: previous not null, current not null, count is same");
                int i = 0;
                foreach (FilterFileItem previousItem in previousItems)
                {
                    FilterFileItem currentItem = currentItems[i++];
                    if (currentItem.Enabled != previousItem.Enabled
                        | currentItem.Exclude != previousItem.Exclude
                        | currentItem.Include != previousItem.Include
                        | currentItem.Regex != previousItem.Regex
                        | currentItem.Filterpattern != previousItem.Filterpattern
                        | currentItem.CaseSensitive != previousItem.CaseSensitive
                        | currentItem.Index != previousItem.Index)
                    {
                        retval = FilterNeed.Filter;
                        SetStatus("CompareFilterList: there is a difference in filter items. setting to filter.");
                        break;
                    }
                    else if (currentItem.BackgroundColor != previousItem.BackgroundColor
                        || currentItem.ForegroundColor != previousItem.ForegroundColor)
                    {
                        retval = FilterNeed.ApplyColor;
                        SetStatus("CompareFilterList: there is a difference in filter item color. setting to apply color.");
                        break;
                    }

                    //SetStatus("CompareFilterList: there is no difference in each filter item. setting to current.");
                    retval = FilterNeed.Current;
                }
            }
            else if (currentItems.Count > 0
                && previousItems.Count > 0
                && currentItems.Count < previousItems.Count)
            {
                SetStatus("CompareFilterList: current items count less than previous items count");
                retval = FilterNeed.Filter;
            }
            else if (currentItems.Count > 0
                && previousItems.Count > 0
                && currentItems.Count > previousItems.Count)
            {
                SetStatus("CompareFilterList: current items count greater than previous items count");
                retval = FilterNeed.Filter;
            }
            else
            {
                SetStatus("CompareFilterList: unknown");
                retval = FilterNeed.Filter;
            }

            SetStatus(string.Format("CompareFilterList:returning:{0}, previous count:{1}, current count:{2}", retval, previousItems.Count, currentItems.Count));
            // WriteFilterList(currentItems, "current items"); WriteFilterList(previousItems,
            // "previous Items");

            return retval;
        }

        public List<FilterFileItem> FilterList()
        {
            List<FilterFileItem> filterFileItems = new List<FilterFileItem>();

            try
            {
                if (IsValidTabIndex())
                {
                    //FilterFile filterFile = (FilterFile)CurrentFile();
                    filterFileItems = new List<FilterFileItem>(CurrentFile().ContentItems);

                    if (!string.IsNullOrEmpty(QuickFindItem.Filterpattern) && !QuickFindItem.Include && !QuickFindItem.Exclude)
                    {
                        // then exclusive search
                        filterFileItems.Clear();
                    }
                }

                // insert quick filter if exists
                if (!string.IsNullOrEmpty(QuickFindItem.Filterpattern))
                {
                    filterFileItems.Insert(0, QuickFindItem);
                }

                if (QuickFindItem.Exclude && !string.IsNullOrEmpty(QuickFindItem.Filterpattern) && filterFileItems.Count == 1)
                {
                    // doing a not quick filter and no filter file is loaded.
                    // in this case, add a new wildcard filter entry to show all lines not matching the not quick filter
                    // without adding this, no output is shown when no filter is loaded
                    filterFileItems.Insert(1, new FilterFileItem()
                    {
                        Filterpattern = ".",
                        Regex = true,
                        Enabled = true
                    });
                }

                if (filterFileItems.Count > 0)
                {
                    return CleanFilterList(filterFileItems);
                }

                return filterFileItems;
            }
            catch (Exception e)
            {
                SetStatus("Exception:FilterTabItem:" + e.ToString());
                return filterFileItems;
            }
        }

        public void FilterLogExecuted()
        {
            // main form {enter}
            _LogViewModel.FilterLogTabItems(FilterCommand.Filter);
        }

        public override void FindNextExecuted(object sender)
        {
            try
            {
                SetStatus("FilterViewModel.FindNextExecuted: enter");
                // get clean index that has empty and disabled filters removed
                int cleanFilterIndex = -1;
                if (sender is string && (sender as string) == "QUICKFIND")
                {
                    SetStatus("FilterViewModel.FindNextExecuted: sender is quick find");
                    cleanFilterIndex = -1;
                }
                else if (((Selector)CurrentTab().Viewer) != null)
                {
                    cleanFilterIndex = (FilterList().FirstOrDefault(x => x == (FilterFileItem)((Selector)CurrentTab().Viewer).SelectedItem)).Index;
                }

                _LogViewModel.FindNextExecuted(cleanFilterIndex);
            }
            catch (Exception e)
            {
                SetStatus("findnextexecuted:exception" + e.ToString());
            }
        }

        public override void GotoLineExecuted(object sender)
        {
            FilterTabViewModel filterTab = (FilterTabViewModel)CurrentTab();
            LogTabViewModel logTab = (LogTabViewModel)_LogViewModel.CurrentTab();

            if (filterTab != null && logTab != null)
            {
                int filterIndex = -1;
                int logIndex = ((Selector)logTab.Viewer).SelectedIndex;

                if (logIndex <= logTab.ContentList.Count)
                {
                    filterIndex = logTab.ContentList[logIndex].FilterIndex;
                }
                else
                {
                    SetStatus("filter:gotoLine:error in index:" + filterIndex.ToString());
                    return;
                }

                if (filterIndex >= 0)
                {
                    SetStatus("filter:gotoLine:" + filterIndex.ToString());
                    ((Selector)filterTab.Viewer).SelectedIndex = filterIndex;

                    DataGrid dataGrid = (DataGrid)CurrentTab().Viewer;
                    FilterFileItem filterFileItem = CurrentFile().ContentItems.FirstOrDefault(x => x.Index == filterIndex);

                    dataGrid.ScrollIntoView(filterFileItem);
                    dataGrid.SelectedItem = filterFileItem;
                    dataGrid.SelectedIndex = dataGrid.Items.IndexOf(filterFileItem);
                    dataGrid.UpdateLayout();

                    DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
                    DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);

                    if (presenter != null)
                    {
                        DataGridCell cell = presenter.ItemContainerGenerator.ContainerFromIndex(0) as DataGridCell;

                        if (cell != null)
                        {
                            cell.Focus();
                        }
                    }
                }
                else if (filterIndex == -1)
                {
                    // quick filter
                    if (QuickFindCombo != null)
                    {
                        Keyboard.Focus(QuickFindCombo);
                    }
                }
            }
        }

        public void GroomFiles()
        {
            // check if recent filters are still valid
            foreach (string file in (new List<string>(Settings.RecentFilterFiles).ToList()))
            {
                if (!File.Exists(file))
                {
                    List<string> recent = Settings.RecentFilterFiles.ToList();
                    recent.Remove(file);
                    Settings.RecentFilterFiles = recent.ToArray();
                }
            }

            UpdateRecentCollection();
        }

        public override void HideExecuted(object sender)
        {
            try
            {
                SetStatus("FilterViewModel.HideExecuted:enter");
                Settings.FilterHide = !Settings.FilterHide;
                if (Settings.FilterHide)
                {
                    FilterHide = "0";
                }
                else
                {
                    FilterHide = "25*";
                }
            }
            catch (Exception e)
            {
                SetStatus("FilterViewModel.HideExecuted:exception:" + e.ToString());
            }
        }

        public void InsertFilterItemExecuted(object sender)
        {
            SetStatus("InsertFilterItemExecuted");
            FilterFile filterFile = (FilterFile)CurrentFile();

            if (filterFile != null)
            {
                int filterIndex = 0;
                if (CurrentTab().Viewer != null)
                {
                    filterIndex = ((Selector)CurrentTab().Viewer).SelectedIndex;
                }

                // add content from logfileitem to new filter
                if (sender is TextBox)
                {
                    filterIndex = filterFile.ContentItems.Max(x => x.Index);
                    TextBox textBox = (sender as TextBox);
                    FilterFileItem fileItem = new FilterFileItem()
                    {
                        Enabled = true,
                        Index = ++filterIndex,
                        Notes = textBox.Text,
                        Filterpattern = string.IsNullOrEmpty(textBox.SelectedText) ? textBox.Text : textBox.SelectedText
                    };

                    ((FilterFileManager)ViewManager).SetFilterItemColors(filterFile, fileItem);

                    filterFile.ContentItems.Add(fileItem);
                    // set filterindex to -1 to add new filter item at end of list
                    filterIndex = -1;
                }

                ((FilterFileManager)ViewManager).ManageFilterFileItem(filterFile, filterIndex);
                VerifyIndex();
            }
        }

        public override void NewFileExecuted(object sender)
        {
            FilterFile file = new FilterFile();
            string tempTag = GenerateTempTagName();

            if (IsValidTabIndex())
            {
                file = (FilterFile)ViewManager.NewFile(tempTag, TabItems[SelectedIndex].ContentList);
            }
            else
            {
                file = (FilterFile)ViewManager.NewFile(tempTag);
            }

            AddTabItem(file);
            VerifyIndex();
            UpdateRecentCollection();

        }

        public override void OpenFileExecuted(object sender)
        {
            bool silent = (sender is string && !String.IsNullOrEmpty(sender as string)) ? true : false;
            if (sender is string && !String.IsNullOrEmpty(sender as string))
            {
                silent = true;
            }

            string[] filterNames;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".rvf";
            dlg.Filter = "Filter Files (*.rvf;*.xml)|*.rvf;*.xml|Tat Files (*.tat)|*.tat|All Files (*.*)|*.*";
            dlg.InitialDirectory = Settings.FilterDirectory ?? "";
            dlg.Multiselect = true;

            Nullable<bool> result = false;

            // Show open file dialog box
            if (silent)
            {
                result = true;
                filterNames = new string[1] { (sender as string) };
            }
            else
            {
                result = dlg.ShowDialog();
                filterNames = dlg.FileNames;
            }

            if (result != true)
            {
                return;
            }

            foreach (string filterName in filterNames)
            {
                // Process open file dialog box results
                if (File.Exists(filterName))
                {
                    SetStatus(string.Format("opening filter:{0}", filterName));
                    VerifyAndOpenFile(filterName);
                }
            }

            UpdateRecentCollection();
        }

        public override void PasteText(object sender)
        {
            SetStatus("paste text");
        }

        public void QuickFindChangedExecuted(object sender)
        {
            SetStatus(string.Format("quickfindchangedexecuted:enter: {0}", (sender is ComboBox)));
            bool buttonStatus = (sender is Button);
            bool comboQuickFind = (sender is ComboBox);
            bool textBoxSelectedText = (sender is TextBox);

            // save combo if passed in
            if (comboQuickFind && QuickFindCombo == null)
            {
                QuickFindCombo = (sender as ComboBox);
            }

            if (comboQuickFind)
            {
                QuickFindItem.Filterpattern = (sender as ComboBox).Text;
            }
            else if (textBoxSelectedText)
            {
                TextBox textBox = sender as TextBox;
                string text = string.IsNullOrEmpty(textBox.SelectedText) ? textBox.Text : textBox.SelectedText;
                QuickFindText = text;
                QuickFindItem.Filterpattern = text;
            }

            bool foundItem = string.IsNullOrEmpty(QuickFindItem.Filterpattern);
            foreach (ListBoxItem item in QuickFindList)
            {
                if ((string)item.Content == QuickFindItem.Filterpattern)
                {
                    foundItem = true;
                    break;
                }
            }

            if (!foundItem)
            {
                QuickFindList.Insert(0, new ListBoxItem()
                {
                    Content = QuickFindItem.Filterpattern,
                    Background = Settings.BackgroundColor,
                    Foreground = Settings.ForegroundColor,
                    BorderBrush = Settings.BackgroundColor
                });
            }

            SetStatus(string.Format("quickfindchangedexecuted:string.length: {0}", QuickFindItem.Filterpattern.Length));
            if (!buttonStatus && string.IsNullOrEmpty(QuickFindItem.Filterpattern))
            {
                QuickFindItem.Enabled = false;

                if (FilterList().Count > 0)
                {
                    // send filter request
                    _LogViewModel.FilterLogTabItems(FilterCommand.Filter);
                }
                else
                {
                    // no filter show all
                    _LogViewModel.FilterLogTabItems(FilterCommand.ShowAll);
                }

                return;
            }
            else if (!buttonStatus)
            {
                QuickFindItem.Enabled = true;
            }

            if (_quickFindRegex)
            {
                try
                {
                    Regex test = new Regex(QuickFindItem.Filterpattern);
                    QuickFindItem.Regex = true;
                }
                catch
                {
                    SetStatus("quick find not a regex:" + QuickFindItem.Filterpattern);
                    QuickFindItem.Regex = false;
                }
            }
            else
            {
                QuickFindItem.Regex = false;
            }

            // status button toggle
            if (buttonStatus)
            {
                CurrentStatusSetting setting;
                object currentStatus = (sender as Button).Content;
                if (currentStatus != null && Enum.TryParse(currentStatus.ToString().ToLower().Replace(" ", "_"), out setting))
                {
                    switch (setting)
                    {
                        case CurrentStatusSetting.enter_to_filter:
                            _LogViewModel.FilterLogTabItems(FilterCommand.Filter);
                            break;

                        case CurrentStatusSetting.filtering:
                        case CurrentStatusSetting.filtered:
                        case CurrentStatusSetting.quick_filtered:
                        case CurrentStatusSetting.showing_all:
                            _LogViewModel.HideExecuted(null);
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    SetStatus("warning:unable to toggle status button");
                }
            }
            else
            {
                // send filter request
                _LogViewModel.FilterLogTabItems(FilterCommand.Filter);
            }
        }

        public void QuickFindTextExecuted(object sender)
        {
            TextBox textBox = new TextBox();
            if (sender is DataGrid)
            {
                textBox = TextBoxFromDataGrid(sender as DataGrid);
            }
            else if (sender is TextBox)
            {
                textBox = (sender as TextBox);
            }
            else
            {
                return;
            }

            QuickFindChangedExecuted(textBox);

        }

        public override void RenameTabItem(string logName)
        {
            // rename tab
            ITabViewModel<FilterFileItem> tabItem = TabItems[SelectedIndex];
            Settings.RemoveFilterFile(tabItem.Tag);

            if (CurrentFile() != null)
            {
                tabItem.Tag = CurrentFile().Tag = logName;
                CurrentFile().FileName = tabItem.Header = tabItem.Name = Path.GetFileName(logName);
            }
            else
            {
                SetStatus("RenameTabItem:error: current file is null: " + logName);
            }

            Settings.AddFilterFile(logName);
        }

        public override void SaveFileAsExecuted(object sender)
        {
            ITabViewModel<FilterFileItem> tabItem;

            if (sender is TabItem)
            {
                tabItem = (ITabViewModel<FilterFileItem>)(sender as TabItem);
            }
            else
            {
                if (IsValidTabIndex())
                {
                    tabItem = (ITabViewModel<FilterFileItem>)TabItems[SelectedIndex];
                }
                else
                {
                    return;
                }
            }

            bool silent = (sender is string && !String.IsNullOrEmpty(sender as string)) ? true : false;

            string logName = string.Empty;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".rvf";
            dlg.Filter = "Filter Files (*.rvf;*.xml)|*.rvf;*.xml|Tat Files (*.tat)|*.tat|All Files (*.*)|*.*";

            dlg.InitialDirectory = Path.GetDirectoryName(tabItem.Tag) ?? Settings.FilterDirectory;

            string extension = string.IsNullOrEmpty(Path.GetExtension(tabItem.Tag)) ? ".rvf" : Path.GetExtension(tabItem.Tag);
            string fileName = Path.GetFileNameWithoutExtension(tabItem.Tag) + extension;

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
                SetStatus(string.Format("saving file:{0}", logName));

                RenameTabItem(logName);

                // SaveFileExecuted(null);
                FilterFile file = (FilterFile)CurrentFile();
                if (file != null)
                {
                    tabItem.IsNew = false;
                    file.ContentItems = tabItem.ContentList;
                    ViewManager.SaveFile(tabItem.Tag, file);
                }
            }

            return;
        }

        public override void SaveFileExecuted(object sender)
        {
            ITabViewModel<FilterFileItem> tabItem;

            if (sender is TabItem)
            {
                tabItem = (ITabViewModel<FilterFileItem>)(sender as TabItem);
            }
            else
            {
                if (IsValidTabIndex())
                {
                    tabItem = (ITabViewModel<FilterFileItem>)TabItems[SelectedIndex];
                }
                else
                {
                    // can get here by having no filters and hitting save file.
                    // todo: disable save file if no tab items
                    return;
                }
            }

            SetStatus(string.Format("FilterViewModel.SaveFileExecuted:header: {0} tag: {1}", tabItem.Header, tabItem.Tag, tabItem.Name));

            //if (string.IsNullOrEmpty(tabItem.Tag) || Regex.IsMatch(tabItem.Tag, _tempTabNameFormatPattern, RegexOptions.IgnoreCase))
            if (tabItem.IsNew)
            {
                // if saving new file
                SaveFileAsExecuted(tabItem);
            }
            else
            {
                FilterFile file = (FilterFile)CurrentFile();
                if (file != null)
                {
                    tabItem.IsNew = false;
                    file.ContentItems = tabItem.ContentList;
                    ViewManager.SaveFile(tabItem.Tag, file);
                }
            }
        }

        internal bool VerifyAndOpenFile(string fileName)
        {
            SetStatus(string.Format("checking filter file:{0}", fileName));

            if (((FilterFileManager)ViewManager).FilterFileVersion(fileName) == FilterFileManager.FilterFileVersionResult.NotAFilterFile)
            {
                return false;
            }

            FilterFile filterFile = new FilterFile();
            if (String.IsNullOrEmpty((filterFile = (FilterFile)ViewManager.OpenFile(fileName)).Tag))
            {
                return false;
            }

            // parser will open file on event
            AddTabItem(filterFile);

            return true;
        }

        private void FilterNotesExecuted()
        {
            if (CurrentFile() == null)
            {
                return;
            }

            FilterFile filterFile = (FilterFile)CurrentFile();
            FilterNotesDialog dialog = new FilterNotesDialog(filterFile.FilterNotes);

            dialog.Title = string.Format("{0} version:{1}", filterFile.FileName, filterFile.FilterVersion);
            dialog.DialogCanSave = !filterFile.IsReadOnly;

            filterFile.FilterNotes = dialog.WaitForResult();

            if (!filterFile.IsReadOnly & !filterFile.IsNew)
            {
                FilterFile tempFilterFile = (FilterFile)ViewManager.ReadFile(filterFile.Tag);
                tempFilterFile.FilterNotes = filterFile.FilterNotes;
                tempFilterFile.Tag = filterFile.Tag;

                // save only new notes to file.
                ((FilterFileManager)ViewManager).SaveFile(tempFilterFile.Tag, tempFilterFile);
            }
            else if (!filterFile.IsReadOnly & filterFile.IsNew)
            {
                // save new notes to current filter
                filterFile.FilterNotes = filterFile.FilterNotes;
                filterFile.Modified = true;
            }
        }

        private void InsertFilterItemFromTextExecuted(object sender)
        {
            TextBox textBox = TextBoxFromSender(sender);

            if (CurrentFile() == null)
            {
                NewFilterFromTextExecuted(textBox);
            }
            else
            {
                InsertFilterItemExecuted(textBox);
            }
        }

        private void NewFilterFromTextExecuted(object sender)
        {
            TextBox textBox = TextBoxFromSender(sender);
            NewFileExecuted(sender);
            InsertFilterItemExecuted(textBox);
        }
        private void QuickFindKeyPressExecuted(object sender)
        {
            SetStatus(string.Format("quickfindKeyPressexecuted:enter: {0}", (sender is ComboBox)));

            if (QuickFindCombo != null)
            {
                if (QuickFindCombo.Text.Length > 0)
                {
                    if (QuickFindCombo.Text != QuickFindItem.Filterpattern)
                    {
                        SetCurrentStatus(CurrentStatusSetting.enter_to_filter);
                    }

                    QuickFindCombo.BorderBrush = ((SolidColorBrush)new BrushConverter().ConvertFromString("LightGreen"));
                    QuickFindCombo.BorderThickness = new Thickness(1.5);
                }
                else
                {
                    QuickFindCombo.BorderBrush = Settings.ForegroundColor;
                    QuickFindCombo.BorderThickness = new Thickness(1);
                    //QuickFindChangedExecuted(sender);
                }
            }
        }

        private void RemoveFilterItemExecuted(object sender)
        {
            SetStatus("RemoveeFilterItemExecuted");
            FilterFile filterFile = (FilterFile)CurrentFile();

            if (filterFile != null)
            {
                int filterIndex = 0;
                filterIndex = ((Selector)CurrentTab().Viewer).SelectedIndex;

                ((FilterFileManager)ViewManager).ManageFilterFileItem(filterFile, filterIndex, true);
                VerifyIndex();
            }
        }

        private void tabItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("filterViewModel:tabItem_PropertyChanged");
            OnPropertyChanged(e.PropertyName);
        }

        private void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("filterViewModel:TabItems_CollectionChanged");
            OnPropertyChanged("TabItems");
        }

        private TextBox TextBoxFromSender(object sender)
        {
            TextBox textBox = new TextBox();

            if (sender is DataGrid)
            {
                // from hotkey ctrl+shift+a and ctrl+shift+n
                textBox = TextBoxFromDataGrid(sender as DataGrid);
            }
            else if (sender is ComboBox)
            {
                // from quick filter hotkey ctrl+shift+a and ctrl+shift+n
                //textBox.Text = (sender as ComboBox).Text;
                textBox.SelectedText = (sender as ComboBox).Text;
            }
            else if (sender is TextBox)
            {
                // from selected text context menu in logfile view
                textBox = sender as TextBox;
            }

            return textBox;
        }
        private void UpdateRecentCollection()
        {
            // setting to null forces refresh
            RecentCollection = null;
        }

        private void VerifyIndex(FilterFileItem filterFileItem = null)
        {
            bool lockTaken = false;

            try
            {
                _spinLock.TryEnter(ref lockTaken);
                FilterFile filterFile = new FilterFile();
                filterFile = (FilterFile)CurrentFile();
                ObservableCollection<FilterFileItem> contentList = TabItems[SelectedIndex].ContentList;

                // filterFile.EnablePatternNotifications(false);
                ObservableCollection<FilterFileItem> contentItems = filterFile.ContentItems;
                List<FilterFileItem> sortedFilterItems = new List<FilterFileItem>(contentItems.OrderBy(x => x.Index));
                SetStatus(string.Format("VerifyIndex: contentList count: {0} contentItems count: {1}", contentList.Count, contentItems.Count));
                if (filterFileItem != null)
                {
                    SetStatus(string.Format("VerifyIndex: filterFileItem index: {0} pattern: {1}", filterFileItem.Index, filterFileItem.Filterpattern));
                }

                bool dupes = false;
                bool needsSorting = false;
                bool needsReIndexing = false;
                List<int> indexList = new List<int>();

                for (int i = 0; i < sortedFilterItems.Count; i++)
                {
                    int orderedFilterItemIndex = sortedFilterItems[i].Index;
                    if (orderedFilterItemIndex != contentItems[i].Index)
                    {
                        // original index does not equal sorted index
                        needsSorting = true;
                    }

                    if (!indexList.Contains(orderedFilterItemIndex))
                    {
                        // add filter item to temp list for compare
                        indexList.Add(orderedFilterItemIndex);
                    }
                    else
                    {
                        // item already exists in temp list based on filter index
                        dupes = true;
                    }

                    if (i != orderedFilterItemIndex)
                    {
                        needsReIndexing = true;
                    }
                }

                // does index need to be modified?
                if (!needsSorting && !dupes && !needsReIndexing)
                {
                    // do nothing
                    return;
                }
                else
                {
                    filterFile.EnablePatternNotifications(false);
                    // needs sorting or has dupes or needs reindexing
                    if (filterFileItem != null && sortedFilterItems.Count(x => x.Index == filterFileItem.Index) > 1)
                    {
                        // new / modifed filteritem index remove and insert selected item in list at
                        // lowest position in index of dupes
                        sortedFilterItems.RemoveAt(sortedFilterItems.IndexOf(filterFileItem));
                        sortedFilterItems.Insert((int)(sortedFilterItems.IndexOf(sortedFilterItems.First(x => x.Index == filterFileItem.Index))), filterFileItem);
                    }

                    // sync contentList
                    for (int i = 0; i < sortedFilterItems.Count; i++)
                    {
                        Debug.Print(string.Format("VerifyIndex:sync:sortedFilterItems index: {0} filterpattern: {1}", i, sortedFilterItems[i].Filterpattern));
                        Debug.Print(string.Format("VerifyIndex:sync:contentList index: {0} filterpattern: {1}", i, contentList[i].Filterpattern));
                        Debug.Print(string.Format("VerifyIndex:sync:contentItems index: {0} filterpattern: {1}", i, contentItems[i].Filterpattern));
                        contentList[i] = sortedFilterItems[i];
                        contentItems[i] = sortedFilterItems[i];
                        contentList[i].Index = i;
                        contentItems[i].Index = i;
                    }

                    filterFile.EnablePatternNotifications(true);
                }
            }
            catch (LockRecursionException)
            {
                SetStatus("VerifyIndex:reentrant:skipping");
            }
            catch (Exception ex)
            {
                SetStatus("VerifyIndex:exception:" + ex.ToString());
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit();
                }
            }
        }

        private void ViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("FilterViewModel.ViewManager_PropertyChanged: " + e.PropertyName);
            if (sender is FilterFileItem)
            {
                FilterFile filterFile = (FilterFile)CurrentFile();
                if (filterFile != null)
                {
                    ((FilterFileManager)ViewManager).ManageFilterFileItem(filterFile);
                    if (e.PropertyName == FilterFileItemEvents.Index)
                    {
                        VerifyIndex((sender as FilterFileItem));
                    }

                    SetCurrentStatus(CurrentStatusSetting.enter_to_filter);
                }
            }

            //OnPropertyChanged(sender, e);
        }

        private void WriteFilterList(List<FilterFileItem> currentItems, string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("WriteFilterList:{0}:{1}", name, currentItems.Count));

            foreach (FilterFileItem item in currentItems)
            {
                sb.AppendLine("\tindex:" + item.Index);
                sb.AppendLine("\tenabled:" + item.Enabled);
                sb.AppendLine("\texclude:" + item.Exclude);
                sb.AppendLine("\tregex:" + item.Regex);
                sb.AppendLine("\tfilterpattern:" + item.Filterpattern);
                sb.AppendLine("\tnotes:" + item.Notes);
            }

            SetStatus(sb.ToString());
        }
    }
}