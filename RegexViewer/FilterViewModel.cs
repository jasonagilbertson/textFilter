using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace RegexViewer
{
    public class FilterViewModel : BaseViewModel<FilterFileItem>
    {
        #region Public Constructors
        private string _tempFilterNameFormat = "*new {0}*";
        private string _tempFilterNameFormatPattern = @"\*new [0-9]{1,2}\*";
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

        public override void NewFile(object sender)
        {
            FilterFile filterFile = new FilterFile();
            // add temp name
            for(int i = 0;i< 100; i++)
            {
                string tempTag = string.Format(_tempFilterNameFormat, i);
                if(this.TabItems.Any(x => String.Compare((string)x.Tag, tempTag, true) == 0))
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

            if (string.IsNullOrEmpty(tabItem.Tag) || Regex.IsMatch(tabItem.Tag,_tempFilterNameFormatPattern))
            {
                
                SaveAsFile(tabItem);
            }
            else
            {
                this.ViewManager.SaveFile(tabItem.Tag, tabItem.ContentList);
            }
        }

        public void SaveAsFile(object sender)
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
            OnPropertyChanged("ContentList");
        }

        private void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("TabItems");
        }

        private void ViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        #endregion Private Methods
    }
}