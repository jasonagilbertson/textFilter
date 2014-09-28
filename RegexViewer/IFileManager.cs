namespace RegexViewer
{
    public interface IFileManager<T>
    {
        #region Public Properties

        System.Collections.Generic.List<IFileProperties<T>> Files { get; set; }

        #endregion Public Properties

        #region Public Methods

        bool CloseLog(string LogName);

        IFileProperties<T> OpenFile(string LogName);

        System.Collections.Generic.List<IFileProperties<T>> OpenFiles(string[] files);

        #endregion Public Methods

       
    }
}