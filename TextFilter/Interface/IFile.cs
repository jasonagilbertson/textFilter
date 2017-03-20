// ************************************************************************************
// Assembly: TextFilter
// File: ifile.cs
// Created: 12/24/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.ObjectModel;

namespace TextFilter
{
    public interface IFile<T>
    {
        ObservableCollection<T> ContentItems { get; set; }

        string FileName { get; set; }

        bool IsNew { get; set; }

        bool IsReadOnly { get; set; }

        bool Modified { get; set; }

        string Tag { get; set; }
    }
}