// ************************************************************************************
// Assembly: TextFilter
// File: FileItem.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Windows.Media;

namespace TextFilter
{
    public class FileItem : IFileItem
    {
        public Brush Background { get; set; }

        public string Content { get; set; }

        public Brush Foreground { get; set; }

        public int Index { get; set; }

        public IFileItem ShallowCopy()
        {
            return (IFileItem)MemberwiseClone();
        }
    }
}