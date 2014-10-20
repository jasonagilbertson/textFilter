using System.Collections.ObjectModel;

namespace RegexViewer
{
    public interface IFileProperties<T>
    {
        #region Public Properties

        ObservableCollection<T> ContentItems { get; set; }

        bool Dirty { get; set; }

        string FileName { get; set; }

        string Tag { get; set; }

        #endregion Public Properties
    }
}