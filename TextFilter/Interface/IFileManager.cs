// ************************************************************************************
// Assembly: TextFilter
// File: IFileManager.cs
// Created: 12/24/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TextFilter
{
    public interface IFileManager<T>
    {
        event PropertyChangedEventHandler PropertyChanged;

        List<IFile<T>> FileManager { get; set; }

        bool CloseFile(string LogName);

        IFile<T> ManageFileProperties(string LogName, IFile<T> items = null);

        IFile<T> NewFile(string LogName, ObservableCollection<T> items = null);

        IFile<T> OpenFile(string LogName);

        List<IFile<T>> OpenFiles(string[] files);

        IFile<T> ReadFile(string LogName);

        bool SaveFile(string FileName, IFile<T> file);
    }
}