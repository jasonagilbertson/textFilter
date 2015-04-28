namespace RegexViewer
{
    public interface IViewModel<T>
    {
        #region Public Methods

        void CloseAllFilesExecuted(object sender);

        void CloseFileExecuted(object sender);

        IFile<T> CurrentFile();

        ITabViewModel<T> CurrentTab();

        void NewFileExecuted(object sender);

        void OnPropertyChanged(string name);

        void OpenDropExecuted(object sender);

        void OpenFileExecuted(object sender);

        void RemoveTabItem(ITabViewModel<T> tabItem);

        void RenameTabItem(string newName);

        void SaveFileAsExecuted(object sender);

        void SaveFileExecuted(object sender);

        #endregion Public Methods

        #region Public Events

        // ITabViewModel<T> SelectedTabItem { get; set; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        Command CloseCommand { get; set; }

        Command DragDropCommand { get; set; }

        Command OpenCommand { get; set; }

        bool OpenDialogVisible { get; set; }

        int SelectedIndex { get; set; }

        System.Collections.ObjectModel.ObservableCollection<ITabViewModel<T>> TabItems { get; set; }

        IFileManager<T> ViewManager { get; set; }

        #endregion Public Properties

        void AddTabItem(IFile<T> fileProperties);
    }
}