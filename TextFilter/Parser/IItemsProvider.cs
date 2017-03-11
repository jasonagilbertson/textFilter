// ************************************************************************************
// Assembly: TextFilter
// File: IItemsProvider.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.Generic;

namespace TextFilter
{
    public interface IItemsProvider<T>
    {
        int FetchCount();

        IList<T> FetchRange(int startIndex, int count);
    }
}