using System.Collections.Generic;

namespace RegexViewer
{
    public abstract class BaseFileProperties<T> : RegexViewer.IFileProperties<T>
    {
        //
        #region Public Constructors

        public BaseFileProperties()
        {
            this.Dirty = false;
        }

        #endregion Public Constructors

        #region Public Properties

        public abstract List<T> ContentItems { get; set; }

        public bool Dirty { get; set; }

        public string FileName { get; set; }

        public string Tag { get; set; }

        #endregion Public Properties
    }
}