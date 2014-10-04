using System.Collections.Generic;

namespace RegexViewer
{
    public interface IFileManager<T>
    {
        #region Public Properties

        List<IFileProperties<T>> Files { get; set; }

        #endregion Public Properties

        #region Public Methods

        bool CloseFile(string LogName);

        IFileProperties<T> OpenFile(string LogName);

        List<IFileProperties<T>> OpenFiles(string[] files);

        bool SaveFile(string LogName, List<T> list);

        #endregion Public Methods
    }
}