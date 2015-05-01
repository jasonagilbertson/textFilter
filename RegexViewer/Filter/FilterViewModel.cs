using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace RegexViewer
{
    public class FilterViewModel : BaseViewModel<FilterFileItem>
    {
        #region Private Fields

        private Command _filterNotesCommand;

        private Command _insertFilterItemCommand;

        #endregion Private Fields

        #region Public Constructors

        public FilterViewModel()
        {
            this.TabItems = new ObservableCollection<ITabViewModel<FilterFileItem>>();
            this.TabItems.CollectionChanged += TabItems_CollectionChanged;
            this.ViewManager = new FilterFileManager();
            this.ViewManager.PropertyChanged += ViewManager_PropertyChanged;

            // load tabs from last session
            foreach (FilterFile logProperty in this.ViewManager.OpenFiles(this.Settings.CurrentFilterFiles.ToArray()))
            {
                AddTabItem(logProperty);
            }
        }

        #endregion Public Constructors

        #region Public Properties

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

        public ObservableCollection<WPFMenuItem> RecentCollection
        {
            get
            {
                return (RecentCollectionBuilder(Settings.RecentFilterFiles));
            }
        }

        public TabControl TabControl { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void AddTabItem(IFile<FilterFileItem> filterFile)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, filterFile.Tag, true) == 0))
            {
                SetStatus("adding tab:" + filterFile.Tag);
                FilterTabViewModel tabItem = new FilterTabViewModel()
                {
                    Name = filterFile.FileName,
                    ContentList = ((FilterFile)filterFile).ContentItems,
                    Tag = filterFile.Tag,
                    Header = filterFile.FileName,
                    Modified = false
                };

                // tabItem.ContentList.CollectionChanged += ContentList_CollectionChanged;
                tabItem.PropertyChanged += tabItem_PropertyChanged;
                TabItems.Add(tabItem);

                this.SelectedIndex = this.TabItems.Count - 1;
            }
        }

        public List<FilterFileItem> CleanFilterList(FilterFile filterFile)
        {
            List<FilterFileItem> fileItems = new List<FilterFileItem>();

            // clean up list
            foreach (FilterFileItem fileItem in filterFile.ContentItems.OrderBy(x => x.Index))
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
            FilterNeed retval = FilterNeed.Unknown;
            List<FilterFileItem> currentItems = this.FilterList();

            SetStatus("CompareFilterList: enter");

            if (previousFilterFileItems == null && currentItems != null)
            {
                retval = FilterNeed.Filter;
            }
            else if (currentItems == null)
            {
                retval = FilterNeed.ShowAll;
            }
            else if (currentItems.Count == 0)
            {
                retval = FilterNeed.ShowAll;
            }
            else if (currentItems.Count > 0
                && previousFilterFileItems.Count > 0
                && currentItems.Count == previousFilterFileItems.Count)
            {
                int i = 0;
                foreach (FilterFileItem fileItem in previousFilterFileItems.OrderBy(x => x.Index))
                {
                    FilterFileItem currentItem = currentItems[i++];
                    if (currentItem.Enabled != fileItem.Enabled
                        || currentItem.Exclude != fileItem.Exclude
                        || currentItem.Regex != fileItem.Regex
                        || currentItem.Filterpattern != fileItem.Filterpattern
                        || currentItem.CaseSensitive != currentItem.CaseSensitive)
                    {
                        retval = FilterNeed.Filter;

                        break;
                    }
                    else if (currentItem.BackgroundColor != fileItem.BackgroundColor
                        || currentItem.ForegroundColor != fileItem.ForegroundColor)
                    {
                        retval = FilterNeed.ApplyColor;
                        break;
                    }

                    retval = FilterNeed.Current;
                }
            }
            else
            {
                retval = FilterNeed.Filter;
            }

            SetStatus("CompareFilterList:returning:" + retval.ToString());
            return retval;
        }

        public List<FilterFileItem> FilterList()
        {
            List<FilterFileItem> filterFileItems = new List<FilterFileItem>();

            try
            {
                if (SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count)
                {
                    FilterFile filterFile = (FilterFile)CurrentFile();
                    if (filterFile != null)
                    {
                        return CleanFilterList(filterFile);
                    }
                }

                return filterFileItems;
            }
            catch (Exception e)
            {
                SetStatus("Exception:FilterTabItem:" + e.ToString());
                return filterFileItems;
            }
        }

        public void InsertFilterItemExecuted(object sender)
        {
            SetStatus("InsertFilterItemExecuted");
            FilterFile filterFile = (FilterFile)CurrentFile();
            if (filterFile != null)
            {
                ((FilterFileManager)this.ViewManager).ManageNewFilterFileItem(filterFile);
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
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Xml Files (*.xml)|*.xml|Tat Files (*.tat)|*.tat|All Files (*.*)|*.*";
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

        public override void RenameTabItem(string logName)
        {
            // rename tab
            ITabViewModel<FilterFileItem> tabItem = this.TabItems[SelectedIndex];
            Settings.RemoveFilterFile(tabItem.Tag);
            tabItem.Tag = CurrentFile().Tag = logName;
            CurrentFile().FileName = tabItem.Header = tabItem.Name = Path.GetFileName(logName);
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
                if (SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count)
                {
                    tabItem = (ITabViewModel<FilterFileItem>)this.TabItems[this.SelectedIndex];
                }
                else
                {
                    return;
                }
            }

            bool silent = (sender is string && !String.IsNullOrEmpty(sender as string)) ? true : false;

            string logName = string.Empty;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Xml Files (*.xml)|*.xml|Tat Files (*.tat)|*.tat|All Files (*.*)|*.*";

            dlg.InitialDirectory = Path.GetDirectoryName(tabItem.Tag) ?? Settings.FilterDirectory;
            dlg.FileName = Path.GetFileName(tabItem.Tag);

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

                SaveFileExecuted(null);
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
                if (SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count)
                {
                    tabItem = (ITabViewModel<FilterFileItem>)this.TabItems[this.SelectedIndex];
                }
                else
                {
                    // can get here by having no filters and hitting save file.
                    // todo: disable save file if no tab items
                    return;
                }
            }

            if (string.IsNullOrEmpty(tabItem.Tag) || Regex.IsMatch(tabItem.Tag, _tempTabNameFormatPattern, RegexOptions.IgnoreCase))
            {
                SaveFileAsExecuted(tabItem);
            }
            else
            {
                FilterFile file = (FilterFile)CurrentFile();
                if (file != null)
                {
                    file.ContentItems = tabItem.ContentList;
                    this.ViewManager.SaveFile(tabItem.Tag, file);
                }
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal bool VerifyAndOpenFile(string fileName)
        {
            SetStatus(string.Format("checking filter file:{0}", fileName));
            FilterFile filterFile = new FilterFile();
            if (String.IsNullOrEmpty((filterFile = (FilterFile)this.ViewManager.OpenFile(fileName)).Tag))
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
                FilterFile tempFilterFile = (FilterFile)this.ViewManager.ReadFile(filterFile.Tag);
                tempFilterFile.FilterNotes = filterFile.FilterNotes;

                // save only new notes to file.
                ((FilterFileManager)this.ViewManager).SaveFile(tempFilterFile.Tag, tempFilterFile);
            }
            else if(!filterFile.IsReadOnly & filterFile.IsNew)
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
            FilterFile filterFile = new FilterFile();

            try
            {
                filterFile = (FilterFile)CurrentFile();
                ObservableCollection<FilterFileItem> contentList = this.TabItems[SelectedIndex].ContentList;

                filterFile.EnablePatternNotifications(false);
                ObservableCollection<FilterFileItem> contentItems = filterFile.ContentItems;
                List<FilterFileItem> sortedFilterItems = new List<FilterFileItem>(contentItems.OrderBy(x => x.Index));
                SetStatus(string.Format("VerifyIndex: contentList count: {0} contentItems count: {1}",contentList.Count,contentItems.Count));
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
                        contentList[i] = sortedFilterItems[i];
                        contentItems[i] = sortedFilterItems[i];
                        contentList[i].Index = i;
                        contentItems[i].Index = i;
                        //sortedFilterItems[i].Index = i;
                    }

                    // sync contentitems
                    //filterFile.ContentItems = new ObservableCollection<FilterFileItem>(sortedFilterItems);
                }
            }
            catch (Exception ex)
            {
                SetStatus("VerifyIndex:exception:" + ex.ToString());
            }
            finally
            {
                filterFile.EnablePatternNotifications(true);
            }
        }

        private void ViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("FilterViewModel.ViewManager_ProperyChanged: " + e.PropertyName);

            if (sender is FilterFileItem)
            {

                FilterFile filterFile = (FilterFile)CurrentFile();
                if (filterFile != null)
                {
                    ((FilterFileManager)this.ViewManager).ManageNewFilterFileItem(filterFile);
                    VerifyIndex((sender as FilterFileItem));
                }
            }

            OnPropertyChanged(sender, e);

        }

        #endregion Private Methods
    }
}