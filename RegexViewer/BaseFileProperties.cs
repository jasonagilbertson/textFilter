using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RegexViewer
{
    public abstract class BaseFileProperties<T> : Base, IFileProperties<T>
    {
        //

        #region Public Constructors

        public BaseFileProperties()
        {
            this.Dirty = false;
        }

        #endregion Public Constructors

        #region Public Properties

        public abstract ObservableCollection<T> ContentItems { get; set; }

        public bool Dirty { get; set; }

        public string FileName { get; set; }

        public string Tag { get; set; }

        #endregion Public Properties
    }
}