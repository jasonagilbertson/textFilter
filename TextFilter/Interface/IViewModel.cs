// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-25-2015 ***********************************************************************
// <copyright file="IViewModel.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************

namespace TextFilter
{
    public interface IViewModel<T>
    {
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        Command CloseCommand { get; set; }

        Command DragDropCommand { get; set; }

        Command FindNextCommand { get; set; }

        Command HideCommand { get; set; }

        Command OpenCommand { get; set; }

        bool OpenDialogVisible { get; set; }

        Command PasteCommand { get; set; }

        int SelectedIndex { get; set; }

        System.Collections.ObjectModel.ObservableCollection<ITabViewModel<T>> TabItems { get; set; }

        IFileManager<T> ViewManager { get; set; }

        void AddTabItem(IFile<T> fileProperties);

        void CloseAllFilesExecuted(object sender);

        void CloseFileExecuted(object sender);

        IFile<T> CurrentFile();

        ITabViewModel<T> CurrentTab();

        void FindNextExecuted(object sender);

        void HideExecuted(object sender);

        bool IsValidTabIndex();

        void NewFileExecuted(object sender);

        void OnPropertyChanged(string name);

        void OpenDropExecuted(object sender);

        void OpenFileExecuted(object sender);

        void PasteText(object sender);

        void RemoveTabItem(ITabViewModel<T> tabItem);

        void RenameTabItem(string newName);

        void SaveFileAsExecuted(object sender);

        void SaveFileExecuted(object sender);

        void UpdateView(WorkerItem workerItem);
    }
}