using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RegexViewer
{
    public interface IFileManager<T>
    {
        #region Public Events

        event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        List<IFile<T>> FileManager { get; set; }

        #endregion Public Properties

        #region Public Methods

        bool CloseFile(string LogName);

        IFile<T> NewFile(string LogName, ObservableCollection<T> items = null);

        IFile<T> OpenFile(string LogName);

        List<IFile<T>> OpenFiles(string[] files);

        List<T> ReadFile(string LogName);

        bool SaveFile(string FileName, ObservableCollection<T> list);

        #endregion Public Methods
    }
}