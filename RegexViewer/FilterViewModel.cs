using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace RegexViewer
{
    public class FilterViewModel : BaseViewModel<FilterFileItem>
    {
        #region Private Fields

        private List<FilterFileItem> _previousFilterFileItems = new List<FilterFileItem>();
        private int _previousIndex = -1;
        private string _tempFilterNameFormat = "*new {0}*";
        private string _tempFilterNameFormatPattern = @"\*new [0-9]{1,2}\*";

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

            // doesnt work from here
            //VerifyIndex(null);
        }

        #endregion Public Constructors

        #region Public Methods

        public override void AddTabItem(IFile<FilterFileItem> logProperties)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                SetStatus("adding tab:" + logProperties.Tag);
                FilterTabViewModel tabItem = new FilterTabViewModel();
                tabItem.Name = logProperties.FileName;
                tabItem.ContentList = ((FilterFile)logProperties).ContentItems;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.FileName;
                tabItem.Modified = false;
                tabItem.PropertyChanged += tabItem_PropertyChanged;
                TabItems.Add(tabItem);
                this.SelectedIndex = this.TabItems.Count - 1;
            }
        }

        public List<FilterFileItem> CleanFilterList(FilterFile filterFile)
        {
            // todo: move to filter class
            List<FilterFileItem> fileItems = new List<FilterFileItem>();
            // clean up list
            foreach (FilterFileItem fileItem in filterFile.ContentItems.OrderBy(x => x.Index))
            {
                //fileItem.Count = 0;

                if (!fileItem.Enabled || string.IsNullOrEmpty(fileItem.Filterpattern))
                {
                    continue;
                }

                fileItems.Add(fileItem);
            }

            return fileItems;
        }

        public bool CompareFilterList(List<FilterFileItem> filterFileItems)
        {
            // todo: move to filter class
            bool retval = false;
            if (_previousFilterFileItems.Count > 0
                && filterFileItems.Count > 0
                && _previousFilterFileItems.Count == filterFileItems.Count)
            {
                int i = 0;
                foreach (FilterFileItem fileItem in filterFileItems.OrderBy(x => x.Index))
                {
                    FilterFileItem previousItem = _previousFilterFileItems[i++];
                    if (previousItem.BackgroundColor != fileItem.BackgroundColor
                        || previousItem.ForegroundColor != fileItem.ForegroundColor
                        || previousItem.Enabled != fileItem.Enabled
                        || previousItem.Exclude != fileItem.Exclude
                        || previousItem.Regex != fileItem.Regex
                        || previousItem.Filterpattern != fileItem.Filterpattern)
                    {
                        retval = false;
                        Debug.Print("returning false");
                        break;
                    }

                    retval = true;
                }
            }

            _previousFilterFileItems.Clear();
            foreach (FilterFileItem item in filterFileItems)
            {
                _previousFilterFileItems.Add((FilterFileItem)item.ShallowCopy());
            }

            Debug.Print("CompareFilterList:returning:" + retval.ToString());
            return retval;
        }

        public FilterCommand DetermineFilterAction(FilterCommand filterIntent = FilterCommand.Filter)
        {
            try
            {
                // Debug.Assert(TabItems != null & SelectedIndex != -1);
                List<FilterFileItem> filterFileItems = new List<FilterFileItem>();

                if (this.TabItems.Count > 0
                    && this.TabItems.Count >= SelectedIndex)
                {
                    FilterFile filterFile = CurrentFile();

                    filterFileItems = CleanFilterList(filterFile);

                    if (filterIntent == FilterCommand.DynamicFilter)
                    {
                        // reset previous filter list
                        CompareFilterList(filterFileItems);
                        return filterIntent;
                    }

                    if (filterIntent == FilterCommand.Reset)
                    {
                        // reset previous filter list
                        CompareFilterList(filterFileItems);
                    }

                    // return full list if no filters
                    if ((filterFileItems.Count == 0 | filterFileItems.Count(x => x.Enabled) == 0) & filterIntent == FilterCommand.Filter)
                    {
                        // reset colors

                        //this.TabItems[SelectedIndex].ContentList = ResetColors(logFile.ContentItems);

                        // reset previous filter list
                        CompareFilterList(filterFileItems);

                        return FilterCommand.Reset;
                    }

                    // return if nothing changed
                    if (_previousIndex == this.SelectedIndex & CompareFilterList(filterFileItems) & filterIntent == FilterCommand.Filter)
                    {
                        return FilterCommand.Current;
                    }
                    else if (_previousIndex != this.SelectedIndex)
                    {
                        _previousIndex = SelectedIndex;
                    }

                    // apply filter
                    return FilterCommand.Filter;
                }
                else
                {
                    // return unfiltered
                    return FilterCommand.Current;
                }
            }
            catch (Exception e)
            {
                SetStatus("Exception:FilterTabItem:" + e.ToString());
                return FilterCommand.Unknown;
            }
        }

        public List<FilterFileItem> FilterList(FilterFileItem fileItem = null)
        {
            // Debug.Assert(TabItems != null & SelectedIndex != -1);
            List<FilterFileItem> filterFileItems = new List<FilterFileItem>();

            try
            {
                if (fileItem == null
                    && this.TabItems.Count > 0
                    && this.TabItems.Count >= SelectedIndex)
                {
                    FilterFile filterFile = (FilterFile)this.ViewManager.FileManager.First(
                        x => x.Tag == this.TabItems[SelectedIndex].Tag);

                    return CleanFilterList(filterFile);
                }
                else
                {
                    filterFileItems.Add(fileItem);
                }

                return filterFileItems;
            }
            catch (Exception e)
            {
                SetStatus("Exception:FilterTabItem:" + e.ToString());
                return filterFileItems;
            }
        }

        public override void NewFile(object sender)
        {
            FilterFile filterFile = new FilterFile();
            // add temp name
            for (int i = 0; i < 100; i++)
            {
                string tempTag = string.Format(_tempFilterNameFormat, i);
                if (this.TabItems.Any(x => String.Compare((string)x.Tag, tempTag, true) == 0))
                {
                    continue;
                }
                else
                {
                    filterFile = (FilterFile)this.ViewManager.NewFile(tempTag);
                    break;
                }
            }

            // make new tab
            AddTabItem(filterFile);
        }

        /// <summary>
        /// Open File Dialog To test specify valid file for object sender
        /// </summary>
        /// <param name="sender"></param>
        public override void OpenFile(object sender)
        {
            // this.OpenDialogVisible = true;

            bool test = false;
            if (sender is string && !String.IsNullOrEmpty(sender as string))
            {
                test = true;
            }

            string logName = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Xml Files (*.xml)|*.xml|All Files (*.*)|*.*"; // Filter files by extension

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

                //SetStatusHandler(string.Format("opening file:{0}", logName));
                SetStatus(string.Format("opening file:{0}", logName));
                FilterFile logProperties = new FilterFile();
                if (String.IsNullOrEmpty((logProperties = (FilterFile)this.ViewManager.OpenFile(logName)).Tag))
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

        public bool SaveAsFile(object sender)
        {
            // this.OpenDialogVisible = true;

            bool test = false;
            if (sender is string && !String.IsNullOrEmpty(sender as string))
            {
                test = true;
            }

            string logName = string.Empty;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Xml Files (*.xml)|*.xml|All Files (*.*)|*.*"; // Filter files by extension

            Nullable<bool> result = false;
            // Show save file dialog box
            if (test)
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
                    return false;
                }
            }

            // Process save file dialog box results
            if (result == true)// && File.Exists(logName))
            {
                // Save document
                SetStatus(string.Format("saving file:{0}", logName));
                if (sender is ITabViewModel<FilterFileItem>)
                {
                    ITabViewModel<FilterFileItem> item = (sender as ITabViewModel<FilterFileItem>);
                    item.Tag = logName;
                    item.Header = item.Name = Path.GetFileName(logName);
                    Settings.AddFilterFile(logName);
                    SaveFile(sender);
                }
                else
                {
                    SaveFile(sender);
                }
            }

            return true;
        }

        public override void SaveFile(object sender)
        {
            ITabViewModel<FilterFileItem> tabItem;

            if (sender is TabItem)
            {
                tabItem = (ITabViewModel<FilterFileItem>)(sender as TabItem);
            }
            else
            {
                tabItem = (ITabViewModel<FilterFileItem>)this.TabItems[this.SelectedIndex];
            }

            if (string.IsNullOrEmpty(tabItem.Tag) || Regex.IsMatch(tabItem.Tag, _tempFilterNameFormatPattern))
            {
                if (!SaveAsFile(tabItem))
                {
                    this.TabItems.Remove(tabItem);
                }
            }
            else
            {
                this.ViewManager.SaveFile(tabItem.Tag, tabItem.ContentList);
            }
        }

        public void SaveModifiedFiles(object sender)
        {
            foreach (IFile<FilterFileItem> item in this.ViewManager.FileManager.Where(x => x.Modified == true))
            {
                // todo: prompt for saving?
                if (!RegexViewerSettings.Settings.AutoSaveFilters)
                {
                    TimedSaveDialog dialog = new TimedSaveDialog(item.Tag);
                    dialog.Enable();

                    switch (dialog.WaitForResult())
                    {
                        case TimedSaveDialog.Results.Disable:
                            RegexViewerSettings.Settings.AutoSaveFilters = true;
                            break;

                        case TimedSaveDialog.Results.DontSave:
                            item.Modified = false;
                            break;

                        case TimedSaveDialog.Results.Save:
                            this.SaveFile(item);
                            item.Modified = false;
                            break;

                        case TimedSaveDialog.Results.SaveAs:
                            this.SaveAsFile(item);
                            break;

                        case TimedSaveDialog.Results.Unknown:
                            // dont worry about errors since we are closing.
                            break;
                    }
                }
                else
                {
                    this.SaveFile(item);
                    item.Modified = false;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void tabItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("TabItems");
        }

        private void VerifyIndex(FilterFileItem filterFileItem = null)
        {
            FilterFile filterFile = CurrentFile();

            try
            {
                filterFile.EnablePatternNotifications(false);
                List<FilterFileItem> filterItems = filterFile.ContentItems.ToList();
                List<FilterFileItem> sortedFilterItems = new List<FilterFileItem>(filterItems.OrderBy(x => x.Index));

                bool dupes = false;
                bool needsSorting = false;
                List<Int64> indexList = new List<Int64>();

                for (int i = 0; i < sortedFilterItems.Count; i++)
                {
                    Int64 index = sortedFilterItems[i].Index;
                    if (index != filterItems[i].Index)
                    {
                        needsSorting = true;
                    }

                    if (!indexList.Contains(index))
                    {
                        indexList.Add(index);
                    }
                    else
                    {
                        dupes = true;
                    }
                }

                // does it need to be resorted
                if (!needsSorting && !dupes)
                {
                    // do nothing
                    return;
                }
                else if (needsSorting && !dupes)
                {
                    this.TabItems[SelectedIndex].ContentList = filterFile.ContentItems = new ObservableCollection<FilterFileItem>(sortedFilterItems);

                    //filterFile.ContentItems = filterFile.ContentItems.OrderBy(x => x.Index);
                }
                else if (dupes)
                {
                    Int64 currentIndex = -1;

                    if (filterFileItem != null && sortedFilterItems.Count(x => x.Index == filterFileItem.Index) > 1)
                    {
                        // remove and insert selected item in list at lowest position in index of dupes
                        sortedFilterItems.RemoveAt(sortedFilterItems.IndexOf(filterFileItem));
                        sortedFilterItems.Insert((int)(sortedFilterItems.IndexOf(sortedFilterItems.First(x => x.Index == filterFileItem.Index))), filterFileItem);
                    }

                    for (int i = 0; i < sortedFilterItems.Count; i++)
                    {
                        Int64 index = sortedFilterItems[i].Index;

                        if (index <= currentIndex)
                        {
                            filterItems[i] = sortedFilterItems[i];
                            filterItems[i].Index = ++currentIndex;
                        }
                        else
                        {
                            filterItems[i] = sortedFilterItems[i];
                            currentIndex = index;
                        }
                    }

                    this.TabItems[SelectedIndex].ContentList = filterFile.ContentItems = new ObservableCollection<FilterFileItem>(filterItems);
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

        private FilterFile CurrentFile()
        {
            if (this.TabItems.Count > 0
                    && this.TabItems.Count >= SelectedIndex)
            {
                return (FilterFile)this.ViewManager.FileManager.First(x => x.Tag == this.TabItems[SelectedIndex].Tag);
            }

            return new FilterFile();
        }
        public void ManageNewFilterFileItem()
        {
            // add blank new item so defaults / modifications can be set some type of bug
            IEnumerable<FilterFileItem> results = null;
            FilterFile filterFile = CurrentFile();
            Int64 indexMax = -1;

            SetStatus("ManageNewFilterFileItem:" + filterFile.FileName);

            results = filterFile.ContentItems.Where(x => x.Enabled == false
                    && x.Exclude == false
                    && x.Regex == false
                    && string.IsNullOrEmpty(x.Filterpattern)
                    && string.IsNullOrEmpty(x.Notes));

            if (filterFile.ContentItems.Count > 0)
            {
                indexMax = filterFile.ContentItems.Max(x => x.Index);
            }

            if (results == null | results != null && results.Count() == 0)
            {
                FilterFileItem fileItem = new FilterFileItem();

                filterFile.EnablePatternNotifications(false);
                fileItem.Index = indexMax + 1;
                filterFile.ContentItems.Add(fileItem);
                filterFile.EnablePatternNotifications(true);
            }
            else if (results.Count() == 1)
            {
                if (results.ToList()[0].Index != indexMax)
                {
                    filterFile.EnablePatternNotifications(false);
                    results.ToList()[0].Index = indexMax + 1;
                    filterFile.EnablePatternNotifications(true);
                }

                return;
            }
            else
            {
                for (int i = 0; i < results.Count() - 1; i++)
                {
                    filterFile.ContentItems.Remove(results.ToList()[i]);
                }
            }
        }

        private void ViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == FilterFileItemEvents.Index && (sender is FilterFileItem))
            {
                VerifyIndex((sender as FilterFileItem));
            }
            else if(sender is FilterFileItem | sender is FilterFile | sender is FilterFileManager)
            {
                ManageNewFilterFileItem();
            }
            //OnPropertyChanged(e.PropertyName);
            OnPropertyChanged(sender, e);
        }

        #endregion Private Methods
    }
}