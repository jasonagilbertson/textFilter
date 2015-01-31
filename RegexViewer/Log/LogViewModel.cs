using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegexViewer
{
    public class LogViewModel : BaseViewModel<LogFileItem>
    {
        #region Private Fields

        private FilterViewModel _filterViewModel;
        private Command _hideCommand;
        private bool _hiding = true;
        private Command _keyDownCommand;
        private LogFileManager _logFileManager;
        private int _previousIndex;
        private Command _quickFindChangedCommand;
        private string _quickFindText = string.Empty;
        //private int _selectedItemIndex;

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
            
            FilterLogTabItems(null, null, FilterCommand.Reset);
            // FilterActiveTabItem();

        }

        #endregion Public Constructors

        #region Public Properties

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
                _previousIndex = this.SelectedIndex;
                this.SelectedIndex = this.TabItems.Count - 1;
                FilterLogTabItems(null, (LogFile)logFile, FilterCommand.Filter);
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

        public void FilterLogTabItems(FilterFileItem filter = null, LogFile logFile = null, FilterCommand filterIntent = FilterCommand.Filter)
        {
            SetStatus("filterLogTabItems");
            if (logFile == null)
            {
                // find logFile if it was not supplied
                if (_logFileManager.FileManager.Count > 0)
                {
                    logFile = (LogFile)_logFileManager.FileManager.FirstOrDefault(x => x.Tag == this.TabItems[SelectedIndex].Tag);
                }
                else
                {
                    return;
                }
            }

            

            List<FilterFileItem> filterFileItems = _filterViewModel.FilterList(filter);
        
            if (_filterViewModel.CompareFilterList(filterFileItems) 
                & _previousIndex == SelectedIndex
                & filter == null 
                & filterIntent == FilterCommand.Filter)
            {
                SetStatus("filterLogTabItems:no change");
            }
            else if (filterFileItems == null | (filterFileItems != null && filterFileItems.Count == 0))
            {
                // no filter so return full list
                SetStatus("filterLogTabItems:no filter, returning full list");
                this.TabItems[this.SelectedIndex].ContentList = logFile.ContentItems;
            }
            else
            {
                this.TabItems[this.SelectedIndex].ContentList = _logFileManager.ApplyFilter(logFile, filterFileItems, filterIntent == FilterCommand.Highlight);
            }
            
            _previousIndex = SelectedIndex;
        }
        //public new ListBox ViewObject { get; set; }
        public void HideExecuted(object sender)
        {
            // move to MainWindow.cs and handle event there
            //int currentPosition = this.TabItems[SelectedIndex].SelectedIndex;
            //int currentPosition = this.SelectedItemIndex;
         //   int currentPosition = this.ViewObject.SelectedIndex;
       //     ListBox listbox = FindVisualParent<ListBox>((UIElement)sender);
        //    ListBox listbox2 = GetFirstChildByType<ListBox>((DependencyObject)sender);

            //Function: RegexViewer.BaseTabViewModel<T>.SelectionChangedExecuted(object), Thread: 0x4664C Main Thread
            LogFileItem currentPosition = this.TabItems[SelectedIndex].SelectedIndexItem;

            
            SetStatus("hiding:currentposition:" + currentPosition.Content);
            if (_hiding)
            {
            
    
                this.FilterLogTabItems(null, null, FilterCommand.Highlight);
                
            }
            else
            {
                // send empty function to reset to current filter in filterview
                if (!string.IsNullOrEmpty(QuickFindText))
                {
                    QuickFindChangedExecuted(null);
                }
                else
                {
                    this.FilterLogTabItems(null, null, FilterCommand.Reset);
                }
            }

            try
            {
                ListBox listBox = (ListBox)this.TabItems[SelectedIndex].Viewer;
                if (listBox != null && listBox.Items.Contains(currentPosition))
                {
                    SetStatus("hiding:scrollingintoview:");
                    listBox.ScrollIntoView(currentPosition);
                    listBox.SelectedItem = currentPosition;
                }
            }
            catch (Exception e)
            {
                SetStatus("hiding:exception:" + e.ToString());
            }
            // move to MainWindow.cs and handle event there
            //this.TabItems[SelectedIndex].SelectedIndex = currentPosition;
            //this.SelectedItemIndex = currentPosition;
            // (ListBox)this.TabItems[SelectedIndex];
       //     this.ViewObject.ScrollIntoView(currentPosition);
            _hiding = !_hiding;

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

            if (sender is string)
            {
                string filter = (sender as string);
                if (string.IsNullOrEmpty(filter))
                {
                    // send empty function to reset to current filter in filterview
                    this.FilterLogTabItems(null, null, FilterCommand.Reset);
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
            this.FilterLogTabItems(fileItem, null, FilterCommand.DynamicFilter);
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

        //public TabControl TabControl { get; set; }

        //public int SelectedItemIndex 
        //{
        //    get
        //    {
        //        return _selectedItemIndex;
        //    }
        //    set
        //    {
        //        if (_selectedItemIndex != value)
        //        {
        //            _selectedItemIndex = value;
        //            OnPropertyChanged("SelectedItemIndex");
        //        }
        //    }
        //}
    }
}