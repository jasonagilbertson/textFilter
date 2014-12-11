﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RegexViewer
{
    public class FilterViewModel : BaseViewModel<FilterFileItem>
    {
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

        void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("TabItems");
        }

        void ViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("ContentList");
        }

        #endregion Public Constructors

        #region Public Methods

        public override void AddTabItem(IFile<FilterFileItem> logProperties)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                SetStatus("adding tab:" + logProperties.Tag);
                FilterTabViewModel tabItem = new FilterTabViewModel();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                tabItem.Name = this.TabItems.Count.ToString();
                tabItem.ContentList = ((FilterFile)logProperties).ContentItems;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.FileName;
                tabItem.Modified = false;
                tabItem.PropertyChanged += tabItem_PropertyChanged;
                TabItems.Add(tabItem);
            }
        }

        void tabItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("ContentList");
        }

        public override void NewFile(object sender)
        {
            throw new NotImplementedException();
        }

        public void SaveModifiedFiles(object sender)
        {
            foreach(IFile<FilterFileItem> item in this.ViewManager.FileManager.Where(x => x.Modified == true))
            {
                // todo: prompt for saving?
                if (!RegexViewerSettings.Settings.AutoSaveFilters)
                {
                    TimedSaveDialog dialog = new TimedSaveDialog(item.Tag);
                    dialog.Enable();


                    switch (dialog.WaitForResult())
                    {
                        case TimedSaveDialog.Results.Disable:
                            //throw new NotImplementedException();
                            RegexViewerSettings.Settings.AutoSaveFilters = true;
                            //     Save_Click(null, null);
                            break;

                        case TimedSaveDialog.Results.DontSave:
                            item.Modified = false;
                            break;

                        case TimedSaveDialog.Results.Save:
                            this.SaveFile(item);
                            item.Modified = false;
                            break;

                        case TimedSaveDialog.Results.SaveAs:
                            //SaveAs_Click(null, null);
                            throw new NotImplementedException();
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

        /// <summary>
        /// Open File Dialog
        /// To test specify valid file for object sender
        /// </summary>
        /// <param name="sender"></param>
        public override void OpenFile(object sender)
        {
            //   this.OpenDialogVisible = true;

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

        #endregion Public Methods
    }
}