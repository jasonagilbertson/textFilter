// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 09-06-2015 ***********************************************************************
// <copyright file="IFileItem.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************

namespace TextFilter
{
    public interface IFileItem
    {
        System.Windows.Media.Brush Background { get; set; }

        string Content { get; set; }

        System.Windows.Media.Brush Foreground { get; set; }

        int Index { get; set; }

        IFileItem ShallowCopy();
    }
}