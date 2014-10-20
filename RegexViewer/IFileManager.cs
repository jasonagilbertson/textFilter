using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        bool SaveFile(string LogName, ObservableCollection<T> list);

        #endregion Public Methods
    }
}