using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RegexViewer
{
    public interface IFileManager<T> 
    {
        #region Public Properties

        List<IFile<T>> FileManager { get; set; }

        #endregion Public Properties

        #region Public Methods

        bool CloseFile(string LogName);

        IFile<T> OpenFile(string LogName);

        List<IFile<T>> OpenFiles(string[] files);
        event PropertyChangedEventHandler PropertyChanged;
        bool SaveFile(string LogName, ObservableCollection<T> list);
        
        #endregion Public Methods
    }
}