namespace RegexViewer
{
    public interface ITabViewModel
    {
        #region Public Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        string Background { get; set; }

        string Content { get; set; }

        Command CopyCommand { get; set; }

        string Header { get; set; }

        string Name { get; set; }

        string Tag { get; set; }

        #endregion Public Properties

        #region Public Methods

        void CopyExecuted(object target);

        void OnPropertyChanged(string name);

        #endregion Public Methods
    }
}