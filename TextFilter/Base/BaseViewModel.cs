﻿// ************************************************************************************
// Assembly: TextFilter
// File: BaseViewModel.cs
// Created: 3/19/2017
// Modified: 3/28/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TextFilter
{
    public abstract class BaseViewModel<T> : Base, INotifyPropertyChanged, IViewModel<T>
    {
        private Command _clearRecentCommand;

        private Command _closeAllCommand;

        private Command _closeCommand;

        private Command _copyFilePathCommand;

        private Command _displayAllDialogCommand;

        private Command _findNextCommand;

        private Command _findPreviousCommand;

        private Command _gotFocusCommand;

        private Command _gotoLineCommand;

        private Command _hideCommand;

        private Command _lostFocusCommand;

        private Command _newCommand;

        private Command _openCommand;

        private bool _openDialogVisible;

        private Command _openFolderCommand;

        private Command _pasteCommand;

        private int _previousIndex = -1;

        private Command _recentCommand;

        private ObservableCollection<WPFMenuItem> _recentFilterCollection;

        private ObservableCollection<WPFMenuItem> _recentLogCollection;

        private Command _reloadCommand;

        private Command _renameCommand;

        private Command _saveAsCommand;

        private Command _saveCommand;

        private int _selectedIndex = -1;

        private Command _sharedCommand;

        private TextFilterSettings settings = TextFilterSettings.Settings;

        private ObservableCollection<ITabViewModel<T>> tabItems;

        public Command ClearRecentCommand
        {
            get { return _clearRecentCommand ?? new Command(ClearRecentExecuted); }
            set { _clearRecentCommand = value; }
        }

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

        public Command DisplayAllDialogCommand
        {
            get
            {
                if (_displayAllDialogCommand == null)
                {
                    _displayAllDialogCommand = new Command(DisplayAllDialogExecuted);
                }

                _displayAllDialogCommand.CanExecute = true;
                return _displayAllDialogCommand;
            }

            set
            {
                _displayAllDialogCommand = value;
            }
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

        public Command FindPreviousCommand
        {
            get
            {
                if (_findPreviousCommand == null)
                {
                    _findPreviousCommand = new Command(FindPreviousExecuted);
                }
                _findPreviousCommand.CanExecute = true;

                return _findPreviousCommand;
            }
            set { _findPreviousCommand = value; }
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

        public Command GotoLineCommand
        {
            get
            {
                if (_gotoLineCommand == null)
                {
                    _gotoLineCommand = new Command(GotoLineExecuted);
                }
                _gotoLineCommand.CanExecute = true;

                return _gotoLineCommand;
            }
            set { _gotoLineCommand = value; }
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

        public Command LostFocusCommand
        {
            get
            {
                if (_lostFocusCommand == null)
                {
                    _lostFocusCommand = new Command(LostFocusExecuted);
                }
                _lostFocusCommand.CanExecute = true;

                return _lostFocusCommand;
            }
            set { _lostFocusCommand = value; }
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

        public ObservableCollection<WPFMenuItem> RecentFilterCollection
        {
            get
            {
                return _recentFilterCollection ?? RecentCollectionBuilder(Settings.RecentFilterFiles);
            }

            set
            {
                _recentFilterCollection = value ?? RecentCollectionBuilder(Settings.RecentFilterFiles);
                OnPropertyChanged("RecentFilterCollection");
            }
        }

        public ObservableCollection<WPFMenuItem> RecentLogCollection
        {
            get
            {
                return _recentLogCollection ?? RecentCollectionBuilder(Settings.RecentLogFiles);
            }

            set
            {
                _recentLogCollection = value ?? RecentCollectionBuilder(Settings.RecentLogFiles);
                OnPropertyChanged("RecentLogCollection");
            }
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

        public ITabViewModel<T> SelectedTab
        {
            get
            {
                return TabItems[SelectedIndex];
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

        public BaseViewModel()
        {
        }

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

        public abstract void ClearRecentExecuted();

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
                SaveModifiedFile(tabItem.Tag);

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

        public void DisplayAllDialogExecuted(object sender)
        {
            SetStatus("DisplayAllDialogExecuted");
            try
            {
                LogFile lFile = ((LogFile)_LogViewModel.CurrentFile());
                FilterFile fFile = null;
                int index = 0;

                if (((Selector)CurrentTab().Viewer).SelectedItem != null)
                {
                    if (typeof(T) == typeof(LogFileItem))
                    {
                        index = (int?)((LogFileItem)((Selector)CurrentTab().Viewer).SelectedItem).FilterIndex ?? 0;
                    }
                    else
                    {
                        index = (int?)((FilterFileItem)((Selector)CurrentTab().Viewer).SelectedItem).Index ?? 0;
                    }
                }

                if (_FilterViewModel.CurrentFile() != null)
                {
                    fFile = ((FilterFile)_FilterViewModel.CurrentFile());
                }

                if (lFile != null)
                {
                    DisplayAllFile dialog = new DisplayAllFile(lFile, fFile, index.ToString());
                    dialog.Show();
                }
                else
                {
                    SetStatus("DisplayAllExecuted:current file null!");
                }
            }
            catch (Exception e)
            {
                SetStatus("Exception:DisplayAllDialogExecuted: " + e.ToString());
            }
        }

        public abstract void FindNextExecuted(object sender);

        public abstract void FindPreviousExecuted(object sender);

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

        public IFile<T> GetFile(string tag)
        {
            IFile<T> file = default(IFile<T>);

            file = ViewManager.FileManager.FirstOrDefault(x => x.Tag == tag);
            if (file == null)
            {
                SetStatus(string.Format("GetFile: warning: returning default T tag: {0}", tag));
            }

            return file;
        }

        public void GotFocusExecuted(object sender)
        {
            if (CurrentFile() != null)
            {
                App.Current.MainWindow.Title = string.Format("{0} {1}", System.AppDomain.CurrentDomain.FriendlyName, CurrentFile().Tag);
            }
        }

        public abstract void GotoLineExecuted(object sender);

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

        public void LostFocusExecuted(object sender)
        {
            IFile<T> currentFile = CurrentFile() == null ? null : CurrentFile();

            if (currentFile != null && (Mouse.LeftButton == MouseButtonState.Pressed))
            {
                SetStatus(string.Format("LostFocusExecuted:drag:{0} {1}", (Mouse.LeftButton == MouseButtonState.Pressed), currentFile.Tag));

                if (currentFile.IsNew)
                {
                    // new files in temp so use temp location to save current changes for drag out
                    currentFile.IsNew = false;
                    SaveFileExecuted(null);
                    currentFile.IsNew = true;
                }
                else if (currentFile.Modified)
                {
                    SaveFileExecuted(null);
                }

                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    // launch new instance
                    SetStatus(string.Format("LostFocusExecuted:new instance:{0} {1}", (Mouse.LeftButton == MouseButtonState.Pressed), currentFile.Tag));
                    NewWindow(false, currentFile.Tag);
                }
                else
                {
                    // add to clipboard for drag out
                    SetStatus(string.Format("LostFocusExecuted:drag out:{0} {1}", (Mouse.LeftButton == MouseButtonState.Pressed), currentFile.Tag));
                    DataObject ddo = new DataObject(DataFormats.FileDrop, new string[1] { currentFile.Tag });
                    DragDrop.DoDragDrop(ddo, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }

            SetStatus("LostFocusExecuted:drag:" + (Mouse.LeftButton == MouseButtonState.Pressed));
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
                        Header = file.Replace(directory, "").TrimStart('\\'),
                        Background = Settings.BackgroundColor,
                        Foreground = Settings.ForegroundColor
                    };

                    menuCollection.Add(wpfMenuItem);
                }

                foreach (string dir in new List<string>(dirs))
                {
                    MenuItem dItem = new MenuItem()
                    {
                        Header = dir.Replace(directory, "").TrimStart('\\'),
                        ItemsSource = new ObservableCollection<MenuItem>(),
                        Background = Settings.BackgroundColor,
                        Foreground = Settings.ForegroundColor
                    };

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
            file = (IFile<T>)ViewManager.NewFile(tempTag);

            if (this is LogViewModel)
            {
                file = (IFile<T>)ViewManager.NewFile(tempTag, TabItems[SelectedIndex].ContentList);
                RecentLogCollection = null;
            }
            else
            {
                file = (IFile<T>)ViewManager.NewFile(tempTag);
                RecentFilterCollection = null;
            }

            AddTabItem(file);
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

        public void Refresh()
        {
            // force reset of menus for shared filters. part of F5 refresh
            _FilterViewModel.SharedCollection = Menubuilder(Settings.SharedFilterDirectory);
        }

        public void ReloadFileExecuted(object sender)
        {
            IFile<T> file = default(IFile<T>);
            SetStatus("ReloadFile:enter");

            Refresh();

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

        public void RenameTabItem(string fileName)
        {
            // rename tab
            ITabViewModel<T> tabItem = TabItems[SelectedIndex];
            if (this is FilterViewModel)
            {
                Settings.RemoveFilterFile(tabItem.Tag);
            }
            else
            {
                Settings.RemoveLogFile(tabItem.Tag);
            }

            if (CurrentFile() != null)
            {
                tabItem.Tag = CurrentFile().Tag = fileName;
                CurrentFile().FileName = tabItem.Header = tabItem.Name = Path.GetFileName(fileName);
            }
            else
            {
                SetStatus("RenameTabItem:error: current file is null: " + fileName);
            }

            if (this is FilterViewModel)
            {
                Settings.AddFilterFile(fileName);
            }
            else
            {
                Settings.AddLogFile(fileName);
            }
        }

        public abstract void SaveFileAsExecuted(object sender);

        public void SaveFileExecuted(object sender)
        {
            IFile<T> fileItem;

            if (sender is IFile<T>)
            {
                fileItem = (IFile<T>)(sender);
            }
            else
            {
                fileItem = CurrentFile();

                if (fileItem == null || fileItem == default(IFile<FilterFileItem>))
                {
                    // can get here by having no filters and hitting save file.
                    // todo: disable save file if no tab items
                    return;
                }
            }

            SetStatus(string.Format("SaveFileExecuted:tag: {0} name: {1}", fileItem.Tag, fileItem.FileName));

            if (fileItem.IsNew)
            {
                SaveFileAsExecuted(fileItem);
            }
            else
            {
                ViewManager.SaveFile(fileItem.Tag, fileItem);
                fileItem.Modified = false;
                if (this is FilterViewModel)
                {
                    Settings.AddFilterFile(fileItem.Tag);
                    RecentFilterCollection = null;
                }
                else
                {
                    Settings.AddLogFile(fileItem.Tag);
                    RecentLogCollection = null;
                }
            }
        }

        public void SaveModifiedFile(object sender)
        {
            if (sender is string)
            {
                IFile<T> item = ViewManager.FileManager.FirstOrDefault(x => x.Tag.ToLower() == (sender as string).ToLower());
                SaveModifiedFile(false, item);
            }
        }

        public void SaveModifiedFiles(object sender)
        {
            List<string> delList = new List<string>();
            bool noPrompt = false;

            try
            {
                foreach (IFile<T> item in new List<IFile<T>>(ViewManager.FileManager))
                {
                    // set tab index to current
                    SelectedIndex = TabItems.IndexOf(TabItems.First(x => x.Tag == item.Tag));
                    if (!IsValidTabIndex())
                    {
                        continue;
                    }

                    noPrompt = SaveModifiedFile(noPrompt, item);
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

        public TextBox TextBoxFromDataGrid(DataGrid dataGrid)
        {
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);

            if (row != null)
            {
                return FindVisualChild<TextBox>(row);
            }

            SetStatus("TextBoxFromDataGrid:error: unable to find datgrid row");
            return new TextBox();
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

        private void OpenFolderExecuted()
        {
            if (IsValidTabIndex())
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                CreateProcess("explorer.exe", string.Format("\"{0}\"", Path.GetDirectoryName(tabItem.Tag)));
            }
        }

        private bool SaveModifiedFile(bool noPrompt, IFile<T> item)
        {
            if (item == default(IFile<T>) || item.Modified == false)
            {
                SetStatus("SaveModifiedFile: not modified. returning");
                return noPrompt;
            }

            // prompt for saving
            if (!TextFilterSettings.Settings.AutoSave & !noPrompt)
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

                    case TimedSaveDialog.Results.DontSaveAll:
                        noPrompt = true;
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
            else if (TextFilterSettings.Settings.AutoSave)
            {
                SaveFileExecuted(item);
                item.Modified = false;
            }

            DeleteIfTempFile(item);
            return noPrompt;
        }
    }
}