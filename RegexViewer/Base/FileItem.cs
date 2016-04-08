// ***********************************************************************
// Assembly         : RegexViewer
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-31-2015
// ***********************************************************************
// <copyright file="FileItem.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Windows.Media;

namespace RegexViewer
{
    public class FileItem : IFileItem
    {
        #region Public Properties

        public Brush Background { get; set; }

        public string Content { get; set; }

        public Brush Foreground { get; set; }

        public int Index { get; set; }

        #endregion Public Properties

        #region Public Methods

        public IFileItem ShallowCopy()
        {
            return (IFileItem)MemberwiseClone();
        }

        #endregion Public Methods
    }
}