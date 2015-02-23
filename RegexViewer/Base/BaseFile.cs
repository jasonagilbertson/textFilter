using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RegexViewer
{
    public abstract class BaseFile<T> : Base, IFile<T>, INotifyPropertyChanged
    {
        #region Private Fields

        private bool _modified;

        #endregion Private Fields

        #region Public Constructors

        public BaseFile()
        {
            this.Modified = false;
        }

        #endregion Public Constructors

        #region Public Properties

        public abstract ObservableCollection<T> ContentItems { get; set; }

        public string FileName { get; set; }

        public bool Modified
        {
            get
            {
                return _modified;
            }
            set
            {
                _modified = value;
            }
        }

        public string Tag { get; set; }

        #endregion Public Properties
    }
}