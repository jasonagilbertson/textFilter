// ************************************************************************************
// Assembly: TextFilter
// File: IFileItem.cs
// Created: 12/24/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

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