using System.Collections.ObjectModel;

namespace RegexViewer
{
    public interface IFile<T>
    {
        #region Public Properties

        ObservableCollection<T> ContentItems { get; set; }

        bool Modified { get; set; }

        string FileName { get; set; }

        string Tag { get; set; }

        #endregion Public Properties
    }
}