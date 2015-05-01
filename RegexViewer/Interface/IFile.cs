using System.Collections.ObjectModel;

namespace RegexViewer
{
    public interface IFile<T>
    {
        #region Public Properties

        ObservableCollection<T> ContentItems { get; set; }

        string FileName { get; set; }

        bool Modified { get; set; }

        string Tag { get; set; }

        bool IsNew { get; set; }

        bool IsReadOnly { get; set; }



        #endregion Public Properties
    }
}