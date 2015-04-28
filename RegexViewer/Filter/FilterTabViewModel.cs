namespace RegexViewer
{
    public class FilterTabViewModel : BaseTabViewModel<FilterFileItem>
    {
        #region Private Fields

        private bool _maskedVisibility = RegexViewerSettings.Settings.CountMaskedMatches;

        #endregion Private Fields

        #region Public Constructors

        public FilterTabViewModel()
        {
        }

        #endregion Public Constructors

        #region Public Properties

        public bool MaskedVisibility
        {
            get
            {
                return _maskedVisibility;
            }
            set
            {
                if (_maskedVisibility != value)
                {
                    _maskedVisibility = value;
                    OnPropertyChanged("MaskedVisibility");
                }
            }
        }

        #endregion Public Properties
    }
}