using System.Collections.Generic;

namespace RegexViewer
{
    public interface IFileProperties<T>
    {
        #region Public Properties

        bool Dirty { get; set; }

        string FileName { get; set; }

        string Tag { get; set; }

        List<T> ContentItems { get; set; }

        #endregion Public Properties
    }
}