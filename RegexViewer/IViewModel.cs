namespace RegexViewer
{
    public interface IViewModel<T>
    {
        #region Public Methods

        void CloseFile(object sender);

        void NewFile(object sender);

        void OnPropertyChanged(string name);

        void OpenFile(object sender);

        void RemoveTabItem(ITabViewModel<T> tabItem);

        void SaveFile(object sender);

        #endregion Public Methods

        #region Public Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        Command CloseCommand { get; set; }

        IFileManager<T> FileManager { get; set; }

        Command OpenCommand { get; set; }

        bool OpenDialogVisible { get; set; }

        int SelectedIndex { get; set; }

        // public ObservableCollection<string> Status;
        //void SetStatus(string statusData);
        //public static StatusDelegate SetStatusHandler;
        //public delegate void StatusDelegate(string status);
        System.Collections.ObjectModel.ObservableCollection<ITabViewModel<T>> TabItems { get; set; }

        #endregion Public Properties

        void AddTabItem(IFileItems<T> fileProperties);
    }
}