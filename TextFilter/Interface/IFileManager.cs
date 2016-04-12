// ***********************************************************************
// Assembly         : TextFilter
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-25-2015
// ***********************************************************************
// <copyright file="IFileManager.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TextFilter
{
    public interface IFileManager<T>
    {
        #region Public Events

        event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        List<IFile<T>> FileManager { get; set; }

        #endregion Public Properties

        #region Public Methods

        bool CloseFile(string LogName);

        IFile<T> ManageFileProperties(string LogName, IFile<T> items = null);

        IFile<T> NewFile(string LogName, ObservableCollection<T> items = null);

        IFile<T> OpenFile(string LogName);

        List<IFile<T>> OpenFiles(string[] files);

        IFile<T> ReadFile(string LogName);

        bool SaveFile(string FileName, IFile<T> file);

        #endregion Public Methods
    }
}