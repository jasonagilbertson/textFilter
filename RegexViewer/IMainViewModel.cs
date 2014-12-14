namespace RegexViewer
{
    public interface IMainViewModel
    {
        #region Public Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Methods

        void SetViewStatus(string statusData);

        #endregion Public Methods
    }
}