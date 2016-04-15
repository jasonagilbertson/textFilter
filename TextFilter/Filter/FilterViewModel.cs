// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-13-2015 ***********************************************************************
// <copyright file="FilterViewModel.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TextFilter
{
    public class FilterViewModel : BaseViewModel<FilterFileItem>
    {
        #region Private Fields

        private Command _addFilterItemCommand;
        private string _filterHide;
        private Command _filterNotesCommand;
        private bool _quickFindAnd;
        private int _quickFindIndex;
        private ObservableCollection<ListBoxItem> _quickFindList;
        private bool _quickFindNot;
        private bool _quickFindOr;
        private bool _quickFindRegex;
        private SpinLock _spinLock = new SpinLock();

        #endregion Private Fields

        #region Public Constructors

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

        #endregion Public Constructors

        #region Public Properties

        public Command AddFilterItemCommand
        {
            get
            {
                if (_addFilterItemCommand == null)
                {
                    _addFilterItemCommand = new Command(InsertFilterItemExecuted);
                }
                _addFilterItemCommand.CanExecute = true;

                return _addFilterItemCommand;
            }
            set
            {
                _addFilterItemCommand = value;
            }
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
                    //if (_filterHide == "0")
                    //{
                    //   // FilterHide = "0";
                    //    Settings.FilterHide = true;
                    //}
                    //else
                    //{
                    //  //  FilterHide = "25*";
                    //    Settings.FilterHide = false;
                    //}

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

        public ObservableCollection<WPFMenuItem> RecentCollection
        {
            get
            {
                return (RecentCollectionBuilder(Settings.RecentFilterFiles));
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


        public ObservableCollection<MenuItem> SharedCollection
        {
            get
            {
                if (_sharedCollection == null)
                {
                    _sharedCollection = Menubuilder(Settings.SharedFilterDirectory);
                }

                return _sharedCollection;
            }
            set
            {
                _sharedCollection = value;
            }
        }

        public TabControl TabControl { get; set; }

        #endregion Public Properties

        #region Public Methods

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
                filterIndex = ((Selector)CurrentTab().Viewer).SelectedIndex;

                ((FilterFileManager)ViewManager).ManageFilterFileItem(filterFile, filterIndex);
                VerifyIndex();
            }
        }

        public override void OpenFileExecuted(object sender)
        {
            bool silent = (sender is string && !String.IsNullOrEmpty(sender as string)) ? true : false;
            if (sender is string && !String.IsNullOrEmpty(sender as string))
            {
                silent = true;
            }

            string[] logNames;
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
                    SetStatus(string.Format("opening file:{0}", logName));
                    VerifyAndOpenFile(logName);
                }
            }
        }

        public override void PasteText(object sender)
        {
            SetStatus("paste text");
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

        #endregion Public Methods

        #region Internal Methods

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

            AddTabItem(filterFile);

            return true;
        }

        #endregion Internal Methods

        #region Private Methods

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
                        //sortedFilterItems[i].Index = i;
                    }

                    // sync contentitems
                    //filterFile.ContentItems = new ObservableCollection<FilterFileItem>(sortedFilterItems);
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
                }
            }

            OnPropertyChanged(sender, e);
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

        #endregion Private Methods

        private Command _quickFindChangedCommand;

        private FilterFileItem _quickFindItem = new FilterFileItem() { Index = -1 };
        private Command _removeFilterItemCommand;
        private ObservableCollection<MenuItem> _sharedCollection;

        public LogViewModel _LogViewModel { get; internal set; }

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

        public void QuickFindChangedExecuted(object sender)
        {
            SetStatus(string.Format("quickfindchangedexecuted:enter: {0}", (sender is ComboBox)));
            if (sender is ComboBox)
            {
                ComboBox comboBox = (sender as ComboBox);
                QuickFindItem.Filterpattern = (sender as ComboBox).Text;
                bool foundItem = string.IsNullOrEmpty(QuickFindItem.Filterpattern);
                foreach (ComboBoxItem item in comboBox.Items)
                {
                    if ((string)item.Content == QuickFindItem.Filterpattern)
                    {
                        foundItem = true;
                        break;
                    }
                }

                if (!foundItem)
                {
                    comboBox.Items.Insert(0, new ComboBoxItem()
                    {
                        Content = QuickFindItem.Filterpattern,
                        Background = Settings.BackgroundColor,
                        Foreground = Settings.ForegroundColor,
                        BorderBrush = Settings.BackgroundColor
                    });
                }

                
                SetStatus(string.Format("quickfindchangedexecuted:string.length: {0}", QuickFindItem.Filterpattern.Length));
                if (string.IsNullOrEmpty(QuickFindItem.Filterpattern))
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
                else
                {
                    QuickFindItem.Enabled = true;
                }

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

            //if (!QuickFindAnd && !QuickFindOr && !QuickFindNot)
            //{
            //    foreach (FilterFileItem filterItem in FilterList())
            //    {
            //        filterItem.Count = 0;
            //    }
            //}

            // send filter request
            _LogViewModel.FilterLogTabItems(FilterCommand.Filter);
        }
    }
}