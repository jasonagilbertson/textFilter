namespace RegexViewer
{
    internal interface IViewModel<T>
    {
        #region Public Methods

        void CloseFile(object sender);

        void OnPropertyChanged(string name);

        void OpenFile(object sender);

        void RemoveTabItem(ITabViewModel tabItem);

        #endregion Public Methods

        #region Public Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        Command CloseCommand { get; set; }

        Command OpenCommand { get; set; }

        int SelectedIndex { get; set; }
        IFileManager<T> FileManager { get; set; }
        System.Collections.ObjectModel.ObservableCollection<ITabViewModel> TabItems { get; set; }

        #endregion Public Properties

        void AddTabItem(IFileProperties<T> fileProperties);
    }
}