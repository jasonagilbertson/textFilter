namespace RegexViewer
{
    internal interface IViewModel
    {
        #region Public Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        Command CloseCommand { get; set; }

        Command OpenCommand { get; set; }

        int SelectedIndex { get; set; }

        System.Collections.ObjectModel.ObservableCollection<ItemViewModel> TabItems { get; set; }

        #endregion Public Properties

        #region Public Methods

        void CloseFile(object sender);

        void OnPropertyChanged(string name);

        void OpenFile(object sender);

        #endregion Public Methods
    }
}