namespace RegexViewer
{
    public interface IViewModel<T>
    {
        #region Public Methods

        void CloseAllFiles(object sender);

        void CloseFile(object sender);

        IFile<T> CurrentFile();

        ITabViewModel<T> CurrentTab();

        void NewFile(object sender);

        void OnPropertyChanged(string name);

        void OpenDrop(object sender);

        void OpenFile(object sender);

        void RemoveTabItem(ITabViewModel<T> tabItem);

        void RenameTabItem(string newName);

        void SaveFile(object sender);

        void SaveFileAs(object sender);

        #endregion Public Methods

        #region Public Events

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