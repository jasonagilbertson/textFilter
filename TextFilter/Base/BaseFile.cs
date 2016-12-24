// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-25-2015 ***********************************************************************
// <copyright file="BaseFile.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
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