// ************************************************************************************
// Assembly: TextFilter
// File: FilterCommand.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

namespace TextFilter
{
    public enum FilterCommand
    {
        Filter,

        ShowAll,

        Unknown,

        Hide
    }

    public enum FilterNeed
    {
        Current,

        Filter,

        ApplyColor,

        Unknown,

        ShowAll
    }
}