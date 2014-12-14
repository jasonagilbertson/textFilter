using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RegexViewer
{
    public abstract class BaseFile<T> : Base, IFile<T>, INotifyPropertyChanged
    {
        //

        #region Public Constructors

        public BaseFile()
        {
            this.Modified = false;
        }

        #endregion Public Constructors

        #region Public Properties

        public abstract ObservableCollection<T> ContentItems { get; set; }

        public string FileName { get; set; }

        public bool Modified { get; set; }

        public string Tag { get; set; }

        #endregion Public Properties
    }
}