using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RegexViewer
{
    public abstract class BaseFileItems<T> : Base, IFileItems<T>, INotifyPropertyChanged
    {
        //
     
        
        #region Public Constructors

        public BaseFileItems()
        {
            this.Modified = false;
        }

        #endregion Public Constructors

        #region Public Properties

        public abstract ObservableCollection<T> ContentItems { get; set; }

        public bool Modified { get; set; }

        public string FileName { get; set; }

        public string Tag { get; set; }

        #endregion Public Properties
    }
}