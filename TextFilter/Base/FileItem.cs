// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-31-2015 ***********************************************************************
// <copyright file="FileItem.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System.Windows.Media;

namespace TextFilter
{
    public class FileItem : IFileItem
    {
        #region Properties

        public Brush Background { get; set; }

        public string Content { get; set; }

        public Brush Foreground { get; set; }

        public int Index { get; set; }

        #endregion Properties

        #region Methods

        public IFileItem ShallowCopy()
        {
            return (IFileItem)MemberwiseClone();
        }

        #endregion Methods
    }
}