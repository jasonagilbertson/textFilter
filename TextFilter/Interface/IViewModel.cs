// ************************************************************************************
// Assembly: TextFilter
// File: IViewModel.cs
// Created: 12/24/2016
// Modified: 2/12/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

namespace TextFilter
{
    public interface IViewModel<T>
    {
        // ITabViewModel<T> SelectedTabItem { get; set; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        Command ClearRecentCommand { get; set; }

        Command CloseCommand { get; set; }

        Command FindNextCommand { get; set; }

        Command GotFocusCommand { get; set; }

        Command GotoLineCommand { get; set; }

        Command HideCommand { get; set; }

        Command LostFocusCommand { get; set; }

        Command OpenCommand { get; set; }

        bool OpenDialogVisible { get; set; }

        Command PasteCommand { get; set; }

        int SelectedIndex { get; set; }

        System.Collections.ObjectModel.ObservableCollection<ITabViewModel<T>> TabItems { get; set; }

        IFileManager<T> ViewManager { get; set; }

        void AddTabItem(IFile<T> fileProperties);

        void ClearRecentExecuted();

        void CloseAllFilesExecuted(object sender);

        void CloseFileExecuted(object sender);

        IFile<T> CurrentFile();

        ITabViewModel<T> CurrentTab();

        void FindNextExecuted(object sender);

        void FindPreviousExecuted(object sender);

        IFile<T> GetFile(string tag);

        void GotFocusExecuted(object sender);

        void HideExecuted(object sender);

        bool IsValidTabIndex();

        void LostFocusExecuted(object sender);

        void NewFileExecuted(object sender);

        void OnPropertyChanged(string name);

        void OpenFileExecuted(object sender);

        void PasteText(object sender);

        void RemoveTabItem(ITabViewModel<T> tabItem);

        void RenameTabItem(string newName);

        void SaveFileAsExecuted(object sender);

        void SaveFileExecuted(object sender);
    }
}