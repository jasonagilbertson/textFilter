namespace RegexViewer
{
    public interface IFileItem
    {
        #region Public Properties

        System.Windows.Media.Brush Background { get; set; }

        string Content { get; set; }

        System.Windows.Media.Brush Foreground { get; set; }

        #endregion Public Properties

        #region Public Methods

        IFileItem ShallowCopy();

        #endregion Public Methods
    }
}