// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-31-2015 ***********************************************************************
// <copyright file="BaseViewModel.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextFilter
{
    public abstract class BaseViewModel<T> : Base, INotifyPropertyChanged, IViewModel<T>
    {
        #region Private Fields

        private Command _closeAllCommand;

        private Command _closeCommand;

        private Command _copyFilePathCommand;

        private Command _findNextCommand;

        private Command _gotFocusCommand;

        private Command _hideCommand;

        private Command _newCommand;

        private Command _openCommand;

        private bool _openDialogVisible;

        private Command _openFolderCommand;

        private Command _pasteCommand;

        private int _previousIndex = -1;

        private Command _recentCommand;

        private Command _reloadCommand;

        private Command _renameCommand;

        private Command _saveAsCommand;

        private Command _saveCommand;

        private int _selectedIndex = -1;

        private Command _sharedCommand;

        private TextFilterSettings settings = TextFilterSettings.Settings;

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

        public Command FindNextCommand
        {
            get
            {
                if (_findNextCommand == null)
                {
                    _findNextCommand = new Command(FindNextExecuted);
                }
                _findNextCommand.CanExecute = true;

                return _findNextCommand;
            }
            set { _findNextCommand = value; }
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

        public Command PasteCommand
        {
            get
            {
                if (_pasteCommand == null)
                {
                    _pasteCommand = new Command(PasteText);
                }
                _pasteCommand.CanExecute = true;

                return _pasteCommand;
            }
            set { _pasteCommand = value; }
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

        public Command RenameCommand
        {
            get
            {
                if (_renameCommand == null)
                {
                    _renameCommand = new Command(RenameFileExecuted);
                }
                _renameCommand.CanExecute = true;

                return _renameCommand;
            }
            set { _renameCommand = value; }
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

        public TextFilterSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public Command SharedCommand
        {
            get { return _sharedCommand ?? new Command(SharedFileExecuted); }
            set { _sharedCommand = value; }
        }

        public ObservableCollection<ITabViewModel<T>> TabItems
        {
            get
            {
                return tabItems;
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
                SelectedIndex = tabItems.Count - 1;
            }
        }

        public abstract void AddTabItem(IFile<T> fileProperties);

        public void AddTabItems(List<IFile<T>> items)
        {
            // remove _transitioning when using parser _transitioning = true;
            for (int i = 0; i < items.Count; i++)
            {
                if (i == items.Count - 1)
                {
                    _transitioning = false;
                }

                AddTabItem(items[i]);
            }
        }

        public void CloseAllFilesExecuted(object sender)
        {
            ObservableCollection<ITabViewModel<T>> items = new ObservableCollection<ITabViewModel<T>>(tabItems);
            RemoveTabItems(items.ToList());
        }

        public void CloseFileExecuted(object sender)
        {
            if (IsValidTabIndex())
            {
                DeleteIfTempFile(CurrentFile());
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];

                if (!ViewManager.CloseFile(tabItem.Tag))
                {
                    return;
                }

                RemoveTabItem(tabItem);
            }
        }

        public void CopyFilePathExecuted(object sender)
        {
            if (IsValidTabIndex())
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                Clipboard.Clear();
                Clipboard.SetText(tabItem.Tag);
            }
        }

        public IFile<T> CurrentFile()
        {
            if (IsValidTabIndex())
            {
                return ViewManager.FileManager.FirstOrDefault(x => x.Tag == TabItems[SelectedIndex].Tag);
            }

            SetStatus(string.Format("CurrentFile: warning: returning default T SelectedIndex: {0}", SelectedIndex));
            return default(IFile<T>);
        }

        public ITabViewModel<T> CurrentTab()
        {
            if (SelectedIndex >= 0)
            {
                return TabItems[SelectedIndex];
            }

            SetStatus(string.Format("CurrentTab: warning: returning default T SelectedTab: {0}", SelectedIndex));
            return default(ITabViewModel<T>);
        }

        public abstract void FindNextExecuted(object sender);

        public string GenerateTempTagName()
        {
            // generate -new ##- index number
            string tempTag = string.Empty;

            for (int i = 0; i < 100; i++)
            {
                tempTag = string.Format(_tempTabNameFormat, i);
                if (TabItems.Any(x => String.Compare((string)x.Header, tempTag, true) == 0))
                {
                    continue;
                }
                else
                {
                    break;
                }
            }

            return tempTag;
        }

        public void GotFocusExecuted(object sender)
        {
            if (CurrentFile() != null)
            {
                App.Current.MainWindow.Title = string.Format("{0} {1}", System.AppDomain.CurrentDomain.FriendlyName, CurrentFile().Tag);
            }
        }

        public abstract void HideExecuted(object sender);

        public bool IsValidTabIndex()
        {
            bool retVal = false;
            if (SelectedIndex >= 0 && SelectedIndex < TabItems.Count)
            {
                retVal = true;
            }

            // noisy
            if (!retVal)
            {
                SetStatus(string.Format("IsValidTabIndex: return: {0}, {1}", retVal, SelectedIndex));
            }

            return retVal;
        }

        public ObservableCollection<MenuItem> Menubuilder(string directory)
        {
            ObservableCollection<MenuItem> menuCollection = new ObservableCollection<MenuItem>();

            try
            {
                if (string.IsNullOrEmpty(directory))
                {
                    SetStatus("MenuBuilder:exit: directory not specified.");
                    return menuCollection;
                }

                List<string> files = Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly).Where(x => x.ToLower().EndsWith("tat") || x.ToLower().EndsWith("rvf")).ToList();
                List<string> dirs = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly).ToList();
                foreach (string file in files)
                {
                    MenuItem wpfMenuItem = new MenuItem()
                    {
                        Command = SharedCommand,
                        CommandParameter = file,
                        Header = file.Replace(directory, "").TrimStart('\\')
                    };

                    menuCollection.Add(wpfMenuItem);
                }

                foreach (string dir in new List<string>(dirs))
                {
                    MenuItem dItem = new MenuItem();
                    dItem.Header = dir.Replace(directory, "").TrimStart('\\');
                    dItem.ItemsSource = new ObservableCollection<MenuItem>();

                    foreach (MenuItem item in Menubuilder(dir))
                    {
                        ((ObservableCollection<MenuItem>)dItem.ItemsSource).Add(item);
                    }

                    menuCollection.Add(dItem);
                }

                return menuCollection;
            }
            catch (Exception e)
            {
                SetStatus("Exception:MenuBuilder: " + e.ToString());
                return new ObservableCollection<MenuItem>();
            }
        }

        public void NewFileExecuted(object sender)
        {
            IFile<T> file = default(IFile<T>);

            string tempTag = GenerateTempTagName();

            if (IsValidTabIndex())
            {
                file = ViewManager.NewFile(tempTag, TabItems[SelectedIndex].ContentList);
            }
            else
            {
                file = ViewManager.NewFile(tempTag);
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

        public abstract void PasteText(object sender);

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
            SetStatus("ReloadFile:enter");

            if (IsValidTabIndex())
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                if (!File.Exists(tabItem.Tag))
                {
                    SetStatus("ReloadFile:returning: file does not exist: " + tabItem.Tag);
                    return;
                }

                ViewManager.CloseFile(tabItem.Tag);
                RemoveTabItem(tabItem);
                file = ViewManager.OpenFile(tabItem.Tag);
                AddTabItem(file);
                SetStatus("ReloadFile:exit");
            }
        }

        public void RemoveTabItem(ITabViewModel<T> tabItem)
        {
            if (tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Remove(tabItem);
                SelectedIndex = tabItems.Count - 1;
            }
        }

        public void RemoveTabItems(List<ITabViewModel<T>> items)
        {
            // remove _transitioning when using parser
            _transitioning = true;
            for (int i = 0; i < items.Count; i++)
            {
                DeleteIfTempFile(ViewManager.FileManager.FirstOrDefault(x => x.Tag == items[i].Tag));
                if (!ViewManager.CloseFile(items[i].Tag))
                {
                    continue;
                }

                if (i == items.Count - 1)
                {
                    _transitioning = false;
                }

                RemoveTabItem(items[i]);
            }
        }

        public void RenameFileExecuted(object sender)
        {
            SetStatus("RenameFile:enter");

            if (IsValidTabIndex())
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                RenameDialog dialog = new RenameDialog();
                string result = dialog.WaitForResult();
                if (!string.IsNullOrEmpty(result))
                {
                    tabItem.Modified = true;
                    RenameTabItem(result);
                }

                SetStatus("RenameFile:exit");
            }
        }

        public abstract void RenameTabItem(string newName);

        public abstract void SaveFileAsExecuted(object sender);

        public abstract void SaveFileExecuted(object sender);

        public void SaveModifiedFiles(object sender)
        {
            List<string> delList = new List<string>();
            try
            {
                foreach (IFile<T> item in new List<IFile<T>>(ViewManager.FileManager.Where(x => x.Modified == true)))
                {
                    // set tab index to current
                    SelectedIndex = TabItems.IndexOf(TabItems.First(x => x.Tag == item.Tag));
                    if (!IsValidTabIndex())
                    {
                        continue;
                    }

                    // prompt for saving
                    if (!TextFilterSettings.Settings.AutoSave)
                    {
                        TimedSaveDialog dialog = new TimedSaveDialog(item.Tag);

                        dialog.Enable();

                        switch (dialog.WaitForResult())
                        {
                            case TimedSaveDialog.Results.Disable:
                                TextFilterSettings.Settings.AutoSave = true;
                                break;

                            case TimedSaveDialog.Results.DontSave:
                                item.Modified = false;
                                break;

                            case TimedSaveDialog.Results.Save:
                                SaveFileExecuted(item);
                                item.Modified = false;
                                break;

                            case TimedSaveDialog.Results.SaveAs:
                                SaveFileAsExecuted(item);
                                item.Modified = false;
                                break;

                            case TimedSaveDialog.Results.Unknown:
                                // dont worry about errors since we are closing.
                                break;
                        }
                    }
                    else
                    {
                        SaveFileExecuted(item);
                        item.Modified = false;
                    }

                    DeleteIfTempFile(item);
                }
            }
            catch (Exception e)
            {
                SetStatus("SaveModifiedFiles: exception: " + e.ToString());
            }
        }

        public void SharedFileExecuted(object sender)
        {
            SetStatus("SharedFile:enter");
            OpenFileExecuted(sender);
        }

        private bool DeleteIfTempFile(IFile<T> item)
        {
            try
            {
                if (item.IsNew && item.Tag.ToLower().EndsWith(".tmp") && Path.GetFileName(item.Tag).ToLower().StartsWith("tmp"))
                {
                    Settings.RemoveLogFile(item.Tag);
                    Settings.RemoveFilterFile(item.Tag);
                    if (File.Exists(item.Tag))
                    {
                        SetStatus("DeleteTempFile: deleting temporary file:" + item.Tag);
                        File.Delete(item.Tag);
                    }
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                SetStatus("DeleteTempFile: exception:" + e.ToString());
                return false;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void OpenFolderExecuted()
        {
            if (IsValidTabIndex())
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                CreateProcess("explorer.exe", string.Format("\"{0}\"", Path.GetDirectoryName(tabItem.Tag)));
            }
        }

        #endregion Private Methods
    }
}