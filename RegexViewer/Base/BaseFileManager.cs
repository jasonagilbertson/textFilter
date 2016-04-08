// ***********************************************************************
// Assembly         : RegexViewer
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-25-2015
// ***********************************************************************
// <copyright file="BaseFileManager.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RegexViewer
{
    public abstract class BaseFileManager<T> : Base, IFileManager<T>
    {
        #region Public Fields

        public abstract IFile<T> ManageFileProperties(string LogName, IFile<T> items = null);

        public RegexViewerSettings Settings = RegexViewerSettings.Settings;

        #endregion Public Fields

        #region Public Constructors

        public BaseFileManager()
        {
        }

        #endregion Public Constructors

        #region Public Properties

        public List<IFile<T>> FileManager { get; set; }

        #endregion Public Properties

        #region Public Methods

        public bool CloseFile(string FileName)
        {
            try
            {
                if (FileManager.Exists(x => String.Compare(x.Tag, FileName, true) == 0))
                {
                    SetStatus("file not open. removing:" + FileName);
                    FileManager.Remove(FileManager.Find(x => String.Compare(x.Tag, FileName, true) == 0));
                    if (typeof(T) == typeof(FilterFileItem))
                    {
                        Settings.RemoveFilterFile(FileName);
                    }
                    if (typeof(T) == typeof(LogFileItem))
                    {
                        Settings.RemoveLogFile(FileName);
                    }
                    return true;
                }
                else
                {
                    SetStatus("file not open:" + FileName);

                    return false;
                }
            }
            catch (Exception e)
            {
                SetStatus("CloseFile exception:" + e.ToString());
                return false;
            }
        }

        public abstract IFile<T> NewFile(string LogName, ObservableCollection<T> items);

        public abstract IFile<T> OpenFile(string LogName);

        public abstract List<IFile<T>> OpenFiles(string[] files);

        public abstract IFile<T> ReadFile(string LogName);

        public abstract bool SaveFile(string FileName, IFile<T> file);

        #endregion Public Methods
    }
}