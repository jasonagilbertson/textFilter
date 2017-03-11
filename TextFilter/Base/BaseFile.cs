// ************************************************************************************
// Assembly: TextFilter
// File: BaseFile.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TextFilter
{
    public abstract class BaseFile<T> : Base, IFile<T>, INotifyPropertyChanged
    {
        public BaseFile()
        {
            Modified = false;
            IsNew = true;
            IsReadOnly = false;
        }

        public abstract ObservableCollection<T> ContentItems { get; set; }

        public string FileName { get; set; }

        public bool IsNew { get; set; }

        public bool IsReadOnly { get; set; }

        public bool Modified { get; set; }

        public string Tag { get; set; }
    }
}