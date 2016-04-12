// ***********************************************************************
// Assembly         : TextFilter
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 09-06-2015
// ***********************************************************************
// <copyright file="IFile.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.ObjectModel;

namespace TextFilter
{
    public interface IFile<T>
    {
        #region Public Properties

        ObservableCollection<T> ContentItems { get; set; }

        string FileName { get; set; }

        bool IsNew { get; set; }

        bool IsReadOnly { get; set; }

        bool Modified { get; set; }

        string Tag { get; set; }

        #endregion Public Properties
    }
}