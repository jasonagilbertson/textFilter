using System;
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
            this.FileManager = new FilterFileManager();


            // load tabs from last session
            foreach (FilterFileItems logProperty in this.FileManager.OpenFiles(this.Settings.CurrentFilterFiles.ToArray()))
            {
                AddTabItem(logProperty);
            }
        }

        #endregion Public Constructors

        #region Public Methods

        public override void AddTabItem(IFileItems<FilterFileItem> logProperties)
        {
            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                SetStatus("adding tab:" + logProperties.Tag);
                FilterTabViewModel tabItem = new FilterTabViewModel();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                tabItem.Name = this.TabItems.Count.ToString();
                tabItem.ContentList = ((FilterFileItems)logProperties).ContentItems;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.FileName;
                TabItems.Add(tabItem);
            }
        }

        public override void NewFile(object sender)
        {
            throw new NotImplementedException();
        }

        public void SaveModifiedFiles(object sender)
        {
            foreach(ITabViewModel<FilterFileItem> item in this.TabItems.Where(x => x.Modified == true))
            {
                // todo: prompt for saving?
                
                TimedSaveDialog dialog = new TimedSaveDialog(item.Tag);
                dialog.Enable();

                switch (dialog.WaitForResult())
                {
                    case TimedSaveDialog.Results.Disable:
                        throw new NotImplementedException();
                 //       _etwMonitor.Config.AppSettings.Annoyance = false;
                   //     Save_Click(null, null);
                        break;

                    case TimedSaveDialog.Results.DontSave:
                        break;

                    case TimedSaveDialog.Results.Save:
                        this.SaveFile(item);
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
                FilterFileItems logProperties = new FilterFileItems();
                if (String.IsNullOrEmpty((logProperties = (FilterFileItems)this.FileManager.OpenFile(logName)).Tag))
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