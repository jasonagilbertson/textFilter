using System.Collections.Generic;

namespace RegexViewer
{
    public interface IFileProperties<T>
    {
        #region Public Properties

        List<T> ContentItems { get; set; }

        bool Dirty { get; set; }

        string FileName { get; set; }

        string Tag { get; set; }

        #endregion Public Properties
    }
}