namespace RegexViewer
{
    public class FilterTabViewModel : BaseTabViewModel<FilterFileItem>
    {
        #region Public Constructors
        private bool _maskedVisibility = RegexViewerSettings.Settings.CountMaskedMatches;
        public FilterTabViewModel()
        {
        }

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
        #endregion Public Constructors
    }
}