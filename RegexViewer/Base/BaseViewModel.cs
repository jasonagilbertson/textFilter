using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace RegexViewer
{
    public abstract class BaseViewModel<T> : Base, INotifyPropertyChanged, IViewModel<T>
    {
        #region Private Fields

        private Command _closeAllCommand;
        private Command _closeCommand;
        private Command _copyFilePathCommand;
        private Command _gotFocusCommand;
        private Command _newCommand;
        private Command _openCommand;
        private bool _openDialogVisible;
        private Command _openFolderCommand;
        private int _previousIndex = -1;
        private Command _recentCommand;
        private Command _reloadCommand;
        private Command _saveAsCommand;
        private Command _saveCommand;
        private int _selectedIndex = -1;
        private RegexViewerSettings settings = RegexViewerSettings.Settings;
        private ObservableCollection<ITabViewModel<T>> tabItems;

        #endregion Private Fields

        #region Public Constructors

        public BaseViewModel()
        {
        }

        #endregion Public Constructors

        #region Public Properties

        public Command CloseAllCommand
        {
            get { return _closeAllCommand ?? new Command(CloseAllFilesExecuted); }
            set { _closeAllCommand = value; }
        }

        public Command CloseCommand
        {
            get { return _closeCommand ?? new Command(CloseFileExecuted); }
            set { _closeCommand = value; }
        }

        public Command CopyFilePathCommand
        {
            get
            {
                if (_copyFilePathCommand == null)
                {
                    _copyFilePathCommand = new Command(CopyFilePathExecuted);
                }
                _copyFilePathCommand.CanExecute = true;

                return _copyFilePathCommand;
            }
            set { _copyFilePathCommand = value; }
        }

        public Command DragDropCommand
        {
            get { return _openCommand ?? new Command(OpenDropExecuted); }
            set { _openCommand = value; }
        }

        public Command GotFocusCommand
        {
            get
            {
                if (_gotFocusCommand == null)
                {
                    _gotFocusCommand = new Command(GotFocusExecuted);
                }
                _gotFocusCommand.CanExecute = true;

                return _gotFocusCommand;
            }
            set { _gotFocusCommand = value; }
        }

        public Command NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new Command(NewFileExecuted);
                }
                _newCommand.CanExecute = true;

                return _newCommand;
            }
            set { _newCommand = value; }
        }

        public Command OpenCommand
        {
            get { return _openCommand ?? new Command(OpenFileExecuted); }
            set { _openCommand = value; }
        }

        public bool OpenDialogVisible
        {
            get
            {
                return _openDialogVisible;
            }

            set
            {
                if (_openDialogVisible != value)
                {
                    _openDialogVisible = value;
                    OnPropertyChanged("OpenDialogVisible");
                }
            }
        }

        public Command OpenFolderCommand
        {
            get
            {
                if (_openFolderCommand == null)
                {
                    _openFolderCommand = new Command(OpenFolderExecuted);
                }
                _openFolderCommand.CanExecute = true;

                return _openFolderCommand;
            }
            set { _openFolderCommand = value; }
        }

        public int PreviousIndex
        {
            get
            {
                return _previousIndex;
            }
            set
            {
                _previousIndex = value;
            }
        }

        public Command RecentCommand
        {
            get { return _recentCommand ?? new Command(RecentFileExecuted); }
            set { _recentCommand = value; }
        }

        public Command ReloadCommand
        {
            get
            {
                if (_reloadCommand == null)
                {
                    _reloadCommand = new Command(ReloadFileExecuted);
                }
                _reloadCommand.CanExecute = true;

                return _reloadCommand;
            }
            set { _reloadCommand = value; }
        }

        public Command SaveAsCommand
        {
            get { return _saveAsCommand ?? new Command(SaveFileAsExecuted); }
            set { _saveAsCommand = value; }
        }

        public Command SaveCommand
        {
            get { return _saveCommand ?? new Command(SaveFileExecuted); }
            set { _saveCommand = value; }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_selectedIndex != value)
                {
                    SetStatus(string.Format("BaseViewModel:SelectedIndex changed old index: {0} new index: {1}", _selectedIndex, value));
                    _selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        public RegexViewerSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public ObservableCollection<ITabViewModel<T>> TabItems
        {
            get
            {
                return this.tabItems;
            }
            set
            {
                tabItems = value;
            }
        }

        public IFileManager<T> ViewManager { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void AddTabItem(ITabViewModel<T> tabItem)
        {
            if (!tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Add(tabItem);
                this.SelectedIndex = tabItems.Count - 1;
            }
        }

        public abstract void AddTabItem(IFile<T> fileProperties);

        public void CloseAllFilesExecuted(object sender)
        {
            ObservableCollection<ITabViewModel<T>> items = new ObservableCollection<ITabViewModel<T>>(tabItems);
            foreach (ITabViewModel<T> tabItem in items)
            {
                if (!this.ViewManager.CloseFile(tabItem.Tag))
                {
                    continue;
                }

                RemoveTabItem(tabItem);
            }
        }

        public void CloseFileExecuted(object sender)
        {
            if (SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count)
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                if (!this.ViewManager.CloseFile(tabItem.Tag))
                {
                    return;
                }

                RemoveTabItem(tabItem);
            }
        }

        public void CopyFilePathExecuted(object sender)
        {
            if (SelectedIndex >= 0 & SelectedIndex < this.TabItems.Count)
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                Clipboard.Clear();
                Clipboard.SetText(tabItem.Tag);
            }
        }

        public IFile<T> CurrentFile()
        {
            if (SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count)
            {
                return this.ViewManager.FileManager.FirstOrDefault(x => x.Tag == this.TabItems[SelectedIndex].Tag);
            }

            SetStatus(string.Format("CurrentFile: warning: returning default T SelectedIndex: {0}", SelectedIndex));
            return default(IFile<T>);
        }

        public ITabViewModel<T> CurrentTab()
        {
            if (SelectedIndex >= 0)
            {
                return this.TabItems[SelectedIndex];
            }

            SetStatus(string.Format("CurrentTab: warning: returning default T SelectedTab: {0}", SelectedIndex));
            return default(ITabViewModel<T>);
        }

        public void GotFocusExecuted(object sender)
        {
            if (CurrentFile() != null)
            {
                App.Current.MainWindow.Title = string.Format("{0} {1}", System.AppDomain.CurrentDomain.FriendlyName, CurrentFile().Tag);
            }
        }

        public void NewFileExecuted(object sender)
        {
            IFile<T> file = default(IFile<T>);
            // add temp name
            for (int i = 0; i < 100; i++)
            {
                string tempTag = string.Format(_tempTabNameFormat, i);
                if (this.TabItems.Any(x => String.Compare((string)x.Tag, tempTag, true) == 0))
                {
                    continue;
                }
                else
                {
                    if (SelectedIndex >= 0 & SelectedIndex < this.TabItems.Count)
                    {
                        file = this.ViewManager.NewFile(tempTag, this.TabItems[SelectedIndex].ContentList);
                    }
                    else
                    {
                        file = this.ViewManager.NewFile(tempTag);
                    }
                    break;
                }
            }

            AddTabItem(file);
        }

        public void OpenDropExecuted(object sender)
        {
            SetStatus("OpenDrop: " + sender.GetType().ToString());
            SetStatus("OpenDrop: " + sender.ToString());
            if (sender is string)
            {
                SetStatus("OpenDrop: " + (sender as string));
            }
        }

        public abstract void OpenFileExecuted(object sender);

        public ObservableCollection<WPFMenuItem> RecentCollectionBuilder(string[] files)
        {
            ObservableCollection<WPFMenuItem> fileCollection = new ObservableCollection<WPFMenuItem>();

            foreach (string file in files)
            {
                WPFMenuItem menuItem = new WPFMenuItem()
                {
                    Command = RecentCommand,
                    Text = file
                };
                fileCollection.Add(menuItem);
            }

            return fileCollection;
        }

        public void RecentFileExecuted(object sender)
        {
            SetStatus("RecentFile:enter");
            OpenFileExecuted(sender);
        }

        public void ReloadFileExecuted(object sender)
        {
            IFile<T> file = default(IFile<T>);
            if (SelectedIndex >= 0 & SelectedIndex < this.TabItems.Count)
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                if (!this.ViewManager.CloseFile(tabItem.Tag) | !File.Exists(tabItem.Tag))
                {
                    return;
                }

                RemoveTabItem(tabItem);
                file = this.ViewManager.OpenFile(tabItem.Tag);
                AddTabItem(file);
            }
        }

        public void RemoveTabItem(ITabViewModel<T> tabItem)
        {
            if (tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Remove(tabItem);
                this.SelectedIndex = tabItems.Count - 1;
            }
        }

        public abstract void RenameTabItem(string newName);

        public abstract void SaveFileAsExecuted(object sender);

        public abstract void SaveFileExecuted(object sender);

        //        file.ContentItems = tabItem.ContentList;
        //        this.ViewManager.SaveFile(tabItem.Tag, file);
        //    }
        //}
        public void SaveModifiedFiles(object sender)
        {
            foreach (IFile<T> item in this.ViewManager.FileManager.Where(x => x.Modified == true))
            {
                // todo: prompt for saving?
                if (!RegexViewerSettings.Settings.AutoSave)
                {
                    TimedSaveDialog dialog = new TimedSaveDialog(item.Tag);
                    dialog.Enable();

                    switch (dialog.WaitForResult())
                    {
                        case TimedSaveDialog.Results.Disable:
                            RegexViewerSettings.Settings.AutoSave = true;
                            break;

                        case TimedSaveDialog.Results.DontSave:
                            item.Modified = false;
                            break;

                        case TimedSaveDialog.Results.Save:
                            this.SaveFileExecuted(item);
                            item.Modified = false;
                            break;

                        case TimedSaveDialog.Results.SaveAs:
                            this.SaveFileAsExecuted(item);
                            break;

                        case TimedSaveDialog.Results.Unknown:
                            // dont worry about errors since we are closing.
                            break;
                    }
                }
                else
                {
                    this.SaveFileExecuted(item);
                    item.Modified = false;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void OpenFolderExecuted()
        {
            if (SelectedIndex >= 0 & SelectedIndex < this.TabItems.Count)
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                CreateProcess("explorer.exe", Path.GetDirectoryName(tabItem.Tag));
            }
        }

        #endregion Private Methods

        //public void SaveFileExecuted(object sender)
        //{
        //    ITabViewModel<T> tabItem;

        // if (sender is TabItem) { tabItem = (ITabViewModel<T>)(sender as TabItem); } else { if
        // (SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count) { tabItem =
        // (ITabViewModel<T>)this.TabItems[this.SelectedIndex]; } else { // can get here by having
        // no filters and hitting save file. // todo: disable save file if no tab items return; } }

        // if (string.IsNullOrEmpty(tabItem.Tag) || Regex.IsMatch(tabItem.Tag,
        // _tempTabNameFormatPattern, RegexOptions.IgnoreCase)) { SaveFileAsExecuted(tabItem); }
        // else { //IFile<T> file = (IFile<T>)new object(); IFile<T> file = default(IFile<T>);
        // if(typeof(T) == typeof(LogFileItem)) { file = (IFile<T>)new LogFile(); } else if
        // (typeof(T) == typeof(FilterFileItem)) { file = (IFile<T>)new FilterFile(); }
    }
}